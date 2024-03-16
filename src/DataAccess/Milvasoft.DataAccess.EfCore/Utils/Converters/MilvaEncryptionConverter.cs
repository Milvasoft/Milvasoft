using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Milvasoft.Cryptography.Abstract;

namespace Milvasoft.DataAccess.EfCore.Utils.Converters;

/// <summary>
/// Defines the internal encryption converter for string values.
/// </summary>
/// <remarks>
/// Creates a new <see cref="MilvaEncryptionConverter"/> instance.
/// </remarks>
/// <param name="encryptionProvider">Encryption provider</param>
/// <param name="mappingHints">Entity Framework mapping hints</param>
public sealed class MilvaEncryptionConverter(IMilvaCryptographyProvider encryptionProvider, ConverterMappingHints mappingHints = null) : ValueConverter<string, string>(value => encryptionProvider.Encrypt(value), x => encryptionProvider.Decrypt(x), mappingHints)
{
}
