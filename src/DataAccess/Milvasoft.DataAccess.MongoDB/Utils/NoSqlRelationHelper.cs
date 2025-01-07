using Milvasoft.DataAccess.MongoDB.Utils.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Milvasoft.DataAccess.MongoDB.Utils;

/// <summary>
/// Extension methods for relational operations. Like "include approach" in RDBMS.
/// </summary>
public static class NoSqlRelationHelper
{
    /// <summary>
    /// Db name of your project. Dont forget enter this value for proper <see cref="MongoDBRef"/> documents.
    /// </summary>
    public static string DbName { get; set; } = "db";

    /// <summary>
    /// Pulls and mapped the <paramref name="_"/> object inside the <paramref name="entity"/> object from the database.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TReferenceProperty"></typeparam>
    /// <typeparam name="TMapProperty"></typeparam>
    /// <param name="entity"> The document of collection. </param>
    /// <param name="mongoDatabase"> The mongo database. </param>
    /// <param name="_"> Reference property name for code readiness. The property that contains the MongoDbRef object. </param>
    /// <param name="toBeMappedPropertySelector"> To be mapped property. </param>
    /// <param name="filterExpression"></param>
    /// <param name="projectExpression"></param>
    /// <returns></returns>
    public static async Task MapReferenceAsync<TEntity, TReferenceProperty, TMapProperty>(this TEntity entity,
                                                                                          IMongoDatabase mongoDatabase,
                                                                                          Expression<Func<TEntity, TReferenceProperty>> _,
                                                                                          Expression<Func<TEntity, TMapProperty>> toBeMappedPropertySelector,
                                                                                          Expression<Func<TMapProperty, TMapProperty>> projectExpression = null,
                                                                                          Expression<Func<TMapProperty, bool>> filterExpression = null)
    where TEntity : IAuditable<ObjectId>
    where TReferenceProperty : MongoDBRef
    where TMapProperty : IAuditable<ObjectId>
    {
        if (entity == null)
            return;

        var mappedPropertyType = typeof(TMapProperty);

        var entityType = entity.GetType();

        foreach (var item in entityType.GetProperties().Where(item => item.PropertyType.IsAssignableFrom(typeof(TReferenceProperty))))
        {
            var referenceValue = (TReferenceProperty)item.GetValue(entity);
            if (referenceValue == null)
                break;
            var collectionName = mappedPropertyType.GetCollectionName();
            if (referenceValue.CollectionName == collectionName)
            {
                var collection = mongoDatabase.GetCollection<TMapProperty>(collectionName);

                var collectionFilter = Builders<TMapProperty>.Filter.Eq(p => p.Id, referenceValue.Id);

                var filter = filterExpression ?? Builders<TMapProperty>.Filter.Empty;

                var andOperation = Builders<TMapProperty>.Filter.And(collectionFilter, filter);

                var projectDefinition = Builders<TMapProperty>.Projection.Expression(projectExpression ?? (entity => entity));

                var findOptions = new FindOptions<TMapProperty> { Projection = projectDefinition };

                var toBeMappedValue = await (await collection.FindAsync(andOperation, findOptions)).FirstOrDefaultAsync();

                entityType.GetProperty(toBeMappedPropertySelector.GetPropertyName()).SetValue(entity, toBeMappedValue);

                break;
            }
        }
    }

    /// <summary>
    /// Pulls and mapped the <paramref name="_"/> list inside the <paramref name="entity"/> object from the database.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TReferenceProperty"></typeparam>
    /// <typeparam name="TMapProperty"></typeparam>
    /// <param name="entity"> The document of collection. </param>
    /// <param name="mongoDatabase"> The mongo database. </param>
    /// <param name="_"> Reference property name for code readiness. The property that contains the MongoDbRef object. </param>
    /// <param name="toBeMappedPropertySelector"> To be mapped property. </param>
    /// <param name="filterExpression"></param>
    /// <param name="projectExpression"></param>
    /// <returns></returns>
    public static async Task MapReferenceAsync<TEntity, TReferenceProperty, TMapProperty>(this TEntity entity,
                                                                                          IMongoDatabase mongoDatabase,
                                                                                          Expression<Func<TEntity, List<TReferenceProperty>>> _,
                                                                                          Expression<Func<TEntity, List<TMapProperty>>> toBeMappedPropertySelector,
                                                                                          Expression<Func<TMapProperty, TMapProperty>> projectExpression = null,
                                                                                          Expression<Func<TMapProperty, bool>> filterExpression = null)
    where TEntity : IAuditable<ObjectId>
    where TReferenceProperty : MongoDBRef
    where TMapProperty : IAuditable<ObjectId>
    {
        if (entity == null)
            return;

        var mappedPropertyType = typeof(TMapProperty);

        foreach (var item in entity.GetType().GetProperties().Where(item => item.PropertyType.IsAssignableFrom(typeof(List<TReferenceProperty>))))
        {
            var referenceValues = (List<TReferenceProperty>)item.GetValue(entity);
            if (referenceValues.IsNullOrEmpty())
                break;
            var collectionName = mappedPropertyType.GetCollectionName();
            if (referenceValues.FirstOrDefault()?.CollectionName == collectionName)
            {
                var collection = mongoDatabase.GetCollection<TMapProperty>(collectionName);

                List<FilterDefinition<TMapProperty>> filterDefinitions = [];

                FilterDefinition<TMapProperty> filterDefinition = filterExpression ?? Builders<TMapProperty>.Filter.Empty;

                foreach (var refValue in referenceValues)
                    filterDefinitions.Add(Builders<TMapProperty>.Filter.Eq(p => p.Id, refValue.Id));

                var orOperations = Builders<TMapProperty>.Filter.Or(filterDefinitions);

                var andOperation = Builders<TMapProperty>.Filter.And(orOperations, filterDefinition);

                var projectDefinition = Builders<TMapProperty>.Projection.Expression(projectExpression ?? (entity => entity));

                var toBeMappedValues = await collection.Find(andOperation).Project(projectDefinition).ToListAsync();

                entity.GetType().GetProperty(toBeMappedPropertySelector.GetPropertyName()).SetValue(entity, toBeMappedValues);

                break;
            }
        }
    }

    /// <summary>
    /// Pulls and mapped the <paramref name="_"/> object inside the <paramref name="entities"/> list from the database.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TReferenceProperty"></typeparam>
    /// <typeparam name="TMapProperty"></typeparam>
    /// <param name="entities"> The documents of collection. </param>
    /// <param name="mongoDatabase"> The mongo database. </param>
    /// <param name="_"> Reference property name for code readiness. The property that contains the MongoDbRef object. </param>
    /// <param name="toBeMappedPropertySelector"> To be mapped property. </param>
    /// <param name="projectExpression"></param>
    /// <param name="filterExpression"></param>
    /// <returns></returns>
    public static async Task<List<TMapProperty>> MapReferenceAsync<TEntity, TReferenceProperty, TMapProperty>(this List<TEntity> entities,
                                                                                                              IMongoDatabase mongoDatabase,
                                                                                                              Expression<Func<TEntity, TReferenceProperty>> _,
                                                                                                              Expression<Func<TEntity, TMapProperty>> toBeMappedPropertySelector,
                                                                                                              Expression<Func<TMapProperty, TMapProperty>> projectExpression = null,
                                                                                                              Expression<Func<TMapProperty, bool>> filterExpression = null)
    where TEntity : IAuditable<ObjectId>
    where TMapProperty : IAuditable<ObjectId>
    where TReferenceProperty : MongoDBRef
    {
        var toBeMappedValueReferences = FindValueReferencesToBeMapped<TEntity, TReferenceProperty, TMapProperty, TReferenceProperty>(entities, out string collectionName);

        if (toBeMappedValueReferences.IsNullOrEmpty())
            return null;

        List<FilterDefinition<TMapProperty>> filterDefinitions = [];

        foreach (var refValue in toBeMappedValueReferences.Select(p => p.Item1).ToList())
            filterDefinitions.Add(Builders<TMapProperty>.Filter.Eq(p => p.Id, refValue.Id));

        var andFilterDefinition = filterExpression ?? Builders<TMapProperty>.Filter.Empty;

        var orFilterDefinition = Builders<TMapProperty>.Filter.Or(filterDefinitions);

        var totalFilterDefinition = Builders<TMapProperty>.Filter.And(andFilterDefinition, orFilterDefinition);

        var collection = mongoDatabase.GetCollection<TMapProperty>(collectionName);

        var projectionDefinition = Builders<TMapProperty>.Projection.Expression(projectExpression ?? (entity => entity));

        var toBeMappedValues = await collection.Find(totalFilterDefinition).Project(projectionDefinition).ToListAsync();

        foreach (var tobeMappedValueReference in toBeMappedValueReferences)
        {
            var entity = entities.Find(p => p.Id == tobeMappedValueReference.Item2);

            var toBeMappedValue = toBeMappedValues.Find(p => p.Id == tobeMappedValueReference.Item1.Id.AsObjectId);

            entity.GetType().GetProperty(toBeMappedPropertySelector.GetPropertyName()).SetValue(entity, toBeMappedValue);
        }

        return toBeMappedValues;
    }

    /// <summary>
    /// Pulls and mapped the <paramref name="_"/> list inside the <paramref name="entities"/> list from the database.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TReferenceProperty"></typeparam>
    /// <typeparam name="TMapProperty"></typeparam>
    /// <param name="entities"> The documents of collection. </param>
    /// <param name="mongoDatabase"> The mongo database. </param>
    /// <param name="_"> Reference property name for code readiness. The property that contains the MongoDbRef object. </param>
    /// <param name="toBeMappedPropertySelector"> To be mapped property. </param>
    /// <param name="projectExpression"></param>
    /// <returns></returns>
    public static async Task<List<TMapProperty>> MapReferenceAsync<TEntity, TReferenceProperty, TMapProperty>(this List<TEntity> entities,
                                                                                                              IMongoDatabase mongoDatabase,
                                                                                                              Expression<Func<TEntity, List<TReferenceProperty>>> _,
                                                                                                              Expression<Func<TEntity, List<TMapProperty>>> toBeMappedPropertySelector,
                                                                                                              Expression<Func<TMapProperty, TMapProperty>> projectExpression = null)
    where TEntity : IAuditable<ObjectId>
    where TMapProperty : IAuditable<ObjectId>
    where TReferenceProperty : MongoDBRef
    {
        var toBeMappedValueReferences = FindValueReferencesToBeMapped<TEntity, TReferenceProperty, TMapProperty, List<TReferenceProperty>>(entities, out string collectionName);

        if (toBeMappedValueReferences.IsNullOrEmpty())
            return null;

        List<FilterDefinition<TMapProperty>> filterDefinitions = [];

        foreach (var refValue in toBeMappedValueReferences.SelectMany(p => p.Item1).Where(p => p != null))
            filterDefinitions.Add(Builders<TMapProperty>.Filter.Eq(i => i.Id, refValue.Id));

        var collection = mongoDatabase.GetCollection<TMapProperty>(collectionName);

        var projectDefinition = Builders<TMapProperty>.Projection.Expression(projectExpression ?? (entity => entity));

        var toBeMappedValues = await collection.Find(Builders<TMapProperty>.Filter.Or(filterDefinitions)).Project(projectDefinition).ToListAsync();

        int i = 0;

        foreach (var toBeMappedValueReference in toBeMappedValueReferences)
        {
            var entity = entities.Find(p => p.Id == toBeMappedValueReference.Item2);

            List<TMapProperty> mappedValues = [];

            if (toBeMappedValueReference.Item1 != null)
                foreach (var mongoRef in toBeMappedValueReference.Item1)
                    mappedValues.Add(toBeMappedValues.Find(i => i.Id == mongoRef.Id.AsObjectId));

            entity.GetType().GetProperty(toBeMappedPropertySelector.GetPropertyName()).SetValue(entity, mappedValues);

            i++;
        }

        return toBeMappedValues;
    }

    /// <summary>
    /// Pulls and mapped the <paramref name="embeddedPropertySelector"/> object inside the <paramref name="entities"/> list from the database.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEmbeddedProperty"></typeparam>
    /// <param name="entities"> The documents of collection. </param>
    /// <param name="embeddedPropertySelector"> Embedded document to be step into. </param>
    /// <returns></returns>
    public static List<TEmbeddedProperty> StepIntoEmbedded<TEntity, TEmbeddedProperty>(this List<TEntity> entities,
                                                                                       Expression<Func<TEntity, TEmbeddedProperty>> embeddedPropertySelector)
    {
        if (entities.IsNullOrEmpty())
            return [];

        List<TEmbeddedProperty> embeddedProperties = [];

        foreach (var entity in entities)
        {
            var embeddedProperty = (TEmbeddedProperty)entities.GetType()
                                                              .GetGenericArguments()
                                                              .FirstOrDefault()?
                                                              .GetProperty(embeddedPropertySelector.GetPropertyName())
                                                              .GetValue(entity);

            if (embeddedProperty != null)
                embeddedProperties.Add(embeddedProperty);
        }

        return embeddedProperties;
    }

    /// <summary>
    /// Pulls and mapped the <paramref name="embeddedPropertySelector"/> list inside the <paramref name="entities"/> list from the database.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEmbeddedProperty"></typeparam>
    /// <param name="entities"> The documents of collection. </param>
    /// <param name="embeddedPropertySelector"> Embedded document to be step into </param>
    /// <returns></returns>
    public static List<TEmbeddedProperty> StepIntoEmbedded<TEntity, TEmbeddedProperty>(this List<TEntity> entities,
                                                                                       Expression<Func<TEntity, List<TEmbeddedProperty>>> embeddedPropertySelector)
    {
        if (entities.IsNullOrEmpty())
            return [];

        List<List<TEmbeddedProperty>> embeddedProperties = [];

        foreach (var entity in entities)
        {
            var embeddedProperty = (List<TEmbeddedProperty>)entities.GetType()
                                                                    .GetGenericArguments()
                                                                    .FirstOrDefault()?
                                                                    .GetProperty(embeddedPropertySelector.GetPropertyName())
                                                                    .GetValue(entity);

            if (embeddedProperty != null)
                embeddedProperties.Add(embeddedProperty);
        }

        return embeddedProperties.SelectMany(p => p).ToList();
    }

    /// <summary>
    /// Creates contains query.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="filterDefinitionBuilder"></param>
    /// <param name="field"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static FilterDefinition<TEntity> Contains<TEntity>(this FilterDefinitionBuilder<TEntity> filterDefinitionBuilder, Expression<Func<TEntity, object>> field, string value)
        => filterDefinitionBuilder.Regex(field, new BsonRegularExpression(value, "i"));

    /// <summary>
    /// Returns collection name.
    /// </summary>
    /// <param name="documentType"></param>
    /// <returns></returns>
    public static string GetCollectionName(this Type documentType)
        => ((BsonCollectionAttribute)documentType.GetCustomAttributes(typeof(BsonCollectionAttribute), true).FirstOrDefault())?.CollectionName;

    /// <summary>
    /// Returns <see cref="MongoDBRef"/>
    /// </summary>
    /// <param name="objectId"></param>
    /// <returns></returns>
    public static MongoDBRef GetMongoDBRef<TEntity>(this ObjectId objectId)
       => new(DbName, typeof(TEntity).GetCollectionName(), objectId);

    /// <summary>
    /// Returns <see cref="MongoDBRef"/>
    /// </summary>
    /// <param name="objectIds"></param>
    /// <returns></returns>
    public static List<MongoDBRef> GetMongoDBRef<TEntity>(this List<ObjectId> objectIds)
    {
        if (objectIds.IsNullOrEmpty())
            return [];

        List<MongoDBRef> mongoDBRefs = [];

        foreach (var objectId in objectIds)
            mongoDBRefs.Add(new(DbName, typeof(TEntity).GetCollectionName(), objectId));

        return mongoDBRefs;
    }

    /// <summary>
    /// Converts <paramref name="value"/>'s type to <see cref="ObjectId"/>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static ObjectId ToObjectId(this int value)
    {
        var totalObjectIdLenth = ObjectId.GenerateNewId().ToString().Length;

        var valueConverted = value.ToString();

        if (totalObjectIdLenth <= valueConverted.Length)
            return new ObjectId("");

        var objectIdStringBuilder = new StringBuilder();

        for (int i = 0; i < totalObjectIdLenth - valueConverted.Length; i++)
        {
            objectIdStringBuilder.Append('0');
        }

        return new ObjectId(objectIdStringBuilder.ToString() + valueConverted);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2436:Types and methods should not have too many generic parameters", Justification = "<Pending>")]
    private static List<Tuple<TReturn, ObjectId>> FindValueReferencesToBeMapped<TEntity, TReferenceProperty, TMapProperty, TReturn>(List<TEntity> entities, out string collectionName)
         where TEntity : IAuditable<ObjectId>
         where TMapProperty : IAuditable<ObjectId>
         where TReferenceProperty : MongoDBRef
    {
        collectionName = string.Empty;
        List<Tuple<TReturn, ObjectId>> toBeMappedValueReferences = null;

        if (entities.IsNullOrEmpty())
            return toBeMappedValueReferences;

        var mapPropertyCollectionName = typeof(TMapProperty).GetCollectionName();

        foreach (var entity in entities)
        {
            var referenceValuesCollection = entity.GetType()
                                                  .GetProperties()
                                                  .Where(e => e.PropertyType.IsAssignableFrom(typeof(TReturn))
                                                              && (TReturn)e.GetValue(entity) != null)
                                                  .Select(e => (TReturn)e.GetValue(entity));

            var referenceValue = referenceValuesCollection.FirstOrDefault(rv => GetCollectionName(rv) == mapPropertyCollectionName);

            if (referenceValue != null)
            {
                collectionName = GetCollectionName(referenceValue);

                toBeMappedValueReferences ??= [];

                toBeMappedValueReferences.Add(Tuple.Create(referenceValue, entity.Id));
            }
        }

        return toBeMappedValueReferences;

        static string GetCollectionName(TReturn referenceValue)
        {
            if (referenceValue is List<TReferenceProperty> listReference)
            {
                return listReference.FirstOrDefault()?.CollectionName;
            }
            else if (referenceValue is TReferenceProperty reference)
                return reference.CollectionName;
            else
                return string.Empty;
        }
    }
}
