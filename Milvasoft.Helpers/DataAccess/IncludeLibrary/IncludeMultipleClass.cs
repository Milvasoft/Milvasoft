using System;
using System.Linq;

namespace Milvasoft.Helpers.DataAccess.IncludeLibrary
{
    /// <summary>
    /// Allows to include multiple class.
    /// </summary>
    public static class IncludeMultipleClass
    {
        /// <summary>
        ///<para><b>EN: </b> Supports queryable Include/ThenInclude chaining operators.</para> 
        ///<para><b>TR: </b>Sorgulanabilir Include / ThenInclude zincirleme operatörlerini destekler.</para> 
        /// </summary>
        /// <example>
        /// var personnelTable = _personnelTableRepository.GetAllWithIncludes(i => i.Include(p=>p.personnel).Include(p=>p.tableSetting));
        /// </example>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="includes"></param>
        /// <returns></returns>
        public static IQueryable<T> IncludeMultiple<T>(this IQueryable<T> query,
       Func<IIncludable<T>, IIncludable> includes)
       where T : class
        {
            if (includes == null)
                return query;

            var includable = (Includable<T>)includes(new Includable<T>(query));
            return includable.Input;
        }
    }
}
