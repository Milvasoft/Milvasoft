using Microsoft.EntityFrameworkCore;
using Milvasoft.Attributes.Annotations;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Components.Rest.Request;
using Milvasoft.Core.MultiLanguage.EntityBases;
using Milvasoft.Types.Structs;
using System.Linq.Expressions;
using System.Reflection;

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
    /// <param name="preCalculatedTotalCount">An optional pre-calculated total count of the data. If provided, it will be used instead of querying the database for the count. (for postgresql performance use cases)</param>
    /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A Task representing the asynchronous operation that yields a ListResponse containing the paginated list result.
    /// </returns>
    /// <remarks>
    /// This method asynchronously retrieves a paginated list result from the provided IQueryable data source based on the specified list request parameters.
    /// It applies filtering, sorting, and pagination options to the IQueryable data before returning the paginated list result.
    /// The ListResponse object contains the retrieved data along with pagination metadata such as total data count, total page count, and current page number.
    /// If no pagination parameters are specified, the method returns the entire result set without pagination.
    /// </remarks>
    public static async Task<ListResponse<TEntity>> ToListResponseAsync<TEntity>(this IQueryable<TEntity> query, ListRequest listRequest, int? preCalculatedTotalCount = null, CancellationToken cancellationToken = default) where TEntity : class
    {
        if (query == null)
            return ListResponse<TEntity>.Success();

        listRequest ??= new ListRequest();

        query = query.WithFilteringAndSorting(listRequest);

        var aggregationResults = listRequest.Aggregation != null ? await listRequest.Aggregation.ApplyAggregationAsync(query, cancellationToken: cancellationToken) : null;

        int? totalDataCount = preCalculatedTotalCount;
        int? totalPageCount = null;

        if (listRequest.PageNumber.HasValue && listRequest.RowCount.HasValue)
        {
            if (!totalDataCount.HasValue)
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
    /// Retrieves a cursor-paginated result asynchronously from the provided IQueryable data source.
    /// Applies filtering, sorting, and cursor-based pagination. Fetches <c>RowCount + 1</c> items to detect whether a next page exists.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity in the IQueryable.</typeparam>
    /// <param name="query">The source IQueryable data.</param>
    /// <param name="cursorListRequest">The cursor list request containing pagination, filtering, and sorting options.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="CursorListResponse{TEntity}"/> containing the page data, <c>NextCursor</c>, and <c>HasNextPage</c>.</returns>
    public static async Task<CursorListResponse<TEntity>> ToCursorListResponseAsync<TEntity>(this IQueryable<TEntity> query,
                                                                                              CursorListRequest cursorListRequest,
                                                                                              CancellationToken cancellationToken = default) where TEntity : class
    {
        if (query == null)
            return CursorListResponse<TEntity>.Success();

        cursorListRequest ??= new CursorListRequest();
        cursorListRequest.Sorting ??= new SortRequest { SortBy = EntityPropertyNames.Id, Type = SortType.Desc };

        query = query.WithFiltering(cursorListRequest.Filtering).WithSorting(cursorListRequest.Sorting);

        var aggregationResults = cursorListRequest.Aggregation != null ? await cursorListRequest.Aggregation.ApplyAggregationAsync(query, cancellationToken: cancellationToken) : null;

        var totalDataCount = cursorListRequest.PreCalculatedTotalCount.HasValue ? cursorListRequest.PreCalculatedTotalCount : null;

        CursorData cursorData = null;

        if (!string.IsNullOrEmpty(cursorListRequest.Cursor))
        {
            cursorData = CursorData.Decode(cursorListRequest.Cursor);
            query = CursorExtensions.ApplyCursorCondition(query, cursorData, cursorData.IsBackward);
        }

        var isBackward = cursorData?.IsBackward ?? false;

        // For backward paging we reverse the sort so the DB returns items closest to the cursor first, then we flip the list back to the original order before returning it.
        if (isBackward)
            query = query.WithSorting(CursorExtensions.ReversedSorting(cursorListRequest.Sorting));

        string nextCursor = null;
        string prevCursor = null;
        bool hasNextPage = false;
        bool hasPreviousPage = false;
        List<TEntity> list;

        if (cursorListRequest.RowCount.HasValue)
        {
            var items = await query.Take(cursorListRequest.RowCount.Value + 1).ToListAsync(cancellationToken);
            var hasExtra = items.Count > cursorListRequest.RowCount.Value;

            if (hasExtra)
                items.RemoveAt(items.Count - 1);

            if (isBackward)
            {
                items.Reverse();
                hasPreviousPage = hasExtra;
                hasNextPage = cursorData != null; // there is a page after (we came from it)

                if (hasPreviousPage)
                    prevCursor = CursorExtensions.BuildCursor(items[0], cursorListRequest.Sorting, isBackward: true);

                if (hasNextPage)
                    nextCursor = CursorExtensions.BuildCursor(items[^1], cursorListRequest.Sorting, isBackward: false);
            }
            else
            {
                hasNextPage = hasExtra;
                hasPreviousPage = cursorData != null;

                if (hasNextPage)
                    nextCursor = CursorExtensions.BuildCursor(items[^1], cursorListRequest.Sorting, isBackward: false);

                if (hasPreviousPage)
                    prevCursor = CursorExtensions.BuildCursor(items[0], cursorListRequest.Sorting, isBackward: true);
            }

            list = items;
        }
        else
        {
            list = await query.ToListAsync(cancellationToken);

            if (isBackward)
                list.Reverse();
        }

        var result = CursorListResponse<TEntity>.Success(list);

        result.NextCursor = nextCursor;
        result.PrevCursor = prevCursor;
        result.HasNextPage = hasNextPage;
        result.HasPreviousPage = hasPreviousPage;
        result.AggregationResults = aggregationResults;
        result.TotalDataCount = totalDataCount;

        return result;
    }

    /// <summary>
    /// Retrieves a cursor-paginated result asynchronously from the provided IQueryable data source with projection.
    /// Applies filtering, sorting, cursor-based pagination, and projection. Fetches <c>RowCount + 1</c> entities to detect
    /// whether a next page exists and to extract the cursor value before projection is applied.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity in the IQueryable.</typeparam>
    /// <typeparam name="TResult">The projected result type.</typeparam>
    /// <param name="query">The source IQueryable data.</param>
    /// <param name="cursorListRequest">The cursor list request containing pagination, filtering, and sorting options.</param>
    /// <param name="projection">Projection expression applied to each entity.</param>
    /// <param name="conditionAfterProjection">Optional filter applied after projection.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="CursorListResponse{TResult}"/> containing the projected page data, <c>NextCursor</c>, and <c>HasNextPage</c>.</returns>
    public static async Task<CursorListResponse<TResult>> ToCursorListResponseAsync<TEntity, TResult>(this IQueryable<TEntity> query,
                                                                                                       CursorListRequest cursorListRequest,
                                                                                                       Expression<Func<TEntity, TResult>> projection,
                                                                                                       Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                                                                                       CancellationToken cancellationToken = default) where TEntity : class where TResult : class
    {
        if (query == null)
            return CursorListResponse<TResult>.Success();

        cursorListRequest ??= new CursorListRequest();
        cursorListRequest.Sorting ??= new SortRequest { SortBy = EntityPropertyNames.Id, Type = SortType.Desc };

        query = query.WithFiltering(cursorListRequest.Filtering).WithSorting(cursorListRequest.Sorting);

        var aggregationResults = cursorListRequest.Aggregation != null ? await cursorListRequest.Aggregation.ApplyAggregationAsync(query, cancellationToken: cancellationToken) : null;

        var totalDataCount = cursorListRequest.PreCalculatedTotalCount.HasValue ? cursorListRequest.PreCalculatedTotalCount : null;

        CursorData cursorData = null;

        if (!string.IsNullOrEmpty(cursorListRequest.Cursor))
        {
            cursorData = CursorData.Decode(cursorListRequest.Cursor);
            query = CursorExtensions.ApplyCursorCondition(query, cursorData, cursorData.IsBackward);
        }

        var isBackward = cursorData?.IsBackward ?? false;

        if (isBackward)
            query = query.WithSorting(CursorExtensions.ReversedSorting(cursorListRequest.Sorting));

        string nextCursor = null;
        string prevCursor = null;
        bool hasNextPage = false;
        bool hasPreviousPage = false;
        List<TResult> list;

        var meta = projection != null ? CursorExtensions.GetCursorPropertyMetadata(typeof(TEntity), cursorListRequest.Sorting.SortBy) : null;

        if (cursorListRequest.RowCount.HasValue)
        {
            List<CursorProjectionCarrier<TResult>> carriers;

            if (projection != null && meta != null)
            {
                var combinedExpr = CursorExtensions.BuildCombinedProjection(meta, projection);

                carriers = await query.Select(combinedExpr).Take(cursorListRequest.RowCount.Value + 1).ToListAsync(cancellationToken);

                // conditionAfterProjection cannot be SQL-translated when applied to a carrier wrapper, so evaluate it in-memory after materialization.
                if (conditionAfterProjection != null)
                {
                    var compiledCondition = conditionAfterProjection.Compile();
                    carriers = [.. carriers.Where(c => compiledCondition(c.Result))];
                }
            }
            else
            {
                // No projection: build carrier with null sort-value (will use BuildCursor fallback on entity)
                var entityItems = await query.Take(cursorListRequest.RowCount.Value + 1).ToListAsync(cancellationToken);
                carriers = [.. entityItems.Select(e => new CursorProjectionCarrier<TResult> { SortValue = null, Result = (TResult)(object)e })];
            }

            var hasExtra = carriers.Count > cursorListRequest.RowCount.Value;

            if (hasExtra)
                carriers.RemoveAt(carriers.Count - 1);

            if (isBackward)
            {
                carriers.Reverse();

                hasPreviousPage = hasExtra;
                hasNextPage = cursorData != null;

                if (hasPreviousPage)
                    prevCursor = CursorExtensions.BuildCursorFromBoxedValue(carriers[0].SortValue, cursorListRequest.Sorting, isBackward: true);

                if (hasNextPage)
                    nextCursor = CursorExtensions.BuildCursorFromBoxedValue(carriers[^1].SortValue, cursorListRequest.Sorting, isBackward: false);
            }
            else
            {
                hasNextPage = hasExtra;
                hasPreviousPage = cursorData != null;

                if (hasNextPage)
                    nextCursor = CursorExtensions.BuildCursorFromBoxedValue(carriers[^1].SortValue, cursorListRequest.Sorting, isBackward: false);

                if (hasPreviousPage)
                    prevCursor = CursorExtensions.BuildCursorFromBoxedValue(carriers[0].SortValue, cursorListRequest.Sorting, isBackward: true);
            }

            list = [.. carriers.Select(c => c.Result)];
        }
        else
        {
            var projectedQuery = projection != null ? query.Select(projection) : query.Cast<TResult>();

            list = conditionAfterProjection != null
                ? await projectedQuery.Where(conditionAfterProjection).ToListAsync(cancellationToken)
                : await projectedQuery.ToListAsync(cancellationToken);

            if (isBackward)
                list.Reverse();
        }

        var result = CursorListResponse<TResult>.Success(list);

        result.NextCursor = nextCursor;
        result.PrevCursor = prevCursor;
        result.HasNextPage = hasNextPage;
        result.HasPreviousPage = hasPreviousPage;
        result.AggregationResults = aggregationResults;
        result.TotalDataCount = totalDataCount;

        return result;
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

            query = query.Skip((listRequest.PageNumber.Value - 1) * listRequest.RowCount.Value).Take(listRequest.RowCount.Value);
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

        FindUpdatablePropertiesAndAct<TEntity, TDto>(dto, (matchingEntityProp, dtoPropertyValue) =>
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
    /// Assigns the updated properties of a DTO object to the corresponding properties of an entity object.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity class that implements the <see cref="EntityBase"/> interface.</typeparam>
    /// <typeparam name="TDto">The type of the DTO class that inherits from the <see cref="DtoBase"/> class.</typeparam>
    /// <param name="entity">The entity object to update.</param>
    /// <param name="dto">The DTO object containing the updated property values.</param>
    /// <returns>A list of PropertyInfo objects representing the updated properties.</returns>
    /// <remarks> To find out how this method finds the updated properties, please see <see cref="FindUpdatablePropertiesAndAct{TEntity, TDto}(TDto, Action{PropertyInfo, object})"/>. </remarks>
    public static List<PropertyInfo> AssignUpdatedProperties<TEntity, TDto>(this TEntity entity, TDto dto) where TEntity : IMilvaEntity where TDto : DtoBase
    {
        if (entity == null || dto == null)
            return [];

        List<PropertyInfo> updatedProps = null;

        FindUpdatablePropertiesAndAct<TEntity, TDto>(dto, (matchingEntityProp, dtoPropertyValue) =>
        {
            matchingEntityProp.SetValue(entity, dtoPropertyValue);

            updatedProps ??= [];

            updatedProps.Add(matchingEntityProp);
        });

        return updatedProps;
    }

    /// <summary>
    /// Finds the updatable properties in the provided DTO object and performs the specified action on each property.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity class that implements the IMilvaEntity interface.</typeparam>
    /// <typeparam name="TDto">The type of the DTO class that inherits from the DtoBase class.</typeparam>
    /// <param name="dto">The DTO object from which to find the updatable properties.</param>
    /// <param name="action">The action to perform on each updatable property. It takes two parameters: the PropertyInfo of the matching property in the entity class, and the value of the property in the DTO object.</param>
    /// <remarks>
    /// This method is used to update the entity object with the values of the updatable properties in the DTO object.
    /// It iterates over the updatable properties in the DTO object and finds the matching property in the entity class.
    /// If a matching property is found and the property value is an instance of <see cref="IUpdateProperty"/> and IsUpdated property is true,
    /// the specified action is performed on the matching property in the entity object.
    /// </remarks>
    public static void FindUpdatablePropertiesAndAct<TEntity, TDto>(TDto dto, Action<PropertyInfo, object> action) where TEntity : IMilvaEntity where TDto : DtoBase
    {
        if (dto == null || action == null)
            return;

        var updatableProperties = dto.GetUpdatableProperties();

        updatableProperties = updatableProperties.Where(p => p.GetCustomAttribute<UpdatableIgnoreAttribute>() == null);

        foreach (var dtoProp in updatableProperties)
        {
            var matchingEntityProp = typeof(TEntity).GetPublicPropertyIgnoreCase(dtoProp.Name);

            if (matchingEntityProp == null)
                continue;

            var dtoValue = dtoProp.GetValue(dto);

            var updateProp = (IUpdateProperty)dtoValue;

            if (updateProp?.IsUpdated ?? false)
            {
                action(matchingEntityProp, updateProp.GetValue());
            }
        }
    }

    /// <summary>
    /// Allows to all entities associated with deletions to be Included to the entity(s) to be included in the process.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="context"></param>
    /// <param name="maxDepth"></param>
    public static IQueryable<T> IncludeAll<T>(this IQueryable<T> query, DbContext context, int maxDepth = 2) where T : class
    {
        var visited = new HashSet<string>();

        return IncludeNavigationProperties(query, context, typeof(T), visited, maxDepth);
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

    private static IQueryable<T> IncludeNavigationProperties<T>(IQueryable<T> query,
                                                                DbContext context,
                                                                Type entityType,
                                                                HashSet<string> visitedPaths,
                                                                int maxDepth,
                                                                string basePath = "",
                                                                int currentDepth = 0) where T : class
    {
        // Exit if max depth is reached
        if (currentDepth >= maxDepth)
            return query;

        // Get entity metadata
        var entityTypeMetadata = context.Model.FindEntityType(entityType);

        if (entityTypeMetadata == null)
            return query;

        // Get navigation properties
        var navigationProperties = entityTypeMetadata.GetNavigations();

        foreach (var navigation in navigationProperties)
        {
            // Build full path
            var navigationPath = string.IsNullOrEmpty(basePath)
                                        ? navigation.Name
                                        : $"{basePath}.{navigation.Name}";

            // Skip if already visited (circular reference prevention)
            if (visitedPaths.Contains(navigationPath))
                continue;

            // Mark as visited
            visitedPaths.Add(navigationPath);

            // Include the navigation
            query = query.Include(navigationPath);

            // Recursively include nested navigations
            query = IncludeNavigationProperties(query,
                                                context,
                                                navigation.TargetEntityType.ClrType,
                                                visitedPaths,
                                                maxDepth,
                                                navigationPath,
                                                currentDepth + 1);
        }

        return query;
    }
}