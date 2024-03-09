using System.ComponentModel.DataAnnotations.Schema;

namespace Milvasoft.Core.MultiLanguage.EntityBases.Abstract;

/// <summary>
/// Dummy interface for type findings.
/// </summary>
public interface IHasTranslation
{
}

/// <summary>
/// Interface for entities with multiple language data.
/// </summary>
/// <typeparam name="TTranslationEntity"></typeparam>
public interface IHasTranslation<TTranslationEntity> : IHasTranslation where TTranslationEntity : class
{
    /// <summary>
    /// Translation data.
    /// </summary>
    [NotMapped]
    public ICollection<TTranslationEntity> Translations { get; set; }
}
