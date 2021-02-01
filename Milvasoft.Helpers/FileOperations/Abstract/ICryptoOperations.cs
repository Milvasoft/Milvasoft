using System.Threading.Tasks;

namespace Milvasoft.Helpers.FileOperations.Abstract
{
    /// <summary>
    /// Provides file encryption and decryption with AES Algorithm. Milvasoft Corporation is not responsible of possible data loss.
    /// </summary>
    public interface ICryptoOperations
    {

        #region Async File Encryption

        /// <summary>
        /// Encrypt file in <paramref name="filePath"/> with AES Algorithm and <paramref name="key"/>.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// 
        /// <remarks>
        /// 
        /// <para><b>Remarks:</b></para>
        /// 
        /// <para><paramref name="key"/> must be between 128-256 bit.</para>
        /// 
        /// </remarks>
        /// 
        /// <param name="filePath"></param>
        /// <param name="key"></param>
        Task EncryptFileAsync(string filePath, string key);

        /// <summary>
        /// Decrypt file in <paramref name="filePath"/> with AES Algorithm and <paramref name="key"/>.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// 
        /// <remarks>
        /// 
        /// <para><b>Remarks:</b></para>
        /// 
        /// <para><paramref name="key"/> must be between 128-256 bit.</para>
        /// 
        /// </remarks>
        /// 
        /// <param name="filePath"></param>
        /// <param name="key"></param>
        Task DecryptFileAsync(string filePath, string key);

        #endregion

        #region Sync File Encryption

        /// <summary>
        /// Encrypt file in <paramref name="filePath"/> with <paramref name="key"/>.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// 
        /// <remarks>
        /// 
        /// <para><b>Remarks:</b></para>
        /// 
        /// <para><paramref name="key"/> must be between 128-256 bit.</para>
        /// 
        /// </remarks>
        /// 
        /// <param name="filePath"></param>
        /// <param name="key"></param>
        void EncryptFile(string filePath, string key);

        /// <summary>
        /// Decrypt file in <paramref name="filePath"/> with <paramref name="key"/>.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// 
        /// <remarks>
        /// 
        /// <para><b>Remarks:</b></para>
        /// 
        /// <para><paramref name="key"/> must be between 128-256 bit.</para>
        /// 
        /// </remarks>
        /// 
        /// <param name="filePath"></param>
        /// <param name="key"></param>
        void DecryptFile(string filePath, string key);

        #endregion

    }
}
