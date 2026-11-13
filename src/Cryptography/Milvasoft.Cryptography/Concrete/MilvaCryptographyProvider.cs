using Microsoft.IO;
using Milvasoft.Cryptography.Abstract;
using Milvasoft.Cryptography.Builder;
using System.Security.Cryptography;
using System.Text;

namespace Milvasoft.Cryptography.Concrete;

/// <summary>
/// Provides authenticated encryption (AES-256-GCM) for strings, byte arrays and files.
/// </summary>
/// <remarks>
/// <para><b>Formats</b></para>
/// <list type="bullet">
/// <item>String (current): <c>"v2:" + Base64(nonce[12] | ciphertext | tag[16])</c>.</item>
/// <item>Bytes and files (current): <c>"MVENC" | 0x02 | nonce[12] | ciphertext | tag[16]</c>.</item>
/// <item>Legacy string (decryption only): <c>Base64(iv[16] | AES ciphertext)</c> produced by older versions with the configured key, <see cref="IMilvaCryptographyOptions.Cipher"/> and <see cref="IMilvaCryptographyOptions.Padding"/>.
/// Legacy encrypted files contain this string as text.</item>
/// </list>
/// <para><b>Keys</b></para>
/// <list type="bullet">
/// <item>Raw key (recommended for new deployments): <c>"base64:" + Base64(32 random bytes)</c>, for example the output of <see cref="GenerateKey"/>
/// or <c>"base64:" + $(openssl rand -base64 32)</c>. The 32 decoded bytes are used directly as the AES-256-GCM key.
/// The legacy AES-CBC format cannot be decrypted with such a key (no value was ever encrypted with it in that format).</item>
/// <item>UTF-8 key (existing deployments): a string of 16, 24 or 32 bytes in UTF-8. The AES-GCM key is derived from it with HKDF-SHA256 and
/// the UTF-8 bytes are also used as the key of the legacy AES-CBC format, so values encrypted with this key by any version stay decryptable.</item>
/// </list>
/// <para>
/// The two kinds never overlap: a <c>"base64:"</c> key is at least 50 characters long, longer than any UTF-8 key. A string that starts with
/// <c>"base64:"</c> but is 16, 24 or 32 bytes long is therefore treated as a UTF-8 key, as it was in older versions.
/// The key must be a random secret; it is not stretched like a password. It is validated when the services are registered (see <see cref="ValidateKey(string)"/>).
/// Changing the key, including converting a UTF-8 key to the <c>"base64:"</c> form, makes existing values undecryptable.
/// </para>
/// </remarks>
public class MilvaCryptographyProvider : IMilvaCryptographyProvider
{
    #region Fields

    /// <summary>
    /// Prefix of a configured key that is the Base64 encoding of a 32 byte raw AES-256 key.
    /// </summary>
    public const string Base64KeyPrefix = "base64:";

    private const int _rawKeySize = 32;
    private const string _stringFormatPrefix = "v2:";
    private const byte _formatVersion = 2;
    private const int _nonceSize = 12;
    private const int _tagSize = 16;
    private const int _legacyIvSize = 16;
    private static readonly byte[] _fileMagic = "MVENC"u8.ToArray();
    private static readonly byte[] _keyDerivationInfo = "Milvasoft.Cryptography.AesGcm.v2"u8.ToArray();

    private readonly byte[] _legacyKey;
    private readonly byte[] _key;
    private readonly CipherMode _legacyMode;
    private readonly PaddingMode _legacyPadding;

    #endregion

    /// <summary>
    /// Creates a new <see cref="MilvaCryptographyProvider"/> instance.
    /// </summary>
    /// <param name="milvaCryptographyOptions">Cryptography options.</param>
    /// <exception cref="CryptographicException">Thrown when the key is invalid. See <see cref="ValidateKey(string)"/>.</exception>
    public MilvaCryptographyProvider(IMilvaCryptographyOptions milvaCryptographyOptions)
    {
        ArgumentNullException.ThrowIfNull(milvaCryptographyOptions);

        ValidateKey(milvaCryptographyOptions.Key);

        if (TryDecodeBase64Key(milvaCryptographyOptions.Key, out var rawKey))
        {
            _key = rawKey;
            _legacyKey = null;
        }
        else
        {
            _legacyKey = Encoding.UTF8.GetBytes(milvaCryptographyOptions.Key);
            _key = HKDF.DeriveKey(HashAlgorithmName.SHA256, _legacyKey, _rawKeySize, salt: [], info: _keyDerivationInfo);
        }

        _legacyMode = milvaCryptographyOptions.Cipher;
        _legacyPadding = milvaCryptographyOptions.Padding;
    }

    /// <summary>
    /// Creates a new <see cref="MilvaCryptographyProvider"/> instance.
    /// </summary>
    /// <param name="milvaCryptographyOptions">Cryptography options.</param>
    /// <param name="recyclableMemoryStreamManager">Not used anymore. Kept for constructor compatibility.</param>
    /// <exception cref="CryptographicException">Thrown when the key is invalid. See <see cref="ValidateKey(string)"/>.</exception>
    public MilvaCryptographyProvider(IMilvaCryptographyOptions milvaCryptographyOptions, RecyclableMemoryStreamManager recyclableMemoryStreamManager) : this(milvaCryptographyOptions)
    {
    }

    /// <summary>
    /// Validates that <paramref name="key"/> is either <c>"base64:"</c> followed by the Base64 encoding of exactly 32 bytes,
    /// or a string of 16, 24 or 32 bytes in UTF-8.
    /// </summary>
    /// <param name="key">Configured key.</param>
    /// <exception cref="CryptographicException">Thrown when the key is null or invalid.</exception>
    public static void ValidateKey(string key)
    {
        if (TryDecodeBase64Key(key, out _) || IsUtf8Key(key))
            return;

        if (key != null && key.StartsWith(Base64KeyPrefix, StringComparison.Ordinal))
            throw new CryptographicException($"Milvasoft cryptography key with the '{Base64KeyPrefix}' prefix must be the Base64 encoding of exactly {_rawKeySize} bytes.");

        throw new CryptographicException($"Milvasoft cryptography key must be '{Base64KeyPrefix}' followed by the Base64 encoding of {_rawKeySize} random bytes, or 16, 24 or 32 bytes (UTF-8).");
    }

    /// <summary>
    /// Generates a new random key in the <c>"base64:"</c> form (32 random bytes).
    /// </summary>
    /// <returns>A key such as <c>base64:3q2+7w...=</c>.</returns>
    public static string GenerateKey() => Base64KeyPrefix + Convert.ToBase64String(RandomNumberGenerator.GetBytes(_rawKeySize));

    /// <summary>
    /// Decodes a <c>"base64:"</c> key. A UTF-8 key of 16, 24 or 32 bytes is never decoded, even when it starts with the prefix,
    /// because a Base64 encoded 32 byte key does not fit in 32 bytes.
    /// </summary>
    private static bool TryDecodeBase64Key(string key, out byte[] rawKey)
    {
        rawKey = null;

        if (key == null || !key.StartsWith(Base64KeyPrefix, StringComparison.Ordinal) || IsUtf8Key(key))
            return false;

        var encoded = key[Base64KeyPrefix.Length..];

        // Room for one extra byte so that a longer key is detected instead of being truncated.
        Span<byte> buffer = stackalloc byte[_rawKeySize + 3];

        if (!Convert.TryFromBase64String(encoded, buffer, out var written) || written != _rawKeySize)
        {
            CryptographicOperations.ZeroMemory(buffer);
            return false;
        }

        rawKey = buffer[.._rawKeySize].ToArray();
        CryptographicOperations.ZeroMemory(buffer);

        return true;
    }

    private static bool IsUtf8Key(string key) => key != null && Encoding.UTF8.GetByteCount(key) is 16 or 24 or 32;

    #region Async Encryption

    /// <inheritdoc/>
    public Task<string> EncryptAsync(string value) => Task.FromResult(Encrypt(value));

    /// <inheritdoc/>
    public Task<string> DecryptAsync(string value) => Task.FromResult(Decrypt(value));

    /// <inheritdoc/>
    public async Task EncryptFileAsync(string filePath)
    {
        var content = await File.ReadAllBytesAsync(filePath);

        await WriteAllBytesAtomicallyAsync(filePath, EncryptBytes(content));
    }

    /// <inheritdoc/>
    public Task DecryptFileAsync(string filePath) => DecryptFileAsync(filePath, Encoding.UTF8);

    /// <summary>
    /// Decrypts the file in <paramref name="filePath"/> in place. The file is replaced atomically.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="encoding">Encoding used to write the content of legacy (text) encrypted files. e.g. <see cref="Encoding.UTF8"/></param>
    public async Task DecryptFileAsync(string filePath, Encoding encoding)
    {
        var content = await File.ReadAllBytesAsync(filePath);

        await WriteAllBytesAtomicallyAsync(filePath, DecryptFileContent(content, encoding));
    }

    #endregion

    #region Sync Encryption

    /// <inheritdoc/>
    public string Encrypt(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return _stringFormatPrefix + Convert.ToBase64String(EncryptCore(Encoding.UTF8.GetBytes(value)));
    }

    /// <inheritdoc/>
    public string Decrypt(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.StartsWith(_stringFormatPrefix, StringComparison.Ordinal))
            return Encoding.UTF8.GetString(DecryptCore(Convert.FromBase64String(value[_stringFormatPrefix.Length..])));

        return DecryptLegacy(value);
    }

    /// <inheritdoc/>
    public byte[] EncryptBytes(byte[] value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var payload = EncryptCore(value);
        var result = new byte[_fileMagic.Length + 1 + payload.Length];

        _fileMagic.CopyTo(result, 0);
        result[_fileMagic.Length] = _formatVersion;
        payload.CopyTo(result, _fileMagic.Length + 1);

        return result;
    }

    /// <inheritdoc/>
    public byte[] DecryptBytes(byte[] value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (!HasBinaryHeader(value))
            throw new CryptographicException("The data is not in Milvasoft encrypted binary format.");

        return DecryptCore(value.AsSpan(_fileMagic.Length + 1));
    }

    /// <inheritdoc/>
    public void EncryptFile(string filePath)
    {
        var content = File.ReadAllBytes(filePath);

        WriteAllBytesAtomically(filePath, EncryptBytes(content));
    }

    /// <inheritdoc/>
    public void DecryptFile(string filePath) => DecryptFile(filePath, Encoding.UTF8);

    /// <summary>
    /// Decrypts the file in <paramref name="filePath"/> in place. The file is replaced atomically.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="encoding">Encoding used to write the content of legacy (text) encrypted files. e.g. <see cref="Encoding.UTF8"/></param>
    public void DecryptFile(string filePath, Encoding encoding)
    {
        var content = File.ReadAllBytes(filePath);

        WriteAllBytesAtomically(filePath, DecryptFileContent(content, encoding));
    }

    #endregion

    #region Core

    /// <summary>
    /// Encrypts with AES-256-GCM. Returns nonce | ciphertext | tag.
    /// </summary>
    private byte[] EncryptCore(ReadOnlySpan<byte> plainBytes)
    {
        var result = new byte[_nonceSize + plainBytes.Length + _tagSize];
        var nonce = result.AsSpan(0, _nonceSize);
        var cipherText = result.AsSpan(_nonceSize, plainBytes.Length);
        var tag = result.AsSpan(_nonceSize + plainBytes.Length, _tagSize);

        RandomNumberGenerator.Fill(nonce);

        using var aesGcm = new AesGcm(_key, _tagSize);

        aesGcm.Encrypt(nonce, plainBytes, cipherText, tag);

        return result;
    }

    /// <summary>
    /// Decrypts nonce | ciphertext | tag with AES-256-GCM. Throws <see cref="CryptographicException"/> if the data was modified.
    /// </summary>
    private byte[] DecryptCore(ReadOnlySpan<byte> payload)
    {
        if (payload.Length < _nonceSize + _tagSize)
            throw new CryptographicException("Encrypted data is too short.");

        var cipherTextLength = payload.Length - _nonceSize - _tagSize;
        var plainBytes = new byte[cipherTextLength];

        using var aesGcm = new AesGcm(_key, _tagSize);

        aesGcm.Decrypt(payload[.._nonceSize], payload.Slice(_nonceSize, cipherTextLength), payload[^_tagSize..], plainBytes);

        return plainBytes;
    }

    /// <summary>
    /// Decrypts values produced by older versions: Base64(iv[16] | AES ciphertext) with the configured key, cipher mode and padding.
    /// </summary>
    private string DecryptLegacy(string value)
    {
        if (_legacyKey == null)
            throw new CryptographicException($"The value is not in the '{_stringFormatPrefix}' format. The legacy format cannot be decrypted with a '{Base64KeyPrefix}' key.");

        var input = Convert.FromBase64String(value);

        if (input.Length <= _legacyIvSize)
            throw new CryptographicException("Encrypted data is too short.");

        using var aes = Aes.Create();

        aes.Mode = _legacyMode;
        aes.Padding = _legacyPadding;
        aes.Key = _legacyKey;

        using var decryptor = aes.CreateDecryptor(_legacyKey, input[.._legacyIvSize]);

        var plainBytes = decryptor.TransformFinalBlock(input, _legacyIvSize, input.Length - _legacyIvSize);

        // Same decoding as the legacy implementation (UTF-8 with BOM detection, '\0' trimmed).
        using var reader = new StreamReader(new MemoryStream(plainBytes));

        return reader.ReadToEnd().Trim('\0');
    }

    private byte[] DecryptFileContent(byte[] content, Encoding encoding)
    {
        if (HasBinaryHeader(content))
            return DecryptBytes(content);

        // Legacy encrypted files (and files written from the string API) contain the encrypted value as text.
        string encryptedText;

        using (var reader = new StreamReader(new MemoryStream(content), Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
            encryptedText = reader.ReadToEnd();

        var plainText = Decrypt(encryptedText.Trim());

        encoding ??= Encoding.UTF8;

        return [.. encoding.GetPreamble(), .. encoding.GetBytes(plainText)];
    }

    private static bool HasBinaryHeader(ReadOnlySpan<byte> content) => content.Length > _fileMagic.Length
                                                                       && content.StartsWith(_fileMagic)
                                                                       && content[_fileMagic.Length] == _formatVersion;

    #endregion

    #region Atomic file write

    private static void WriteAllBytesAtomically(string filePath, byte[] content)
    {
        var tempFilePath = CreateTempFilePath(filePath);

        try
        {
            using (var stream = new FileStream(tempFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
                stream.Write(content);

            File.Move(tempFilePath, filePath, overwrite: true);
        }
        catch
        {
            TryDelete(tempFilePath);
            throw;
        }
    }

    private static async Task WriteAllBytesAtomicallyAsync(string filePath, byte[] content)
    {
        var tempFilePath = CreateTempFilePath(filePath);

        try
        {
            await using (var stream = new FileStream(tempFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough | FileOptions.Asynchronous))
                await stream.WriteAsync(content);

            File.Move(tempFilePath, filePath, overwrite: true);
        }
        catch
        {
            TryDelete(tempFilePath);
            throw;
        }
    }

    /// <summary>
    /// Temp file is created in the same directory so the final move is a same-volume rename.
    /// </summary>
    private static string CreateTempFilePath(string filePath)
    {
        var fullPath = Path.GetFullPath(filePath);

        return Path.Combine(Path.GetDirectoryName(fullPath)!, $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");
    }

    private static void TryDelete(string filePath)
    {
        try
        {
            File.Delete(filePath);
        }
        catch (IOException)
        {
            // Best effort cleanup.
        }
        catch (UnauthorizedAccessException)
        {
            // Best effort cleanup.
        }
    }

    #endregion
}
