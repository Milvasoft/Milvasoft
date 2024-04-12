using Microsoft.EntityFrameworkCore;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Components.Rest.Request;
using Milvasoft.Core.MultiLanguage.EntityBases;
using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;
using Milvasoft.Types.Structs;

namespace Milvasoft.DataAccess.EfCore.Utils;

/// <summary>
/// Entity framework related extensions.
/// </summary>
public static class MilvaEfExtensions
{
    /// <summary>
    /// Applies filtering options to the IQueryable data source.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity in the IQueryable.</typeparam>
    /// <param name="query">The source IQueryable data.</param>
    /// <param name="listRequest">The list request containing filter options.</param>
    /// <returns>An IQueryable object with applied filtering.</returns>
    /// <remarks>
    /// This method applies filter options to the provided IQueryable data source based on the given <paramref name="listRequest"/> object.
    /// If the <paramref name="listRequest"/> is null, the original IQueryable data is returned without any modifications.
    /// </remarks>
    public static IQueryable<TEntity> WithFiltering<TEntity>(this IQueryable<TEntity> query, ListRequest listRequest) where TEntity : class
        => query.WithFiltering(listRequest?.Filtering);

    /// <summary>
    /// Applies filtering options to the IQueryable data source.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity in the IQueryable.</typeparam>
    /// <param name="query">The source IQueryable data.</param>
    /// <param name="filteringRequest">The list request containing filter options.</param>
    /// <returns>An IQueryable object with applied filtering.</returns>
    /// <remarks>
    /// This method applies filter options to the provided IQueryable data source based on the given <paramref name="filteringRequest"/> object.
    /// If the <paramref name="filteringRequest"/> is null, the original IQueryable data is returned without any modifications.
    /// </remarks>
    public static IQueryable<TEntity> WithFiltering<TEntity>(this IQueryable<TEntity> query, FilterRequest filteringRequest) where TEntity : class
    {
        var expression = filteringRequest?.BuildFilterExpression<TEntity>();

        return expression != null ? query?.Where(expression) : query;
    }

    /// <summary>
    /// Applies sorting options to the IQueryable data source.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity in the IQueryable.</typeparam>
    /// <param name="query">The source IQueryable data.</param>
    /// <param name="listRequest">The list request containing sort options.</param>
    /// <returns>An IQueryable object with applied sorting.</returns>
    /// <remarks>
    /// This method applies sorting options to the provided IQueryable data source based on the given <paramref name="listRequest"/> object.
    /// If the <paramref name="listRequest"/> is null, the original IQueryable data is returned without any modifications.
    /// </remarks>
    public static IQueryable<TEntity> WithSorting<TEntity>(this IQueryable<TEntity> query, ListRequest listRequest) where TEntity : class
        => query.WithSorting(listRequest?.Sorting);

    /// <summary>
    /// Applies sorting options to the IQueryable data source.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity in the IQueryable.</typeparam>
    /// <param name="query">The source IQueryable data.</param>
    /// <param name="sortingRequest">The list request containing sort options.</param>
    /// <returns>An IQueryable object with applied sorting.</returns>
    /// <remarks>
    /// This method applies sorting options to the provided IQueryable data source based on the given <paramref name="sortingRequest"/> object.
    /// If the <paramref name="sortingRequest"/> is null, the original IQueryable data is returned without any modifications.
    /// </remarks>
    public static IQueryable<TEntity> WithSorting<TEntity>(this IQueryable<TEntity> query, SortRequest sortingRequest) where TEntity : class
    {
        var propExpression = sortingRequest?.BuildPropertySelectorExpression<TEntity>();

        return propExpression == null
            ? query
            : sortingRequest.Type switch
            {
                SortType.Asc => query?.OrderBy(propExpression),
                SortType.Desc => query?.OrderByDescending(propExpression),
                _ => query,
            };
    }

    /// <summary>
    /// Applies sorting options to the IQueryable data source.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity in the IQueryable.</typeparam>
    /// <param name="query">The source IQueryable data.</param>
    /// <param name="listRequest">The list request containing sort options.</param>
    /// <returns>An IQueryable object with applied sorting.</returns>
    /// <remarks>
    /// This method applies sorting options to the provided IQueryable data source based on the given <paramref name="listRequest"/> object.
    /// If the <paramref name="listRequest"/> is null, the original IQueryable data is returned without any modifications.
    /// </remarks>
    public static IQueryable<TEntity> WithFilteringAndSorting<TEntity>(this IQueryable<TEntity> query, ListRequest listRequest) where TEntity : class
        => query.WithFiltering(listRequest?.Filtering).WithSorting(listRequest?.Sorting);

    /// <summary>
    /// Retrieves a paginated list result asynchronously from the provided IQueryable data source based on the specified list request parameters.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity in the IQueryable.</typeparam>
    /// <param name="query">The source IQueryable data.</param>
    /// <param name="listRequest">The list request containing pagination, filtering, and sorting options.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// A Task representing the asynchronous operation that yields a ListResponse containing the paginated list result.
    /// </returns>
    /// <remarks>
    /// This method asynchronously retrieves a paginated list result from the provided IQueryable data source based on the specified list request parameters.
    /// It applies filtering, sorting, and pagination options to the IQueryable data before returning the paginated list result.
    /// The ListResponse object contains the retrieved data along with pagination metadata such as total data count, total page count, and current page number.
    /// If no pagination parameters are specified, the method returns the entire result set without pagination.
    /// </remarks>
    public static async Task<ListResponse<TEntity>> ToListResponseAsync<TEntity>(this IQueryable<TEntity> query, ListRequest listRequest, CancellationToken cancellationToken = default) where TEntity : class
    {
        if (query == null)
            return ListResponse<TEntity>.Success();

        listRequest ??= new ListRequest();

        query = query.WithFilteringAndSorting(listRequest);

        var aggregationResults = listRequest.Aggregation != null ? await listRequest.Aggregation.ApplyAggregationAsync(query, cancellationToken: cancellationToken) : null;

        int? totalDataCount = null;
        int? totalPageCount = null;

        if (listRequest.PageNumber.HasValue && listRequest.RowCount.HasValue)
        {
            totalDataCount = await query.CountAsync(cancellationToken);
            totalPageCount = listRequest.CalculatePageCountAndCompareWithRequested(totalDataCount);

            query = query.Skip((listRequest.PageNumber.Value - 1) * listRequest.RowCount.Value)
                         .Take(listRequest.RowCount.Value);
        }

        var list = await query.ToListAsync(cancellationToken);

        var listResult = ListResponse<TEntity>.Success(list, LocalizerKeys.Successful, listRequest.PageNumber, totalPageCount, totalDataCount);

        listResult.AggregationResults = aggregationResults;

        return listResult;
    }

    /// <summary>
    /// Retrieves a paginated list result from the provided IQueryable data source based on the specified list request parameters.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity in the IQueryable.</typeparam>
    /// <param name="query">The source IQueryable data.</param>
    /// <param name="listRequest">The list request containing pagination, filtering, and sorting options.</param>
    /// <returns>
    /// A ListResponse containing the paginated list result.
    /// </returns>
    /// <remarks>
    /// This method retrieves a paginated list result from the provided IQueryable data source based on the specified list request parameters.
    /// It applies filtering, sorting, and pagination options to the IQueryable data before returning the paginated list result.
    /// The ListResponse object contains the retrieved data along with pagination metadata such as total data count, total page count, and current page number.
    /// If no pagination parameters are specified, the method returns the entire result set without pagination.
    /// </remarks>
    public static ListResponse<TEntity> ToListResponse<TEntity>(this IQueryable<TEntity> query, ListRequest listRequest) where TEntity : class
    {
        if (query == null)
            return ListResponse<TEntity>.Success();

        listRequest ??= new ListRequest();

        query = query.WithFilteringAndSorting(listRequest);

        var aggregationResults = listRequest.Aggregation?.ApplyAggregationAsync(query, false).Result;

        int? totalDataCount = null;
        int? totalPageCount = null;

        if (listRequest.PageNumber.HasValue && listRequest.RowCount.HasValue)
        {
            totalDataCount = query.Count();
            totalPageCount = listRequest.CalculatePageCountAndCompareWithRequested(totalDataCount);

            query = query.Skip((listRequest.PageNumber.Value - 1) * listRequest.RowCount.Value)
                         .Take(listRequest.RowCount.Value);
        }

        var list = query.ToList();

        var listResult = ListResponse<TEntity>.Success(list, LocalizerKeys.Successful, listRequest.PageNumber, totalPageCount, totalDataCount);

        listResult.AggregationResults = aggregationResults;

        return listResult;
    }

    /// <summary>
    /// Gets <see cref="SetPropertyBuilder{TSource}"/> for entity's matching properties with <paramref name="dto"/>'s updatable properties.
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="dto"></param>
    /// <param name="useUtcForDateTimes">Converts <see cref="DateTime"/> typed properties to utc.</param>
    /// <remarks>
    /// This method is used to update the entity object with the values of the updatable properties in the DTO object.
    /// It iterates over the updatable properties in the DTO object and finds the matching property in the entity class.
    /// If a matching property is found and the property value is an instance of <see cref="IUpdateProperty"/> and IsUpdated property is true,
    /// the specified action is performed on the matching property in the entity object.
    /// </remarks>
    public static SetPropertyBuilder<TEntity> GetUpdatablePropertiesBuilder<TEntity, TDto>(this TDto dto, bool useUtcForDateTimes = false) where TEntity : class, IMilvaEntity where TDto : DtoBase
    {
        if (dto == null)
            return null;

        var builder = new SetPropertyBuilder<TEntity>();

        var entityType = typeof(TEntity);

        CommonHelper.FindUpdatablePropertiesAndAct<TEntity, TDto>(dto, (matchingEntityProp, dtoPropertyValue) =>
        {
            var genericMethod = SetPropertyBuilder<TEntity>.SetPropertyValueMethodInfo.MakeGenericMethod(matchingEntityProp.PropertyType);

            var expression = CommonHelper.DynamicInvokeCreatePropertySelector(nameof(CommonHelper.CreatePropertySelector),
                                                                              entityType,
                                                                              matchingEntityProp.PropertyType,
                                                                              matchingEntityProp.Name);

            var propType = dtoPropertyValue?.GetType();

            // If property is datetime and utc conversion requested in configuration, convert date prop to utc
            if (useUtcForDateTimes && dtoPropertyValue != null && propType.CanAssignableTo(typeof(DateTime)))
                dtoPropertyValue = ((DateTime)dtoPropertyValue).ToUniversalTime();

            builder = (SetPropertyBuilder<TEntity>)genericMethod.Invoke(builder, [expression, dtoPropertyValue]);
        });

        return builder;
    }

    /// <summary>
    /// Allows to all entities associated with deletions to be Included to the entity(s) to be included in the process.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="context"></param>
    public static IQueryable<T> IncludeAll<T>(this IQueryable<T> source, DbContext context)
        where T : class
    {
        var navigations = context.Model.FindEntityType(typeof(T))
                                       .GetDerivedTypesInclusive()
                                       .SelectMany(type => type.GetNavigations())
                                       .Distinct();

        foreach (var property in navigations)
            source = source.Include(property.Name);

        return source;
    }

    /// <summary>
    /// Allows to all entities associated with deletions to be Included to the entity(s) to be included in the process.
    /// Entities must be contains "Translations" navigation property for include process. (e.g. Poco.Translations)
    /// </summary>
    /// <param name="source"></param>
    /// <param name="context"></param>
    public static IQueryable<TEntity> IncludeTranslations<TEntity>(this IQueryable<TEntity> source, DbContext context) where TEntity : class
    {
        var navigations = context.Model.FindEntityType(typeof(TEntity))
                                       .GetDerivedTypesInclusive()
                                       .SelectMany(type => type.GetNavigations())
                                       .Distinct();

        foreach (var property in navigations.Where(property => property.Name == MultiLanguageEntityPropertyNames.Translations))
            source = source.Include(property.Name);

        return source;
    }

    /// <summary>
    /// Allows to all entities associated with deletions to be Included to the entity(s) to be included in the process.
    /// Entities must be contains "Translations" navigation property for include process. (e.g. Poco.Translations)
    /// </summary>
    /// <param name="source"></param>
    public static IQueryable<TEntity> IncludeTranslations<TEntity, TLangEntity>(this IQueryable<TEntity> source) where TEntity : class, IHasTranslation<TLangEntity> where TLangEntity : class
        => source.Include(MultiLanguageEntityPropertyNames.Translations);
}
