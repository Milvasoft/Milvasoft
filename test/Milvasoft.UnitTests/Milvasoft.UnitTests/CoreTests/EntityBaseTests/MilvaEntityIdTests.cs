using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Milvasoft.Core.EntityBases.Abstract;
using Milvasoft.Core.EntityBases.Concrete;
using System.Linq.Expressions;

namespace Milvasoft.UnitTests.CoreTests.EntityBaseTests;

[Trait("Core Unit Tests", "Milvasoft.Core project unit tests.")]
public class MilvaEntityIdTests
{
    public class IntEntity : BaseEntity<int>
    {
        public string Name { get; set; }
    }

    public class StringEntity : EntityBase<string>;

    public class GuidDto : BaseDto<Guid>;

    public class LongDto : DtoBase<long>;

    public class IdTestDbContext(DbContextOptions<IdTestDbContext> options) : DbContext(options)
    {
        public DbSet<IntEntity> Entities { get; set; }
    }

    [Fact]
    public void Id_ReadThroughIMilvaEntity_ShouldReturnStronglyTypedId()
    {
        // Arrange
        var guid = Guid.NewGuid();
        IMilvaEntity intEntity = new IntEntity { Id = 5 };
        IMilvaEntity stringEntity = new StringEntity { Id = "abc" };
        IMilvaEntity guidDto = new GuidDto { Id = guid };
        IMilvaEntity longDto = new LongDto { Id = 7L };

        // Act & Assert
        intEntity.Id.Should().Be(5);
        stringEntity.Id.Should().Be("abc");
        guidDto.Id.Should().Be(guid);
        longDto.Id.Should().Be(7L);
    }

    [Fact]
    public void Id_UsedInCompiledGenericExpression_ShouldEvaluateWithoutRecursion()
    {
        // Arrange
        var entity = new IntEntity { Id = 5 };
        var predicate = CreateKeyEqualityExpression<IntEntity>(5).Compile();

        // Act
        var result = predicate(entity);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Id_EfModel_ShouldStillMapStronglyTypedIdAsPrimaryKey()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(IntEntity));
        var primaryKey = entityType.FindPrimaryKey();

        // Assert
        primaryKey.Properties.Should().ContainSingle();
        primaryKey.Properties[0].Name.Should().Be(nameof(IntEntity.Id));
        primaryKey.Properties[0].ClrType.Should().Be<int>();
        entityType.GetProperties().Count(p => p.Name == nameof(IntEntity.Id)).Should().Be(1);
    }

    [Fact]
    public async Task Id_EfQueryThroughIMilvaEntity_ShouldFindEntity()
    {
        // Arrange
        using var context = CreateContext();
        context.Entities.AddRange(new IntEntity { Id = 1, Name = "one" }, new IntEntity { Id = 2, Name = "two" });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await context.Entities.Where(CreateKeyEqualityExpression<IntEntity>(2)).ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Should().ContainSingle().Which.Name.Should().Be("two");
    }

    private static Expression<Func<TEntity, bool>> CreateKeyEqualityExpression<TEntity>(object key) where TEntity : class, IMilvaEntity
        => i => i.Id.Equals(key);

    private static IdTestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<IdTestDbContext>().UseInMemoryDatabase($"MilvaEntityIdTests_{Guid.NewGuid()}").Options;

        return new IdTestDbContext(options);
    }
}
