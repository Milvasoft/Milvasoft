using Milvasoft.Helpers.FileOperations.Abstract;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.FileOperations.Concrete
{
    /// <summary>
    /// Provides file encryption and decryption with AES Algorithm. Milvasoft Corporation is not responsible of possible data loss.
    /// </summary>
    public class CryptoOperations : ICryptoOperations
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
        public async Task EncryptFileAsync(string filePath, string key)
        {
            byte[] plainContent = await File.ReadAllBytesAsync(filePath).ConfigureAwait(false);

            using var algorithm = new AesCryptoServiceProvider();

            var keyBytes = Encoding.UTF8.GetBytes(key);

            if (keyBytes.Length != 16) throw new ArgumentOutOfRangeException("Key is not proper length. Key bit length must be 16.");

            algorithm.IV = keyBytes;
            algorithm.Key = keyBytes;
            algorithm.Mode = CipherMode.CBC;
            algorithm.Padding = PaddingMode.PKCS7;

            using var memStream = new MemoryStream();

            using var cryptoStream = new CryptoStream(memStream, algorithm.CreateEncryptor(), CryptoStreamMode.Write);

            cryptoStream.Write(plainContent, 0, plainContent.Length);
            try
            {
                cryptoStream.FlushFinalBlock();
            }
            catch (CryptographicException)
            {
                throw;
            }

            await File.WriteAllBytesAsync(filePath, memStream.ToArray()).ConfigureAwait(false);
        }

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
        public async Task DecryptFileAsync(string filePath, string key)
        {
            byte[] plainContent = await File.ReadAllBytesAsync(filePath).ConfigureAwait(false);

            using var algorithm = new AesCryptoServiceProvider();
            var keyBytes = Encoding.UTF8.GetBytes(key);

            if (keyBytes.Length != 16) throw new ArgumentOutOfRangeException("Key is not proper length. Key bit length must be 16.");

            algorithm.IV = keyBytes;
            algorithm.Key = keyBytes;
            algorithm.Mode = CipherMode.CBC;
            algorithm.Padding = PaddingMode.PKCS7;

            using var memStream = new MemoryStream();

            using var cryptoStream = new CryptoStream(memStream, algorithm.CreateDecryptor(), CryptoStreamMode.Write);

            cryptoStream.Write(plainContent, 0, plainContent.Length);

            try
            {
                cryptoStream.FlushFinalBlock();
            }
            catch (Exception)
            {
                throw new ArgumentException("Incorrect key.");
            }

            await File.WriteAllBytesAsync(filePath, memStream.ToArray()).ConfigureAwait(false);
        }

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
        public void EncryptFile(string filePath, string key)
        {
            byte[] plainContent = File.ReadAllBytes(filePath);

            using var algorithm = new AesCryptoServiceProvider();

            var keyBytes = Encoding.UTF8.GetBytes(key);

            if (keyBytes.Length != 16) throw new ArgumentOutOfRangeException("Key is not proper length. Key bit length must be 16.");

            algorithm.IV = keyBytes;
            algorithm.Key = keyBytes;
            algorithm.Mode = CipherMode.CBC;
            algorithm.Padding = PaddingMode.PKCS7;

            using var memStream = new MemoryStream();

            using var cryptoStream = new CryptoStream(memStream, algorithm.CreateEncryptor(), CryptoStreamMode.Write);

            cryptoStream.Write(plainContent, 0, plainContent.Length);

            cryptoStream.FlushFinalBlock();

            File.WriteAllBytes(filePath, memStream.ToArray());
        }

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
        public void DecryptFile(string filePath, string key)
        {
            byte[] plainContent = File.ReadAllBytes(filePath);

            using var algorithm = new AesCryptoServiceProvider();

            var keyBytes = Encoding.UTF8.GetBytes(key);

            if (keyBytes.Length != 16) throw new ArgumentOutOfRangeException("Key is not proper length. Key bit length must be 16.");

            algorithm.IV = keyBytes;
            algorithm.Key = keyBytes;
            algorithm.Mode = CipherMode.CBC;
            algorithm.Padding = PaddingMode.PKCS7;

            using var memStream = new MemoryStream();

            using var cryptoStream = new CryptoStream(memStream, algorithm.CreateDecryptor(), CryptoStreamMode.Write);

            cryptoStream.Write(plainContent, 0, plainContent.Length);
            try
            {
                cryptoStream.FlushFinalBlock();
            }
            catch (Exception)
            {
                throw new ArgumentException("Incorrect key.");
            }

            File.WriteAllBytes(filePath, memStream.ToArray());

        }

        #endregion

    }
}
