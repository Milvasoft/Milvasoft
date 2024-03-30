using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Milvasoft.Attributes.Annotations;
using Milvasoft.Core.EntityBases.MultiTenancy;
using Milvasoft.Core.MultiLanguage.EntityBases;
using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;
using Milvasoft.Cryptography.Abstract;
using Milvasoft.DataAccess.EfCore.Utils.Converters;
using MongoDB.Bson;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;

namespace Milvasoft.DataAccess.EfCore.DbContextBase;

/// <summary>
/// Extension methods for MilvaDbContexts
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Adds <see cref="TenantId"/> converters to <see cref="TenantId"/> typed properties.
    /// </summary>
    /// <param name="modelBuilder"></param>
    public static ModelBuilder UseTenantId(this ModelBuilder modelBuilder)
    {
        var tenantIdConverter = new TenantIdStringConverter();

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var properties = entityType.ClrType
                                       .GetProperties()
                                       .Where(p => p.PropertyType == typeof(TenantId) || p.PropertyType == typeof(TenantId?));

            foreach (var property in properties)
            {
                modelBuilder.Entity(entityType.Name)
                            .Property(property.Name)
                            .HasConversion(tenantIdConverter);
            }
        }

        return modelBuilder;
    }

    /// <summary>
    /// Use UTC for datetime types.
    /// </summary>
    /// <param name="modelBuilder"></param>
    public static ModelBuilder UseUtcDateTime(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    modelBuilder.Entity(entityType.ClrType)
                                .Property<DateTime>(property.Name)
                                .HasConversion(v => v.ToUniversalTime(), v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    modelBuilder.Entity(entityType.ClrType)
                                .Property<DateTime?>(property.Name)
                                .HasConversion(v => v.HasValue ? v.Value.ToUniversalTime() : v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);
                }
            }
        }

        return modelBuilder;
    }

    /// <summary>
    /// Adds <see cref="ObjectId"/> converters to <see cref="ObjectId"/> typed properties.
    /// </summary>
    /// <param name="modelBuilder"></param>
    public static ModelBuilder UseObjectId(this ModelBuilder modelBuilder)
    {
        var objectIdConverter = new MilvaObjectIdStringConverter();

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var properties = entityType.ClrType
                                       .GetProperties()
                                       .Where(p => p.PropertyType == typeof(ObjectId) || p.PropertyType == typeof(ObjectId?));

            foreach (var property in properties)
            {
                modelBuilder.Entity(entityType.Name)
                            .Property(property.Name)
                            .HasConversion(objectIdConverter);
            }
        }

        return modelBuilder;
    }

    /// <summary>
    /// Adds value converter(<see cref="MilvaEncryptionConverter"/>) to string properties which marked with <see cref="EncryptedAttribute"/>.
    /// </summary>
    /// <param name="modelBuilder"></param>
    /// <param name="encryptionProvider"></param>
    public static ModelBuilder UseAnnotationEncryption(this ModelBuilder modelBuilder, IMilvaCryptographyProvider encryptionProvider)
    {
        if (modelBuilder is null)
            throw new MilvaDeveloperException("The given model builder cannot be null");

        if (encryptionProvider is null)
            throw new MilvaDeveloperException("Cannot initialize encryption with a null provider.");

        var encryptionConverter = new MilvaEncryptionConverter(encryptionProvider);

        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (IMutableProperty property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(string) /* && !IsDiscriminator(property)*/)
                {
                    var attributes = property.PropertyInfo.GetCustomAttributes(typeof(EncryptedAttribute), false);

                    if (attributes.Length != 0)
                        property.SetValueConverter(encryptionConverter);
                }
            }
        }

        return modelBuilder;
    }

    /// <summary>
    /// Adds value converter(<see cref="MilvaEncryptionConverter"/>) to all strings properties.
    /// </summary>
    /// <param name="modelBuilder"></param>
    /// <param name="encryptionProvider"></param>
    public static ModelBuilder UseEncryption(this ModelBuilder modelBuilder, IMilvaCryptographyProvider encryptionProvider)
    {
        if (modelBuilder is null)
            throw new MilvaDeveloperException("The given model builder cannot be null");

        if (encryptionProvider is null)
            throw new MilvaDeveloperException("Cannot initialize encryption with a null provider.");

        var encryptionConverter = new MilvaEncryptionConverter(encryptionProvider);

        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
            foreach (IMutableProperty property in entityType.GetProperties())
                if (property.ClrType == typeof(string) /* && !IsDiscriminator(property)*/)
                    property.SetValueConverter(encryptionConverter);
        return modelBuilder;
    }

    /// <summary>
    /// For PostgreSql, makes string properties for Turkish character compatible.
    /// </summary>
    /// <param name="modelBuilder"></param>
    public static ModelBuilder UseTurkishCollation(this ModelBuilder modelBuilder)
    {
        var entityClrTypesIsString = modelBuilder.Model.GetEntityTypes()
                                                       .Where(e => Array.Exists(e.ClrType.GetProperties(), p => p.PropertyType.IsAssignableFrom(typeof(string))))
                                                       .Select(e => e.ClrType);

        foreach (var clrType in entityClrTypesIsString)
        {
            var properties = clrType.GetProperties().Where(p => p.PropertyType.IsAssignableFrom(typeof(string))
                                                                && !p.CustomAttributes.Any(cA => cA.AttributeType.IsEquivalentTo(typeof(NotMappedAttribute))));

            foreach (var prop in properties)
                modelBuilder.Entity(clrType).Property(prop.Name).UseCollation("tr-TR-x-icu");
        }

        return modelBuilder;
    }

    /// <summary>
    /// Adds an index for each indelible entity for IsDeleted property.
    /// </summary>
    /// <param name="modelBuilder"></param>
    public static ModelBuilder UseIndexToIndelibleEntities(this ModelBuilder modelBuilder)
    {
        var indelibleEntites = modelBuilder.Model.GetEntityTypes().Where(entityType => entityType.FindProperty(EntityPropertyNames.IsDeleted) != null);

        foreach (var entityType in indelibleEntites)
            modelBuilder.Entity(entityType.ClrType).HasIndex(EntityPropertyNames.IsDeleted);

        return modelBuilder;
    }

    /// <summary>
    /// Adds an index for each creation auditable entity for CreationDate property.
    /// </summary>
    /// <param name="modelBuilder"></param>
    public static ModelBuilder UseIndexToCreationAuditableEntities(this ModelBuilder modelBuilder)
    {
        var indelibleEntites = modelBuilder.Model.GetEntityTypes().Where(entityType => entityType.FindProperty(EntityPropertyNames.CreationDate) != null);

        foreach (var entityType in indelibleEntites)
            modelBuilder.Entity(entityType.ClrType).HasIndex(EntityPropertyNames.CreationDate);

        return modelBuilder;
    }

    /// <summary>
    /// Configures default value for update database.
    /// </summary>
    /// <param name="modelBuilder"></param>
    public static ModelBuilder UseDefaultValue(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            foreach (var property in entityType.GetProperties())
            {
                var memberInfo = property.PropertyInfo ?? (MemberInfo)property.FieldInfo;

                if (memberInfo == null)
                    continue;

                if (Attribute.GetCustomAttribute(memberInfo, typeof(DefaultValueAttribute)) is not DefaultValueAttribute defaultValue)
                    continue;

                modelBuilder.Entity(entityType.ClrType).Property(property.Name).HasDefaultValue(defaultValue.Value);
            }

        return modelBuilder;
    }

    /// <summary>
    /// Enable the query filter for indelible entities.
    /// </summary>
    /// <param name="modelBuilder"></param>
    public static ModelBuilder UseSoftDeleteQueryFilter(this ModelBuilder modelBuilder)
    {
        var indelibleEntites = modelBuilder.Model.GetEntityTypes().Where(entityType => entityType.FindProperty(EntityPropertyNames.IsDeleted) != null);

        foreach (var entityType in indelibleEntites)
        {
            var parameter = Expression.Parameter(entityType.ClrType, "entity");

            var prop = entityType.FindProperty(EntityPropertyNames.IsDeleted);

            var filterExpression = Expression.Equal(Expression.Property(parameter, prop.PropertyInfo), Expression.Constant(false, typeof(bool)));

            var dynamicLambda = Expression.Lambda(filterExpression, parameter);

            entityType.SetQueryFilter(dynamicLambda);
        }

        return modelBuilder;
    }

    /// <summary>
    /// Configures the decimal property of entities with decimal properties in decimal default(18,2) format.
    /// </summary>
    /// <param name="modelBuilder"></param>
    /// <param name="precision"></param>
    /// <param name="scale"></param>
    public static ModelBuilder UsePrecision(this ModelBuilder modelBuilder, int precision = 18, int scale = 10)
    {
        var entityClrTypesHasDecimalProperty = modelBuilder.Model.GetEntityTypes()
                                                         .Where(e => Array.Exists(e.ClrType.GetProperties(), p => p.PropertyType.IsAssignableFrom(typeof(decimal))))
                                                         .Select(e => e.ClrType);

        foreach (var clrType in entityClrTypesHasDecimalProperty)
        {
            var properties = clrType.GetProperties().Where(p => p.PropertyType.IsAssignableFrom(typeof(decimal))
                                                                      && (!p.CustomAttributes?.Any(cA => (cA.AttributeType?.IsEquivalentTo(typeof(NotMappedAttribute)) ?? false)
                                                                                                      || (cA.AttributeType?.IsEquivalentTo(typeof(DecimalPrecisionAttribute)) ?? false)) ?? true));

            foreach (var prop in properties)
                modelBuilder.Entity(clrType).Property(prop.Name).HasPrecision(precision, scale);
        }

        return modelBuilder;
    }

    /// <summary>
    /// Configures the decimal property of entities with decimal properties in decimal format according to <see cref="DecimalPrecisionAttribute"/>.
    /// </summary>
    /// <remarks>
    /// You can use this method with <see cref="UsePrecision(ModelBuilder, int, int)"/> method. 
    /// </remarks>
    /// <param name="modelBuilder"></param>
    public static ModelBuilder UseAnnotationPrecision(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            foreach (var property in entityType.GetProperties())
            {
                var memberInfo = property.PropertyInfo ?? (MemberInfo)property.FieldInfo;

                if (memberInfo == null)
                    continue;

                if (Attribute.GetCustomAttribute(memberInfo, typeof(DecimalPrecisionAttribute)) is not DecimalPrecisionAttribute precisionAttribute)
                    continue;

                modelBuilder.Entity(entityType.ClrType).Property(property.Name).HasPrecision(precisionAttribute.Precision, precisionAttribute.Scale);
            }

        return modelBuilder;
    }

    /// <summary>
    /// Allows to all entities associated with deletions to be Included to the entity(s) to be included in the process.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="context"></param>
    public static IQueryable<T> IncludeAll<T>(this IQueryable<T> source, DbContext context)
        where T : class
    {
        var navigations = context.Model.FindEntityType(typeof(T))
                                       .GetDerivedTypesInclusive()
                                       .SelectMany(type => type.GetNavigations())
                                       .Distinct();

        foreach (var property in navigations)
            source = source.Include(property.Name);

        return source;
    }

    /// <summary>
    /// Allows to all entities associated with deletions to be Included to the entity(s) to be included in the process.
    /// Entities must be contains "Translations" navigation property for include process. (e.g. Poco.Translations)
    /// </summary>
    /// <param name="source"></param>
    /// <param name="context"></param>
    public static IQueryable<TEntity> IncludeTranslations<TEntity>(this IQueryable<TEntity> source, DbContext context) where TEntity : class
    {
        var navigations = context.Model.FindEntityType(typeof(TEntity))
                                       .GetDerivedTypesInclusive()
                                       .SelectMany(type => type.GetNavigations())
                                       .Distinct();

        foreach (var property in navigations.Where(property => property.Name == MultiLanguageEntityPropertyNames.Translations))
            source = source.Include(property.Name);

        return source;
    }

    /// <summary>
    /// Allows to all entities associated with deletions to be Included to the entity(s) to be included in the process.
    /// Entities must be contains "Translations" navigation property for include process. (e.g. Poco.Translations)
    /// </summary>
    /// <param name="source"></param>
    public static IQueryable<TEntity> IncludeTranslations<TEntity, TLangEntity>(this IQueryable<TEntity> source) where TEntity : class, IHasTranslation<TLangEntity> where TLangEntity : class
        => source.Include(MultiLanguageEntityPropertyNames.Translations);

    /// <summary>
    /// Translation entities relationships.
    /// </summary>
    /// <remarks>
    /// You can use this method with <see cref="UsePrecision(ModelBuilder, int, int)"/> method. 
    /// </remarks>
    /// <param name="modelBuilder"></param>
    public static ModelBuilder UseTranslationEntityRelations(this ModelBuilder modelBuilder)
    {
        var languageEntities = modelBuilder.Model.GetEntityTypes().Where(e => e.ClrType.CanAssignableTo(typeof(IHasTranslation<>)));

        foreach (var entityType in languageEntities)
        {
            modelBuilder.Entity(entityType.ClrType)
                        .HasMany(MultiLanguageEntityPropertyNames.Translations)
                        .WithOne(MultiLanguageEntityPropertyNames.Entity)
                        .HasForeignKey(MultiLanguageEntityPropertyNames.EntityId);
        }

        return modelBuilder;
    }
}
