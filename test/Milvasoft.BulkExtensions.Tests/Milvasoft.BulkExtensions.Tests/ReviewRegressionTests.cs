using Microsoft.EntityFrameworkCore;
using Milvasoft.BulkExtensions.PostgreSql;
using Milvasoft.BulkExtensions.Tests.Infrastructure;

namespace Milvasoft.BulkExtensions.Tests;

/// <summary>
/// Cases found by an independent correctness review.
/// </summary>
public class ReviewRegressionTests(PostgresFixture fixture) : BulkTestBase(fixture)
{
    [Fact]
    public async Task BulkSaveChangesAsync_AttachedStub_UpdatesOnlyModifiedColumns()
    {
        var seeded = (await SeedAsync(NewProduct("A", 1)))[0];

        await using (var context = CreateContext())
        {
            var stub = new Product { Id = seeded.Id, Sku = "A", Name = "stub", Price = 99, Version = seeded.Version };
            context.Attach(stub);
            context.Entry(stub).Property(p => p.Price).IsModified = true;

            (await context.BulkSaveChangesAsync(cancellationToken: CancellationToken)).Should().Be(1);
        }

        await using var verify = CreateContext();

        var saved = await verify.Products.SingleAsync(CancellationToken);

        saved.Price.Should().Be(99);
        saved.Name.Should().Be("Product A");
        saved.Tags.Should().Equal("a", "b");
        saved.ReleaseDate.Should().Be(seeded.ReleaseDate);
        saved.Size.Width.Should().Be(1.5);
        saved.CreatedAt.Should().Be(seeded.CreatedAt);
    }

    [Fact]
    public async Task BulkSaveChangesAsync_ComplexPropertyChange_UpdatesOnlyThatColumn()
    {
        await SeedAsync(NewProduct("A", 1));

        await using (var context = CreateContext())
        {
            var product = await context.Products.SingleAsync(CancellationToken);
            product.Size.Width = 42;

            await context.BulkSaveChangesAsync(cancellationToken: CancellationToken);
        }

        await using var verify = CreateContext();

        var saved = await verify.Products.SingleAsync(CancellationToken);

        saved.Size.Width.Should().Be(42);
        saved.Size.Height.Should().Be(2.5);
    }

    [Fact]
    public async Task BulkSaveChangesAsync_ConcurrencyTokenChanged_ThrowsAndKeepsChanges()
    {
        await SeedAsync(NewProduct("A", 1), NewProduct("B", 2));

        await using var context = CreateContext();

        var products = await context.Products.OrderBy(p => p.Sku).ToListAsync(CancellationToken);

        await using (var other = CreateContext())
        {
            var changed = await other.Products.SingleAsync(p => p.Sku == "A", CancellationToken);
            changed.Price = 50;
            await other.SaveChangesAsync(CancellationToken);
        }

        products[0].Price = 10;
        products[1].Price = 20;

        var act = () => context.BulkSaveChangesAsync(cancellationToken: CancellationToken);

        var thrown = await act.Should().ThrowAsync<DbUpdateConcurrencyException>();

        thrown.Which.Entries.Should().ContainSingle().Which.Entity.Should().BeSameAs(products[0]);
        context.Entry(products[1]).State.Should().Be(EntityState.Modified, "the failed save is rolled back and nothing is accepted");

        await using var verify = CreateContext();

        (await verify.Products.OrderBy(p => p.Sku).Select(p => p.Price).ToListAsync(CancellationToken)).Should().Equal(50, 2);
    }

    [Fact]
    public async Task BulkSaveChangesAsync_DeleteOfMissingRow_Throws()
    {
        var seeded = (await SeedAsync(new Inventory { WarehouseId = 1, ProductCode = "X" }))[0];

        await using var context = CreateContext();

        context.Remove(new Inventory { WarehouseId = 1, ProductCode = "missing" });

        var act = () => context.BulkSaveChangesAsync(cancellationToken: CancellationToken);

        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
        (await context.Inventories.CountAsync(CancellationToken)).Should().Be(1);
        seeded.Should().NotBeNull();
    }

    [Fact]
    public async Task BulkSaveChangesAsync_ReadsBackNewVersionAfterUpdate()
    {
        await SeedAsync(NewProduct("A", 1));

        await using var context = CreateContext();

        var product = await context.Products.SingleAsync(CancellationToken);
        var oldVersion = product.Version;

        product.Price = 7;
        await context.BulkSaveChangesAsync(cancellationToken: CancellationToken);

        product.Version.Should().NotBe(oldVersion);

        product.Price = 8;
        (await context.BulkSaveChangesAsync(cancellationToken: CancellationToken)).Should().Be(1, "the second save uses the new version as the concurrency token");
    }

    [Fact]
    public async Task BulkInsertOrUpdateAsync_ByNaturalKey_ExistingRowKeepsAndReturnsItsClientGeneratedKey()
    {
        var existing = (await SeedAsync(new Category { Name = "Books" }))[0];
        var categories = new[] { new Category { Name = "Books" }, new Category { Name = "Music" } };

        await using (var context = CreateContext())
            await context.BulkInsertOrUpdateAsync(categories, c => c.UpdateByProperties = [nameof(Category.Name)], CancellationToken);

        categories[0].Id.Should().Be(existing.Id, "the existing row keeps its key and the entity gets it, even without SetOutputIdentity");

        await using var verify = CreateContext();

        (await verify.Categories.SingleAsync(c => c.Name == "Music", CancellationToken)).Id.Should().Be(categories[1].Id);
    }

    [Fact]
    public async Task BulkInsertAsync_Failure_DoesNotAssignClientGeneratedKeys()
    {
        await SeedAsync(new Category { Name = "Taken" });

        var categories = new[] { new Category { Name = "Free" }, new Category { Name = "Taken" } };

        await using var context = CreateContext();

        var act = () => context.BulkInsertAsync(categories, cancellationToken: CancellationToken);

        await act.Should().ThrowAsync<Npgsql.PostgresException>();
        categories.Should().OnlyContain(c => c.Id == Guid.Empty);
    }

    [Fact]
    public async Task BulkInsertAsync_TrackedShadowForeignKeyToUnsavedPrincipal_Throws()
    {
        await using var context = CreateContext();

        var review = new Review { Text = "r", Product = NewProduct("unsaved") };
        context.Add(review);

        var act = () => context.BulkInsertAsync([review], cancellationToken: CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*has not been saved*");
    }

    [Fact]
    public async Task BulkUpdateAndDeleteAsync_NullMatchValue_MatchesNoRow()
    {
        await SeedAsync(NewProduct("A", 1), NewProduct("B", 2));

        await using (var context = CreateContext())
        {
            var updated = await context.BulkUpdateAsync([new Product { Description = null, Price = 100 }], c =>
            {
                c.UpdateByProperties = [nameof(Product.Description)];
                c.PropertiesToInclude = [nameof(Product.Price)];
            }, CancellationToken);

            var deleted = await context.BulkDeleteAsync([new Product { Description = null }], c => c.UpdateByProperties = [nameof(Product.Description)], CancellationToken);

            updated.Should().Be(0);
            deleted.Should().Be(0);
        }

        await using var verify = CreateContext();

        (await verify.Products.OrderBy(p => p.Sku).Select(p => p.Price).ToListAsync(CancellationToken)).Should().Equal(1, 2);
    }

    [Fact]
    public async Task BulkInsertOrUpdateAsync_NullMatchValue_IsInsertedAndReadBack()
    {
        await using (var context = CreateContext())
        {
            var products = new[] { NewProduct("A"), NewProduct("B") };

            await context.BulkInsertOrUpdateAsync(products, c =>
            {
                c.UpdateByProperties = [nameof(Product.Description)];
                c.SetOutputIdentity = true;
            }, CancellationToken);

            products.Should().OnlyContain(p => p.Id > 0);
        }

        await using var verify = CreateContext();

        (await verify.Products.CountAsync(CancellationToken)).Should().Be(2);
    }

    [Fact]
    public async Task BulkUpdateAsync_PaddedCharacterKey_ReadsBackGeneratedValues()
    {
        var currency = (await SeedAsync(new Currency { Code = "TR", Name = "Lira" }))[0];
        var oldVersion = currency.Version;

        var change = new Currency { Code = "TR", Name = "Turkish lira" };

        await using (var context = CreateContext())
            (await context.BulkUpdateAsync([change], c => c.SetOutputIdentity = true, CancellationToken)).Should().Be(1);

        change.Version.Should().NotBe(oldVersion);
    }

    [Fact]
    public async Task BulkSaveChangesAsync_ForeignKeyToTpcBaseType_SavesPrincipalFirst()
    {
        var car = new Car { Plate = "06 ABC 1", Seats = 4 };

        await using (var context = CreateContext())
        {
            context.Add(new Trip { Vehicle = car });
            context.Add(new Truck { Plate = "34 XYZ 2", Axles = 3 });

            (await context.BulkSaveChangesAsync(cancellationToken: CancellationToken)).Should().Be(3);
        }

        await using var verify = CreateContext();

        (await verify.Trips.SingleAsync(CancellationToken)).VehicleId.Should().Be(car.Id);
        (await verify.Vehicles.CountAsync(CancellationToken)).Should().Be(2);
    }

    [Theory]
    [InlineData("character varying(100)", "character varying")]
    [InlineData("numeric(18,2)", "numeric")]
    [InlineData("timestamp(3) with time zone", "timestamp with time zone")]
    [InlineData("geometry(Point,4326)", "geometry")]
    [InlineData("geography (point, 4326)", "geography")]
    [InlineData("character varying(50)[]", "character varying[]")]
    [InlineData("\"MySchema\".\"Mood\"", "MySchema.Mood")]
    [InlineData("\"char\"", "\"char\"")]
    public void GetDataTypeName_RemovesTypeModifiersAndQuotes(string storeType, string expected)
        => NpgsqlValueWriter.GetDataTypeName(storeType).Should().Be(expected);
}