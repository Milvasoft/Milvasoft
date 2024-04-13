using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Helpers;
using Milvasoft.UnitTests.CoreTests.HelperTests.CollectionTests.Fixtures;
using Milvasoft.UnitTests.CoreTests.HelperTests.PropertyTests.Fixtures;
using Moq;
using System.Linq.Expressions;

namespace Milvasoft.UnitTests.CoreTests.HelperTests.CollectionTests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1042:The member referenced by the MemberData attribute returns untyped data rows", Justification = "<Pending>")]
[Trait("Core Unit Tests", "Milvasoft.Core project unit tests.")]
public partial class CollectionHelperTests
{

    #region IsNullOrEmpty

    /// <summary>
    /// source , expected result
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> IsNullOrEmptyData()
    {
        yield return new object[] { null, true };
        yield return new object[] { Array.Empty<byte>(), true };
        yield return new object[] { new List<byte> { }, true };
        yield return new object[] { new List<byte> { 1 }, false };
    }

    [Theory]
    [MemberData(nameof(IsNullOrEmptyData))]
    public void IsNullOrEmpty_WithValidEnumerableInput_ShouldReturnExpected(IEnumerable<byte> input, bool expected)
    {
        // Arrange

        // Act
        var result = input.IsNullOrEmpty();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void IsNullOrEmpty_WithEmptyDictionaryInput_ShouldReturnTrue()
    {
        // Arrange
        var input = new Dictionary<string, object>();

        // Act
        var result = input.IsNullOrEmpty();

        // Assert
        result.Should().Be(true);
    }

    #endregion

    #region OrderByProperty

    /// <summary>
    /// source , property name , expected result
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> InvalidSourceForOrderByPropertyMethodData()
    {
        var emptySource = new List<PropertyExistsTestModelFixture>().AsQueryable();
        var validSource = new List<PropertyExistsTestModelFixture>() { new() { Poco = 1 } }.AsQueryable();

        yield return new object[] { emptySource, "", emptySource };
        yield return new object[] { null, "", null };
        yield return new object[] { validSource, "NotExistsPropName", validSource };
    }

    /// <summary>
    /// source , property name , expected result
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> ValidSourceForOrderByPropertyMethodData()
    {
        var validSource = new List<PropertyExistsTestModelFixture>()
        {
            new()
            {
                Poco = 2,
            },
            new()
            {
                Poco = 1,
            }
        }.AsQueryable();

        yield return new object[] { validSource, "Poco" };
    }

    [Theory]
    [MemberData(nameof(InvalidSourceForOrderByPropertyMethodData))]
    public void OrderByProperty_WithInvalidSourceOrPropertyNameInput_ShouldReturnSource(IQueryable<PropertyExistsTestModelFixture> source, string propertyName, IQueryable<PropertyExistsTestModelFixture> expected)
    {
        // Arrange

        // Act
        var result = source.OrderByProperty(propertyName);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [MemberData(nameof(ValidSourceForOrderByPropertyMethodData))]
    public void OrderByProperty_WithValidSourceAndPropertyNameInput_ShouldReturnAscendingOrderedSource(IQueryable<PropertyExistsTestModelFixture> source, string propertyName)
    {
        // Arrange

        // Act
        var result = source.OrderByProperty(propertyName);

        // Assert
        result.Should().BeInAscendingOrder(propertyName);
    }

    #endregion

    #region OrderByPropertyDescending

    /// <summary>
    /// source , property name
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> ValidSourceForOrderByPropertyDescendingMethodData()
    {
        var validSource = new List<PropertyExistsTestModelFixture>()
        {
            new()
            {
                Poco = 1,
            },
            new()
            {
                Poco = 2,
            }
        }.AsQueryable();

        yield return new object[] { validSource, "Poco" };
    }

    [Theory]
    [MemberData(nameof(InvalidSourceForOrderByPropertyMethodData))]
    public void OrderByPropertyDescending_WithInvalidSourceOrPropertyNameInput_ShouldReturnSource(IQueryable<PropertyExistsTestModelFixture> source, string propertyName, IQueryable<PropertyExistsTestModelFixture> expected)
    {
        // Arrange

        // Act
        var result = source.OrderByPropertyDescending(propertyName);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [MemberData(nameof(ValidSourceForOrderByPropertyMethodData))]
    public void OrderByPropertyDescending_WithValidSourceAndPropertyNameInput_ShouldReturnDescendingOrderedSource(IQueryable<PropertyExistsTestModelFixture> source, string propertyName)
    {
        // Arrange

        // Act
        var result = source.OrderByPropertyDescending(propertyName);

        // Assert
        result.Should().BeInDescendingOrder(propertyName);
    }

    #endregion

    #region ToBatches

    /// <summary>
    /// source , expected result
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> InvalidSourceForToBatchesMethodData()
    {
        var emptySource = new List<byte>();

        yield return new object[] { emptySource, emptySource };
        yield return new object[] { null, emptySource };
    }

    /// <summary>
    /// source , batch size , expected batch count
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> ValidSourceForToBatchesMethodData()
    {
        var validSource = new List<byte>() { 1, 2, 3, 4, 5 };

        yield return new object[] { validSource, 100, 1 };
        yield return new object[] { validSource, 2, 3 };
        yield return new object[] { validSource, 3, 2 };
    }

    [Theory]
    [MemberData(nameof(InvalidSourceForToBatchesMethodData))]
    public void ToBatches_WithInvalidSourceInput_ShouldReturnEmptySource(List<byte> source, List<byte> expected)
    {
        // Arrange

        // Act
        var result = source.ToBatches();

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [MemberData(nameof(ValidSourceForToBatchesMethodData))]
    public void ToBatches_WithValidSourceInput_ShouldReturnValidBatches(List<byte> source, int batchSize, int expectedBatchCount)
    {
        // Arrange

        // Act
        var result = source.ToBatches(batchSize);

        // Assert
        result.Should().HaveCount(expectedBatchCount);
    }

    #endregion

    #region UpdateSingletonInstance

    /// <summary>
    /// source , expected result
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> InvalidSourceForUpdateSingletonInstanceMethodData()
    {
        IServiceCollection emptyServiceCollection = new ServiceCollection();

        yield return new object[] { emptyServiceCollection, emptyServiceCollection };
        yield return new object[] { null, null };
    }

    [Theory]
    [MemberData(nameof(InvalidSourceForUpdateSingletonInstanceMethodData))]
    public void UpdateSingletonInstance_ForOverloadWithOneGenericParameter_WithServiceCollectionIsNullOrEmpty_ShouldReturnInputServiceCollection(IServiceCollection source, IServiceCollection expected)
    {
        // Arrange

        // Act
        var result = source.UpdateSingletonInstance<UpdateSingletonInstanceTestModel>(null);

        // Assert
        result.Should().Equal(expected);
    }

    [Fact]
    public void UpdateSingletonInstance_ForOverloadWithOneGenericParameter_WithUpdateActionIsNull_ShouldReturnInputServiceCollection()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(new UpdateSingletonInstanceTestModel());

        // Act
        var result = services.UpdateSingletonInstance<UpdateSingletonInstanceTestModel>(null);

        // Assert
        result.Should().Equal(services);
    }

    [Fact]
    public void UpdateSingletonInstance_ForOverloadWithOneGenericParameter_WithInstanceNotFoundInServiceCollection_ShouldDoNothingAndReturnsUnchangedServiceCollection()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton<IUpdateSingletonInstanceTestModel, UpdateSingletonInstanceTestModel>();
        var mockValidator = new Mock<Action<UpdateSingletonInstanceTestModel>>();

        // Act
        var result = services.UpdateSingletonInstance(mockValidator.Object);

        // Assert
        mockValidator.Verify(m => m.Invoke(null), Times.Never());
        result.Should().Equal(services);
    }

    [Fact]
    public void UpdateSingletonInstance_ForOverloadWithOneGenericParameter_WithInstanceFoundInServiceCollectionButNotSingleton_ShouldDoNothingAndReturnsUnchangedServiceCollection()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        services.AddScoped<UpdateSingletonInstanceTestModel>();
        var mockValidator = new Mock<Action<UpdateSingletonInstanceTestModel>>();

        // Act
        var result = services.UpdateSingletonInstance(mockValidator.Object);

        // Assert
        mockValidator.Verify(m => m.Invoke(null), Times.Never());
        result.Should().Equal(services);
    }

    [Fact]
    public void UpdateSingletonInstance_ForOverloadWithOneGenericParameter_WithSingletonInstanceFoundInServiceCollection_ShouldUpdateInstanceAndReturnsUpdatedServiceCollection()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        var singletonInstance = new UpdateSingletonInstanceTestModel
        {
            Name = "Test",
            Order = 1,
        };

        services.AddSingleton(singletonInstance);

        static void Action(UpdateSingletonInstanceTestModel a)
        {
            a.Name = "Changed";
            a.Order = 1;
        }

        var mockValidator = new Mock<Action<UpdateSingletonInstanceTestModel>>();
        mockValidator.Setup(x => x.Invoke(singletonInstance)).Callback((Action<UpdateSingletonInstanceTestModel>)Action);

        // Act
        var result = services.UpdateSingletonInstance(mockValidator.Object);

        // Assert
        mockValidator.Verify(m => m.Invoke(singletonInstance), Times.Once());
        result.Should().BeEquivalentTo(services);
        result.Should().Contain(s => s.ImplementationInstance as UpdateSingletonInstanceTestModel == singletonInstance);
        result.Should().Contain(s => (s.ImplementationInstance as UpdateSingletonInstanceTestModel).Name == "Changed");
        result.Should().Contain(s => (s.ImplementationInstance as UpdateSingletonInstanceTestModel).Order == 1);
        result.Should().NotContain(s => (s.ImplementationInstance as UpdateSingletonInstanceTestModel).Name == "Test");
    }

    [Theory]
    [MemberData(nameof(InvalidSourceForUpdateSingletonInstanceMethodData))]
    public void UpdateSingletonInstance_ForOverloadWithTwoGenericParameter_WithServiceCollectionIsNullOrEmpty_ShouldReturnInputServiceCollection(IServiceCollection source, IServiceCollection expected)
    {
        // Arrange

        // Act
        var result = source.UpdateSingletonInstance<IUpdateSingletonInstanceTestModel, UpdateSingletonInstanceTestModel>(null);

        // Assert
        result.Should().Equal(expected);
    }

    [Fact]
    public void UpdateSingletonInstance_ForOverloadWithTwoGenericParameter_WithUpdateActionIsNull_ShouldReturnInputServiceCollection()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton<IUpdateSingletonInstanceTestModel>(new UpdateSingletonInstanceTestModel());

        // Act
        var result = services.UpdateSingletonInstance<IUpdateSingletonInstanceTestModel, UpdateSingletonInstanceTestModel>(null);

        // Assert
        result.Should().Equal(services);
    }

    [Fact]
    public void UpdateSingletonInstance_ForOverloadWithTwoGenericParameter_WithInstanceNotFoundInServiceCollection_ShouldDoNothingAndReturnsUnchangedServiceCollection()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton<UpdateSingletonInstanceTestModel>();
        var mockValidator = new Mock<Action<UpdateSingletonInstanceTestModel>>();

        // Act
        var result = services.UpdateSingletonInstance<IUpdateSingletonInstanceTestModel, UpdateSingletonInstanceTestModel>(mockValidator.Object);

        // Assert
        mockValidator.Verify(m => m.Invoke(null), Times.Never());
        result.Should().Equal(services);
    }

    [Fact]
    public void UpdateSingletonInstance_ForOverloadWithTwoGenericParameter_WithInstanceFoundInServiceCollectionButNotSingleton_ShouldDoNothingAndReturnsUnchangedServiceCollection()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        services.AddScoped<IUpdateSingletonInstanceTestModel, UpdateSingletonInstanceTestModel>();
        var mockValidator = new Mock<Action<UpdateSingletonInstanceTestModel>>();

        // Act
        var result = services.UpdateSingletonInstance<IUpdateSingletonInstanceTestModel, UpdateSingletonInstanceTestModel>(mockValidator.Object);

        // Assert
        mockValidator.Verify(m => m.Invoke(null), Times.Never());
        result.Should().Equal(services);
    }

    [Fact]
    public void UpdateSingletonInstance_ForOverloadWithTwoGenericParameter_WithSingletonInstanceFoundInServiceCollection_ShouldUpdateInstanceAndReturnsUpdatedServiceCollection()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        var singletonInstance = new UpdateSingletonInstanceTestModel
        {
            Name = "Test",
            Order = 1,
        };

        services.AddSingleton<IUpdateSingletonInstanceTestModel>(singletonInstance);

        static void Action(UpdateSingletonInstanceTestModel a)
        {
            a.Name = "Changed";
            a.Order = 1;
        }

        var mockValidator = new Mock<Action<UpdateSingletonInstanceTestModel>>();
        mockValidator.Setup(x => x.Invoke(singletonInstance)).Callback((Action<UpdateSingletonInstanceTestModel>)Action);

        // Act
        var result = services.UpdateSingletonInstance<IUpdateSingletonInstanceTestModel, UpdateSingletonInstanceTestModel>(mockValidator.Object);

        // Assert
        mockValidator.Verify(m => m.Invoke(singletonInstance), Times.Once());
        result.Should().BeEquivalentTo(services);
        result.Should().Contain(s => s.ImplementationInstance as UpdateSingletonInstanceTestModel == singletonInstance);
        result.Should().Contain(s => (s.ImplementationInstance as UpdateSingletonInstanceTestModel).Name == "Changed");
        result.Should().Contain(s => (s.ImplementationInstance as UpdateSingletonInstanceTestModel).Order == 1);
        result.Should().NotContain(s => (s.ImplementationInstance as UpdateSingletonInstanceTestModel).Name == "Test");
    }

    #endregion

    #region ApplyDateSearch

    [Theory]
    [ClassData(typeof(DateSearchInvalidDataFixture))]
    public void ApplyDateSearch_WithInvalidParameters_ReturnsInputList(List<DateSearchTestModel> input,
                                                                       Expression<Func<DateSearchTestModel, DateTime?>> propertySelector,
                                                                       DateTime? startDate,
                                                                       DateTime? endDate,
                                                                       List<DateSearchTestModel> expected)
    {
        // Arrange

        // Act
        var result = input.ApplyDateSearch(propertySelector, startDate, endDate);

        // Assert
        result.Should().BeEquivalentTo(expected);
        result?.Count().Should().Be(expected?.Count);
    }

    [Theory]
    [ClassData(typeof(DateSearchValidDataFixture))]
    public void ApplyDateSearch_WithValidParameters_ReturnsSearchAppliedList(List<DateSearchTestModel> input,
                                                                             Expression<Func<DateSearchTestModel, DateTime?>> propertySelector,
                                                                             DateTime? startDate,
                                                                             DateTime? endDate,
                                                                             List<DateSearchTestModel> expected)
    {
        // Arrange

        // Act
        var result = input.ApplyDateSearch(propertySelector, startDate, endDate);

        // Assert
        result.Should().BeEquivalentTo(expected);
        result.Should().Contain(expected);
        result?.Count().Should().Be(expected?.Count);
    }

    #endregion
}
