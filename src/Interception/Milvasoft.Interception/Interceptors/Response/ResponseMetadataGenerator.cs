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
        var properties = callerObjectInfo.ReviewedType.GetProperties();

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
        if (property == null || TryGetAttribute(property, out ExcludeFromMetadataAttribute _))
            return;

        bool removePropMetadataFromResponse = ShouldHide(property);
        bool mask = ShouldMask(property);

        ApplyMetadataRules(callerObjectInfo.Object, callerObjectInfo.ActualTypeIsCollection, property, mask, removePropMetadataFromResponse);

        if (!_interceptionOptions.MetadataCreationEnabled || removePropMetadataFromResponse)
            return;

        metadatas ??= [];

        var metadata = new ResponseDataMetadata();

        if (property.PropertyType.IsClass && !CallerObjectInfo.ReviewObjectType(property.PropertyType, out bool _).Namespace.Contains(nameof(System)))
        {
            GenerateChildComplexMetadata(callerObjectInfo, property, metadata);
        }

        metadata.Name = property.Name;
        metadata.Type = GetPropertyFriendlyName(property);
        metadata.DefaultValue = TryGetAttribute(property, out DefaultValueAttribute defaultValueAttribute) ? defaultValueAttribute.Value : null;

        ApplyTranslationToMetadata(metadata, callerObjectInfo.ReviewedType, property, defaultValueAttribute);

        //If no attribute specified no need to proceed.
        if (!property.GetCustomAttributes().Any())
        {
            metadatas.Add(metadata);
            return;
        }

        //Fill metadata object
        metadata.Display = !TryGetAttribute(property, out BrowsableAttribute browsableAttribute) || browsableAttribute.Browsable;
        metadata.Mask = mask;
        metadata.Filterable = !TryGetAttribute(property, out FilterableAttribute filterableAttribute) || filterableAttribute.Filterable;
        metadata.FilterFormat = filterableAttribute?.FilterFormat ?? property.Name;
        metadata.Pinned = TryGetAttribute(property, out PinnedAttribute pinnedAttribute) && pinnedAttribute.Pinned;
        metadata.DecimalPrecision = TryGetAttribute(property, out DecimalPrecisionAttribute decimalPrecisionAttribute) ? decimalPrecisionAttribute.DecimalPrecision : null;
        metadata.TooltipFormat = TryGetAttribute(property, out TooltipFormatAttribute cellTooltipFormatAttribute) ? cellTooltipFormatAttribute.Format : null;
        metadata.DisplayFormat = TryGetAttribute(property, out DisplayFormatAttribute cellDisplayFormatAttribute) ? cellDisplayFormatAttribute.Format : null;

        metadatas.Add(metadata);
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

        if (callerObjectInfo.ActualTypeIsCollection)
        {
            var callerObjectsAsList = callerObjectInfo.Object as IList;

            callerObject = callerObjectsAsList.Count > 0 ? callerObjectsAsList[0] : callerObjectInfo.Object;
        }

        object propertyValue = property.GetValue(callerObject, null);

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
    private void ApplyTranslationToMetadata(ResponseDataMetadata metadata, Type callerObjectType, PropertyInfo property, DefaultValueAttribute defaultValueAttribute)
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
        }

        metadata.LocalizedName = localizedName;
    }

    /// <summary>
    /// Applies metadata rules to the <see cref="IResponse{T}.Data"/>.
    /// </summary>
    /// <param name="callerObj">The caller object.</param>
    /// <param name="callerObjectTypeIsCollection">Indicates whether the caller object type is a collection.</param>
    /// <param name="prop">The property to apply metadata rules to.</param>
    /// <param name="mask">Indicates whether to mask the property value.</param>
    /// <param name="hide">Indicates whether to remove the property metadata from the response.</param>
    private void ApplyMetadataRules(object callerObj, bool callerObjectTypeIsCollection, PropertyInfo prop, bool mask, bool hide)
    {
        if (callerObj == null || !_interceptionOptions.ApplyMetadataRules)
            return;

        if (callerObjectTypeIsCollection)
        {
            var callerObjAsList = callerObj as IList;

            foreach (var item in callerObjAsList)
                ApplyMetadataRules(item, false, prop, mask, hide);

            return;
        }

        if (hide)
        {
            object defaultValue = prop.PropertyType.IsValueType ? Activator.CreateInstance(prop.PropertyType) : null;

            prop.SetValue(callerObj, defaultValue);

            return;
        }

        //Mask
        if (mask && prop.PropertyType == typeof(string))
        {
            string propertyValue = (string)prop.GetValue(callerObj);

            if (propertyValue != null)
            {
                propertyValue = propertyValue.Mask(percentToApply: 60);

                prop.SetValue(callerObj, propertyValue);
            }
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
    /// <param name="prop">The property.</param>
    /// <returns>The user-friendly name of the property.</returns>
    private static string GetPropertyFriendlyName(PropertyInfo prop)
        => prop.PropertyType.IsGenericType
            ? $"{prop.PropertyType.Name.Remove(prop.PropertyType.Name.IndexOf('`'))}.{string.Join(',', prop.PropertyType.GetGenericArguments().Select(i => i.Name))}"
            : prop.PropertyType.Name;
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
