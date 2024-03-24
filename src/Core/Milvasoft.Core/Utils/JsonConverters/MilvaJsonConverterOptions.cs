using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Milvasoft.Core.Utils.JsonConverters;

/// <summary>
/// Provides options for serialization and deserialization operations used by the library.
/// </summary>
public static class MilvaJsonConverterOptions
{
    /// <summary>
    /// Gets or sets the current <see cref="JsonSerializerOptions"/> used by the library.
    /// </summary>
    public static JsonSerializerOptions Current { get; private set; } = JsonSerializerOptions.Default;

    /// <summary>
    /// Resets the current <see cref="JsonSerializerOptions"/> to the default options.
    /// </summary>
    public static void ResetCurrentOptionsToDefault() => Current = JsonSerializerOptions.Default;

    /// <summary>
    /// Creates a new <see cref="JsonSerializerOptions"/> instance based on the current <see cref="Current"/> object.
    /// </summary>
    /// <returns>A new <see cref="JsonSerializerOptions"/> instance.</returns>
    public static JsonSerializerOptions NewOptionsObjectFromCurrent() => new(Current);

    /// <summary>
    /// Configures the <see cref="Current"/> options with the specified <paramref name="options"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> used for dependency injection.</param>
    /// <param name="options">The <see cref="Action{JsonSerializerOptions}"/> used to configure the options.</param>
    /// <param name="includeMilvaConverters">A boolean value indicating whether to include Milva converters.</param>
    /// <returns>The configured <see cref="JsonSerializerOptions"/>.</returns>
    public static JsonSerializerOptions ConfigureCurrentMilvaJsonSerializerOptions(this IServiceCollection services, Action<JsonSerializerOptions> options = null, bool includeMilvaConverters = true)
    {
        ArgumentNullException.ThrowIfNull(services);

        var newOptions = NewOptionsObjectFromCurrent();

        options?.Invoke(newOptions);

        if (includeMilvaConverters)
            newOptions.Converters.Add(new ExceptionConverter<Exception>());

        Current = newOptions;

        return newOptions;
    }
}
