using FluentAssertions;
using Milvasoft.Core;
using Milvasoft.Core.Extensions;
using Milvasoft.UnitTests.CoreTests.ExtensionsTests.CollectionTests.Fixtures;

namespace Milvasoft.UnitTests.CoreTests.ExtensionsTests.CollectionTests;

public partial class CollectionExtensionTests
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
    public void IsNullOrEmpty_ValidEnumerableInput_ReturnsExpected(IEnumerable<byte> input, bool expected)
    {
        // Arrange

        // Act
        var result = input.IsNullOrEmpty();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void IsNullOrEmpty_EmptyDictionaryInput_ReturnsTrue()
    {
        // Arrange
        var input = new Dictionary<string, object>();

        // Act
        var result = input.IsNullOrEmpty();

        // Assert
        result.Should().Be(true);
    }

    #endregion

    #region PropertyExists

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    public void PropertyExists_NullOrEmptyOrWhiteSpacePropertyNameInput_ReturnsFalse(string propertyName, bool expected)
    {
        // Arrange
        IQueryable<PropertyExistsTestModel> inputList = null;

        // Act
        var result = inputList.PropertyExists(propertyName);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("poco", true)]
    [InlineData("Poco", true)]
    [InlineData("NotExistsPropName", false)]
    public void PropertyExists_PropertyNameInput_ReturnsExpected(string propertyName, bool expected)
    {
        // Arrange
        IQueryable<PropertyExistsTestModel> inputList = null;

        // Act
        var result = inputList.PropertyExists(propertyName);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region OrderByProperty

    /// <summary>
    /// source , property name , expected result
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> InvalidSourceForOrderByPropertyMethodData()
    {
        var emptySource = new List<PropertyExistsTestModel>().AsQueryable();
        var validSource = new List<PropertyExistsTestModel>() { new() { Poco = 1 } }.AsQueryable();

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
        var validSource = new List<PropertyExistsTestModel>()
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
    public void OrderByProperty_InvalidSourceOrPropertyNameInput_ReturnsSource(IQueryable<PropertyExistsTestModel> source, string propertyName, IQueryable<PropertyExistsTestModel> expected)
    {
        // Arrange

        // Act
        var result = source.OrderByProperty(propertyName);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [MemberData(nameof(ValidSourceForOrderByPropertyMethodData))]
    public void OrderByProperty_ValidSourceAndPropertyNameInput_ReturnsAscendingOrderedSource(IQueryable<PropertyExistsTestModel> source, string propertyName)
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
    /// source , property name , expected result
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> InvalidSourceForOrderByPropertyDescendingMethodData()
    {
        var emptySource = new List<PropertyExistsTestModel>().AsQueryable();
        var validSource = new List<PropertyExistsTestModel>() { new() { Poco = 1 } }.AsQueryable();

        yield return new object[] { emptySource, "", emptySource };
        yield return new object[] { null, "", null };
        yield return new object[] { validSource, "NotExistsPropName", validSource };
    }

    /// <summary>
    /// source , property name
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> ValidSourceForOrderByPropertyDescendingMethodData()
    {
        var validSource = new List<PropertyExistsTestModel>()
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
    public void OrderByPropertyDescending_InvalidSourceOrPropertyNameInput_ReturnsSource(IQueryable<PropertyExistsTestModel> source, string propertyName, IQueryable<PropertyExistsTestModel> expected)
    {
        // Arrange

        // Act
        var result = source.OrderByPropertyDescending(propertyName);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [MemberData(nameof(ValidSourceForOrderByPropertyMethodData))]
    public void OrderByPropertyDescending_ValidSourceAndPropertyNameInput_ReturnsDescendingOrderedSource(IQueryable<PropertyExistsTestModel> source, string propertyName)
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
    public void ToBatches_InvalidSourceInput_ReturnsEmptySource(List<byte> source, List<byte> expected)
    {
        // Arrange

        // Act
        var result = source.ToBatches();

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [MemberData(nameof(ValidSourceForToBatchesMethodData))]
    public void ToBatches_ValidSourceInput_ReturnsValidBatches(List<byte> source, int batchSize, int expectedBatchCount)
    {
        // Arrange

        // Act
        var result = source.ToBatches(batchSize);

        // Assert
        result.Should().HaveCount(expectedBatchCount);
    }

    #endregion

    //TODO TEST updatesingleton instance tests will be added.
}
