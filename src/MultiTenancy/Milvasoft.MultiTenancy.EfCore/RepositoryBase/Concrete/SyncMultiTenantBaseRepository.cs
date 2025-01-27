using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Milvasoft.Core.EntityBases.Abstract;
using Milvasoft.Core.Helpers;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.DataAccess.EfCore.Bulk;

namespace Milvasoft.MultiTenancy.EfCore.RepositoryBase.Concrete;

/// <summary>
///  Base repository for concrete repositories.
/// </summary>
public abstract partial class MultiTenantBaseRepository<TEntity, TContext> where TEntity : class, IMilvaEntity where TContext : DbContext, IMilvaBulkDbContextBase, IMultiTenantDbContext
{
    #region Bulk 

    /// <summary>
    /// Bulk add operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <returns></returns>
    public override void BulkAdd(List<TEntity> entities, Action<BulkConfig> bulkConfig = null)
    {
        if (!entities.IsNullOrEmpty())
        {
            SetTenantId(entities);

            if (_dataAccessConfiguration.Auditing.AuditModificationDate)
                SetPerformTimeValues(entities, EntityPropertyNames.CreationDate);

            if (_dataAccessConfiguration.Auditing.AuditModifier)
                SetPerformerUserValues(entities, EntityPropertyNames.CreatorUserName);

            _dbContext.BulkInsert(entities, bulkConfig);
        }
    }

    #endregion
}
