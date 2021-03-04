using System.Collections.Generic;

namespace Milvasoft.SampleAPI.Utils
{
    /// <summary>
    /// <para><b>EN: </b> Allowed file extensions for media files. </para>
    /// <para><b>TR: </b> Medya dosyaları için izin verilen uzantılar.  </para>
    /// </summary>
    public class AllowedFileExtensions
    {
        /// <summary>
        /// <para><b>EN: </b> File type of media file.  </para>
        /// <para><b>TR: </b> Medya dosyasının dosya tipi. </para>
        /// </summary>
        public string FileType { get; set; }

        /// <summary>
        /// <para><b>EN: </b> Allowed extensions for this media type.  </para>
        /// <para><b>TR: </b> Bu medya tipi için izin verilen uzantılar. </para>
        /// </summary>
        public List<string> AllowedExtensions { get; set; }
    }
}
