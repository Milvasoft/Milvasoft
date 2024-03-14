using System.Text.Json;
using System.Text.Json.Serialization;

namespace Milvasoft.Core.Utils.JsonConverters;

/// <summary>
/// Converter for deserializing generic interfaces with System.Text.Json. 
/// </summary>
/// <typeparam name="TImplementation"></typeparam>
/// <typeparam name="TInterface"></typeparam>
public class InterfaceConverter<TImplementation, TInterface> : JsonConverter<TInterface> where TImplementation : class, TInterface
{
    /// <summary>
    /// Deserialization operation.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override TInterface Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => JsonSerializer.Deserialize<TImplementation>(ref reader, options);

    /// <summary>
    /// Serialization operation.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, TInterface value, JsonSerializerOptions options)
    {
    }
}

/// <summary>
/// Converter factory for deserializing generic interfaces with System.Text.Json. 
/// </summary>
/// <typeparam name="TImplementation"></typeparam>
/// <typeparam name="TInterface"></typeparam>
public class InterfaceConverterFactory<TImplementation, TInterface> : InterfaceConverterFactory
{
    /// <summary>
    /// Initializes new instance.
    /// </summary>
    public InterfaceConverterFactory() : base(typeof(TImplementation), typeof(TInterface))
    {
    }
}

/// <summary>
/// Converter factory for deserializing generic interfaces with System.Text.Json. 
/// </summary>
/// <param name="implementationType"></param>
/// <param name="interfaceType"></param>
public class InterfaceConverterFactory(Type implementationType, Type interfaceType) : JsonConverterFactory
{
    /// <summary>
    /// Implementation type.
    /// </summary>
    public Type ImplementationType { get; } = implementationType;

    /// <summary>
    /// Interface type.
    /// </summary>
    public Type InterfaceType { get; } = interfaceType;

    /// <summary>
    /// Determnies whether <paramref name="typeToConvert"/> is convertible to interface type or not.
    /// </summary>
    /// <param name="typeToConvert"></param>
    /// <returns></returns>
    public override bool CanConvert(Type typeToConvert)
    {
        var q = typeToConvert == InterfaceType || (typeToConvert.IsAssignableTo(InterfaceType) && typeToConvert.IsGenericType);

        return q;
    }

    /// <summary>
    /// Creates json converter.
    /// </summary>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type converterType;

        if (typeToConvert.IsGenericType && ImplementationType.IsGenericType)
            converterType = typeof(InterfaceConverter<,>).MakeGenericType(ImplementationType.MakeGenericType(typeToConvert.GenericTypeArguments[0]), InterfaceType);
        else
            converterType = typeof(InterfaceConverter<,>).MakeGenericType(ImplementationType, InterfaceType);

        return Activator.CreateInstance(converterType) as JsonConverter;
    }
}