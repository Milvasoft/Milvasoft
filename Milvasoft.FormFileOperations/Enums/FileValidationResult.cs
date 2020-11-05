namespace Milvasoft.FormFileOperations.Enums
{
    /// <summary>
    /// <para> File validation results. </para>
    /// </summary>
    public enum FileValidationResult
    {
        /// <summary> 
        /// <para>Valid file result.</para>
        /// </summary>
        Valid = 1,
        /// <summary> 
        /// <para>File size too big result.</para>
        /// </summary>
        FileSizeTooBig = 2,
        /// <summary> 
        /// <para>Invalid file extension result.</para>
        /// </summary>
        InvalidFileExtension =3,
        /// <summary> 
        /// <para>Invalid file extension result.</para>
        /// </summary>
        NullFile = 4
    }
}
