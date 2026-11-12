using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Attributes.Annotations;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Types.Classes;
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Milvasoft.Interception.Interceptors.Response;

/// <summary>
/// Generates metadata for the response data and applies metadata rules (hide, mask, linked values) to it.
/// </summary>
/// <remarks>
/// <para>Collections are recognized by their element type (arrays and any <see cref="IEnumerable{T}"/> except strings), so rules apply to every element.</para>
/// <para>Reflection results (properties and their attributes) are cached per type.</para>
/// <para>Rules modify the returned objects in place. Do not return entities tracked by a DbContext that is saved afterwards in the same scope.</para>
/// </remarks>
/// <param name="serviceProvider">The service provider.</param>
public class ResponseMetadataGenerator(IServiceProvider serviceProvider) : IResponseMetadataGenerator
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IResponseInterceptionOptions _interceptionOptions = serviceProvider.GetService<IResponseInterceptionOptions>() ?? new ResponseInterceptionOptions();
    private readonly bool _generateMetadataFuncResult = (!serviceProvider.GetService<IResponseInterceptionOptions>()?.GenerateMetadataFunc?.Invoke(serviceProvider) ?? false);
    private static readonly ConcurrentDictionary<Type, Func<object>> _defaultValueFactoryCache = new();
    private const int _maxDepth = 3;

    /// <summary>
    /// Options to fetch for generated metadata. Fetching is deferred so asynchronous fetchers can be awaited.
    /// </summary>
    private List<(ResponseDataMetadata Metadata, IOptionsDataFetcher Fetcher, object OptionalData)> _pendingOptionFetches;

    /// <summary>
    /// Applies localization to the specified property.
    /// </summary>
    /// <param name="localizationKey">The localization key.</param>
    /// <param name="localizer">The localizer.</param>
    /// <param name="callerObjectType">The result data type.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The localized property name. Falls back to the global key, then to <paramref name="propertyName"/>.</returns>
    public static string ApplyLocalization(string localizationKey, IMilvaLocalizer localizer, Type callerObjectType, string propertyName)
    {
        var key = $"{localizationKey ?? callerObjectType.Name}.{propertyName}";

        LocalizedValue localizedValue = localizer[key];

        if (localizedValue.ResourceFound)
            return localizedValue;

        LocalizedValue globalLocalizedValue = localizer[$"Global.{propertyName}"];

        return globalLocalizedValue.ResourceFound ? globalLocalizedValue : propertyName;
    }

    /// <summary>
    /// Generates metadata for the specified object and assing <paramref name="hasMetadataObject"/>.Metadatas.
    /// Asynchronous options fetchers are waited synchronously; prefer <see cref="GenerateMetadataAsync(IHasMetadata)"/>.
    /// </summary>
    /// <param name="hasMetadataObject">The object to generate metadata for.</param>
    public void GenerateMetadata(IHasMetadata hasMetadataObject)
    {
        CollectMetadata(hasMetadataObject);

        foreach (var (metadata, fetcher, optionalData) in TakePendingOptionFetches())
            metadata.Options = fetcher.IsAsync ? fetcher.FetchAsync(optionalData).GetAwaiter().GetResult() : fetcher.Fetch(optionalData);
    }

    /// <summary>
    /// Generates metadata for the specified object and assing <paramref name="hasMetadataObject"/>.Metadatas.
    /// </summary>
    /// <param name="hasMetadataObject">The object to generate metadata for.</param>
    public async Task GenerateMetadataAsync(IHasMetadata hasMetadataObject)
    {
        CollectMetadata(hasMetadataObject);

        foreach (var (metadata, fetcher, optionalData) in TakePendingOptionFetches())
            metadata.Options = fetcher.IsAsync ? await fetcher.FetchAsync(optionalData) : fetcher.Fetch(optionalData);
    }

    private List<(ResponseDataMetadata Metadata, IOptionsDataFetcher Fetcher, object OptionalData)> TakePendingOptionFetches()
    {
        var pending = _pendingOptionFetches ?? [];

        _pendingOptionFetches = null;

        return pending;
    }

    private void CollectMetadata(IHasMetadata hasMetadataObject)
    {
        hasMetadataObject.Metadatas = [];
        _pendingOptionFetches = [];

        var (responseData, resultDataType) = hasMetadataObject.GetResponseDataTypePair();

        if (resultDataType == null)
            return;

        var callerObjectInfo = CallerObjectInfo.CreateCallerInformation(responseData, resultDataType);

        if (CallerObjectInfo.IsSimpleType(callerObjectInfo.ReviewedType))
        {
            object actualObject = hasMetadataObject;

            GeneratePropMetadata(callerObjectInfo, actualObject.GetType().GetProperty("Data"), hasMetadataObject.Metadatas, currentDepth: 0);
            return;
        }

        GenerateMetadata(callerObjectInfo, hasMetadataObject.Metadatas, currentDepth: 0);
    }

    /// <summary>
    /// Generates metadata recursively for caller object.
    /// </summary>
    /// <param name="callerObjectInfo"></param>
    /// <param name="metadatas"></param>
    /// <param name="currentDepth"></param>
    private void GenerateMetadata(CallerObjectInfo callerObjectInfo, List<ResponseDataMetadata> metadatas = null, int currentDepth = 0)
    {
        if (currentDepth > _maxDepth)
            return;

        foreach (var property in TypeDescriptorCache.GetProperties(callerObjectInfo.ReviewedType))
            GeneratePropMetadata(callerObjectInfo, property.Property, metadatas, currentDepth + 1);
    }

    /// <summary>
    /// Generaters metadata for <paramref name="property"/> and adds this metadata to <paramref name="metadatas"/>.
    /// </summary>
    /// <param name="callerObjectInfo"></param>
    /// <param name="property"></param>
    /// <param name="metadatas"></param>
    /// <param name="currentDepth"></param>
    private void GeneratePropMetadata(CallerObjectInfo callerObjectInfo, PropertyInfo property, List<ResponseDataMetadata> metadatas, int currentDepth)
    {
        if (property == null)
            return;

        var descriptor = TypeDescriptorCache.GetProperty(property);

        bool removePropMetadataFromResponse = ShouldHide(descriptor);
        bool mask = ShouldMask(descriptor);

        metadatas ??= [];

        var metadata = new ResponseDataMetadata();

        var shouldCreateMetadata = ShouldCreateMetadata(callerObjectInfo, descriptor, removePropMetadataFromResponse);

        if (descriptor.IsCustomComplexType)
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
                        Type = descriptor.FriendlyTypeName,
                        Filterable = false,
                    };

                    ApplyMetadataTags(selfMetadata, descriptor, mask);

                    AddMetadata(metadatas, selfMetadata, descriptor);
                }

                return;
            }

            GenerateChildComplexMetadata(callerObjectInfo, property, metadata, currentDepth);
        }

        ApplyMetadataRules(callerObjectInfo.Object, callerObjectInfo.ActualTypeIsCollection, descriptor, mask, removePropMetadataFromResponse);

        if (!shouldCreateMetadata)
            return;

        metadata.Name = property.Name.ToLowerInvariantFirst();
        metadata.Type = descriptor.FriendlyTypeName;
        metadata.DefaultValue = descriptor.ClientDefaultValue?.Value;
        metadata.Info = descriptor.Info?.Info;

        ApplyTranslationToMetadata(metadata, callerObjectInfo.ReviewedType, descriptor);

        //Fill metadata object
        ApplyMetadataTags(metadata, descriptor, mask);

        AddMetadata(metadatas, metadata, descriptor);
    }

    /// <summary>
    /// Apply metadata tags to <paramref name="metadata"/> object.
    /// </summary>
    /// <param name="metadata"></param>
    /// <param name="descriptor"></param>
    /// <param name="mask"></param>
    private static void ApplyMetadataTags(ResponseDataMetadata metadata, PropertyDescriptorInfo descriptor, bool mask)
    {
        metadata.Display = descriptor.Browsable?.Browsable ?? true;
        metadata.Mask = mask;
        metadata.Filterable = descriptor.Filterable?.Filterable ?? true;
        metadata.Aggregatable = descriptor.Aggregatable?.Aggregatable ?? false;
        metadata.FilterFormat = descriptor.Filterable?.FilterFormat ?? descriptor.Property.Name;
        metadata.FilterComponentType = descriptor.Filterable?.FilterComponentType ?? UiInputConstant.TextInput;
        metadata.Pinned = descriptor.Pinned?.Pinned ?? false;
        metadata.DecimalPrecision = descriptor.DecimalPrecision?.DecimalPrecision;
        metadata.TooltipFormat = descriptor.TooltipFormat?.Format;
        metadata.DisplayFormat = descriptor.DisplayFormat?.Format;
    }

    /// <summary>
    /// Adds <paramref name="metadata"/> if an equal one is not added yet and schedules its options fetch.
    /// </summary>
    private void AddMetadata(List<ResponseDataMetadata> metadatas, ResponseDataMetadata metadata, PropertyDescriptorInfo descriptor)
    {
        if (metadatas.Exists(m => m.Name == metadata.Name && m.Type == metadata.Type))
            return;

        metadatas.Add(metadata);

        if (descriptor.Options == null)
            return;

        var fetcher = _serviceProvider.GetKeyedService<IOptionsDataFetcher>(descriptor.Options.ServiceCollectionKey);

        if (fetcher is not null)
            (_pendingOptionFetches ??= []).Add((metadata, fetcher, descriptor.Options.OptionalData));
    }

    /// <summary>
    /// Generates metadata for child and complex properties.
    /// </summary>
    /// <param name="callerObjectInfo"></param>
    /// <param name="property"></param>
    /// <param name="metadata"></param>
    /// <param name="currentDepth"></param>
    private void GenerateChildComplexMetadata(CallerObjectInfo callerObjectInfo, PropertyInfo property, ResponseDataMetadata metadata, int currentDepth)
    {
        if (currentDepth > _maxDepth)
            return;

        object callerObject = callerObjectInfo.Object;
        object propertyValue = null;

        if (callerObject is not null)
        {
            if (callerObjectInfo.ActualTypeIsCollection)
            {
                if (callerObject is IEnumerable callerObjects)
                {
                    bool hasItem = false;

                    foreach (var item in callerObjects)
                    {
                        if (item is null)
                            continue;

                        hasItem = true;

                        propertyValue = property.GetValue(item, null);

                        CallerObjectInfo itemCallerInfo = CallerObjectInfo.CreateCallerInformation(propertyValue, property.PropertyType);

                        metadata.Metadatas ??= [];

                        GenerateMetadata(itemCallerInfo, metadata.Metadatas, currentDepth + 1);
                    }

                    if (hasItem)
                        return;

                    propertyValue = null;
                }
            }
            else
                propertyValue = property.GetValue(callerObject, null);
        }

        CallerObjectInfo childCallerInfo = CallerObjectInfo.CreateCallerInformation(propertyValue, property.PropertyType);

        metadata.Metadatas ??= [];
        GenerateMetadata(childCallerInfo, metadata.Metadatas, currentDepth + 1);
    }

    /// <summary>
    /// Determines whether property metadata should mask.
    /// </summary>
    private bool ShouldMask(PropertyDescriptorInfo descriptor)
        => descriptor.Mask != null
           || (descriptor.MaskByRole != null
               && descriptor.MaskByRole.Roles?.Length > 0
               && (_interceptionOptions.MaskByRoleFunc?.Invoke(_serviceProvider, descriptor.MaskByRole) ?? false));

    /// <summary>
    /// Determines whether property metadata should hide.
    /// </summary>
    private bool ShouldHide(PropertyDescriptorInfo descriptor)
        => descriptor.HideByRole != null
           && descriptor.HideByRole.Roles?.Length > 0
           && (_interceptionOptions.HideByRoleFunc?.Invoke(_serviceProvider, descriptor.HideByRole) ?? false);

    /// <summary>
    /// Applies translation to the metadata.
    /// </summary>
    /// <param name="metadata">The metadata to apply translation to.</param>
    /// <param name="callerObjectType">The type of the caller object.</param>
    /// <param name="descriptor">The property to apply translation to.</param>
    private void ApplyTranslationToMetadata(ResponseDataMetadata metadata, Type callerObjectType, PropertyDescriptorInfo descriptor)
    {
        var translateAttribute = _interceptionOptions.TranslateMetadata ? TypeDescriptorCache.GetTranslateAttribute(callerObjectType) : null;
        var localizer = translateAttribute != null ? _serviceProvider?.GetService<IMilvaLocalizer>() : null;
        string localizedName = descriptor.Property.Name;

        //Apply localization to property
        if (translateAttribute != null && localizer != null)
        {
            localizedName = _interceptionOptions.ApplyLocalizationFunc == null
                                     ? ApplyLocalization(translateAttribute.Key, localizer, callerObjectType, descriptor.Property.Name)
                                     : _interceptionOptions.ApplyLocalizationFunc.Invoke(translateAttribute.Key, localizer, callerObjectType, descriptor.Property.Name);

            // Only string default values are localization keys; other values (numbers, booleans, ...) are kept as they are.
            if (metadata.DefaultValue is string defaultValueKey)
                metadata.DefaultValue = (string)localizer[defaultValueKey];

            if (metadata.Info != null)
                metadata.Info = (string)localizer[metadata.Info];
        }

        metadata.LocalizedName = localizedName;
    }

    /// <summary>
    /// Applies metadata rules to the <see cref="IResponse{T}.Data"/>.
    /// </summary>
    /// <param name="callerObj">The caller object.</param>
    /// <param name="callerObjectTypeIsCollection">Indicates whether the caller object type is a collection.</param>
    /// <param name="descriptor">The property to apply metadata rules to.</param>
    /// <param name="mask">Indicates whether to mask the property value.</param>
    /// <param name="hide">Indicates whether to remove the property metadata from the response.</param>
    /// <param name="currentDepth"></param>
    private void ApplyMetadataRules(object callerObj, bool callerObjectTypeIsCollection, PropertyDescriptorInfo descriptor, bool mask, bool hide, int currentDepth = 0)
    {
        if (currentDepth > _maxDepth || callerObj == null || !_interceptionOptions.ApplyMetadataRules)
            return;

        if (callerObjectTypeIsCollection)
        {
            if (callerObj is IEnumerable items)
                foreach (var item in items)
                    ApplyMetadataRules(item, false, descriptor, mask, hide, currentDepth);

            return;
        }

        var property = descriptor.Property;

        // The property may belong to the declared type while the object is of an unrelated runtime type (e.g. the Data property of the response itself).
        if (property.DeclaringType != null && !property.DeclaringType.IsInstanceOfType(callerObj))
            return;

        if (hide)
        {
            if (property.CanWrite)
                property.SetValue(callerObj, GetDefaultValue(property.PropertyType));

            return;
        }

        //Mask
        if (mask && property.PropertyType == typeof(string) && property.CanWrite)
        {
            string propertyValue = (string)property.GetValue(callerObj);

            if (propertyValue != null)
                property.SetValue(callerObj, propertyValue.Mask(percentToApply: 60));
        }

        var linkedWithAttribute = descriptor.LinkedWith;

        if (linkedWithAttribute != null && property.CanWrite)
        {
            var linkedProperty = callerObj.GetType().GetProperty(linkedWithAttribute.PropertyName);

            var formatter = _serviceProvider.GetKeyedService<ILinkedWithFormatter>(linkedWithAttribute.ServiceCollectionKey);

            var linkedPropValue = linkedProperty?.GetValue(callerObj);

            var formattedValue = formatter?.Format(linkedPropValue);

            property.SetValue(callerObj, formattedValue);
        }

        // Apply the same rules to nested properties (recursive)
        if (descriptor.IsCustomComplexType)
        {
            var nestedObj = property.GetValue(callerObj);

            if (nestedObj != null)
            {
                Type nestedType = CallerObjectInfo.ReviewObjectType(property.PropertyType, out bool isCollection);

                foreach (var nestedDescriptor in TypeDescriptorCache.GetProperties(nestedType))
                {
                    if (!nestedDescriptor.Property.CanRead || !nestedDescriptor.Property.CanWrite)
                        continue;

                    ApplyMetadataRules(nestedObj, isCollection, nestedDescriptor, ShouldMask(nestedDescriptor), ShouldHide(nestedDescriptor), currentDepth + 1);
                }
            }
        }
    }

    private bool ShouldCreateMetadata(CallerObjectInfo callerObjectInfo, PropertyDescriptorInfo descriptor, bool removePropMetadataFromResponse)
    {
        if (!_interceptionOptions.MetadataCreationEnabled
            || _generateMetadataFuncResult
            || removePropMetadataFromResponse
            || descriptor.ExcludeFromMetadata
            || TypeDescriptorCache.IsExcludedFromMetadata(callerObjectInfo.ReviewedType))
            return false;

        return true;
    }

    private static bool IsSelfReferencing(CallerObjectInfo callerObjectInfo, PropertyInfo property)
        => CallerObjectInfo.ReviewObjectType(property.PropertyType, out _) == callerObjectInfo.ReviewedType;

    private static object GetDefaultValue(Type type)
    {
        if (!type.IsValueType)
            return null;

        return _defaultValueFactoryCache.GetOrAdd(type, t =>
        {
            // Default(T) → object
            var body = Expression.Convert(Expression.Default(t), typeof(object));
            var lambda = Expression.Lambda<Func<object>>(body);
            return lambda.Compile();
        })();
    }

    #region Reflection cache

    /// <summary>
    /// Cached reflection information of a property.
    /// </summary>
    private sealed class PropertyDescriptorInfo
    {
        public PropertyInfo Property { get; init; }
        public string FriendlyTypeName { get; init; }
        public bool IsCustomComplexType { get; init; }
        public bool ExcludeFromMetadata { get; init; }
        public BrowsableAttribute Browsable { get; init; }
        public FilterableAttribute Filterable { get; init; }
        public AggregatableAttribute Aggregatable { get; init; }
        public PinnedAttribute Pinned { get; init; }
        public DecimalPrecisionAttribute DecimalPrecision { get; init; }
        public TooltipFormatAttribute TooltipFormat { get; init; }
        public DisplayFormatAttribute DisplayFormat { get; init; }
        public OptionsAttribute Options { get; init; }
        public ClientDefaultValueAttribute ClientDefaultValue { get; init; }
        public InfoAttribute Info { get; init; }
        public MaskAttribute Mask { get; init; }
        public MaskByRoleAttribute MaskByRole { get; init; }
        public HideByRoleAttribute HideByRole { get; init; }
        public LinkedWithAttribute LinkedWith { get; init; }
    }

    private static class TypeDescriptorCache
    {
        private static readonly ConcurrentDictionary<PropertyInfo, PropertyDescriptorInfo> _properties = new();
        private static readonly ConcurrentDictionary<Type, PropertyDescriptorInfo[]> _typeProperties = new();
        private static readonly ConcurrentDictionary<Type, (TranslateAttribute Translate, bool Excluded)> _typeAttributes = new();

        /// <summary>
        /// Gets the public properties of <paramref name="type"/> that are not excluded from metadata.
        /// </summary>
        public static PropertyDescriptorInfo[] GetProperties(Type type)
            => _typeProperties.GetOrAdd(type, static t => [.. t.GetProperties().Select(GetProperty).Where(p => !p.ExcludeFromMetadata)]);

        public static PropertyDescriptorInfo GetProperty(PropertyInfo property) => _properties.GetOrAdd(property, static p => new PropertyDescriptorInfo
        {
            Property = p,
            FriendlyTypeName = GetPropertyFriendlyName(p.PropertyType),
            IsCustomComplexType = CallerObjectInfo.IsCustomComplexType(p.PropertyType),
            ExcludeFromMetadata = p.GetCustomAttribute<ExcludeFromMetadataAttribute>() != null,
            Browsable = p.GetCustomAttribute<BrowsableAttribute>(),
            Filterable = p.GetCustomAttribute<FilterableAttribute>(),
            Aggregatable = p.GetCustomAttribute<AggregatableAttribute>(),
            Pinned = p.GetCustomAttribute<PinnedAttribute>(),
            DecimalPrecision = p.GetCustomAttribute<DecimalPrecisionAttribute>(),
            TooltipFormat = p.GetCustomAttribute<TooltipFormatAttribute>(),
            DisplayFormat = p.GetCustomAttribute<DisplayFormatAttribute>(),
            Options = p.GetCustomAttribute<OptionsAttribute>(),
            ClientDefaultValue = p.GetCustomAttribute<ClientDefaultValueAttribute>(),
            Info = p.GetCustomAttribute<InfoAttribute>(),
            Mask = p.GetCustomAttribute<MaskAttribute>(),
            MaskByRole = p.GetCustomAttribute<MaskByRoleAttribute>(),
            HideByRole = p.GetCustomAttribute<HideByRoleAttribute>(),
            LinkedWith = p.GetCustomAttribute<LinkedWithAttribute>(),
        });

        public static TranslateAttribute GetTranslateAttribute(Type type) => GetTypeAttributes(type).Translate;

        public static bool IsExcludedFromMetadata(Type type) => GetTypeAttributes(type).Excluded;

        private static (TranslateAttribute Translate, bool Excluded) GetTypeAttributes(Type type)
            => _typeAttributes.GetOrAdd(type, static t => (t.GetCustomAttribute<TranslateAttribute>(), t.GetCustomAttribute<ExcludeFromMetadataAttribute>() != null));

        /// <summary>
        /// Gets the user-friendly name of the property.
        /// </summary>
        private static string GetPropertyFriendlyName(Type propertyType)
            => propertyType.IsGenericType
                   ? $"{propertyType.Name[..propertyType.Name.IndexOf('`')]}.{string.Join(',', propertyType.GetGenericArguments().Select(GetPropertyFriendlyName))}"
                   : $"{GetTypePrefix(propertyType)}{propertyType.Name}";

        private static string GetTypePrefix(Type propertyType)
        {
            if (propertyType.IsPrimitive || propertyType == typeof(string) || propertyType == typeof(decimal) || propertyType == typeof(DateTime))
                return "System.";

            if (propertyType.IsEnum)
                return "Enum.";

            return "Object.";
        }
    }

    #endregion
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

    /// <summary>
    /// Returns the element type of collections (arrays and any <see cref="IEnumerable{T}"/> except <see cref="string"/>), otherwise <paramref name="objectType"/>.
    /// </summary>
    internal static Type ReviewObjectType(Type objectType, out bool typeIsCollection)
    {
        var elementType = GetCollectionElementType(objectType);

        typeIsCollection = elementType != null;

        return elementType ?? objectType;
    }

    /// <summary>
    /// Whether the type is a primitive like type (BCL types such as numbers, strings, dates, guids, enums of the System namespace and their nullable versions)
    /// whose properties are not described separately.
    /// </summary>
    internal static bool IsSimpleType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal))
            return true;

        var typeNamespace = type.Namespace;

        return typeNamespace != null && (typeNamespace == nameof(System) || typeNamespace.StartsWith($"{nameof(System)}.", StringComparison.Ordinal));
    }

    /// <summary>
    /// Whether properties of the type (or of its collection element type) are described and processed recursively.
    /// </summary>
    internal static bool IsCustomComplexType(Type type)
        => !type.IsValueType && !IsSimpleType(ReviewObjectType(type, out _));

    private static Type GetCollectionElementType(Type type)
    {
        if (type == null || type == typeof(string))
            return null;

        if (type.IsArray)
            return type.GetElementType();

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            return type.GetGenericArguments()[0];

        var enumerableInterface = Array.Find(type.GetInterfaces(), i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        return enumerableInterface?.GetGenericArguments()[0];
    }
}
