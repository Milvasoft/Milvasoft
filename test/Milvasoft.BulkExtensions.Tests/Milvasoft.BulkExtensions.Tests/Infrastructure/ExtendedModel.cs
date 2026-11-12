using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Milvasoft.BulkExtensions.Tests.Infrastructure;

/// <summary>
/// Many column types in one table.
/// </summary>
public class TypeZoo
{
    public int Id { get; set; }

    public bool Flag { get; set; }

    public short Small { get; set; }

    public long Big { get; set; }

    public float Single { get; set; }

    public double Double { get; set; }

    public decimal Money { get; set; }

    public char Letter { get; set; }

    public string Json { get; set; }

    public byte[] Bytes { get; set; }

    public int[] Numbers { get; set; }

    public List<Guid> Guids { get; set; } = [];

    public Guid? MaybeGuid { get; set; }

    public DateTime UtcTime { get; set; }

    public DateTime LocalTime { get; set; }

    public DateTimeOffset Offset { get; set; }

    public DateOnly Date { get; set; }

    public TimeOnly Time { get; set; }

    public TimeSpan Interval { get; set; }

    public DayOfWeek EnumAsInt { get; set; }

    public IPAddress Address { get; set; }

    public string Unicode { get; set; }
}

/// <summary>
/// Schema and identifiers that need quoting.
/// </summary>
public class OddName
{
    public int Id { get; set; }

    public string Value { get; set; }
}

/// <summary>
/// Strongly typed key stored through a value converter.
/// </summary>
public readonly record struct CouponCode(string Value);

public class Coupon
{
    public CouponCode Code { get; set; }

    public int Discount { get; set; }
}

/// <summary>
/// <see cref="byte"/> array primary key.
/// </summary>
public class BlobItem
{
    public byte[] Hash { get; set; }

    public string Data { get; set; }
}

/// <summary>
/// Key generated with a HiLo sequence on the client.
/// </summary>
public class HiLoItem
{
    public int Id { get; set; }

    public string Name { get; set; }
}

public class Student
{
    public int Id { get; set; }

    public string Name { get; set; }

    public List<Course> Courses { get; set; } = [];
}

public class Course
{
    public int Id { get; set; }

    public string Title { get; set; }

    public List<Student> Students { get; set; } = [];
}

/// <summary>
/// Global query filter and a shadow property that is not a foreign key.
/// </summary>
public class Note
{
    public int Id { get; set; }

    public string Text { get; set; }

    public bool IsDeleted { get; set; }
}

/// <summary>
/// Keyless type mapped to a table.
/// </summary>
public class LogLine
{
    public string Message { get; set; }

    public int Level { get; set; }
}

/// <summary>
/// Two tables that refer to each other.
/// </summary>
public class Husband
{
    public int Id { get; set; }

    public int? WifeId { get; set; }

    public Wife Wife { get; set; }
}

public class Wife
{
    public int Id { get; set; }

    public int? HusbandId { get; set; }

    public Husband Husband { get; set; }
}

/// <summary>
/// Default value whose CLR default is a meaningful value.
/// </summary>
public class Feature
{
    public int Id { get; set; }

    public string Name { get; set; }

    public bool IsEnabled { get; set; }
}

public partial class TestDbContext
{
    public DbSet<TypeZoo> TypeZoos => Set<TypeZoo>();

    public DbSet<OddName> OddNames => Set<OddName>();

    public DbSet<Coupon> Coupons => Set<Coupon>();

    public DbSet<BlobItem> BlobItems => Set<BlobItem>();

    public DbSet<HiLoItem> HiLoItems => Set<HiLoItem>();

    public DbSet<Student> Students => Set<Student>();

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Note> Notes => Set<Note>();

    public DbSet<LogLine> LogLines => Set<LogLine>();

    public DbSet<Husband> Husbands => Set<Husband>();

    public DbSet<Wife> Wives => Set<Wife>();

    public DbSet<Feature> Features => Set<Feature>();

    private static void ConfigureExtendedModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Dog>().OwnsOne(d => d.Collar);

        modelBuilder.Entity<TypeZoo>(b =>
        {
            b.Property(t => t.Json).HasColumnType("jsonb");
            b.Property(t => t.Money).HasPrecision(20, 6);
            b.Property(t => t.LocalTime).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<OddName>(b =>
        {
            b.ToTable("Odd \"Table\" Name", "Sales Data");
            b.Property(o => o.Id).HasColumnName("Key Column");
            b.Property(o => o.Value).HasColumnName("Value \"Quoted\"");
        });

        modelBuilder.Entity<Coupon>(b =>
        {
            b.HasKey(c => c.Code);
            b.Property(c => c.Code).HasConversion(c => c.Value, v => new CouponCode(v)).HasMaxLength(20);
        });

        modelBuilder.Entity<BlobItem>().HasKey(b => b.Hash);

        modelBuilder.HasSequence<int>("HiLoSequence").IncrementsBy(10);
        modelBuilder.Entity<HiLoItem>().Property(h => h.Id).UseHiLo("HiLoSequence");

        modelBuilder.Entity<Student>().HasMany(s => s.Courses).WithMany(c => c.Students);

        modelBuilder.Entity<Note>(b =>
        {
            b.HasQueryFilter(n => !n.IsDeleted);
            b.Property<string>("AuditedBy").HasMaxLength(50);
        });

        modelBuilder.Entity<LogLine>().HasNoKey().ToTable("LogLines");

        modelBuilder.Entity<Husband>().HasOne(h => h.Wife).WithMany().HasForeignKey(h => h.WifeId);
        modelBuilder.Entity<Wife>().HasOne(w => w.Husband).WithMany().HasForeignKey(w => w.HusbandId);

        modelBuilder.Entity<Feature>().Property(f => f.IsEnabled).HasDefaultValue(true);
    }
}

/// <summary>
/// Mappings bulk operations reject.
/// </summary>
public class UnsupportedDbContext(DbContextOptions<UnsupportedDbContext> options) : DbContext(options)
{
    public DbSet<TptBase> TptBases => Set<TptBase>();

    public DbSet<JsonOwner> JsonOwners => Set<JsonOwner>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TptBase>().UseTptMappingStrategy();
        modelBuilder.Entity<TptDerived>();
        modelBuilder.Entity<JsonOwner>().OwnsOne(o => o.Settings, s => s.ToJson());
    }
}

public class TptBase
{
    public int Id { get; set; }

    public string Name { get; set; }
}

public class TptDerived : TptBase
{
    public int Extra { get; set; }
}

public class JsonOwner
{
    public int Id { get; set; }

    public JsonSettings Settings { get; set; }
}

public class JsonSettings
{
    public string Theme { get; set; }
}