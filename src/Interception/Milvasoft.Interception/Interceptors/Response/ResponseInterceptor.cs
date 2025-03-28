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
    public int InterceptionOrder { get; set; } = int.MaxValue;

    /// <inheritdoc/>
    public async Task OnInvoke(Call call)
    {
        await call.NextAsync();

        if (!_interceptionOptions.MetadataCreationEnabled && !_interceptionOptions.ApplyMetadataRules)
            return;

        var excludeAttribute = call.Method.GetCustomAttribute<ExcludeFromMetadataAttribute>() ?? call.MethodImplementation.GetCustomAttribute<ExcludeFromMetadataAttribute>();

        if (call.ReturnType.CanAssignableTo(typeof(IResponse)))
        {
            if (excludeAttribute is null && call.ReturnType.CanAssignableTo(typeof(IHasMetadata)))
            {
                if (call.ReturnValue is not IHasMetadata hasMetadataResponse)
                    return;

                var metadataGenerator = new ResponseMetadataGenerator(_serviceProvider);

                metadataGenerator.GenerateMetadata(hasMetadataResponse);
            }

            var response = call.ReturnValue as IResponse;

            if (_interceptionOptions.TranslateResultMessages && !response.Messages.IsNullOrEmpty())
                TranslateResultMessages(response);
        }
    }

    /// <summary>
    /// Translates <see cref="IResponse.Messages"/> <see cref="ResponseMessage.Message"/> property.
    /// </summary>
    /// <param name="response"></param>
    private void TranslateResultMessages(IResponse response)
    {
        var localizer = _serviceProvider.GetService<IMilvaLocalizer>();

        if (localizer != null)
            foreach (var message in response.Messages)
                message.Message = localizer[message.Message];

    }
}