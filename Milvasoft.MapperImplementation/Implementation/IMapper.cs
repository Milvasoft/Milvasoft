using System;
using System.Linq.Expressions;

namespace Milvasoft.MapperImplementation.Implementation
{
    /// <summary>
    /// <para><b>EN: </b>Responsible interface for mapping from the source object to the object we want to access</para>
    /// <para><b>TR: </b>Kaynak nesneden ulaşmak istediğimiz nesneye mapleme işlemini yapmakta sorumlu inteface</para>
    /// </summary>
    public interface IMapper
    {
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
}
