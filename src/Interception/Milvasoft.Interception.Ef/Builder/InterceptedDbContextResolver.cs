using Microsoft.EntityFrameworkCore;
using Milvasoft.Core.Exceptions;
using Milvasoft.Interception.Decorator;

namespace Milvasoft.Interception.Ef.Builder;

/// <summary>
/// Finds the <see cref="DbContext"/> an EF interceptor works on.
/// </summary>
internal static class InterceptedDbContextResolver
{
    /// <summary>
    /// Gets the db context from the service provider or from the intercepted object (<see cref="ICanRetrieveDbContext"/>).
    /// </summary>
    /// <param name="call">Intercepted call.</param>
    /// <param name="getDbContextFromServiceProvider">Whether the context is resolved from <paramref name="serviceProvider"/>.</param>
    /// <param name="options">Options containing the db context type.</param>
    /// <param name="serviceProvider">Service provider.</param>
    /// <param name="interceptorName">Interceptor name for error messages.</param>
    /// <returns>The db context.</returns>
    /// <exception cref="MilvaDeveloperException">When the context cannot be found. The intercepted method is not run in that case.</exception>
    public static DbContext Resolve(Call call, bool getDbContextFromServiceProvider, IHaveDbContextType options, IServiceProvider serviceProvider, string interceptorName)
    {
        var dbContextType = options?.GetDbContextType()
                            ?? throw new MilvaDeveloperException($"{interceptorName} requires a DbContext type. Configure DbContextType in the {interceptorName} options.");

        DbContext context;

        if (getDbContextFromServiceProvider)
        {
            context = serviceProvider.GetService(dbContextType) as DbContext;
        }
        else
        {
            if (call.Object is not ICanRetrieveDbContext canRetrieveDbContext)
                throw new MilvaDeveloperException($"{interceptorName}: '{call.MethodImplementation.DeclaringType?.Name}' must implement {nameof(ICanRetrieveDbContext)} when the DbContext is not taken from the service provider.");

            context = canRetrieveDbContext.GetDbContext(dbContextType);
        }

        return context ?? throw new MilvaDeveloperException($"{interceptorName}: DbContext of type '{dbContextType.Name}' could not be resolved.");
    }
}
