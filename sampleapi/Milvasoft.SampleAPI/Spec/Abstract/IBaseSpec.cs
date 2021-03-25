using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Spec.Abstract
{
    /// <summary>
    /// <para><b>EN: </b>Filtering <typeparamref name="TEntity"/> object lists.</para>
    /// <para><b>TR: </b><typeparamref name="TEntity"/> nesne listelerini filtreleme.</para>
    /// </summary>
    public interface IBaseSpec<TEntity>
    {

        /// <summary>
        /// <para><b>EN: </b>Filtering TEntity list by  with requested properties.</para>
        /// <para><b>TR: </b>TEntity listesi, istenen özelliklere göre filtreleniyor.</para>
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        List<TEntity> GetFilteredEntities(IEnumerable<TEntity> entities);

        /// <summary>
        /// Converts spesifications to expression.
        /// </summary>
        /// <returns></returns>
        Expression<Func<TEntity, bool>> ToExpression();
    }
}
