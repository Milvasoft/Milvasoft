using Microsoft.EntityFrameworkCore;
using Milvasoft.BulkExtensions.Tests.Infrastructure;

namespace Milvasoft.BulkExtensions.Tests;

public class InsertTests(PostgresFixture fixture) : BulkTestBase(fixture)
{
    [Fact]
    public async Task BulkInsertAsync_WritesEveryMappedValue()
    {
        var products = new[]
        {
            NewProduct("A-1", 12.34m),
            NewProduct("A-2", 0m),
            new Product { Sku = "A-3", Name = "Ünicode ğüşiöç", Description = "has \"quotes\" and \\ backslash", Price = 99999.99m, Status = ProductStatus.Discontinued, Tags = [], Mood = null },
        };

        await using (var context = CreateContext())
        {
            var inserted = await context.BulkInsertAsync(products, cancellationToken: CancellationToken);

            inserted.Should().Be(3);
        }

        await using var verify = CreateContext();

        var saved = await verify.Products.OrderBy(p => p.Sku).ToListAsync(CancellationToken);

        saved.Should().HaveCount(3);
        saved[0].Should().BeEquivalentTo(products[0], o => o.Excluding(p => p.Id).Excluding(p => p.CreatedAt).Excluding(p => p.NameLength).Excluding(p => p.Version));
        saved[2].Name.Should().Be("Ünicode ğüşiöç");
        saved[2].Description.Should().Be("has \"quotes\" and \\ backslash");
        saved[2].Status.Should().Be(ProductStatus.Discontinued);
        saved[2].Mood.Should().BeNull();
        saved[2].Tags.Should().BeEmpty();
        saved.Should().OnlyContain(p => p.Id > 0);
        saved.Should().OnlyContain(p => p.CreatedAt > DateTime.UtcNow.AddMinutes(-5), "the database default fills it");
        saved.Should().OnlyContain(p => p.NameLength == p.Name.Length, "the computed column is calculated by the database");
    }

    [Fact]
    public async Task BulkInsertAsync_WithSetOutputIdentity_WritesGeneratedValuesBackInInputOrder()
    {
        var products = Enumerable.Range(1, 50).Select(i => NewProduct($"P-{i:000}", name: new string('x', i))).ToList();

        await using (var context = CreateContext())
            await context.BulkInsertAsync(products, c => c.SetOutputIdentity = true, CancellationToken);

        products.Select(p => p.Id).Should().OnlyHaveUniqueItems().And.NotContain(0);
        products.Should().OnlyContain(p => p.NameLength == p.Name.Length);
        products.Should().OnlyContain(p => p.CreatedAt != default);
        products.Should().OnlyContain(p => p.Version != 0);

        await using var verify = CreateContext();

        var saved = await verify.Products.ToDictionaryAsync(p => p.Id, CancellationToken);

        foreach (var product in products)
        {
            saved[product.Id].Sku.Should().Be(product.Sku);
            saved[product.Id].CreatedAt.Should().Be(product.CreatedAt);
            saved[product.Id].Version.Should().Be(product.Version);
        }
    }

    [Fact]
    public async Task BulkInsertAsync_WithoutSetOutputIdentity_DoesNotChangeEntities()
    {
        var products = new List<Product> { NewProduct("A"), NewProduct("B") };

        await using var context = CreateContext();

        await context.BulkInsertAsync(products, cancellationToken: CancellationToken);

        products.Should().OnlyContain(p => p.Id == 0 && p.NameLength == 0);
        context.ChangeTracker.Entries().Should().BeEmpty("bulk operations do not track entities");
    }

    [Fact]
    public async Task BulkInsertAsync_EmptyInput_ReturnsZero()
    {
        await using var context = CreateContext();

        (await context.BulkInsertAsync(Array.Empty<Product>(), cancellationToken: CancellationToken)).Should().Be(0);
    }

    [Fact]
    public async Task BulkInsertAsync_ExplicitAndGeneratedKeysInSameCall_UsesExplicitKeysAndGeneratesTheRest()
    {
        var explicitKey = NewProduct("explicit");
        explicitKey.Id = 1000;

        var generated = new[] { NewProduct("gen-1"), NewProduct("gen-2") };
        var products = new[] { generated[0], explicitKey, generated[1] };

        await using (var context = CreateContext())
            (await context.BulkInsertAsync(products, c => c.SetOutputIdentity = true, CancellationToken)).Should().Be(3);

        explicitKey.Id.Should().Be(1000);
        generated.Should().OnlyContain(p => p.Id > 0 && p.Id != 1000);

        await using var verify = CreateContext();

        (await verify.Products.SingleAsync(p => p.Id == 1000, CancellationToken)).Sku.Should().Be("explicit");
        (await verify.Products.SingleAsync(p => p.Id == generated[1].Id, CancellationToken)).Sku.Should().Be("gen-2");
    }

    [Fact]
    public async Task BulkInsertAsync_GeneratesClientSideGuidKeys()
    {
        var categories = new[] { new Category { Name = "One" }, new Category { Name = "Two" } };
        var preset = new Category { Id = Guid.NewGuid(), Name = "Preset" };
        var presetId = preset.Id;

        await using (var context = CreateContext())
            await context.BulkInsertAsync(categories.Append(preset), cancellationToken: CancellationToken);

        categories.Should().OnlyContain(c => c.Id != Guid.Empty);
        categories[0].Id.Should().NotBe(categories[1].Id);
        preset.Id.Should().Be(presetId);

        await using var verify = CreateContext();

        (await verify.Categories.Select(c => c.Id).ToListAsync(CancellationToken)).Should().BeEquivalentTo(categories.Select(c => c.Id).Append(presetId));
    }

    [Fact]
    public async Task BulkInsertAsync_TphHierarchy_WritesDiscriminatorPerType()
    {
        var animals = new List<Animal>
        {
            new Dog { Name = "Rex", Breed = "Collie" },
            new Cat { Name = "Tom", Lives = 9 },
            new Dog { Name = "Fido", Breed = "Pug" },
        };

        await using (var context = CreateContext())
            await context.BulkInsertAsync(animals, c => c.SetOutputIdentity = true, CancellationToken);

        animals.Should().OnlyContain(a => a.Id > 0);

        await using var verify = CreateContext();

        var dogs = await verify.Animals.OfType<Dog>().OrderBy(d => d.Name).ToListAsync(CancellationToken);
        var cat = await verify.Animals.OfType<Cat>().SingleAsync(CancellationToken);

        dogs.Select(d => (d.Name, d.Breed)).Should().Equal(("Fido", "Pug"), ("Rex", "Collie"));
        cat.Lives.Should().Be(9);
        cat.Id.Should().Be(animals[1].Id);
    }

    [Fact]
    public async Task BulkInsertAsync_DerivedTypeList_Works()
    {
        var dogs = new List<Dog> { new() { Name = "A", Breed = "X" }, new() { Name = "B", Breed = "Y" } };

        await using (var context = CreateContext())
            (await context.BulkInsertAsync(dogs, cancellationToken: CancellationToken)).Should().Be(2);

        await using var verify = CreateContext();

        (await verify.Animals.OfType<Dog>().CountAsync(CancellationToken)).Should().Be(2);
    }

    [Fact]
    public async Task BulkInsertAsync_OwnedTypeInSameTable_IsWrittenWithOwner()
    {
        var orders = new[]
        {
            new Order { Number = "O-1", ShippingAddress = new Address { Street = "Main 1", City = "Ankara" } },
            new Order { Number = "O-2", ShippingAddress = null },
        };

        await using (var context = CreateContext())
            await context.BulkInsertAsync(orders, cancellationToken: CancellationToken);

        await using var verify = CreateContext();

        var saved = await verify.Orders.OrderBy(o => o.Number).ToListAsync(CancellationToken);

        saved[0].ShippingAddress.Should().BeEquivalentTo(orders[0].ShippingAddress);
        saved[1].ShippingAddress.Should().BeNull();
    }

    [Fact]
    public async Task BulkInsertAsync_ShadowForeignKey_IsTakenFromNavigation()
    {
        var product = (await SeedAsync(NewProduct("reviewed")))[0];
        var reviews = new[] { new Review { Text = "Good", Product = new Product { Id = product.Id } }, new Review { Text = "Bad", Product = new Product { Id = product.Id } } };

        await using (var context = CreateContext())
            await context.BulkInsertAsync(reviews, cancellationToken: CancellationToken);

        await using var verify = CreateContext();

        var productIds = await verify.Reviews.Select(r => EF.Property<int>(r, "ProductId")).ToListAsync(CancellationToken);

        productIds.Should().Equal(product.Id, product.Id);
    }

    [Fact]
    public async Task BulkInsertAsync_PropertiesToExclude_LeavesColumnsNull()
    {
        var product = NewProduct("A");
        product.Description = "will not be written";

        await using (var context = CreateContext())
            await context.BulkInsertAsync([product], c => c.PropertiesToExclude = [nameof(Product.Description), $"{nameof(Product.Size)}.{nameof(Dimensions.Width)}"], CancellationToken);

        await using var verify = CreateContext();

        var saved = await verify.Products.SingleAsync(CancellationToken);

        saved.Description.Should().BeNull();
        saved.Size.Width.Should().Be(0);
        saved.Size.Height.Should().Be(2.5);
    }

    [Fact]
    public async Task BulkInsertAsync_UnknownPropertyName_Throws()
    {
        await using var context = CreateContext();

        var act = () => context.BulkInsertAsync([NewProduct("A")], c => c.PropertiesToExclude = ["DoesNotExist"], CancellationToken);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*DoesNotExist*");
        (await context.Products.CountAsync(CancellationToken)).Should().Be(0);
    }

    [Fact]
    public async Task BulkInsertAsync_NullElement_Throws()
    {
        await using var context = CreateContext();

        var act = () => context.BulkInsertAsync(new Product[] { NewProduct("A"), null }, cancellationToken: CancellationToken);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task BulkInsertAsync_EveryColumnGeneratedByDatabase_InsertsDefaultRows()
    {
        var tickets = Enumerable.Range(0, 5).Select(_ => new Ticket()).ToList();

        await using (var context = CreateContext())
            (await context.BulkInsertAsync(tickets, c => c.SetOutputIdentity = true, CancellationToken)).Should().Be(5);

        tickets.Select(t => t.Id).Should().Equal(1, 2, 3, 4, 5);
        tickets.Should().OnlyContain(t => t.IssuedAt != default);

        await using (var context = CreateContext())
            (await context.BulkInsertAsync([new Ticket(), new Ticket()], cancellationToken: CancellationToken)).Should().Be(2);

        await using var verify = CreateContext();

        (await verify.Tickets.CountAsync(CancellationToken)).Should().Be(7);
    }

    [Fact]
    public async Task BulkInsertAsync_UniqueViolation_RollsBackWholeOperation()
    {
        await SeedAsync(NewProduct("taken"));

        await using var context = CreateContext();

        var act = () => context.BulkInsertAsync([NewProduct("new-1"), NewProduct("taken")], cancellationToken: CancellationToken);

        await act.Should().ThrowAsync<Npgsql.PostgresException>().Where(e => e.SqlState == "23505");
        (await context.Products.CountAsync(CancellationToken)).Should().Be(1);
    }

    [Fact]
    public async Task BulkInsertAsync_InsideUserTransaction_JoinsIt()
    {
        await using (var context = CreateContext())
        {
            await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken);

            await context.BulkInsertAsync([NewProduct("A"), NewProduct("B")], c => c.SetOutputIdentity = true, CancellationToken);
            await context.BulkInsertAsync([NewProduct("C")], cancellationToken: CancellationToken);

            (await context.Products.CountAsync(CancellationToken)).Should().Be(3, "the rows are visible inside the transaction");

            await transaction.RollbackAsync(CancellationToken);
        }

        await using var verify = CreateContext();

        (await verify.Products.CountAsync(CancellationToken)).Should().Be(0);
    }

    [Fact]
    public async Task BulkInsertAsync_WithRetryingExecutionStrategy_Works()
    {
        await using (var context = CreateContext(configure: o => o.EnableRetryOnFailure()))
            await context.BulkInsertAsync([NewProduct("A")], c => c.SetOutputIdentity = true, CancellationToken);

        await using var verify = CreateContext();

        (await verify.Products.CountAsync(CancellationToken)).Should().Be(1);
    }

    [Fact]
    public async Task BulkInsertAsync_WithoutRegistration_FindsProviderByConvention()
    {
        await using (var context = CreateContext(registerBulkProvider: false))
            (await context.BulkInsertAsync([NewProduct("A")], cancellationToken: CancellationToken)).Should().Be(1);

        await using var verify = CreateContext();

        (await verify.Products.CountAsync(CancellationToken)).Should().Be(1);
    }

    [Fact]
    public async Task BulkInsertAsync_ManyRows()
    {
        var products = Enumerable.Range(0, 20_000).Select(i => NewProduct($"bulk-{i}", i)).ToList();

        await using (var context = CreateContext())
            (await context.BulkInsertAsync(products, c => c.SetOutputIdentity = true, CancellationToken)).Should().Be(20_000);

        products.Select(p => p.Id).Should().OnlyHaveUniqueItems();

        await using var verify = CreateContext();

        (await verify.Products.CountAsync(CancellationToken)).Should().Be(20_000);
        (await verify.Products.SingleAsync(p => p.Id == products[12_345].Id, CancellationToken)).Sku.Should().Be("bulk-12345");
    }

    [Fact]
    public void BulkInsert_Sync_Works()
    {
        var products = new List<Product> { NewProduct("A"), NewProduct("B") };

        using (var context = CreateContext())
            context.BulkInsert(products, new BulkConfig { SetOutputIdentity = true }).Should().Be(2);

        products.Should().OnlyContain(p => p.Id > 0);

        using var verify = CreateContext();

        verify.Products.Count().Should().Be(2);
    }
}