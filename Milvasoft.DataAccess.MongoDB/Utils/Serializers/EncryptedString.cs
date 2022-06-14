namespace Milvasoft.DataAccess.MongoDB.Utils.Serializers;

/// <summary>
/// If property type is EncryptedString, client side encryption is applied to property.
/// </summary>
[Serializable]
public struct EncryptedString
{
    private readonly string _value;

    /// <summary>
    /// Initializes new <see cref="EncryptedString"/>.
    /// </summary>
    /// <param name="value"></param>
    public EncryptedString(string value)
    {
        _value = value;
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static implicit operator string(EncryptedString s) => s._value;

    public static explicit operator EncryptedString(string value) => new(value);

    public bool IsNullOrWhiteSpace() => string.IsNullOrWhiteSpace(_value);
    public override string ToString() => $"{_value}";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

