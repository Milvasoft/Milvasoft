namespace Milvasoft.Core.EntityBases.Abstract.Auditing;

/// <summary>
/// Determines entity has deleter.
/// </summary>
public interface IHasDeleter
{
    /// <summary>
    /// Deleter of entity.
    /// </summary>er
    string DeleterUserName { get; set; }
}
