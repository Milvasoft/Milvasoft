using Microsoft.EntityFrameworkCore;

namespace Milvasoft.Interception.Ef;

/// <summary>
/// This contract specifies that the implementing class should have a method called GetDbContext which returns an instance of DbContext. 
/// The GetDbContext method takes a parameter of type Type which represents the context type.
/// </summary>
public interface ICanRetrieveDbContext
{
    /// <summary>
    /// Returns <see cref="DbContext"/> of <paramref name="contextType"/>.
    /// </summary>
    /// <param name="contextType"></param>
    /// <returns></returns>
    public DbContext GetDbContext(Type contextType);
}
