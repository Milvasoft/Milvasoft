using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.MultiTenancy.ResolutionStrategy
{
    /// <summary>
    /// Resolve the header to a tenant identifier
    /// </summary>
    public class HeaderResolutionStrategy : ITenantResolutionStrategy<string>
    {
        /// <summary>
        /// Header key.
        /// </summary>
        public string HeaderKey = "X-Tenant";

        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Creates new instance of <see cref="HostResolutionStrategy"/>
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        public HeaderResolutionStrategy(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Get the tenant identifier from header.
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetTenantIdentifierAsync()
        {
            var keyExist = await Task.FromResult(_httpContextAccessor.HttpContext.Request.Headers.ContainsKey(HeaderKey));

            return keyExist ? await Task.FromResult(_httpContextAccessor.HttpContext.Request.Headers["HeaderKey"]) : "";
        }
    }
}
