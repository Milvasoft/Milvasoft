using Milvasoft.Core.EntityBases.MultiTenancy;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Milvasoft.DataAccess.MongoDB.Utils.Serializers;

/// <summary>
/// Serializer for <see cref="TenantId"/>.
/// </summary>
public class TenantIdSerializer : SerializerBase<TenantId>
{
    /// <summary>
    /// Deserializes a value.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public override TenantId Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var type = context.Reader.GetCurrentBsonType();

        if (type == BsonType.String)
        {
            var s = context.Reader.ReadString();

            if (s.Equals("N/A", StringComparison.InvariantCultureIgnoreCase) || string.IsNullOrWhiteSpace(s))
            {
                return TenantId.Empty;
            }
            else
            {
                return TenantId.Parse(s);
            }
        }
        else
        {
            // Consume the value, otherwise the reader is left on it and the rest of the document can not be read.
            context.Reader.SkipValue();

            return TenantId.Empty;
        }
    }

    /// <summary>
    /// Serializes a value.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="args"></param>
    /// <param name="value"></param>
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TenantId value) => context.Writer.WriteString(value);
}

/// <summary>
/// Serializer for nullable <see cref="TenantId"/>.
/// </summary>
public class NullableTenantIdSerializer : SerializerBase<TenantId?>
{
    /// <summary>
    /// Deserializes a value.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public override TenantId? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var type = context.Reader.GetCurrentBsonType();

        if (type == BsonType.String)
        {
            var s = context.Reader.ReadString();

            if (string.IsNullOrWhiteSpace(s) || s.Equals("N/A", StringComparison.InvariantCultureIgnoreCase))
                return null;

            return TenantId.Parse(s);
        }

        // Consume the value (null or an unsupported type). Deserializing TenantId? again through BsonSerializer would resolve this serializer and recurse forever.
        context.Reader.SkipValue();

        return null;
    }

    /// <summary>
    /// Serializes a value.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="args"></param>
    /// <param name="value"></param>
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TenantId? value)
    {
        if (value != null)
            context.Writer.WriteString(value);
        else
            context.Writer.WriteNull();
    }
}
