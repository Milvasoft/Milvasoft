using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Milvasoft.UnitTests.CoreTests.UtilTests.JsonConverterTests.Helpers;

public static class JsonConverterTestExtensions
{
    public static TResult? Read<TResult>(this JsonConverter<TResult> converter,
                                         string token,
                                         JsonSerializerOptions options = null)
    {
        options ??= JsonSerializerOptions.Default;

        var bytes = Encoding.UTF8.GetBytes(token);

        var reader = new Utf8JsonReader(bytes);

        // advance to token
        reader.Read();

        var result = converter.Read(ref reader, typeof(TResult), options);

        return result;
    }

    public static (bool IsSuccessful, TResult? Result) TryRead<TResult>(this JsonConverter<TResult> converter,
                                                                        string token,
                                                                        JsonSerializerOptions options = null)
    {
        try
        {
            var result = converter.Read(token, options);

            return (true, result);
        }
        catch (Exception)
        {
            return (IsSuccessful: false, Result: default);
        }
    }

    public static string Write<T>(this JsonConverter<T> converter,
                                  T value,
                                  JsonSerializerOptions options = null)
    {
        options ??= JsonSerializerOptions.Default;

        using var ms = new MemoryStream();

        using var writer = new Utf8JsonWriter(ms);

        converter.Write(writer, value, options);

        writer.Flush();

        var result = Encoding.UTF8.GetString(ms.ToArray());

        return result;
    }

    public static (bool IsSuccessful, string Result) TryWrite<T>(this JsonConverter<T> converter,
                                                                 T value,
                                                                 JsonSerializerOptions options = null)
    {
        try
        {
            var result = converter.Write(value, options);

            return (true, result);
        }
        catch
        {
            return (false, null);
        }
    }
}