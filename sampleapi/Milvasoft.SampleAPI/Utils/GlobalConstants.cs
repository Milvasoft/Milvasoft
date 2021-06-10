using Milvasoft.Helpers.Models;
using Milvasoft.SampleAPI.AppStartup;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace Milvasoft.SampleAPI.Utils
{
    /// <summary>
    /// Global constants.
    /// </summary>
    public static class GlobalConstants
    {
        /// <summary>
        /// <para><b>EN: </b>Rootpath of application. </para>
        /// <para><b>TR: </b>Uygulamanın kök yolu.</para>
        /// </summary>
        public static string RootPath { get; } = Environment.CurrentDirectory;

        /// <summary>m
        /// <para><b>EN: </b>Process list.</para>
        /// <para><b>TR: </b>İşlem listesi.</para>
        /// </summary>
        public static List<Process> Processes { get; set; } = new List<Process>();

        /// <summary>
        /// <para><b>EN: </b> Allowed file extensions for media files. </para>
        /// <para><b>TR: </b> Medya dosyaları için izin verilen uzantılar.  </para>
        /// </summary>
        public static List<AllowedFileExtensions> AllowedFileExtensions { get; set; }

        /// <summary>
        /// <para><b>EN: </b> Invalid strings for prevent hacking or someting ;)  </para>
        /// <para><b>TR: </b> Hacking veya başka bir şeyi önlemek için geçersiz string değerler ;) </para>
        /// </summary>
        public static List<InvalidString> StringBlacklist { get; set; }

        ///<summary>
        /// <para><b>EN: </b> Id of Institution.  </para>
        /// <para><b>TR: </b> İşletmenin Id'si. </para>
        /// </summary>
        public static Guid InstitutionId { get; set; }

        /// <summary>
        /// <para><b>EN: </b> Thumbnail file size.  </para>
        /// <para><b>TR: </b> Thumbnail dosyalarının boyutu. </para>
        /// </summary>
        public static List<Size> ThumbnailSizes { get; } = new List<Size>
        {
            new Size(width: 75, height: 75),
            new Size(width: 100, height: 100),
            new Size(width: 125, height: 125),
            new Size(width: 200, height: 200),
            new Size(width: 400, height: 400),
            new Size (width: 600, height: 600),
        };

        /// <summary>
        /// <para><b>EN: </b>Path of "Media Library" folder in wwwroot folder.  </para>
        /// <para><b>TR: </b>Wwwroot klasöründeki "Media Library" klasörünün yolu.</para>
        /// </summary>
        public static string BackupsFolderPath { get; } = Path.Combine(Startup.WebHostEnvironment.WebRootPath, "Backups");

        /// <summary>
        /// <para><b>EN: </b>Path of "Media Library" folder in wwwroot folder.  </para>
        /// <para><b>TR: </b>Wwwroot klasöründeki "Media Library" klasörünün yolu.</para>
        /// </summary>
        public static string UserActivityLogsBackupsPath { get; } = Path.Combine(BackupsFolderPath, "UserActivityLogsBackups");

        /// <summary>
        /// <para><b>EN: </b>Path of "Media Library" folder in wwwroot folder.  </para>
        /// <para><b>TR: </b>Wwwroot klasöründeki "Media Library" klasörünün yolu.</para>
        /// </summary>
        public static string MediaLibraryPath { get; } = Path.Combine(Startup.WebHostEnvironment.WebRootPath, "Media Library");

        /// <summary>
        /// <para><b>EN: </b>Path of "Image Library" folder in wwwroot folder.   </para>
        /// <para><b>TR: </b>Wwwroot klasöründeki "Image Library" klasörünün yolu.</para>
        /// </summary>
        public static string ImageLibraryPath { get => Path.Combine(MediaLibraryPath, "Image Library"); }

        /// <summary>
        /// <para><b>EN: </b>Path of "ARModel Library" folder in wwwroot folder. </para>
        /// <para><b>TR: </b>Wwwroot klasöründeki "ARModel Library" klasörünün yolu.</para>
        /// </summary>
        public static string ARModelLibraryPath { get => Path.Combine(MediaLibraryPath, "ARModel Library"); }


        /// <summary>
        /// <para><b>EN: </b>Path of "Video Library" folder in wwwroot folder.</para>
        /// <para><b>TR: </b>Wwwroot klasöründeki "Video Library" klasörünün yolu.</para>
        /// </summary>
        public static string VideoLibraryPath { get; } = Path.Combine(MediaLibraryPath, "Video Library");

        /// <summary>
        /// <para><b>EN: </b>Path of "Video Library" folder in wwwroot folder.</para>
        /// <para><b>TR: </b>Wwwroot klasöründeki "Video Library" klasörünün yolu.</para>
        /// </summary>
        public static string DocumentLibraryPath { get; } = Path.Combine(MediaLibraryPath, "Document Library");


        public static byte[] ApiKey { get; set; }

        public static string MilvaKey { get; } = "w!z%C*F-JaNdRgUk";

        public const string ApplicationSiteUrl = "https://educationapp.vercel.app";

        public const string DeveloperSiteUrl = "https://www.milvasoft.com";

        public const string MilvaSampleApiKey = "w!z%C*F-JaNdRgUk";

        /// <summary>
        /// Zero
        /// </summary>
        public const int Zero = 0;

        /// <summary>
        /// Mail content of injection mails.
        /// </summary>
        public static string MailContent { get; } = $"Injection warning from business with global Id value {InstitutionId}";

        /// <summary>
        /// 
        /// </summary>
        public static int DefaultNumberOfPeople { get; } = 2;
        public static sbyte DefaultLanguageId { get; set; } = 1;

    }
}
