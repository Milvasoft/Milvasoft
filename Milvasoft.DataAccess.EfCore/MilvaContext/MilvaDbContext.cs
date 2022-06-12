﻿using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Milvasoft.Core;
using Milvasoft.Core.Abstractions;
using Milvasoft.Core.EntityBase.Abstract;
using Milvasoft.Core.EntityBase.Concrete;
using Milvasoft.Core.Exceptions;
using Milvasoft.Core.Extensions;
using System.Linq.Expressions;
using System.Reflection;

namespace Milvasoft.DataAccess.EfCore.MilvaContext;

/// <summary>
/// Interface for base repository.
/// </summary>
public interface IMilvaDbContextBase
{
    /// <summary>
    /// Overrided the BulkSaveChanges method for soft deleting.
    /// </summary>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    void SaveChangesBulk(BulkConfig bulkConfig = null, CancellationToken cancellationToken = new CancellationToken());

    /// <summary>
    /// Overrided the BulkSaveChangesAsync method for soft deleting.
    /// </summary>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SaveChangesBulkAsync(BulkConfig bulkConfig = null, CancellationToken cancellationToken = new CancellationToken());
}

/// <summary>
/// Handles all database operations.
/// </summary>
public abstract class MilvaDbContextBase<TUser, TUserKey> : DbContext, IMilvaDbContextBase
    where TUser : class, IFullAuditable<TUserKey>
    where TUserKey : struct, IEquatable<TUserKey>
{
    #region Public Properties

    /// <summary>
    /// If it is true converts all saved DateTimes to UTC. Default value is false.
    /// </summary>
    public bool UseUtcForDateTimes { get; set; } = false;

    /// <summary>
    /// Required for auditing. HttpMethods.IsPost(HttpMethod) || HttpMethods.IsPut(HttpMethod) || HttpMethods.IsDelete(HttpMethod) comparision is done.
    /// </summary>
    public string HttpMethod { get; set; }

    /// <summary>
    /// Logged user's username for auditing.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Audit configuration.
    /// </summary>
    public IAuditConfiguration AuditConfiguration { get; set; }

    /// <summary>
    /// It will be set internally.
    /// </summary>
    public TUserKey? CurrentUserId { get; set; }

    /// <summary>
    /// Soft delete state. Default value is false.
    /// </summary>
    public bool IgnoreSoftDelete { get; set; } = false;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes new instance of <see cref="MilvaDbContextBase{TUser, TUserKey}"/>.
    /// </summary>
    /// <param name="options"></param>
    public MilvaDbContextBase(DbContextOptions options) : base(options)
    {
    }

    #endregion

    /// <summary>
    /// Overrided the OnModelCreating for custom configurations to database.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseTenantId();

        if (UseUtcForDateTimes)
            modelBuilder.UseUtcDateTime();

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Ignores soft delete for next process.
    /// </summary>
    public void IgnoreSoftDeleteForNextProcess() => IgnoreSoftDelete = true;

    /// <summary>
    /// Activate soft delete.
    /// </summary>
    public void ActivateSoftDelete() => IgnoreSoftDelete = false;

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
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        AuditEntites();

        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Overrided the BulkSaveChanges method for soft deleting.
    /// </summary>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual void SaveChangesBulk(BulkConfig bulkConfig = null, CancellationToken cancellationToken = new CancellationToken())
    {
        AuditEntites();

        this.BulkSaveChanges(bulkConfig);
    }

    /// <summary>
    /// Overrided the BulkSaveChangesAsync method for soft deleting.
    /// </summary>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task SaveChangesBulkAsync(BulkConfig bulkConfig = null, CancellationToken cancellationToken = new CancellationToken())
    {
        AuditEntites();

        await this.BulkSaveChangesAsync(bulkConfig, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets requested contents by <paramref name="type"/> DbSet.
    /// If this method gets soft deleted entities please override <see cref="CommonHelper.CreateIsDeletedFalseExpression{TEntity}"/> method your own condition.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public async Task<object> GetRequiredContentsDynamicallyAsync(Type type)
    {
        string propName = $"{type.Name}Langs";

        if (!CommonHelper.PropertyExists(type, propName))
            throw new MilvaDeveloperException($"Type of {type}'s properties doesn't contain '{propName}'.");

        ParameterExpression paramterExpression = Expression.Parameter(type, "i");

        Expression orderByProperty = Expression.Property(paramterExpression, propName);

        Expression<Func<object, object>> predicate = Expression.Lambda<Func<object, object>>(Expression.Convert(Expression.Property(paramterExpression, propName), typeof(object)), paramterExpression);

        var dbSet = GetType().GetMethod("Set").MakeGenericMethod(type).Invoke(this, null);

        var whereMethods = typeof(EntityFrameworkQueryableExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public)
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
    /// If this method gets soft deleted entities please override <see cref="CommonHelper.CreateIsDeletedFalseExpression{TEntity}"/> method your own condition.
    /// </summary>
    /// <returns></returns>
    public async Task<List<TEntity>> GetRequiredContentsAsync<TEntity>(Expression<Func<TEntity, TEntity>> projectionExpression = null) where TEntity : class
        => await Set<TEntity>().Where(CommonHelper.CreateIsDeletedFalseExpression<TEntity>() ?? (entity => true))
                               .IncludeLang(this)
                               .Select(projectionExpression ?? (entity => entity))
                               .ToListAsync()
                               .ConfigureAwait(false);

    /// <summary>
    /// Gets the requested <typeparamref name="TEntity"/>'s property's(<paramref name="propName"/>) max value.
    /// If this method gets soft deleted entities please override <see cref="CommonHelper.CreateIsDeletedFalseExpression{TEntity}"/> method your own condition.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="propName"></param>
    /// <returns></returns>
    public async Task<decimal> GetMaxValueAsync<TEntity>(string propName) where TEntity : class
    {
        var entityType = typeof(TEntity);

        if (!CommonHelper.PropertyExists<TEntity>(propName))
            throw new MilvaDeveloperException($"Type of {entityType}'s properties doesn't contain '{propName}'.");

        ParameterExpression parameterExpression = Expression.Parameter(entityType, "i");
        Expression<Func<TEntity, decimal>> predicate = Expression.Lambda<Func<TEntity, decimal>>(Expression.Convert(Expression.Property(parameterExpression, propName),
                                                                                                                    typeof(decimal)), parameterExpression);

        return await Set<TEntity>().Where(CommonHelper.CreateIsDeletedFalseExpression<TEntity>() ?? (entity => true)).IncludeLang(this).MaxAsync(predicate).ConfigureAwait(false);
    }

    #region Protected Methods

    /// <summary>
    /// Soft delete operation.
    /// </summary>
    /// <param name="entry"></param>
    protected virtual void SoftDelete(EntityEntry entry)
    {
        //Apply soft delete to entry.
        AuditDeletion(entry);

        //If entry is a many side on one to many or many to many relation, skip this entry's navigations for soft delete.
        if (entry.Collections.IsNullOrEmpty())
            return;

        //For changing includes.
        //If navigation entry is a collection entry and included apply soft delete.
        //If navigation entry included apply soft delete.
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
    protected virtual void AuditDeletion(EntityEntry entry)
    {
        if (!entry.Metadata.GetProperties().Any(x => x.Name == EntityPropertyNames.IsDeleted))
            return;

        entry.State = EntityState.Modified;

        //Change "IsDeleted" property value.
        entry.Property(EntityPropertyNames.IsDeleted).CurrentValue = true;
        entry.Property(EntityPropertyNames.IsDeleted).IsModified = true;

        //Change "DeletionDate" property value.
        entry.Property(EntityPropertyNames.DeletionDate).CurrentValue = DateTime.Now;
        entry.Property(EntityPropertyNames.DeletionDate).IsModified = true;

        if (entry.Metadata.GetProperties().Any(prop => prop.Name == EntityPropertyNames.DeleterUserId))
        {
            if (AuditConfiguration.AuditDeleter)
            {
                AuditPerformerUser(entry, EntityPropertyNames.DeleterUserId);
            }
        }
    }

    /// <summary>
    /// Entity auditing by date. 
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="propertyName"></param>
    protected virtual void AuditDate(EntityEntry entry, string propertyName)
    {
        entry.Property(propertyName).CurrentValue = DateTime.Now;
        entry.Property(propertyName).IsModified = true;
    }

    /// <summary>
    /// Entity auditing by performer user.
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="propertyName"></param>
    protected virtual void AuditPerformerUser(EntityEntry entry, string propertyName)
    {
        entry.Property(propertyName).CurrentValue = CurrentUserId;
        entry.Property(propertyName).IsModified = true;
    }

    /// <summary>
    /// Provides auditing entities by <see cref="AuditConfiguration"/>.
    /// If deletion process happens then sets the <see cref="IgnoreSoftDelete"/> variable to true at the end of process.
    /// </summary>
    public virtual void AuditEntites()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (AuditConfiguration.AuditCreationDate)
                    {
                        if (entry.Metadata.GetProperties().Any(prop => prop.Name == EntityPropertyNames.CreationDate))
                            AuditDate(entry, EntityPropertyNames.CreationDate);
                    }
                    if (AuditConfiguration.AuditCreator)
                    {
                        if (entry.Metadata.GetProperties().Any(prop => prop.Name == EntityPropertyNames.CreatorUserId))
                            AuditPerformerUser(entry, EntityPropertyNames.CreatorUserId);
                    }
                    break;
                case EntityState.Modified:
                    if (AuditConfiguration.AuditModificationDate)
                    {
                        if (entry.Metadata.GetProperties().Any(prop => prop.Name == EntityPropertyNames.LastModificationDate))
                            AuditDate(entry, EntityPropertyNames.LastModificationDate);
                    }
                    if (AuditConfiguration.AuditModifier)
                    {
                        if (entry.Metadata.GetProperties().Any(prop => prop.Name == EntityPropertyNames.LastModifierUserId))
                            AuditPerformerUser(entry, EntityPropertyNames.LastModifierUserId);
                    }
                    break;
                case EntityState.Deleted:
                    if (entry.Metadata.GetProperties().Any(prop => prop.Name == EntityPropertyNames.IsDeleted))
                    {
                        if (!IgnoreSoftDelete)
                            SoftDelete(entry);
                    }
                    break;
                default:
                    break;
            }
        }

        IgnoreSoftDelete = false;
    }

    /// <summary>
    /// Convert date times to UTC Zero if entry state is not <see cref="EntityState.Unchanged"/>.
    /// </summary>
    /// <remarks>
    /// 
    /// This will applied when "useUtcForDateTimes" in constructor property is true.
    /// This will applied <see cref="DateTime"/> and nullable <see cref="DateTime"/>.
    /// 
    /// </remarks>
    public virtual void ConvertDateTimesToUtc()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Unchanged)
            {
                foreach (var prop in entry.Metadata.GetProperties())
                {
                    if (prop.ClrType == typeof(DateTime))
                    {
                        var propEntry = (DateTime)entry.Property(prop.Name).CurrentValue;

                        if (propEntry != default)
                            entry.Property(prop.Name).CurrentValue = propEntry.ToUniversalTime();
                    }
                    else if (prop.ClrType == typeof(DateTime?))
                    {
                        var propEntry = (DateTime?)entry.Property(prop.Name).CurrentValue;

                        if (propEntry.HasValue && propEntry.Value != default)
                            entry.Property(prop.Name).CurrentValue = propEntry.Value.ToUniversalTime();
                    }
                }
            }
        }
    }

    #endregion
}

/// <summary>
/// Handles all database operations. Inherits <see cref="IdentityDbContext{TUser, TRole, TKey}"/>
/// </summary>
/// 
/// <remarks>
/// <para> You must register <see cref="IAuditConfiguration"/> in your application startup. </para>
/// <para> If <see cref="IAuditConfiguration"/>'s AuditDeleter, AuditModifier or AuditCreator is true
///        and HttpMethod is POST,PUT or DELETE it will gets performer user in constructor from database.
///        This can affect performance little bit. But you want audit every record easily you must use this :( </para>
/// </remarks>
/// <typeparam name="TUser"></typeparam>
/// <typeparam name="TRole"></typeparam>
/// <typeparam name="TKey"></typeparam>
public abstract class MilvaIdentityDbContextBase<TUser, TRole, TKey> : IdentityDbContext<TUser, TRole, TKey>, IMilvaDbContextBase
    where TUser : IdentityUser<TKey>, IFullAuditable<TKey>
    where TRole : IdentityRole<TKey>, IFullAuditable<TKey>
    where TKey : struct, IEquatable<TKey>
{
    #region Public Properties

    /// <summary>
    /// If it is true converts all saved DateTimes to UTC. Default value is false.
    /// </summary>
    public bool UseUtcForDateTimes { get; set; } = false;

    /// <summary>
    /// Required for auditing. HttpMethods.IsPost(HttpMethod) || HttpMethods.IsPut(HttpMethod) || HttpMethods.IsDelete(HttpMethod) comparision is done.
    /// </summary>
    public string HttpMethod { get; set; }

    /// <summary>
    /// Logged user's username for auditing.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Audit configuration.
    /// </summary>
    public IAuditConfiguration AuditConfiguration { get; set; }

    /// <summary>
    /// It will be set internally.
    /// </summary>
    public TKey? CurrentUserId { get; set; }

    /// <summary>
    /// Soft delete state. Default value is false.
    /// </summary>
    public bool IgnoreSoftDelete { get; set; } = false;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes new instance of <see cref="MilvaIdentityDbContextBase{TUser, TRole, TKey}"/>.
    /// </summary>
    /// <param name="options"></param>
    public MilvaIdentityDbContextBase(DbContextOptions options) : base(options)
    {
    }

    #endregion

    /// <summary>
    /// Overrided the OnModelCreating for custom configurations to database.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseTenantId();

        if (UseUtcForDateTimes)
            modelBuilder.UseUtcDateTime();

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Ignores soft delete for next process.
    /// </summary>
    public void IgnoreSoftDeleteForNextProcess() => IgnoreSoftDelete = true;

    /// <summary>
    /// Activate soft delete.
    /// </summary>
    public void ActivateSoftDelete() => IgnoreSoftDelete = false;

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
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        AuditEntites();

        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Overrided the BulkSaveChanges method for soft deleting.
    /// </summary>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual void SaveChangesBulk(BulkConfig bulkConfig = null, CancellationToken cancellationToken = new CancellationToken())
    {
        AuditEntites();

        this.BulkSaveChanges(bulkConfig);
    }

    /// <summary>
    /// Overrided the BulkSaveChangesAsync method for soft deleting.
    /// </summary>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task SaveChangesBulkAsync(BulkConfig bulkConfig = null, CancellationToken cancellationToken = new CancellationToken())
    {
        AuditEntites();

        await this.BulkSaveChangesAsync(bulkConfig, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets requested contents by <paramref name="type"/> DbSet.
    /// If this method gets soft deleted entities please override <see cref="CommonHelper.CreateIsDeletedFalseExpression{TEntity}"/> method your own condition.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public async Task<object> GetRequiredContentsDynamicallyAsync(Type type)
    {
        string propName = $"{type.Name}Langs";

        if (!CommonHelper.PropertyExists(type, propName))
            throw new MilvaDeveloperException($"Type of {type}'s properties doesn't contain '{propName}'.");

        ParameterExpression paramterExpression = Expression.Parameter(type, "i");

        Expression orderByProperty = Expression.Property(paramterExpression, propName);

        Expression<Func<object, object>> predicate = Expression.Lambda<Func<object, object>>(Expression.Convert(Expression.Property(paramterExpression, propName), typeof(object)), paramterExpression);

        var dbSet = GetType().GetMethod("Set").MakeGenericMethod(type).Invoke(this, null);

        var whereMethods = typeof(EntityFrameworkQueryableExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public)
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
    /// If this method gets soft deleted entities please override <see cref="CommonHelper.CreateIsDeletedFalseExpression{TEntity}"/> method your own condition.
    /// </summary>
    /// <returns></returns>
    public async Task<List<TEntity>> GetRequiredContentsAsync<TEntity>(Expression<Func<TEntity, TEntity>> projectionExpression = null) where TEntity : class
        => await Set<TEntity>().Where(CommonHelper.CreateIsDeletedFalseExpression<TEntity>() ?? (entity => true))
                               .IncludeLang(this)
                               .Select(projectionExpression ?? (entity => entity))
                               .ToListAsync()
                               .ConfigureAwait(false);

    /// <summary>
    /// Gets the requested <typeparamref name="TEntity"/>'s property's(<paramref name="propName"/>) max value.
    /// If this method gets soft deleted entities please override <see cref="CommonHelper.CreateIsDeletedFalseExpression{TEntity}"/> method your own condition.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="propName"></param>
    /// <returns></returns>
    public async Task<decimal> GetMaxValueAsync<TEntity>(string propName) where TEntity : class
    {
        var entityType = typeof(TEntity);

        if (!CommonHelper.PropertyExists<TEntity>(propName))
            throw new MilvaDeveloperException($"Type of {entityType}'s properties doesn't contain '{propName}'.");

        ParameterExpression parameterExpression = Expression.Parameter(entityType, "i");
        Expression<Func<TEntity, decimal>> predicate = Expression.Lambda<Func<TEntity, decimal>>(Expression.Convert(Expression.Property(parameterExpression, propName),
                                                                                                                    typeof(decimal)), parameterExpression);

        return await Set<TEntity>().Where(CommonHelper.CreateIsDeletedFalseExpression<TEntity>() ?? (entity => true)).IncludeLang(this).MaxAsync(predicate).ConfigureAwait(false);
    }

    #region Protected Methods

    /// <summary>
    /// Soft delete operation.
    /// </summary>
    /// <param name="entry"></param>
    protected virtual void SoftDelete(EntityEntry entry)
    {
        //Apply soft delete to entry.
        AuditDeletion(entry);

        //If entry is a many side on one to many or many to many relation, skip this entry's navigations for soft delete.
        if (entry.Collections.IsNullOrEmpty())
            return;

        //For changing includes.
        //If navigation entry is a collection entry and included apply soft delete.
        //If navigation entry included apply soft delete.
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
    protected virtual void AuditDeletion(EntityEntry entry)
    {
        if (!entry.Metadata.GetProperties().Any(x => x.Name == EntityPropertyNames.IsDeleted))
            return;

        entry.State = EntityState.Modified;

        //Change "IsDeleted" property value.
        entry.Property(EntityPropertyNames.IsDeleted).CurrentValue = true;
        entry.Property(EntityPropertyNames.IsDeleted).IsModified = true;

        //Change "DeletionDate" property value.
        entry.Property(EntityPropertyNames.DeletionDate).CurrentValue = DateTime.Now;
        entry.Property(EntityPropertyNames.DeletionDate).IsModified = true;

        if (entry.Metadata.GetProperties().Any(prop => prop.Name == EntityPropertyNames.DeleterUserId))
        {
            if (AuditConfiguration.AuditDeleter)
            {
                AuditPerformerUser(entry, EntityPropertyNames.DeleterUserId);
            }
        }
    }

    /// <summary>
    /// Entity auditing by date. 
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="propertyName"></param>
    protected virtual void AuditDate(EntityEntry entry, string propertyName)
    {
        entry.Property(propertyName).CurrentValue = DateTime.Now;
        entry.Property(propertyName).IsModified = true;
    }

    /// <summary>
    /// Entity auditing by performer user.
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="propertyName"></param>
    protected virtual void AuditPerformerUser(EntityEntry entry, string propertyName)
    {
        if (!string.IsNullOrWhiteSpace(UserName))
            if (CurrentUserId == null)
            {
                if (AuditConfiguration.AuditCreator || AuditConfiguration.AuditModifier || AuditConfiguration.AuditDeleter)
                {
                    if (HttpMethods.IsPost(HttpMethod) || HttpMethods.IsPut(HttpMethod) || HttpMethods.IsDelete(HttpMethod))
                    {
                        CurrentUserId = Users.FirstOrDefaultAsync(i => i.UserName == UserName).Result.Id;
                    }
                }
            }

        entry.Property(propertyName).CurrentValue = CurrentUserId;
        entry.Property(propertyName).IsModified = true;
    }

    /// <summary>
    /// Provides auditing entities by <see cref="AuditConfiguration"/>.
    /// If deletion process happens then sets the <see cref="IgnoreSoftDelete"/> variable to true at the end of process.
    /// </summary>
    public virtual void AuditEntites()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (AuditConfiguration.AuditCreationDate)
                    {
                        if (entry.Metadata.GetProperties().Any(prop => prop.Name == EntityPropertyNames.CreationDate))
                            AuditDate(entry, EntityPropertyNames.CreationDate);
                    }
                    if (AuditConfiguration.AuditCreator)
                    {
                        if (entry.Metadata.GetProperties().Any(prop => prop.Name == EntityPropertyNames.CreatorUserId))
                            AuditPerformerUser(entry, EntityPropertyNames.CreatorUserId);
                    }
                    break;
                case EntityState.Modified:
                    if (AuditConfiguration.AuditModificationDate)
                    {
                        if (entry.Metadata.GetProperties().Any(prop => prop.Name == EntityPropertyNames.LastModificationDate))
                            AuditDate(entry, EntityPropertyNames.LastModificationDate);
                    }
                    if (AuditConfiguration.AuditModifier)
                    {
                        if (entry.Metadata.GetProperties().Any(prop => prop.Name == EntityPropertyNames.LastModifierUserId))
                            AuditPerformerUser(entry, EntityPropertyNames.LastModifierUserId);
                    }
                    break;
                case EntityState.Deleted:
                    if (entry.Metadata.GetProperties().Any(prop => prop.Name == EntityPropertyNames.IsDeleted))
                    {
                        if (!IgnoreSoftDelete)
                            SoftDelete(entry);
                    }
                    break;
                default:
                    break;
            }
        }

        IgnoreSoftDelete = false;
    }

    /// <summary>
    /// Convert date times to UTC Zero if entry state is not <see cref="EntityState.Unchanged"/>.
    /// </summary>
    /// <remarks>
    /// 
    /// This will applied when "useUtcForDateTimes" in constructor property is true.
    /// This will applied <see cref="DateTime"/> and nullable <see cref="DateTime"/>.
    /// 
    /// </remarks>
    public virtual void ConvertDateTimesToUtc()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Unchanged)
            {
                foreach (var prop in entry.Metadata.GetProperties())
                {
                    if (prop.ClrType == typeof(DateTime))
                    {
                        var propEntry = (DateTime)entry.Property(prop.Name).CurrentValue;

                        if (propEntry != default)
                            entry.Property(prop.Name).CurrentValue = propEntry.ToUniversalTime();
                    }
                    else if (prop.ClrType == typeof(DateTime?))
                    {
                        var propEntry = (DateTime?)entry.Property(prop.Name).CurrentValue;

                        if (propEntry.HasValue && propEntry.Value != default)
                            entry.Property(prop.Name).CurrentValue = propEntry.Value.ToUniversalTime();
                    }
                }
            }
        }
    }

    #endregion
}

/// <summary>
/// Handles all database operations. Inherits <see cref="MilvaDbContextBase{TUser, TKey}"/>
/// </summary>
/// 
/// <remarks>
/// <para> You must register <see cref="IAuditConfiguration"/> in your application startup. </para>
/// <para> If <see cref="IAuditConfiguration"/>'s AuditDeleter, AuditModifier or AuditCreator is true
///        and HttpMethod is POST,PUT or DELETE it will gets performer user in constructor from database.
///        This can affect performance little bit. But you want audit every record easily you must use this :( </para>
/// </remarks>
/// <typeparam name="TUser"></typeparam>
/// <typeparam name="TKey"></typeparam>
public abstract class MilvaDbContext<TUser, TKey> : MilvaDbContextBase<TUser, TKey>
    where TUser : class, IFullAuditable<TKey>, IFullAuditableWithCustomUser<TUser, TKey, TKey>
    where TKey : struct, IEquatable<TKey>
{
    #region Constructors

    /// <summary>
    /// Cunstructor of <see cref="MilvaDbContext{TUser, TKey}"></see>.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="httpContextAccessor"></param>
    /// <param name="auditConfiguration"></param>
    /// <param name="useUtcForDateTimes"></param>
    public MilvaDbContext(DbContextOptions options,
                          IHttpContextAccessor httpContextAccessor,
                          IAuditConfiguration auditConfiguration,
                          bool useUtcForDateTimes = false) : base(options)
    {
        UseUtcForDateTimes = useUtcForDateTimes;
        HttpMethod = httpContextAccessor?.HttpContext?.Request?.Method;
        UserName = httpContextAccessor?.HttpContext?.User?.Identity?.Name;
        AuditConfiguration = auditConfiguration;
    }

    #endregion

    /// <summary>
    /// Overrided the OnModelCreating for custom configurations to database.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        #region TUser.Set_ForeignKeys & TRole.Set_ForeignKeys

        modelBuilder.Entity<TUser>()
                    .HasOne(p => p.DeleterUser)
                    .WithMany()
                    .HasForeignKey(p => p.DeleterUserId)
                    .IsRequired(false);

        modelBuilder.Entity<TUser>()
                    .HasOne(p => p.CreatorUser)
                    .WithMany()
                    .HasForeignKey(p => p.CreatorUserId)
                    .IsRequired(false);

        modelBuilder.Entity<TUser>()
                    .HasOne(p => p.LastModifierUser)
                    .WithMany()
                    .HasForeignKey(p => p.LastModifierUserId)
                    .IsRequired(false);

        #endregion

        base.OnModelCreating(modelBuilder);
    }
}

/// <summary>
/// Handles all database operations. Inherits <see cref="MilvaIdentityDbContextBase{TUser, TRole, TKey}"/>
/// </summary>
/// 
/// <remarks>
/// <para> You must register <see cref="IAuditConfiguration"/> in your application startup. </para>
/// <para> If <see cref="IAuditConfiguration"/>'s AuditDeleter, AuditModifier or AuditCreator is true
///        and HttpMethod is POST,PUT or DELETE it will gets performer user in constructor from database.
///        This can affect performance little bit. But you want audit every record easily you must use this :( </para>
/// </remarks>
/// <typeparam name="TUser"></typeparam>
/// <typeparam name="TRole"></typeparam>
/// <typeparam name="TKey"></typeparam>
public abstract class MilvaIdentityDbContext<TUser, TRole, TKey> : MilvaIdentityDbContextBase<TUser, TRole, TKey>
    where TUser : IdentityUser<TKey>, IFullAuditable<TKey>, IFullAuditable<TUser, TKey, TKey>
    where TRole : IdentityRole<TKey>, IFullAuditable<TKey>, IFullAuditable<TUser, TKey, TKey>
    where TKey : struct, IEquatable<TKey>
{
    #region Constructors

    /// <summary>
    /// Cunstructor of <see cref="MilvaIdentityDbContext{TUser, TRole, TKey}"></see>.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="httpContextAccessor"></param>
    /// <param name="auditConfiguration"></param>
    /// <param name="useUtcForDateTimes"></param>
    public MilvaIdentityDbContext(DbContextOptions options,
                                  IHttpContextAccessor httpContextAccessor,
                                  IAuditConfiguration auditConfiguration,
                                  bool useUtcForDateTimes = false) : base(options)
    {
        UseUtcForDateTimes = useUtcForDateTimes;
        HttpMethod = httpContextAccessor?.HttpContext?.Request?.Method;
        UserName = httpContextAccessor?.HttpContext?.User?.Identity?.Name;
        AuditConfiguration = auditConfiguration;
    }

    #endregion

    /// <summary>
    /// Overrided the OnModelCreating for custom configurations to database.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        #region TUser.Set_ForeignKeys & TRole.Set_ForeignKeys

        modelBuilder.Entity<TUser>()
                    .HasOne(p => p.DeleterUser)
                    .WithMany()
                    .HasForeignKey(p => p.DeleterUserId)
                    .IsRequired(false);

        modelBuilder.Entity<TUser>()
                    .HasOne(p => p.CreatorUser)
                    .WithMany()
                    .HasForeignKey(p => p.CreatorUserId)
                    .IsRequired(false);

        modelBuilder.Entity<TUser>()
                    .HasOne(p => p.LastModifierUser)
                    .WithMany()
                    .HasForeignKey(p => p.LastModifierUserId)
                    .IsRequired(false);

        modelBuilder.Entity<TRole>()
                    .HasOne(p => p.DeleterUser)
                    .WithMany()
                    .HasForeignKey(p => p.DeleterUserId)
                    .IsRequired(false);

        modelBuilder.Entity<TRole>()
                    .HasOne(p => p.CreatorUser)
                    .WithMany()
                    .HasForeignKey(p => p.CreatorUserId)
                    .IsRequired(false);

        modelBuilder.Entity<TRole>()
                    .HasOne(p => p.LastModifierUser)
                    .WithMany()
                    .HasForeignKey(p => p.LastModifierUserId)
                    .IsRequired(false);

        #endregion

        base.OnModelCreating(modelBuilder);
    }
}

/// <summary>
/// Handles all database operations. Inherits <see cref="MilvaDbContextBase{TUser, TKey}"/>
/// </summary>
/// 
/// <remarks>
/// <para> You must register <see cref="IAuditConfiguration"/> in your application startup. </para>
/// <para> If <see cref="IAuditConfiguration"/>'s AuditDeleter, AuditModifier or AuditCreator is true
///        and HttpMethod is POST,PUT or DELETE it will gets performer user in constructor from database.
///        This can affect performance little bit. But you want audit every record easily you must use this :( </para>
/// </remarks>
/// <typeparam name="TUser"></typeparam>
/// <typeparam name="TKey"></typeparam>
public abstract class MilvaPooledDbContext<TUser, TKey> : MilvaDbContextBase<TUser, TKey>
    where TUser : class, IFullAuditable<TKey>, IFullAuditableWithCustomUser<TUser, TKey, TKey>
    where TKey : struct, IEquatable<TKey>
{
    #region Constructors

    /// <summary>
    /// Cunstructor of <see cref="MilvaPooledDbContext{TUser, TKey}"></see>.
    /// </summary>
    /// <param name="options"></param>
    public MilvaPooledDbContext(DbContextOptions options) : base(options)
    {
    }

    #endregion


    /// <summary>
    /// Overrided the OnModelCreating for custom configurations to database.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        #region TUser.Set_ForeignKeys & TRole.Set_ForeignKeys

        modelBuilder.Entity<TUser>()
                    .HasOne(p => p.DeleterUser)
                    .WithMany()
                    .HasForeignKey(p => p.DeleterUserId)
                    .IsRequired(false);

        modelBuilder.Entity<TUser>()
                    .HasOne(p => p.CreatorUser)
                    .WithMany()
                    .HasForeignKey(p => p.CreatorUserId)
                    .IsRequired(false);

        modelBuilder.Entity<TUser>()
                    .HasOne(p => p.LastModifierUser)
                    .WithMany()
                    .HasForeignKey(p => p.LastModifierUserId)
                    .IsRequired(false);

        #endregion

        base.OnModelCreating(modelBuilder);
    }
}

/// <summary>
/// Handles all database operations. Inherits <see cref="MilvaIdentityDbContextBase{TUser, TRole, TKey}"/>
/// </summary>
/// 
/// <remarks>
/// <para> You must register <see cref="IAuditConfiguration"/> in your application startup. </para>
/// <para> If <see cref="IAuditConfiguration"/>'s AuditDeleter, AuditModifier or AuditCreator is true
///        and HttpMethod is POST,PUT or DELETE it will gets performer user in constructor from database.
///        This can affect performance little bit. But you want audit every record easily you must use this :( </para>
/// </remarks>
/// <typeparam name="TUser"></typeparam>
/// <typeparam name="TRole"></typeparam>
/// <typeparam name="TKey"></typeparam>
public abstract class MilvaPooledIdentityDbContext<TUser, TRole, TKey> : MilvaIdentityDbContextBase<TUser, TRole, TKey>
    where TUser : IdentityUser<TKey>, IFullAuditable<TKey>, IFullAuditable<TUser, TKey, TKey>
    where TRole : IdentityRole<TKey>, IFullAuditable<TKey>, IFullAuditable<TUser, TKey, TKey>
    where TKey : struct, IEquatable<TKey>
{
    #region Constructors

    /// <summary>
    /// Cunstructor of <see cref="MilvaPooledIdentityDbContext{TUser, TRole, TKey}"></see>.
    /// </summary>
    /// <param name="options"></param>
    public MilvaPooledIdentityDbContext(DbContextOptions options) : base(options)
    {
    }

    #endregion

    /// <summary>
    /// Overrided the OnModelCreating for custom configurations to database.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        #region TUser.Set_ForeignKeys & TRole.Set_ForeignKeys

        modelBuilder.Entity<TUser>()
                    .HasOne(p => p.DeleterUser)
                    .WithMany()
                    .HasForeignKey(p => p.DeleterUserId)
                    .IsRequired(false);

        modelBuilder.Entity<TUser>()
                    .HasOne(p => p.CreatorUser)
                    .WithMany()
                    .HasForeignKey(p => p.CreatorUserId)
                    .IsRequired(false);

        modelBuilder.Entity<TUser>()
                    .HasOne(p => p.LastModifierUser)
                    .WithMany()
                    .HasForeignKey(p => p.LastModifierUserId)
                    .IsRequired(false);

        modelBuilder.Entity<TRole>()
                    .HasOne(p => p.DeleterUser)
                    .WithMany()
                    .HasForeignKey(p => p.DeleterUserId)
                    .IsRequired(false);

        modelBuilder.Entity<TRole>()
                    .HasOne(p => p.CreatorUser)
                    .WithMany()
                    .HasForeignKey(p => p.CreatorUserId)
                    .IsRequired(false);

        modelBuilder.Entity<TRole>()
                    .HasOne(p => p.LastModifierUser)
                    .WithMany()
                    .HasForeignKey(p => p.LastModifierUserId)
                    .IsRequired(false);

        #endregion

        base.OnModelCreating(modelBuilder);
    }
}



