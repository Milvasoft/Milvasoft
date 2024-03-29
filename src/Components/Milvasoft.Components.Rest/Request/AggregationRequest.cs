﻿using Milvasoft.Components.Rest.Response;

namespace Milvasoft.Components.Rest.Request;

/// <summary>
/// Aggregation criterias.
/// </summary>
public class AggregationRequest
{
    /// <summary>
    /// Filter details
    /// </summary>
    public virtual List<AggregationCriteria> Criterias { get; set; }

    /// <summary>
    /// If <see cref="SortBy"/> property is not null or empty apply sorting with <see cref="SortBy"/> and <see cref="Type"/>.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="query"></param>
    /// <returns></returns>
    public virtual async Task<List<AggregationResult>> ApplyAggregationAsync<TEntity>(IQueryable<TEntity> query, bool runAsync = true, CancellationToken cancellationToken = default)
    {
        if (Criterias.IsNullOrEmpty())
            return null;

        var result = new List<AggregationResult>();

        foreach (var criteria in Criterias)
        {
            var res = await criteria.ApplyAggregationAsync(query, runAsync, cancellationToken);

            result.Add(res);
        }

        return result;
    }
}
