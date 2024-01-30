using Microsoft.EntityFrameworkCore;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.Request;
using Milvasoft.Components.Rest.Response;
using Milvasoft.Core;

namespace Milvasoft.DataAccess.EfCore.Utils;

/// <summary>
/// Entity framework related extensions.
/// </summary>
public static class QueryExtensions
{
    /// <summary>
    /// Applies filtering and sorting options to the IQueryable data source.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity in the IQueryable.</typeparam>
    /// <param name="query">The source IQueryable data.</param>
    /// <param name="filterRequest">The filter request containing filter and sort options.</param>
    /// <returns>An IQueryable object with applied filters and sorting.</returns>
    /// <remarks>
    /// This method applies filters and sorting options to the provided IQueryable data source based on the given FilterRequest object.
    /// If the FilterRequest is null, the original IQueryable data is returned without any modifications.
    /// </remarks>
    public static IQueryable<TEntity> WithFilter<TEntity>(this IQueryable<TEntity> query, FilterRequest filterRequest) where TEntity : class
    {
        IQueryable<TEntity> result = query;

        if (filterRequest == null)
            return result;

        var expression = filterRequest.BuildFilterExpression<TEntity>();

        result = result.Where(expression);

        if (!string.IsNullOrEmpty(filterRequest.Sort))
        {
            result = filterRequest.SortType switch
            {
                SortType.Asc => result.OrderBy(CommonHelper.CreateOrderByKeySelector<TEntity>(filterRequest.Sort)),
                SortType.Desc => result.OrderByDescending(CommonHelper.CreateOrderByKeySelector<TEntity>(filterRequest.Sort)),
                _ => result,
            };
        }

        return result;
    }

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
        IQueryable<TEntity> result = query;

        listRequest ??= new ListRequest();

        var expression = listRequest.BuildFilterExpression<TEntity>();

        result = result.Where(expression);

        int? totalDataCount = null;
        int? totalPageCount = null;

        result = listRequest.ApplySort(result);

        if (listRequest.PageNumber.HasValue && listRequest.RowCount.HasValue)
        {
            totalDataCount = result.Count();
            totalPageCount = listRequest.CalculatePageCountAndCompareWithRequested(totalDataCount);

            result = result.Skip((listRequest.PageNumber.Value - 1) * listRequest.RowCount.Value)
                           .Take(listRequest.RowCount.Value);
        }

        var list = await result.ToListAsync();

        var listResult = new ListResponse<TEntity>(true, "Ok", list, listRequest.PageNumber, totalPageCount, totalDataCount);

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
        IQueryable<TEntity> result = query;

        listRequest ??= new ListRequest();

        var expression = listRequest.BuildFilterExpression<TEntity>();

        result = result.Where(expression);

        int? totalDataCount = null;
        int? totalPageCount = null;

        result = listRequest.ApplySort(result);

        if (listRequest.PageNumber.HasValue && listRequest.RowCount.HasValue)
        {
            totalDataCount = result.Count();
            totalPageCount = listRequest.CalculatePageCountAndCompareWithRequested(totalDataCount);

            result = result.Skip((listRequest.PageNumber.Value - 1) * listRequest.RowCount.Value)
                           .Take(listRequest.RowCount.Value);
        }

        var list = result.ToList();

        var listResult = new ListResponse<TEntity>(true, "Ok", list, listRequest.PageNumber, totalPageCount, totalDataCount);

        return listResult;
    }
}
