using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Milvasoft.Core.EntityBases.MultiTenancy;

namespace Milvasoft.DataAccess.EfCore.Utils.Converters;

/// <summary>
/// Converts <see cref="TenantId"/> to <see cref="string"/> or <see cref="string"/> to <see cref="TenantId"/>.
/// </summary>
/// <remarks>
/// Creates a new <see cref="TenantIdStringConverter"/> instance.
/// </remarks>
/// <param name="mappingHints">Entity Framework mapping hints</param>
public sealed class TenantIdStringConverter(ConverterMappingHints mappingHints = null) : ValueConverter<TenantId, string>(to => to.ToString(), from => TenantId.Parse(from), mappingHints)
{
}
