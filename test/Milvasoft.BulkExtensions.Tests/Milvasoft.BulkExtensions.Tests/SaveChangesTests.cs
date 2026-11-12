using Microsoft.EntityFrameworkCore;
using Milvasoft.BulkExtensions.Tests.Infrastructure;

namespace Milvasoft.BulkExtensions.Tests;

public class SaveChangesTests(PostgresFixture fixture) : BulkTestBase(fixture)
{
    [Fact]
    public async Task BulkSaveChangesAsync_NoChanges_ReturnsZero()
    {
        await using var context = CreateContext();

        (await context.BulkSaveChangesAsync(cancellationToken: CancellationToken)).Should().Be(0);
    }

    [Fact]
    public async Task BulkSaveChangesAsync_SelfReferencingTree_GetsKeysLevelByLevel()
    {
        var root = new TreeNode { Name = "root" };
        var child1 = new TreeNode { Name = "child1", Parent = root };
        var child2 = new TreeNode { Name = "child2", Parent = root };
        var grandChild = new TreeNode { Name = "grandChild", Parent = child1 };

        await using (var context = CreateContext())
        {
            context.AddRange(grandChild, child2, child1, root);

            (await context.BulkSaveChangesAsync(cancellationToken: CancellationToken)).Should().Be(4);

            context.ChangeTracker.Entries().Should().OnlyContain(e => e.State == EntityState.Unchanged);
            root.Id.Should().BeGreaterThan(0);
            child1.ParentId.Should().Be(root.Id);
            child2.ParentId.Should().Be(root.Id);
            grandChild.ParentId.Should().Be(child1.Id);
        }

        await using var verify = CreateContext();

        var saved = await verify.TreeNodes.ToDictionaryAsync(n => n.Name, CancellationToken);

        saved["root"].ParentId.Should().BeNull();
        saved["child1"].ParentId.Should().Be(saved["root"].Id);
        saved["grandChild"].ParentId.Should().Be(saved["child1"].Id);
    }

    [Fact]
    public async Task BulkSaveChangesAsync_PrincipalAndDependentTables_AreSavedInOrder()
    {
        var category = new Category { Name = "Books" };
        var products = new[] { NewProduct("A"), NewProduct("B") };
        var review = new Review { Text = "nice", Product = products[0] };

        await using (var context = CreateContext())
        {
            category.Products.AddRange(products);
            context.Add(review);
            context.Add(category);

            (await context.BulkSaveChangesAsync(cancellationToken: CancellationToken)).Should().Be(4);

            products.Should().OnlyContain(p => p.Id > 0 && p.CategoryId == category.Id && p.NameLength > 0 && p.Version != 0);
            context.Entry(review).Property<int>("ProductId").CurrentValue.Should().Be(products[0].Id);
            context.Entry(review).Property<int>("ProductId").IsTemporary.Should().BeFalse();
        }

        await using var verify = CreateContext();

        (await verify.Products.CountAsync(p => p.CategoryId == category.Id, CancellationToken)).Should().Be(2);
        (await verify.Reviews.Select(r => EF.Property<int>(r, "ProductId")).SingleAsync(CancellationToken)).Should().Be(products[0].Id);
    }

    [Fact]
    public async Task BulkSaveChangesAsync_OwnedTypesAndOwnedCollections()
    {
        var order = new Order
        {
            Number = "O-1",
            ShippingAddress = new Address { Street = "S", City = "C" },
            Lines = [new OrderLine { Product = "p1", Quantity = 1 }, new OrderLine { Product = "p2", Quantity = 2 }],
        };

        await using (var context = CreateContext())
        {
            context.Add(order);

            (await context.BulkSaveChangesAsync(cancellationToken: CancellationToken)).Should().Be(3);
        }

        await using var verify = CreateContext();

        var saved = await verify.Orders.SingleAsync(CancellationToken);

        saved.ShippingAddress.City.Should().Be("C");
        saved.Lines.Select(l => l.Product).Should().BeEquivalentTo(["p1", "p2"]);
    }

    [Fact]
    public async Task BulkSaveChangesAsync_AddsUpdatesAndDeletesTogether()
    {
        await SeedAsync(NewProduct("keep", 1), NewProduct("change", 2), NewProduct("remove", 3));

        await using (var context = CreateContext())
        {
            var products = await context.Products.ToListAsync(CancellationToken);

            products.Single(p => p.Sku == "change").Price = 20;
            context.Remove(products.Single(p => p.Sku == "remove"));
            context.Add(NewProduct("added", 4));

            (await context.BulkSaveChangesAsync(cancellationToken: CancellationToken)).Should().Be(3);

            context.ChangeTracker.Entries().Should().HaveCount(3).And.OnlyContain(e => e.State == EntityState.Unchanged);
        }

        await using var verify = CreateContext();

        var saved = await verify.Products.OrderBy(p => p.Sku).ToListAsync(CancellationToken);

        saved.Select(p => (p.Sku, p.Price)).Should().Equal(("added", 4m), ("change", 20m), ("keep", 1m));
    }

    [Fact]
    public async Task BulkSaveChangesAsync_ChangeOfOwnedTypeOnly_UpdatesOwnerRow()
    {
        await SeedAsync(new Order { Number = "O-1", ShippingAddress = new Address { Street = "S", City = "Old" } },
                        new Order { Number = "O-2", ShippingAddress = new Address { Street = "S", City = "Gone" } });

        await using (var context = CreateContext())
        {
            var orders = await context.Orders.OrderBy(o => o.Number).ToListAsync(CancellationToken);

            orders[0].ShippingAddress.City = "New";
            orders[1].ShippingAddress = null;

            (await context.BulkSaveChangesAsync(cancellationToken: CancellationToken)).Should().Be(2);
        }

        await using var verify = CreateContext();

        var saved = await verify.Orders.OrderBy(o => o.Number).ToListAsync(CancellationToken);

        saved[0].ShippingAddress.City.Should().Be("New");
        saved[1].ShippingAddress.Should().BeNull();
    }

    [Fact]
    public async Task BulkSaveChangesAsync_Failure_LeavesChangeTrackerAsItWas()
    {
        await SeedAsync(NewProduct("taken"));

        await using var context = CreateContext();

        var category = new Category { Name = "C" };
        var product = NewProduct("taken");
        product.Category = category;
        context.Add(product);

        var act = () => context.BulkSaveChangesAsync(cancellationToken: CancellationToken);

        await act.Should().ThrowAsync<Npgsql.PostgresException>().Where(e => e.SqlState == "23505");

        context.Entry(product).State.Should().Be(EntityState.Added);
        context.Entry(product).Property(p => p.Id).IsTemporary.Should().BeTrue();
        context.Entry(category).State.Should().Be(EntityState.Added);

        product.Sku = "free";

        (await context.BulkSaveChangesAsync(cancellationToken: CancellationToken)).Should().Be(2);

        context.Entry(product).State.Should().Be(EntityState.Unchanged);
        product.Id.Should().BeGreaterThan(0);
        product.CategoryId.Should().Be(category.Id);
    }

    [Fact]
    public async Task BulkSaveChangesAsync_InsideUserTransaction()
    {
        await using (var context = CreateContext())
        {
            await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken);

            context.Add(NewProduct("A"));
            await context.BulkSaveChangesAsync(cancellationToken: CancellationToken);

            await transaction.RollbackAsync(CancellationToken);
        }

        await using var verify = CreateContext();

        (await verify.Products.CountAsync(CancellationToken)).Should().Be(0);
    }

    [Fact]
    public void BulkSaveChanges_Sync_Works()
    {
        var root = new TreeNode { Name = "root", Children = [new TreeNode { Name = "child" }] };

        using (var context = CreateContext())
        {
            context.Add(root);
            context.BulkSaveChanges().Should().Be(2);
        }

        using var verify = CreateContext();

        verify.TreeNodes.Single(n => n.Name == "child").ParentId.Should().Be(root.Id);
    }
}