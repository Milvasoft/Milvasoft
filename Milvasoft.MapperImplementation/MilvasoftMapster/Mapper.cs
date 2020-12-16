using Mapster;
using Milvasoft.MapperImplementation.Implementation;
using System;
using System.Linq.Expressions;

namespace Milvasoft.MapperImplementation.MilvasoftMapster
{
    /// <summary>
    /// Class written instead of manual mapping. It is written to map DTO class object to entity class.
    /// </summary>
    public class Mapper : IMapper
    {
        /// <summary>
        /// Responsible for mapping from the source object to the object we want to map.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public TDestination Map<TSource, TDestination>(TSource source)
        {
            TypeAdapterConfig<TSource, TDestination>.NewConfig().PreserveReference(true);
            return source.Adapt<TDestination>();
        }

        /// <summary>
        /// Responsible for mapping from the source object to the object we want to map.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <param name="ignoredMembers"></param>
        /// <returns></returns>
        public TDestination Map<TSource, TDestination>(TSource source, Expression<Func<TDestination, object>> ignoredMembers)
        {
            TypeAdapterConfig<TSource, TDestination>.NewConfig()
                        .Ignore(ignoredMembers)
                        .PreserveReference(true);
            return source.Adapt<TDestination>();
        }

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
        public TDestination Map<TSource, TDestination>(TSource source,
                                                       int maxDepth,
                                                       bool ignoreNullValues = false,
                                                       params Expression<Func<TDestination, object>>[] ignoredMembers)
        {
            TypeAdapterConfig<TSource, TDestination>.NewConfig()
                      .Ignore(ignoredMembers)
                      .IgnoreNullValues(ignoreNullValues)
                      .PreserveReference(true)
                      .MaxDepth(maxDepth);
            return source.Adapt<TDestination>();
        }


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
        public TDestination Map<TSource, TDestination>(TSource source,
                                                       int maxDepth,
                                                       bool preserveReference,
                                                       bool ignoreNullValues = false,
                                                       params Expression<Func<TDestination, object>>[] ignoredMembers)
        {
            TypeAdapterConfig<TSource, TDestination>.NewConfig()
                      .Ignore(ignoredMembers)
                      .IgnoreNullValues(ignoreNullValues)
                      .PreserveReference(preserveReference)
                      .MaxDepth(maxDepth);
            return source.Adapt<TDestination>();
        }

    }
}