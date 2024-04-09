using Milvasoft.Components.Rest.MilvaResponse;

namespace Milvasoft.Components.Rest.Request;

/// <summary>
/// Aggregation criterias.
/// </summary>
public class AggregationRequest
{
    /// <summary>
    /// Filter details.
    /// </summary>
    public virtual List<AggregationCriteria> Criterias { get; set; }

    /// <summary>
    /// Applies aggregation to the specified query.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="query">The query to apply aggregation to.</param>
    /// <param name="runAsync">Indicates whether to run the aggregation asynchronously.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of aggregation results.</returns>
    public virtual async Task<List<AggregationResult>> ApplyAggregationAsync<TEntity>(IQueryable<TEntity> query, bool runAsync = true, CancellationToken cancellationToken = default)
    {
        if (Criterias.IsNullOrEmpty())
            return null;

        var result = new List<AggregationResult>();

        foreach (var criteria in Criterias)
        {
            var res = await criteria.ApplyAggregationAsync(query, runAsync, cancellationToken).ConfigureAwait(false);

            result.Add(res);
        }

        return result;
    }
}
