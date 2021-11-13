using Milvasoft.SampleAPI.Utils;
using System.IO;

namespace Milvasoft.SampleAPI.AppStartup;

/// <summary>
/// Class in which initial configurations are specified.
/// </summary>
public static class StartupConfiguration
{
    /// <summary>
    /// Checks media library folders. If folders not exists creates that folders.
    /// </summary>
    public static void CheckPublicFiles()
    {
        if (!Directory.Exists(GlobalConstants.BackupsFolderPath))
        {
            Directory.CreateDirectory(GlobalConstants.UserActivityLogsBackupsPath);
        }
        if (!Directory.Exists(GlobalConstants.UserActivityLogsBackupsPath))
        {
            Directory.CreateDirectory(GlobalConstants.UserActivityLogsBackupsPath);
        }

        if (!Directory.Exists(GlobalConstants.MediaLibraryPath))
        {
            Directory.CreateDirectory(GlobalConstants.MediaLibraryPath);
            Directory.CreateDirectory(GlobalConstants.ImageLibraryPath);
            Directory.CreateDirectory(GlobalConstants.ARModelLibraryPath);
            Directory.CreateDirectory(GlobalConstants.VideoLibraryPath);
            Directory.CreateDirectory(GlobalConstants.DocumentLibraryPath);
        }
        if (!Directory.Exists(GlobalConstants.ImageLibraryPath))
        {
            Directory.CreateDirectory(GlobalConstants.ImageLibraryPath);
        }
        if (!Directory.Exists(GlobalConstants.ARModelLibraryPath))
        {
            Directory.CreateDirectory(GlobalConstants.ARModelLibraryPath);
        }
        if (!Directory.Exists(GlobalConstants.VideoLibraryPath))
        {
            Directory.CreateDirectory(GlobalConstants.VideoLibraryPath);
        }
        if (!Directory.Exists(GlobalConstants.DocumentLibraryPath))
        {
            Directory.CreateDirectory(GlobalConstants.DocumentLibraryPath);
        }
    }
}
