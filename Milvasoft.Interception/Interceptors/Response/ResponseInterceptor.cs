using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Attributes.Annotations;
using Milvasoft.Components.Rest.Response;
using Milvasoft.Core.Abstractions.Localization;
using Milvasoft.Core.Extensions;
using Milvasoft.Interception.Decorator;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace Milvasoft.Interception.Interceptors.Response;

public class ResponseInterceptor(IServiceProvider serviceProvider, IResponseInterceptionOptions interceptionOptions) : IMilvaInterceptor
{
    public static int InterceptionOrder { get; set; } = int.MaxValue;

    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IResponseInterceptionOptions _interceptionOptions = interceptionOptions;

    public async Task OnInvoke(Call call)
    {
        await call.NextAsync();

        var returnValueType = call?.ReturnValue?.GetType();

        if (returnValueType.IsAssignableTo(typeof(IResponse)))
        {
            if (returnValueType.IsAssignableTo(typeof(IHasMetadata)))
            {
                var hasMetadataResponse = call.ReturnValue as IHasMetadata;

                hasMetadataResponse.Metadatas = [];

                //Gets result data and generic type
                var (responseData, resultDataType) = hasMetadataResponse.GetResponseData();

                if (!resultDataType.IsClass)
                    return;

                CreateMetadata(hasMetadataResponse, resultDataType, responseData);
            }

            var response = call.ReturnValue as IResponse;

            if (_interceptionOptions.TranslateResultMessages && !response.Messages.IsNullOrEmpty())
            {
                TranslateResultMessages(response);
            }
        }
    }

    /// <summary>
    /// Creates metadata recursively and assigns to <see cref="IHasMetadata.Metadatas"/>.
    /// </summary>
    /// <param name="response"></param>
    /// <param name="resultDataType"></param>
    /// <param name="propObject"></param>
    private void CreateMetadata(IHasMetadata response, Type resultDataType, object propObject)
    {
        bool dataTypeIsCollection = resultDataType.IsGenericType && typeof(IList).IsAssignableFrom(resultDataType);

        resultDataType = dataTypeIsCollection ? resultDataType.GetGenericArguments()[0] : resultDataType;

        var properties = resultDataType.GetProperties();

        if (properties == null || properties.Length == 0)
            return;

        foreach (var prop in properties)
        {
            ResponseDataMetadata metadata = new()
            {
                Metadatas = []
            };

            if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                CreateMetadata(metadata, prop.PropertyType, propObject);

            //Get used attributes
            var browsableAttribute = prop.GetCustomAttribute<BrowsableAttribute>();
            var hideByRoleAttribute = prop.GetCustomAttribute<HideByRoleAttribute>();
            var maskByRoleAttribute = prop.GetCustomAttribute<MaskByRoleAttribute>();
            var filterableAttribute = prop.GetCustomAttribute<FilterableAttribute>();
            var pinnedAttribute = prop.GetCustomAttribute<PinnedAttribute>();
            var decimalDigitAttribute = prop.GetCustomAttribute<DecimalPrecisionAttribute>();
            var cellTooltipFormatAttribute = prop.GetCustomAttribute<CellTooltipFormatAttribute>();
            var cellDisplayFormatAttribute = prop.GetCustomAttribute<CellDisplayFormatAttribute>();
            var defaultValueAttribute = prop.GetCustomAttribute<DefaultValueAttribute>();


            bool removePropMetadataFromResponse = false;
            bool mask = false;

            if (hideByRoleAttribute != null && hideByRoleAttribute.Roles.Length != 0 && (_interceptionOptions.HideByRoleFunc?.Invoke(hideByRoleAttribute) ?? false))
                removePropMetadataFromResponse = true;

            if (maskByRoleAttribute != null && maskByRoleAttribute.Roles.Length != 0 == false && (_interceptionOptions.HideByRoleFunc?.Invoke(hideByRoleAttribute) ?? false))
                mask = true;

            var hasResponseData = propObject != null;
            var translateAttribute = _interceptionOptions.TranslateMetadata ? resultDataType.GetCustomAttribute<TranslateAttribute>() : null;
            var localizer = translateAttribute != null ? _serviceProvider.GetService<IMilvaLocalizer>() : null;
            string localizedName = prop.Name;

            //Apply localization to property
            if (translateAttribute != null && localizer != null)
                localizedName = _interceptionOptions.ApplyLocalizationFunc == null
                                         ? ApplyLocalization(translateAttribute.Key, localizer, resultDataType, prop.Name)
                                         : _interceptionOptions.ApplyLocalizationFunc.Invoke(translateAttribute.Key, localizer, resultDataType, prop.Name);

            //Fill metadata object
            metadata.Name = prop.Name;
            metadata.LocalizedName = localizedName;
            metadata.Type = prop.PropertyType.Name;
            metadata.Display = browsableAttribute == null || browsableAttribute.Browsable;
            metadata.DefaultValue = defaultValueAttribute?.Value;
            metadata.Mask = mask;
            metadata.Filterable = filterableAttribute == null || filterableAttribute.Filterable;
            metadata.Pinned = pinnedAttribute != null && pinnedAttribute.Pinned;
            metadata.DecimalPrecision = decimalDigitAttribute?.DecimalPrecision;
            metadata.CellTooltipFormat = cellTooltipFormatAttribute?.Format;
            metadata.CellDisplayFormat = cellDisplayFormatAttribute?.Format;

            if (!removePropMetadataFromResponse)
                response.Metadatas.Add(metadata);

            if (hasResponseData)
            {
                if (dataTypeIsCollection)
                {
                    foreach (var item in propObject as IList)
                        ApplyMetadataRulesToResponseData(item, prop, metadata, removePropMetadataFromResponse);
                }
                else ApplyMetadataRulesToResponseData(propObject, prop, metadata, removePropMetadataFromResponse);
            }
        }
    }

    private static void ApplyMetadataRulesToResponseData(object responseObject, PropertyInfo prop, ResponseDataMetadata metadata, bool removeFromResponse)
    {
        if (removeFromResponse)
        {
            object defaultValue = prop.PropertyType.IsValueType ? Activator.CreateInstance(prop.PropertyType) : null;

            prop.SetValue(responseObject, defaultValue);

            return;
        }

        //Mask
        if (metadata.Mask && prop.PropertyType == typeof(string))
        {
            string propertyValue = (string)prop.GetValue(responseObject);

            if (propertyValue != null)
            {
                int showCharCount = Convert.ToInt32(Math.Floor(propertyValue.Length * 0.25M));

                propertyValue = $"{propertyValue[..showCharCount]}" + $"{new string('*', propertyValue.Length - showCharCount * 2)}" + $"{propertyValue[^showCharCount..]}";

                prop.SetValue(responseObject, propertyValue);
            }
        }
    }

    /// <summary>
    /// Default localization method which called by response inteceptor.
    /// </summary>
    /// <param name="localizationKey">Localization key which gets from <see cref="TranslateAttribute"/> </param>
    /// <param name="localizer"><see cref="IMilvaLocalizer"/> comes from ServiceProvider via DI.</param>
    /// <param name="resultDataType">Intercepted method return data type.</param>
    /// <param name="prop">Property name to which localization will be applied.</param>
    /// <returns></returns>
    public static string ApplyLocalization(string localizationKey, IMilvaLocalizer localizer, Type resultDataType, string propName)
    {
        var key = $"{localizationKey ?? resultDataType.Name}.{propName}";

        string localizedName = localizer[key];

        localizedName ??= localizer[$"Global.{propName}"];

        localizedName ??= propName;

        return localizedName;
    }


    private void TranslateResultMessages(IResponse response)
    {
        var localizer = _serviceProvider.GetService<IMilvaLocalizer>();

        if (localizer != null)
            foreach (var message in response.Messages)
                message.Message = localizer[message.Message];

    }
}