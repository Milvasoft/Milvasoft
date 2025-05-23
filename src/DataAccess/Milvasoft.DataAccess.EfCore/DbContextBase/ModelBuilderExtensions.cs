﻿using Microsoft.EntityFrameworkCore;
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
                else if (property.ClrType == typeof(DateTimeOffset))
                {
                    modelBuilder.Entity(entityType.ClrType)
                                .Property<DateTimeOffset>(property.Name)
                                .HasConversion(v => v.ToUniversalTime(), v => v);
                }
                else if (property.ClrType == typeof(DateTimeOffset?))
                {
                    modelBuilder.Entity(entityType.ClrType)
                                .Property<DateTimeOffset?>(property.Name)
                                .HasConversion(v => v.HasValue ? v.Value.ToUniversalTime() : v, v => v);
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
    /// Uses <paramref name="collation"/> for string properties.
    /// </summary>
    /// <param name="modelBuilder"></param>
    /// <param name="collation"></param>
    public static ModelBuilder UseCollationOnStringProperties(this ModelBuilder modelBuilder, string collation)
    {
        var entityClrTypesIsString = modelBuilder.Model.GetEntityTypes()
                                                       .Where(e => Array.Exists(e.ClrType.GetProperties(), p => p.PropertyType.IsAssignableFrom(typeof(string))))
                                                       .Select(e => e.ClrType);

        foreach (var clrType in entityClrTypesIsString)
        {
            var properties = clrType.GetProperties().Where(p => p.PropertyType.IsAssignableFrom(typeof(string))
                                                                && !p.CustomAttributes.Any(cA => cA.AttributeType.IsEquivalentTo(typeof(NotMappedAttribute))));

            foreach (var prop in properties)
                modelBuilder.Entity(clrType).Property(prop.Name).UseCollation(collation);
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
    /// Enable the query filter for single database multi tenancy scenarios.
    /// </summary>
    /// <param name="modelBuilder"></param>
    /// <param name="tenantId"></param>
    public static ModelBuilder UseTenantIdQueryFilter(this ModelBuilder modelBuilder, TenantId tenantId)
    {
        // Get entities that have the tenant id property
        var tenantEntities = modelBuilder.Model.GetEntityTypes().Where(entityType => entityType.ClrType.IsAssignableTo(typeof(IHasTenantId)));

        foreach (var entityType in tenantEntities)
        {
            var parameter = Expression.Parameter(entityType.ClrType, "entity");

            // Find the property that matches the tenant ID property name
            var property = entityType.FindProperty(EntityPropertyNames.TenantId);

            if (property?.PropertyInfo == null)
                continue;

            // Create the expression: entity.TenantId == tenantId
            var propertyAccess = Expression.Property(parameter, property.PropertyInfo);
            var tenantIdValue = Expression.Constant(tenantId, property.ClrType);
            var filterExpression = Expression.Equal(propertyAccess, tenantIdValue);

            // Build the lambda expression: entity => entity.TenantId == tenantId
            var lambda = Expression.Lambda(filterExpression, parameter);

            entityType.SetQueryFilter(lambda);
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
                                                         .Where(e => Array.Exists(e.ClrType.GetProperties(),
                                                                                  p => p.PropertyType.IsAssignableFrom(typeof(decimal))
                                                                                     || p.PropertyType.CanAssignableTo(typeof(decimal?))))
                                                         .Select(e => e.ClrType);

        foreach (var clrType in entityClrTypesHasDecimalProperty)
        {
            var properties = clrType.GetProperties().Where(p => (p.PropertyType.IsAssignableFrom(typeof(decimal)) || p.PropertyType.CanAssignableTo(typeof(decimal?)))
                                                                      && (!p.CustomAttributes?.Any(ca => (ca.AttributeType?.IsEquivalentTo(typeof(NotMappedAttribute)) ?? false)
                                                                                                      || (ca.AttributeType?.IsEquivalentTo(typeof(DecimalPrecisionAttribute)) ?? false)) ?? true));

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
    /// Translation entities relationships. It is not build relationships for ColumnAttribute(TypeName='jsonb') decorated properties.
    /// </summary>
    /// <remarks>
    /// You can use this method with <see cref="UsePrecision(ModelBuilder, int, int)"/> method. 
    /// </remarks>
    /// <param name="modelBuilder"></param>
    public static ModelBuilder UseTranslationEntityRelations(this ModelBuilder modelBuilder)
    {
        var languageEntities = modelBuilder.Model.GetEntityTypes().Where(e => e.ClrType.CanAssignableTo(typeof(IHasTranslation<>)));

        foreach (var entityClrType in languageEntities.Select(i => i.ClrType))
        {
            if (Array.Exists(entityClrType.GetProperties(), p => (p.GetCustomAttribute<ColumnAttribute>()?.TypeName == "jsonb") && p.Name == MultiLanguageEntityPropertyNames.Translations))
                continue;

            modelBuilder.Entity(entityClrType)
                        .HasMany(MultiLanguageEntityPropertyNames.Translations)
                        .WithOne(MultiLanguageEntityPropertyNames.Entity)
                        .HasForeignKey(MultiLanguageEntityPropertyNames.EntityId);
        }

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
    /// Adds an index for each indelible entity for IsDeleted property.
    /// </summary>
    /// <param name="modelBuilder"></param>
    public static ModelBuilder UseIndexToSoftDeletableEntities(this ModelBuilder modelBuilder)
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
    /// <param name="createCompositeWithIsDeleted"> If the entity has IsDeleted property, creates index as composite with IsDeleted and TenantId </param>
    public static ModelBuilder UseIndexToCreationAuditableEntities(this ModelBuilder modelBuilder, bool createCompositeWithIsDeleted = true)
    {
        var hasCreationDateEntities = modelBuilder.Model.GetEntityTypes().Where(entityType => entityType.FindProperty(EntityPropertyNames.CreationDate) != null);

        foreach (var entityType in hasCreationDateEntities)
        {
            if (entityType.ClrType.GetCustomAttribute<DontIndexCreationDateAttribute>() is null &&
                entityType.FindProperty(EntityPropertyNames.CreationDate)?.ClrType.GetCustomAttribute<DontIndexCreationDateAttribute>() is null)
            {

                if (createCompositeWithIsDeleted && entityType.FindProperty(EntityPropertyNames.IsDeleted) is not null)
                {
                    modelBuilder.Entity(entityType.ClrType).HasIndex(EntityPropertyNames.CreationDate, EntityPropertyNames.IsDeleted);
                }
                else
                    modelBuilder.Entity(entityType.ClrType).HasIndex(EntityPropertyNames.CreationDate);
            }
        }

        return modelBuilder;
    }

    /// <summary>
    /// Adds an index for <see cref="LogEntityBase{TKey}"/> entity.
    /// Added indexes;
    /// <para></para> Unique <see cref="EntityPropertyNames.TransactionId"/>
    /// <para></para> Descending <see cref="EntityPropertyNames.UtcLogTime"/> 
    /// <para></para><see cref="EntityPropertyNames.MethodName"/>
    /// <para></para><see cref="EntityPropertyNames.IsSuccess"/>
    /// </summary>
    /// <param name="modelBuilder"></param>
    public static ModelBuilder UseLogEntityBaseIndexes(this ModelBuilder modelBuilder)
    {
        var logEntityBaseEntities = modelBuilder.Model.GetEntityTypes().Where(entityType => entityType.ClrType.CanAssignableTo(typeof(LogEntityBase<>)));

        foreach (var entityType in logEntityBaseEntities.Select(e => e.ClrType))
        {
            modelBuilder.Entity(entityType).HasIndex(EntityPropertyNames.TransactionId);
            modelBuilder.Entity(entityType).HasIndex(EntityPropertyNames.UtcLogTime).IsDescending(true);
            modelBuilder.Entity(entityType).HasIndex(EntityPropertyNames.MethodName);
            modelBuilder.Entity(entityType).HasIndex(EntityPropertyNames.IsSuccess);
        }

        return modelBuilder;
    }

    /// <summary>
    /// Adds new composite index of entities that implement <see cref="WithTenantIdIndexAttribute"/>.
    /// </summary>
    /// <param name="modelBuilder"></param>
    /// <returns></returns>
    public static ModelBuilder UseWithTenantIdCompositeIndexes(this ModelBuilder modelBuilder)
    {
        var entitiesWithTenantId = modelBuilder.Model.GetEntityTypes().Where(entityType => entityType.ClrType.CanAssignableTo(typeof(IHasTenantId)));

        foreach (var entityType in entitiesWithTenantId)
        {
            var tenantIdProperty = Array.Find(entityType.ClrType.GetProperties(), p => p.Name == EntityPropertyNames.TenantId);

            if (tenantIdProperty == null)
                continue;

            var properties = entityType.GetProperties().Where(p => p.PropertyInfo.GetCustomAttribute<WithTenantIdIndexAttribute>() != null).ToArray();

            var mutableEntity = modelBuilder.Entity(entityType.ClrType);

            foreach (var prop in properties)
            {
                var newProps = new List<PropertyInfo>(capacity: 2)
                {
                    tenantIdProperty,
                    prop.PropertyInfo
                };

                mutableEntity.HasIndex([.. newProps.Select(p => p.Name)]);
            }
        }

        return modelBuilder;
    }

    /// <summary>
    /// Adds new composite index of entities that implement <see cref="WithTenantIdIndexAttribute"/>.
    /// </summary>
    /// <param name="modelBuilder"></param>
    /// <param name="createCompositeWithIsDeleted"> If the entity has IsDeleted property, creates index as composite with IsDeleted and TenantId </param>
    /// <returns></returns>
    public static ModelBuilder UseTenantIdIndexes(this ModelBuilder modelBuilder, bool createCompositeWithIsDeleted = true)
    {
        var entitiesWithTenantId = modelBuilder.Model.GetEntityTypes().Where(entityType => entityType.ClrType.CanAssignableTo(typeof(IHasTenantId)));

        foreach (var (entityType, tenantIdProperty) in from entityType in entitiesWithTenantId
                                                       let tenantIdProperty = Array.Find(entityType.ClrType.GetProperties(), p => p.Name == EntityPropertyNames.TenantId)
                                                       select (entityType, tenantIdProperty))
        {
            if (tenantIdProperty == null)
                continue;

            var mutableEntity = modelBuilder.Entity(entityType.ClrType);

            if (createCompositeWithIsDeleted && entityType.FindProperty(EntityPropertyNames.IsDeleted) is not null)
            {
                mutableEntity.HasIndex(EntityPropertyNames.IsDeleted, tenantIdProperty.Name);
            }
            else
                mutableEntity.HasIndex(tenantIdProperty.Name);
        }

        return modelBuilder;
    }

    /// <summary>
    /// Adds TenantId to the existing indexes of entities that implement <see cref="IHasTenantId"/>.
    /// </summary>
    /// <param name="modelBuilder"></param>
    /// <returns></returns>
    public static ModelBuilder ConvertHasTenantIdIndexesToCompositeIndexes(this ModelBuilder modelBuilder)
    {
        var entitiesWithTenantId = modelBuilder.Model.GetEntityTypes().Where(entityType => entityType.ClrType.CanAssignableTo(typeof(IHasTenantId)));

        foreach (var entityType in entitiesWithTenantId)
        {
            var tenantIdProperty = entityType.ClrType.GetProperties().First(p => p.Name == EntityPropertyNames.TenantId);

            if (tenantIdProperty == null)
                continue;

            var mutableEntity = modelBuilder.Entity(entityType.ClrType);

            foreach (var index in entityType.GetIndexes().ToList())
            {
                var props = index.Properties;

                // If the index already contains TenantId, skip it.
                if (props.Any(p => p.Name == EntityPropertyNames.TenantId))
                    continue;

                entityType.RemoveIndex(index);

                var newProps = props.Select(p => p.PropertyInfo).ToList();

                newProps.Add(tenantIdProperty);

                mutableEntity.HasIndex([.. newProps.Select(p => p.Name)]);
            }
        }

        return modelBuilder;
    }
}