using Microsoft.EntityFrameworkCore;

namespace Milvasoft.Interception.Ef;

public interface ICanRetrieveDbContext
{
    public DbContext GetDbContext(Type contextType);
}
