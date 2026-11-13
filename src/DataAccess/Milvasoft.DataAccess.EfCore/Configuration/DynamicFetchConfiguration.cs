using Milvasoft.Components.Rest.Request;
using Milvasoft.DataAccess.EfCore.DbContextBase;
using Milvasoft.DataAccess.EfCore.Utils.LookupModels;
using System.Reflection;

namespace Milvasoft.DataAccess.EfCore.Configuration;

/// <summary>
/// IOptions aware custom MilvaDbContext configurations.
/// </summary>
public class DynamicFetchConfiguration
{
    private static Assembly _entityAssembly = null;

    /// <summary>
    /// Entity assembly for <see cref="MilvaDbContext.GetLookupsAsync(LookupRequest, CancellationToken)"/> and <see cref="MilvaDbContext.GetPropertyValuesAsync(EntityPropertyValuesRequest, CancellationToken)"/> operations.
    /// </summary>
    public string EntityAssemblyName { get; set; }

    /// <summary>
    /// Allowed entity names for <see cref="MilvaDbContext.GetLookupsAsync(LookupRequest, CancellationToken)"/> and <see cref="MilvaDbContext.GetPropertyValuesAsync(EntityPropertyValuesRequest, CancellationToken)"/> operations.
    /// </summary>
    public List<string> AllowedEntityNamesForLookup { get; set; } = [];

    /// <summary>
    /// Allowed entity count for dynamic fetch. Default value is 5.
    /// </summary>
    public int MaxAllowedPropertyCountForLookup { get; set; } = 5;

    /// <summary>
    /// Maximum number of rows returned for each entity of a lookup request and for a property values request. Larger results are truncated (after sorting).
    /// When null, <see cref="RequestQueryOptions.MaxRowCount"/> (default 1000) is used. Zero or less disables the limit.
    /// </summary>
    public int? MaxRowCount { get; set; }

    /// <summary>
    /// Gets the row limit to apply (see <see cref="MaxRowCount"/>), or null when there is no limit.
    /// </summary>
    /// <returns></returns>
    public int? GetEffectiveMaxRowCount()
    {
        var maxRowCount = MaxRowCount ?? RequestQueryOptions.MaxRowCount;

        return maxRowCount > 0 ? maxRowCount : null;
    }

    /// <summary>
    /// Gets assembly for entity operations.    
    /// </summary>
    /// <returns></returns>
    public Assembly GetEntityAssembly() => _entityAssembly ??= Assembly.Load(EntityAssemblyName);

    /// <summary>
    /// Gets types of <see cref="GetEntityAssembly"/>. Types are loaded once and cached, instead of scanning the assembly on every request.
    /// </summary>
    /// <returns></returns>
    public Type[] GetEntityAssemblyTypes()
    {
        var assembly = GetEntityAssembly();

        var cache = _entityAssemblyTypes;

        if (cache?.Item1 != assembly)
        {
            cache = Tuple.Create(assembly, assembly.GetTypes());
            _entityAssemblyTypes = cache;
        }

        return cache.Item2;
    }

    private static Tuple<Assembly, Type[]> _entityAssemblyTypes;
}