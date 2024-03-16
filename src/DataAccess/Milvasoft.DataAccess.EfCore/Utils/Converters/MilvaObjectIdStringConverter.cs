using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MongoDB.Bson;

namespace Milvasoft.DataAccess.EfCore.Utils.Converters;

/// <summary>
/// Converts <see cref="ObjectId"/> to <see cref="string"/> or <see cref="string"/> to <see cref="ObjectId"/>.
/// </summary>
/// <remarks>
/// Creates a new <see cref="MilvaObjectIdStringConverter"/> instance.
/// </remarks>
/// <param name="mappingHints">Entity Framework mapping hints</param>
public sealed class MilvaObjectIdStringConverter(ConverterMappingHints mappingHints = null) : ValueConverter<ObjectId, string>(to => to.ToString(), from => ObjectId.Parse(from), mappingHints)
{
}
