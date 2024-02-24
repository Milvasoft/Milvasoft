﻿using Microsoft.EntityFrameworkCore;
using Milvasoft.Core.Extensions;
using Milvasoft.DataAccess.EfCore.Utils.Enums;
using Milvasoft.Helpers.DataAccess.EfCore.Concrete;

namespace Milvasoft.DataAccess.EfCore.Configuration;

/// <summary>
/// <see cref="BaseRepository{TEntity, TKey, TContext}"/> configuration.
/// </summary>
public class RepositoryConfiguration
{
    /// <summary>
    /// All repository methods save changes(<see cref="DbContext.SaveChangesAsync(CancellationToken)"/>) after every operation. Except get operations. 
    /// </summary>
    public SaveChangesChoice DefaultSaveChangesChoice { get; set; } = SaveChangesChoice.AfterEveryOperation;

    /// <summary>
    /// Soft delete state. Default value is false.
    /// </summary>
    public bool DefaultSoftDeletedFetchState { get; set; } = false;

    /// <summary>
    /// Soft delete state. Default value is false.
    /// </summary>
    public bool ResetSoftDeletedFetchStateToDefault { get; set; } = true;
}