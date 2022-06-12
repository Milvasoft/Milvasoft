using System.Linq.Expressions;

namespace Milvasoft.Core.Abstractions;

/// <summary>
/// Responsible interface for mapping from the source object to the object we want to access.
/// </summary>
public interface IMapper
{

    /// <summary>
    /// Responsible for mapping from the source object to the object we want to map.
    /// </summary>
    /// <typeparam name="TDestination"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    TDestination Map<TDestination>(object source);

    /// <summary>
    /// Responsible for mapping from the source object to the object we want to map.
    /// </summary>
    /// <typeparam name="TDestination"></typeparam>
    /// <param name="source"></param>
    /// <param name="ignoredMembers"></param>
    /// <returns></returns>
    TDestination Map<TDestination>(object source, Expression<Func<TDestination, object>> ignoredMembers);

    /// <summary>
    /// Responsible for mapping from the source object to the object we want to map.
    /// </summary>
    /// <typeparam name="TDestination"></typeparam>
    /// <param name="source"></param>
    /// <param name="ignoredMembers"></param>
    /// <param name="maxDepth"></param>
    /// <param name="ignoreNullValues"></param>
    /// <returns></returns>
    TDestination Map<TDestination>(object source,
                                   int maxDepth,
                                   bool ignoreNullValues = false,
                                   params Expression<Func<TDestination, object>>[] ignoredMembers);

    /// <summary>
    /// Responsible for mapping from the source object to the object we want to map.
    /// </summary>
    /// <typeparam name="TDestination"></typeparam>
    /// <param name="source"></param>
    /// <param name="ignoredMembers"></param>
    /// <param name="maxDepth"></param>
    /// <param name="preserveReference"></param>
    /// <param name="ignoreNullValues"></param>
    /// <returns></returns>
    TDestination Map<TDestination>(object source,
                                   int maxDepth,
                                   bool preserveReference,
                                   bool ignoreNullValues = false,
                                   params Expression<Func<TDestination, object>>[] ignoredMembers);

    /// <summary>
    /// Responsible for mapping from the source object to the object we want to map.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDestination"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    TDestination Map<TSource, TDestination>(TSource source);

    /// <summary>
    /// Responsible for mapping from the source object to the object we want to map.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDestination"></typeparam>
    /// <param name="source"></param>
    /// <param name="ignoredMembers"></param>
    /// <returns></returns>
    TDestination Map<TSource, TDestination>(TSource source, Expression<Func<TDestination, object>> ignoredMembers);

    /// <summary>
    /// Responsible for mapping from the source object to the object we want to map.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDestination"></typeparam>
    /// <param name="source"></param>
    /// <param name="ignoredMembers"></param>
    /// <param name="maxDepth"></param>
    /// <param name="ignoreNullValues"></param>
    /// <returns></returns>
    TDestination Map<TSource, TDestination>(TSource source,
                                            int maxDepth,
                                            bool ignoreNullValues = false,
                                            params Expression<Func<TDestination, object>>[] ignoredMembers);

    /// <summary>
    /// Responsible for mapping from the source object to the object we want to map.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDestination"></typeparam>
    /// <param name="source"></param>
    /// <param name="ignoredMembers"></param>
    /// <param name="maxDepth"></param>
    /// <param name="preserveReference"></param>
    /// <param name="ignoreNullValues"></param>
    /// <returns></returns>
    TDestination Map<TSource, TDestination>(TSource source,
                                            int maxDepth,
                                            bool preserveReference,
                                            bool ignoreNullValues = false,
                                            params Expression<Func<TDestination, object>>[] ignoredMembers);
}
