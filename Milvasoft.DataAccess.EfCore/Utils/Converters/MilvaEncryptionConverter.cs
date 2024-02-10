using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Milvasoft.Encryption.Abstract;

namespace Milvasoft.DataAccess.EfCore.Utils.Converters;

/// <summary>
/// Defines the internal encryption converter for string values.
/// </summary>
public sealed class MilvaEncryptionConverter : ValueConverter<string, string>
{
    /// <summary>
    /// Creates a new <see cref="MilvaEncryptionConverter"/> instance.
    /// </summary>
    /// <param name="encryptionProvider">Encryption provider</param>
    /// <param name="mappingHints">Entity Framework mapping hints</param>
    public MilvaEncryptionConverter(IMilvaEncryptionProvider encryptionProvider, ConverterMappingHints mappingHints = null)
        : base(value => encryptionProvider.Encrypt(value), x => encryptionProvider.Decrypt(x), mappingHints)
    {
    }
}
