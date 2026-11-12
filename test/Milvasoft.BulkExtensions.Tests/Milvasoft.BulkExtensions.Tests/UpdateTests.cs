using Microsoft.EntityFrameworkCore;
using Milvasoft.BulkExtensions.Tests.Infrastructure;

namespace Milvasoft.BulkExtensions.Tests;

public class UpdateTests(PostgresFixture fixture) : BulkTestBase(fixture)
{
    [Fact]
    public async Task BulkUpdateAsync_UpdatesMatchingRowsByPrimaryKey()
    {
        var seeded = await SeedAsync(NewProduct("A", 1), NewProduct("B", 2), NewProduct("C", 3));

        var changes = seeded.Take(2).Select(p => new Product
        {
            Id = p.Id,
            Sku = p.Sku,
            Name = p.Name + " v2",
            Price = p.Price * 10,
            Status = ProductStatus.Discontinued,
            Tags = ["changed"],
            Mood = Mood.Sad,
            Size = new Dimensions { Width = 9, Height = 8 },
            CreatedAt = p.CreatedAt,
        }).ToList();

        await using (var context = CreateContext())
            (await context.BulkUpdateAsync(changes, cancellationToken: CancellationToken)).Should().Be(2);

        await using var verify = CreateContext();

        var saved = await verify.Products.OrderBy(p => p.Sku).ToListAsync(CancellationToken);

        saved[0].Name.Should().Be("Product A v2");
        saved[0].Price.Should().Be(10);
        saved[0].Status.Should().Be(ProductStatus.Discontinued);
        saved[0].Tags.Should().Equal("changed");
        saved[0].Mood.Should().Be(Mood.Sad);
        saved[0].Size.Width.Should().Be(9);
        saved[0].NameLength.Should().Be("Product A v2".Length);
        saved[0].CreatedAt.Should().Be(seeded[0].CreatedAt);
        saved[0].Version.Should().NotBe(seeded[0].Version);
        saved[2].Name.Should().Be("Product C", "rows not in the input are not touched");
    }

    [Fact]
    public async Task BulkUpdateAsync_RowsThatDoNotExist_AreIgnored()
    {
        var seeded = await SeedAsync(NewProduct("A"));

        var existing = NewProduct("A", 50);
        existing.Id = seeded[0].Id;
        existing.CreatedAt = seeded[0].CreatedAt;

        var missing = NewProduct("missing");
        missing.Id = 999_999;
        missing.CreatedAt = DateTime.UtcNow;

        await using (var context = CreateContext())
            (await context.BulkUpdateAsync([existing, missing], cancellationToken: CancellationToken)).Should().Be(1);

        await using var verify = CreateContext();

        (await verify.Products.CountAsync(CancellationToken)).Should().Be(1);
        (await verify.Products.SingleAsync(CancellationToken)).Price.Should().Be(50);
    }

    [Fact]
    public async Task BulkUpdateAsync_PropertiesToInclude_UpdatesOnlyThoseColumns()
    {
        var seeded = await SeedAsync(NewProduct("A", 1));
        var change = new Product { Id = seeded[0].Id, Sku = "ignored", Name = "ignored", Price = 77, Status = ProductStatus.Draft };

        await using (var context = CreateContext())
            await context.BulkUpdateAsync([change], c => c.PropertiesToInclude = [nameof(Product.Price)], CancellationToken);

        await using var verify = CreateContext();

        var saved = await verify.Products.SingleAsync(CancellationToken);

        saved.Price.Should().Be(77);
        saved.Sku.Should().Be("A");
        saved.Name.Should().Be("Product A");
        saved.Status.Should().Be(ProductStatus.Active);
        saved.Tags.Should().Equal("a", "b");
    }

    [Fact]
    public async Task BulkUpdateAsync_UpdateByProperties_MatchesByOtherColumns()
    {
        await SeedAsync(NewProduct("A", 1), NewProduct("B", 2));

        var changes = new[] { new Product { Sku = "B", Price = 20 }, new Product { Sku = "A", Price = 10 } };

        await using (var context = CreateContext())
        {
            var updated = await context.BulkUpdateAsync(changes, c =>
            {
                c.UpdateByProperties = [nameof(Product.Sku)];
                c.PropertiesToInclude = [nameof(Product.Price)];
            }, CancellationToken);

            updated.Should().Be(2);
        }

        await using var verify = CreateContext();

        (await verify.Products.OrderBy(p => p.Sku).Select(p => p.Price).ToListAsync(CancellationToken)).Should().Equal(10, 20);
    }

    [Fact]
    public async Task BulkUpdateAsync_DuplicateKeysInInput_Throws()
    {
        var seeded = await SeedAsync(NewProduct("A"));
        var first = NewProduct("A");
        var second = NewProduct("A");
        first.Id = second.Id = seeded[0].Id;

        await using var context = CreateContext();

        var act = () => context.BulkUpdateAsync([first, second], cancellationToken: CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*more than one entity*");
    }

    [Fact]
    public async Task BulkUpdateAsync_WithSetOutputIdentity_ReadsBackValuesGeneratedOnUpdate()
    {
        var seeded = await SeedAsync(NewProduct("A"));
        var oldVersion = seeded[0].Version;

        var change = NewProduct("A", name: "A much longer name");
        change.Id = seeded[0].Id;
        change.CreatedAt = seeded[0].CreatedAt;

        await using (var context = CreateContext())
            await context.BulkUpdateAsync([change], c => c.SetOutputIdentity = true, CancellationToken);

        change.NameLength.Should().Be("A much longer name".Length);
        change.Version.Should().NotBe(oldVersion);

        await using var verify = CreateContext();

        (await verify.Products.SingleAsync(CancellationToken)).Version.Should().Be(change.Version);
    }

    [Fact]
    public async Task BulkUpdateAsync_ShadowForeignKeyWithoutNavigation_IsNotOverwritten()
    {
        var product = (await SeedAsync(NewProduct("A")))[0];

        int reviewId;

        await using (var context = CreateContext())
        {
            var review = new Review { Text = "old", Product = await context.Products.SingleAsync(CancellationToken) };
            context.Add(review);
            await context.SaveChangesAsync(CancellationToken);
            reviewId = review.Id;
        }

        await using (var context = CreateContext())
            (await context.BulkUpdateAsync([new Review { Id = reviewId, Text = "new" }], cancellationToken: CancellationToken)).Should().Be(1);

        await using var verify = CreateContext();

        var saved = await verify.Reviews.Select(r => new { r.Text, ProductId = EF.Property<int>(r, "ProductId") }).SingleAsync(CancellationToken);

        saved.Text.Should().Be("new");
        saved.ProductId.Should().Be(product.Id);
    }

    [Fact]
    public async Task BulkUpdateAsync_TrackedEntityShadowForeignKey_IsWritten()
    {
        var products = await SeedAsync(NewProduct("A"), NewProduct("B"));

        await using (var context = CreateContext())
        {
            context.Add(new Review { Text = "r", Product = await context.Products.SingleAsync(p => p.Sku == "A", CancellationToken) });
            await context.SaveChangesAsync(CancellationToken);
        }

        await using (var context = CreateContext())
        {
            var review = await context.Reviews.SingleAsync(CancellationToken);
            context.Entry(review).Property("ProductId").CurrentValue = products[1].Id;

            await context.BulkUpdateAsync([review], cancellationToken: CancellationToken);
        }

        await using var verify = CreateContext();

        (await verify.Reviews.Select(r => EF.Property<int>(r, "ProductId")).SingleAsync(CancellationToken)).Should().Be(products[1].Id);
    }

    [Fact]
    public async Task BulkUpdateAsync_TphMixedTypes_UpdatesEachTypesColumns()
    {
        var dog = new Dog { Name = "Rex", Breed = "Collie" };
        var cat = new Cat { Name = "Tom", Lives = 9 };
        await SeedAsync<Animal>(dog, cat);

        await using (var context = CreateContext())
        {
            var updated = await context.BulkUpdateAsync(new List<Animal>
            {
                new Dog { Id = dog.Id, Name = "Rex 2", Breed = "Pug" },
                new Cat { Id = cat.Id, Name = "Tom 2", Lives = 8 },
            }, cancellationToken: CancellationToken);

            updated.Should().Be(2);
        }

        await using var verify = CreateContext();

        var savedDog = await verify.Animals.OfType<Dog>().SingleAsync(CancellationToken);
        var savedCat = await verify.Animals.OfType<Cat>().SingleAsync(CancellationToken);

        savedDog.Should().BeEquivalentTo(new { Name = "Rex 2", Breed = "Pug" });
        savedCat.Should().BeEquivalentTo(new { Name = "Tom 2", Lives = 8 });
    }

    [Fact]
    public async Task BulkUpdateAsync_OwnedType_IsUpdatedWithOwner()
    {
        var order = (await SeedAsync(new Order { Number = "O-1", ShippingAddress = new Address { Street = "Old", City = "Old" } }))[0];

        await using (var context = CreateContext())
        {
            await context.BulkUpdateAsync([new Order { Id = order.Id, Number = "O-1", ShippingAddress = new Address { Street = "New", City = "İzmir" } }],
                                          cancellationToken: CancellationToken);
        }

        await using var verify = CreateContext();

        (await verify.Orders.SingleAsync(CancellationToken)).ShippingAddress.Should().BeEquivalentTo(new Address { Street = "New", City = "İzmir" });
    }

    [Fact]
    public async Task BulkUpdateAsync_NoColumnsLeft_Throws()
    {
        await using var context = CreateContext();

        var act = () => context.BulkUpdateAsync([new Inventory { WarehouseId = 1, ProductCode = "X" }],
                                                c => c.PropertiesToExclude = [nameof(Inventory.Quantity)],
                                                CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*no columns to update*");
    }

    [Fact]
    public async Task BulkUpdate_Sync_Works()
    {
        var seeded = await SeedAsync(NewProduct("A", 1));
        var change = NewProduct("A", 5);
        change.Id = seeded[0].Id;
        change.CreatedAt = seeded[0].CreatedAt;

        using (var context = CreateContext())
            context.BulkUpdate([change], c => c.PropertiesToInclude = [nameof(Product.Price)]).Should().Be(1);

        using var verify = CreateContext();

        verify.Products.Single().Price.Should().Be(5);
    }
}