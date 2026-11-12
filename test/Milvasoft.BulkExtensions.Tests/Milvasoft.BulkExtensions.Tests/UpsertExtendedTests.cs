using Microsoft.EntityFrameworkCore;
using Milvasoft.BulkExtensions.Tests.Infrastructure;

namespace Milvasoft.BulkExtensions.Tests;

public class UpsertExtendedTests(PostgresFixture fixture) : BulkTestBase(fixture)
{
    [Fact]
    public async Task AllRowsExisting_ReadsBackKeysAndVersions()
    {
        var seeded = await SeedAsync(NewProduct("A", 1), NewProduct("B", 2));
        var products = new[] { NewProduct("B", 20), NewProduct("A", 10) };

        await using (var context = CreateContext())
        {
            (await context.BulkInsertOrUpdateAsync(products, c =>
            {
                c.UpdateByProperties = [nameof(Product.Sku)];
                c.SetOutputIdentity = true;
            }, CancellationToken)).Should().Be(2);
        }

        products[0].Id.Should().Be(seeded[1].Id);
        products[1].Id.Should().Be(seeded[0].Id);
        products[0].Version.Should().NotBe(seeded[1].Version);
        products[0].CreatedAt.Should().Be(seeded[1].CreatedAt);
    }

    [Fact]
    public async Task ExplicitKeyThatDoesNotExist_IsInsertedWithThatKey()
    {
        var product = NewProduct("A");
        product.Id = 777;

        await using (var context = CreateContext())
            (await context.BulkInsertOrUpdateAsync([product], cancellationToken: CancellationToken)).Should().Be(1);

        await using var verify = CreateContext();

        (await verify.Products.SingleAsync(CancellationToken)).Id.Should().Be(777);
    }

    [Fact]
    public async Task TphEntities()
    {
        var dog = new Dog { Name = "Rex", Breed = "Collie" };
        await SeedAsync<Animal>(dog);

        var animals = new List<Animal> { new Dog { Id = dog.Id, Name = "Rex", Breed = "Pug" }, new Cat { Name = "Tom", Lives = 9 } };

        await using (var context = CreateContext())
            (await context.BulkInsertOrUpdateAsync(animals, c => c.SetOutputIdentity = true, CancellationToken)).Should().Be(2);

        animals[1].Id.Should().BeGreaterThan(0);

        await using var verify = CreateContext();

        (await verify.Animals.OfType<Dog>().SingleAsync(CancellationToken)).Breed.Should().Be("Pug");
        (await verify.Animals.OfType<Cat>().SingleAsync(CancellationToken)).Id.Should().Be(animals[1].Id);
    }

    [Fact]
    public async Task OwnedTypes()
    {
        var order = (await SeedAsync(new Order { Number = "O-1", ShippingAddress = new Address { City = "Old" } }))[0];

        await using (var context = CreateContext())
        {
            await context.BulkInsertOrUpdateAsync([new Order { Id = order.Id, Number = "O-1", ShippingAddress = new Address { City = "New" } }, new Order { Number = "O-2" }],
                                                  cancellationToken: CancellationToken);
        }

        await using var verify = CreateContext();

        var saved = await verify.Orders.OrderBy(o => o.Number).ToListAsync(CancellationToken);

        saved[0].ShippingAddress.City.Should().Be("New");
        saved[1].ShippingAddress.Should().BeNull();
    }

    [Fact]
    public async Task PropertiesToIncludeOnUpdate_UpdatesOnlyThoseColumns()
    {
        await SeedAsync(NewProduct("A", 1, "Old name"));

        await using (var context = CreateContext())
        {
            await context.BulkInsertOrUpdateAsync([NewProduct("A", 50, "New name")], c =>
            {
                c.UpdateByProperties = [nameof(Product.Sku)];
                c.PropertiesToIncludeOnUpdate = [nameof(Product.Price)];
            }, CancellationToken);
        }

        await using var verify = CreateContext();

        var saved = await verify.Products.SingleAsync(CancellationToken);

        saved.Price.Should().Be(50);
        saved.Name.Should().Be("Old name");
    }

    [Fact]
    public async Task MatchByNonUniqueColumns_FailsAtTheDatabase()
    {
        await using var context = CreateContext();

        var act = () => context.BulkInsertOrUpdateAsync([NewProduct("A")], c => c.UpdateByProperties = [nameof(Product.Name)], CancellationToken);

        await act.Should().ThrowAsync<Npgsql.PostgresException>().Where(e => e.SqlState == "42P10", "there is no unique constraint for ON CONFLICT");
    }

    [Fact]
    public async Task ManyRows_HalfExisting()
    {
        await using (var context = CreateContext())
            await context.BulkInsertAsync(Enumerable.Range(0, 10_000).Select(i => NewProduct($"p{i}", i)), cancellationToken: CancellationToken);

        var products = Enumerable.Range(5_000, 10_000).Select(i => NewProduct($"p{i}", i * 2)).ToList();

        await using (var context = CreateContext())
        {
            (await context.BulkInsertOrUpdateAsync(products, c =>
            {
                c.UpdateByProperties = [nameof(Product.Sku)];
                c.SetOutputIdentity = true;
            }, CancellationToken)).Should().Be(10_000);
        }

        products.Select(p => p.Id).Should().OnlyHaveUniqueItems().And.NotContain(0);

        await using var verify = CreateContext();

        (await verify.Products.CountAsync(CancellationToken)).Should().Be(15_000);
        (await verify.Products.SingleAsync(p => p.Sku == "p7000", CancellationToken)).Should().Match<Product>(p => p.Price == 14_000 && p.Id == products[2000].Id);
        (await verify.Products.SingleAsync(p => p.Sku == "p100", CancellationToken)).Price.Should().Be(100);
    }

    [Fact]
    public async Task ClientGeneratedKeyWithSetOutputIdentity_ReturnsExistingKey()
    {
        var existing = (await SeedAsync(new Category { Name = "Books" }))[0];
        var category = new Category { Name = "Books" };

        await using (var context = CreateContext())
        {
            await context.BulkInsertOrUpdateAsync([category], c =>
            {
                c.UpdateByProperties = [nameof(Category.Name)];
                c.SetOutputIdentity = true;
            }, CancellationToken);
        }

        category.Id.Should().Be(existing.Id);
    }
}