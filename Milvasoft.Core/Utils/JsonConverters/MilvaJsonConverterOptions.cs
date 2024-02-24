using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Milvasoft.Core.Utils.JsonConverters;

/// <summary>
/// Options that the library will use for serialization and deserialization operations.
/// </summary>
public static class MilvaJsonConverterOptions
{
    /// <summary>
    /// Current options.
    /// </summary>
    public static JsonSerializerOptions Current { get; set; } = new JsonSerializerOptions();

    /// <summary>
    /// Create new <see cref="JsonSerializerOptions"/> instance from <see cref="Current"/> object.
    /// </summary>
    /// <returns></returns>
    public static JsonSerializerOptions NewOptionsObjectFromCurrent() => new(Current);

    /// <summary>
    /// Configure <see cref="Current"/> options with <paramref name="options"/>.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="options"></param>
    /// <param name="includeMilvaConverters"></param>
    /// <returns></returns>
    public static JsonSerializerOptions ConfigureCurrentMilvaJsonSerializerOptions(this IServiceCollection services, Action<JsonSerializerOptions> options = null, bool includeMilvaConverters = true)
    {
        ArgumentNullException.ThrowIfNull(services);

        options?.Invoke(Current);

        if (includeMilvaConverters)
        {
            Current.Converters.Add(new ExceptionConverter<Exception>());
        }

        return Current;
    }
}
