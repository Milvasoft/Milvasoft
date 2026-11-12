using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Milvasoft.BulkExtensions.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Milvasoft.BulkExtensions.PostgreSql;

/// <summary>
/// Methods for enabling PostgreSQL bulk operations on a context.
/// </summary>
/// <remarks>
/// Registration is optional: when this package is referenced, bulk operations find the PostgreSQL provider by themselves.
/// Registering makes the dependency explicit and fails early if the package is missing.
/// </remarks>
public static class NpgsqlBulkDbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Enables PostgreSQL bulk operations, for example <c>options.UseNpgsql(connectionString, o => o.UseBulkExtensions())</c>.
    /// </summary>
    /// <param name="builder">Npgsql options builder.</param>
    /// <returns>The same builder.</returns>
    public static NpgsqlDbContextOptionsBuilder UseBulkExtensions(this NpgsqlDbContextOptionsBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        ((IRelationalDbContextOptionsBuilderInfrastructure)builder).OptionsBuilder.UseBulkProvider<NpgsqlBulkProvider>();

        return builder;
    }

    /// <summary>
    /// Enables PostgreSQL bulk operations. Call it together with <c>UseNpgsql</c>.
    /// </summary>
    /// <param name="builder">Options builder of the context.</param>
    /// <returns>The same builder.</returns>
    public static DbContextOptionsBuilder UseNpgsqlBulkExtensions(this DbContextOptionsBuilder builder) => builder.UseBulkProvider<NpgsqlBulkProvider>();
}