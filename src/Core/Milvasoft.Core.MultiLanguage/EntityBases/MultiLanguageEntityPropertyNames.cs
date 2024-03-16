using Milvasoft.Core.MultiLanguage.EntityBases.Concrete;

namespace Milvasoft.Core.MultiLanguage.EntityBases;

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
