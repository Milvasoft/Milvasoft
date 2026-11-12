using Microsoft.EntityFrameworkCore;
using Milvasoft.BulkExtensions.Tests.Infrastructure;

namespace Milvasoft.BulkExtensions.Tests;

public class DeleteReadTruncateTests(PostgresFixture fixture) : BulkTestBase(fixture)
{
    [Fact]
    public async Task BulkDeleteAsync_DeletesByPrimaryKey()
    {
        var seeded = await SeedAsync(NewProduct("A"), NewProduct("B"), NewProduct("C"));

        await using (var context = CreateContext())
        {
            var deleted = await context.BulkDeleteAsync([new Product { Id = seeded[0].Id }, new Product { Id = seeded[2].Id }, new Product { Id = 999_999 }],
                                                        cancellationToken: CancellationToken);

            deleted.Should().Be(2);
        }

        await using var verify = CreateContext();

        (await verify.Products.Select(p => p.Sku).ToListAsync(CancellationToken)).Should().Equal("B");
    }

    [Fact]
    public async Task BulkDeleteAsync_CompositeKey()
    {
        await SeedAsync(new Inventory { WarehouseId = 1, ProductCode = "X", Quantity = 1 },
                        new Inventory { WarehouseId = 1, ProductCode = "Y", Quantity = 2 },
                        new Inventory { WarehouseId = 2, ProductCode = "X", Quantity = 3 });

        await using (var context = CreateContext())
        {
            var deleted = await context.BulkDeleteAsync([new Inventory { WarehouseId = 1, ProductCode = "X" }, new Inventory { WarehouseId = 2, ProductCode = "X" }],
                                                        cancellationToken: CancellationToken);

            deleted.Should().Be(2);
        }

        await using var verify = CreateContext();

        (await verify.Inventories.SingleAsync(CancellationToken)).Quantity.Should().Be(2);
    }

    [Fact]
    public async Task BulkDeleteAsync_ByUpdateByProperties_AndDuplicateInputIsAllowed()
    {
        await SeedAsync(NewProduct("A"), NewProduct("B"));

        await using (var context = CreateContext())
        {
            var deleted = await context.BulkDeleteAsync([new Product { Sku = "A" }, new Product { Sku = "A" }],
                                                        c => c.UpdateByProperties = [nameof(Product.Sku)],
                                                        CancellationToken);

            deleted.Should().Be(1);
        }

        await using var verify = CreateContext();

        (await verify.Products.Select(p => p.Sku).ToListAsync(CancellationToken)).Should().Equal("B");
    }

    [Fact]
    public async Task BulkDeleteAsync_TphRows()
    {
        var dog = new Dog { Name = "Rex", Breed = "Collie" };
        var cat = new Cat { Name = "Tom", Lives = 9 };
        await SeedAsync<Animal>(dog, cat);

        await using (var context = CreateContext())
            (await context.BulkDeleteAsync(new List<Animal> { new Dog { Id = dog.Id } }, cancellationToken: CancellationToken)).Should().Be(1);

        await using var verify = CreateContext();

        (await verify.Animals.SingleAsync(CancellationToken)).Should().BeOfType<Cat>();
    }

    [Fact]
    public async Task BulkDelete_Sync_Works()
    {
        var seeded = await SeedAsync(NewProduct("A"), NewProduct("B"));

        using (var context = CreateContext())
            context.BulkDelete(seeded).Should().Be(2);

        using var verify = CreateContext();

        verify.Products.Count().Should().Be(0);
    }

    [Fact]
    public async Task BulkReadAsync_ByPrimaryKey_ReturnsMatchingEntities()
    {
        var seeded = await SeedAsync(NewProduct("A"), NewProduct("B"), NewProduct("C"));

        await using var context = CreateContext();

        var read = await context.BulkReadAsync([new Product { Id = seeded[0].Id }, new Product { Id = seeded[2].Id }, new Product { Id = 999_999 }],
                                               cancellationToken: CancellationToken);

        read.Select(p => p.Sku).Should().BeEquivalentTo(["A", "C"]);
        read.Single(p => p.Sku == "A").Should().BeEquivalentTo(seeded[0], o => o.Excluding(p => p.Category));
    }

    [Fact]
    public async Task BulkReadAsync_ByUpdateByProperties()
    {
        await SeedAsync(NewProduct("A", 1), NewProduct("B", 2));

        await using var context = CreateContext();

        var read = await context.BulkReadAsync([new Product { Sku = "B" }, new Product { Sku = "B" }, new Product { Sku = "nope" }],
                                               c => c.UpdateByProperties = [nameof(Product.Sku)],
                                               CancellationToken);

        read.Should().ContainSingle().Which.Price.Should().Be(2);
    }

    [Fact]
    public async Task BulkReadAsync_CompositeKey_AndOwnedTypes()
    {
        await SeedAsync(new Inventory { WarehouseId = 1, ProductCode = "X", Quantity = 5 }, new Inventory { WarehouseId = 2, ProductCode = "X", Quantity = 6 });
        var order = (await SeedAsync(new Order { Number = "O-1", ShippingAddress = new Address { Street = "S", City = "C" } }))[0];

        await using var context = CreateContext();

        var inventories = await context.BulkReadAsync([new Inventory { WarehouseId = 2, ProductCode = "X" }], cancellationToken: CancellationToken);
        var orders = await context.BulkReadAsync([new Order { Id = order.Id }], cancellationToken: CancellationToken);

        inventories.Should().ContainSingle().Which.Quantity.Should().Be(6);
        orders.Should().ContainSingle().Which.ShippingAddress.City.Should().Be("C");
    }

    [Fact]
    public async Task BulkReadAsync_TracksEntitiesLikeAnyQuery()
    {
        var seeded = await SeedAsync(NewProduct("A"));

        await using var context = CreateContext();

        var read = await context.BulkReadAsync([new Product { Id = seeded[0].Id }], cancellationToken: CancellationToken);

        context.Entry(read[0]).State.Should().Be(EntityState.Unchanged);
    }

    [Fact]
    public async Task TruncateAsync_RemovesAllRows()
    {
        await SeedAsync(new Inventory { WarehouseId = 1, ProductCode = "X" }, new Inventory { WarehouseId = 2, ProductCode = "Y" });

        await using (var context = CreateContext())
            await context.TruncateAsync<Inventory>(CancellationToken);

        await using var verify = CreateContext();

        (await verify.Inventories.CountAsync(CancellationToken)).Should().Be(0);
    }
}