using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Milvasoft.Core.EntityBase.MultiTenancy;

/// <summary>
/// Converts <see cref="TenantId"/> to <see cref="string"/> or <see cref="string"/> to <see cref="TenantId"/>.
/// </summary>
public sealed class TenantIdStringConverter : ValueConverter<TenantId, string>
{
    /// <summary>
    /// Creates a new <see cref="TenantIdStringConverter"/> instance.
    /// </summary>
    /// <param name="mappingHints">Entity Framework mapping hints</param>
    public TenantIdStringConverter(ConverterMappingHints mappingHints = null)
        : base(to => to.ToString(), from => TenantId.Parse(from), mappingHints) { }
}
