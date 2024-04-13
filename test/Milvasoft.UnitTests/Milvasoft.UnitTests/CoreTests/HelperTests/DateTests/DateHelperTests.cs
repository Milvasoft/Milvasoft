using FluentAssertions;
using Milvasoft.Core.Helpers;
using Milvasoft.UnitTests.CoreTests.HelperTests.DateTests.Fixtures;
using System.Linq.Expressions;

namespace Milvasoft.UnitTests.CoreTests.HelperTests.DateTests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1042:The member referenced by the MemberData attribute returns untyped data rows", Justification = "<Pending>")]
[Trait("Core Unit Tests", "Milvasoft.Core project unit tests.")]
public partial class DateHelperTests
{
    #region CreateDateSearchExpression

    /// <summary>
    /// searchList
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> SearchListForCreateDateSearchExpressionMethod()
    {
        var searchList = new List<CreateDateSearchExpressionTestModelFixture>()
        {
            new() {
                Id = 1,
                Date = DateTime.Now,
                NullableDate = DateTime.Now
            },
            new() {
                Id = 2,
                Date = DateTime.Now.AddYears(1),
                NullableDate = DateTime.Now.AddYears(1),
            },
            new() {
                Id = 3,
                Date = DateTime.Now.AddYears(2),
                NullableDate = DateTime.Now.AddYears(1),
            },
            new() {
                Id = 4,
                Date = DateTime.Now.AddYears(2),
                NullableDate = null,
            }
        };

        yield return new object[] { searchList };
    }

    [Fact]
    public void CreateDateSearchExpression_WithPropertySelectorIsNull_ShouldReturnNull()
    {
        // Arrange
        Expression<Func<CreateDateSearchExpressionTestModelFixture, DateTime?>> datePropertySelector = null;

        // Act
        var result = CommonHelper.CreateDateSearchExpression(datePropertySelector, DateTime.Now, DateTime.Now.AddDays(1));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CreateDateSearchExpression_WithBothDateParamsAreNull_ShouldReturnNull()
    {
        // Arrange
        Expression<Func<CreateDateSearchExpressionTestModelFixture, DateTime?>> datePropertySelector = i => i.Date;

        // Act
        var result = CommonHelper.CreateDateSearchExpression(datePropertySelector, null, null);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(SearchListForCreateDateSearchExpressionMethod))]
    public void CreateDateSearchExpression_WithBothDateParamsAreNotNull_ShouldReturnCorrectExpression(List<CreateDateSearchExpressionTestModelFixture> searchList)
    {
        // Arrange
        Expression<Func<CreateDateSearchExpressionTestModelFixture, DateTime?>> datePropertySelector = i => i.Date;
        DateTime startDate = DateTime.Now.AddDays(1);
        DateTime endDate = startDate.AddYears(1);
        var expectedResult = searchList.Where(i => i.Date >= startDate && i.Date <= endDate);

        // Act
        var resultExpression = CommonHelper.CreateDateSearchExpression(datePropertySelector, startDate, endDate);
        var result = searchList.Where(resultExpression.Compile());

        // Assert
        result.Should().HaveCount(expectedResult.Count());
        result.Should().Contain(i => i.Id == 2);
    }

    [Theory]
    [MemberData(nameof(SearchListForCreateDateSearchExpressionMethod))]
    public void CreateDateSearchExpression_WithStartDateIsNotNullButEndDateIsNull_ShouldReturnCorrectExpression(List<CreateDateSearchExpressionTestModelFixture> searchList)
    {
        // Arrange
        Expression<Func<CreateDateSearchExpressionTestModelFixture, DateTime?>> datePropertySelector = i => i.Date;
        DateTime startDate = DateTime.Now.AddDays(1);
        DateTime? endDate = null;
        var expectedResult = searchList.Where(i => i.Date >= startDate);

        // Act
        var resultExpression = CommonHelper.CreateDateSearchExpression(datePropertySelector, startDate, endDate);
        var result = searchList.Where(resultExpression.Compile());

        // Assert
        result.Should().HaveCount(expectedResult.Count());
        result.Should().Contain(i => i.Id == 2);
        result.Should().Contain(i => i.Id == 3);
    }

    [Theory]
    [MemberData(nameof(SearchListForCreateDateSearchExpressionMethod))]
    public void CreateDateSearchExpression_WithEndDateIsNotNullButStartDateIsNull_ShouldReturnCorrectExpression(List<CreateDateSearchExpressionTestModelFixture> searchList)
    {
        // Arrange
        Expression<Func<CreateDateSearchExpressionTestModelFixture, DateTime?>> datePropertySelector = i => i.Date;
        DateTime? startDate = null;
        DateTime endDate = DateTime.Now.AddDays(1);
        var expectedResult = searchList.Where(i => i.Date <= endDate);

        // Act
        var resultExpression = CommonHelper.CreateDateSearchExpression(datePropertySelector, startDate, endDate);
        var result = searchList.Where(resultExpression.Compile());

        // Assert
        result.Should().HaveCount(expectedResult.Count());
        result.Should().Contain(i => i.Id == 1);
    }

    [Fact]
    public void CreateDateSearchExpression_ForOverloadWithNullableDatetimeProperty_WithBothDateParamsAreNull_ShouldReturnNull()
    {
        // Arrange
        Expression<Func<CreateDateSearchExpressionTestModelFixture, DateTime?>> datePropertySelector = i => i.NullableDate;

        // Act
        var result = CommonHelper.CreateDateSearchExpression(datePropertySelector, null, null);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(SearchListForCreateDateSearchExpressionMethod))]
    public void CreateDateSearchExpression_ForOverloadWithNullableDatetimeProperty_WithBothDateParamsAreNotNull_ShouldReturnCorrectExpression(List<CreateDateSearchExpressionTestModelFixture> searchList)
    {
        // Arrange
        Expression<Func<CreateDateSearchExpressionTestModelFixture, DateTime?>> datePropertySelector = i => i.NullableDate;
        DateTime startDate = DateTime.Now.AddDays(1);
        DateTime endDate = startDate.AddYears(1);
        var expectedResult = searchList.Where(i => i.NullableDate >= startDate && i.NullableDate <= endDate);

        // Act
        var resultExpression = CommonHelper.CreateDateSearchExpression(datePropertySelector, startDate, endDate);
        var result = searchList.Where(resultExpression.Compile());

        // Assert
        result.Should().HaveCount(expectedResult.Count());
        result.Should().Contain(i => i.Id == 2);
    }

    [Theory]
    [MemberData(nameof(SearchListForCreateDateSearchExpressionMethod))]
    public void CreateDateSearchExpression_ForOverloadWithNullableDatetimeProperty_WithStartDateIsNotNullButEndDateIsNull_ShouldReturnCorrectExpression(List<CreateDateSearchExpressionTestModelFixture> searchList)
    {
        // Arrange
        Expression<Func<CreateDateSearchExpressionTestModelFixture, DateTime?>> datePropertySelector = i => i.NullableDate;
        DateTime startDate = DateTime.Now.AddDays(1);
        DateTime? endDate = null;
        var expectedResult = searchList.Where(i => i.NullableDate >= startDate);

        // Act
        var resultExpression = CommonHelper.CreateDateSearchExpression(datePropertySelector, startDate, endDate);
        var result = searchList.Where(resultExpression.Compile());

        // Assert
        result.Should().HaveCount(expectedResult.Count());
        result.Should().Contain(i => i.Id == 2);
        result.Should().Contain(i => i.Id == 3);
    }

    [Theory]
    [MemberData(nameof(SearchListForCreateDateSearchExpressionMethod))]
    public void CreateDateSearchExpression_ForOverloadWithNullableDatetimeProperty_WithEndDateIsNotNullButStartDateIsNull_ShouldReturnCorrectExpression(List<CreateDateSearchExpressionTestModelFixture> searchList)
    {
        // Arrange
        Expression<Func<CreateDateSearchExpressionTestModelFixture, DateTime?>> datePropertySelector = i => i.NullableDate;
        DateTime? startDate = null;
        DateTime endDate = DateTime.Now.AddDays(1);
        var expectedResult = searchList.Where(i => i.NullableDate <= endDate);

        // Act
        var resultExpression = CommonHelper.CreateDateSearchExpression(datePropertySelector, startDate, endDate);
        var result = searchList.Where(resultExpression.Compile());

        // Assert
        result.Should().HaveCount(expectedResult.Count());
        result.Should().Contain(i => i.Id == 1);
    }

    #endregion

    #region IsBetween

    /// <summary>
    /// date , start time , end time , convert times to utc , expected result
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> DatesAndTimeSpansForIsBetweenMethod()
    {
        TimeZoneInfo tzone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
        var date = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(2024, 01, 01, 11, 00, 00, DateTimeKind.Unspecified), tzone);//14:00
        var dateForTimeSpan = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(2024, 01, 01, 10, 00, 00, DateTimeKind.Unspecified), tzone);//13:00

        yield return new object[] { date, new TimeSpan(13, 00, 00), new TimeSpan(16, 00, 00), false, true };//13:00 - 16:00
        yield return new object[] { date, new TimeSpan(dateForTimeSpan.Ticks), new TimeSpan(dateForTimeSpan.AddHours(-2).Ticks), false, true };//13:00 - 11:00 -> rollover day
        yield return new object[] { date, new TimeSpan(dateForTimeSpan.Ticks), new TimeSpan(dateForTimeSpan.AddHours(3).Ticks), false, true };//13:00 - 16:00
        yield return new object[] { date, new TimeSpan(dateForTimeSpan.AddHours(2).Ticks), new TimeSpan(dateForTimeSpan.AddHours(3).Ticks), false, false };//15:00 - 16:00
        yield return new object[] { date, new TimeSpan(dateForTimeSpan.Ticks), new TimeSpan(dateForTimeSpan.AddHours(5).Ticks), true, true };//13:00 - 18:00 -> 10:00 - 15:00
        yield return new object[] { date, new TimeSpan(dateForTimeSpan.AddHours(2).Ticks), new TimeSpan(dateForTimeSpan.AddHours(3).Ticks), true, false };//15:00 - 16:00 -> 12:00 - 13:00
    }

    /// <summary>
    /// date , start date , end date  , expected result
    /// </summary>
    /// <returns></returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S6562:Always set the \"DateTimeKind\" when creating new \"DateTime\" instances", Justification = "<Pending>")]
    public static IEnumerable<object[]> DatesForIsBetweenMethod()
    {
        var date = new DateTime(2024, 01, 01, 11, 00, 00);
        var date2 = date.AddDays(-1);
        var date3 = date.AddDays(1);
        var date4 = date.AddDays(2);

        yield return new object[] { date, date, date, true };
        yield return new object[] { date, date, date4, true };
        yield return new object[] { date, date2, date3, true };
        yield return new object[] { date, date3, date4, false };
    }

    [Theory]
    [MemberData(nameof(DatesAndTimeSpansForIsBetweenMethod))]
    public void IsBetween_ForOverloadWithTwoTimeSpanParameter_WithAllParametersAreValid_ShouldReturnCorrectResult(DateTime inputDate, TimeSpan startTime, TimeSpan endTime, bool convertTimesToUtc, bool expectedResult)
    {
        // Arrange

        // Act
        var result = inputDate.IsBetween(startTime, endTime, convertTimesToUtc);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [MemberData(nameof(DatesAndTimeSpansForIsBetweenMethod))]
    public void IsBetween_ForOverloadWithThreeTimeSpanParameter_WithAllParametersAreValid_ShouldReturnCorrectResult(DateTime inputDate, TimeSpan startTime, TimeSpan endTime, bool convertTimesToUtc, bool expectedResult)
    {
        // Arrange
        TimeZoneInfo tzone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
        var dateForTimeSpan = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(2024, 01, 01, 10, 00, 00, DateTimeKind.Unspecified), tzone);//13:00
        var compareTime = new TimeSpan(dateForTimeSpan.Ticks);

        // Act
        var result = inputDate.IsBetween(compareTime, startTime, endTime, convertTimesToUtc);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [MemberData(nameof(DatesForIsBetweenMethod))]
    public void IsBetween_ForOverloadWithTwoDateTimeParameter_WithAllParametersAreValid_ShouldReturnCorrectResult(DateTime inputDate, DateTime startDate, DateTime endDate, bool expectedResult)
    {
        // Arrange

        // Act
        var result = inputDate.IsBetween(startDate, endDate);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [MemberData(nameof(DatesForIsBetweenMethod))]
    public void IsBetween_ForOverloadWithTwoDateTimeAndOneTimeSpanParameter_WithAllParametersAreValidConvertTimeToUtcFalse_ShouldReturnCorrectResult(DateTime inputDate, DateTime startDate, DateTime endDate, bool expectedResult)
    {
        // Arrange
        TimeZoneInfo tzone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
        var dateForTimeSpan = TimeZoneInfo.ConvertTimeFromUtc(inputDate.ToUniversalTime(), tzone);
        var compareTime = new TimeSpan(dateForTimeSpan.Ticks);

        // Act
        var result = inputDate.IsBetween(compareTime, startDate, endDate, false);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [MemberData(nameof(DatesForIsBetweenMethod))]
    public void IsBetween_ForOverloadWithTwoDateTimeAndOneTimeSpanParameter_WithAllParametersAreValidConvertTimeToUtcTrue_ShouldReturnCorrectResult(DateTime inputDate, DateTime startDate, DateTime endDate, bool expectedResult)
    {
        // Arrange
        TimeZoneInfo tzone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
        var dateForTimeSpan = TimeZoneInfo.ConvertTimeFromUtc(inputDate.ToUniversalTime().AddHours(3), tzone);
        var compareTime = new TimeSpan(dateForTimeSpan.Ticks);

        // Act
        var result = inputDate.IsBetween(compareTime, startDate, endDate, true);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S6562:Always set the \"DateTimeKind\" when creating new \"DateTime\" instances", Justification = "<Pending>")]
    public void WithTime_WithInputTimeAndDateIsValid_ShouldReturnDateWithInputTime()
    {
        // Arrange
        var date = DateTime.Now;
        var time = new TimeSpan(13, 13, 13, 13, 13, 13);
        var expected = new DateTime(date.Year, date.Month, date.Day, time.Hours, time.Minutes, time.Seconds, time.Milliseconds, time.Microseconds);

        // Act
        var result = date.WithTime(time);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ConvertToUtc_WithInputIsTurkeyTime_ShouldReturnTimeAsConvertedToUtc()
    {
        // Arrange
        TimeZoneInfo tzone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
        var utcDate = new DateTime(2024, 01, 01, 10, 00, 00, DateTimeKind.Unspecified);
        var dateForTimeSpan = TimeZoneInfo.ConvertTimeFromUtc(utcDate, tzone);//13:00
        var time = new TimeSpan(dateForTimeSpan.Ticks);
        var expected = new TimeSpan(utcDate.Ticks);

        // Act
        var result = time.ConvertToUtc();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ConvertToUtc_WithInputIsUtcTime_ShouldReturnTimeAsConvertedToUtc()
    {
        // Arrange
        var utcDate = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2024, 01, 01, 10, 00, 00, DateTimeKind.Unspecified));
        var time = new TimeSpan(utcDate.Ticks);
        var expected = new TimeSpan(utcDate.AddHours(-3).Ticks);

        // Act
        var result = time.ConvertToUtc();

        // Assert
        result.Should().Be(expected);
    }
}
