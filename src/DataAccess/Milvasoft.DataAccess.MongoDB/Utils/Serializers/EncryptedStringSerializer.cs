using Milvasoft.Cryptography.Abstract;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Milvasoft.DataAccess.MongoDB.Utils.Serializers;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public interface IEncryptedStringSerializer : IBsonSerializer<EncryptedString> { }

public class EncryptedStringSerializer(IMilvaCryptographyProvider encrypter) : SerializerBase<EncryptedString>, IEncryptedStringSerializer
{
    private readonly IMilvaCryptographyProvider _encrypter = encrypter;

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
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member