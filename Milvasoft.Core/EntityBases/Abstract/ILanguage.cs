using Milvasoft.Core.Utils.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milvasoft.Core.EntityBases.Abstract;

/// <summary>
/// Interface for language entity.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public interface ILanguage<TKey>
{
    /// <summary> 
    /// Id of language.
    /// </summary>
    public TKey Id { get; set; }

    /// <summary> 
    /// Name of language. e.g. English
    /// </summary>
    public string Name { get; set; }

    /// <summary> 
    /// Iso code of language. e.g. en-US
    /// </summary>
    public string Code { get; set; }
}