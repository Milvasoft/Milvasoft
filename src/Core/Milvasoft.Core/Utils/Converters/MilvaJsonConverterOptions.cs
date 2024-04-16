using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Milvasoft.Core.Utils.Converters;

/// <summary>
/// Provides options for serialization and deserialization operations used by the library.
/// </summary>
public static class MilvaJsonConverterOptions
{
    private static readonly object _statObjLocker = new();

    /// <summary>
    /// Gets or sets the current <see cref="JsonSerializerOptions"/> used by the library.
    /// </summary>
    public static JsonSerializerOptions Current { get; private set; } = JsonSerializerOptions.Default;

    /// <summary>
    /// Resets the current <see cref="JsonSerializerOptions"/> to the default options.
    /// </summary>
    public static void ResetCurrentOptionsToDefault()
    {
        lock (_statObjLocker)
        {
            Current = JsonSerializerOptions.Default;
        }
    }

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
    /// <param name="includeMilvaOptions">A boolean value indicating whether to include Milva converters.</param>
    /// <returns>The configured <see cref="JsonSerializerOptions"/>.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
    public static JsonSerializerOptions ConfigureCurrentMilvaJsonSerializerOptions(this IServiceCollection services, Action<JsonSerializerOptions> options = null, bool includeMilvaOptions = true)
    {
        var newOptions = NewOptionsObjectFromCurrent();

        options?.Invoke(newOptions);

        if (includeMilvaOptions)
        {
            newOptions.Converters.Add(new ExceptionConverter<Exception>());
            newOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        }

        lock (_statObjLocker)
        {
            Current = newOptions;
        }

        return newOptions;
    }
}
