using Milvasoft.Core.EntityBases.Abstract.Auditing;

namespace Milvasoft.DataAccess.EfCore.Utils.Enums;

/// <summary>
/// Enumerates the states of soft deletion for database records.
/// </summary>
public enum SoftDeletionState
{
    /// <summary>
    /// When SoftDeletion is active, the record is not deleted from the database, the <see cref="ISoftDeletable.IsDeleted"/> property is updated.
    /// </summary>
    Active,

    /// <summary>
    /// When SoftDeletion is disabled, the record is deleted from the database.
    /// </summary>
    Passive,
}