namespace Milvasoft.Core.MultiLanguage.EntityBases.Abstract;

/// <summary>
/// Interface for language entity.
/// </summary>
public interface ILanguage
{
    /// <summary> 
    /// Id of language.
    /// </summary>
    public int Id { get; set; }

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
    public bool Supported { get; set; }

    /// <summary> 
    /// Determines whether this language is default by system or not. Defualt is false.
    /// </summary>
    public bool IsDefault { get; set; }
}