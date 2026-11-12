using FluentAssertions;
using Milvasoft.Core.EntityBases.MultiTenancy;
using Milvasoft.DataAccess.MongoDB.Concrete;
using Milvasoft.DataAccess.MongoDB.Utils;
using Milvasoft.DataAccess.MongoDB.Utils.Serializers;
using Milvasoft.DataAccess.MongoDB.Utils.Settings;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Moq;
using System.Linq.Expressions;

namespace Milvasoft.UnitTests.DataAccessTests.MongoDBTests;

[Trait("MongoDB Data Access Unit Tests", "Unit tests for Milvasoft.DataAccess.MongoDB unit testable parts.")]
public class MongoDBTests
{
    #region BaseRepository

    [Fact]
    public async Task GetProjectionQuery_WithNestedUnwindExpression_ShouldReturnFieldPathWithoutHanging()
    {
        // Arrange
        var repository = new TestDocumentRepository(out _);

        // Act
        var queryTask = Task.Run(() => repository.GetProjectionQueryPublic<TestEmbeddedDocument>(d => d.Inner.Items, [e => e.Name]), TestContext.Current.CancellationToken);
        var completedTask = await Task.WhenAny(queryTask, Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));

        // Assert
        completedTask.Should().BeSameAs(queryTask);
        (await queryTask).Should().Be("_id:'$Inner.Items._id',Name:'$Inner.Items.Name'");
        TestDocumentRepository.GetUnwindPathPublic(d => d.Count).Should().Be("$Count");
    }

    [Fact]
    public async Task UpdateAsync_WithUpdateDefinition_ShouldAlsoPersistLastModificationDate()
    {
        // Arrange
        var repository = new TestDocumentRepository(out var collection);
        UpdateDefinition<TestDocument> sentUpdate = null;

        collection.Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<TestDocument>>(), It.IsAny<UpdateDefinition<TestDocument>>(), It.IsAny<UpdateOptions>(), It.IsAny<CancellationToken>()))
                  .Callback<FilterDefinition<TestDocument>, UpdateDefinition<TestDocument>, UpdateOptions, CancellationToken>((_, u, _, _) => sentUpdate = u)
                  .ReturnsAsync((UpdateResult)null);

        var document = new TestDocument { Name = "name" };

        // Act
        await repository.UpdateAsync(document, Builders<TestDocument>.Update.Set(d => d.Name, "updated"));

        // Assert
        var rendered = Render(sentUpdate)["$set"].AsBsonDocument;
        rendered["Name"].AsString.Should().Be("updated");
        rendered.Contains(nameof(TestDocument.LastModificationDate)).Should().BeTrue();
        document.LastModificationDate.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateRangeAsync_WithMultipleFields_ShouldSendOneUpdatePerDocumentWithUtcDatesAndModificationDate()
    {
        // Arrange
        var repository = new TestDocumentRepository(out var collection);
        List<WriteModel<TestDocument>> sentWrites = null;

        collection.Setup(c => c.BulkWriteAsync(It.IsAny<IEnumerable<WriteModel<TestDocument>>>(), It.IsAny<BulkWriteOptions>(), It.IsAny<CancellationToken>()))
                  .Callback<IEnumerable<WriteModel<TestDocument>>, BulkWriteOptions, CancellationToken>((w, _, _) => sentWrites = [.. w])
                  .ReturnsAsync((BulkWriteResult<TestDocument>)null);

        var localDate = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Local);
        var documents = new List<TestDocument>
        {
            new() { Name = "first", Date = localDate },
            new() { Name = "second", Date = localDate },
        };

        // Act
        await repository.UpdateRangeAsync(documents, d => d.Name, d => d.Date);
        await repository.UpdateRangeAsync([]);

        // Assert
        sentWrites.Should().HaveCount(2);
        var set = Render(((UpdateOneModel<TestDocument>)sentWrites[0]).Update)["$set"].AsBsonDocument;
        set["Name"].AsString.Should().Be("first");
        set["Date"].ToUniversalTime().Should().Be(localDate.ToUniversalTime());
        set.Contains(nameof(TestDocument.LastModificationDate)).Should().BeTrue();
        collection.Verify(c => c.BulkWriteAsync(It.IsAny<IEnumerable<WriteModel<TestDocument>>>(), It.IsAny<BulkWriteOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Serializers

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TenantIdSerializers_WithNonStringValue_ShouldConsumeValueAndReturnEmpty(bool nullable)
    {
        // Arrange
        var document = new BsonDocument { { "tenant", 5 }, { "next", "value" } };
        using var reader = new BsonDocumentReader(document);
        reader.ReadStartDocument();
        reader.ReadName();
        var context = BsonDeserializationContext.CreateRoot(reader);

        // Act
        object result = nullable
            ? new NullableTenantIdSerializer().Deserialize(context, new BsonDeserializationArgs())
            : new TenantIdSerializer().Deserialize(context, new BsonDeserializationArgs());

        // Assert
        if (nullable)
            result.Should().BeNull();
        else
            result.Should().BeOfType<TenantId>().Which.ToString().Should().Be(TenantId.Empty.ToString());

        reader.ReadName().Should().Be("next");
        reader.ReadString().Should().Be("value");
    }

    [Fact]
    public void NullableTenantIdSerializer_WithNullAndValidString_ShouldDeserializeCorrectly()
    {
        // Arrange
        var tenantId = new TenantId("milvasoft", 1);
        var document = new BsonDocument { { "empty", BsonNull.Value }, { "tenant", tenantId.ToString() } };
        using var reader = new BsonDocumentReader(document);
        reader.ReadStartDocument();
        var serializer = new NullableTenantIdSerializer();

        // Act
        reader.ReadName();
        var empty = serializer.Deserialize(BsonDeserializationContext.CreateRoot(reader), new BsonDeserializationArgs());
        reader.ReadName();
        var tenant = serializer.Deserialize(BsonDeserializationContext.CreateRoot(reader), new BsonDeserializationArgs());

        // Assert
        empty.Should().BeNull();
        tenant.Should().Be(tenantId);
    }

    #endregion

    #region NoSqlRelationHelper

    [Fact]
    public void Contains_WithRegexSpecialCharacters_ShouldMatchValueLiterally()
    {
        // Act
        var filter = Builders<TestDocument>.Filter.Contains(d => d.Name, "a.b(*");

        // Assert
        var rendered = filter.Render(new RenderArgs<TestDocument>(BsonSerializer.SerializerRegistry.GetSerializer<TestDocument>(), BsonSerializer.SerializerRegistry));
        rendered["Name"].AsBsonRegularExpression.Pattern.Should().Be(@"a\.b\(\*");
    }

    #endregion

    private static BsonDocument Render(UpdateDefinition<TestDocument> update)
        => update.Render(new RenderArgs<TestDocument>(BsonSerializer.SerializerRegistry.GetSerializer<TestDocument>(), BsonSerializer.SerializerRegistry)).AsBsonDocument;

    public class TestDocument : Milvasoft.Core.EntityBases.Concrete.Auditing.AuditableEntity<ObjectId>
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public TestInnerDocument Inner { get; set; }
    }

    public class TestInnerDocument
    {
        public List<TestEmbeddedDocument> Items { get; set; }
    }

    public class TestEmbeddedDocument
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
    }

    private sealed class TestDocumentRepository : BaseRepository<TestDocument>
    {
        public TestDocumentRepository(out Mock<IMongoCollection<TestDocument>> collection) : this(CreateClient(out collection))
        {
        }

        private TestDocumentRepository(IMongoClient client) : base(new MongoDbSettings { DatabaseName = "test" }, client)
        {
        }

        public string GetProjectionQueryPublic<TEmbedded>(Expression<Func<TestDocument, object>> unwindExpression, List<Expression<Func<TEmbedded, object>>> projectExpressions)
            => GetProjectionQuery(unwindExpression, projectExpressions);

        public static string GetUnwindPathPublic(Expression<Func<TestDocument, object>> unwindExpression) => GetUnwindPath(unwindExpression);

        private static IMongoClient CreateClient(out Mock<IMongoCollection<TestDocument>> collection)
        {
            collection = new Mock<IMongoCollection<TestDocument>>();

            var database = new Mock<IMongoDatabase>();
            database.Setup(d => d.GetCollection<TestDocument>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>())).Returns(collection.Object);

            var client = new Mock<IMongoClient>();
            client.Setup(c => c.GetDatabase(It.IsAny<string>(), It.IsAny<MongoDatabaseSettings>())).Returns(database.Object);

            return client.Object;
        }
    }
}
