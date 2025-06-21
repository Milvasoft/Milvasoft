using FluentAssertions;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Core.Exceptions;
using Milvasoft.Core.Utils.Enums;
using Milvasoft.DataAccess.EfCore.Utils.Enums;
using Milvasoft.FileOperations.Enums;
using Milvasoft.Identity.TokenProvider;
using Milvasoft.Identity.TokenProvider.AuthToken;
using Milvasoft.Storage.Models;

namespace Milvasoft.UnitTests.ComponentsTests.TypesTests;

[Trait("Enums Unit Tests", "Enums unit tests.")]
public class EnumTests
{
    #region AggregationType

    [Theory]
    [InlineData(AggregationType.Avg, "Avg")]
    [InlineData(AggregationType.Sum, "Sum")]
    [InlineData(AggregationType.Min, "Min")]
    [InlineData(AggregationType.Max, "Max")]
    [InlineData(AggregationType.Count, "Count")]
    public void AggregationType_ShouldHaveExpectedNames(AggregationType aggregationType, string expectedName) => aggregationType.ToString().Should().Be(expectedName);

    [Fact]
    public void AggregationType_ShouldContainExpectedValues()
    {
        // Arrange
        var expectedValues = new[]
        {
            AggregationType.Avg,
            AggregationType.Sum,
            AggregationType.Min,
            AggregationType.Max,
            AggregationType.Count
        };

        // Act
        var enumValues = Enum.GetValues<AggregationType>();

        // Assert
        enumValues.Should().BeEquivalentTo(expectedValues);
    }

    [Fact]
    public void AggregationType_ShouldContainFiveValues()
    {
        // Act
        var enumValues = Enum.GetValues<AggregationType>();

        // Assert
        enumValues.Length.Should().Be(5);
    }

    #endregion

    #region Purpose Tests

    [Theory]
    [InlineData(Purpose.EmailConfirm, "EmailConfirm")]
    [InlineData(Purpose.EmailChange, "EmailChange")]
    [InlineData(Purpose.PasswordReset, "PasswordReset")]
    public void Purpose_ShouldHaveExpectedNames(Purpose purpose, string expectedName) => purpose.ToString().Should().Be(expectedName);

    [Fact]
    public void Purpose_ShouldContainThreeValues() => Enum.GetValues<Purpose>().Length.Should().Be(3);

    #endregion

    #region SecurityKeyType Tests

    [Theory]
    [InlineData(SecurityKeyType.Symmetric, "Symmetric")]
    [InlineData(SecurityKeyType.Rsa, "Rsa")]
    public void SecurityKeyType_ShouldHaveExpectedNames(SecurityKeyType type, string expectedName) => type.ToString().Should().Be(expectedName);

    [Fact]
    public void SecurityKeyType_ShouldContainTwoValues() => Enum.GetValues<SecurityKeyType>().Length.Should().Be(2);

    #endregion

    #region FileValidationResult Tests

    [Theory]
    [InlineData(FileValidationResult.Valid, 1)]
    [InlineData(FileValidationResult.FileSizeTooBig, 2)]
    [InlineData(FileValidationResult.InvalidFileExtension, 3)]
    [InlineData(FileValidationResult.NullFile, 4)]
    public void FileValidationResult_ShouldHaveExpectedValues(FileValidationResult result, int expectedValue) => ((int)result).Should().Be(expectedValue);

    [Fact]
    public void FileValidationResult_ShouldContainFourValues() => Enum.GetValues<FileValidationResult>().Length.Should().Be(4);

    #endregion

    #region FileType Tests

    [Theory]
    [InlineData(FileType.Image, "Image")]
    [InlineData(FileType.Video, "Video")]
    [InlineData(FileType.ARModel, "ARModel")]
    [InlineData(FileType.Audio, "Audio")]
    [InlineData(FileType.Document, "Document")]
    [InlineData(FileType.Compressed, "Compressed")]
    [InlineData(FileType.EMail, "EMail")]
    [InlineData(FileType.Font, "Font")]
    [InlineData(FileType.InternetRelated, "InternetRelated")]
    public void FileType_ShouldHaveExpectedNames(FileType type, string expectedName) => type.ToString().Should().Be(expectedName);

    [Fact]
    public void FileType_ShouldContainNineValues() => Enum.GetValues<FileType>().Length.Should().Be(9);

    #endregion

    #region SoftDeletionState Tests

    [Theory]
    [InlineData(SoftDeletionState.Active, "Active")]
    [InlineData(SoftDeletionState.Passive, "Passive")]
    public void SoftDeletionState_ShouldHaveExpectedNames(SoftDeletionState state, string expectedName) => state.ToString().Should().Be(expectedName);

    [Fact]
    public void SoftDeletionState_ShouldContainTwoValues() => Enum.GetValues<SoftDeletionState>().Length.Should().Be(2);

    #endregion

    #region SaveChangesChoice Tests

    [Theory]
    [InlineData(SaveChangesChoice.AfterEveryOperation, "AfterEveryOperation")]
    [InlineData(SaveChangesChoice.Manual, "Manual")]
    public void SaveChangesChoice_ShouldHaveExpectedNames(SaveChangesChoice choice, string expectedName) => choice.ToString().Should().Be(expectedName);

    [Fact]
    public void SaveChangesChoice_ShouldContainTwoValues() => Enum.GetValues<SaveChangesChoice>().Length.Should().Be(2);

    #endregion

    #region MaskOption Tests

    [Theory]
    [InlineData(MaskOption.AtTheBeginingOfString, 1)]
    [InlineData(MaskOption.InTheMiddleOfString, 2)]
    [InlineData(MaskOption.AtTheEndOfString, 3)]
    public void MaskOption_ShouldHaveExpectedValues(MaskOption option, byte expectedValue) => ((byte)option).Should().Be(expectedValue);

    [Fact]
    public void MaskOption_ShouldContainThreeValues() => Enum.GetValues<MaskOption>().Length.Should().Be(3);

    #endregion

    #region SortType Tests

    [Theory]
    [InlineData(SortType.Asc, "Asc")]
    [InlineData(SortType.Desc, "Desc")]
    public void SortType_ShouldHaveExpectedNames(SortType sortType, string expectedName) => sortType.ToString().Should().Be(expectedName);

    [Fact]
    public void SortType_ShouldContainTwoValues() => Enum.GetValues<SortType>().Length.Should().Be(2);

    #endregion

    #region MessageType Tests

    [Theory]
    [InlineData(MessageType.Information, 1)]
    [InlineData(MessageType.Validation, 2)]
    [InlineData(MessageType.Unauthorized, 3)]
    [InlineData(MessageType.Forbidden, 4)]
    [InlineData(MessageType.Warning, 5)]
    [InlineData(MessageType.Error, 6)]
    [InlineData(MessageType.Fatal, 7)]
    public void MessageType_ShouldHaveExpectedValues(MessageType messageType, byte expectedValue) => ((byte)messageType).Should().Be(expectedValue);

    [Fact]
    public void MessageType_ShouldContainSevenValues() => Enum.GetValues<MessageType>().Length.Should().Be(7);

    #endregion

    #region FilterType Tests

    [Fact]
    public void FilterType_ShouldContainExpectedValues()
    {
        var expectedValues = new[]
        {
            FilterType.Between,
            FilterType.Contains,
            FilterType.DoesNotContain,
            FilterType.StartsWith,
            FilterType.EndsWith,
            FilterType.EqualTo,
            FilterType.NotEqualTo,
            FilterType.GreaterThan,
            FilterType.GreaterThanOrEqualTo,
            FilterType.LessThan,
            FilterType.LessThanOrEqualTo,
            FilterType.IsEmpty,
            FilterType.IsNotEmpty,
            FilterType.IsNull,
            FilterType.IsNotNull,
            FilterType.IsNullOrWhiteSpace,
            FilterType.IsNotNullNorWhiteSpace,
            FilterType.In,
            FilterType.NotIn,
            FilterType.DateEqualTo
        };

        Enum.GetValues<FilterType>().Should().BeEquivalentTo(expectedValues);
    }

    [Fact]
    public void FilterType_ShouldContainTwentyValues() => Enum.GetValues<FilterType>().Length.Should().Be(20);

    #endregion

    #region MilvaException Tests

    [Theory]
    [InlineData(MilvaException.Base, 0)]
    [InlineData(MilvaException.General, 1)]
    [InlineData(MilvaException.CannotFindEntity, 2)]
    [InlineData(MilvaException.AddingNewEntityWithExistsId, 3)]
    [InlineData(MilvaException.Encryption, 4)]
    [InlineData(MilvaException.CannotFindEnumById, 5)]
    [InlineData(MilvaException.CannotStartRedisServer, 6)]
    [InlineData(MilvaException.DeletingEntityWithRelations, 7)]
    [InlineData(MilvaException.DeletingInvalidEntity, 8)]
    [InlineData(MilvaException.TenancyNameRequired, 9)]
    [InlineData(MilvaException.EmptyList, 10)]
    [InlineData(MilvaException.InvalidNumericValue, 11)]
    [InlineData(MilvaException.InvalidParameter, 12)]
    [InlineData(MilvaException.InvalidStringExpression, 13)]
    [InlineData(MilvaException.NullParameter, 14)]
    [InlineData(MilvaException.FeatureNotImplemented, 15)]
    [InlineData(MilvaException.UpdatingInvalidEntity, 16)]
    [InlineData(MilvaException.NotLoggedInUser, 17)]
    [InlineData(MilvaException.AnotherLoginExists, 18)]
    [InlineData(MilvaException.OldVersion, 19)]
    [InlineData(MilvaException.InvalidLicence, 20)]
    [InlineData(MilvaException.ExpiredDemo, 21)]
    [InlineData(MilvaException.NeedForceOperation, 22)]
    [InlineData(MilvaException.CannotGetResponse, 23)]
    [InlineData(MilvaException.IdentityResult, 24)]
    [InlineData(MilvaException.CannotUpdateOrDeleteDefaultRecord, 25)]
    [InlineData(MilvaException.Validation, 26)]
    [InlineData(MilvaException.InvalidRelatedProcess, 27)]
    [InlineData(MilvaException.WrongPaginationParams, 28)]
    [InlineData(MilvaException.WrongRequestedPageNumber, 29)]
    [InlineData(MilvaException.WrongRequestedItemCount, 30)]
    [InlineData(MilvaException.InvalidTenantId, 31)]
    public void MilvaException_ShouldHaveExpectedValues(MilvaException exception, int expectedValue) => ((int)exception).Should().Be(expectedValue);

    [Fact]
    public void MilvaException_ShouldContainExpectedCount()
    {
        // Act
        var values = Enum.GetValues<MilvaException>();

        // Assert
        values.Length.Should().Be(32);
    }

    #endregion
}
