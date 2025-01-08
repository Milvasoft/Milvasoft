using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Milvasoft.DataAccess.EfCore.Bulk.DbContextBase;
using Milvasoft.DataAccess.EfCore.Configuration;
using Milvasoft.DataAccess.EfCore.DbContextBase;
using Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures;

namespace Milvasoft.IntegrationTests.Client.Fixtures.Persistence;

public class MilvaBulkDbContextFixture(DbContextOptions<MilvaBulkDbContextFixture> contextOptions, IDataAccessConfiguration dataAccessConfiguration, IServiceProvider serviceProvider) : MilvaBulkDbContext(contextOptions, dataAccessConfiguration, serviceProvider)
{
    public DbSet<SomeEntityFixture> Entities { get; set; }
    public DbSet<SomeRelatedEntityFixture> RelatedEntities { get; set; }
    public DbSet<SomeBaseEntityFixture> BaseEntities { get; set; }
    public DbSet<SomeFullAuditableEntityFixture> FullAuditableEntities { get; set; }
    public DbSet<AnotherFullAuditableEntityFixture> AnotherFullAuditableEntities { get; set; }
    public DbSet<SomeManyToOneFullAuditableEntityFixture> SomeManyToOneEntities { get; set; }
    public DbSet<SomeManyToManyEntityFixture> SomeManyToManyEntities { get; set; }
    public DbSet<SomeCircularReferenceFullAuditableEntityFixture> SomeCircularReferenceEntities { get; set; }
    public DbSet<HasTranslationEntityFixture> HasTranslationEntities { get; set; }
    public DbSet<TranslationEntityFixture> TranslationEntities { get; set; }
    public DbSet<RestTestEntityFixture> RestTestEntities { get; set; }
    public DbSet<RestChildrenTestEntityFixture> RestChildrenTestEntities { get; set; }
}

public class TranslationRelationsModelCustomizer : IModelCustomizer
{
    public void Customize(ModelBuilder modelBuilder, DbContext context) => modelBuilder.UseTranslationEntityRelations();
}