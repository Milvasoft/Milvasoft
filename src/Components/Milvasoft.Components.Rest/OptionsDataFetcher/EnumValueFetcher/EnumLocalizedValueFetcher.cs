using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Attributes.Annotations;

namespace Milvasoft.Components.Rest.OptionsDataFetcher.EnumValueFetcher;

/// <summary>
/// Metadata options fetcher. <see cref="OptionsAttribute"/> optional data should be enum type.
/// </summary>
public class EnumLocalizedValueFetcher(IServiceProvider serviceProvider) : IOptionsDataFetcher
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <inheritdoc/>
    public const string FetcherName = "EnumValueFetcher";

    /// <inheritdoc/>
    public bool IsAsync { get; set; } = false;

    /// <summary>
    /// Fetches enum values.
    /// </summary>
    /// <param name="optionalData">Optional data format should be enum <see cref="Type"/>.</param>
    /// <returns></returns>
    public List<object> Fetch(object optionalData = null)
    {
        if (optionalData is null || optionalData is not Type)
            return null;

        Type enumType = optionalData as Type;

        var enumValues = Enum.GetValues(enumType);

        var enumUnderlyingType = Enum.GetUnderlyingType(enumType);

        var enumLookups = new List<object>();

        var localizer = _serviceProvider.GetService<IMilvaLocalizer>();

        foreach (var enumValue in enumValues)
        {
            var resourceKey = $"{enumType.Name}.{enumValue}";

            var localizedEnumValue = localizer != null ? localizer[resourceKey] : resourceKey;

            var enumActualValue = Convert.ChangeType(enumValue, enumUnderlyingType);

            enumLookups.Add(new OptionLookupModel { Value = enumActualValue, Name = localizedEnumValue });
        }

        return enumLookups;
    }

    /// <inheritdoc/>
    public Task<List<object>> FetchAsync(object optionalData = null) => throw new NotImplementedException();
}
