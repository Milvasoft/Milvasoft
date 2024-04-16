using Milvasoft.Types.Structs;
using System.Reflection;

namespace Milvasoft.Core.EntityBases.Abstract;

/// <summary>
/// Dummy interface given for constraint
/// </summary>
public interface IBaseDto : IMilvaEntity
{
    /// <summary>
    /// Returns a collection of PropertyInfo objects that represent properties of the DTO that implements <see cref="IUpdateProperty"/>.
    /// </summary>
    /// <returns>A collection of PropertyInfo objects representing the updatable properties of the DTO.</returns>
    public IEnumerable<PropertyInfo> GetUpdatableProperties();
}
