using Milvasoft.Core.Abstractions.Cache;
using Milvasoft.DataAccess.EfCore.DbContextBase;
using Milvasoft.DataAccess.EfCore.Utils.Enums;
using System.Reflection;

namespace Milvasoft.DataAccess.EfCore.Configuration;

/// <summary>
/// IOptions aware custom MilvaDbContext configurations.
/// </summary>
public class DbContextConfiguration
{
    private static Assembly _entityAssembly = null;

    /// <summary>
    /// If it is true converts all saved DateTimes to UTC. Default value is false.
    /// </summary>
    public bool UseUtcForDateTimes { get; set; } = false;

    /// <summary>
    /// Soft delete state when delete operation. Default value is false.
    /// </summary>
    public SoftDeletionState DefaultSoftDeletionState { get; set; } = SoftDeletionState.Passive;

    /// <summary>
    /// Entity assembly for <see cref="MilvaDbContextBase.GetLookupsAsync(Utils.LookupModels.LookupRequest)"/> operations.
    /// </summary>
    public string EntityAssemblyName { get; set; }

    /// <summary>
    /// Allowed entity names for <see cref="MilvaDbContextBase.GetLookupsAsync(Utils.LookupModels.LookupRequest)"/> operations.
    /// </summary>
    public List<string> AllowedEntityNamesForLookup { get; set; } = [];

    /// <summary>
    /// Current user id getter method for auditing operations.
    /// </summary>
    public Func<IServiceProvider, string> GetCurrentUserNameMethod { get; set; }

    /// <summary>
    /// Current language id getter method for multi language operations.
    /// </summary>
    public Func<IServiceProvider, object> GetCurrentLanguageIdMethod { get; set; }

    /// <summary>
    /// Current language id getter method for multi language operations.
    /// </summary>
    public Func<IServiceProvider, object> GetDefaultLanguageIdMethod { get; set; }

    /// <summary>
    /// Gets generic accessor type as <see cref="ICacheAccessor{TAccessor}"/>
    /// </summary>
    /// <returns></returns>
    public Assembly GetEntityAssembly() => _entityAssembly ??= Assembly.Load(EntityAssemblyName);
}