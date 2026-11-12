using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Milvasoft.Attributes.Annotations;
using Milvasoft.Core.EntityBases.MultiTenancy;
using Milvasoft.DataAccess.EfCore.DbContextBase;

namespace Milvasoft.UnitTests.DataAccessTests.EfCoreTests;

[Trait("EF Core Data Access Unit Tests", "Unit tests for Milvasoft.DataAccess.EfCore unit testable parts.")]
public class ModelBuilderExtensionsIndexTests
{
    [Fact]
    public void ConvertHasTenantIdIndexesToCompositeIndexes_WithConfiguredAndShadowPropertyIndexes_ShouldKeepIndexConfiguration()
    {
        // Arrange
        var modelBuilder = new ModelBuilder();

        modelBuilder.Entity<IndexTestEntity>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.TenantId);
            b.Property(e => e.Code);
            b.Property<string>("ShadowCode");
            b.HasIndex(e => e.Code).IsUnique().HasFilter("\"Code\" IS NOT NULL");
            b.HasIndex(["ShadowCode"], "IX_Named");
        });

        // Act
        modelBuilder.ConvertHasTenantIdIndexesToCompositeIndexes();

        // Assert
        var indexes = modelBuilder.Model.FindEntityType(typeof(IndexTestEntity)).GetIndexes().ToList();

        indexes.Should().HaveCount(2);

        var codeIndex = indexes.Single(i => i.Properties[0].Name == nameof(IndexTestEntity.Code));
        codeIndex.Properties.Select(p => p.Name).Should().Equal(nameof(IndexTestEntity.Code), nameof(IndexTestEntity.TenantId));
        codeIndex.IsUnique.Should().BeTrue();
        codeIndex.GetFilter().Should().Be("\"Code\" IS NOT NULL");

        var namedIndex = indexes.Single(i => i.Name == "IX_Named");
        namedIndex.Properties.Select(p => p.Name).Should().Equal("ShadowCode", nameof(IndexTestEntity.TenantId));
    }

    [Fact]
    public void UseWithTenantIdCompositeIndexes_WithShadowProperty_ShouldNotThrow()
    {
        // Arrange
        var modelBuilder = new ModelBuilder();

        modelBuilder.Entity<IndexTestEntity>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.TenantId);
            b.Property(e => e.Code);
            b.Property<string>("ShadowCode");
        });

        // Act
        modelBuilder.UseWithTenantIdCompositeIndexes();

        // Assert
        modelBuilder.Model.FindEntityType(typeof(IndexTestEntity)).GetIndexes().Single().Properties.Select(p => p.Name)
                    .Should().Equal(nameof(IndexTestEntity.TenantId), nameof(IndexTestEntity.Code));
    }

    public class IndexTestEntity : IHasTenantId
    {
        public int Id { get; set; }
        public TenantId TenantId { get; set; }

        [WithTenantIdIndex]
        public string Code { get; set; }
    }
}
