using Milvasoft.Core.EntityBases.Concrete.MultiLanguage;

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
    public static string LanguageId { get; } = nameof(TranslationEntity<int, object, object, object>.LanguageId);
    public static string Entity { get; } = nameof(TranslationEntity<int, object, object, object>.Entity);
    public static string EntityId { get; } = nameof(TranslationEntity<int, object, object, object>.EntityId);
    public static string Translations { get; } = nameof(HasTranslationEntity<int, object>.Translations);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
