using Microsoft.EntityFrameworkCore;
using Milvasoft.BulkExtensions.Tests.Infrastructure;

namespace Milvasoft.BulkExtensions.Tests;

public class UpsertTests(PostgresFixture fixture) : BulkTestBase(fixture)
{
    [Fact]
    public async Task BulkInsertOrUpdateAsync_ByPrimaryKey_UpdatesExistingAndInsertsNew()
    {
        var seeded = await SeedAsync(NewProduct("A", 1), NewProduct("B", 2));

        var existing = NewProduct("A", 100, "A updated");
        existing.Id = seeded[0].Id;

        var added = NewProduct("C", 3);

        await using (var context = CreateContext())
            (await context.BulkInsertOrUpdateAsync([existing, added], c => c.SetOutputIdentity = true, CancellationToken)).Should().Be(2);

        added.Id.Should().BeGreaterThan(0);
        added.CreatedAt.Should().NotBe(default);
        existing.CreatedAt.Should().Be(seeded[0].CreatedAt, "the existing row keeps its value and it is read back");
        existing.NameLength.Should().Be("A updated".Length);

        await using var verify = CreateContext();

        var saved = await verify.Products.OrderBy(p => p.Sku).ToListAsync(CancellationToken);

        saved.Select(p => p.Sku).Should().Equal("A", "B", "C");
        saved[0].Name.Should().Be("A updated");
        saved[0].Price.Should().Be(100);
        saved[0].CreatedAt.Should().Be(seeded[0].CreatedAt);
        saved[1].Price.Should().Be(2);
        saved[2].Id.Should().Be(added.Id);
    }

    [Fact]
    public async Task BulkInsertOrUpdateAsync_ByNaturalKey_ReturnsIdsOfExistingRows()
    {
        var seeded = await SeedAsync(NewProduct("A", 1));

        var products = new[] { NewProduct("A", 10), NewProduct("B", 20) };

        await using (var context = CreateContext())
            await context.BulkInsertOrUpdateAsync(products, c =>
            {
                c.UpdateByProperties = [nameof(Product.Sku)];
                c.SetOutputIdentity = true;
            }, CancellationToken);

        products[0].Id.Should().Be(seeded[0].Id);
        products[1].Id.Should().BeGreaterThan(seeded[0].Id);

        await using var verify = CreateContext();

        (await verify.Products.OrderBy(p => p.Sku).Select(p => p.Price).ToListAsync(CancellationToken)).Should().Equal(10, 20);
    }

    [Fact]
    public async Task BulkInsertOrUpdateAsync_EmptyPropertiesToIncludeOnUpdate_OnlyInsertsNewRows()
    {
        await SeedAsync(NewProduct("A", 1));

        await using (var context = CreateContext())
        {
            var affected = await context.BulkInsertOrUpdateAsync([NewProduct("A", 10), NewProduct("B", 20)], c =>
            {
                c.UpdateByProperties = [nameof(Product.Sku)];
                c.PropertiesToIncludeOnUpdate = [];
            }, CancellationToken);

            affected.Should().Be(1);
        }

        await using var verify = CreateContext();

        (await verify.Products.OrderBy(p => p.Sku).Select(p => p.Price).ToListAsync(CancellationToken)).Should().Equal(1, 20);
    }

    [Fact]
    public async Task BulkInsertOrUpdateAsync_PropertiesToExcludeOnUpdate_KeepsThoseColumnsOnExistingRows()
    {
        await SeedAsync(NewProduct("A", 1, "Original"));

        await using (var context = CreateContext())
        {
            await context.BulkInsertOrUpdateAsync([NewProduct("A", 10, "Changed"), NewProduct("B", 20, "New")], c =>
            {
                c.UpdateByProperties = [nameof(Product.Sku)];
                c.PropertiesToExcludeOnUpdate = [nameof(Product.Name)];
            }, CancellationToken);
        }

        await using var verify = CreateContext();

        var saved = await verify.Products.OrderBy(p => p.Sku).ToListAsync(CancellationToken);

        saved[0].Name.Should().Be("Original");
        saved[0].Price.Should().Be(10);
        saved[1].Name.Should().Be("New", "new rows are inserted with every column");
    }

    [Fact]
    public async Task BulkInsertOrUpdateAsync_DuplicateKeysInInput_Throws()
    {
        await using var context = CreateContext();

        var act = () => context.BulkInsertOrUpdateAsync([NewProduct("A"), NewProduct("A")], c => c.UpdateByProperties = [nameof(Product.Sku)], CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*more than one entity*");
    }

    [Fact]
    public async Task BulkInsertOrUpdateAsync_CompositeKey()
    {
        await SeedAsync(new Inventory { WarehouseId = 1, ProductCode = "X", Quantity = 1 });

        await using (var context = CreateContext())
        {
            var affected = await context.BulkInsertOrUpdateAsync([new Inventory { WarehouseId = 1, ProductCode = "X", Quantity = 10 }, new Inventory { WarehouseId = 1, ProductCode = "Y", Quantity = 20 }],
                                                                 cancellationToken: CancellationToken);

            affected.Should().Be(2);
        }

        await using var verify = CreateContext();

        (await verify.Inventories.OrderBy(i => i.ProductCode).Select(i => i.Quantity).ToListAsync(CancellationToken)).Should().Equal(10, 20);
    }

    [Fact]
    public async Task BulkInsertOrUpdateAsync_ClientGeneratedKeys()
    {
        var existing = (await SeedAsync(new Category { Name = "Old" }))[0];

        var categories = new[] { new Category { Id = existing.Id, Name = "Renamed" }, new Category { Name = "New" } };

        await using (var context = CreateContext())
            (await context.BulkInsertOrUpdateAsync(categories, cancellationToken: CancellationToken)).Should().Be(2);

        categories[1].Id.Should().NotBeEmpty();

        await using var verify = CreateContext();

        (await verify.Categories.OrderBy(c => c.Name).Select(c => c.Name).ToListAsync(CancellationToken)).Should().Equal("New", "Renamed");
    }

    [Fact]
    public async Task BulkInsertOrUpdate_Sync_Works()
    {
        await SeedAsync(new Inventory { WarehouseId = 1, ProductCode = "X", Quantity = 1 });

        using (var context = CreateContext())
            context.BulkInsertOrUpdate([new Inventory { WarehouseId = 1, ProductCode = "X", Quantity = 5 }]).Should().Be(1);

        using var verify = CreateContext();

        verify.Inventories.Single().Quantity.Should().Be(5);
    }
}