using Autofac;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Core.Resolving;
using Milvasoft.Helpers.MultiTenancy.EntityBase;
using Milvasoft.Helpers.MultiTenancy.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.MultiTenancy.LifetimeManagement
{
    /// <summary>
    ///  Creates, wires dependencies and manages lifetime for a set of components. 
    /// </summary>
    /// <typeparam name="TTenant"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class MultiTenantContainer<TTenant, TKey> : IContainer
        where TTenant : class, IMilvaTenantBase<TKey>
        where TKey : struct, IEquatable<TKey>
    {
        #region Fields

        //This is the base application container
        private readonly IContainer _applicationContainer;
        //This action configures a container builder
        private readonly Action<TTenant, ContainerBuilder> _tenantContainerConfiguration;

        //This dictionary keeps track of all of the tenant scopes that we have created
        private readonly Dictionary<string, ILifetimeScope> _tenantLifetimeScopes = new();

        private readonly object _lock = new object();

        private const string _multiTenantTag = "multitenantcontainer";

        #endregion

        #region Properties

        /// <summary>
        /// Initializes a new instance of the <see cref="LifetimeScopeBeginningEventArgs"/> class.
        /// </summary>
        public event EventHandler<LifetimeScopeBeginningEventArgs> ChildLifetimeScopeBeginning;

        /// <summary>
        /// Describes when a lifetime scope is ending.
        /// </summary>
        public event EventHandler<LifetimeScopeEndingEventArgs> CurrentScopeEnding;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveOperationBeginningEventArgs"/> class.
        /// </summary>
        public event EventHandler<ResolveOperationBeginningEventArgs> ResolveOperationBeginning;

        /// <summary>
        /// Gets the <see cref="DiagnosticListener"/> to which trace events should be written.
        /// </summary>
        public DiagnosticListener DiagnosticSource => _applicationContainer.DiagnosticSource;

        /// <summary>
        /// Gets the disposer associated with this <see cref="ILifetimeScope"/>. Component instances can be associated with it manually if required.
        /// </summary>
        public IDisposer Disposer => GetCurrentTenantScope().Disposer;

        /// <summary>
        /// Gets the tag applied to the <see cref="ILifetimeScope"/>.
        /// </summary>
        public object Tag => GetCurrentTenantScope().Tag;

        /// <summary>
        /// Gets the associated services with the components that provide them.
        /// </summary>
        public IComponentRegistry ComponentRegistry => GetCurrentTenantScope().ComponentRegistry;

        #endregion


        /// <summary>
        /// Creates new instance of <see cref="MultiTenantContainer{TTenant, TKey}"/>
        /// </summary>
        /// <param name="applicationContainer"></param>
        /// <param name="containerConfiguration"></param>
        public MultiTenantContainer(IContainer applicationContainer, Action<TTenant, ContainerBuilder> containerConfiguration)
        {
            _tenantContainerConfiguration = containerConfiguration;
            _applicationContainer = applicationContainer;
        }

        /// <summary>
        /// Get the current tenant from the application container.
        /// </summary>
        /// <returns></returns>
        private TTenant GetCurrentTenant() => _applicationContainer.Resolve<TenantService<TTenant, TKey>>().GetTenantAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Get the scope of the current tenant.
        /// </summary>
        /// <returns></returns>
        public ILifetimeScope GetCurrentTenantScope()
        {
            var currentTenant = GetCurrentTenant();
            return GetTenantScope(currentTenant?.Id.ToString());
        }

        /// <summary>
        /// Get (configure on missing).
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        public ILifetimeScope GetTenantScope(string tenantId)
        {
            //If no tenant (e.g. early on in the pipeline, we just use the application container)
            if (tenantId == null)
                return _applicationContainer;

            //If we have created a lifetime for a tenant, return
            if (_tenantLifetimeScopes.ContainsKey(tenantId))
                return _tenantLifetimeScopes[tenantId];

            lock (_lock)
            {
                if (_tenantLifetimeScopes.ContainsKey(tenantId))
                {
                    return _tenantLifetimeScopes[tenantId];
                }
                else
                {
                    //This is a new tenant, configure a new lifetimescope for it using our tenant sensitive configuration method
                    _tenantLifetimeScopes.Add(tenantId, _applicationContainer.BeginLifetimeScope(_multiTenantTag, a => _tenantContainerConfiguration(GetCurrentTenant(), a)));
                    return _tenantLifetimeScopes[tenantId];
                }
            }
        }

        /// <summary>
        /// Disposes application contaner.
        /// </summary>
        public void Dispose()
        {
            lock (_lock)
            {
                foreach (var scope in _tenantLifetimeScopes)
                    scope.Value.Dispose();
                _applicationContainer.Dispose();
            }
        }

        /// <summary>
        ///  Begin a new nested scope. Component instances created via the new scope will be disposed along with it.
        /// </summary>
        /// <returns></returns>
        public ILifetimeScope BeginLifetimeScope() => GetCurrentTenantScope().BeginLifetimeScope();

        /// <summary>
        /// Begin a new nested scope. Component instances created via the new scope will be disposed along with it.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public ILifetimeScope BeginLifetimeScope(object tag) => GetCurrentTenantScope().BeginLifetimeScope(tag);

        /// <summary>
        /// Begin a new nested scope, with additional components available to it. Component instances created via the new scope will be disposed along with it.
        /// </summary>
        /// <param name="configurationAction"></param>
        /// <returns></returns>
        public ILifetimeScope BeginLifetimeScope(Action<ContainerBuilder> configurationAction) => GetCurrentTenantScope().BeginLifetimeScope(configurationAction);

        /// <summary>
        /// Begin a new nested scope, with additional components available to it. Component instances created via the new scope will be disposed along with it.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="configurationAction"></param>
        /// <returns></returns>
        public ILifetimeScope BeginLifetimeScope(object tag, Action<ContainerBuilder> configurationAction) => GetCurrentTenantScope().BeginLifetimeScope(tag, configurationAction);

        /// <summary>
        /// Resolve an instance of the provided registration within the context.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public object ResolveComponent(ResolveRequest request) => GetCurrentTenantScope().ResolveComponent(request);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
        /// </summary>
        /// <returns></returns>
        public ValueTask DisposeAsync() => GetCurrentTenantScope().DisposeAsync();
    }
}
