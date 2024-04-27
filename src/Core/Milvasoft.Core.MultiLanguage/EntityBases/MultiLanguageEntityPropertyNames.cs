using Milvasoft.Core.MultiLanguage.EntityBases.Concrete;

namespace Milvasoft.Core.MultiLanguage.EntityBases;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
/// <summary>
/// Auditing property names.
/// </summary>
public static class MultiLanguageEntityPropertyNames
{
    public static string LanguageId { get; } = nameof(TranslationEntity<int, object, object>.LanguageId);
    public static string Entity { get; } = nameof(TranslationEntity<int, object, object>.Entity);
    public static string EntityId { get; } = nameof(TranslationEntity<int, object, object>.EntityId);
    public static string Translations { get; } = nameof(HasTranslationEntity<int, object>.Translations);
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
