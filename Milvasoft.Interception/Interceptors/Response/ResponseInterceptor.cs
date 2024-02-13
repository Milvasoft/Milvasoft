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

                if (!resultDataType.IsClass && resultDataType.Namespace.Contains(nameof(System)))
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

        if (resultDataType.Namespace.Contains(nameof(System)))
        {
            CreatePropMetadata(response.GetType().GetProperty("Data"));
            return;
        }

        var properties = resultDataType.GetProperties();

        if (properties == null || properties.Length == 0)
            return;

        foreach (var prop in properties)
        {
            CreatePropMetadata(prop);
        }

        void CreatePropMetadata(PropertyInfo prop)
        {
            ResponseDataMetadata metadata = new()
            {
                Metadatas = []
            };

            if (prop.PropertyType.IsClass && !resultDataType.Namespace.Contains(nameof(System)))
                CreateMetadata(metadata, prop.PropertyType, propObject);

            metadata.Name = prop.Name;
            metadata.Type = GetPropertyFriendlyName(prop);
            metadata.LocalizedName = prop.Name;

            //If no attribute specified no need to proceed.
            if (!prop.GetCustomAttributes().Any())
            {
                response.Metadatas.Add(metadata);
                return;
            }

            bool removePropMetadataFromResponse = false;
            bool mask = false;

            if (TryGetAttribute(prop, out HideByRoleAttribute hideByRoleAttribute) && hideByRoleAttribute.Roles.Length != 0 && (_interceptionOptions.HideByRoleFunc?.Invoke(hideByRoleAttribute) ?? false))
                removePropMetadataFromResponse = true;

            if (TryGetAttribute(prop, out MaskByRoleAttribute maskByRoleAttribute) && maskByRoleAttribute.Roles.Length != 0 == false && (_interceptionOptions.HideByRoleFunc?.Invoke(hideByRoleAttribute) ?? false))
                mask = true;

            var translateAttribute = _interceptionOptions.TranslateMetadata ? resultDataType.GetCustomAttribute<TranslateAttribute>() : null;
            var localizer = translateAttribute != null ? _serviceProvider.GetService<IMilvaLocalizer>() : null;
            string localizedName = prop.Name;

            //Apply localization to property
            if (translateAttribute != null && localizer != null)
                localizedName = _interceptionOptions.ApplyLocalizationFunc == null
                                         ? ApplyLocalization(translateAttribute.Key, localizer, resultDataType, prop.Name)
                                         : _interceptionOptions.ApplyLocalizationFunc.Invoke(translateAttribute.Key, localizer, resultDataType, prop.Name);

            //Fill metadata object
            metadata.LocalizedName = localizedName;
            metadata.Display = !TryGetAttribute(prop, out BrowsableAttribute browsableAttribute) || browsableAttribute.Browsable;
            metadata.DefaultValue = TryGetAttribute(prop, out DefaultValueAttribute defaultValueAttribute) ? defaultValueAttribute.Value : null; ;
            metadata.Mask = mask;
            metadata.Filterable = !TryGetAttribute(prop, out FilterableAttribute filterableAttribute) || filterableAttribute.Filterable;
            metadata.Pinned = !TryGetAttribute(prop, out PinnedAttribute pinnedAttribute) && pinnedAttribute.Pinned;
            metadata.DecimalPrecision = TryGetAttribute(prop, out DecimalPrecisionAttribute decimalPrecisionAttribute) ? decimalPrecisionAttribute.DecimalPrecision : null;
            metadata.CellTooltipFormat = TryGetAttribute(prop, out CellTooltipFormatAttribute cellTooltipFormatAttribute) ? cellTooltipFormatAttribute.Format : null;
            metadata.CellDisplayFormat = TryGetAttribute(prop, out CellDisplayFormatAttribute cellDisplayFormatAttribute) ? cellDisplayFormatAttribute.Format : null;

            if (!removePropMetadataFromResponse)
                response.Metadatas.Add(metadata);

            if (propObject != null)
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

    /// <summary>
    /// Aplly metadata rules to <see cref="IResponse{T}.Data"/>.
    /// </summary>
    /// <param name="responseObject"></param>
    /// <param name="prop"></param>
    /// <param name="metadata"></param>
    /// <param name="removeFromResponse"></param>
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

    /// <summary>
    /// Gets property user friendly name for generic types.
    /// </summary>
    /// <param name="prop"></param>
    /// <returns></returns>
    private static string GetPropertyFriendlyName(PropertyInfo prop)
        => prop.PropertyType.IsGenericType
            ? $"{prop.PropertyType.Name.Remove(prop.PropertyType.Name.IndexOf('`'))}.{string.Join(',', prop.PropertyType.GetGenericArguments().Select(i => i.Name))}"
            : prop.PropertyType.Name;

    /// <summary>
    /// Gets custom attribute.
    /// </summary>
    /// <typeparam name="TAttribute"></typeparam>
    /// <param name="prop"></param>
    /// <param name="attribute"></param>
    /// <returns></returns>
    public static bool TryGetAttribute<TAttribute>(PropertyInfo prop, out TAttribute attribute) where TAttribute : Attribute
    {
        attribute = prop.GetCustomAttribute<TAttribute>();

        return attribute != null;
    }

}