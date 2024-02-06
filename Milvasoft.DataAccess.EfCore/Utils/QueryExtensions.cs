﻿using Azure.Core;
using ExpressionBuilder.Common;
using Microsoft.EntityFrameworkCore;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.Request;
using Milvasoft.Components.Rest.Response;
using Milvasoft.Core;
using Milvasoft.Core.Extensions;

namespace Milvasoft.DataAccess.EfCore.Utils;

/// <summary>
/// Entity framework related extensions.
/// </summary>
public static class QueryExtensions
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
    public static IQueryable<TEntity> WithFilter<TEntity>(this IQueryable<TEntity> query, ListRequest listRequest) where TEntity : class
        => query.WithFilter(listRequest.Filter);

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
    public static IQueryable<TEntity> WithSort<TEntity>(this IQueryable<TEntity> query, ListRequest listRequest) where TEntity : class
        => query.WithSort(listRequest.Sort);

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
    public static IQueryable<TEntity> WithFilterAndSort<TEntity>(this IQueryable<TEntity> query, ListRequest listRequest) where TEntity : class
        => query.WithFilter(listRequest.Filter).WithSort(listRequest.Sort);

    /// <summary>
    /// Applies filtering options to the IQueryable data source.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity in the IQueryable.</typeparam>
    /// <param name="query">The source IQueryable data.</param>
    /// <param name="filterRequest">The list request containing filter options.</param>
    /// <returns>An IQueryable object with applied filtering.</returns>
    /// <remarks>
    /// This method applies filter options to the provided IQueryable data source based on the given <paramref name="filterRequest"/> object.
    /// If the <paramref name="filterRequest"/> is null, the original IQueryable data is returned without any modifications.
    /// </remarks>
    public static IQueryable<TEntity> WithFilter<TEntity>(this IQueryable<TEntity> query, FilterRequest filterRequest) where TEntity : class
    {
        var expression = filterRequest?.BuildFilterExpression<TEntity>();

        return expression != null ? query.Where(expression) : query;
    }

    /// <summary>
    /// Applies sorting options to the IQueryable data source.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity in the IQueryable.</typeparam>
    /// <param name="query">The source IQueryable data.</param>
    /// <param name="sortRequest">The list request containing sort options.</param>
    /// <returns>An IQueryable object with applied sorting.</returns>
    /// <remarks>
    /// This method applies sorting options to the provided IQueryable data source based on the given <paramref name="sortRequest"/> object.
    /// If the <paramref name="sortRequest"/> is null, the original IQueryable data is returned without any modifications.
    /// </remarks>
    public static IQueryable<TEntity> WithSort<TEntity>(this IQueryable<TEntity> query, SortRequest sortRequest) where TEntity : class
    {
        var propExpression = sortRequest?.BuildPropertySelectorExpression<TEntity>();

        return propExpression == null
            ? query
            : sortRequest.Type switch
            {
                SortType.Asc => query.OrderBy(propExpression),
                SortType.Desc => query.OrderByDescending(propExpression),
                _ => query,
            };
    }

    //TODO test this with entity framework

    /// <summary>
    /// Retrieves a paginated list result asynchronously from the provided IQueryable data source based on the specified list request parameters.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity in the IQueryable.</typeparam>
    /// <param name="query">The source IQueryable data.</param>
    /// <param name="listRequest">The list request containing pagination, filtering, and sorting options.</param>
    /// <returns>
    /// A Task representing the asynchronous operation that yields a ListResponse containing the paginated list result.
    /// </returns>
    /// <remarks>
    /// This method asynchronously retrieves a paginated list result from the provided IQueryable data source based on the specified list request parameters.
    /// It applies filtering, sorting, and pagination options to the IQueryable data before returning the paginated list result.
    /// The ListResponse object contains the retrieved data along with pagination metadata such as total data count, total page count, and current page number.
    /// If no pagination parameters are specified, the method returns the entire result set without pagination.
    /// </remarks>
    public static async Task<ListResponse<TEntity>> ToListResponseAsync<TEntity>(this IQueryable<TEntity> query, ListRequest listRequest) where TEntity : class
    {
        if (query == null)
            return new ListResponse<TEntity>(true, "Ok", [], listRequest.PageNumber);

        listRequest ??= new ListRequest();

        query = query.WithFilterAndSort(listRequest);

        var aggregationResults = await listRequest.Aggregation?.ApplyAggregationAsync(query);

        int? totalDataCount = null;
        int? totalPageCount = null;

        if (listRequest.PageNumber.HasValue && listRequest.RowCount.HasValue)
        {
            totalDataCount = query.Count();
            totalPageCount = listRequest.CalculatePageCountAndCompareWithRequested(totalDataCount);

            query = query.Skip((listRequest.PageNumber.Value - 1) * listRequest.RowCount.Value)
                         .Take(listRequest.RowCount.Value);
        }

        var list = await query.ToListAsync();

        var listResult = new ListResponse<TEntity>(true, "Ok", list, listRequest.PageNumber, totalPageCount, totalDataCount)
        {
            AggregationResults = aggregationResults
        };

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
            return new ListResponse<TEntity>(true, "Ok", [], listRequest.PageNumber);

        listRequest ??= new ListRequest();

        query = query.WithFilterAndSort(listRequest);

        var aggregationResults = listRequest.Aggregation?.ApplyAggregation(query);

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

        var listResult = new ListResponse<TEntity>(true, "Ok", list, listRequest.PageNumber, totalPageCount, totalDataCount)
        {
            AggregationResults = aggregationResults
        };

        return listResult;
    }
}
