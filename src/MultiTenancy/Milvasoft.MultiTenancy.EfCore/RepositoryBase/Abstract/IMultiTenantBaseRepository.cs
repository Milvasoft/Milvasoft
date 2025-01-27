using Milvasoft.Core.EntityBases.Abstract;
using Milvasoft.DataAccess.EfCore.Bulk.RepositoryBase.Abstract;

namespace Milvasoft.MultiTenancy.EfCore.RepositoryBase.Abstract;

/// <summary>
/// Base repository for concrete repositories. All repositories must be have this methods.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IMultiTenantBaseRepository<TEntity> : IBulkBaseRepository<TEntity> where TEntity : class, IMilvaEntity
{
}
