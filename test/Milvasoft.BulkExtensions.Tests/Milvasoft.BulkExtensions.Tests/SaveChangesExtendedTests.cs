using Microsoft.EntityFrameworkCore;
using Milvasoft.BulkExtensions.Tests.Infrastructure;

namespace Milvasoft.BulkExtensions.Tests;

public class SaveChangesExtendedTests(PostgresFixture fixture) : BulkTestBase(fixture)
{
    [Fact]
    public async Task ManyToManyJoinRows_AreSaved()
    {
        var math = new Course { Title = "Math" };
        var art = new Course { Title = "Art" };
        var ada = new Student { Name = "Ada", Courses = [math, art] };
        var bob = new Student { Name = "Bob", Courses = [math] };

        await using (var context = CreateContext())
        {
            context.AddRange(ada, bob);

            (await context.BulkSaveChangesAsync(cancellationToken: CancellationToken)).Should().Be(7, "2 students, 2 courses and 3 join rows");
            context.ChangeTracker.Entries().Should().OnlyContain(e => e.State == EntityState.Unchanged);
        }

        await using (var context = CreateContext())
        {
            var loaded = await context.Students.Include(s => s.Courses).SingleAsync(s => s.Name == "Ada", CancellationToken);
            loaded.Courses.Select(c => c.Title).Should().BeEquivalentTo(["Math", "Art"]);

            loaded.Courses.RemoveAll(c => c.Title == "Art");
            (await context.BulkSaveChangesAsync(cancellationToken: CancellationToken)).Should().Be(1, "one join row is deleted");
        }

        await using var verify = CreateContext();

        (await verify.Students.Where(s => s.Name == "Ada").SelectMany(s => s.Courses).Select(c => c.Title).ToListAsync(CancellationToken)).Should().Equal("Math");
    }

    [Fact]
    public async Task TphEntities_OfDifferentTypes_AreSavedAndUpdated()
    {
        await using (var context = CreateContext())
        {
            context.AddRange(new Dog { Name = "Rex", Breed = "Collie", Collar = new Collar { Color = "red" } }, new Cat { Name = "Tom", Lives = 9 });
            (await context.BulkSaveChangesAsync(cancellationToken: CancellationToken)).Should().Be(2);
        }

        await using (var context = CreateContext())
        {
            var dog = await context.Animals.OfType<Dog>().SingleAsync(CancellationToken);
            var cat = await context.Animals.OfType<Cat>().SingleAsync(CancellationToken);

            dog.Collar.Color = "green";
            cat.Lives = 8;

            (await context.BulkSaveChangesAsync(cancellationToken: CancellationToken)).Should().Be(2);
        }

        await using var verify = CreateContext();

        (await verify.Animals.OfType<Dog>().SingleAsync(CancellationToken)).Collar.Color.Should().Be("green");
        (await verify.Animals.OfType<Cat>().SingleAsync(CancellationToken)).Lives.Should().Be(8);
    }

    [Fact]
    public async Task ExplicitIdentityKey_IsKept()
    {
        await using (var context = CreateContext())
        {
            context.Add(new TreeNode { Id = 500, Name = "explicit", Children = [new TreeNode { Name = "generated" }] });
            await context.BulkSaveChangesAsync(cancellationToken: CancellationToken);
        }

        await using var verify = CreateContext();

        (await verify.TreeNodes.SingleAsync(n => n.Name == "generated", CancellationToken)).ParentId.Should().Be(500);
    }

    [Fact]
    public async Task OwnedCollection_ItemsAreAddedChangedAndRemoved()
    {
        await SeedAsync(new Order { Number = "O-1", Lines = [new OrderLine { Product = "a", Quantity = 1 }, new OrderLine { Product = "b", Quantity = 2 }] });

        await using (var context = CreateContext())
        {
            var order = await context.Orders.SingleAsync(CancellationToken);

            order.Lines.Single(l => l.Product == "a").Quantity = 10;
            order.Lines.RemoveAll(l => l.Product == "b");
            order.Lines.Add(new OrderLine { Product = "c", Quantity = 3 });

            await context.BulkSaveChangesAsync(cancellationToken: CancellationToken);
        }

        await using var verify = CreateContext();

        (await verify.Orders.SingleAsync(CancellationToken)).Lines.Select(l => (l.Product, l.Quantity)).Should().BeEquivalentTo([("a", 10), ("c", 3)]);
    }

    [Fact]
    public async Task DeletingOwnerWithOwnedCollection_RemovesEverything()
    {
        await SeedAsync(new Order { Number = "O-1", ShippingAddress = new Address { City = "C" }, Lines = [new OrderLine { Product = "a" }] });

        await using (var context = CreateContext())
        {
            context.Remove(await context.Orders.SingleAsync(CancellationToken));

            (await context.BulkSaveChangesAsync(cancellationToken: CancellationToken)).Should().Be(2);
            context.ChangeTracker.Entries().Should().BeEmpty();
        }

        await using var verify = CreateContext();

        (await verify.Orders.CountAsync(CancellationToken)).Should().Be(0);
        (await verify.Database.SqlQueryRaw<int>("SELECT count(*)::int AS \"Value\" FROM \"OrderLines\"").SingleAsync(CancellationToken)).Should().Be(0);
    }

    [Fact]
    public async Task DeletingTreeWithChildren_DeletesDependentsFirst()
    {
        await SeedAsync(new TreeNode { Name = "root", Children = [new TreeNode { Name = "a", Children = [new TreeNode { Name = "a1" }] }, new TreeNode { Name = "b" }] });

        await using (var context = CreateContext())
        {
            var nodes = await context.TreeNodes.ToListAsync(CancellationToken);
            context.RemoveRange(nodes);

            (await context.BulkSaveChangesAsync(cancellationToken: CancellationToken)).Should().Be(4);
        }

        await using var verify = CreateContext();

        (await verify.TreeNodes.CountAsync(CancellationToken)).Should().Be(0);
    }

    [Fact]
    public async Task ReparentingRows_UpdatesForeignKeys()
    {
        await SeedAsync(new TreeNode { Name = "old", Children = [new TreeNode { Name = "child" }] });

        await using (var context = CreateContext())
        {
            var child = await context.TreeNodes.SingleAsync(n => n.Name == "child", CancellationToken);
            var newParent = new TreeNode { Name = "new" };

            child.Parent = newParent;

            (await context.BulkSaveChangesAsync(cancellationToken: CancellationToken)).Should().Be(2);
            child.ParentId.Should().Be(newParent.Id);
        }

        await using var verify = CreateContext();

        var saved = await verify.TreeNodes.ToDictionaryAsync(n => n.Name, CancellationToken);

        saved["child"].ParentId.Should().Be(saved["new"].Id);
    }

    [Fact]
    public async Task TablesReferringToEachOther_AreRejected()
    {
        await using var context = CreateContext();

        context.Add(new Husband { Wife = new Wife() });

        var act = () => context.BulkSaveChangesAsync(cancellationToken: CancellationToken);

        await act.Should().ThrowAsync<NotSupportedException>().WithMessage("*cycle*");
        context.ChangeTracker.Entries().Should().OnlyContain(e => e.State == EntityState.Added);
    }

    [Fact]
    public async Task UnchangedEntities_AreNotWritten()
    {
        await SeedAsync(NewProduct("A"));

        await using var context = CreateContext();

        var product = await context.Products.SingleAsync(CancellationToken);
        var version = product.Version;

        (await context.BulkSaveChangesAsync(cancellationToken: CancellationToken)).Should().Be(0);

        await using var verify = CreateContext();

        (await verify.Products.SingleAsync(CancellationToken)).Version.Should().Be(version);
    }

    [Fact]
    public async Task ModifiedEntitiesWithDifferentChangedProperties_AreUpdatedInGroups()
    {
        await SeedAsync(NewProduct("A", 1), NewProduct("B", 2), NewProduct("C", 3));

        await using (var context = CreateContext())
        {
            var products = await context.Products.OrderBy(p => p.Sku).ToListAsync(CancellationToken);

            products[0].Price = 10;
            products[1].Name = "B renamed";
            products[2].Price = 30;
            products[2].Tags = ["x"];

            (await context.BulkSaveChangesAsync(cancellationToken: CancellationToken)).Should().Be(3);
            products.Should().OnlyContain(p => context.Entry(p).State == EntityState.Unchanged);
        }

        await using var verify = CreateContext();

        var saved = await verify.Products.OrderBy(p => p.Sku).ToListAsync(CancellationToken);

        saved.Select(p => (p.Price, p.Name)).Should().Equal((10m, "Product A"), (2m, "B renamed"), (30m, "Product C"));
        saved[2].Tags.Should().Equal("x");
        saved[1].NameLength.Should().Be("B renamed".Length);
    }

    [Fact]
    public async Task ManyNewRows_WithGeneratedKeys()
    {
        var categories = Enumerable.Range(0, 50).Select(i => new Category { Name = $"cat {i}" }).ToList();

        foreach (var (category, index) in categories.Select((c, i) => (c, i)))
            category.Products.AddRange(Enumerable.Range(0, 40).Select(j => NewProduct($"{index}-{j}")));

        await using (var context = CreateContext())
        {
            context.AddRange(categories);

            (await context.BulkSaveChangesAsync(cancellationToken: CancellationToken)).Should().Be(50 + 2000);
        }

        categories.SelectMany(c => c.Products).Should().OnlyContain(p => p.Id > 0 && p.CategoryId != null);

        await using var verify = CreateContext();

        (await verify.Products.GroupBy(p => p.CategoryId).CountAsync(CancellationToken)).Should().Be(50);
    }
}