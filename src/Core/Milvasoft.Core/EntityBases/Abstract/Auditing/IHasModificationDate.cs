namespace Milvasoft.Core.EntityBases.Abstract.Auditing;

/// <summary>
/// Determines entity has last modification date.
/// </summary>
public interface IHasModificationDate
{
    /// <summary>
    /// Last modification date of entity.
    /// </summary>
    DateTime? LastModificationDate { get; set; }
}
