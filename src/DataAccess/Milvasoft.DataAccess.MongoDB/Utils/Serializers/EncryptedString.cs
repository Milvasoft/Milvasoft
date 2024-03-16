namespace Milvasoft.DataAccess.MongoDB.Utils.Serializers;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
/// <summary>
/// If property type is EncryptedString, client side encryption is applied to property.
/// </summary>
/// <remarks>
/// Initializes new <see cref="EncryptedString"/>.
/// </remarks>
/// <param name="value"></param>
[Serializable]
public readonly struct EncryptedString(string value)
{
    private readonly string _value = value;

    public static implicit operator string(EncryptedString s) => s._value;

    public static explicit operator EncryptedString(string value) => new(value);

    public readonly bool IsNullOrWhiteSpace() => string.IsNullOrWhiteSpace(_value);
    public override readonly string ToString() => $"{_value}";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

