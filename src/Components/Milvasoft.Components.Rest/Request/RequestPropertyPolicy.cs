using Milvasoft.Attributes.Annotations;
using System.Collections.Concurrent;
using System.Reflection;

namespace Milvasoft.Components.Rest.Request;

/// <summary>
/// How a client request uses a property.
/// </summary>
public enum RequestPropertyUsage
{
    /// <summary>
    /// <see cref="FilterRequest"/> criteria.
    /// </summary>
    Filter,

    /// <summary>
    /// <see cref="SortRequest"/>.
    /// </summary>
    Sort,

    /// <summary>
    /// <see cref="AggregationRequest"/> criteria.
    /// </summary>
    Aggregate,
}

/// <summary>
/// Global options of client driven list requests (filtering, sorting, aggregation and paging).
/// Configure once at startup.
/// </summary>
public static class RequestQueryOptions
{
    /// <summary>
    /// Maximum number of rows a list request can ask for. Larger <c>RowCount</c> values are reduced to this value. Zero or less disables the cap. Default is 1000.
    /// </summary>
    public static int MaxRowCount { get; set; } = 1000;

    /// <summary>
    /// When true, only properties explicitly marked with <see cref="FilterableAttribute"/> (filtering and sorting) or
    /// <see cref="AggregatableAttribute"/> (aggregation) with true can be used. Default is false.
    /// </summary>
    public static bool RequireExplicitOptIn { get; set; }

    /// <summary>
    /// Property names (case insensitive) that clients cannot filter, sort or aggregate by unless they are explicitly marked with
    /// <see cref="FilterableAttribute"/> / <see cref="AggregatableAttribute"/>. Names containing "Password" or "Secret" are always treated as sensitive.
    /// </summary>
    public static HashSet<string> SensitivePropertyNames { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "PasswordHash", "Password", "PasswordSalt", "Salt", "SecurityStamp", "ConcurrencyStamp",
        "Token", "TokenHash", "AccessToken", "RefreshToken", "ApiKey", "AuthenticatorKey", "RecoveryCodes",
    };

    /// <summary>
    /// Applies <see cref="MaxRowCount"/> to a requested row count.
    /// </summary>
    /// <param name="rowCount"></param>
    /// <returns></returns>
    public static int? CapRowCount(int? rowCount) => rowCount is int count && MaxRowCount > 0 && count > MaxRowCount ? MaxRowCount : rowCount;
}

/// <summary>
/// Decides which properties clients may filter, sort or aggregate by.
/// </summary>
/// <remarks>
/// <para>A property is allowed unless one of these applies (an explicit <see cref="FilterableAttribute"/>(true) for filtering/sorting or
/// <see cref="AggregatableAttribute"/>(true) for aggregation always allows it):</para>
/// <list type="bullet">
/// <item>It is marked <see cref="FilterableAttribute"/>(false) (filtering) or <see cref="AggregatableAttribute"/>(false) (aggregation).</item>
/// <item>It is sensitive: its name is in <see cref="RequestQueryOptions.SensitivePropertyNames"/> or contains "Password"/"Secret",
/// or it is marked with <see cref="EncryptedAttribute"/>, <see cref="MaskAttribute"/>, <see cref="MaskByRoleAttribute"/> or <see cref="HideByRoleAttribute"/>.</item>
/// <item><see cref="RequestQueryOptions.RequireExplicitOptIn"/> is true and the property is not explicitly allowed.</item>
/// </list>
/// </remarks>
public static class RequestPropertyPolicy
{
    private static readonly ConcurrentDictionary<(PropertyInfo Property, RequestPropertyUsage Usage, bool Strict), bool> _cache = new();

    /// <summary>
    /// Whether clients may use <paramref name="property"/> for <paramref name="usage"/>.
    /// </summary>
    /// <param name="property"></param>
    /// <param name="usage"></param>
    /// <returns></returns>
    public static bool IsAllowed(PropertyInfo property, RequestPropertyUsage usage)
        => property != null && _cache.GetOrAdd((property, usage, RequestQueryOptions.RequireExplicitOptIn), static key => Evaluate(key.Property, key.Usage, key.Strict));

    /// <summary>
    /// Throws a user friendly exception when clients may not use <paramref name="property"/> for <paramref name="usage"/>.
    /// </summary>
    /// <param name="property"></param>
    /// <param name="usage"></param>
    /// <param name="requestedName">The name the client sent.</param>
    /// <exception cref="MilvaUserFriendlyException"></exception>
    public static void EnsureAllowed(PropertyInfo property, RequestPropertyUsage usage, string requestedName)
    {
        if (!IsAllowed(property, usage))
            throw NotAllowed(requestedName, usage);
    }

    /// <summary>
    /// Creates the exception for a property that does not exist or cannot be used.
    /// </summary>
    /// <param name="requestedName"></param>
    /// <param name="usage"></param>
    /// <returns></returns>
    public static MilvaUserFriendlyException NotAllowed(string requestedName, RequestPropertyUsage usage)
        => new($"'{requestedName}' is not a valid property for {GetUsageText(usage)}.", MilvaException.InvalidParameter);

    private static bool Evaluate(PropertyInfo property, RequestPropertyUsage usage, bool strict)
    {
        bool? explicitlyAllowed = usage switch
        {
            RequestPropertyUsage.Aggregate => property.GetCustomAttribute<AggregatableAttribute>()?.Aggregatable,
            RequestPropertyUsage.Filter => property.GetCustomAttribute<FilterableAttribute>()?.Filterable,
            _ => property.GetCustomAttribute<FilterableAttribute>() is { Filterable: true } ? true : null,
        };

        if (explicitlyAllowed.HasValue)
            return explicitlyAllowed.Value;

        if (strict || IsSensitive(property))
            return false;

        return true;
    }

    private static bool IsSensitive(PropertyInfo property)
        => RequestQueryOptions.SensitivePropertyNames.Contains(property.Name)
           || property.Name.Contains("Password", StringComparison.OrdinalIgnoreCase)
           || property.Name.Contains("Secret", StringComparison.OrdinalIgnoreCase)
           || property.IsDefined(typeof(EncryptedAttribute), true)
           || property.IsDefined(typeof(MaskAttribute), true)
           || property.IsDefined(typeof(MaskByRoleAttribute), true)
           || property.IsDefined(typeof(HideByRoleAttribute), true);

    private static string GetUsageText(RequestPropertyUsage usage) => usage switch
    {
        RequestPropertyUsage.Filter => "filtering",
        RequestPropertyUsage.Sort => "sorting",
        _ => "aggregation",
    };
}
