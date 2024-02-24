﻿using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Milvasoft.Core;
using Milvasoft.Core.EntityBases.Abstract;
using Milvasoft.Core.Exceptions;
using Milvasoft.Core.Extensions;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.DataAccess.EfCore.Configuration;
using Milvasoft.DataAccess.EfCore.RepositoryBase.Abstract;
using Milvasoft.DataAccess.EfCore.Utils.Enums;
using System.Linq.Expressions;
using System.Reflection;

namespace Milvasoft.DataAccess.EfCore.DbContextBase;

/// <summary>
/// Handles all database operations with new features like soft deletion.
/// </summary>
/// <param name="options"></param>
public abstract class MilvaDbContextBase(DbContextOptions options) : DbContext(options), IMilvaDbContextBase
{
    /// <summary>
    /// Milva specific db context configuration.
    /// </summary>
    protected IDataAccessConfiguration _dbContextConfiguration;

    /// <summary>
    /// Default prop for soft delete.
    /// </summary>
    protected SoftDeletionState _currentSoftDeleteState;

    /// <summary>
    /// Default prop for datetime configuration.
    /// </summary>
    protected bool _useUtcForDateTimes = false;

    /// <summary>
    /// Initializes new instance.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="dbContextConfiguration"></param>
    public MilvaDbContextBase(DbContextOptions options, DataAccessConfiguration dbContextConfiguration) : this(options)
    {
        SetDataAccessConfiguration(dbContextConfiguration);
    }

    /// <summary>
    /// Overrided the OnModelCreating for custom configurations to database.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseTenantId();

        if (_useUtcForDateTimes)
            modelBuilder.UseUtcDateTime();

        base.OnModelCreating(modelBuilder);
    }

    #region Configuration

    /// <summary>
    /// Sets milva db context specific configuration.
    /// </summary>
    /// <param name="dbContextConfiguration"></param>
    public void SetDataAccessConfiguration(IDataAccessConfiguration dbContextConfiguration)
    {
        _dbContextConfiguration = dbContextConfiguration;
        _currentSoftDeleteState = dbContextConfiguration.DbContext.DefaultSoftDeletionState;
        _useUtcForDateTimes = dbContextConfiguration.DbContext.UseUtcForDateTimes;
    }

    /// <summary>
    /// Gets milva db context configuration object.
    /// </summary>
    /// <returns></returns>
    public IDataAccessConfiguration GetDataAccessConfiguration() => _dbContextConfiguration;

    /// <summary>
    /// Ignores soft delete for next process.
    /// </summary>
    public void IgnoreSoftDeleteForNextProcess() => _currentSoftDeleteState = SoftDeletionState.Passive;

    /// <summary>
    /// Activate soft delete.
    /// </summary>
    public void ActivateSoftDelete() => _currentSoftDeleteState = SoftDeletionState.Active;

    /// <summary>
    /// Sets soft deletion state to default state in <see cref="DataAccessConfiguration"/>.
    /// </summary>
    public void SetSoftDeleteStateToDefault() => _currentSoftDeleteState = _dbContextConfiguration.DbContext.DefaultSoftDeletionState;

    /// <summary>
    /// Gets current soft deletion state.
    /// </summary>
    /// <returns></returns>
    public SoftDeletionState GetCurrentSoftDeletionState() => _currentSoftDeleteState;

    #endregion

    #region SaveChanges Overrides

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

    #endregion

    #region Auditing

    /// <summary>
    /// Provides auditing entities by AuditConfiguration.
    /// If deletion process happens then sets the IgnoreSoftDelete variable to true at the end of process.
    /// </summary>
    protected internal virtual void AuditEntites()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Added:

                    AuditDate(entry, EntityPropertyNames.CreationDate);
                    AuditPerformerUser(entry, EntityPropertyNames.CreatorUserName);

                    break;
                case EntityState.Modified:

                    AuditDate(entry, EntityPropertyNames.LastModificationDate);
                    AuditPerformerUser(entry, EntityPropertyNames.LastModifierUserName);

                    break;
                case EntityState.Deleted:

                    SoftDelete(entry);

                    break;
                case EntityState.Detached:
                    break;
                case EntityState.Unchanged:
                    break;
                default:
                    break;
            }
        }

        _currentSoftDeleteState = _dbContextConfiguration.DbContext.DefaultSoftDeletionState;
    }

    /// <summary>
    /// Soft delete operation.
    /// </summary>
    /// <param name="entry"></param>
    protected internal virtual void SoftDelete(EntityEntry entry)
    {
        if (_currentSoftDeleteState == SoftDeletionState.Passive)
            return;

        //Apply soft delete to entry.
        AuditDeletion(entry);

        //If entry is a many side on one to many or many to many relation, skip this entry's navigations for soft delete.
        if (entry.Collections.IsNullOrEmpty())
            return;

        //For changing includes.
        //If navigation entry is a collection entry and included apply soft delete.
        //If navigation entry included apply soft delete.
        foreach (var navigationEntry in entry.Navigations)
        {
            if (navigationEntry is CollectionEntry collectionEntry && collectionEntry?.CurrentValue != null)
            {
                foreach (var dependentEntry in collectionEntry.CurrentValue)
                    AuditDeletion(Entry(dependentEntry));
            }
            else if (navigationEntry?.CurrentValue != null)
            {
                AuditDeletion(Entry(navigationEntry.CurrentValue));
            }
        }
    }

    /// <summary>
    /// Entity auditing for delete. 
    /// </summary>
    /// <param name="entry"></param>
    protected internal virtual void AuditDeletion(EntityEntry entry)
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

        AuditPerformerUser(entry, EntityPropertyNames.DeleterUserName);
    }

    /// <summary>
    /// Entity auditing by date. 
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="propertyName"></param>
    protected internal virtual void AuditDate(EntityEntry entry, string propertyName)
    {
        if (_dbContextConfiguration.Auditing.IsAuditingPerformTime())
        {
            if (entry.Metadata.GetProperties().Any(prop => prop.Name == propertyName))
            {
                entry.Property(propertyName).CurrentValue = DateTime.Now;
                entry.Property(propertyName).IsModified = true;
            }
        }
    }

    /// <summary>
    /// Entity auditing by performer user.
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="propertyName"></param>
    protected internal virtual void AuditPerformerUser(EntityEntry entry, string propertyName)
    {
        if (_dbContextConfiguration.Auditing.IsAuditingPerformer())
        {
            var currentUserName = _dbContextConfiguration.DbContext.GetCurrentUserNameDelegate.Invoke();

            if (!string.IsNullOrWhiteSpace(currentUserName))
            {
                if (entry.Metadata.GetProperties().Any(prop => prop.Name == propertyName))
                {
                    entry.Property(propertyName).CurrentValue = currentUserName;
                    entry.Property(propertyName).IsModified = true;
                }
            }
        }
    }

    #endregion

    #region Dynamic Fetch

    /// <summary>
    /// Gets requested contents by <paramref name="type"/> DbSet.
    /// If this method gets soft deleted entities please override <see cref="CommonHelper.CreateIsDeletedFalseExpression{TEntity}"/> method your own condition.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public async Task<object> GetRequiredContentsDynamicallyAsync(Type type)
    {
        var propName = $"{type.Name}Langs";

        if (!type.PropertyExists(propName))
            throw new MilvaDeveloperException($"Type of {type}'s properties doesn't contain '{propName}'.");

        ParameterExpression paramterExpression = Expression.Parameter(type, "i");

        Expression orderByProperty = Expression.Property(paramterExpression, propName);

        var predicate = Expression.Lambda<Func<object, object>>(Expression.Convert(Expression.Property(paramterExpression, propName), typeof(object)), paramterExpression);

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
    public async Task<decimal> GetMaxValueFromDbAsync<TEntity>(string propName) where TEntity : class
    {
        var entityType = typeof(TEntity);

        entityType.ThrowIfPropertyNotExists(propName);

        ParameterExpression parameterExpression = Expression.Parameter(entityType, "i");

        var predicate = Expression.Lambda<Func<TEntity, decimal>>(Expression.Convert(Expression.Property(parameterExpression, propName), typeof(decimal)), parameterExpression);

        return await Set<TEntity>().Where(CommonHelper.CreateIsDeletedFalseExpression<TEntity>() ?? (entity => true)).IncludeLang(this).MaxAsync(predicate).ConfigureAwait(false);
    }

    #endregion
}

/// <summary>
/// Handles all database operations. Inherits <see cref="MilvaDbContextBase"/>
/// </summary>
/// 
/// <remarks>
/// <para> You must register <see cref="IDataAccessConfiguration"/> in your application startup. </para>
/// <para> If <see cref="IDataAccessConfiguration"/>'s AuditDeleter, AuditModifier or AuditCreator is true
///        and HttpMethod is POST,PUT or DELETE it will gets performer user in constructor from database.
///        This can affect performance little bit. But you want audit every record easily you must use this :( </para>
/// </remarks>
/// <remarks>
/// Initializes new instance.
/// </remarks>
/// <param name="options"></param>
/// <param name="dbContextConfiguration"></param>
public abstract class MilvaDbContext(DbContextOptions options, DataAccessConfiguration dbContextConfiguration) : MilvaDbContextBase(options, dbContextConfiguration)
{
}

/// <summary>
/// Handles all database operations. Inherits <see cref="MilvaDbContextBase"/>
/// </summary>
/// 
/// <remarks>
/// <para> You must register <see cref="IDataAccessConfiguration"/> in your application startup. </para>
/// <para> If <see cref="IDataAccessConfiguration"/>'s AuditDeleter, AuditModifier or AuditCreator is true
///        and HttpMethod is POST,PUT or DELETE it will gets performer user in constructor from database.
///        This can affect performance little bit. But you want audit every record easily you must use this :( </para>
/// </remarks>
public abstract class MilvaPooledDbContext(DbContextOptions options) : MilvaDbContextBase(options)
{
}