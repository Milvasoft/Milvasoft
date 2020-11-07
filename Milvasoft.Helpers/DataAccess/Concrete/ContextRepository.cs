using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Milvasoft.Helpers.DataAccess.Abstract;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.DataAccess.Concrete
{
    /// <summary>
    /// Helper repository for general DbContext(<typeparamref name="TContext"/>) operations.
    /// </summary>
    public class ContextRepository<TContext> : IContextRepository<TContext> where TContext : DbContext
    {
        /// <summary>
        /// DbContext object.
        /// </summary>
        protected TContext _dbContext;

        /// <summary>
        /// Constructor of ContextRepository for inject context.
        /// </summary>
        /// <param name="dbContext"></param>
        public ContextRepository(TContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        ///<para> Executes sql query to database asynchronously.(e.g. trigger, event). </para> 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public virtual async Task ExecuteQueryAsync(string query) => await _dbContext.Database.ExecuteSqlRawAsync(query).ConfigureAwait(false);

        /// <summary>
        /// Applies transaction process to requested function.
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        public async Task ApplyTransactionAsync(Func<Task> function)
        {
            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                var transaction = await _dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
                try
                {
                    await function().ConfigureAwait(false);
                    await transaction.CommitAsync().ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);
                    throw exception;
                }
            });
        }

        /// <summary>
        /// Applies transaction process to requested function.
        /// </summary>
        /// <param name="function"></param>
        /// <param name="rollBackFunction"></param>
        /// <returns></returns>
        public async Task ApplyTransactionAsync(Func<Task> function, Func<Task> rollBackFunction)
        {
            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                var transaction = await _dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
                try
                {
                    await function().ConfigureAwait(false);
                    await transaction.CommitAsync().ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);
                    await rollBackFunction().ConfigureAwait(false);
                    throw exception;
                }
            });
        }

        /// <summary>
        /// Applies transaction process to requested function.
        /// </summary>
        /// <param name="function"></param>
        /// <param name="rollBackFunction"></param>
        /// <returns></returns>
        public async Task ApplyTransactionAsync(Func<Task> function, Action rollBackFunction)
        {
            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                var transaction = await _dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
                try
                {
                    await function().ConfigureAwait(false);
                    await transaction.CommitAsync().ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);
                    rollBackFunction();
                    throw exception;
                }
            });
        }

        /// <summary>
        /// <para> User update process. </para>
        /// </summary>
        public void InitializeUpdating<TEntity, TKey>(TEntity entity) where TEntity : class, IBaseEntity<TKey>
                                                                      where TKey : IEquatable<TKey>
        {
            var localEntity = _dbContext.Set<TEntity>().Local.FirstOrDefault(u => u.Id.Equals(entity.Id));
            if (localEntity == null)
                return;
            _dbContext.Entry(localEntity).State = EntityState.Detached;
        }

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
        public async Task RemoveExpiredTokensAsync<TUser, TKey>(UserManager<TUser> userManager,
                                                                string loginProvider,
                                                                string tokenName,
                                                                Dictionary<string, string> cachedTokenDictionary = null) where TUser : IdentityUser<TKey>
                                                                                                                         where TKey : IEquatable<TKey>
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            string userTokensString = "UserTokens";

            if (!PropertyExists<TContext>(userTokensString))
                throw new ArgumentException($"Type of {typeof(TContext)}'s properties doesn't contain '{userTokensString}'.");

            var userTokens = (DbSet<IdentityUserToken<TKey>>)_dbContext.GetType().GetProperty(userTokensString).GetValue(_dbContext, null);

            foreach (var userToken in userTokens)
            {
                if (tokenHandler.ReadJwtToken(userToken.Value).ValidTo <= DateTime.Now)
                {
                    var user = ((List<TUser>)_dbContext.GetType().GetProperty(userTokensString).GetValue(_dbContext, null)).First(u => u.Id.Equals(userToken.UserId));

                    await userManager.RemoveAuthenticationTokenAsync(user, loginProvider, tokenName).ConfigureAwait(false);
                    if (cachedTokenDictionary.ContainsKey(user.UserName))
                        cachedTokenDictionary.Remove(user.UserName);
                }
            }
        }
        private static bool PropertyExists<T>(string propertyName)
        {
            return typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase |
                                                       BindingFlags.Public | BindingFlags.Instance) != null;
        }

    }
}
