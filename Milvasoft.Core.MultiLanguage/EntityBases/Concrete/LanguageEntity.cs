using Milvasoft.Core.EntityBases.Abstract;
using Milvasoft.Core.EntityBases.Concrete;
using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milvasoft.Core.MultiLanguage.EntityBases.Concrete;

/// <summary>
/// Base entity for all of entities.
/// </summary>
public abstract class LanguageEntity : EntityBase, ILanguage, IBaseEntity<int>
{
    /// <summary>
    /// Unique identifier for this entity.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public virtual int Id { get; set; }

    /// <summary> 
    /// Name of language. e.g. English
    /// </summary>
    public string Name { get; set; }

    /// <summary> 
    /// Iso code of language. e.g. en-US
    /// </summary>
    public string Code { get; set; }

    /// <summary> 
    /// Determines whether this language supported by system or not. Defualt is true.
    /// </summary>
    public bool Supported { get; set; } = true;

    /// <summary> 
    /// Determines whether this language is default by system or not. Defualt is false.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Returns this instance of "<see cref="Type"/>.Name <see cref="BaseEntity{TKey}"/>.Id" as string.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"[{GetType().Name} {Id}]";
}