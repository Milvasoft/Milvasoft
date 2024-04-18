namespace Milvasoft.DataAccess.MongoDB.Utils.Serializers;

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

    /// <summary>
    /// Provides implicit casting.
    /// </summary>
    /// <param name="encryptedString"></param>
    public static implicit operator string(EncryptedString encryptedString) => encryptedString._value;

    /// <summary>
    /// Provides explicit casting.
    /// </summary>
    /// <param name="value"></param>
    public static explicit operator EncryptedString(string value) => new(value);

    /// <summary>
    /// Checks if the value of the EncryptedString is null or consists only of white space characters.
    /// </summary>
    /// <returns>True if the value is null or consists only of white space characters, otherwise false.</returns>
    public readonly bool IsNullOrWhiteSpace() => string.IsNullOrWhiteSpace(_value);

    /// <inheritdoc/>
    public override readonly string ToString() => $"{_value}";
}

