using Microsoft.EntityFrameworkCore;
using Milvasoft.Helpers.DataAccess.Abstract.Entity;
using Milvasoft.Helpers.DataAccess.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Data.Concrete
{
    public class BaseRepo<TEntity, TKey, TContext> : BaseRepository<TEntity, TKey, TContext> where TEntity : class, IBaseEntity<TKey>
                                                                                             where TKey : struct, IEquatable<TKey>
                                                                                             where TContext : DbContext
    {
        /// <summary>
        /// Constructor of <c>BillRepository</c> class.
        /// </summary>
        /// <param name="dbContext"></param>
        public BaseRepo(TContext dbContext) : base(dbContext)
        {

        }
    }
}
