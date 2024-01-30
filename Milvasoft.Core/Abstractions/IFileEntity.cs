namespace Milvasoft.Core.Abstractions;

/// <summary>
///  Abstraction for multiple file upload process.
/// </summary>
public interface IFileEntity
{
    /// <summary>
    /// Name of file.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Path of file.
    /// </summary>
    public string FilePath { get; set; }
}
