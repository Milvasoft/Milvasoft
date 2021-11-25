using Milvasoft.Helpers.DataAccess.EfCore.Abstract.Entity;
using Milvasoft.Helpers.DataAccess.MongoDB.Abstract;
using Milvasoft.Helpers.DataAccess.MongoDB.Utils;
using Milvasoft.Helpers.Exceptions;
using Milvasoft.Helpers.Extensions;
using Milvasoft.Helpers.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.DataAccess.MongoDB.Concrete;

/// <summary>
/// Base repository for concrete repositories. All repositories must be have this methods.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class, IAuditable<ObjectId>
{
    /// <summary>
    /// Mongo collection instance.
    /// </summary>
    protected readonly IMongoCollection<TEntity> _collection;

    /// <summary>
    /// Mongo database instance.
    /// </summary>
    protected readonly IMongoDatabase _mongoDatabase;

    /// <summary>
    /// Mongo database instance.
    /// </summary>
    protected readonly bool _useUtcForDateTimes;

    /// <summary>
    /// Constructor of <see cref="BaseRepository{TEntity}"/>
    /// </summary>
    /// <param name="settings"></param>
    public BaseRepository(IMongoDbSettings settings)
    {
        _mongoDatabase = new MongoClient(settings.ConnectionString).GetDatabase(settings.DatabaseName);
        _collection = _mongoDatabase.GetCollection<TEntity>(typeof(TEntity).GetCollectionName());
        _useUtcForDateTimes = settings.UseUtcForDateTimes;
    }

    /// <summary>
    /// Returns <see cref="IMongoDatabase"/>.
    /// </summary>
    /// <returns></returns>
    public IMongoDatabase GetMongoDatabaseInstance() => _mongoDatabase;

    /// <summary>
    /// Returns all entities.
    /// </summary>
    /// <param name="filterExpression"></param>
    /// <param name="projectExpression"></param>
    /// <returns></returns>
    public async Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filterExpression = null, Expression<Func<TEntity, TEntity>> projectExpression = null)
    {
        var filter = filterExpression ?? Builders<TEntity>.Filter.Empty;

        var projectDefinition = Builders<TEntity>.Projection.Expression(projectExpression ?? (entity => entity));

        var findOptions = new FindOptions<TEntity> { Projection = projectDefinition };

        return await (await _collection.FindAsync(filter, findOptions).ConfigureAwait(false)).ToListAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Returns all entities.
    /// </summary>
    /// <param name="filterDefinition"></param>
    /// <param name="projectExpression"></param>
    /// <returns></returns>
    public async Task<List<TEntity>> GetAllAsync(FilterDefinition<TEntity> filterDefinition, Expression<Func<TEntity, TEntity>> projectExpression = null)
    {
        var filter = filterDefinition ?? Builders<TEntity>.Filter.Empty;

        var projectDefinition = Builders<TEntity>.Projection.Expression(projectExpression ?? (entity => entity));

        var findOptions = new FindOptions<TEntity> { Projection = projectDefinition };

        return await (await _collection.FindAsync(filter, findOptions).ConfigureAwait(false)).ToListAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the max value of the property selected with <paramref name="propertySelector"/>.
    /// </summary>
    /// <param name="filterDefinition"></param>
    /// <param name="propertySelector"></param>
    /// <param name="projectExpression"></param>
    /// <returns></returns>
    public async Task<TEntity> GetMaxAsync(FilterDefinition<TEntity> filterDefinition, Expression<Func<TEntity, object>> propertySelector, Expression<Func<TEntity, TEntity>> projectExpression = null)
    {
        var options = new FindOptions<TEntity, TEntity>
        {
            Limit = 1,
            Sort = Builders<TEntity>.Sort.Descending(propertySelector.GetPropertyName()),
            Projection = Builders<TEntity>.Projection.Expression(projectExpression ?? (entity => entity))
        };

        return (await _collection.FindAsync(filterDefinition, options)).FirstOrDefault();
    }

    /// <summary>
    /// Returns entity count.
    /// </summary>
    /// <returns></returns>
    public async Task<int> GetCountAsync(Expression<Func<TEntity, bool>> filterExpression = null)
    {
        var filter = filterExpression ?? Builders<TEntity>.Filter.Empty;

        var countFacet = AggregateFacet.Create("totalDataCount", PipelineDefinition<TEntity, AggregateCountResult>.Create(new[]
        {
                PipelineStageDefinitionBuilder.Count<TEntity>()
            }));

        var aggregateFacetResult = await _collection.Aggregate().Match(filter).Facet(countFacet).ToListAsync().ConfigureAwait(false);

        var count = aggregateFacetResult.First().Facets.First(x => x.Name == "totalDataCount").Output<AggregateCountResult>()?.FirstOrDefault()?.Count ?? 0;

        return (int)count;
    }

    /// <summary>
    /// Returns embedded document count.
    /// </summary>
    /// <typeparam name="TEmbedded"></typeparam>
    /// <param name="unwindExpression"></param>
    /// <param name="filterExpression"></param>
    /// <returns></returns>
    public async Task<int> GetEmbeddedDocumentCountAsync<TEmbedded>(Expression<Func<TEntity, object>> unwindExpression, FilterDefinition<TEmbedded> filterExpression = null)
    {
        var project = GetProjectionQuery<TEmbedded>(unwindExpression);

        return await GetTotalDataCount(unwindExpression, project, null, filterExpression).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns one entity by entity Id from database asynchronously.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="projectExpression"></param>
    /// <returns> The entity found or null. </returns>
    public async Task<TEntity> GetByIdAsync(ObjectId id, Expression<Func<TEntity, TEntity>> projectExpression = null)
    {
        var filter = Builders<TEntity>.Filter.Eq(p => p.Id, id);

        var projectDefinitions = Builders<TEntity>.Projection.Expression(projectExpression ?? (entity => entity));

        var findOptions = new FindOptions<TEntity> { Projection = projectDefinitions };

        return await (await _collection.FindAsync(filter, findOptions).ConfigureAwait(false)).SingleOrDefaultAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Returns one entity by entity Id from database asynchronously.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="projectExpression"></param>
    /// <returns> The entity found or null. </returns>
    public async Task<TEntity> GetFirstOrDefaultAsync(FilterDefinition<TEntity> filter = null, Expression<Func<TEntity, TEntity>> projectExpression = null)
    {
        var filterDefinition = filter ?? Builders<TEntity>.Filter.Empty;

        var projectDefinitions = Builders<TEntity>.Projection.Expression(projectExpression ?? (entity => entity));

        var findOptions = new FindOptions<TEntity> { Projection = projectDefinitions };

        return await (await _collection.FindAsync(filterDefinition, findOptions).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Returns one entity by entity Id from database asynchronously.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="projectExpression"></param>
    /// <returns> The entity found or null. </returns>
    public async Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TEntity>> projectExpression = null)
    {
        var filterDefinition = filter ?? Builders<TEntity>.Filter.Empty;

        var projectDefinitions = Builders<TEntity>.Projection.Expression(projectExpression ?? (entity => entity));

        var findOptions = new FindOptions<TEntity> { Projection = projectDefinitions };

        return await (await _collection.FindAsync(filterDefinition, findOptions).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the nested list in the entity according to <paramref name="entityId"/>.
    /// 
    /// <remarks>
    /// <para><b>Remarks ;</b></para>
    /// 
    /// <para> We specify the nested list with <paramref name="unwindExpression"/>. </para>
    /// <para> You can send the filter value to the Nested list. See <paramref name="filterExpressionForTEmbedded"/>. </para>
    /// <para> You can get the specific properties. See <paramref name="projectExpression"/>. </para>
    /// </remarks>
    /// </summary>
    /// <typeparam name="TEmbedded"></typeparam>
    /// <param name="entityId"></param>
    /// <param name="unwindExpression"></param>
    /// <param name="filterExpressionForTEmbedded"></param>
    /// <param name="projectExpression"></param>
    /// <returns></returns>
    public async Task<List<TEmbedded>> GetNestedArrayByEntityIdAsync<TEmbedded>(ObjectId entityId,
                                                                                Expression<Func<TEntity, object>> unwindExpression,
                                                                                Expression<Func<TEmbedded, bool>> filterExpressionForTEmbedded = null,
                                                                                List<Expression<Func<TEmbedded, object>>> projectExpression = null)
    {
        var filter = filterExpressionForTEmbedded ?? Builders<TEmbedded>.Filter.Empty;

        var projectQuery = GetProjectionQuery(unwindExpression, projectExpression);

        var dataFacet = AggregateFacet.Create("matchingDatas", PipelineDefinition<TEmbedded, TEmbedded>.Create(new[]
        {
                    PipelineStageDefinitionBuilder.Project<TEmbedded, TEmbedded>(BsonDocument.Parse("{" + projectQuery + "}")),
                    PipelineStageDefinitionBuilder.Match(filter)
            }));

        var aggregateFacetResult = await _collection.Aggregate().Match(p => p.Id == entityId).Unwind<TEntity, TEmbedded>(unwindExpression).Facet(dataFacet).ToListAsync().ConfigureAwait(false);

        return aggregateFacetResult.First().Facets.First(x => x.Name == "matchingDatas").Output<TEmbedded>().ToList();
    }

    #region Pagination

    /// <summary>
    /// 
    /// You can bring up the page number you want with the number of data count you want.
    /// 
    /// <para> You can sort when listing data. </para>
    /// <para> You can send the filter value to the Nested list. See <paramref name="filterDefinition"/>. </para>
    /// <para> You can get the specific properties. See <paramref name="projectExpression"/>. </para>
    /// 
    /// </summary>
    /// <param name="pageIndex"></param>
    /// <param name="requestedItemCount"></param>
    /// <param name="orderByProps"></param>
    /// <param name="filterDefinition"></param>
    /// <param name="projectExpression"></param>
    /// <returns></returns>
    public async Task<(List<TEntity> entities, int pageCount, int totalDataCount)> GetAsPaginatedAsync(int pageIndex,
                                                                                                       int requestedItemCount,
                                                                                                       List<OrderByProp> orderByProps,
                                                                                                       FilterDefinition<TEntity> filterDefinition = null,
                                                                                                       Expression<Func<TEntity, TEntity>> projectExpression = null)
    {
        ValidatePaginationParameters(pageIndex, requestedItemCount);

        var stages = GetSortDefinitions<TEntity>(orderByProps).ToList();

        var filter = filterDefinition ?? Builders<TEntity>.Filter.Empty;

        var projectionDefinition = Builders<TEntity>.Projection.Expression(projectExpression ?? (entity => entity));

        var countFacat = AggregateFacet.Create("count", PipelineDefinition<TEntity, AggregateCountResult>.Create(new[]
        {
                PipelineStageDefinitionBuilder.Count<TEntity>()
            }));

        AggregateFacet<TEntity, TEntity> dataFacet = null;

        stages.Add(PipelineStageDefinitionBuilder.Skip<TEntity>((pageIndex - 1) * requestedItemCount));
        stages.Add(PipelineStageDefinitionBuilder.Limit<TEntity>(requestedItemCount));
        stages.Add(PipelineStageDefinitionBuilder.Project(projectionDefinition));

        dataFacet = AggregateFacet.Create("data", PipelineDefinition<TEntity, TEntity>.Create(stages));

        var aggregateFacetResults = await _collection.Aggregate().Match(filter).Facet(countFacat, dataFacet).ToListAsync().ConfigureAwait(false);

        var count = aggregateFacetResults.First().Facets.First(x => x.Name == "count").Output<AggregateCountResult>()?.FirstOrDefault()?.Count ?? 0;

        var totalPages = (int)Math.Ceiling((double)count / requestedItemCount);

        var data = aggregateFacetResults.First().Facets.First(x => x.Name == "data").Output<TEntity>().ToList();

        return (data, totalPages, (int)count);
    }


    /// <summary>
    /// 
    /// According to the <paramref name="entityId"/>, you can bring up the nested list with the page number you want and the number of data you want.
    /// 
    /// <para> You can sort when listing data. </para>
    /// <para> You can send the filter value to the Nested list. See <paramref name="projectExpression"/>. </para>
    /// <para> You can get the specific properties. See <paramref name="projectExpression"/>. </para>
    /// <para> We specify the nested list with <paramref name="unwindExpression"/>. </para>
    /// 
    /// </summary>
    /// <typeparam name="TEmbedded"></typeparam>
    /// <param name="entityId"></param>
    /// <param name="pageIndex"></param>
    /// <param name="requestedItemCount"></param>
    /// <param name="orderByProps"></param>
    /// <param name="unwindExpression"></param>
    /// <param name="projectExpression"></param>
    /// <param name="filterDefinitionForEmbedded"></param>
    /// <returns></returns>
    public async Task<(List<TEmbedded> entities, int pageCount, int totalDataCount)> GetNestedPropertyAsPaginatedAsync<TEmbedded>(ObjectId entityId,
                                                                                                                                  int pageIndex,
                                                                                                                                  int requestedItemCount,
                                                                                                                                  List<OrderByProp> orderByProps,
                                                                                                                                  Expression<Func<TEntity, object>> unwindExpression,
                                                                                                                                  List<Expression<Func<TEmbedded, object>>> projectExpression = null,
                                                                                                                                  FilterDefinition<TEmbedded> filterDefinitionForEmbedded = null)
    {
        ValidatePaginationParameters(pageIndex, requestedItemCount);

        var projectQuery = GetProjectionQuery(unwindExpression, projectExpression);

        var (dataFacet, facetName) = GetAggregateFacetForEmbeddedPagination(pageIndex,
                                                                            requestedItemCount,
                                                                            orderByProps,
                                                                            projectQuery,
                                                                            filterDefinitionForEmbedded);

        var filterDefForTentity = Builders<TEntity>.Filter.Where(p => p.Id == entityId);

        var aggregateFacetResult = await _collection.Aggregate().Match(filterDefForTentity).Unwind<TEntity, TEmbedded>(unwindExpression).Facet(dataFacet).ToListAsync().ConfigureAwait(false);

        var count = await GetTotalDataCount(unwindExpression,
                                            projectQuery,
                                            filterDefForTentity,
                                            filterDefinitionForEmbedded).ConfigureAwait(false);

        var totalPages = (int)Math.Ceiling((double)count / requestedItemCount);

        var data = aggregateFacetResult.First().Facets.First(x => x.Name == facetName).Output<TEmbedded>().ToList();

        return (data, totalPages, count);
    }

    /// <summary>
    /// 
    /// According to the <paramref name="entityIds"/>, you can bring up the nested list with the page number you want and the number of data you want.
    /// 
    /// <para> You can send the filter value to the Nested list. See <paramref name="projectExpression"/>. </para>
    /// <para> You can get the specific properties. See <paramref name="projectExpression"/>. </para>
    /// <para> We specify the nested list with <paramref name="unwindExpression"/>. </para>
    /// 
    /// </summary>
    /// <typeparam name="TEmbedded"></typeparam>
    /// <param name="entityIds"></param>
    /// <param name="pageIndex"></param>
    /// <param name="requestedItemCount"></param>
    /// <param name="orderByProps"></param>
    /// <param name="unwindExpression"></param>
    /// <param name="projectExpression"></param>
    /// <param name="filterDefinitionForEmbedded"></param>
    /// <returns></returns>
    public async Task<(List<TEmbedded> entities, int pageCount, int totalDataCount)> GetNestedPropertyAsPaginatedAsync<TEmbedded>(List<ObjectId> entityIds,
                                                                                                                                  int pageIndex,
                                                                                                                                  int requestedItemCount,
                                                                                                                                  List<OrderByProp> orderByProps,
                                                                                                                                  Expression<Func<TEntity, object>> unwindExpression,
                                                                                                                                  List<Expression<Func<TEmbedded, object>>> projectExpression = null,
                                                                                                                                  FilterDefinition<TEmbedded> filterDefinitionForEmbedded = null)
    {
        ValidatePaginationParameters(pageIndex, requestedItemCount);

        Expression<Func<TEntity, bool>> whereExpression = !entityIds.IsNullOrEmpty() ? p => entityIds.Contains(p.Id) : (entity => false);

        var projectQuery = GetProjectionQuery(unwindExpression, projectExpression);

        var (dataFacet, facetName) = GetAggregateFacetForEmbeddedPagination(pageIndex,
                                                                            requestedItemCount,
                                                                            orderByProps,
                                                                            projectQuery,
                                                                            filterDefinitionForEmbedded);

        var aggregateFacetResult = await _collection.Aggregate().Match(whereExpression).Unwind<TEntity, TEmbedded>(unwindExpression).Facet(dataFacet).ToListAsync().ConfigureAwait(false);

        var count = await GetTotalDataCount(unwindExpression,
                                            projectQuery,
                                            whereExpression,
                                            filterDefinitionForEmbedded).ConfigureAwait(false);

        var totalPages = (int)Math.Ceiling((double)count / requestedItemCount);

        var data = aggregateFacetResult.First().Facets.First(x => x.Name == facetName).Output<TEmbedded>().ToList();

        return (data, totalPages, count);
    }

    /// <summary>
    /// 
    /// You can bring up the nested list with the page number you want and the number of data you want.
    /// 
    /// <para> You can send the filter value to the Nested list. See <paramref name="projectExpression"/>. </para>
    /// <para> You can get the specific properties. See <paramref name="projectExpression"/>. </para>
    /// <para> We specify the nested list with <paramref name="unwindExpression"/>. </para>
    /// 
    /// </summary>
    /// <typeparam name="TEmbedded"></typeparam>
    /// <param name="pageIndex"></param>
    /// <param name="requestedItemCount"></param>
    /// <param name="orderByProps"></param>
    /// <param name="unwindExpression"></param>
    /// <param name="filterDefinition"></param>
    /// <param name="projectExpression"></param>
    /// <param name="filterDefinitionForEmbedded"></param>
    /// <returns></returns>
    public async Task<(List<TEmbedded> entities, int pageCount, int totalDataCount)> GetNestedPropertyAsPaginatedAsync<TEmbedded>(int pageIndex,
                                                                                                                                  int requestedItemCount,
                                                                                                                                  List<OrderByProp> orderByProps,
                                                                                                                                  Expression<Func<TEntity, object>> unwindExpression,
                                                                                                                                  FilterDefinition<TEntity> filterDefinition = null,
                                                                                                                                  List<Expression<Func<TEmbedded, object>>> projectExpression = null,
                                                                                                                                  FilterDefinition<TEmbedded> filterDefinitionForEmbedded = null)
    {
        ValidatePaginationParameters(pageIndex, requestedItemCount);

        var projectQuery = GetProjectionQuery(unwindExpression, projectExpression);

        var (dataFacet, facetName) = GetAggregateFacetForEmbeddedPagination(pageIndex,
                                                                            requestedItemCount,
                                                                            orderByProps,
                                                                            projectQuery,
                                                                            filterDefinitionForEmbedded);

        var aggregateFacetResult = await _collection.Aggregate().Match(filterDefinition ?? Builders<TEntity>.Filter.Empty).Unwind<TEntity, TEmbedded>(unwindExpression).Facet(dataFacet).ToListAsync().ConfigureAwait(false);

        var count = await GetTotalDataCount(unwindExpression,
                                            projectQuery,
                                            filterDefinition,
                                            filterDefinitionForEmbedded).ConfigureAwait(false);

        var totalPages = (int)Math.Ceiling((double)count / requestedItemCount);

        var data = aggregateFacetResult.First().Facets.First(x => x.Name == facetName).Output<TEmbedded>().ToList();

        return (data, totalPages, count);
    }

    #endregion

    /// <summary>
    ///  Adds single entity to database asynchronously.
    /// </summary>
    /// <param name="document"></param>
    /// <returns></returns>
    public virtual async Task AddAsync(TEntity document)
    {
        var options = new InsertOneOptions { BypassDocumentValidation = false };

        await _collection.InsertOneAsync(document, options).ConfigureAwait(false);
    }

    /// <summary>
    ///  Adds multiple entities to database asynchronously.
    /// </summary>
    /// <param name="documents"></param>
    /// <returns></returns>
    public virtual async Task AddRangeAsync(IEnumerable<TEntity> documents)
    {
        var options = new InsertManyOptions { BypassDocumentValidation = false };

        await _collection.InsertManyAsync(documents, options).ConfigureAwait(false);
    }

    /// <summary>
    /// Replaces existing entities with new entities(<paramref name="document"/>).
    /// </summary>
    /// <param name="document"></param>
    /// <returns></returns>
    public virtual async Task UpdateAsync(TEntity document)
    {
        var filter = Builders<TEntity>.Filter.Eq(doc => doc.Id, document.Id);
        document.LastModificationDate = _useUtcForDateTimes ? DateTime.UtcNow : DateTime.Now;
        await _collection.FindOneAndReplaceAsync(filter, document).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates existing assets according to <paramref name="updateDefinition"/>.
    /// </summary>
    /// <param name="document"></param>
    /// <param name="updateDefinition"></param>
    /// <returns></returns>
    public virtual async Task UpdateAsync(TEntity document, UpdateDefinition<TEntity> updateDefinition)
    {
        var filter = Builders<TEntity>.Filter.Eq(doc => doc.Id, document.Id);
        document.LastModificationDate = _useUtcForDateTimes ? DateTime.UtcNow : DateTime.Now;
        await _collection.UpdateOneAsync(filter, updateDefinition).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates existing assets according to <paramref name="updateDefinition"/> matching <paramref name="filterDefinition"/>.
    /// </summary>
    /// <param name="filterDefinition"></param>
    /// <param name="updateDefinition"></param>
    /// <returns></returns>
    public virtual async Task UpdateAsync(FilterDefinition<TEntity> filterDefinition, UpdateDefinition<TEntity> updateDefinition)
    {
        await _collection.UpdateOneAsync(filterDefinition, updateDefinition).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates the data in multiple.
    /// 
    /// <para> You can only update one property in multiple. See <paramref name="fieldDefinitions"/> </para>
    /// 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="fieldDefinitions"></param>
    /// <returns></returns>
    public async Task UpdateRangeAsync(List<TEntity> entities, params Expression<Func<TEntity, object>>[] fieldDefinitions)
    {
        var listWrites = new List<WriteModel<TEntity>>();

        foreach (var entity in entities)
        {
            foreach (var fieldDef in fieldDefinitions)
            {
                var filterDef = Builders<TEntity>.Filter.Eq(p => p.Id, entity.Id);
                var UpdateDef = Builders<TEntity>.Update.Set(fieldDef, entity.GetType().GetProperty(fieldDef.GetPropertyName()).GetValue(entity));

                listWrites.Add(new UpdateOneModel<TEntity>(filterDef, UpdateDef));
            }
        }

        await _collection.BulkWriteAsync(listWrites).ConfigureAwait(false);
    }

    /// <summary>
    ///  Deletes multiple entity from database asynchronously. 
    /// </summary>
    /// <param name="filterExpression"></param>
    /// <returns></returns>
    public async Task DeleteRangeAsync(FilterDefinition<TEntity> filterExpression)
    {
        await _collection.DeleteManyAsync(filterExpression).ConfigureAwait(false);
    }

    /// <summary>
    ///  Deletes single entity from database asynchronously..
    /// </summary>
    /// <param name="filterExpression"></param>
    /// <returns></returns>
    public async Task DeleteAsync(Expression<Func<TEntity, bool>> filterExpression)
    {
        await _collection.FindOneAndDeleteAsync(filterExpression).ConfigureAwait(false);
    }

    /// <summary>
    ///  Deletes single entity from database asynchronously..
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task DeleteAsync(ObjectId id)
    {
        var filter = Builders<TEntity>.Filter.Eq(doc => doc.Id, id);
        await _collection.FindOneAndDeleteAsync(filter).ConfigureAwait(false);
    }

    /// <summary>
    ///  Deletes single entity from database asynchronously..
    /// </summary>
    /// <param name="id"></param>
    /// <returns> The deleted document if one was deleted. </returns>
    public async Task<TEntity> DeleteAndReturnDeletedAsync(ObjectId id)
    {
        var filter = Builders<TEntity>.Filter.Eq(doc => doc.Id, id);
        return await _collection.FindOneAndDeleteAsync(filter).ConfigureAwait(false);
    }

    #region Helper Methods

    /// <summary>
    /// Returns projection query in sttring.
    /// </summary>
    /// <typeparam name="TEmbedded"></typeparam>
    /// <param name="projectExpressions"></param>
    /// <param name="unwindExpression"></param>
    /// <returns></returns>
    protected string GetProjectionQuery<TEmbedded>(Expression<Func<TEntity, object>> unwindExpression,
                                                   List<Expression<Func<TEmbedded, object>>> projectExpressions = null)
    {
        List<string> mappedProps = new();

        List<string> queries = new();

        var queryProp = GetUnwindType();

        if (projectExpressions.IsNullOrEmpty())
        {
            var embeddedType = typeof(TEmbedded);

            foreach (var prop in embeddedType.GetProperties())
            {
                if (prop.Name == "Id")
                    continue;

                mappedProps.Add(prop.Name);
            }
        }
        else
        {
            foreach (var expression in projectExpressions)
            {
                var propName = expression.GetPropertyName();

                if (propName == "Id")
                    continue;

                mappedProps.Add(expression.GetPropertyName());
            }
        }

        foreach (var prop in mappedProps)
        {
            queries.Add($"{prop}:'{queryProp}.{prop}'");
        }

        queries.Add($"_id:'{queryProp}._id'");
        //queries.Add("_id:0");

        return string.Join(',', queries);

        #region Local Functions

        string GetUnwindType()
        {
            List<string> unwindNesteds = new();

            MemberExpression memberExpression = unwindExpression.Body as MemberExpression;

            do
            {
                if (memberExpression == null)
                    break;

                unwindNesteds.Add(memberExpression.Member.Name);

                memberExpression = memberExpression.Expression as MemberExpression;

            } while (true);

            unwindNesteds.Reverse();

            return string.Join('.', unwindNesteds).Insert(0, "$");
        }

        #endregion
    }

    /// <summary>
    /// Returns total data count for <typeparamref name="TEmbedded"/>.
    /// </summary>
    /// <typeparam name="TEmbedded"></typeparam>
    /// <param name="unwindExpression"></param>
    /// <param name="projectQuery"></param>
    /// <param name="filterExpression"></param>
    /// <param name="filterDefForTEmbedded"></param>
    /// <returns></returns>
    protected async Task<int> GetTotalDataCount<TEmbedded>(Expression<Func<TEntity, object>> unwindExpression,
                                                           string projectQuery,
                                                           FilterDefinition<TEntity> filterExpression = null,
                                                           FilterDefinition<TEmbedded> filterDefForTEmbedded = null)
    {

        var filter = filterDefForTEmbedded ?? Builders<TEmbedded>.Filter.Empty;

        string queryBaseName = "matchingDatas";

        var dataFacetr = AggregateFacet.Create(queryBaseName, PipelineDefinition<TEmbedded, TEmbedded>.Create(new[]
        {
                    PipelineStageDefinitionBuilder.Project<TEmbedded, TEmbedded>(BsonDocument.Parse("{" + projectQuery + "}")),
                    PipelineStageDefinitionBuilder.Match(filter)
            }));

        string countPropName = "count";

        var countQuery = await _collection.Aggregate().Match(filterExpression ?? Builders<TEntity>.Filter.Empty)
                                                      .Unwind<TEntity, TEmbedded>(unwindExpression)
                                                      .Facet(dataFacetr)
                                                      .Project(new BsonDocument(countPropName, new BsonDocument("$size", $"${queryBaseName}")))
                                                      .FirstOrDefaultAsync().ConfigureAwait(false);

        return countQuery.GetValue(countPropName).ToInt32();
    }

    /// <summary>
    /// Prepare aggregate facet for embedded pagination operations.
    /// </summary>
    /// <typeparam name="TEmbedded"></typeparam>
    /// <param name="pageIndex"></param>
    /// <param name="requestedItemCount"></param>
    /// <param name="orderByProps"></param>
    /// <param name="projectQuery"></param>
    /// <param name="filterDefForTEmbedded"></param>
    /// <returns></returns>
    protected (AggregateFacet<TEmbedded, TEmbedded>, string) GetAggregateFacetForEmbeddedPagination<TEmbedded>(int pageIndex,
                                                                                                               int requestedItemCount,
                                                                                                               List<OrderByProp> orderByProps,
                                                                                                               string projectQuery,
                                                                                                               FilterDefinition<TEmbedded> filterDefForTEmbedded = null)
    {
        List<IPipelineStageDefinition> stages = new();

        stages.Add(PipelineStageDefinitionBuilder.Project<TEmbedded, TEmbedded>(BsonDocument.Parse("{" + projectQuery + "}")));

        var filter = filterDefForTEmbedded ?? Builders<TEmbedded>.Filter.Empty;

        stages.Add(PipelineStageDefinitionBuilder.Match(filter));

        var sortDefinitions = GetSortDefinitions<TEmbedded>(orderByProps);

        if (!sortDefinitions.IsNullOrEmpty())
            foreach (var sortDef in GetSortDefinitions<TEmbedded>(orderByProps))
                stages.Add(sortDef);

        stages.Add(PipelineStageDefinitionBuilder.Skip<TEmbedded>((pageIndex - 1) * requestedItemCount));
        stages.Add(PipelineStageDefinitionBuilder.Limit<TEmbedded>(requestedItemCount));

        string facetName = "matchingDatas";

        var dataFacet = AggregateFacet.Create(facetName, PipelineDefinition<TEmbedded, TEmbedded>.Create(stages));

        return (dataFacet, facetName);
    }

    /// <summary>
    /// Gets sort definition by <paramref name="orderByProps"/>.
    /// </summary>
    /// <param name="orderByProps"></param>
    /// <returns></returns>
    protected IEnumerable<IPipelineStageDefinition> GetSortDefinitions<T>(List<OrderByProp> orderByProps)
    {
        if (!orderByProps.IsNullOrEmpty())
            foreach (var orderByProp in orderByProps)
            {
                CommonHelper.PropertyExists<T>(orderByProp.PropName);

                var sortDef = orderByProp.Ascending ? Builders<T>.Sort.Ascending(orderByProp.PropName) : Builders<T>.Sort.Descending(orderByProp.PropName);

                yield return PipelineStageDefinitionBuilder.Sort(sortDef);
            }
    }

    /// <summary>
    /// Checks the parameters is valid.
    /// </summary>
    /// <param name="requestedPageNumber"></param>
    /// <param name="countOfRequestedRecordsInPage"></param>
    protected static void ValidatePaginationParameters(int requestedPageNumber, int countOfRequestedRecordsInPage)
    {
        if (requestedPageNumber <= 0) throw new MilvaUserFriendlyException(MilvaException.WrongRequestedPageNumber);

        if (countOfRequestedRecordsInPage <= 0) throw new MilvaUserFriendlyException(MilvaException.WrongRequestedItemCount);
    }

    #endregion



}
