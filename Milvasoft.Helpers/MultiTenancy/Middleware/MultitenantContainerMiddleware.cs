using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Milvasoft.Helpers.MultiTenancy.EntityBase;
using Milvasoft.Helpers.MultiTenancy.LifetimeManagement;
using System;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.MultiTenancy.Middleware
{
    /// <summary>
    /// Sets the lifetime to current tenant scope.
    /// </summary>
    /// <typeparam name="TTenant"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class MultitenantContainerMiddleware<TTenant, TKey>
    where TTenant : class,IMilvaTenantBase<TKey>
    where TKey : struct, IEquatable<TKey>
    {
        private readonly RequestDelegate next;

        /// <summary>
        /// Initializes new instance of <see cref="MultitenantContainerMiddleware{TTenant, TKey}"/>
        /// </summary>
        /// <param name="next"></param>
        public MultitenantContainerMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        /// <summary>
        /// Invokes the method or constructor reflected by this MethodInfo instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="multiTenantContainerAccessor"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context, Func<MultiTenantContainer<TTenant, TKey>> multiTenantContainerAccessor)
        {
            //Set to current tenant container.
            //Begin new scope for request as ASP.NET Core standard scope is per-request
            context.RequestServices = new AutofacServiceProvider(multiTenantContainerAccessor().GetCurrentTenantScope().BeginLifetimeScope());

            await next.Invoke(context);
        }
    }
}
