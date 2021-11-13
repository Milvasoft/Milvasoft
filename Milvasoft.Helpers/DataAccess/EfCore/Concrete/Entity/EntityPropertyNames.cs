namespace Milvasoft.Helpers.DataAccess.EfCore.Concrete.Entity;

/// <summary>
/// Auditing property names.
/// </summary>
public static class EntityPropertyNames
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public const string Id = "Id";
    public const string CreationDate = "CreationDate";
    public const string CreatorUserId = "CreatorUserId";
    public const string LastModificationDate = "LastModificationDate";
    public const string LastModifierUserId = "LastModifierUserId";
    public const string DeletionDate = "DeletionDate";
    public const string DeleterUserId = "DeleterUserId";
    public const string IsDeleted = "IsDeleted";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
