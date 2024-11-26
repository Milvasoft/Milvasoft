namespace Milvasoft.Core.Utils.Constants;

/// <summary>
/// Auditing property names.
/// </summary>
public static class EntityPropertyNames
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public const string Id = nameof(Id);
    public const string CreationDate = nameof(CreationDate);
    public const string CreatorUserName = nameof(CreatorUserName);
    public const string LastModificationDate = nameof(LastModificationDate);
    public const string LastModifierUserName = nameof(LastModifierUserName);
    public const string DeletionDate = nameof(DeletionDate);
    public const string DeleterUserName = nameof(DeleterUserName);
    public const string IsDeleted = nameof(IsDeleted);
    public const string TransactionId = nameof(TransactionId);
    public const string UtcLogTime = nameof(UtcLogTime);
    public const string IsSuccess = nameof(IsSuccess);
    public const string MethodName = nameof(MethodName);
    public const string TenantId = nameof(TenantId);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
