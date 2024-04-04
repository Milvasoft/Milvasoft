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
    public FilterType Type { get; set; }

    /// <summary>
    /// Determines whether the filter criteria contains special characters.
    /// </summary>
    /// <returns>True if the filter criteria contains special characters; otherwise, false.</returns>
    public bool FilterByContainsSpecialChars() => FilterBy?.Any(_specialChars.Contains) ?? false;

    /// <summary>
    /// Gets the substring from the beginning of the filter criteria until the first special character.
    /// </summary>
    /// <returns>The substring from the beginning of the filter criteria until the first special character.</returns>
    public string GetUntilSpecialCharFromFilterBy() => FilterByContainsSpecialChars() ? FilterBy?[..FilterBy.IndexOfAny([.. _specialChars])] : FilterBy;

    /// <summary>
    /// Gets the substring from the first special character until the end of the filter criteria.
    /// </summary>
    /// <returns>The substring from the first special character until the end of the filter criteria.</returns>
    public string GetAfterSpecialCharFromFilterBy() => FilterByContainsSpecialChars() ? FilterBy?[FilterBy.IndexOfAny([.. _specialChars])..] : FilterBy;

    /// <summary>
    /// Gets the children property name from the filter criteria.
    /// </summary>
    /// <returns>The children property name from the filter criteria.</returns>
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

    /// <summary>
    /// Gets the filter criteria in list format.
    /// </summary>
    /// <param name="listPropertyName">The name of the list property.</param>
    /// <returns>The filter criteria in list format.</returns>
    public string GetFilterByAsListFormat(string listPropertyName) => $"{listPropertyName}[{FilterBy}]";

    /// <summary>
    /// Gets the filter criteria in reference format.
    /// </summary>
    /// <param name="referencePropertyName">The name of the reference property.</param>
    /// <returns>The filter criteria in reference format.</returns>
    public string GetFilterByAsReferenceFormat(string referencePropertyName) => $"{referencePropertyName}.{FilterBy}";
}
