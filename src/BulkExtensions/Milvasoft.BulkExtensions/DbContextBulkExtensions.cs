using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Milvasoft.BulkExtensions.Internal;

namespace Milvasoft.BulkExtensions;

/// <summary>
/// Bulk operations on a <see cref="DbContext"/>.
/// </summary>
/// <remarks>
/// <para>
/// Operations write directly to the database and do not use or change the change tracker; only <c>BulkSaveChanges</c> saves tracked changes.
/// <c>SaveChanges</c> interceptors and events do not run, and only <c>BulkSaveChanges</c> checks concurrency tokens.
/// Rows are matched with SQL <c>=</c>, so an entity with a NULL match value matches no row.
/// </para>
/// <para>
/// Each call runs in one transaction. When the context has no transaction, one is started and committed (inside the execution strategy of the
/// context, so retry on failure works). When the context already has a transaction, or a <see cref="System.Transactions.TransactionScope"/> is active,
/// the operation joins it.
/// </para>
/// <para>
/// Entities may be of the given type or of types derived from it (TPH and TPC inheritance). Owned types stored in the owner's table and complex
/// types are written as part of their owner. Types mapped to several tables (TPT, entity splitting) and JSON columns are not supported.
/// </para>
/// <para>
/// Properties that EF Core generates on the client, such as <see cref="Guid"/> keys, are generated for entities that leave them unset.
/// Database generated values (identity keys, default values) are used for entities that leave them at their default value.
/// </para>
/// </remarks>
public static class DbContextBulkExtensions
{
    #region Insert

    /// <summary>
    /// Inserts <paramref name="entities"/>.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="context">The context.</param>
    /// <param name="entities">Entities to insert.</param>
    /// <param name="config">Operation options.</param>
    /// <returns>Number of inserted rows.</returns>
    public static int BulkInsert<T>(this DbContext context, IEnumerable<T> entities, BulkConfig config = null) where T : class
        => RunPerTable(context, entities, config, static (executor, source, async, ct) => executor.InsertAsync(source, async, ct), false, default).GetAwaiter().GetResult();

    /// <inheritdoc cref="BulkInsert{T}(DbContext, IEnumerable{T}, BulkConfig)"/>
    /// <param name="context">The context.</param>
    /// <param name="entities">Entities to insert.</param>
    /// <param name="configure">Configures operation options.</param>
    public static int BulkInsert<T>(this DbContext context, IEnumerable<T> entities, Action<BulkConfig> configure) where T : class
        => context.BulkInsert(entities, CreateConfig(configure));

    /// <inheritdoc cref="BulkInsert{T}(DbContext, IEnumerable{T}, BulkConfig)"/>
    /// <param name="context">The context.</param>
    /// <param name="entities">Entities to insert.</param>
    /// <param name="config">Operation options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static Task<int> BulkInsertAsync<T>(this DbContext context, IEnumerable<T> entities, BulkConfig config = null, CancellationToken cancellationToken = default) where T : class
        => RunPerTable(context, entities, config, static (executor, source, async, ct) => executor.InsertAsync(source, async, ct), true, cancellationToken);

    /// <inheritdoc cref="BulkInsert{T}(DbContext, IEnumerable{T}, BulkConfig)"/>
    /// <param name="context">The context.</param>
    /// <param name="entities">Entities to insert.</param>
    /// <param name="configure">Configures operation options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static Task<int> BulkInsertAsync<T>(this DbContext context, IEnumerable<T> entities, Action<BulkConfig> configure, CancellationToken cancellationToken = default) where T : class
        => context.BulkInsertAsync(entities, CreateConfig(configure), cancellationToken);

    #endregion

    #region Update

    /// <summary>
    /// Updates the rows that match <paramref name="entities"/> by primary key, or by <see cref="BulkConfig.UpdateByProperties"/>.
    /// Entities without a matching row are ignored.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="context">The context.</param>
    /// <param name="entities">Entities with new values. Two entities may not have the same match values.</param>
    /// <param name="config">Operation options.</param>
    /// <returns>Number of updated rows.</returns>
    public static int BulkUpdate<T>(this DbContext context, IEnumerable<T> entities, BulkConfig config = null) where T : class
        => RunPerTable(context, entities, config, static (executor, source, async, ct) => executor.UpdateAsync(source, async, ct), false, default).GetAwaiter().GetResult();

    /// <inheritdoc cref="BulkUpdate{T}(DbContext, IEnumerable{T}, BulkConfig)"/>
    /// <param name="context">The context.</param>
    /// <param name="entities">Entities with new values.</param>
    /// <param name="configure">Configures operation options.</param>
    public static int BulkUpdate<T>(this DbContext context, IEnumerable<T> entities, Action<BulkConfig> configure) where T : class
        => context.BulkUpdate(entities, CreateConfig(configure));

    /// <inheritdoc cref="BulkUpdate{T}(DbContext, IEnumerable{T}, BulkConfig)"/>
    /// <param name="context">The context.</param>
    /// <param name="entities">Entities with new values.</param>
    /// <param name="config">Operation options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static Task<int> BulkUpdateAsync<T>(this DbContext context, IEnumerable<T> entities, BulkConfig config = null, CancellationToken cancellationToken = default) where T : class
        => RunPerTable(context, entities, config, static (executor, source, async, ct) => executor.UpdateAsync(source, async, ct), true, cancellationToken);

    /// <inheritdoc cref="BulkUpdate{T}(DbContext, IEnumerable{T}, BulkConfig)"/>
    /// <param name="context">The context.</param>
    /// <param name="entities">Entities with new values.</param>
    /// <param name="configure">Configures operation options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static Task<int> BulkUpdateAsync<T>(this DbContext context, IEnumerable<T> entities, Action<BulkConfig> configure, CancellationToken cancellationToken = default) where T : class
        => context.BulkUpdateAsync(entities, CreateConfig(configure), cancellationToken);

    #endregion

    #region Delete

    /// <summary>
    /// Deletes the rows that match <paramref name="entities"/> by primary key, or by <see cref="BulkConfig.UpdateByProperties"/>.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="context">The context.</param>
    /// <param name="entities">Entities to delete. Only their key (or match) values are used.</param>
    /// <param name="config">Operation options.</param>
    /// <returns>Number of deleted rows.</returns>
    public static int BulkDelete<T>(this DbContext context, IEnumerable<T> entities, BulkConfig config = null) where T : class
        => RunPerTable(context, entities, config, static (executor, source, async, ct) => executor.DeleteAsync(source, async, ct), false, default).GetAwaiter().GetResult();

    /// <inheritdoc cref="BulkDelete{T}(DbContext, IEnumerable{T}, BulkConfig)"/>
    /// <param name="context">The context.</param>
    /// <param name="entities">Entities to delete.</param>
    /// <param name="configure">Configures operation options.</param>
    public static int BulkDelete<T>(this DbContext context, IEnumerable<T> entities, Action<BulkConfig> configure) where T : class
        => context.BulkDelete(entities, CreateConfig(configure));

    /// <inheritdoc cref="BulkDelete{T}(DbContext, IEnumerable{T}, BulkConfig)"/>
    /// <param name="context">The context.</param>
    /// <param name="entities">Entities to delete.</param>
    /// <param name="config">Operation options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static Task<int> BulkDeleteAsync<T>(this DbContext context, IEnumerable<T> entities, BulkConfig config = null, CancellationToken cancellationToken = default) where T : class
        => RunPerTable(context, entities, config, static (executor, source, async, ct) => executor.DeleteAsync(source, async, ct), true, cancellationToken);

    /// <inheritdoc cref="BulkDelete{T}(DbContext, IEnumerable{T}, BulkConfig)"/>
    /// <param name="context">The context.</param>
    /// <param name="entities">Entities to delete.</param>
    /// <param name="configure">Configures operation options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static Task<int> BulkDeleteAsync<T>(this DbContext context, IEnumerable<T> entities, Action<BulkConfig> configure, CancellationToken cancellationToken = default) where T : class
        => context.BulkDeleteAsync(entities, CreateConfig(configure), cancellationToken);

    #endregion

    #region Insert or update

    /// <summary>
    /// Inserts <paramref name="entities"/>, and updates the rows that already exist (upsert).
    /// Rows are matched by primary key, or by <see cref="BulkConfig.UpdateByProperties"/>, which must have a unique constraint or unique index.
    /// Entities that leave a database generated key at its default value are always inserted.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="context">The context.</param>
    /// <param name="entities">Entities to insert or update. Two entities may not have the same match values.</param>
    /// <param name="config">Operation options.</param>
    /// <returns>Number of inserted and updated rows.</returns>
    public static int BulkInsertOrUpdate<T>(this DbContext context, IEnumerable<T> entities, BulkConfig config = null) where T : class
        => RunPerTable(context, entities, config, static (executor, source, async, ct) => executor.UpsertAsync(source, async, ct), false, default).GetAwaiter().GetResult();

    /// <inheritdoc cref="BulkInsertOrUpdate{T}(DbContext, IEnumerable{T}, BulkConfig)"/>
    /// <param name="context">The context.</param>
    /// <param name="entities">Entities to insert or update.</param>
    /// <param name="configure">Configures operation options.</param>
    public static int BulkInsertOrUpdate<T>(this DbContext context, IEnumerable<T> entities, Action<BulkConfig> configure) where T : class
        => context.BulkInsertOrUpdate(entities, CreateConfig(configure));

    /// <inheritdoc cref="BulkInsertOrUpdate{T}(DbContext, IEnumerable{T}, BulkConfig)"/>
    /// <param name="context">The context.</param>
    /// <param name="entities">Entities to insert or update.</param>
    /// <param name="config">Operation options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static Task<int> BulkInsertOrUpdateAsync<T>(this DbContext context, IEnumerable<T> entities, BulkConfig config = null, CancellationToken cancellationToken = default) where T : class
        => RunPerTable(context, entities, config, static (executor, source, async, ct) => executor.UpsertAsync(source, async, ct), true, cancellationToken);

    /// <inheritdoc cref="BulkInsertOrUpdate{T}(DbContext, IEnumerable{T}, BulkConfig)"/>
    /// <param name="context">The context.</param>
    /// <param name="entities">Entities to insert or update.</param>
    /// <param name="configure">Configures operation options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static Task<int> BulkInsertOrUpdateAsync<T>(this DbContext context, IEnumerable<T> entities, Action<BulkConfig> configure, CancellationToken cancellationToken = default) where T : class
        => context.BulkInsertOrUpdateAsync(entities, CreateConfig(configure), cancellationToken);

    #endregion

    #region Read

    /// <summary>
    /// Loads the rows that match <paramref name="entities"/> by primary key, or by <see cref="BulkConfig.UpdateByProperties"/>.
    /// The query runs through EF Core, so global query filters and the tracking behavior of the context apply.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="context">The context.</param>
    /// <param name="entities">Entities with the key (or match) values to look for. Other properties are ignored.</param>
    /// <param name="config">Operation options. Only <see cref="BulkConfig.UpdateByProperties"/> and <see cref="BulkConfig.CommandTimeout"/> are used.</param>
    /// <returns>The matching entities loaded from the database, in no particular order.</returns>
    public static List<T> BulkRead<T>(this DbContext context, IEnumerable<T> entities, BulkConfig config = null) where T : class
        => ReadAsync(context, entities, config, false, default).GetAwaiter().GetResult();

    /// <inheritdoc cref="BulkRead{T}(DbContext, IEnumerable{T}, BulkConfig)"/>
    /// <param name="context">The context.</param>
    /// <param name="entities">Entities with the key (or match) values to look for.</param>
    /// <param name="configure">Configures operation options.</param>
    public static List<T> BulkRead<T>(this DbContext context, IEnumerable<T> entities, Action<BulkConfig> configure) where T : class
        => context.BulkRead(entities, CreateConfig(configure));

    /// <inheritdoc cref="BulkRead{T}(DbContext, IEnumerable{T}, BulkConfig)"/>
    /// <param name="context">The context.</param>
    /// <param name="entities">Entities with the key (or match) values to look for.</param>
    /// <param name="config">Operation options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static Task<List<T>> BulkReadAsync<T>(this DbContext context, IEnumerable<T> entities, BulkConfig config = null, CancellationToken cancellationToken = default) where T : class
        => ReadAsync(context, entities, config, true, cancellationToken);

    /// <inheritdoc cref="BulkRead{T}(DbContext, IEnumerable{T}, BulkConfig)"/>
    /// <param name="context">The context.</param>
    /// <param name="entities">Entities with the key (or match) values to look for.</param>
    /// <param name="configure">Configures operation options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static Task<List<T>> BulkReadAsync<T>(this DbContext context, IEnumerable<T> entities, Action<BulkConfig> configure, CancellationToken cancellationToken = default) where T : class
        => context.BulkReadAsync(entities, CreateConfig(configure), cancellationToken);

    #endregion

    #region Truncate

    /// <summary>
    /// Removes every row of the table of <typeparamref name="T"/>. With TPH inheritance this removes the rows of every type in the hierarchy.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="context">The context.</param>
    public static void Truncate<T>(this DbContext context) where T : class
        => TruncateAsync<T>(context, false, default).GetAwaiter().GetResult();

    /// <inheritdoc cref="Truncate{T}(DbContext)"/>
    /// <param name="context">The context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static Task TruncateAsync<T>(this DbContext context, CancellationToken cancellationToken = default) where T : class
        => TruncateAsync<T>(context, true, cancellationToken);

    #endregion

    #region Save changes

    /// <summary>
    /// Saves the changes tracked by the context with bulk operations, then accepts them as <c>SaveChanges</c> does.
    /// Database generated values, including keys of new entities, are written to the entities.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Only modified columns are updated. Concurrency tokens are compared with their original values, and an updated or deleted row
    /// that is not found throws <see cref="DbUpdateConcurrencyException"/>; nothing is saved or accepted then.
    /// </para>
    /// <para>
    /// Tables are written in foreign key order: all inserts, then updates, then deletes. Saves that need another order (reusing the key or
    /// a unique value of a deleted row, swapping unique values) and tables that refer to each other in a cycle are not supported;
    /// use <c>SaveChanges</c> for them. Only <see cref="BulkConfig.CommandTimeout"/> of <paramref name="config"/> is used.
    /// </para>
    /// </remarks>
    /// <param name="context">The context.</param>
    /// <param name="config">Operation options.</param>
    /// <returns>Number of inserted, updated and deleted rows.</returns>
    public static int BulkSaveChanges(this DbContext context, BulkConfig config = null)
    {
        ArgumentNullException.ThrowIfNull(context);

        return BulkSaveChangesRunner.SaveAsync(context, config, false, default).GetAwaiter().GetResult();
    }

    /// <inheritdoc cref="BulkSaveChanges(DbContext, BulkConfig)"/>
    /// <param name="context">The context.</param>
    /// <param name="config">Operation options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static Task<int> BulkSaveChangesAsync(this DbContext context, BulkConfig config = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        return BulkSaveChangesRunner.SaveAsync(context, config, true, cancellationToken);
    }

    #endregion

    #region Helpers

    private static BulkConfig CreateConfig(Action<BulkConfig> configure)
    {
        var config = new BulkConfig();

        configure?.Invoke(config);

        return config;
    }

    /// <summary>
    /// Groups entities by table and runs <paramref name="operation"/> for each table in one transaction.
    /// </summary>
    private static async Task<int> RunPerTable<T>(DbContext context,
                                                  IEnumerable<T> entities,
                                                  BulkConfig config,
                                                  Func<BulkExecutor, RowSource, bool, CancellationToken, Task<int>> operation,
                                                  bool async,
                                                  CancellationToken cancellationToken) where T : class
    {
        var groups = GroupByTable(context, entities);

        if (groups.Count == 0)
            return 0;

        return await BulkRunner.RunAsync(context, config, saveChangesMode: false, async (executor, isAsync, ct) =>
        {
            var total = 0;

            foreach (var source in groups)
                total += await operation(executor, source, isAsync, ct);

            return total;
        }, async, cancellationToken);
    }

    private static async Task<List<T>> ReadAsync<T>(DbContext context, IEnumerable<T> entities, BulkConfig config, bool async, CancellationToken cancellationToken) where T : class
    {
        var groups = GroupByTable(context, entities);

        if (groups.Count == 0)
            return [];

        if (groups.Count > 1)
            throw new NotSupportedException($"BulkRead needs all entities to be stored in one table, but the given '{typeof(T).Name}' entities are stored in {groups.Count} tables.");

        return await BulkRunner.RunAsync(context, config, saveChangesMode: false, (executor, isAsync, ct) => executor.ReadAsync<T>(groups[0], isAsync, ct), async, cancellationToken);
    }

    private static Task TruncateAsync<T>(DbContext context, bool async, CancellationToken cancellationToken) where T : class
    {
        ArgumentNullException.ThrowIfNull(context);

        BulkProviderResolver.Resolve(context);

        var entityType = FindEntityType(context.Model, typeof(T));
        var table = TableModel.For(entityType);

        return BulkRunner.RunAsync(context, null, saveChangesMode: false, async (executor, isAsync, ct) =>
        {
            await executor.TruncateAsync(table, isAsync, ct);

            return 0;
        }, async, cancellationToken);
    }

    private static List<RowSource> GroupByTable<T>(DbContext context, IEnumerable<T> entities) where T : class
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(entities);

        // Fail on an unsupported provider before reading the model, which needs a relational provider.
        BulkProviderResolver.Resolve(context);

        var model = context.Model;
        var entityTypes = new Dictionary<Type, (IEntityType EntityType, TableModel Table)>();
        var groups = new Dictionary<TableModel, (List<object> Entities, List<IEntityType> Types)>();
        var order = new List<TableModel>();

        foreach (var entity in entities)
        {
            if (entity is null)
                throw new ArgumentException("The entities contain a null element.", nameof(entities));

            var clrType = entity.GetType();

            if (!entityTypes.TryGetValue(clrType, out var resolved))
            {
                var entityType = FindEntityType(model, clrType);
                entityTypes[clrType] = resolved = (entityType, TableModel.For(entityType));
            }

            if (!groups.TryGetValue(resolved.Table, out var group))
            {
                groups[resolved.Table] = group = ([], []);
                order.Add(resolved.Table);
            }

            group.Entities.Add(entity);
            group.Types.Add(resolved.EntityType);
        }

        return [.. order.Select(table => (RowSource)new EntityRowSource(context, table, groups[table].Entities, [.. groups[table].Types]))];
    }

    /// <summary>
    /// Finds the entity type of a CLR type. Base types are tried too, so proxies created by lazy loading are found.
    /// </summary>
    private static IEntityType FindEntityType(IModel model, Type clrType)
    {
        for (var type = clrType; type is not null && type != typeof(object); type = type.BaseType)
        {
            if (model.FindEntityType(type) is not { } entityType)
                continue;

            if (entityType.IsOwned())
                throw new NotSupportedException($"'{clrType.Name}' is an owned type. Run bulk operations on its owner instead.");

            return entityType;
        }

        throw new InvalidOperationException($"'{clrType.Name}' is not an entity type of the context model. Shared type entity types (property bags) are not supported.");
    }

    #endregion
}