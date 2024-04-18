using Milvasoft.Cryptography.Abstract;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Milvasoft.DataAccess.MongoDB.Utils.Serializers;

/// <summary>
/// Serializer for the EncryptedString type.
/// </summary>
public interface IEncryptedStringSerializer : IBsonSerializer<EncryptedString> { }

/// <summary>
/// Serializer implementation for the EncryptedString type.
/// </summary>
public class EncryptedStringSerializer(IMilvaCryptographyProvider encrypter) : SerializerBase<EncryptedString>, IEncryptedStringSerializer
{
    private readonly IMilvaCryptographyProvider _encrypter = encrypter;

    /// <summary>
    /// Deserializes an EncryptedString from a BsonDeserializationContext.
    /// </summary>
    public override EncryptedString Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        string encryptedString = string.Empty;

        if (context.Reader.CurrentBsonType == BsonType.Null)
        {
            context.Reader.ReadNull();
        }
        else
        {
            encryptedString = context.Reader.ReadString();
        }

        return string.IsNullOrWhiteSpace(encryptedString) ? new(string.Empty) : (EncryptedString)_encrypter.Decrypt(encryptedString);
    }

    /// <summary>
    /// Serializes an EncryptedString to a BsonSerializationContext.
    /// </summary>
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, EncryptedString value)
    {
        if (!value.IsNullOrWhiteSpace())
        {
            var encryptedString = _encrypter.Encrypt(value);

            context.Writer.WriteString(encryptedString);
        }
        else
        {
            context.Writer.WriteString(string.Empty);
        }
    }
}
