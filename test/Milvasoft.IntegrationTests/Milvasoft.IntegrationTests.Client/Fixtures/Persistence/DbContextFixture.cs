using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Milvasoft.Cryptography.Abstract;
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
    public DbSet<SomeModelBuilderTestEntityFixture> ModelBuilderTestEntities { get; set; }
    public DbSet<SomeModelBuilderTestKeylessEntityFixture> ModelBuilderTestKeylessEntities { get; set; }
    public DbSet<SomeLogEntity> SomeLogEntities { get; set; }
    public DbSet<SomeTenantEntity> SomeTenantEntities { get; set; }
    public DbSet<HasJsonTranslationEntityFixture> HasJsonTranslationEntities { get; set; }
}

public class TranslationRelationsModelCustomizer : IModelCustomizer
{
    public void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.UseTenantId();
        modelBuilder.UseTranslationEntityRelations();
        modelBuilder.UseUtcDateTime();
    }
}

public class UseTenantIdModelCustomizer : IModelCustomizer
{
    public void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.UseTenantId();
        modelBuilder.UseUtcDateTime();
    }
}

public class UseUtcDateTimeModelCustomizer : IModelCustomizer
{
    public void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.UseTenantId();
        modelBuilder.UseUtcDateTime();
    }
}
public class NoUseUtcDateTimeModelCustomizer : IModelCustomizer
{
    public void Customize(ModelBuilder modelBuilder, DbContext context) => modelBuilder.UseTenantId();
}

public class UseAnnotationEncryptionModelCustomizer : IModelCustomizer
{
    public void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.UseTenantId();
        modelBuilder.UseAnnotationEncryption(context.GetService<IMilvaCryptographyProvider>());
        modelBuilder.UseUtcDateTime();
    }
}

public class UseEncryptionModelCustomizer : IModelCustomizer
{
    public void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.UseTenantId();
        modelBuilder.UseEncryption(context.GetService<IMilvaCryptographyProvider>());
        modelBuilder.UseUtcDateTime();
    }
}

public class UseDefaultValueModelCustomizer : IModelCustomizer
{
    public void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.UseTenantId();
        modelBuilder.UseUtcDateTime();
        modelBuilder.UseDefaultValue();
    }
}

public class UseTenantIdQueryFilterModelCustomizer : IModelCustomizer
{
    public void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.UseTenantId();
        modelBuilder.UseTenantIdQueryFilter(new Core.EntityBases.MultiTenancy.TenantId("milvasoft_1"));
        modelBuilder.UseUtcDateTime();
    }
}

public class UseSoftDeleteQueryFilterModelCustomizer : IModelCustomizer
{
    public void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.UseTenantId();
        modelBuilder.UseSoftDeleteQueryFilter();
        modelBuilder.UseUtcDateTime();
    }
}

public class UseIndexModelCustomizer : IModelCustomizer
{
    public void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.UseTenantId();
        modelBuilder.UseUtcDateTime();
        modelBuilder.UseIndexToSoftDeletableEntities();
        modelBuilder.UseIndexToCreationAuditableEntities();
        modelBuilder.UseLogEntityBaseIndexes();
    }
}

public class UseTurkishCollationModelCustomizer : IModelCustomizer
{
    public void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.UseTenantId();
        modelBuilder.UseUtcDateTime();
        modelBuilder.UseTurkishCollation();
    }
}

public class UseCollationOnStringPropertiesModelCustomizer : IModelCustomizer
{
    public void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.UseTenantId();
        modelBuilder.UseUtcDateTime();
        modelBuilder.UseCollationOnStringProperties("tr-TR-x-icu");
    }
}

public class UsePrecisionModelCustomizer : IModelCustomizer
{
    public void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.UseTenantId();
        modelBuilder.UseUtcDateTime();
        modelBuilder.UsePrecision();
    }
}

public class UseAnnotationPrecisionModelCustomizer : IModelCustomizer
{
    public void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.UseTenantId();
        modelBuilder.UseUtcDateTime();
        modelBuilder.UseAnnotationPrecision();
    }
}