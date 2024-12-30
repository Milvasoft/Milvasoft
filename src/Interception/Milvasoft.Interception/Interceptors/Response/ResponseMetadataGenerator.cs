using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Attributes.Annotations;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Types.Classes;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace Milvasoft.Interception.Interceptors.Response;

/// <summary>
/// Generates metadata for the response data.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ResponseMetadataGenerator"/> class.
/// </remarks>
/// <param name="responseInterceptionOptions">The response interception options.</param>
/// <param name="serviceProvider">The service provider.</param>
public class ResponseMetadataGenerator(IResponseInterceptionOptions responseInterceptionOptions, IServiceProvider serviceProvider) : IResponseMetadataGenerator
{
    private readonly IResponseInterceptionOptions _interceptionOptions = responseInterceptionOptions;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly bool _generateMetadataFuncResult = (!responseInterceptionOptions.GenerateMetadataFunc?.Invoke(serviceProvider) ?? false);

    /// <summary>
    /// Applies localization to the specified property.
    /// </summary>
    /// <param name="localizationKey">The localization key.</param>
    /// <param name="localizer">The localizer.</param>
    /// <param name="callerObjectType">The result data type.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The localized property name.</returns>
    public static string ApplyLocalization(string localizationKey, IMilvaLocalizer localizer, Type callerObjectType, string propertyName)
    {
        var key = $"{localizationKey ?? callerObjectType.Name}.{propertyName}";

        LocalizedValue localizedValue = localizer[key];

        string localizedName = localizedValue.ResourceFound ? localizedValue : null;

        localizedName ??= localizer[$"Global.{propertyName}"];

        localizedName ??= propertyName;

        return localizedName;
    }

    /// <summary>
    /// Generates metadata for the specified object and assing <paramref name="hasMetadataObject"/>.Metadatas.
    /// </summary>
    /// <param name="hasMetadataObject">The object to generate metadata for.</param>
    /// <returns>The list of generated metadata.</returns>
    public void GenerateMetadata(IHasMetadata hasMetadataObject)
    {
        hasMetadataObject.Metadatas = [];

        var (responseData, resultDataType) = hasMetadataObject.GetResponseDataTypePair();

        var callerObjectInfo = CallerObjectInfo.CreateCallerInformation(responseData, resultDataType);

        if (callerObjectInfo.ReviewedType.Namespace.Contains(nameof(System)))
        {
            object actualObject = hasMetadataObject;

            GeneratePropMetadata(callerObjectInfo, actualObject.GetType().GetProperty("Data"), hasMetadataObject.Metadatas);
            return;
        }

        GenerateMetadata(callerObjectInfo, hasMetadataObject.Metadatas);
    }

    /// <summary>
    /// Generates metadata recursively for caller object.
    /// </summary>
    /// <param name="callerObjectInfo"></param>
    /// <param name="metadatas"></param>
    private void GenerateMetadata(CallerObjectInfo callerObjectInfo, List<ResponseDataMetadata> metadatas)
    {
        var properties = callerObjectInfo.ReviewedType.GetProperties().Where(p => p.GetCustomAttribute<ExcludeFromMetadataAttribute>() == null);

        if (properties.IsNullOrEmpty())
            return;

        foreach (PropertyInfo property in properties)
            GeneratePropMetadata(callerObjectInfo, property, metadatas);
    }

    /// <summary>
    /// Generaters metadata for <paramref name="property"/> and adds this metadata to <paramref name="metadatas"/>.
    /// </summary>
    /// <param name="callerObjectInfo"></param>
    /// <param name="property"></param>
    /// <param name="metadatas"></param>
    private void GeneratePropMetadata(CallerObjectInfo callerObjectInfo, PropertyInfo property, List<ResponseDataMetadata> metadatas)
    {
        if (property == null)
            return;

        bool removePropMetadataFromResponse = ShouldHide(property);
        bool mask = ShouldMask(property);

        metadatas ??= [];

        var metadata = new ResponseDataMetadata();

        var shouldCreateMetadata = ShouldCreateMetadata(callerObjectInfo, property, removePropMetadataFromResponse);

        if (IsCustomComplextType(property))
        {
            //Self referencing
            if (IsSelfReferencing(callerObjectInfo, property))
            {
                if (shouldCreateMetadata)
                {
                    var selfMetadata = new ResponseDataMetadata
                    {
                        Name = "~Self",
                        LocalizedName = "~Self",
                        Type = GetPropertyFriendlyName(property.PropertyType),
                        Filterable = false,
                    };

                    ApplyMetadataTags(selfMetadata, property, mask);

                    CheckAndAddMetadata(metadatas, selfMetadata);
                }

                return;
            }

            GenerateChildComplexMetadata(callerObjectInfo, property, metadata);
        }

        ApplyMetadataRules(callerObjectInfo.Object, callerObjectInfo.ActualTypeIsCollection, property, mask, removePropMetadataFromResponse);

        if (!shouldCreateMetadata)
            return;

        metadata.Name = property.Name.ToLowerInvariantFirst();
        metadata.Type = GetPropertyFriendlyName(property.PropertyType);
        metadata.DefaultValue = TryGetAttribute(property, out DefaultValueAttribute defaultValueAttribute) ? defaultValueAttribute.Value : null;
        metadata.Info = TryGetAttribute(property, out InfoAttribute infoAttribute) ? infoAttribute.Info : null;

        ApplyTranslationToMetadata(metadata, callerObjectInfo.ReviewedType, property, defaultValueAttribute, infoAttribute);

        //Fill metadata object
        ApplyMetadataTags(metadata, property, mask);

        CheckAndAddMetadata(metadatas, metadata);
    }

    /// <summary>
    /// Apply metadata tags to <paramref name="metadata"/> object.
    /// </summary>
    /// <param name="metadata"></param>
    /// <param name="property"></param>
    /// <param name="mask"></param>
    private void ApplyMetadataTags(ResponseDataMetadata metadata, PropertyInfo property, bool mask)
    {
        metadata.Display = !TryGetAttribute(property, out BrowsableAttribute browsableAttribute) || browsableAttribute.Browsable;
        metadata.Mask = mask;
        metadata.Filterable = !TryGetAttribute(property, out FilterableAttribute filterableAttribute) || filterableAttribute.Filterable;
        metadata.FilterFormat = filterableAttribute?.FilterFormat ?? property.Name;
        metadata.FilterComponentType = filterableAttribute?.FilterComponentType ?? UiInputConstant.TextInput;
        metadata.Pinned = TryGetAttribute(property, out PinnedAttribute pinnedAttribute) && pinnedAttribute.Pinned;
        metadata.DecimalPrecision = TryGetAttribute(property, out DecimalPrecisionAttribute decimalPrecisionAttribute) ? decimalPrecisionAttribute.DecimalPrecision : null;
        metadata.TooltipFormat = TryGetAttribute(property, out TooltipFormatAttribute cellTooltipFormatAttribute) ? cellTooltipFormatAttribute.Format : null;
        metadata.DisplayFormat = TryGetAttribute(property, out DisplayFormatAttribute cellDisplayFormatAttribute) ? cellDisplayFormatAttribute.Format : null;

        var optionsAttribute = property.GetCustomAttribute<OptionsAttribute>();

        if (optionsAttribute != null)
        {
            var fetcher = _serviceProvider.GetKeyedService<IOptionsDataFetcher>(optionsAttribute.ServiceCollectionKey);

            if (fetcher is not null)
            {
                var values = fetcher.IsAsync ? fetcher.FetchAsync(optionsAttribute.OptionalData).Result : fetcher.Fetch(optionsAttribute.OptionalData);

                metadata.Options = values;
            }
        }
    }

    /// <summary>
    /// Generates metadata for child and complex properties.
    /// </summary>
    /// <param name="callerObjectInfo"></param>
    /// <param name="property"></param>
    /// <param name="metadata"></param>
    private void GenerateChildComplexMetadata(CallerObjectInfo callerObjectInfo, PropertyInfo property, ResponseDataMetadata metadata)
    {
        object callerObject = callerObjectInfo.Object;
        object propertyValue = null;

        if (callerObject is not null)
        {
            if (callerObjectInfo.ActualTypeIsCollection)
            {
                var callerObjectsAsList = callerObject as IList;

                if (callerObjectsAsList.Count > 0)
                {
                    foreach (var item in callerObjectsAsList)
                    {
                        callerObject = item;

                        propertyValue = property.GetValue(callerObject, null);

                        CallerObjectInfo childCallerInfo1 = CallerObjectInfo.CreateCallerInformation(propertyValue, property.PropertyType);

                        metadata.Metadatas ??= [];

                        GenerateMetadata(childCallerInfo1, metadata.Metadatas);
                    }

                    return;
                }
            }
            else
                propertyValue = property.GetValue(callerObject, null);
        }

        CallerObjectInfo childCallerInfo = CallerObjectInfo.CreateCallerInformation(propertyValue, property.PropertyType);

        metadata.Metadatas ??= [];

        GenerateMetadata(childCallerInfo, metadata.Metadatas);
    }

    /// <summary>
    /// Determines whether property metadata should mask.
    /// </summary>
    /// <param name="property"></param>
    /// <returns></returns>
    private bool ShouldMask(PropertyInfo property)
    {
        if ((TryGetAttribute(property, out MaskByRoleAttribute maskByRoleAttribute)
            && maskByRoleAttribute.Roles.Length != 0
            && (_interceptionOptions.MaskByRoleFunc?.Invoke(_serviceProvider, maskByRoleAttribute) ?? false))
            || TryGetAttribute(property, out MaskAttribute _))
            return true;

        return false;
    }

    /// <summary>
    /// Determines whether property metadata should hide.
    /// </summary>
    /// <param name="property"></param>
    /// <returns></returns>
    private bool ShouldHide(PropertyInfo property)
    {
        if (TryGetAttribute(property, out HideByRoleAttribute hideByRoleAttribute)
            && hideByRoleAttribute.Roles.Length != 0
            && (_interceptionOptions.HideByRoleFunc?.Invoke(_serviceProvider, hideByRoleAttribute) ?? false))
            return true;

        return false;
    }

    /// <summary>
    /// Applies translation to the metadata.
    /// </summary>
    /// <param name="metadata">The metadata to apply translation to.</param>
    /// <param name="callerObjectType">The type of the caller object.</param>
    /// <param name="property">The property to apply translation to.</param>
    /// <param name="defaultValueAttribute">The default value attribute.</param>
    /// <param name="infoAttribute"></param>
    private void ApplyTranslationToMetadata(ResponseDataMetadata metadata,
                                            Type callerObjectType,
                                            PropertyInfo property,
                                            DefaultValueAttribute defaultValueAttribute,
                                            InfoAttribute infoAttribute)
    {
        var translateAttribute = _interceptionOptions.TranslateMetadata ? callerObjectType.GetCustomAttribute<TranslateAttribute>() : null;
        var localizer = translateAttribute != null ? _serviceProvider?.GetService<IMilvaLocalizer>() : null;
        string localizedName = property.Name;

        //Apply localization to property
        if (translateAttribute != null && localizer != null)
        {
            localizedName = _interceptionOptions.ApplyLocalizationFunc == null
                                     ? ApplyLocalization(translateAttribute.Key, localizer, callerObjectType, property.Name)
                                     : _interceptionOptions.ApplyLocalizationFunc.Invoke(translateAttribute.Key, localizer, callerObjectType, property.Name);

            metadata.DefaultValue = metadata.DefaultValue != null ? (string)localizer[(string)defaultValueAttribute.Value] : metadata.DefaultValue;
            metadata.Info = metadata.Info != null ? (string)localizer[infoAttribute.Info] : metadata.Info;
        }

        metadata.LocalizedName = localizedName;
    }

    /// <summary>
    /// Applies metadata rules to the <see cref="IResponse{T}.Data"/>.
    /// </summary>
    /// <param name="callerObj">The caller object.</param>
    /// <param name="callerObjectTypeIsCollection">Indicates whether the caller object type is a collection.</param>
    /// <param name="property">The property to apply metadata rules to.</param>
    /// <param name="mask">Indicates whether to mask the property value.</param>
    /// <param name="hide">Indicates whether to remove the property metadata from the response.</param>
    private void ApplyMetadataRules(object callerObj, bool callerObjectTypeIsCollection, PropertyInfo property, bool mask, bool hide)
    {
        if (callerObj == null || !_interceptionOptions.ApplyMetadataRules)
            return;

        if (callerObjectTypeIsCollection)
        {
            var callerObjAsList = callerObj as IList;

            foreach (var item in callerObjAsList)
                ApplyMetadataRules(item, false, property, mask, hide);

            return;
        }

        if (hide)
        {
            object defaultValue = property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null;

            property.SetValue(callerObj, defaultValue);

            return;
        }

        //Mask
        if (mask && property.PropertyType == typeof(string))
        {
            string propertyValue = (string)property.GetValue(callerObj);

            if (propertyValue != null)
            {
                propertyValue = propertyValue.Mask(percentToApply: 60);

                property.SetValue(callerObj, propertyValue);
            }
        }

        var linkedWithAttribute = property.GetCustomAttribute<LinkedWithAttribute>();

        if (linkedWithAttribute != null)
        {
            var linkedProperty = callerObj.GetType().GetProperty(linkedWithAttribute.PropertyName);

            var formatter = _serviceProvider.GetKeyedService<ILinkedWithFormatter>(linkedWithAttribute.ServiceCollectionKey);

            var linkedPropValue = linkedProperty?.GetValue(callerObj);

            var formattedValue = formatter?.Format(linkedPropValue);

            property.SetValue(callerObj, formattedValue);
        }
    }

    /// <summary>
    /// Tries to get the custom attribute of the specified type from the property.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    /// <param name="prop">The property.</param>
    /// <param name="attribute">The attribute.</param>
    /// <returns>True if the attribute is found, otherwise false.</returns>
    private static bool TryGetAttribute<TAttribute>(PropertyInfo prop, out TAttribute attribute) where TAttribute : Attribute
    {
        attribute = prop.GetCustomAttribute<TAttribute>();

        return attribute != null;
    }

    /// <summary>
    /// Gets the user-friendly name of the property.
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>The user-friendly name of the property.</returns>
    private static string GetPropertyFriendlyName(Type propertyType)
        => propertyType.IsGenericType
               ? $"{propertyType.Name.Remove(propertyType.Name.IndexOf('`'))}.{string.Join(',', propertyType.GetGenericArguments().Select(GetPropertyFriendlyName))}"
               : $"{GetTypePrefix(propertyType)}{propertyType.Name}";

    private static string GetTypePrefix(Type propertyType)
    {
        string prefix = string.Empty;

        if (propertyType.IsGenericType)
            return prefix;
        else if (propertyType.IsPrimitive)
            prefix = "System.";
        else if (propertyType == typeof(string) || propertyType == typeof(decimal) || propertyType == typeof(DateTime))
            prefix = "System.";
        else if (propertyType.IsEnum)
            prefix = "Enum.";
        else
            prefix = "Object.";

        return prefix;
    }

    private bool ShouldCreateMetadata(CallerObjectInfo callerObjectInfo, PropertyInfo property, bool removePropMetadataFromResponse)
    {
        if (!_interceptionOptions.MetadataCreationEnabled
            || _generateMetadataFuncResult
            || removePropMetadataFromResponse
            || TryGetAttribute(property, out ExcludeFromMetadataAttribute _)
            || callerObjectInfo.ReviewedType.GetCustomAttribute<ExcludeFromMetadataAttribute>() != null)
            return false;

        return true;
    }

    private static void CheckAndAddMetadata(List<ResponseDataMetadata> metadatas, ResponseDataMetadata metadata)
    {
        if (!metadatas.Any(m => m.Name == metadata.Name && m.Type == metadata.Type))
            metadatas.Add(metadata);
    }

    private static bool IsSelfReferencing(CallerObjectInfo callerObjectInfo, PropertyInfo property)
        => property.PropertyType == callerObjectInfo.ReviewedType
                || (property.PropertyType.IsGenericType && property.PropertyType.GetGenericArguments()[0] == callerObjectInfo.ReviewedType);
    private static bool IsCustomComplextType(PropertyInfo property)
        => property.PropertyType.IsClass && !CallerObjectInfo.ReviewObjectType(property.PropertyType, out bool isCollectionComplex).Namespace.Contains(nameof(System));
}

/// <summary>
/// Represents the caller object information.
/// </summary>
public class CallerObjectInfo
{
    /// <summary>
    /// Gets or sets the object.
    /// </summary>
    public object Object { get; set; }
    /// <summary>
    /// Gets or sets the actual type.
    /// </summary>
    public Type ActualType { get; set; }
    /// <summary>
    /// Gets or sets the reviewed type.
    /// </summary>
    public Type ReviewedType { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the actual type is a collection.
    /// </summary>
    public bool ActualTypeIsCollection { get; set; }

    /// <summary>
    /// Creates the caller object information.
    /// </summary>
    /// <param name="obj">The object.</param>
    /// <param name="objectType">The type of the object.</param>
    /// <returns>The created caller object information.</returns>
    public static CallerObjectInfo CreateCallerInformation(object obj, Type objectType)
    {
        var reviewedType = ReviewObjectType(objectType, out bool actualTypeIsCollection);

        return new CallerObjectInfo
        {
            Object = obj,
            ActualType = objectType,
            ReviewedType = reviewedType,
            ActualTypeIsCollection = actualTypeIsCollection
        };
    }

    internal static Type ReviewObjectType(Type objectType, out bool typeIsCollection)
    {
        typeIsCollection = objectType.IsGenericType && typeof(IList).IsAssignableFrom(objectType);

        var reviewedType = typeIsCollection ? objectType.GetGenericArguments()[0] : objectType;

        return reviewedType;
    }
}
