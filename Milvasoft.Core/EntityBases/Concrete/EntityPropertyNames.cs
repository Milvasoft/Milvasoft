namespace Milvasoft.Core.EntityBases.Concrete;

/// <summary>
/// Auditing property names.
/// </summary>
public static class EntityPropertyNames
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public const string Id = nameof(Id);
    public const string CreationDate = nameof(CreationDate);
    public const string CreatorUserId = nameof(CreatorUserId);
    public const string CreatorUser = nameof(CreatorUser);
    public const string LastModificationDate = nameof(LastModificationDate);
    public const string LastModifierUserId = nameof(LastModifierUserId);
    public const string LastModifierUser = nameof(LastModifierUser);
    public const string DeletionDate = nameof(DeletionDate);
    public const string DeleterUserId = nameof(DeleterUserId);
    public const string DeleterUser = nameof(DeleterUser);
    public const string IsDeleted = nameof(IsDeleted);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
