using System.Threading.Tasks;

namespace Milvasoft.Helpers.Encryption.Abstract
{
    /// <summary>
    /// Provides file encryption and decryption with AES Algorithm. Milvasoft Corporation is not responsible of possible data loss.
    /// </summary>
    public interface IMilvaEncryptionProvider
    {
        #region Async Encryption

        /// <summary>
        /// Encrypt file in <paramref name="filePath"/> with AES Algorithm and key.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// <param name="filePath"></param>
        Task EncryptFileAsync(string filePath);

        /// <summary>
        /// Decrypt file in <paramref name="filePath"/> with AES Algorithm and key.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// <param name="filePath"></param>
        Task DecryptFileAsync(string filePath);

        #endregion

        #region Sync Encryption

        /// <summary>
        /// Encrypt <paramref name="value"/> with AES Algorithm and key.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// <param name="value"></param>
        string Encrypt(string value);

        /// <summary>
        /// Decrypt <paramref name="value"/> with AES Algorithm and key.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// <param name="value"></param>
        string Decrypt(string value);

        /// <summary>
        /// Encrypt file in <paramref name="filePath"/> with key.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// <param name="filePath"></param>
        void EncryptFile(string filePath);

        /// <summary>
        /// Decrypt file in <paramref name="filePath"/> with key.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// <param name="filePath"></param>
        void DecryptFile(string filePath);

        #endregion
    }
}
