using Milvasoft.Components.Rest.Enums;

namespace Milvasoft.Components.Rest.Request;

/// <summary>
/// Represents criteria used for filtering data.
/// </summary>
public class FilterCriteria
{
    private static readonly List<char> _specialChars = ['[', ']', '.'];

    /// <summary>
    /// Gets or sets the name of the column on which the filter is applied. Collection searchs are not supported.
    /// </summary>
    public string FilterBy { get; set; }

    /// <summary>
    /// Gets or sets the value to filter by.
    /// </summary>
    public object Value { get; set; }

    /// <summary>
    /// Gets or sets the value to filter by.
    /// </summary>
    public object OtherValue { get; set; }

    /// <summary>
    /// Gets or sets the type of filter operation to apply.
    /// </summary>
    public FilterType FilterType { get; set; }

    public bool FilterByContainsSpecialChars() => FilterBy?.Any(_specialChars.Contains) ?? false;

    public string GetUntilSpecialCharFromFilterBy() => FilterByContainsSpecialChars() ? FilterBy?[..FilterBy.IndexOfAny([.. _specialChars])] : FilterBy;

    public string GetAfterSpecialCharFromFilterBy() => FilterByContainsSpecialChars() ? FilterBy?[FilterBy.IndexOfAny([.. _specialChars])..] : FilterBy;

    public string GetChildrenPropertyNameFromFilterBy()
    {
        if (string.IsNullOrWhiteSpace(FilterBy))
            return FilterBy;

        var children = GetAfterSpecialCharFromFilterBy();

        foreach (var specialChar in _specialChars)
        {
            children = children.Replace(specialChar, ' ').Trim();
        }

        return children;
    }

    public string GetFilterByAsListFormat(string listPropertyName) => $"{listPropertyName}[{FilterBy}]";
    public string GetFilterByAsReferenceFormat(string referencePropertyName) => $"{referencePropertyName}.{FilterBy}";
}
