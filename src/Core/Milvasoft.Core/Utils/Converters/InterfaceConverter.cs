using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Milvasoft.Core.Utils.Converters;

/// <summary>
/// Converter for deserializing generic interfaces with System.Text.Json. 
/// </summary>
/// <typeparam name="TImplementation">The implementation type.</typeparam>
/// <typeparam name="TInterface">The interface type.</typeparam>
public class InterfaceConverter<TImplementation, TInterface> : JsonConverter<TInterface> where TImplementation : class, TInterface
{
    /// <summary>
    /// Deserializes the JSON data to the specified type.
    /// </summary>
    /// <param name="reader">The reader used to read the JSON data.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The deserialized object.</returns>
    public override TImplementation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => JsonSerializer.Deserialize<TImplementation>(ref reader);

    /// <summary>
    /// Serializes the specified object to JSON.
    /// </summary>
    /// <param name="writer">The writer used to write the JSON data.</param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, TInterface value, JsonSerializerOptions options) => JsonSerializer.Serialize(writer, (TImplementation)value);
}

/// <summary>
/// Converter factory for deserializing generic interfaces with System.Text.Json. 
/// </summary>
/// <typeparam name="TImplementation">The implementation type.</typeparam>
/// <typeparam name="TInterface">The interface type.</typeparam>
public class InterfaceConverterFactory<TImplementation, TInterface> : InterfaceConverterFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InterfaceConverterFactory{TImplementation, TInterface}"/> class.
    /// </summary>
    public InterfaceConverterFactory() : base(typeof(TImplementation), typeof(TInterface))
    {
    }
}

/// <summary>
/// Converter factory for deserializing generic interfaces with System.Text.Json. 
/// </summary>
/// <param name="implementationType">The implementation type.</param>
/// <param name="interfaceType">The interface type.</param>
public class InterfaceConverterFactory(Type implementationType, Type interfaceType) : JsonConverterFactory
{
    private static readonly ConcurrentDictionary<Type, Func<JsonConverter>> _converterCache = new();

    /// <summary>
    /// Gets the implementation type.
    /// </summary>
    public Type ImplementationType { get; } = implementationType;

    /// <summary>
    /// Gets the interface type.
    /// </summary>
    public Type InterfaceType { get; } = interfaceType;

    /// <summary>
    /// Determines whether the specified type can be converted to the interface type.
    /// </summary>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <returns><c>true</c> if the specified type can be converted to the interface type; otherwise, <c>false</c>.</returns>
    public override bool CanConvert(Type typeToConvert) => (typeToConvert.IsGenericType && InterfaceType.IsGenericType || !typeToConvert.IsGenericType && !InterfaceType.IsGenericType)
                                                           && typeToConvert.CanAssignableTo(InterfaceType);

    /// <summary>
    /// Creates a converter for the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>A converter for the specified type.</returns>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type converterType;

        if (typeToConvert.IsGenericType && ImplementationType.IsGenericType)
        {
            converterType = typeof(InterfaceConverter<,>).MakeGenericType(
                ImplementationType.MakeGenericType(typeToConvert.GenericTypeArguments),
                InterfaceType.MakeGenericType(typeToConvert.GenericTypeArguments)
            );
        }
        else
        {
            converterType = typeof(InterfaceConverter<,>).MakeGenericType(ImplementationType, InterfaceType);
        }

        return _converterCache.GetOrAdd(converterType, CreateFactory)();
    }

    private static Func<JsonConverter> CreateFactory(Type type)
    {
        var ctor = Expression.New(type);

        var lambda = Expression.Lambda<Func<JsonConverter>>(ctor);

        return lambda.Compile();
    }
}