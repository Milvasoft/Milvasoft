using Microsoft.EntityFrameworkCore;

namespace Milvasoft.UnitTests.ComponentsTests.ExpressionBuilderTests.Fixture;

public enum FilterStatusFixture
{
    Active = 0,
    Passive = 1,
    Deleted = 2
}

public class FilterEntityFixture
{
    public int Id { get; set; }
    public string Name { get; set; }
    public char Code { get; set; }
    public int Count { get; set; }
    public int? NullableCount { get; set; }
    public long BigCount { get; set; }
    public decimal Price { get; set; }
    public double? Ratio { get; set; }
    public bool IsActive { get; set; }
    public bool? IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTimeOffset PublishedAt { get; set; }
    public DateTimeOffset? ArchivedAt { get; set; }
    public FilterStatusFixture Status { get; set; }
    public FilterStatusFixture? NullableStatus { get; set; }
    public Guid ExternalId { get; set; }
    public int? CategoryId { get; set; }
    public FilterCategoryFixture Category { get; set; }
    public List<FilterTagFixture> Tags { get; set; } = [];
}

public class FilterCategoryFixture
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class FilterTagFixture
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int FilterEntityFixtureId { get; set; }
}

public class FilterNpgsqlDbContextFixture(DbContextOptions options) : DbContext(options)
{
    public DbSet<FilterEntityFixture> Entities { get; set; }

    /// <summary>
    /// A context that only translates queries. It never opens a connection.
    /// </summary>
    public static FilterNpgsqlDbContextFixture Create()
        => new(new DbContextOptionsBuilder<FilterNpgsqlDbContextFixture>().UseNpgsql("Host=localhost;Database=expression_builder_tests;Username=test;Password=test").Options);
}

public static class FilterDataFixture
{
    /// <summary>
    /// 10:00 UTC. Entities 1 and 4 are on this day in every time zone from UTC-10 to UTC+11.
    /// </summary>
    public static readonly DateTime Base = new(2026, 3, 10, 10, 0, 0, DateTimeKind.Utc);

    public static readonly Guid Guid1 = Guid.Parse("8f6d2c1a-0000-4000-8000-000000000001");
    public static readonly Guid Guid2 = Guid.Parse("8f6d2c1a-0000-4000-8000-000000000002");
    public static readonly Guid Guid3 = Guid.Parse("8f6d2c1a-0000-4000-8000-000000000003");
    public static readonly Guid Guid4 = Guid.Parse("8f6d2c1a-0000-4000-8000-000000000004");
    public static readonly Guid Guid5 = Guid.Parse("8f6d2c1a-0000-4000-8000-000000000005");

    public static DateTimeOffset Offset(DateTime utc) => new(utc, TimeSpan.Zero);

    public static List<FilterEntityFixture> Create() =>
    [
        new()
        {
            Id = 1, Name = "John", Code = 'A', Count = 1, NullableCount = 10, BigCount = 100, Price = 10.5m, Ratio = 0.5,
            IsActive = true, IsVerified = true, CreatedAt = Base, UpdatedAt = Base.AddDays(1), PublishedAt = Offset(Base), ArchivedAt = null,
            Status = FilterStatusFixture.Active, NullableStatus = FilterStatusFixture.Active, ExternalId = Guid1,
            Category = new() { Id = 1, Name = "Books" }, Tags = [new() { Id = 1, Name = "red" }]
        },
        new()
        {
            Id = 2, Name = " Jane ", Code = 'B', Count = 5, NullableCount = null, BigCount = 200, Price = 20m, Ratio = null,
            IsActive = false, IsVerified = false, CreatedAt = Base.AddDays(-2), UpdatedAt = null, PublishedAt = Offset(Base.AddDays(-2)), ArchivedAt = Offset(Base.AddDays(-1)),
            Status = FilterStatusFixture.Passive, NullableStatus = null, ExternalId = Guid2,
            Category = null, Tags = [new() { Id = 2, Name = "red" }, new() { Id = 3, Name = "blue" }]
        },
        new()
        {
            Id = 3, Name = "", Code = 'C', Count = 10, NullableCount = 30, BigCount = 300, Price = 0.5m, Ratio = 1.5,
            IsActive = true, IsVerified = null, CreatedAt = Base.AddDays(5), UpdatedAt = Base.AddDays(5), PublishedAt = Offset(Base.AddDays(5)), ArchivedAt = Offset(Base.AddDays(5)),
            Status = FilterStatusFixture.Deleted, NullableStatus = FilterStatusFixture.Passive, ExternalId = Guid3,
            Category = new() { Id = 3, Name = null }, Tags = []
        },
        new()
        {
            Id = 4, Name = null, Code = 'A', Count = 20, NullableCount = 10, BigCount = 400, Price = 100m, Ratio = 0.5,
            IsActive = false, IsVerified = true, CreatedAt = Base.AddHours(2), UpdatedAt = null, PublishedAt = Offset(Base.AddHours(2)), ArchivedAt = null,
            Status = FilterStatusFixture.Active, NullableStatus = FilterStatusFixture.Deleted, ExternalId = Guid4,
            Category = new() { Id = 4, Name = "Games" }, Tags = [new() { Id = 4, Name = "blue" }]
        },
        new()
        {
            Id = 5, Name = "   ", Code = 'D', Count = 50, NullableCount = null, BigCount = 500, Price = 50m, Ratio = null,
            IsActive = true, IsVerified = false, CreatedAt = Base.AddMonths(-1), UpdatedAt = Base.AddMonths(-1), PublishedAt = Offset(Base.AddMonths(-1)), ArchivedAt = null,
            Status = FilterStatusFixture.Passive, NullableStatus = FilterStatusFixture.Active, ExternalId = Guid5,
            Category = new() { Id = 5, Name = "books" }, Tags = []
        },
    ];
}
