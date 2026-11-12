using FluentAssertions;
using Milvasoft.Attributes.Annotations;
using Milvasoft.Core.EntityBases.Concrete;
using Milvasoft.Helpers.DataAccess.EfCore.Concrete;
using Milvasoft.UnitTests.DataAccessTests.EfCoreTests.Fixtures;

namespace Milvasoft.UnitTests.DataAccessTests.EfCoreTests.RepositoryBaseTests;

[Trait("EF Core Data Access Unit Tests", "Unit tests for Milvasoft.DataAccess.EfCore unit testable parts.")]
public class CascadeIncludePathTests
{
    [Fact]
    public void GetCascadeIncludePaths_WithInterfaceCollectionsReferencesAndCommonPrefixes_ShouldReturnAllDeepestPaths()
    {
        // Act
        var paths = CascadeTestRepository.GetIncludePaths(typeof(CascadeParent));

        // Assert
        paths.Should().BeEquivalentTo("Items.Children", "ItemsArchive.Children", "Detail.Notes", "Self");
    }

    public class CascadeParent : BaseEntity<int>
    {
        [CascadeOnDelete]
        public ICollection<CascadeItem> Items { get; set; }

        [CascadeOnDelete]
        public List<CascadeItem> ItemsArchive { get; set; }

        [CascadeOnDelete]
        public CascadeDetail Detail { get; set; }

        [CascadeOnDelete]
        public CascadeParent Self { get; set; }

        public List<CascadeItem> NotCascaded { get; set; }
    }

    public class CascadeItem : BaseEntity<int>
    {
        [CascadeOnDelete]
        public IReadOnlyCollection<CascadeChild> Children { get; set; }
    }

    public class CascadeChild : BaseEntity<int>
    {
        [CascadeOnDelete]
        public string NotNavigation { get; set; }
    }

    public class CascadeDetail : BaseEntity<int>
    {
        [CascadeOnDelete]
        public CascadeChild[] Notes { get; set; }
    }

    private sealed class CascadeTestRepository(SomeMilvaDbContextFixture dbContext) : BaseRepository<CascadeParent, SomeMilvaDbContextFixture>(dbContext)
    {
        public static IReadOnlyList<string> GetIncludePaths(Type type) => GetCascadeIncludePaths(type);
    }
}
