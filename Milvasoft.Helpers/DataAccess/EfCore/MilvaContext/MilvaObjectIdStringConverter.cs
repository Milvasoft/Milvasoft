using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MongoDB.Bson;

namespace Milvasoft.Helpers.DataAccess.EfCore.MilvaContext;

/// <summary>
/// Converts <see cref="ObjectId"/> to <see cref="string"/> or <see cref="string"/> to <see cref="ObjectId"/>.
/// </summary>
public sealed class MilvaObjectIdStringConverter : ValueConverter<ObjectId, string>
{
    /// <summary>
    /// Creates a new <see cref="MilvaObjectIdStringConverter"/> instance.
    /// </summary>
    /// <param name="mappingHints">Entity Framework mapping hints</param>
    public MilvaObjectIdStringConverter(ConverterMappingHints mappingHints = null)
        : base(to => to.ToString(), from => ObjectId.Parse(from), mappingHints) { }
}
