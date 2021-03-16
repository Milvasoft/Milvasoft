using Milvasoft.Helpers.Encryption.Concrete;
using Milvasoft.Helpers.FileOperations.Abstract;
using Milvasoft.SampleAPI.Utils;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.AppStartup
{
    public static class StartupConfiguration
    {
        /// <summary>
        /// For development.
        /// </summary>
        /// <param name="jsonOperations"></param>
        /// <returns></returns>
        public static async Task EncryptFile(IJsonOperations jsonOperations)
        {
            var provider = new MilvaEncryptionProvider(GlobalConstants.MilvaKey);

            //await provider.EncryptFileAsync(Path.Combine(GlobalConstants.RootPath, "StaticFiles", "JSON", "stringblacklist.json"), GlobalConstants.OpsiyonKey).ConfigureAwait(false);
            //await provider.EncryptFileAsync(Path.Combine(GlobalConstants.RootPath, "StaticFiles", "JSON", "allowedfileextensions.json"), GlobalConstants.OpsiyonKey).ConfigureAwait(false);
            //await provider.EncryptFileAsync(Path.Combine(GlobalConstants.RootPath, "StaticFiles", "JSON", "generalsettings.json"), GlobalConstants.OpsiyonKey).ConfigureAwait(false);
            //await provider.EncryptFileAsync(Path.Combine(GlobalConstants.RootPath, "StaticFiles", "JSON", "hubgroups.json"), GlobalConstants.OpsiyonKey).ConfigureAwait(false);
            //await provider.EncryptFileAsync(Path.Combine(GlobalConstants.RootPath, "StaticFiles", "JSON", "apiaddresses.Development.json"), GlobalConstants.OpsiyonKey).ConfigureAwait(false);
            //await provider.EncryptFileAsync(Path.Combine(GlobalConstants.RootPath, "StaticFiles", "JSON", "apiaddresses.Production.json"), GlobalConstants.OpsiyonKey).ConfigureAwait(false);
            //await provider.EncryptFileAsync(Path.Combine(GlobalConstants.RootPath, "StaticFiles", "JSON", "institutionId.json"), GlobalConstants.OpsiyonKey).ConfigureAwait(false);
            //await provider.EncryptFileAsync(Path.Combine(GlobalConstants.RootPath, "StaticFiles", "JSON", "keymanagement.json"), GlobalConstants.OpsiyonKey).ConfigureAwait(false);
            await provider.EncryptFileAsync(Path.Combine(Environment.CurrentDirectory, "StaticFiles", "JSON", "tokenmanagement.json")).ConfigureAwait(false);
            //await provider.EncryptFileAsync(Path.Combine(GlobalConstants.RootPath, "StaticFiles", "JSON", "MySql.connectionstring.Development.json"), GlobalConstants.OpsiyonKey).ConfigureAwait(false);
            //await provider.EncryptFileAsync(Path.Combine(GlobalConstants.RootPath, "StaticFiles", "JSON", "MySql.connectionstring.Production.json"), GlobalConstants.OpsiyonKey).ConfigureAwait(false);
            //await provider.EncryptFileAsync(Path.Combine(GlobalConstants.RootPath, "StaticFiles", "JSON", "PostgreSql.connectionstring.Development.json"), GlobalConstants.OpsiyonKey).ConfigureAwait(false);
            //await provider.EncryptFileAsync(Path.Combine(GlobalConstants.RootPath, "StaticFiles", "JSON", "PostgreSql.connectionstring.Production.json"), GlobalConstants.OpsiyonKey).ConfigureAwait(false);
        }

        /// <summary>
        /// For development.
        /// </summary>
        /// <param name="jsonOperations"></param>
        /// <returns></returns>
        public static async Task DecryptFile(IJsonOperations jsonOperations)
        {
            var provider = new MilvaEncryptionProvider(GlobalConstants.MilvaKey);

            //await provider.DecryptFileAsync(Path.Combine(GlobalConstants.RootPath, "StaticFiles", "JSON", "stringblacklist.json"), GlobalConstants.OpsiyonKey).ConfigureAwait(false);
            //await provider.DecryptFileAsync(Path.Combine(GlobalConstants.RootPath, "StaticFiles", "JSON", "allowedfileextensions.json"), GlobalConstants.OpsiyonKey).ConfigureAwait(false);
            //await provider.DecryptFileAsync(Path.Combine(GlobalConstants.RootPath, "StaticFiles", "JSON", "generalsettings.json"), GlobalConstants.OpsiyonKey).ConfigureAwait(false);
            //await provider.DecryptFileAsync(Path.Combine(GlobalConstants.RootPath, "StaticFiles", "JSON", "hubgroups.json"), GlobalConstants.OpsiyonKey).ConfigureAwait(false);
            //await provider.DecryptFileAsync(Path.Combine(GlobalConstants.RootPath, "StaticFiles", "JSON", "apiaddresses.Development.json"), GlobalConstants.OpsiyonKey).ConfigureAwait(false);
            //await provider.DecryptFileAsync(Path.Combine(GlobalConstants.RootPath, "StaticFiles", "JSON", "apiaddresses.Production.json"), GlobalConstants.OpsiyonKey).ConfigureAwait(false);
            //await provider.DecryptFileAsync(Path.Combine(GlobalConstants.RootPath, "StaticFiles", "JSON", "institutionId.json"), GlobalConstants.OpsiyonKey).ConfigureAwait(false);
            //await provider.DecryptFileAsync(Path.Combine(GlobalConstants.RootPath, "StaticFiles", "JSON", "keymanagement.json"), GlobalConstants.OpsiyonKey).ConfigureAwait(false);
            await provider.DecryptFileAsync(Path.Combine(Environment.CurrentDirectory, "StaticFiles", "JSON", "tokenmanagement.json")).ConfigureAwait(false);
            //await provider.DecryptFileAsync(Path.Combine(GlobalConstants.RootPath, "StaticFiles", "JSON", "MySql.connectionstring.Development.json"), GlobalConstants.OpsiyonKey).ConfigureAwait(false);
            //await provider.DecryptFileAsync(Path.Combine(GlobalConstants.RootPath, "StaticFiles", "JSON", "MySql.connectionstring.Production.json"), GlobalConstants.OpsiyonKey).ConfigureAwait(false);
            //await provider.DecryptFileAsync(Path.Combine(GlobalConstants.RootPath, "StaticFiles", "JSON", "PostgreSql.connectionstring.Development.json"), GlobalConstants.OpsiyonKey).ConfigureAwait(false);
            //await provider.DecryptFileAsync(Path.Combine(GlobalConstants.RootPath, "StaticFiles", "JSON", "PostgreSql.connectionstring.Production.json"), GlobalConstants.OpsiyonKey).ConfigureAwait(false);
        }


    }
}
