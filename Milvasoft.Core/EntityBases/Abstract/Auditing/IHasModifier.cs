namespace Milvasoft.Core.EntityBases.Abstract.Auditing;

/// <summary>
/// Determines entity has modifier.
/// </summary>
public interface IHasModifier
{
    /// <summary>
    /// Last modifier of entity.
    /// </summary>
    string LastModifierUserName { get; set; }
}
