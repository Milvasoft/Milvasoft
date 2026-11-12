using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Milvasoft.Attributes.Annotations;
using Milvasoft.Core.EntityBases.MultiTenancy;
using Milvasoft.Core.MultiLanguage.EntityBases;
using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;
using Milvasoft.Cryptography.Abstract;
using Milvasoft.DataAccess.EfCore.Utils.Converters;
using MongoDB.Bson;
using System.Collections;
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
    /// <returns></returns>
    /// <exception cref="MilvaDeveloperException"></exception>
    public static ModelBuilder UseAnnotationEncryption(this ModelBuilder modelBuilder, IMilvaCryptographyProvider encryptionProvider)
    {
        if (modelBuilder is null)
            throw new MilvaDeveloperException("The given model builder cannot be null");

        if (encryptionProvider is null)
            throw new MilvaDeveloperException("Cannot initialize encryption with a null provider.");

        var stringEncryptionConverter = new MilvaEncryptionConverter(encryptionProvider);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                var clrType = property.ClrType;

                // string + [Encrypted]
                if (clrType == typeof(string))
                {
                    var propInfo = property.PropertyInfo; // shadow property olabilir
                    if (propInfo is null)
                        continue;

                    if (propInfo.IsDefined(typeof(EncryptedAttribute), inherit: false))
                    {
                        property.SetValueConverter(stringEncryptionConverter);
                    }

                    continue;
                }

                // EncryptedJsonValueConverter<T> for jsonb columns
                // (POCO -> jsonb field based encryption)
                var columnType = property.GetColumnType();
                var isJsonb = !string.IsNullOrWhiteSpace(columnType) && columnType.Contains("jsonb", StringComparison.OrdinalIgnoreCase);

                // class && !string && !enum && !Nullable<> struct etc.
                // Except array, list, etc.
                var isPoco = clrType.IsClass && clrType != typeof(string) && !typeof(IEnumerable).IsAssignableFrom(clrType);

                if (isJsonb && isPoco)
                {
                    var convType = typeof(EncryptedJsonValueConverter<>).MakeGenericType(clrType);

                    var converter = (ValueConverter)Activator.CreateInstance(convType, encryptionProvider)!;

                    property.SetValueConverter(converter);

                    // Value converter for change tracker
                    var comparerType = typeof(JsonValueComparer<>).MakeGenericType(clrType);

                    var comparer = (ValueComparer)Activator.CreateInstance(comparerType)!;

                    property.SetValueComparer(comparer);

                    // If column type is empty, set it as jsonb (if already set, do not touch it)
                    if (string.IsNullOrWhiteSpace(columnType))
                        property.SetColumnType("jsonb");

                    continue;
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
    /// Enable the query filter for indelible entities. Filters out entities whose <see cref="EntityPropertyNames.IsDeleted"/> is true.
    /// </summary>
    /// <remarks>
    /// The filter is added to the root entity type of each hierarchy (EF Core only allows query filters on root entity types) and
    /// does not overwrite the other query filters of the entity type. See <see cref="AddQueryFilter(IMutableEntityType, string, LambdaExpression)"/>.
    /// </remarks>
    /// <param name="modelBuilder"></param>
    public static ModelBuilder UseSoftDeleteQueryFilter(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in GetQueryFilterableEntityTypes(modelBuilder))
        {
            var isDeletedProperty = entityType.FindProperty(EntityPropertyNames.IsDeleted);

            if (isDeletedProperty?.ClrType != typeof(bool))
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "entity");

            // EF.Property also works for shadow properties.
            var filterExpression = Expression.Equal(CreatePropertyAccess(parameter, isDeletedProperty), Expression.Constant(false));

            AddQueryFilter(entityType, SoftDeleteQueryFilterName, Expression.Lambda(filterExpression, parameter));
        }

        return modelBuilder;
    }

    /// <summary>
    /// Enable the query filter for single database multi tenancy scenarios with a fixed tenant id.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b> <paramref name="tenantId"/> is a part of the model, and EF Core caches the model once per context type. </b>
    /// So the tenant id of the first created context is used by every context instance. Use this overload only when the tenant is fixed for the application.
    /// For per request (per context instance) tenants use <see cref="UseTenantIdQueryFilter{TContext}(ModelBuilder, TContext, Expression{Func{TContext, TenantId}})"/>.
    /// </para>
    /// <para> The filter does not overwrite the other query filters of the entity type. See <see cref="AddQueryFilter(IMutableEntityType, string, LambdaExpression)"/>. </para>
    /// </remarks>
    /// <param name="modelBuilder"></param>
    /// <param name="tenantId"></param>
    public static ModelBuilder UseTenantIdQueryFilter(this ModelBuilder modelBuilder, TenantId tenantId)
        => modelBuilder.UseTenantIdQueryFilter(Expression.Constant(tenantId, typeof(TenantId)));

    /// <summary>
    /// Enable the query filter for single database multi tenancy scenarios. The current tenant id is read from <paramref name="context"/>
    /// with <paramref name="currentTenantIdSelector"/> on every query, so each context instance is filtered by its own tenant.
    /// </summary>
    /// <remarks>
    /// Call it in OnModelCreating and pass the context itself. e.g. <c>modelBuilder.UseTenantIdQueryFilter(this, c => c.CurrentTenantId);</c>
    /// The filter does not overwrite the other query filters of the entity type. See <see cref="AddQueryFilter(IMutableEntityType, string, LambdaExpression)"/>.
    /// </remarks>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="modelBuilder"></param>
    /// <param name="context"> The context that is building the model (this). </param>
    /// <param name="currentTenantIdSelector"> Selects current tenant id member of the context. </param>
    public static ModelBuilder UseTenantIdQueryFilter<TContext>(this ModelBuilder modelBuilder, TContext context, Expression<Func<TContext, TenantId>> currentTenantIdSelector) where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(currentTenantIdSelector);

        // A member access on the context instance constant is parameterized by EF Core with the executing context instance.
        var currentTenantId = new ParameterReplacer(currentTenantIdSelector.Parameters[0], Expression.Constant(context, typeof(TContext))).Visit(currentTenantIdSelector.Body);

        return modelBuilder.UseTenantIdQueryFilter(currentTenantId);
    }

    private static ModelBuilder UseTenantIdQueryFilter(this ModelBuilder modelBuilder, Expression currentTenantId)
    {
        foreach (var entityType in GetQueryFilterableEntityTypes(modelBuilder).Where(e => e.ClrType.IsAssignableTo(typeof(IHasTenantId))))
        {
            var tenantIdProperty = entityType.FindProperty(EntityPropertyNames.TenantId);

            if (tenantIdProperty is null || (tenantIdProperty.ClrType != typeof(TenantId) && tenantIdProperty.ClrType != typeof(TenantId?)))
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "entity");

            var tenantIdValue = tenantIdProperty.ClrType == currentTenantId.Type ? currentTenantId : Expression.Convert(currentTenantId, tenantIdProperty.ClrType);

            // entity => entity.TenantId == tenantId
            var filterExpression = Expression.Equal(CreatePropertyAccess(parameter, tenantIdProperty), tenantIdValue);

            AddQueryFilter(entityType, TenantIdQueryFilterName, Expression.Lambda(filterExpression, parameter));
        }

        return modelBuilder;
    }

    /// <summary>
    /// Query filter name used by <see cref="UseSoftDeleteQueryFilter(ModelBuilder)"/> when the entity type uses named query filters.
    /// </summary>
    public const string SoftDeleteQueryFilterName = "MilvaSoftDelete";

    /// <summary>
    /// Query filter name used by tenant id query filters when the entity type uses named query filters.
    /// </summary>
    public const string TenantIdQueryFilterName = "MilvaTenantId";

    /// <summary>
    /// Adds <paramref name="filter"/> to the query filters of <paramref name="entityType"/> without overwriting the existing ones.
    /// </summary>
    /// <remarks>
    /// EF Core does not allow combining an anonymous query filter with named query filters, and setting an anonymous filter replaces the existing one.
    /// <list type="bullet">
    /// <item> If the entity type has named query filters, <paramref name="filter"/> is added as a named filter with <paramref name="filterName"/>. </item>
    /// <item> Otherwise it is combined (AND) with the existing anonymous filter (if any) and set as the anonymous filter, which keeps
    ///        compatibility with anonymous filters configured with <c>HasQueryFilter</c>. Configure anonymous filters before calling this method,
    ///        an anonymous filter configured later replaces the combined filter. </item>
    /// </list>
    /// </remarks>
    /// <param name="entityType"></param>
    /// <param name="filterName"></param>
    /// <param name="filter"></param>
    public static void AddQueryFilter(this IMutableEntityType entityType, string filterName, LambdaExpression filter)
    {
        ArgumentNullException.ThrowIfNull(entityType);
        ArgumentNullException.ThrowIfNull(filter);

        var existingFilters = entityType.GetDeclaredQueryFilters();

        if (existingFilters.Any(f => !f.IsAnonymous))
        {
            entityType.SetQueryFilter(filterName, filter);
            return;
        }

        var existingAnonymousFilter = existingFilters.FirstOrDefault(f => f.IsAnonymous)?.Expression;

        if (existingAnonymousFilter is not null)
        {
            var parameter = existingAnonymousFilter.Parameters[0];
            var filterBody = new ParameterReplacer(filter.Parameters[0], parameter).Visit(filter.Body);

            filter = Expression.Lambda(Expression.AndAlso(existingAnonymousFilter.Body, filterBody), parameter);
        }

        entityType.SetQueryFilter(filter);
    }

    /// <summary>
    /// Root, not owned entity types. EF Core only allows query filters on them.
    /// </summary>
    private static List<IMutableEntityType> GetQueryFilterableEntityTypes(ModelBuilder modelBuilder)
        => [.. modelBuilder.Model.GetEntityTypes().Where(e => e.BaseType is null && !e.IsOwned())];

    private static Expression CreatePropertyAccess(ParameterExpression parameter, IReadOnlyProperty property)
        => property.PropertyInfo is not null && property.DeclaringType.ClrType.IsAssignableFrom(parameter.Type)
            ? Expression.Property(parameter, property.PropertyInfo)
            : Expression.Call(typeof(EF), nameof(EF.Property), [property.ClrType], parameter, Expression.Constant(property.Name));

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