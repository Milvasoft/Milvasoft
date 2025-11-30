using Milvasoft.Core.MultiLanguage.EntityBases.Concrete;

namespace Milvasoft.Core.MultiLanguage.EntityBases;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
/// <summary>
/// Auditing property names.
/// </summary>
public static class MultiLanguageEntityPropertyNames
{
    public static string LanguageId { get; } = nameof(TranslationEntity<,,>.LanguageId);
    public static string Entity { get; } = nameof(TranslationEntity<,,>.Entity);
    public static string EntityId { get; } = nameof(TranslationEntity<,,>.EntityId);
    public static string Translations { get; } = nameof(HasTranslationEntity<,>.Translations);
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
