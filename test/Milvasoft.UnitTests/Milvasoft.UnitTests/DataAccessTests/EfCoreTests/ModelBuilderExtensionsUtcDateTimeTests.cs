using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Milvasoft.DataAccess.EfCore.Configuration;
using Milvasoft.DataAccess.EfCore.DbContextBase;

namespace Milvasoft.UnitTests.DataAccessTests.EfCoreTests;

[Trait("EF Core Data Access Unit Tests", "Unit tests for Milvasoft.DataAccess.EfCore unit testable parts.")]
public class ModelBuilderExtensionsUtcDateTimeTests
{
    [Fact]
    public void DbContextConfiguration_UseUtcForDateTime_ShouldBeTrueByDefault()
        => new DbContextConfiguration().UseUtcForDateTime.Should().BeTrue();

    [Theory]
    [InlineData(nameof(UtcTestEntity.Date))]
    [InlineData(nameof(UtcTestEntity.NullableDate))]
    public void UseUtcDateTime_WithUnspecifiedKind_ShouldKeepClockValueAndMarkAsUtc(string propertyName)
    {
        // Arrange
        var converter = GetConverter(propertyName);
        var unspecified = new DateTime(2026, 1, 15, 12, 30, 0, DateTimeKind.Unspecified);

        // Act
        var stored = (DateTime)converter.ConvertToProvider(unspecified);

        // Assert
        stored.Kind.Should().Be(DateTimeKind.Utc);
        stored.Ticks.Should().Be(unspecified.Ticks);
    }

    [Theory]
    [InlineData(nameof(UtcTestEntity.Date))]
    [InlineData(nameof(UtcTestEntity.NullableDate))]
    public void UseUtcDateTime_WithLocalKind_ShouldConvertToUtc(string propertyName)
    {
        // Arrange
        var converter = GetConverter(propertyName);
        var local = new DateTime(2026, 1, 15, 12, 30, 0, DateTimeKind.Local);

        // Act
        var stored = (DateTime)converter.ConvertToProvider(local);

        // Assert
        stored.Kind.Should().Be(DateTimeKind.Utc);
        stored.Should().Be(local.ToUniversalTime());
    }

    [Theory]
    [InlineData(nameof(UtcTestEntity.Date))]
    [InlineData(nameof(UtcTestEntity.NullableDate))]
    public void UseUtcDateTime_WithUtcKind_ShouldKeepValue(string propertyName)
    {
        // Arrange
        var converter = GetConverter(propertyName);
        var utc = new DateTime(2026, 1, 15, 12, 30, 0, DateTimeKind.Utc);

        // Act
        var stored = (DateTime)converter.ConvertToProvider(utc);
        var read = (DateTime)converter.ConvertFromProvider(DateTime.SpecifyKind(stored, DateTimeKind.Unspecified));

        // Assert
        stored.Should().Be(utc);
        read.Kind.Should().Be(DateTimeKind.Utc);
        read.Ticks.Should().Be(utc.Ticks);
    }

    [Fact]
    public void UseUtcDateTime_WithNullValue_ShouldKeepNull()
        => GetConverter(nameof(UtcTestEntity.NullableDate)).ConvertToProvider(null).Should().BeNull();

    private static ValueConverter GetConverter(string propertyName)
    {
        var modelBuilder = new ModelBuilder();

        modelBuilder.Entity<UtcTestEntity>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.Date);
            b.Property(e => e.NullableDate);
        });

        modelBuilder.UseUtcDateTime();

        return modelBuilder.Model.FindEntityType(typeof(UtcTestEntity)).FindProperty(propertyName).GetValueConverter();
    }

    public class UtcTestEntity
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public DateTime? NullableDate { get; set; }
    }
}
