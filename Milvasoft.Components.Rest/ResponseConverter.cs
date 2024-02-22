using Milvasoft.Components.Rest.Response;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Milvasoft.Core.Utils.JsonConverters;

/// <summary>
/// Converter for deserializing generic interfaces with System.Text.Json. 
/// </summary>
/// <typeparam name="TImplementation"></typeparam>
/// <typeparam name="TInterface"></typeparam>
internal class ResponseConverter<TImplementation, TInterface> : JsonConverter<TInterface> where TImplementation : class, TInterface
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
/// <param name="implementationType"></param>
/// <param name="interfaceType"></param>
internal class ResponseConverterFactory() : JsonConverterFactory
{
    /// <summary>
    /// Implementation type.
    /// </summary>
    public Type ImplementationType { get; set; }

    /// <summary>
    /// Interface type.
    /// </summary>
    public Type InterfaceType { get; set; }

    /// <summary>
    /// Determnies whether <paramref name="typeToConvert"/> is convertible to interface type or not.
    /// </summary>
    /// <param name="typeToConvert"></param>
    /// <returns></returns>
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsInterface && typeToConvert.IsAssignableTo(typeof(IResponse)) && typeToConvert.IsGenericType;

    /// <summary>
    /// Creates json converter.
    /// </summary>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(ResponseConverter<,>).MakeGenericType(typeof(Response<>).MakeGenericType(typeToConvert.GenericTypeArguments[0]), InterfaceType);

        return Activator.CreateInstance(converterType) as JsonConverter;
    }
}