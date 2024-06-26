﻿using Milvasoft.DataAccess.EfCore.DbContextBase;
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
    /// Entity assembly for <see cref="MilvaDbContext.GetLookupsAsync(LookupRequest)"/> operations.
    /// </summary>
    public string EntityAssemblyName { get; set; }

    /// <summary>
    /// Allowed entity names for <see cref="MilvaDbContext.GetLookupsAsync(LookupRequest)"/> operations.
    /// </summary>
    public List<string> AllowedEntityNamesForLookup { get; set; } = [];

    /// <summary>
    /// Allowed entity count for dynamic fetch. Default value is 5.
    /// </summary>
    public int MaxAllowedPropertyCountForLookup { get; set; } = 5;

    /// <summary>
    /// Gets assembly for entity operations.    
    /// </summary>
    /// <returns></returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S2696:Instance members should not write to \"static\" fields", Justification = "<Pending>")]
    public Assembly GetEntityAssembly() => _entityAssembly ??= Assembly.Load(EntityAssemblyName);
}