using Microsoft.EntityFrameworkCore;
using Milvasoft.Helpers.DataAccess.EfCore.Abstract.Entity;
using Milvasoft.Helpers.DataAccess.EfCore.Concrete;
using System;

namespace Milvasoft.SampleAPI.Data.Repositories
{
    public class EducationRepositoryBase<TEntity, TKey, TContext> : BaseRepository<TEntity, TKey, TContext>
    where TEntity : class, IBaseEntity<TKey>
    where TKey : struct, IEquatable<TKey>
    where TContext : DbContext
    {
        public EducationRepositoryBase(TContext context) : base(context) { }
    }
}
