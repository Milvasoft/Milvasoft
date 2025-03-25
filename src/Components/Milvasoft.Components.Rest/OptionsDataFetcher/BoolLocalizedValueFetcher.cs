using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Attributes.Annotations;
using System.Text.RegularExpressions;

namespace Milvasoft.Components.Rest.OptionsDataFetcher.EnumValueFetcher;

/// <summary>
/// Metadata options fetcher. <see cref="OptionsAttribute"/> optional data should be enum type.
/// </summary>
public partial class BoolLocalizedValueFetcher(IServiceProvider serviceProvider) : IOptionsDataFetcher
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <inheritdoc/>
    public const string FetcherName = "BoolLocalizedValueFetcher";

    /// <inheritdoc/>
    public bool IsAsync { get; set; } = false;

    /// <summary>
    /// Fetches the values of the boolean type.
    /// </summary>
    /// <param name="optionalData">Optional data format should be <see cref="string"/>. Sample : "Yes,No" . 
    /// The left side is the value of the true state, and the right side is the value of the false state.</param>
    /// <returns></returns>
    public List<object> Fetch(object optionalData = null)
    {
        if (optionalData is not string || !OptionalDataRegex().IsMatch(optionalData as string))
            return null;

        var localizer = _serviceProvider.GetService<IMilvaLocalizer>();

        var resourceKeys = ((string)optionalData).Split(',');

        var enumLookups = new List<object>(capacity: 2)
        {
            new OptionLookupModel { Value = true, Name = localizer != null ? localizer[resourceKeys[0]] : "true" },
            new OptionLookupModel { Value = false, Name = localizer != null ? localizer[resourceKeys[1]] : "false" },
        };

        return enumLookups;
    }

    /// <inheritdoc/>
    public Task<List<object>> FetchAsync(object optionalData = null) => throw new NotImplementedException();

    [GeneratedRegex(@"^\w+,\w+$")]
    private static partial Regex OptionalDataRegex();
}
