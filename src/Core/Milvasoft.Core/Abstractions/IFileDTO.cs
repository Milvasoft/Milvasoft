using Microsoft.AspNetCore.Http;

namespace Milvasoft.Core.Abstractions;

/// <summary>
/// Abstraction for multiple file upload process. 
/// </summary>
public interface IFileDto
{
    /// <summary>
    /// Name of file.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// File.
    /// </summary>
    public IFormFile File { get; set; }
}
