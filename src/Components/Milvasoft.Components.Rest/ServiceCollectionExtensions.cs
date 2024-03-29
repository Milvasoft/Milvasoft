using Milvasoft.Components.Rest.Response;
using Milvasoft.Core.Utils.JsonConverters;
using System.Text.Json;

namespace Milvasoft.Components.Rest;
public static partial class RestExtensions
{
    public static JsonSerializerOptions AddResponseConverters(this JsonSerializerOptions options)
    {
        options.Converters.Add(new InterfaceConverterFactory<Response.Response, IResponse>());

        return options;
    }
}
