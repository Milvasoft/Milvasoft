using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Milvasoft.Helpers.DataAccess.Abstract.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.DataAccess.Abstract
{
    /// <summary>
    /// Helper repository for general DbContext(<typeparamref name="TContext"/>) operations.
    /// </summary>
    public interface IContextRepository<TContext> where TContext : DbContext
    {
        /// <summary>
        /// Ignores soft delete for next process. Runs correctly, if <typeparamref name="TContext"/> inherit from MilvaDbContext.
        /// </summary>
        void IgnoreSoftDeleteForNextProcess();

        /// <summary>
        /// Activate soft delete. Runs correctly, if <typeparamref name="TContext"/> inherit from MilvaDbContext.
        /// </summary>
        void ActivateSoftDelete();

        /// <summary>
        /// Executes sql query to database asynchronously.(e.g. trigger, event)
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task ExecuteQueryAsync(string query);

        /// <summary>
        /// Applies transaction process to requested function.
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        Task ApplyTransactionAsync(Func<Task> function);

        /// <summary>
        /// Applies transaction process to requested function.
        /// </summary>
        /// <param name="function"></param>
        /// <param name="rollbackFunction"></param>
        /// <returns></returns>
        Task ApplyTransactionAsync(Func<Task> function, Action rollbackFunction);

        /// <summary>
        /// Applies transaction process to requested function.
        /// </summary>
        /// <param name="function"></param>
        /// <param name="rollbackFunction"></param>
        /// <returns></returns>
        Task ApplyTransactionAsync(Func<Task> function, Func<Task> rollbackFunction);

        /// <summary>
        /// User update process.
        /// </summary>
        void InitializeUpdating<TEntity, TKey>(TEntity entity) where TEntity : class, IBaseEntity<TKey>
                                                               where TKey : struct, IEquatable<TKey>;

        /// <summary>
        /// <para> Removes expired tokens from the system. </para>
        /// </summary>
        /// 
        /// <remarks>
        /// 
        /// <para><b>Remarks :</b></para>
        /// 
        /// <para> Your DbContext(<typeparamref name="TContext"/>) must inherit from <see cref="IdentityDbContext"/>. </para>
        /// <para> Your DbContext(<typeparamref name="TContext"/>) must contain <see cref="IdentityUserToken{TKey}"/> DbSet of name "UserTokens". </para>
        /// 
        /// </remarks>
        /// 
        /// <exception cref="ArgumentException"> Throwns when type of <typeparamref name="TContext"/>'s properties doesn't contain <see cref="IdentityUserToken{TKey}"/> of name 'UserTokens'. </exception>
        /// 
        /// <param name="userManager"> User manager </param>
        /// <param name="loginProvider"> Login Provider (e.g. Facebook) </param>
        /// <param name="tokenName"> Token Type </param>
        /// <param name="cachedTokenDictionary"> Dictionary params : Key = userName of <typeparamref name="TUser"/>. Value = JWT security Token as string. </param>
        /// <returns></returns>
        Task RemoveExpiredTokensAsync<TUser, TKey>(UserManager<TUser> userManager,
                                                   string loginProvider,
                                                   string tokenName,
                                                   Dictionary<string, string> cachedTokenDictionary = null) where TUser : IdentityUser<TKey>, IBaseEntity<TKey>
                                                                                                            where TKey : struct, IEquatable<TKey>;

        /// <summary>
        /// Gets requested DbSet by <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        DbSet<TEntity> GetDbSet<TEntity>() where TEntity : class;

        /// <summary>
        /// Returns whether or not the entity that satisfies this condition exists. 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> ExistsAsync<TEntity, TKey>(TKey id) where TEntity : class, IBaseEntity<TKey> where TKey : struct, IEquatable<TKey>;
    }
}
