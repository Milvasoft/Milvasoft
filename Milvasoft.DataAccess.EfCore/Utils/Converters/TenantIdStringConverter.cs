using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Milvasoft.Core.EntityBase.MultiTenancy;

namespace Milvasoft.DataAccess.EfCore.Utils.Converters;

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
