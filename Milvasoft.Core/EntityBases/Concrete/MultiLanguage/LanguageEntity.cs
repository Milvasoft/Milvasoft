using Milvasoft.Core.EntityBases.Abstract;
using Milvasoft.Core.EntityBases.Abstract.MultiLanguage;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milvasoft.Core.EntityBases.Concrete.MultiLanguage;

/// <summary>
/// Base entity for all of entities.
/// </summary>
public abstract class LanguageEntity<TKey> : EntityBase, ILanguage<TKey>, IBaseEntity<TKey>
        where TKey : struct, IEquatable<TKey>
{
    /// <summary>
    /// Unique identifier for this entity.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public virtual TKey Id { get; set; }

    /// <summary> 
    /// Name of language. e.g. English
    /// </summary>
    public string Name { get; set; }

    /// <summary> 
    /// Iso code of language. e.g. en-US
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Returns this instance of "<see cref="Type"/>.Name <see cref="BaseEntity{TKey}"/>.Id" as string.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"[{GetType().Name} {Id}]";
}