using Microsoft.EntityFrameworkCore;
using Milvasoft.BulkExtensions.Tests.Infrastructure;
using System.Net;

namespace Milvasoft.BulkExtensions.Tests;

/// <summary>
/// Column types, naming and model mappings.
/// </summary>
public class MappingTests(PostgresFixture fixture) : BulkTestBase(fixture)
{
    private static TypeZoo NewZoo(int seed) => new()
    {
        Flag = seed % 2 == 0,
        Small = (short)(seed * -3),
        Big = long.MaxValue - seed,
        Single = 1.25f * seed,
        Double = Math.PI * seed,
        Money = 123456.789012m + seed,
        Letter = (char)('a' + seed),
        // jsonb stores keys in its own order; this is that order.
        Json = $$"""{"list": [1, 2, 3], "seed": {{seed}}}""",
        Bytes = [0, (byte)seed, 255],
        Numbers = [seed, -seed, int.MaxValue],
        Guids = [Guid.NewGuid(), Guid.NewGuid()],
        MaybeGuid = seed % 2 == 0 ? null : Guid.NewGuid(),
        UtcTime = new DateTime(2026, 3, 4, 5, 6, 7, DateTimeKind.Utc).AddTicks(seed * 10),
        LocalTime = new DateTime(2026, 3, 4, 5, 6, 7, DateTimeKind.Unspecified).AddTicks(seed * 10),
        Offset = new DateTimeOffset(2026, 3, 4, 5, 6, 7, TimeSpan.Zero),
        Date = new DateOnly(2026, 2, 28),
        Time = new TimeOnly(23, 59, 58, 123),
        Interval = TimeSpan.FromHours(seed) + TimeSpan.FromMilliseconds(5),
        EnumAsInt = (DayOfWeek)(seed % 7),
        Address = IPAddress.Parse($"10.0.0.{seed}"),
        Unicode = "Çğıöşü 日本語 🚀 \\ \" ' \t newline\n end",
    };

    [Fact]
    public async Task EveryColumnType_RoundTripsThroughInsertUpdateUpsertAndRead()
    {
        var zoos = Enumerable.Range(1, 5).Select(NewZoo).ToList();

        await using (var context = CreateContext())
            (await context.BulkInsertAsync(zoos, c => c.SetOutputIdentity = true, CancellationToken)).Should().Be(5);

        await using (var verify = CreateContext())
        {
            var saved = await verify.TypeZoos.OrderBy(z => z.Id).ToListAsync(CancellationToken);

            saved.Should().BeEquivalentTo(zoos, o => o.WithStrictOrdering().Excluding(z => z.Json));
            saved.Select(z => z.Json.Replace(" ", "")).Should().Equal(zoos.Select(z => z.Json.Replace(" ", "")));
        }

        var changed = zoos.Select(z => NewZoo(z.Id + 10)).ToList();

        for (int i = 0; i < changed.Count; i++)
            changed[i].Id = zoos[i].Id;

        await using (var context = CreateContext())
            (await context.BulkUpdateAsync(changed, cancellationToken: CancellationToken)).Should().Be(5);

        await using (var verify = CreateContext())
            (await verify.TypeZoos.OrderBy(z => z.Id).ToListAsync(CancellationToken)).Should().BeEquivalentTo(changed, o => o.WithStrictOrdering().Excluding(z => z.Json));

        var upserted = changed.Take(2).Select(z => NewZoo(z.Id + 20)).ToList();
        upserted[0].Id = changed[0].Id;

        await using (var context = CreateContext())
            (await context.BulkInsertOrUpdateAsync(upserted, c => c.SetOutputIdentity = true, CancellationToken)).Should().Be(2);

        upserted[1].Id.Should().BeGreaterThan(5);

        await using (var context = CreateContext())
        {
            var read = await context.BulkReadAsync(upserted.Select(u => new TypeZoo { Id = u.Id }), cancellationToken: CancellationToken);

            read.OrderBy(z => z.Id).Should().BeEquivalentTo(upserted, o => o.WithStrictOrdering().Excluding(z => z.Json));
        }
    }

    [Fact]
    public async Task SchemaAndIdentifiersThatNeedQuoting_Work()
    {
        var items = new[] { new OddName { Value = "one" }, new OddName { Value = "two \"quoted\"" } };

        await using (var context = CreateContext())
        {
            await context.BulkInsertAsync(items, c => c.SetOutputIdentity = true, CancellationToken);

            items[0].Value = "ONE";
            await context.BulkUpdateAsync(items, cancellationToken: CancellationToken);
            await context.BulkInsertOrUpdateAsync([new OddName { Id = items[1].Id, Value = "TWO" }, new OddName { Value = "three" }], cancellationToken: CancellationToken);

            (await context.BulkReadAsync([new OddName { Id = items[0].Id }], cancellationToken: CancellationToken)).Single().Value.Should().Be("ONE");

            (await context.BulkDeleteAsync([items[0]], cancellationToken: CancellationToken)).Should().Be(1);
        }

        await using var verify = CreateContext();

        (await verify.OddNames.OrderBy(o => o.Id).Select(o => o.Value).ToListAsync(CancellationToken)).Should().Equal("TWO", "three");

        await using (var context = CreateContext())
            await context.TruncateAsync<OddName>(CancellationToken);

        (await verify.OddNames.CountAsync(CancellationToken)).Should().Be(0);
    }

    [Fact]
    public async Task KeyWithValueConverter_WorksForEveryOperation()
    {
        var coupons = new[] { new Coupon { Code = new CouponCode("SAVE10"), Discount = 10 }, new Coupon { Code = new CouponCode("SAVE20"), Discount = 20 } };

        await using var context = CreateContext();

        (await context.BulkInsertAsync(coupons, cancellationToken: CancellationToken)).Should().Be(2);
        (await context.BulkUpdateAsync([new Coupon { Code = new CouponCode("SAVE10"), Discount = 15 }], cancellationToken: CancellationToken)).Should().Be(1);
        (await context.BulkInsertOrUpdateAsync([new Coupon { Code = new CouponCode("SAVE20"), Discount = 25 }, new Coupon { Code = new CouponCode("NEW"), Discount = 5 }],
                                               c => c.SetOutputIdentity = true,
                                               CancellationToken)).Should().Be(2);

        (await context.BulkReadAsync([new Coupon { Code = new CouponCode("SAVE10") }], cancellationToken: CancellationToken)).Single().Discount.Should().Be(15);
        (await context.BulkDeleteAsync([new Coupon { Code = new CouponCode("NEW") }], cancellationToken: CancellationToken)).Should().Be(1);

        (await context.Coupons.AsNoTracking().OrderBy(c => c.Discount).Select(c => c.Discount).ToListAsync(CancellationToken)).Should().Equal(15, 25);
    }

    [Fact]
    public async Task ByteArrayKey_IsComparedByValue()
    {
        await using var context = CreateContext();

        await context.BulkInsertAsync([new BlobItem { Hash = [1, 2, 3], Data = "a" }, new BlobItem { Hash = [4, 5], Data = "b" }], cancellationToken: CancellationToken);

        var update = () => context.BulkUpdateAsync([new BlobItem { Hash = [1, 2, 3], Data = "x" }, new BlobItem { Hash = [1, 2, 3], Data = "y" }], cancellationToken: CancellationToken);

        await update.Should().ThrowAsync<InvalidOperationException>("two inputs have equal byte array keys");

        (await context.BulkInsertOrUpdateAsync([new BlobItem { Hash = [1, 2, 3], Data = "A" }, new BlobItem { Hash = [9], Data = "new" }],
                                               c => c.SetOutputIdentity = true,
                                               CancellationToken)).Should().Be(2);

        (await context.BlobItems.AsNoTracking().OrderBy(b => b.Data).Select(b => b.Data).ToListAsync(CancellationToken)).Should().Equal("A", "b", "new");
    }

    [Fact]
    public async Task HiLoKeys_AreGeneratedOnTheClient()
    {
        var items = Enumerable.Range(0, 25).Select(i => new HiLoItem { Name = $"item {i}" }).ToList();

        await using (var context = CreateContext())
            await context.BulkInsertAsync(items, cancellationToken: CancellationToken);

        items.Select(i => i.Id).Should().OnlyHaveUniqueItems().And.NotContain(0);

        await using var verify = CreateContext();

        (await verify.HiLoItems.ToDictionaryAsync(i => i.Id, i => i.Name, CancellationToken)).Should().BeEquivalentTo(items.ToDictionary(i => i.Id, i => i.Name));
    }

    [Fact]
    public async Task OwnedTypeOfDerivedTphType_IsWrittenAndRead()
    {
        var animals = new List<Animal>
        {
            new Dog { Name = "Rex", Breed = "Collie", Collar = new Collar { Color = "red", Size = 3 } },
            new Cat { Name = "Tom", Lives = 9 },
            new Dog { Name = "Fido", Breed = "Pug" },
        };

        await using (var context = CreateContext())
        {
            await context.BulkInsertAsync(animals, c => c.SetOutputIdentity = true, CancellationToken);

            await context.BulkUpdateAsync([new Dog { Id = animals[2].Id, Name = "Fido", Breed = "Pug", Collar = new Collar { Color = "blue", Size = 1 } }], cancellationToken: CancellationToken);
        }

        await using var verify = CreateContext();

        var dogs = await verify.Animals.OfType<Dog>().OrderBy(d => d.Name).ToListAsync(CancellationToken);

        dogs[0].Collar.Should().BeEquivalentTo(new Collar { Color = "blue", Size = 1 });
        dogs[1].Collar.Should().BeEquivalentTo(new Collar { Color = "red", Size = 3 });
        (await verify.Animals.OfType<Cat>().SingleAsync(CancellationToken)).Lives.Should().Be(9);
    }

    [Fact]
    public async Task PropertiesToInclude_NavigationNameSelectsAllOwnedColumns()
    {
        var order = (await SeedAsync(new Order { Number = "O-1", ShippingAddress = new Address { Street = "S", City = "C" } }))[0];

        await using (var context = CreateContext())
        {
            await context.BulkUpdateAsync([new Order { Id = order.Id, Number = "ignored", ShippingAddress = new Address { Street = "S2", City = "C2" } }],
                                          c => c.PropertiesToInclude = [nameof(Order.ShippingAddress)],
                                          CancellationToken);
        }

        await using var verify = CreateContext();

        var saved = await verify.Orders.SingleAsync(CancellationToken);

        saved.Number.Should().Be("O-1");
        saved.ShippingAddress.Should().BeEquivalentTo(new Address { Street = "S2", City = "C2" });
    }

    [Fact]
    public async Task PropertiesToIncludeAndExclude_AreCombined()
    {
        var seeded = (await SeedAsync(NewProduct("A", 1)))[0];

        await using (var context = CreateContext())
        {
            var change = new Product { Id = seeded.Id, Name = "new name", Price = 9, Description = "d" };

            await context.BulkUpdateAsync([change], c =>
            {
                c.PropertiesToInclude = [nameof(Product.Name), nameof(Product.Price)];
                c.PropertiesToExclude = [nameof(Product.Name)];
            }, CancellationToken);
        }

        await using var verify = CreateContext();

        var saved = await verify.Products.SingleAsync(CancellationToken);

        saved.Price.Should().Be(9);
        saved.Name.Should().Be("Product A");
        saved.Description.Should().BeNull();
    }

    [Fact]
    public async Task ExcludingTheDiscriminator_IsIgnored()
    {
        await using (var context = CreateContext())
            await context.BulkInsertAsync(new List<Animal> { new Cat { Name = "Tom", Lives = 1 } }, c => c.PropertiesToExclude = ["Kind"], CancellationToken);

        await using var verify = CreateContext();

        (await verify.Animals.SingleAsync(CancellationToken)).Should().BeOfType<Cat>();
    }

    [Fact]
    public async Task UnknownUpdateByPropertiesName_Throws()
    {
        await using var context = CreateContext();

        var act = () => context.BulkDeleteAsync([NewProduct("A")], c => c.UpdateByProperties = ["Nope"], CancellationToken);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Nope*");
    }

    [Fact]
    public async Task GlobalQueryFilter_AppliesToBulkReadButNotToWrites()
    {
        var notes = new[] { new Note { Text = "visible" }, new Note { Text = "hidden", IsDeleted = true } };

        await using var context = CreateContext();

        await context.BulkInsertAsync(notes, c => c.SetOutputIdentity = true, CancellationToken);

        (await context.BulkReadAsync(notes.Select(n => new Note { Id = n.Id }), cancellationToken: CancellationToken)).Select(n => n.Text).Should().Equal("visible");

        (await context.BulkUpdateAsync([new Note { Id = notes[1].Id, Text = "changed", IsDeleted = true }], cancellationToken: CancellationToken)).Should().Be(1, "writes are not filtered");

        (await context.Notes.IgnoreQueryFilters().AsNoTracking().SingleAsync(n => n.Id == notes[1].Id, CancellationToken)).Text.Should().Be("changed");
    }

    [Fact]
    public async Task ShadowPropertyThatIsNotAForeignKey_IsWrittenOnlyWhenKnown()
    {
        int trackedId, untrackedId;

        await using (var context = CreateContext())
        {
            var tracked = new Note { Text = "tracked" };
            context.Add(tracked);
            context.Entry(tracked).Property("AuditedBy").CurrentValue = "alice";

            var untracked = new Note { Text = "untracked" };

            await context.BulkInsertAsync([tracked, untracked], c => c.SetOutputIdentity = true, CancellationToken);

            trackedId = tracked.Id;
            untrackedId = untracked.Id;
        }

        await using (var context = CreateContext())
            await context.BulkUpdateAsync([new Note { Id = trackedId, Text = "tracked 2" }], cancellationToken: CancellationToken);

        await using var verify = CreateContext();

        var audited = await verify.Notes.OrderBy(n => n.Id).Select(n => new { n.Id, n.Text, AuditedBy = EF.Property<string>(n, "AuditedBy") }).ToListAsync(CancellationToken);

        audited.Single(a => a.Id == trackedId).Should().BeEquivalentTo(new { Text = "tracked 2", AuditedBy = "alice" }, "an unknown shadow value is not overwritten");
        audited.Single(a => a.Id == untrackedId).AuditedBy.Should().BeNull();
    }

    [Fact]
    public async Task KeylessTable_SupportsInsertButNeedsMatchPropertiesForUpdate()
    {
        await using var context = CreateContext();

        (await context.BulkInsertAsync([new LogLine { Message = "a", Level = 1 }, new LogLine { Message = "b", Level = 2 }], cancellationToken: CancellationToken)).Should().Be(2);

        var update = () => context.BulkUpdateAsync([new LogLine { Message = "a", Level = 5 }], cancellationToken: CancellationToken);

        await update.Should().ThrowAsync<InvalidOperationException>().WithMessage("*no primary key*");

        (await context.BulkUpdateAsync([new LogLine { Message = "a", Level = 5 }], c => c.UpdateByProperties = [nameof(LogLine.Message)], CancellationToken)).Should().Be(1);
        (await context.LogLines.OrderBy(l => l.Message).Select(l => l.Level).ToListAsync(CancellationToken)).Should().Equal(5, 2);
    }

    [Fact]
    public async Task DefaultValueWithMeaningfulClrDefault_BehavesLikeSaveChanges()
    {
        await using (var context = CreateContext())
        {
            context.Add(new Feature { Name = "ef", IsEnabled = false });
            await context.SaveChangesAsync(CancellationToken);

            await context.BulkInsertAsync([new Feature { Name = "bulk", IsEnabled = false }], cancellationToken: CancellationToken);
        }

        await using var verify = CreateContext();

        var saved = await verify.Features.ToDictionaryAsync(f => f.Name, f => f.IsEnabled, CancellationToken);

        saved["bulk"].Should().Be(saved["ef"], "an unset value takes the database default in both");
    }

    [Fact]
    public async Task TptMapping_IsRejected()
    {
        await using var context = await CreateUnsupportedContextAsync();

        var act = () => context.BulkInsertAsync([new TptDerived { Name = "x", Extra = 1 }], cancellationToken: CancellationToken);

        await act.Should().ThrowAsync<NotSupportedException>().WithMessage("*more than one table*");
    }

    [Fact]
    public async Task JsonMappedOwnedType_IsRejectedForWrites()
    {
        await using var context = await CreateUnsupportedContextAsync();

        var act = () => context.BulkInsertAsync([new JsonOwner { Settings = new JsonSettings { Theme = "dark" } }], cancellationToken: CancellationToken);

        await act.Should().ThrowAsync<NotSupportedException>().WithMessage("*JSON*");
    }

    [Fact]
    public async Task ProviderWithoutBulkSupport_ThrowsClearError()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase("bulk").Options;

        await using var context = new TestDbContext(options);

        var act = () => context.BulkInsertAsync([new Inventory { WarehouseId = 1, ProductCode = "X" }], cancellationToken: CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*No bulk provider*");
    }

    [Fact]
    public async Task EntityTypeNotInModel_Throws()
    {
        await using var context = CreateContext();

        var act = () => context.BulkInsertAsync([new TptBase { Name = "x" }], cancellationToken: CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not an entity type*");
    }
}