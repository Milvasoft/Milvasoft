using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Linq;

namespace Milvasoft.Helpers.DataAccess.IncludeLibrary
{
    /// <summary>
    ///<para><b>EN: </b> Supports queryable Include/ThenInclude chaining operators.</para> 
    ///<para><b>TR: </b>Sorgulanabilir Include / ThenInclude zincirleme operatörlerini destekler.</para> 
    /// </summary>
    public interface IIncludable { }

    /// <summary>
    ///<para><b>EN: </b>Supports queryable Include/ThenInclude chaining operators.</para> 
    ///<para><b>TR: </b>Sorgulanabilir Include / ThenInclude zincirleme operatörlerini destekler.</para> 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IIncludable<out TEntity> : IIncludable { }

    /// <summary>
    ///<para><b>EN: </b>Supports queryable Include/ThenInclude chaining operators.</para> 
    ///<para><b>TR: </b>Sorgulanabilir Include / ThenInclude zincirleme operatörlerini destekler.</para> 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    public interface IIncludable<out TEntity, out TProperty> : IIncludable<TEntity> { }

    /// <summary>
    ///<para><b>EN: </b> Supports queryable Include/ThenInclude chaining operators.</para> 
    ///<para><b>TR: </b>Sorgulanabilir Include / ThenInclude zincirleme operatörlerini destekler.</para> 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    internal class Includable<TEntity> : IIncludable<TEntity> where TEntity : class
    {
        internal IQueryable<TEntity> Input { get; }

        internal Includable(IQueryable<TEntity> queryable)
        {
            // C# 7 syntax, just rewrite it "old style" if you do not have Visual Studio 2017
            Input = queryable ?? throw new ArgumentNullException(nameof(queryable));
        }
    }

    /// <summary>
    ///<para><b>EN: </b>Supports queryable Include/ThenInclude chaining operators.</para> 
    ///<para><b>TR: </b>Sorgulanabilir Include / ThenInclude zincirleme operatörlerini destekler.</para> 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    internal class Includable<TEntity, TProperty> :
        Includable<TEntity>, IIncludable<TEntity, TProperty>
        where TEntity : class
    {
        internal IIncludableQueryable<TEntity, TProperty> IncludableInput { get; }

        internal Includable(IIncludableQueryable<TEntity, TProperty> queryable) :
            base(queryable)
        {
            IncludableInput = queryable;
        }
    }
}
