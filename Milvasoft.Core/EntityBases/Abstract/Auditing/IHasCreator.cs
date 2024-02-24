namespace Milvasoft.Core.EntityBases.Abstract.Auditing;

/// <summary>
/// Determines entity has creator.
/// </summary>
public interface IHasCreator
{
    /// <summary>
    /// Creator of entity.
    /// </summary>
    string CreatorUserName { get; set; }
}
