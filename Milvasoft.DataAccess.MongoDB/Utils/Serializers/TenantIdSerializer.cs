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

            if (s.Equals("N/A", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            else if (context.Reader.CurrentBsonType == BsonType.Null)
            {
                context.Reader.ReadNull();
                return null;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(s))
                    return null;

                return TenantId.Parse(s);
            }
        }
        else if (type == BsonType.Null)
        {
            context.Reader.ReadNull();
            return null;
        }
        else
            return BsonSerializer.Deserialize<TenantId?>(context.Reader);
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
