﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Milvasoft.Helpers.DataAccess.Abstract.Entity;
using Milvasoft.Helpers.DataAccess.Concrete.Entity;
using Milvasoft.Helpers.DependencyInjection;
using Milvasoft.Helpers.Exceptions;
using Milvasoft.Helpers.Identity.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.DataAccess.MilvaContext
{

    /// <summary>
    /// This class handles all database operations.
    /// </summary>
    /// 
    /// <remarks>
    /// <para> You must register <see cref="IAuditConfiguration"/> in your application startup. </para>
    /// </remarks>
    /// <typeparam name="TUser"></typeparam>
    /// <typeparam name="TRole"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public abstract class MilvaDbContext<TUser, TRole, TKey> : IdentityDbContext<TUser, TRole, TKey>
    where TUser : MilvaUser<TKey>
    where TRole : IdentityRole<TKey>
    where TKey : struct, IEquatable<TKey>
    {
        private readonly TUser _currentUser;
        private IAuditConfiguration _auditConfiguration;
        private static AsyncLocal<bool> _ignoreSoftDelete = new AsyncLocal<bool>();

        /// <summary>
        /// Cunstructor of <c cref="MilvaDbContext{TUser, TRole, TKey}"></c>.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="auditConfiguration"></param>
        public MilvaDbContext(DbContextOptions<MilvaDbContext<TUser, TRole, TKey>> options,
                              UserManager<TUser> userManager,
                              IHttpContextAccessor httpContextAccessor,
                              IAuditConfiguration auditConfiguration) : base(options)
        {
            var userName = httpContextAccessor.HttpContext.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userName))
            {
                _currentUser = userManager.FindByNameAsync(httpContextAccessor.HttpContext.User.Identity.Name).Result;
            }
            _auditConfiguration = auditConfiguration;
            _ignoreSoftDelete.Value = false;
        }

        /// <summary>
        /// Overrided the OnModelCreating for custom configurations to database.
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region TUser.Set_ForeignKeys

            modelBuilder.Entity<TUser>()
                .HasOne(p => p.DeleterUser)
                .WithMany()
                .HasForeignKey(p => p.DeleterUserId);

            modelBuilder.Entity<TUser>()
                .HasOne(p => p.CreatorUser)
                .WithMany()
                .HasForeignKey(p => p.CreatorUserId);

            modelBuilder.Entity<TUser>()
                .HasOne(p => p.LastModifierUser)
                .WithMany()
                .HasForeignKey(p => p.LastModifierUserId);

            #endregion

            modelBuilder.ConfigureDecimalProperties();
            modelBuilder.AddIndexToIndelibleEntities();
            modelBuilder.ConfigureDefaultValue();
            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Ignores soft delete for next process.
        /// </summary>
        public static void IgnoreSoftDeleteForNextProcess() => _ignoreSoftDelete.Value = true;

        /// <summary>
        /// Activate soft delete.
        /// </summary>
        public static void ActivateSoftDelete() => _ignoreSoftDelete.Value = false;

        /// <summary>
        /// Overrided the SaveChanges method for soft deleting.
        /// </summary>
        /// <returns></returns>
        public override int SaveChanges()
        {
            AuditEntites();
            return base.SaveChanges();
        }

        /// <summary>
        /// Overrided the SaveChangesAsync method for soft deleting.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            AuditEntites();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Gets requested contents by <paramref name="type"/> DbSet.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<object> GetRequiredContents(Type type)
        {
            string propName = $"{type.Name}Langs";
            if (!CommonHelper.PropertyExists(type, propName))
                throw new NullParameterException($"Type of {type}'s properties doesn't contain '{propName}'.");

            ParameterExpression paramterExpression = Expression.Parameter(type, "i");
            Expression orderByProperty = Expression.Property(paramterExpression, propName);

            Expression<Func<object, object>> predicate = Expression.Lambda<Func<object, object>>(Expression.Convert(Expression.Property(paramterExpression, propName), typeof(object)), paramterExpression);

            var dbSet = this.GetType().GetMethod("Set").MakeGenericMethod(type).Invoke(this, null);

            var whereMethods = typeof(Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(mi => mi.Name == "ToListAsync");

            MethodInfo whereMethod = whereMethods.FirstOrDefault();

            whereMethod = whereMethod.MakeGenericMethod(type);

            var ret = (Task)whereMethod.Invoke(dbSet, new object[] { dbSet, null });
            await ret.ConfigureAwait(false);

            var resultProperty = ret.GetType().GetProperty("Result");
            return resultProperty.GetValue(ret);
        }

        /// <summary>
        /// Gets requested contents by <typeparamref name="TEntity"/> DbSet.
        /// </summary>
        /// <returns></returns>
        public async Task<List<TEntity>> GetRequiredContentsAsync<TEntity>() where TEntity : class
            => await this.Set<TEntity>().Where(CreateIsDeletedFalseExpression<TEntity>()).IncludeLang(this).ToListAsync().ConfigureAwait(false);

        /// <summary>
        /// Gets the requested <typeparamref name="TEntity"/>'s property's(<paramref name="propName"/>) max value.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="propName"></param>
        /// <returns></returns>
        public async Task<decimal> GetMaxValueAsync<TEntity>(string propName) where TEntity : class
        {
            var entityType = typeof(TEntity);

            if (!CommonHelper.PropertyExists<TEntity>(propName))
                throw new InvalidParameterException($"Type of {entityType}'s properties doesn't contain '{propName}'.");

            ParameterExpression parameterExpression = Expression.Parameter(entityType, "i");
            Expression<Func<TEntity, decimal>> predicate = Expression.Lambda<Func<TEntity, decimal>>(Expression.Convert(Expression.Property(parameterExpression, propName), typeof(decimal)), parameterExpression);

            return await this.Set<TEntity>().Where(CreateIsDeletedFalseExpression<TEntity>()).IncludeLang(this).MaxAsync(predicate).ConfigureAwait(false);
        }


        #region Private Methods

        /// <summary>
        /// Gets <b>entity => entity.IsDeleted == false</b> expression, if <typeparamref name="TEntity"/> is assignable from <see cref="IFullAuditable{TKey}"/>.
        /// </summary>
        /// <returns></returns>
        private Expression<Func<TEntity, bool>> CreateIsDeletedFalseExpression<TEntity>()
        {
            var entityType = typeof(TEntity);
            if (entityType.BaseType.Name == typeof(FullAuditableEntity<>).Name)
            {
                var parameter = Expression.Parameter(entityType, "entity");
                var filterExpression = Expression.Equal(Expression.Property(parameter, entityType.GetProperty(EntityPropertyNames.IsDeleted)), Expression.Constant(false, typeof(bool)));
                return Expression.Lambda<Func<TEntity, bool>>(filterExpression, parameter);
            }
            else return null;
        }

        /// <summary>
        /// Soft delete operation.
        /// </summary>
        /// <param name="entry"></param>
        private void SoftDelete(EntityEntry entry)
        {
            AuditDeletion(entry);

            //For changing includes
            foreach (var navigationEntry in entry.Navigations)
                if (navigationEntry is CollectionEntry collectionEntry && collectionEntry?.CurrentValue != null)
                    foreach (var dependentEntry in collectionEntry.CurrentValue)
                        AuditDeletion(Entry(dependentEntry));
                else if (navigationEntry?.CurrentValue != null)
                    AuditDeletion(Entry(navigationEntry.CurrentValue));
        }

        /// <summary>
        /// Entity auditing for delete. 
        /// </summary>
        /// <param name="entry"></param>
        private void AuditDeletion(EntityEntry entry)
        {
            if (!entry.Metadata.GetProperties().Any(x => x.Name == EntityPropertyNames.IsDeleted))
                return;

            entry.State = EntityState.Modified;

            entry.Property(EntityPropertyNames.IsDeleted).CurrentValue = true;
            entry.Property(EntityPropertyNames.IsDeleted).IsModified = true;

            entry.Property(EntityPropertyNames.DeletionDate).CurrentValue = DateTime.Now;
            entry.Property(EntityPropertyNames.DeletionDate).IsModified = true;

            if (entry.Metadata.GetProperties().Any(prop => prop.Name == EntityPropertyNames.DeleterUserId))
            {
                if (_auditConfiguration.AuditDeleter)
                {
                    entry.Property(EntityPropertyNames.DeleterUserId).CurrentValue = _currentUser?.Id;
                    entry.Property(EntityPropertyNames.DeleterUserId).IsModified = true;
                }
            }
        }

        /// <summary>
        /// Entity auditing by date. 
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="propertyName"></param>
        private void AuditDate(EntityEntry entry, string propertyName)
        {
            entry.Property(propertyName).CurrentValue = DateTime.Now;
            entry.Property(propertyName).IsModified = true;
        }

        /// <summary>
        /// Entity auditing by performer user.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="propertyName"></param>
        private void AuditPerformerUser(EntityEntry entry, string propertyName)
        {
            entry.Property(propertyName).CurrentValue = _currentUser?.Id;
            entry.Property(propertyName).IsModified = true;
        }

        /// <summary>
        /// Provides auditing entities by <see cref="_auditConfiguration"/>.
        /// If deletion process happens then sets the <see cref="_ignoreSoftDelete"/> variable to true at the end of process.
        /// </summary>
        private void AuditEntites()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        if (_auditConfiguration.AuditCreationDate)
                        {
                            if (entry.Metadata.GetProperties().Any(prop => prop.Name == EntityPropertyNames.CreationDate))
                                AuditDate(entry, EntityPropertyNames.CreationDate);
                        }
                        if (_auditConfiguration.AuditCreator)
                        {
                            if (entry.Metadata.GetProperties().Any(prop => prop.Name == EntityPropertyNames.CreatorUserId))
                                AuditPerformerUser(entry, EntityPropertyNames.CreatorUserId);
                        }
                        break;
                    case EntityState.Modified:
                        if (_auditConfiguration.AuditModificationDate)
                        {
                            if (entry.Metadata.GetProperties().Any(prop => prop.Name == EntityPropertyNames.LastModificationDate))
                                AuditDate(entry, EntityPropertyNames.CreationDate);
                        }
                        if (_auditConfiguration.AuditModifier)
                        {
                            if (entry.Metadata.GetProperties().Any(prop => prop.Name == EntityPropertyNames.LastModifierUserId))
                                AuditPerformerUser(entry, EntityPropertyNames.CreatorUserId);
                        }
                        break;
                    case EntityState.Deleted:
                        if (entry.Metadata.GetProperties().Any(prop => prop.Name == EntityPropertyNames.IsDeleted))
                        {
                            if (!_ignoreSoftDelete.Value)
                                SoftDelete(entry);
                        }
                        _ignoreSoftDelete.Value = false;
                        break;
                    default:
                        break;
                }
            }
           
        }

        #endregion
    }

}