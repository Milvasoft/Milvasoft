using System.ComponentModel.DataAnnotations.Schema;

namespace Milvasoft.Core.MultiLanguage.EntityBases.Abstract;

/// <summary>
/// Interface for entities with multiple language data. 
/// </summary>
/// <typeparam name="TTranslationEntity"></typeparam>
public interface IHasTranslation<TTranslationEntity> where TTranslationEntity : class
{
    /// <summary>
    /// Translation data.
    /// </summary>
    [NotMapped]
    public ICollection<TTranslationEntity> Translations { get; set; }
}
