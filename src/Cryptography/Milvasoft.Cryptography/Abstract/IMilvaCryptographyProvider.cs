namespace Milvasoft.Cryptography.Abstract;

/// <summary>
/// Provides authenticated encryption (AES-256-GCM) for strings, byte arrays and files.
/// Values and files encrypted by older versions (AES-CBC) can still be decrypted.
/// </summary>
public interface IMilvaCryptographyProvider
{
    #region Async Encryption

    /// <summary>
    /// Encrypts <paramref name="value"/> (UTF-8) and returns the result as a versioned Base64 string.
    /// </summary>
    /// <param name="value"></param>
    Task<string> EncryptAsync(string value);

    /// <summary>
    /// Decrypts <paramref name="value"/> produced by <see cref="EncryptAsync(string)"/> or by older versions of this library.
    /// </summary>
    /// <param name="value"></param>
    /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown when the value is corrupted, tampered or encrypted with another key.</exception>
    Task<string> DecryptAsync(string value);

    /// <summary>
    /// Encrypts the file in <paramref name="filePath"/> in place. The content is treated as binary and the file is replaced atomically.
    /// </summary>
    /// <param name="filePath"></param>
    Task EncryptFileAsync(string filePath);

    /// <summary>
    /// Decrypts the file in <paramref name="filePath"/> in place. The file is replaced atomically.
    /// Files encrypted by older versions (Base64 text) are also supported; their content is written as UTF-8 text.
    /// </summary>
    /// <param name="filePath"></param>
    Task DecryptFileAsync(string filePath);

    #endregion

    #region Sync Encryption

    /// <summary>
    /// Encrypts <paramref name="value"/> (UTF-8) and returns the result as a versioned Base64 string.
    /// </summary>
    /// <param name="value"></param>
    string Encrypt(string value);

    /// <summary>
    /// Decrypts <paramref name="value"/> produced by <see cref="Encrypt(string)"/> or by older versions of this library.
    /// </summary>
    /// <param name="value"></param>
    /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown when the value is corrupted, tampered or encrypted with another key.</exception>
    string Decrypt(string value);

    /// <summary>
    /// Encrypts <paramref name="value"/> and returns the versioned binary format.
    /// </summary>
    /// <param name="value"></param>
    byte[] EncryptBytes(byte[] value);

    /// <summary>
    /// Decrypts <paramref name="value"/> produced by <see cref="EncryptBytes(byte[])"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown when the value is corrupted, tampered, not in the binary format or encrypted with another key.</exception>
    byte[] DecryptBytes(byte[] value);

    /// <summary>
    /// Encrypts the file in <paramref name="filePath"/> in place. The content is treated as binary and the file is replaced atomically.
    /// </summary>
    /// <param name="filePath"></param>
    void EncryptFile(string filePath);

    /// <summary>
    /// Decrypts the file in <paramref name="filePath"/> in place. The file is replaced atomically.
    /// Files encrypted by older versions (Base64 text) are also supported; their content is written as UTF-8 text.
    /// </summary>
    /// <param name="filePath"></param>
    void DecryptFile(string filePath);

    #endregion
}
