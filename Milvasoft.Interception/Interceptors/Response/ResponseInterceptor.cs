﻿using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Attributes.Annotations;
using Milvasoft.Components.Rest.Response;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Decorator;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace Milvasoft.Interception.Interceptors.Logging;

public class ResponseInterceptor : IMilvaInterceptor
{
    public int InterceptionOrder { get; set; } = 999;

    private IServiceProvider _serviceProvider;
    private readonly IResponseInterceptionOptions _interceptionOptions;

    public ResponseInterceptor(IServiceProvider serviceProvider, IResponseInterceptionOptions interceptionOptions)
    {
        _serviceProvider = serviceProvider;
        _interceptionOptions = interceptionOptions;
    }

    public async Task OnInvoke(Call call)
    {
        await call.NextAsync();

        if (call.ReturnValue.GetType().IsAssignableTo(typeof(IHasMetadata)))
        {
            var response = (IHasMetadata)call.ReturnValue;

            FillResponseMetadata(response);
        }

    }

    private void FillResponseMetadata(IHasMetadata response)
    {
        response.Metadata = [];

        var (resultData, resultDataType) = response.GetResponseData();

        if (!resultDataType.IsClass)
            return;

        bool dataTypeIsList = false;

        if (resultDataType.IsGenericType && typeof(IList).IsAssignableFrom(resultDataType))
        {
            resultDataType = resultDataType.GetGenericArguments()[0];

            dataTypeIsList = true;
        }

        var properties = resultDataType.GetProperties();

        if (properties == null || properties.Length == 0)
            return;

        var hasResult = resultData != null;

        var translateAttribute = resultDataType.GetCustomAttribute<TranslateAttribute>();
        var localizer = translateAttribute != null ? _serviceProvider.GetService<IMilvaLocalizer>() : null;

        foreach (var prop in properties)
        {
            var propertyType = prop.PropertyType.IsGenericType ? prop.PropertyType.GetGenericArguments()[0] : prop.PropertyType;

            var browsableAttribute = prop.GetCustomAttribute<BrowsableAttribute>();
            var hideByRoleAttribute = prop.GetCustomAttribute<HideByRoleAttribute>();
            var maskByRoleAttribute = prop.GetCustomAttribute<MaskByRoleAttribute>();
            var filterableAttribute = prop.GetCustomAttribute<FilterableAttribute>();
            var pinnedAttribute = prop.GetCustomAttribute<PinnedAttribute>();
            var decimalDigitAttribute = prop.GetCustomAttribute<DecimalPrecisionAttribute>();
            var cellTooltipFormatAttribute = prop.GetCustomAttribute<CellTooltipFormatAttribute>();
            var cellDisplayFormatAttribute = prop.GetCustomAttribute<CellDisplayFormatAttribute>();
            var defaultValueAttribute = prop.GetCustomAttribute<DefaultValueAttribute>();

            bool removeFromResponse = false;
            bool mask = false;
            DecimalPrecision decimalPrecision = decimalDigitAttribute?.DecimalPrecision;
            string cellTooltipFormat = cellTooltipFormatAttribute?.Format;
            string cellDisplayFormat = cellDisplayFormatAttribute?.Format;

            if (hideByRoleAttribute != null && (hideByRoleAttribute.Roles.Length != 0 /*&& listRoleName.Any(r => hideByRoleAttribute.Roles.Contains(r))*/))
                removeFromResponse = true;

            if (maskByRoleAttribute != null && (maskByRoleAttribute.Roles.Length != 0 == false /*&& listRoleName.Any(r => maskByRoleAttribute.Roles.Contains(r))*/))
                mask = true;

            string localizedName = prop.Name;

            if (translateAttribute != null && localizer != null)
            {
                localizedName = _interceptionOptions.LocalizationMethod == null
                                         ? ApplyLocalization(translateAttribute.Key, localizer, resultDataType, prop.Name)
                                         : _interceptionOptions.LocalizationMethod.Invoke(translateAttribute.Key, localizer, resultDataType, prop.Name);
            }

            var metadata = new ResponseDataMetadata()
            {
                Name = prop.Name,
                LocalizedName = localizedName,
                Type = propertyType.Name,
                Display = browsableAttribute == null || browsableAttribute.Browsable,
                RemoveFromResponse = removeFromResponse,
                DefaultValue = defaultValueAttribute?.Value,
                Mask = mask,
                Filterable = filterableAttribute == null || filterableAttribute.Filterable,
                Pinned = pinnedAttribute == null || pinnedAttribute.Pinned,
                DecimalPrecision = decimalPrecision,
                CellTooltipFormat = cellTooltipFormat,
                CellDisplayFormat = cellDisplayFormat,
            };

            if (!removeFromResponse)
                response.Metadata.Add(metadata);

            if (hasResult)
            {
                if (dataTypeIsList)
                {
                    var list = resultData as IList;

                    foreach (var item in list)
                        ApplyMetadataRulesToResponseData(item, prop, metadata);
                }
                else
                {
                    ApplyMetadataRulesToResponseData(resultData, prop, metadata);
                }
            }
        }

        return;
    }

    private static void ApplyMetadataRulesToResponseData(object responseObject, PropertyInfo prop, ResponseDataMetadata metadata)
    {
        if (metadata.RemoveFromResponse)
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

                propertyValue = $"{propertyValue[..showCharCount]}" + $"{new string('*', propertyValue.Length - (showCharCount * 2))}" + $"{propertyValue[^showCharCount..]}";

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

}