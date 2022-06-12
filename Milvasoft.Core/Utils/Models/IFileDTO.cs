using Microsoft.AspNetCore.Http;

namespace Milvasoft.Core.Utils.Models;

/// <summary>
/// Abstraction for multiple file upload process.
/// </summary>
public interface IFileDTO
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
