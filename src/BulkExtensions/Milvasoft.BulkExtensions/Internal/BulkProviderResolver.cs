using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Milvasoft.BulkExtensions.Infrastructure;
using System.Collections.Concurrent;

namespace Milvasoft.BulkExtensions.Internal;

/// <summary>
/// Finds the <see cref="BulkProvider"/> of a context. A provider registered through the context options wins.
/// Otherwise the provider package that matches the EF Core database provider is loaded by name, so referencing the package is enough.
/// </summary>
internal static class BulkProviderResolver
{
    /// <summary>
    /// EF Core provider name to the assembly qualified name of the bulk provider for it.
    /// New provider packages are added here.
    /// </summary>
    private static readonly Dictionary<string, string> _knownProviders = new(StringComparer.Ordinal)
    {
        ["Npgsql.EntityFrameworkCore.PostgreSQL"] = "Milvasoft.BulkExtensions.PostgreSql.NpgsqlBulkProvider, Milvasoft.BulkExtensions.PostgreSql",
    };

    private static readonly ConcurrentDictionary<string, BulkProvider> _conventionProviders = new(StringComparer.Ordinal);

    public static BulkProvider Resolve(DbContext context)
    {
        if (!context.Database.IsRelational())
            throw new InvalidOperationException($"No bulk provider is available for the database provider '{context.Database.ProviderName}'. Bulk operations need a relational database provider.");

        var serviceProvider = ((IInfrastructure<IServiceProvider>)context).Instance;

        if (serviceProvider.GetService(typeof(BulkProvider)) is BulkProvider registered)
            return registered;

        var providerName = context.Database.ProviderName;

        if (providerName is not null && _knownProviders.TryGetValue(providerName, out var typeName))
        {
            var provider = _conventionProviders.GetOrAdd(typeName, static name =>
            {
                var type = Type.GetType(name, throwOnError: false);

                return type is null ? null : (BulkProvider)Activator.CreateInstance(type);
            });

            if (provider is not null)
                return provider;

            // Do not cache a missing assembly; it can be loaded later in some hosting models.
            _conventionProviders.TryRemove(typeName, out _);

            var packageName = typeName[(typeName.IndexOf(',') + 1)..].Trim();

            throw new InvalidOperationException($"Bulk operations for '{providerName}' need the '{packageName}' package. Add a reference to it.");
        }

        throw new InvalidOperationException($"No bulk provider is available for the database provider '{providerName}'. "
                                            + $"Reference a Milvasoft.BulkExtensions provider package and register it in the context options.");
    }
}