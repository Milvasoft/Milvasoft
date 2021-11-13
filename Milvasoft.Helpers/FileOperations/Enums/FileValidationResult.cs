namespace Milvasoft.Helpers.FileOperations.Enums;

/// <summary>
/// File validation results. 
/// </summary>
public enum FileValidationResult
{
    /// <summary> 
    /// Valid file result.
    /// </summary>
    Valid = 1,
    /// <summary> 
    /// File size too big result.
    /// </summary>
    FileSizeTooBig = 2,
    /// <summary> 
    /// Invalid file extension result.
    /// </summary>
    InvalidFileExtension = 3,
    /// <summary> 
    /// Invalid file extension result.
    /// </summary>
    NullFile = 4
}
