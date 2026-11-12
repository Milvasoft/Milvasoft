using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Attributes.Annotations;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Interception.Decorator;
using System.Reflection;

namespace Milvasoft.Interception.Interceptors.Response;

/// <summary>
/// Allows modification of the return values of methods that can be assigned to the <see cref="IResponse"/>.
/// </summary>
public class ResponseInterceptor(IServiceProvider serviceProvider, IResponseInterceptionOptions interceptionOptions) : IMilvaInterceptor
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IResponseInterceptionOptions _interceptionOptions = interceptionOptions;

    /// <inheritdoc/>
    /// <remarks>
    /// Default is 0: inside <see cref="Logging.LogInterceptor"/> (-1) so the processed response is logged, and outside <see cref="Cache.CacheInterceptor"/> (1)
    /// so the raw result is cached and role/culture dependent processing runs for every caller, including cache hits.
    /// </remarks>
    public int InterceptionOrder { get; set; } = 0;

    /// <inheritdoc/>
    public async Task OnInvoke(Call call)
    {
        await call.NextAsync();

        if (!_interceptionOptions.MetadataCreationEnabled && !_interceptionOptions.ApplyMetadataRules)
            return;

        var excludeAttribute = call.Method.GetCustomAttribute<ExcludeFromMetadataAttribute>() ?? call.MethodImplementation.GetCustomAttribute<ExcludeFromMetadataAttribute>();

        // The runtime value is checked, so Task<T>/ValueTask<T> returning methods and null results are handled.
        if (call.ReturnValue is not IResponse response)
            return;

        if (excludeAttribute is null && response is IHasMetadata hasMetadataResponse)
        {
            var metadataGenerator = new ResponseMetadataGenerator(_serviceProvider);

            await metadataGenerator.GenerateMetadataAsync(hasMetadataResponse);
        }

        if (_interceptionOptions.TranslateResultMessages && !response.Messages.IsNullOrEmpty())
            TranslateResultMessages(response);
    }

    /// <summary>
    /// Translates <see cref="IResponse.Messages"/> <see cref="ResponseMessage.Message"/> property.
    /// The untranslated value is preserved in <see cref="ResponseMessage.Key"/> (when it is empty) so callers
    /// can still reason about the original message key after localization (e.g. mapping keys to HTTP status codes).
    /// </summary>
    /// <param name="response"></param>
    private void TranslateResultMessages(IResponse response)
    {
        var localizer = _serviceProvider.GetService<IMilvaLocalizer>();

        if (localizer != null)
            foreach (var message in response.Messages)
            {
                if (message == null || string.IsNullOrEmpty(message.Message))
                    continue;

                if (string.IsNullOrEmpty(message.Key))
                    message.Key = message.Message;

                // A missing resource keeps the original message instead of replacing it with null.
                message.Message = (string)localizer[message.Message] ?? message.Message;
            }
    }
}