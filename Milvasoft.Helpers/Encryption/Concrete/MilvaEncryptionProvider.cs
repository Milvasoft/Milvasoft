using Milvasoft.Helpers.Encryption.Abstract;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.Encryption.Concrete
{
    /// <summary>
    /// Provides file encryption and decryption with AES Algorithm. Milvasoft Corporation is not responsible of possible data loss.
    /// </summary>
    public class MilvaEncryptionProvider : IMilvaEncryptionProvider
    {

        #region Fields

        private readonly byte[] _key;
        private readonly CipherMode _mode;
        private readonly PaddingMode _padding;

        #endregion

        #region Properties

        /// <summary>
        /// AES block size constant.
        /// </summary>
        public const int AesBlockSize = 128;

        /// <summary>
        /// Initialization vector size constant.(IV)
        /// </summary>
        public const int InitializationVectorSize = 16;

        #endregion

        /// <summary>
        /// Creates a new <see cref="MilvaEncryptionProvider"/> instance.
        /// </summary>
        /// <param name="key"> Must be between 128-256 bit.</param>
        /// <param name="mode"></param>
        /// <param name="padding"></param>
        public MilvaEncryptionProvider(string key, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7)
        {
            _key = Encoding.UTF8.GetBytes(key);

            //if (_key.Length != 16) throw new ArgumentOutOfRangeException("Key is not proper length. Key bit length must be 16.");

            _mode = mode;
            _padding = padding;
        }


        #region Async Encryption

        /// <summary>
        /// Encrypt file in <paramref name="filePath"/> with AES Algorithm and key.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// <param name="filePath"></param>
        public async Task EncryptFileAsync(string filePath)
        {
            var inputValue = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);

            var encryptedContent = Encrypt(inputValue);

            await File.WriteAllTextAsync(filePath, encryptedContent, Encoding.UTF8).ConfigureAwait(false);
        }


        /// <summary>
        /// Decrypt file in <paramref name="filePath"/> with AES Algorithm and key.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// <param name="filePath"></param>
        public async Task DecryptFileAsync(string filePath)
        {
            var inputValue = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);

            var decryptedContent = Decrypt(inputValue);

            await File.WriteAllTextAsync(filePath, decryptedContent, Encoding.UTF8).ConfigureAwait(false);
        }

        /// <summary>
        /// Encrypt file in <paramref name="filePath"/> with AES Algorithm and key.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="encoding"> e.g. <see cref="Encoding.UTF8"/></param>
        public async Task EncryptFileAsync(string filePath, Encoding encoding)
        {
            var inputValue = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);

            var encryptedContent = Encrypt(inputValue);

            await File.WriteAllTextAsync(filePath, encryptedContent, encoding).ConfigureAwait(false);
        }


        /// <summary>
        /// Decrypt file in <paramref name="filePath"/> with AES Algorithm and key.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="encoding"> e.g. <see cref="Encoding.UTF8"/></param>
        public async Task DecryptFileAsync(string filePath, Encoding encoding)
        {
            var inputValue = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);

            var decryptedContent = Decrypt(inputValue);

            await File.WriteAllTextAsync(filePath, decryptedContent, encoding).ConfigureAwait(false);
        }

        #endregion

        #region Sync Encryption

        /// <summary>
        /// Encrypt <paramref name="value"/> with AES Algorithm and key.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// <param name="value"></param>
        public string Encrypt(string value)
        {
            byte[] inputValue = Encoding.UTF8.GetBytes(value);

            using var aesProvider = CreateCryptographyProvider();

            aesProvider.GenerateIV();

            byte[] initializationVector = aesProvider.IV;

            using ICryptoTransform encryptor = aesProvider.CreateEncryptor(_key, initializationVector);

            using var memoryStream = new MemoryStream();

            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                memoryStream.Write(initializationVector, 0, initializationVector.Length);
                cryptoStream.Write(inputValue, 0, inputValue.Length);
                cryptoStream.FlushFinalBlock();
            }

            return Convert.ToBase64String(memoryStream.ToArray());
        }

        /// <summary>
        /// Decrypt <paramref name="value"/> with AES Algorithm and key.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// <param name="value"></param>
        public string Decrypt(string value)
        {
            byte[] inputValue = Convert.FromBase64String(value);

            string outputValue;

            using (var memoryStream = new MemoryStream(inputValue))
            {
                var initializationVector = new byte[InitializationVectorSize];

                memoryStream.Read(initializationVector, 0, initializationVector.Length);

                using AesCryptoServiceProvider aesProvider = CreateCryptographyProvider();

                using ICryptoTransform decryptor = aesProvider.CreateDecryptor(_key, initializationVector);

                using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

                using var reader = new StreamReader(cryptoStream);

                outputValue = reader.ReadToEnd().Trim('\0');
            }

            return outputValue;
        }

        /// <summary>
        /// Encrypt file in <paramref name="filePath"/> with AES Algorithm and key.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// <param name="filePath"></param>
        public void EncryptFile(string filePath)
        {
            var inputValue = File.ReadAllText(filePath);

            var encryptedContent = Encrypt(inputValue);

            File.WriteAllText(filePath, encryptedContent, Encoding.UTF8);
        }


        /// <summary>
        /// Decrypt file in <paramref name="filePath"/> with AES Algorithm and key.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// <param name="filePath"></param>
        public void DecryptFile(string filePath)
        {
            var inputValue = File.ReadAllText(filePath);

            var decryptedContent = Decrypt(inputValue);

            File.WriteAllText(filePath, decryptedContent, Encoding.UTF8);
        }

        /// <summary>
        /// Encrypt file in <paramref name="filePath"/> with AES Algorithm and key.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="encoding"> e.g. <see cref="Encoding.UTF8"/></param>
        public void EncryptFile(string filePath, Encoding encoding)
        {
            var inputValue = File.ReadAllText(filePath);

            var encryptedContent = Encrypt(inputValue);

            File.WriteAllText(filePath, encryptedContent, encoding);
        }


        /// <summary>
        /// Decrypt file in <paramref name="filePath"/> with AES Algorithm and key.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="encoding"> e.g. <see cref="Encoding.UTF8"/></param>
        public void DecryptFile(string filePath, Encoding encoding)
        {
            var inputValue = File.ReadAllText(filePath);

            var decryptedContent = Decrypt(inputValue);

            File.WriteAllText(filePath, decryptedContent, encoding);
        }


        #endregion

        /// <summary>
        /// Creates an AES cryptography provider.
        /// </summary>
        /// <returns></returns>
        private AesCryptoServiceProvider CreateCryptographyProvider()
        {
            return new AesCryptoServiceProvider
            {
                BlockSize = AesBlockSize,
                Mode = _mode,
                Padding = _padding,
                Key = _key,
                KeySize = _key.Length * 8
            };
        }

    }
}
