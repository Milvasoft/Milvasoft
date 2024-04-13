using FluentAssertions;
using Milvasoft.Components.Rest.Request;

namespace Milvasoft.UnitTests.ComponentsTests.RestTests.RequestTests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1042:The member referenced by the MemberData attribute returns untyped data rows", Justification = "<Pending>")]
[Trait("Rest Components Unit Tests", "Milvasoft.Components.Rest project unit tests.")]
public class FilterCriteriaTests
{
    /// <summary>
    /// filter criteria 
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> FilterCriteriaDataWithFilterByIsNullOrEmptyOrWhitespace()
    {
        yield return new object[] { new FilterCriteria { FilterBy = null } };
        yield return new object[] { new FilterCriteria { FilterBy = "" } };
        yield return new object[] { new FilterCriteria { FilterBy = " " } };
    }

    /// <summary>
    /// filter criteria
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> FilterCriteriaDataWithFilterByIsListOrReferenceProp()
    {
        yield return new object[] { new FilterCriteria { FilterBy = "Prop[ChildrenPropName]" } };
        yield return new object[] { new FilterCriteria { FilterBy = "Prop.ChildrenPropName" } };
    }

    #region FilterByContainsSpecialChars

    [Theory]
    [MemberData(nameof(FilterCriteriaDataWithFilterByIsNullOrEmptyOrWhitespace))]
    public void FilterByContainsSpecialChars_WithInvalidFilterBy_ShouldReturnFalse(FilterCriteria criteria)
    {
        // Arrange

        // Act
        var result = criteria.FilterByContainsSpecialChars();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void FilterByContainsSpecialChars_WithFilterByNotContainsSpecialCharacter_ShouldReturnFalse()
    {
        // Arrange
        var criteria = new FilterCriteria
        {
            FilterBy = "PropName"
        };

        // Act
        var result = criteria.FilterByContainsSpecialChars();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(FilterCriteriaDataWithFilterByIsListOrReferenceProp))]
    public void FilterByContainsSpecialChars_WithValidFilterBy_ShouldReturnTrue(FilterCriteria criteria)
    {
        // Arrange

        // Act
        var result = criteria.FilterByContainsSpecialChars();

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region GetUntilSpecialCharFromFilterBy

    [Theory]
    [MemberData(nameof(FilterCriteriaDataWithFilterByIsNullOrEmptyOrWhitespace))]
    public void GetUntilSpecialCharFromFilterBy_WithFilterByIsNullOrEmptyOrWhitespace_ShouldReturnFilterBy(FilterCriteria criteria)
    {
        // Arrange

        // Act
        var result = criteria.GetUntilSpecialCharFromFilterBy();

        // Assert
        result.Should().Be(criteria.FilterBy);
    }

    [Fact]
    public void GetUntilSpecialCharFromFilterBy_WithFilterByNotContainsSpecialCharacter_ShouldReturnCorrectResult()
    {
        // Arrange
        var criteria = new FilterCriteria
        {
            FilterBy = "PropName"
        };

        // Act
        var result = criteria.GetUntilSpecialCharFromFilterBy();

        // Assert
        result.Should().Be(criteria.FilterBy);
    }

    [Theory]
    [MemberData(nameof(FilterCriteriaDataWithFilterByIsListOrReferenceProp))]
    public void GetUntilSpecialCharFromFilterBy_WithValidFilterBy_ShouldReturnCorrectResult(FilterCriteria criteria)
    {
        // Arrange

        // Act
        var result = criteria.GetUntilSpecialCharFromFilterBy();

        // Assert
        result.Should().Be("Prop");
    }

    #endregion

    #region GetAfterSpecialCharFromFilterBy

    [Theory]
    [MemberData(nameof(FilterCriteriaDataWithFilterByIsNullOrEmptyOrWhitespace))]
    public void GetAfterSpecialCharFromFilterBy_WithFilterByIsNullOrEmptyOrWhitespace_ShouldReturnFilterBy(FilterCriteria criteria)
    {
        // Arrange

        // Act
        var result = criteria.GetAfterSpecialCharFromFilterBy();

        // Assert
        result.Should().Be(criteria.FilterBy);
    }

    [Fact]
    public void GetAfterSpecialCharFromFilterBy_WithFilterByNotContainsSpecialCharacter_ShouldReturnCorrectResult()
    {
        // Arrange
        var criteria = new FilterCriteria
        {
            FilterBy = "PropName"
        };

        // Act
        var result = criteria.GetAfterSpecialCharFromFilterBy();

        // Assert
        result.Should().Be(criteria.FilterBy);
    }

    [Theory]
    [InlineData("Prop[ChildrenPropName]", "[ChildrenPropName]")]
    [InlineData("Prop.ChildrenPropName", ".ChildrenPropName")]
    public void GetAfterSpecialCharFromFilterBy_WithValidFilterBy_ShouldReturnCorrectResult(string filterBy, string expected)
    {
        // Arrange
        var criteria = new FilterCriteria
        {
            FilterBy = filterBy
        };

        // Act
        var result = criteria.GetAfterSpecialCharFromFilterBy();

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region GetChildrenPropertyNameFromFilterBy

    [Theory]
    [MemberData(nameof(FilterCriteriaDataWithFilterByIsNullOrEmptyOrWhitespace))]
    public void GetChildrenPropertyNameFromFilterBy_WithFilterByIsNullOrEmptyOrWhitespace_ShouldReturnFilterBy(FilterCriteria criteria)
    {
        // Arrange

        // Act
        var result = criteria.GetChildrenPropertyNameFromFilterBy();

        // Assert
        result.Should().Be(criteria.FilterBy);
    }

    [Fact]
    public void GetChildrenPropertyNameFromFilterBy_WithFilterByNotContainsSpecialCharacter_ShouldReturnCorrectResult()
    {
        // Arrange
        var criteria = new FilterCriteria
        {
            FilterBy = "PropName"
        };

        // Act
        var result = criteria.GetChildrenPropertyNameFromFilterBy();

        // Assert
        result.Should().Be(criteria.FilterBy);
    }

    [Theory]
    [MemberData(nameof(FilterCriteriaDataWithFilterByIsListOrReferenceProp))]
    public void GetChildrenPropertyNameFromFilterBy_WithValidFilterBy_ShouldReturnCorrectResult(FilterCriteria criteria)
    {
        // Arrange

        // Act
        var result = criteria.GetChildrenPropertyNameFromFilterBy();

        // Assert
        result.Should().Be("ChildrenPropName");
    }

    #endregion

    #region GetFilterByAsListFormat

    [Theory]
    [MemberData(nameof(FilterCriteriaDataWithFilterByIsNullOrEmptyOrWhitespace))]
    public void GetFilterByAsListFormat_WithFilterByIsNullOrEmptyOrWhitespace_ShouldReturnFormattedFilterBy(FilterCriteria criteria)
    {
        // Arrange

        // Act
        var result = criteria.GetFilterByAsListFormat("ListPropName");

        // Assert
        result.Should().Be($"ListPropName[{criteria.FilterBy}]");
    }

    [Fact]
    public void GetFilterByAsListFormat_WithFilterByNotContainsSpecialCharacter_ShouldReturnCorrectResult()
    {
        // Arrange
        var criteria = new FilterCriteria
        {
            FilterBy = "PropName"
        };
        var input = "ListPropName";

        // Act
        var result = criteria.GetFilterByAsListFormat(input);

        // Assert
        result.Should().Be($"{input}[{criteria.FilterBy}]");
    }

    #endregion

    #region GetFilterByAsReferenceFormat

    [Theory]
    [MemberData(nameof(FilterCriteriaDataWithFilterByIsNullOrEmptyOrWhitespace))]
    public void GetFilterByAsReferenceFormat_WithFilterByIsNullOrEmptyOrWhitespace_ShouldReturnFormattedFilterBy(FilterCriteria criteria)
    {
        // Arrange

        // Act
        var result = criteria.GetFilterByAsReferenceFormat("ReferencePropName");

        // Assert
        result.Should().Be($"ReferencePropName.{criteria.FilterBy}");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void GetFilterByAsReferenceFormat_WithInputIsNullOrEmptyOrWhitespace_ShouldReturnCorrectResult(string input)
    {
        // Arrange
        var criteria = new FilterCriteria
        {
            FilterBy = "PropName"
        };

        // Act
        var result = criteria.GetFilterByAsReferenceFormat(input);

        // Assert
        result.Should().Be($"{input}.{criteria.FilterBy}");
    }

    [Fact]
    public void GetFilterByAsReferenceFormat_WithFilterByNotContainsSpecialCharacter_ShouldReturnCorrectResult()
    {
        // Arrange
        var criteria = new FilterCriteria
        {
            FilterBy = "PropName"
        };
        var input = "ListPropName";

        // Act
        var result = criteria.GetFilterByAsReferenceFormat(input);

        // Assert
        result.Should().Be($"{input}.{criteria.FilterBy}");
    }

    #endregion
}
