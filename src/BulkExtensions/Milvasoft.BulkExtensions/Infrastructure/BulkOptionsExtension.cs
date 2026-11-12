using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Milvasoft.BulkExtensions.Infrastructure;

/// <summary>
/// Registers a <see cref="BulkProvider"/> in the internal service provider of a <see cref="DbContext"/>.
/// Provider packages expose a friendlier method, such as <c>UseBulkExtensions()</c> on the provider options builder.
/// </summary>
public sealed class BulkOptionsExtension : IDbContextOptionsExtension
{
    private DbContextOptionsExtensionInfo _info;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="providerType">A non abstract type deriving from <see cref="BulkProvider"/> with a public parameterless constructor.</param>
    public BulkOptionsExtension(Type providerType)
    {
        ArgumentNullException.ThrowIfNull(providerType);

        if (!typeof(BulkProvider).IsAssignableFrom(providerType) || providerType.IsAbstract || providerType.GetConstructor(Type.EmptyTypes) is null)
            throw new ArgumentException($"'{providerType}' must be a non abstract class deriving from {nameof(BulkProvider)} with a public parameterless constructor.", nameof(providerType));

        ProviderType = providerType;
    }

    /// <summary>
    /// The registered provider type.
    /// </summary>
    public Type ProviderType { get; }

    /// <inheritdoc/>
    public DbContextOptionsExtensionInfo Info => _info ??= new ExtensionInfo(this);

    /// <inheritdoc/>
    public void ApplyServices(IServiceCollection services) => services.TryAddSingleton(typeof(BulkProvider), ProviderType);

    /// <inheritdoc/>
    public void Validate(IDbContextOptions options)
    {
    }

    private sealed class ExtensionInfo(BulkOptionsExtension extension) : DbContextOptionsExtensionInfo(extension)
    {
        private new BulkOptionsExtension Extension => (BulkOptionsExtension)base.Extension;

        public override bool IsDatabaseProvider => false;

        public override string LogFragment => $"BulkProvider={Extension.ProviderType.Name} ";

        public override int GetServiceProviderHashCode() => Extension.ProviderType.GetHashCode();

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => other is ExtensionInfo otherInfo && otherInfo.Extension.ProviderType == Extension.ProviderType;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
            => debugInfo["Milvasoft:BulkExtensions:Provider"] = Extension.ProviderType.FullName;
    }
}

/// <summary>
/// Methods for registering a <see cref="BulkProvider"/>.
/// </summary>
public static class BulkDbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Registers <typeparamref name="TProvider"/> as the bulk provider of the context. Provider packages call this method.
    /// </summary>
    /// <typeparam name="TProvider">Bulk provider type.</typeparam>
    /// <param name="optionsBuilder">Options builder of the context.</param>
    /// <returns>The same builder.</returns>
    public static DbContextOptionsBuilder UseBulkProvider<TProvider>(this DbContextOptionsBuilder optionsBuilder) where TProvider : BulkProvider, new()
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(new BulkOptionsExtension(typeof(TProvider)));

        return optionsBuilder;
    }
}