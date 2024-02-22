using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Milvasoft.Core.Utils.JsonConverters;

public static class MilvaJsonConverterOptions
{
    public static JsonSerializerOptions Current { get; set; } = new JsonSerializerOptions();

    public static JsonSerializerOptions NewOptionsObjectFromCurrent() => new(Current);

    public static JsonSerializerOptions ConfigureCurrentMilvaJsonSerializerOptions(this IServiceCollection services, Action<JsonSerializerOptions> options = null, bool includeMilvaConverters = true)
    {
        options?.Invoke(Current);

        if (includeMilvaConverters)
        {
            Current.Converters.Add(new ExceptionConverter<Exception>());
        }

        return Current;
    }
}
