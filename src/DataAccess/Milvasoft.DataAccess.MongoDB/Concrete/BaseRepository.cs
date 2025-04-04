﻿using Milvasoft.Core.Utils.Models;
using Milvasoft.DataAccess.MongoDB.Abstract;
using Milvasoft.DataAccess.MongoDB.Utils;
using Milvasoft.DataAccess.MongoDB.Utils.Settings;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Milvasoft.DataAccess.MongoDB.Concrete;

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
    /// Constructor of <see cref="BaseRepository{TEntity}"/>. Initializes new IMongoDatabase object from registered IMongoClient. Register IMongoClient object as singleton to your IoC to use this consructor.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="mongoClient"></param>
    public BaseRepository(IMongoDbSettings settings, IMongoClient mongoClient)
    {
        _mongoDatabase = mongoClient.GetDatabase(settings.DatabaseName);
        _collection = _mongoDatabase.GetCollection<TEntity>(typeof(TEntity).GetCollectionName());
        _useUtcForDateTimes = settings.UseUtcForDateTimes;
    }

    /// <summary>
    /// Returns <see cref="IMongoDatabase"/>.
    /// </summary>
    /// <returns></returns>
    public IMongoDatabase GetMongoDatabaseInstance() => _mongoDatabase;

    #region Get Data

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

        return await (await _collection.FindAsync(filter, findOptions)).ToListAsync();
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

        return await (await _collection.FindAsync(filter, findOptions)).ToListAsync();
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
    public async Task<long> GetCountAsync(Expression<Func<TEntity, bool>> filterExpression = null)
    {
        var filter = filterExpression ?? Builders<TEntity>.Filter.Empty;

        var countFacet = AggregateFacet.Create("totalDataCount", PipelineDefinition<TEntity, AggregateCountResult>.Create(new[]
        {
                PipelineStageDefinitionBuilder.Count<TEntity>()
            }));

        var aggregateFacetResult = await _collection.Aggregate().Match(filter).Facet(countFacet).ToListAsync();

#pragma warning disable CA1826 // Do not use Enumerable methods on indexable collections
        var count = aggregateFacetResult[0].Facets.First(x => x.Name == "totalDataCount").Output<AggregateCountResult>()?.FirstOrDefault()?.Count ?? 0;
#pragma warning restore CA1826 // Do not use Enumerable methods on indexable collections

        return count;
    }

    /// <summary>
    /// Returns embedded document count.
    /// </summary>
    /// <typeparam name="TEmbedded"></typeparam>
    /// <param name="unwindExpression"></param>
    /// <param name="filterDefinition"></param>
    /// <returns></returns>
    public Task<long> GetEmbeddedDocumentCountAsync<TEmbedded>(Expression<Func<TEntity, object>> unwindExpression, FilterDefinition<TEmbedded> filterDefinition = null)
    {
        var project = GetProjectionQuery<TEmbedded>(unwindExpression);

        return GetTotalDataCount(unwindExpression, project, null, filterDefinition);
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

        return await (await _collection.FindAsync(filter, findOptions)).SingleOrDefaultAsync();
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

        return await (await _collection.FindAsync(filterDefinition, findOptions)).FirstOrDefaultAsync();
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

        return await (await _collection.FindAsync(filterDefinition, findOptions)).FirstOrDefaultAsync();
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

        var aggregateFacetResult = await _collection.Aggregate().Match(p => p.Id == entityId).Unwind<TEntity, TEmbedded>(unwindExpression).Facet(dataFacet).ToListAsync();

        return [.. aggregateFacetResult[0].Facets.First(x => x.Name == "matchingDatas").Output<TEmbedded>()];
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
    public async Task<(List<TEntity> entities, int pageCount, long totalDataCount)> GetAsPaginatedAsync(int pageIndex,
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

        var aggregateFacetResults = await _collection.Aggregate().Match(filter).Facet(countFacat, dataFacet).ToListAsync();

#pragma warning disable CA1826 // Do not use Enumerable methods on indexable collections
        var count = aggregateFacetResults[0].Facets.First(x => x.Name == "count").Output<AggregateCountResult>()?.FirstOrDefault()?.Count ?? 0;
#pragma warning restore CA1826 // Do not use Enumerable methods on indexable collections

        var totalPages = (int)Math.Ceiling((double)count / requestedItemCount);

        var data = aggregateFacetResults[0].Facets.First(x => x.Name == "data").Output<TEntity>().ToList();

        return (data, totalPages, count);
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
    public async Task<(List<TEmbedded> entities, int pageCount, long totalDataCount)> GetNestedPropertyAsPaginatedAsync<TEmbedded>(ObjectId entityId,
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

        var aggregateFacetResult = await _collection.Aggregate().Match(filterDefForTentity).Unwind<TEntity, TEmbedded>(unwindExpression).Facet(dataFacet).ToListAsync();

        var count = await GetTotalDataCount(unwindExpression,
                                            projectQuery,
                                            filterDefForTentity,
                                            filterDefinitionForEmbedded);

        var totalPages = (int)Math.Ceiling((double)count / requestedItemCount);

        var data = aggregateFacetResult[0].Facets.First(x => x.Name == facetName).Output<TEmbedded>().ToList();

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
    public async Task<(List<TEmbedded> entities, int pageCount, long totalDataCount)> GetNestedPropertyAsPaginatedAsync<TEmbedded>(List<ObjectId> entityIds,
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

        var aggregateFacetResult = await _collection.Aggregate().Match(whereExpression).Unwind<TEntity, TEmbedded>(unwindExpression).Facet(dataFacet).ToListAsync();

        var count = await GetTotalDataCount(unwindExpression,
                                            projectQuery,
                                            whereExpression,
                                            filterDefinitionForEmbedded);

        var totalPages = (int)Math.Ceiling((double)count / requestedItemCount);

        var data = aggregateFacetResult[0].Facets.First(x => x.Name == facetName).Output<TEmbedded>().ToList();

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
    public async Task<(List<TEmbedded> entities, int pageCount, long totalDataCount)> GetNestedPropertyAsPaginatedAsync<TEmbedded>(int pageIndex,
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

        var aggregateFacetResult = await _collection.Aggregate().Match(filterDefinition ?? Builders<TEntity>.Filter.Empty).Unwind<TEntity, TEmbedded>(unwindExpression).Facet(dataFacet).ToListAsync();

        var count = await GetTotalDataCount(unwindExpression,
                                            projectQuery,
                                            filterDefinition,
                                            filterDefinitionForEmbedded);

        var totalPages = (int)Math.Ceiling((double)count / requestedItemCount);

        var data = aggregateFacetResult[0].Facets.First(x => x.Name == facetName).Output<TEmbedded>().ToList();

        return (data, totalPages, count);
    }

    #endregion

    #endregion

    /// <summary>
    ///  Adds single entity to database asynchronously.
    /// </summary>
    /// <param name="document"></param>
    /// <returns></returns>
    public virtual Task AddAsync(TEntity document)
    {
        var options = new InsertOneOptions { BypassDocumentValidation = false };

        if (_useUtcForDateTimes)
            ConvertDateTimePropertiesToUtc(document);

        return _collection.InsertOneAsync(document, options);
    }

    /// <summary>
    ///  Adds multiple entities to database asynchronously.
    /// </summary>
    /// <param name="documents"></param>
    /// <returns></returns>
    public virtual Task AddRangeAsync(IEnumerable<TEntity> documents)
    {
        var options = new InsertManyOptions { BypassDocumentValidation = false };

        if (_useUtcForDateTimes)
            ConvertDateTimePropertiesToUtc(documents);

        return _collection.InsertManyAsync(documents, options);
    }

    /// <summary>
    /// Replaces existing entities with new entities(<paramref name="document"/>).
    /// </summary>
    /// <param name="document"></param>
    /// <returns></returns>
    public virtual Task UpdateAsync(TEntity document)
    {
        var filter = Builders<TEntity>.Filter.Eq(doc => doc.Id, document.Id);

        document.LastModificationDate = CommonHelper.GetNow(_useUtcForDateTimes);

        if (_useUtcForDateTimes)
            ConvertDateTimePropertiesToUtc(document);

        return _collection.FindOneAndReplaceAsync(filter, document);
    }

    /// <summary>
    /// Updates existing assets according to <paramref name="updateDefinition"/>.
    /// </summary>
    /// <param name="document"></param>
    /// <param name="updateDefinition"></param>
    /// <returns></returns>
    public virtual Task UpdateAsync(TEntity document, UpdateDefinition<TEntity> updateDefinition)
    {
        var filter = Builders<TEntity>.Filter.Eq(doc => doc.Id, document.Id);

        document.LastModificationDate = CommonHelper.GetNow(_useUtcForDateTimes);

        if (_useUtcForDateTimes)
            ConvertDateTimePropertiesToUtc(document);

        return _collection.UpdateOneAsync(filter, updateDefinition);
    }

    /// <summary>
    /// Updates existing assets according to <paramref name="updateDefinition"/> matching <paramref name="filterDefinition"/>.
    /// </summary>
    /// <remarks>
    /// 
    /// UTC convert operation cannot apply even if you set <see cref="IMongoDbSettings.UseUtcForDateTimes"/> to true when using this method.
    /// 
    /// </remarks>
    /// <param name="filterDefinition"></param>
    /// <param name="updateDefinition"></param>
    /// <returns></returns>
    public virtual Task UpdateAsync(FilterDefinition<TEntity> filterDefinition, UpdateDefinition<TEntity> updateDefinition) => _collection.UpdateOneAsync(filterDefinition, updateDefinition);

    /// <summary>
    /// Updates the data in multiple.
    /// 
    /// <para> You can only update one property in multiple. </para>
    /// 
    /// </summary>
    /// <param name="documents"></param>
    /// <param name="fieldDefinitions"></param>
    /// <returns></returns>
    public Task UpdateRangeAsync(List<TEntity> documents, params Expression<Func<TEntity, object>>[] fieldDefinitions)
    {
        var listWrites = new List<WriteModel<TEntity>>();

        foreach (var entity in documents)
        {
            var filterDef = Builders<TEntity>.Filter.Eq(p => p.Id, entity.Id);

            foreach (var fieldDef in fieldDefinitions)
            {
                if (fieldDef.ReturnType == typeof(DateTime))
                {
                    var propValue = (DateTime)entity.GetType().GetProperty(fieldDef.GetPropertyName()).GetValue(entity);

                    var updateDef = Builders<TEntity>.Update.Set(fieldDef, propValue.ToUniversalTime());

                    listWrites.Add(new UpdateOneModel<TEntity>(filterDef, updateDef));
                }
                else if (fieldDef.ReturnType == typeof(DateTime?))
                {
                    var propValue = (DateTime?)entity.GetType().GetProperty(fieldDef.GetPropertyName()).GetValue(entity);

                    if (propValue.HasValue)
                    {
                        var updateDef = Builders<TEntity>.Update.Set(fieldDef, propValue.Value.ToUniversalTime());

                        listWrites.Add(new UpdateOneModel<TEntity>(filterDef, updateDef));
                    }
                }
                else
                {
                    var updateDef = Builders<TEntity>.Update.Set(fieldDef, entity.GetType().GetProperty(fieldDef.GetPropertyName()).GetValue(entity));

                    listWrites.Add(new UpdateOneModel<TEntity>(filterDef, updateDef));
                }
            }

            var updateDefForLastModificationDate = Builders<TEntity>.Update.Set(i => i.LastModificationDate, CommonHelper.GetNow(_useUtcForDateTimes));

            listWrites.Add(new UpdateOneModel<TEntity>(filterDef, updateDefForLastModificationDate));
        }

        return _collection.BulkWriteAsync(listWrites);
    }

    /// <summary>
    ///  Deletes multiple entity from database asynchronously. 
    /// </summary>
    /// <param name="filterExpression"></param>
    /// <returns></returns>
    public Task DeleteRangeAsync(FilterDefinition<TEntity> filterExpression) => _collection.DeleteManyAsync(filterExpression);

    /// <summary>
    ///  Deletes single entity from database asynchronously..
    /// </summary>
    /// <param name="filterExpression"></param>
    /// <returns></returns>
    public Task DeleteAsync(Expression<Func<TEntity, bool>> filterExpression) => _collection.FindOneAndDeleteAsync(filterExpression);

    /// <summary>
    ///  Deletes single entity from database asynchronously..
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task DeleteAsync(ObjectId id)
    {
        var filter = Builders<TEntity>.Filter.Eq(doc => doc.Id, id);
        return _collection.FindOneAndDeleteAsync(filter);
    }

    /// <summary>
    ///  Deletes single entity from database asynchronously..
    /// </summary>
    /// <param name="id"></param>
    /// <returns> The deleted document if one was deleted. </returns>
    public Task<TEntity> DeleteAndReturnDeletedAsync(ObjectId id)
    {
        var filter = Builders<TEntity>.Filter.Eq(doc => doc.Id, id);
        return _collection.FindOneAndDeleteAsync(filter);
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
        List<string> queries = [];

        var propertyNames = projectExpressions.IsNullOrEmpty() ? typeof(TEmbedded).GetProperties().Select(p => p.Name).ToList() : [.. projectExpressions.Select(e => e.GetPropertyName())];
        propertyNames.Remove(EntityPropertyNames.Id);
        var queryProp = GetUnwindType();
        queries.Add($"_id:'{queryProp}._id'");

        foreach (var propName in propertyNames)
            queries.Add(ConvertToQueryParameter(propName));

        return string.Join(',', queries);

        #region Local Functions

        string ConvertToQueryParameter(string propName) => $"{propName}:'{GetUnwindType()}.{propName}'";

        string GetUnwindType()
        {
            List<string> unwindNesteds = [];

            do
            {
                if (unwindExpression.Body is not MemberExpression memberExpression)
                    break;

                unwindNesteds.Add(memberExpression.Member.Name);

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
    protected async Task<long> GetTotalDataCount<TEmbedded>(Expression<Func<TEntity, object>> unwindExpression,
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
                                                      .FirstOrDefaultAsync();

        return countQuery.GetValue(countPropName).ToInt64();
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
        List<IPipelineStageDefinition> stages = [PipelineStageDefinitionBuilder.Project<TEmbedded, TEmbedded>(BsonDocument.Parse("{" + projectQuery + "}"))];

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
                CommonHelper.ThrowIfPropertyNotExists<T>(orderByProp.PropName);

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
        if (requestedPageNumber <= 0)
            throw new MilvaUserFriendlyException(MilvaException.WrongRequestedPageNumber);

        if (countOfRequestedRecordsInPage <= 0)
            throw new MilvaUserFriendlyException(MilvaException.WrongRequestedItemCount);
    }

    /// <summary>
    /// Convert date times to UTC Zero.
    /// </summary>
    /// <remarks>
    /// 
    /// This will applied when "useUtcForDateTimes" in constructor property is true.
    /// This will applied <see cref="DateTime"/> and nullable <see cref="DateTime"/>.
    /// 
    /// </remarks>
    protected virtual void ConvertDateTimePropertiesToUtc(TEntity document)
    {
        foreach (var prop in document.GetType().GetProperties())
        {
            if (prop.PropertyType == typeof(DateTime))
            {
                var propValue = (DateTime)prop.GetValue(document);

                prop.SetValue(document, propValue.ToUniversalTime());
            }
            else if (prop.PropertyType == typeof(DateTime?))
            {
                var propValue = (DateTime?)prop.GetValue(document);

                if (propValue.HasValue)
                    prop.SetValue(document, propValue.Value.ToUniversalTime());
            }
        }
    }

    /// <summary>
    /// Convert date times to UTC Zero.
    /// </summary>
    /// <remarks>
    /// 
    /// This will applied when "useUtcForDateTimes" in constructor property is true.
    /// This will applied <see cref="DateTime"/> and nullable <see cref="DateTime"/>.
    /// 
    /// </remarks>
    protected virtual void ConvertDateTimePropertiesToUtc(IEnumerable<TEntity> documents)
    {
        foreach (var document in documents)
            ConvertDateTimePropertiesToUtc(document);
    }

    #endregion

}
