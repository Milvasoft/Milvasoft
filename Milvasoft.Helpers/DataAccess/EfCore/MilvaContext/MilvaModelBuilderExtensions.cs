using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Milvasoft.Helpers.DataAccess.Attributes;
using Milvasoft.Helpers.DataAccess.Concrete.Entity;
using Milvasoft.Helpers.Encryption.Abstract;
using Milvasoft.Helpers.Encryption.Concrete;
using Milvasoft.Helpers.Exceptions;
using Milvasoft.Helpers.MultiTenancy.EntityBase;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Milvasoft.Helpers.DataAccess.MilvaContext
{
    /// <summary>
    /// Extension methods for <c cref="MilvaDbContext{TUser, TRole, TKey}"></c>.
    /// </summary>
    public static class MilvaModelBuilderExtensions
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
                var properties = entityType
                    .ClrType
                    .GetProperties()
                    .Where(p => p.PropertyType == typeof(TenantId));

                foreach (var property in properties)
                {
                    modelBuilder
                        .Entity(entityType.Name)
                        .Property(property.Name)
                        .HasConversion(tenantIdConverter);
                }
            }

            return modelBuilder;
        }

        /// <summary>
        /// Adds value converter(<see cref="MilvaEncryptionConverter"/>) to string properties which marked with <see cref="MilvaEncryptedAttribute"/>.
        /// </summary>
        /// <param name="modelBuilder"></param>
        /// <param name="encryptionProvider"></param>
        public static ModelBuilder UseAnnotationEncryption(this ModelBuilder modelBuilder, IMilvaEncryptionProvider encryptionProvider)
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
                        object[] attributes = property.PropertyInfo.GetCustomAttributes(typeof(MilvaEncryptedAttribute), false);

                        if (attributes.Any())
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
        public static ModelBuilder UseEncryption(this ModelBuilder modelBuilder, IMilvaEncryptionProvider encryptionProvider)
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
        public static ModelBuilder ConfigureStringProperties(this ModelBuilder modelBuilder)
        {
            var entitiesHasDecimalProperty = modelBuilder.Model.GetEntityTypes().Where(prop => prop.ClrType.GetProperties().Any(p => p.PropertyType.IsAssignableFrom(typeof(string))));

            foreach (var entityType in entitiesHasDecimalProperty)
            {
                var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType.IsAssignableFrom(typeof(string))
                                                                                && !p.CustomAttributes.Any(cA => cA.AttributeType.IsAssignableFrom(typeof(NotMappedAttribute))));

                foreach (var prop in properties)
                    modelBuilder.Entity(entityType.ClrType).Property(prop.Name).UseCollation("tr-TR-x-icu");
            }
            return modelBuilder;
        }

        /// <summary>
        /// Adds an index for each indelible entity for IsDeleted property.
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static ModelBuilder AddIndexToIndelibleEntities(this ModelBuilder modelBuilder)
        {
            var indelibleEntites = modelBuilder.Model.GetEntityTypes().Where(entityType => entityType.FindProperty(EntityPropertyNames.IsDeleted) != null);

            foreach (var entityType in indelibleEntites)
                modelBuilder.Entity(entityType.ClrType).HasIndex(EntityPropertyNames.IsDeleted);

            return modelBuilder;
        }

        /// <summary>
        /// Configures default value for update database.
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static ModelBuilder ConfigureDefaultValue(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                foreach (var property in entityType.GetProperties())
                {
                    var memberInfo = property.PropertyInfo ?? (MemberInfo)property.FieldInfo;

                    if (memberInfo == null) continue;

                    var defaultValue = Attribute.GetCustomAttribute(memberInfo, typeof(MilvaDefaultValueAttribute)) as MilvaDefaultValueAttribute;

                    if (defaultValue == null) continue;

                    modelBuilder.Entity(entityType.ClrType).Property(property.Name).HasDefaultValue(defaultValue.DefaultValue);
                }
            return modelBuilder;
        }

        /// <summary>
        /// Enable the query filter for indelible entities.
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static ModelBuilder FilterDeletedEntities(this ModelBuilder modelBuilder)
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
        /// Enable the query filter for indelible entities.
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static ModelBuilder IgnoreDefaultRecords(this ModelBuilder modelBuilder)
        {
            var indelibleEntites = modelBuilder.Model.GetEntityTypes().Where(entityType => entityType.FindProperty(EntityPropertyNames.Id) != null
                                                                                          && entityType.FindProperty(EntityPropertyNames.Id).PropertyInfo.PropertyType == typeof(int));
            foreach (var entityType in indelibleEntites)
            {
                var parameter = Expression.Parameter(entityType.ClrType, "entity");

                var prop = entityType.FindProperty(EntityPropertyNames.Id);

                var filterExpression = Expression.GreaterThan(Expression.Property(parameter, prop.PropertyInfo), Expression.Constant(50, typeof(int)));

                var dynamicLambda = Expression.Lambda(filterExpression, parameter);

                entityType.SetQueryFilter(dynamicLambda);
            }
            return modelBuilder;
        }

        /// <summary>
        /// Configures the decimal property of entities with decimal properties in decimal (18,2) format.
        /// </summary>
        /// <param name="modelBuilder"></param>
        /// <param name="beforeSeperatorCount"></param>
        /// <param name="afterSeperatorCount"></param>
        public static ModelBuilder ConfigureDecimalProperties(this ModelBuilder modelBuilder, int beforeSeperatorCount = 18, int afterSeperatorCount = 10)
        {
            var entitiesHasDecimalProperty = modelBuilder.Model.GetEntityTypes().Where(prop => prop.ClrType.GetProperties().Any(p => p.PropertyType.IsAssignableFrom(typeof(decimal))));

            foreach (var entityType in entitiesHasDecimalProperty)
            {
                var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType.IsAssignableFrom(typeof(decimal))
                                                                                && !p.CustomAttributes.Any(cA => cA.AttributeType.IsAssignableFrom(typeof(NotMappedAttribute))));

                foreach (var prop in properties)
                    modelBuilder.Entity(entityType.ClrType).Property(prop.Name).HasColumnType($"decimal({beforeSeperatorCount},{afterSeperatorCount})");
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
        /// Entities must be contains "Langs" navigation property for include process. (e.g. ProductLangs)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="context"></param>
        public static IQueryable<T> IncludeLang<T>(this IQueryable<T> source, DbContext context)
            where T : class
        {
            var navigations = context.Model.FindEntityType(typeof(T))
                                           .GetDerivedTypesInclusive()
                                           .SelectMany(type => type.GetNavigations())
                                           .Distinct();

            foreach (var property in navigations)
                if (property.Name.Contains("Langs"))
                    source = source.Include(property.Name);

            return source;
        }
    }
}
