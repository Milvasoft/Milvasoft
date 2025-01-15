using FluentAssertions;
using Milvasoft.Core.EntityBases.MultiTenancy;
using Milvasoft.Core.Exceptions;
using Milvasoft.Core.Helpers;
using System.Text.Json;

namespace Milvasoft.UnitTests.CoreTests;

[Trait("Core Unit Tests", "Milvasoft.Core project unit tests.")]
public class TenantIdTests
{
    #region Constructor

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithNullOrEmptyOrWhitespaceTenancyName_ShouldThrowException(string tenancyName)
    {
        // Arrange
        TenantId id;

        // Act
        Action act = () => id = new TenantId(tenancyName, 1);

        // Assert
        act.Should().Throw<MilvaUserFriendlyException>().WithMessage("TenancyNameRequired");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithInvalidBranchNo_ShouldThrowException(int branchNo)
    {
        // Arrange
        TenantId id;
        var tenancyName = "milvasoft";

        // Act
        Action act = () => id = new TenantId(tenancyName, branchNo);

        // Assert
        act.Should().Throw<MilvaDeveloperException>().WithMessage("Branch No cannot be 0 or less.");
    }

    [Fact]
    public void Constructor_WithValidTenancyNameAndBranchNo_ShouldBuildCorrectly()
    {
        // Arrange
        var tenancyName = "milvasoft";
        var branchNo = 1;

        // Act
        var id = new TenantId(tenancyName, branchNo);

        // Assert
        id.TenancyName.Should().Be(tenancyName);
        id.BranchNo.Should().Be(branchNo);
        id.GetHashString().Should().Be($"{tenancyName}_{branchNo}".Hash());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid")]
    [InlineData("invalid_1_tenant")]
    [InlineData("_invalid")]
    [InlineData("invalid_")]
    [InlineData(" _ ")]
    [InlineData("_1")]
    [InlineData(" _1")]
    [InlineData("valid_ ")]
    [InlineData("valid_invalid")]
    [InlineData("valid_0")]
    public void Constructor_WithInvalidTenantIdString_ShouldThrowException(string tenantIdString)
    {
        // Arrange
        TenantId id;

        // Act
        Action act = () => id = new TenantId(tenantIdString);

        // Assert
        act.Should().Throw<MilvaDeveloperException>();
    }

    [Fact]
    public void Constructor_WithValidTenantIdString_ShouldBuildCorrectly()
    {
        // Arrange
        var tenancyName = "milvasoft";
        var branchNo = 1;
        var tenancyString = $"{tenancyName}_{branchNo}";

        // Act
        var id = new TenantId(tenancyString);

        // Assert
        id.TenancyName.Should().Be(tenancyName);
        id.BranchNo.Should().Be(branchNo);
        id.GetHashString().Should().Be($"{tenancyName}_{branchNo}".Hash());
    }

    #endregion

    #region NewTenantId

    [Fact]
    public void NewTenantId_ShouldGenerateCorrectly()
    {
        // Arrange
        var expectedBranchNo = 1;

        // Act
        var result = TenantId.NewTenantId();

        // Assert
        result.TenancyName.Should().NotBeNullOrWhiteSpace();
        result.BranchNo.Should().Be(expectedBranchNo);
    }

    #endregion

    #region GetHashCode

    [Fact]
    public void GetHashCode_ShouldCombineCorrectly()
    {
        // Arrange
        var tenancyName = "milvasoft";
        var branchNo = 1;
        var id = new TenantId(tenancyName, branchNo);
        var expectedHashCode = HashCode.Combine(tenancyName, branchNo);

        // Act
        var result = id.GetHashCode();

        // Assert
        result.Should().Be(expectedHashCode);
    }

    #endregion

    #region GetHashString

    [Fact]
    public void GetHashString_ShouldReturnCorrectly()
    {
        // Arrange
        var tenancyName = "milvasoft";
        var branchNo = 1;
        var id = new TenantId(tenancyName, branchNo);
        var expectedHashString = $"{tenancyName}_{branchNo}".Hash();

        // Act
        var result = id.GetHashString();

        // Assert
        result.Should().Be(expectedHashString);
    }

    #endregion

    #region Equals

    [Fact]
    public void Equals_ForTenantIdOverload_WithTenancyNameEqualButBranchNoDifferent_ShouldReturnFalse()
    {
        // Arrange
        var tenantId = new TenantId("milvasoft_1");
        var otherTenantId = new TenantId("milvasoft_2");

        // Act
        var result = tenantId.Equals(otherTenantId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_ForTenantIdOverload_WithDifferentTenancyName_ShouldReturnFalse()
    {
        // Arrange
        var tenantId = new TenantId("milvasoft_1");
        var otherTenantId = new TenantId("milvasof_1");

        // Act
        var result = tenantId.Equals(otherTenantId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_ForTenantIdOverload_WithEqualTenantIds_ShouldReturnTrue()
    {
        // Arrange
        var tenantId = new TenantId("milvasoft_1");
        var otherTenantId = new TenantId("milvasoft_1");

        // Act
        var result = tenantId.Equals(otherTenantId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_ForObjectOverload_WithObjectIsNotTenantId_ShouldReturnFalse()
    {
        // Arrange
        var tenantId = new TenantId("milvasoft_1");
        object otherTenantId = "milvasoft_2";

        // Act
        var result = tenantId.Equals(otherTenantId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_ForObjectOverload_WithTenancyNameEqualButBranchNoDifferent_ShouldReturnFalse()
    {
        // Arrange
        var tenantId = new TenantId("milvasoft_1");
        object otherTenantId = new TenantId("milvasoft_2");

        // Act
        var result = tenantId.Equals(otherTenantId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_ForObjectOverload_WithDifferentTenancyName_ShouldReturnFalse()
    {
        // Arrange
        var tenantId = new TenantId("milvasoft_1");
        object otherTenantId = new TenantId("milvasof_1");

        // Act
        var result = tenantId.Equals(otherTenantId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_ForObjectOverload_WithEqualTenantIds_ShouldReturnTrue()
    {
        // Arrange
        var tenantId = new TenantId("milvasoft_1");
        object otherTenantId = new TenantId("milvasoft_1");

        // Act
        var result = tenantId.Equals(otherTenantId);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region TenancyNameEquals

    [Fact]
    public void TenancyNameEquals_WithDifferentTenancyName_ShouldReturnFalse()
    {
        // Arrange
        var tenantId = new TenantId("milvasoft_1");
        var otherTenantId = new TenantId("milvasof_1");

        // Act
        var result = tenantId.TenancyNameEquals(otherTenantId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void TenancyNameEquals_WithTenancyNameEqualButBranchNoDifferent_ShouldReturnFalse()
    {
        // Arrange
        var tenantId = new TenantId("milvasoft_1");
        var otherTenantId = new TenantId("milvasoft_2");

        // Act
        var result = tenantId.TenancyNameEquals(otherTenantId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void TenancyNameEquals_WithEqualTenantIds_ShouldReturnTrue()
    {
        // Arrange
        var tenantId = new TenantId("milvasoft_1");
        var otherTenantId = new TenantId("milvasoft_1");

        // Act
        var result = tenantId.TenancyNameEquals(otherTenantId);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region BranchNoEquals

    [Fact]
    public void BranchNoEquals_WithDifferentTenancyName_ShouldReturnFalse()
    {
        // Arrange
        var tenantId = new TenantId("milvasoft_1");
        var otherTenantId = new TenantId("milvasof_1");

        // Act
        var result = tenantId.BranchNoEquals(otherTenantId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void BranchNoEquals_WithDifferentTenancyNameAndBranchNo_ShouldReturnFalse()
    {
        // Arrange
        var tenantId = new TenantId("milvasoft_1");
        var otherTenantId = new TenantId("milvasof_2");

        // Act
        var result = tenantId.BranchNoEquals(otherTenantId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void BranchNoEquals_WithTenancyNameEqualButBranchNoDifferent_ShouldReturnFalse()
    {
        // Arrange
        var tenantId = new TenantId("milvasoft_1");
        var otherTenantId = new TenantId("milvasoft_2");

        // Act
        var result = tenantId.BranchNoEquals(otherTenantId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void BranchNoEquals_WithEqualTenantIds_ShouldReturnTrue()
    {
        // Arrange
        var tenantId = new TenantId("milvasoft_1");
        var otherTenantId = new TenantId("milvasoft_1");

        // Act
        var result = tenantId.BranchNoEquals(otherTenantId);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region ToString

    [Fact]
    public void ToString_ShouldReturnCorrectly()
    {
        // Arrange
        var tenancyName = "milvasoft";
        var branchNo = 1;
        var id = new TenantId(tenancyName, branchNo);
        var expectedString = $"{tenancyName}_{branchNo}";

        // Act
        var result = id.ToString();

        // Assert
        result.Should().Be(expectedString);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("G")]
    public void ToString_WithNullOrEmptyOrWhiteSpaceOrGFormat_ShouldReturnCorrectly(string format)
    {
        // Arrange
        var tenancyName = "milvasoft";
        var branchNo = 1;
        var id = new TenantId(tenancyName, branchNo);
        var expectedString = $"{tenancyName}_{branchNo}";

        // Act
        var result = id.ToString(format);

        // Assert
        result.Should().Be(expectedString);
    }

    [Fact]
    public void ToString_WithHFormat_ShouldReturnCorrectly()
    {
        // Arrange
        var tenancyName = "milvasoft";
        var branchNo = 1;
        var format = "H";
        var id = new TenantId(tenancyName, branchNo);
        var expectedString = $"{tenancyName}_{branchNo}_{id.GetHashString()}";

        // Act
        var result = id.ToString(format);

        // Assert
        result.Should().Be(expectedString);
    }

    [Fact]
    public void ToString_WithInvalidFormat_ShouldThrowException()
    {
        // Arrange
        var tenancyName = "milvasoft";
        var branchNo = 1;
        var format = "O";
        var id = new TenantId(tenancyName, branchNo);

        // Act
        Action act = () => id.ToString(format);

        // Assert
        act.Should().Throw<FormatException>().WithMessage($"The {format} format string is not supported.");
    }

    #endregion

    #region Parse

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid")]
    [InlineData("invalid_1_tenant")]
    [InlineData("_invalid")]
    [InlineData("invalid_")]
    [InlineData(" _ ")]
    [InlineData("_1")]
    [InlineData(" _1")]
    [InlineData("invalid_ ")]
    public void Parse_WithInvalidTenantIdString_ShouldThrowException(string tenantIdString)
    {
        // Arrange

        // Act
        Action act = () => TenantId.Parse(tenantIdString);

        // Assert
        act.Should().Throw<MilvaDeveloperException>().WithMessage("This string is not convertible to TenantId!");
    }

    [Fact]
    public void Parse_WithValidTenantIdString_ShouldReturnCorrectTenantId()
    {
        // Arrange
        var validString = "milvasoft_1";
        var expectedId = new TenantId(validString);

        // Act
        var result = TenantId.Parse(validString);

        // Assert
        result.Should().Be(expectedId);
    }

    #endregion

    #region TryParse

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid")]
    [InlineData("invalid_1_tenant")]
    [InlineData("_invalid")]
    [InlineData("invalid_")]
    [InlineData(" _ ")]
    [InlineData("_1")]
    [InlineData(" _1")]
    [InlineData("invalid_ ")]
    public void TryParse_WithInvalidTenantIdString_ShouldReturnFalse(string tenantIdString)
    {
        // Arrange

        // Act
        var result = TenantId.TryParse(tenantIdString);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("valid_1")]
    [InlineData("valid_2")]
    [InlineData("1_1")]
    public void TryParse_WithValidTenantIdString_ShouldReturnTrue(string tenantIdString)
    {
        // Arrange

        // Act
        var result = TenantId.TryParse(tenantIdString);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Operator Tests

    [Fact]
    public void ImplicitOperator_ShouldReturnExpected()
    {
        // Arrange
        var tenantString = "milvasoft_1";
        var tenantId = new TenantId(tenantString);

        // Act
        string tenantIdAsString = tenantId;

        // Assert
        tenantIdAsString.Should().Be(tenantString);
    }

    [Fact]
    public void ExplicitOperator_WithValidString_ShouldReturnExpected()
    {
        // Arrange
        var tenantString = "milvasoft_1";
        var expected = new TenantId(tenantString);

        // Act
        var tenantId = (TenantId)tenantString;

        // Assert
        tenantId.Should().Be(expected);
    }

    [Fact]
    public void ExplicitOperator_WithInvalidString_ShouldReturnExpected()
    {
        // Arrange
        var tenantString = "milvasoft_1_";

        // Act
        Action act = () => { _ = (TenantId)tenantString; };

        // Assert
        act.Should().Throw<MilvaDeveloperException>();
    }

    [Theory]
    [InlineData("milvasoft", 1, true)]
    [InlineData("milvasoft", 2, false)]
    [InlineData("milvasof", 1, false)]
    public void EqualsOperator_WithTenancyIds_ShouldReturnExpected(string otherTenancyName, int otherBrancNo, bool expected)
    {
        // Arrange
        var tenantId = new TenantId("milvasoft_1");
        var otherTenantId = new TenantId(otherTenancyName, otherBrancNo);

        // Act
        var result = tenantId == otherTenantId;

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("milvasoft", 1, false)]
    [InlineData("milvasoft", 2, true)]
    [InlineData("milvasof", 1, true)]
    public void NotEqualsOperator_WithTenancyIds_ShouldReturnExpected(string otherTenancyName, int otherBrancNo, bool expected)
    {
        // Arrange
        var tenantId = new TenantId("milvasoft_1");
        var otherTenantId = new TenantId(otherTenancyName, otherBrancNo);

        // Act
        var result = tenantId != otherTenantId;

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("milvasoft", 1, false)]
    [InlineData("milvasof", 1, false)]
    [InlineData("milvasoft", 2, true)]
    public void LowerThanOperator_WithTenancyIds_ShouldReturnExpected(string otherTenancyName, int otherBrancNo, bool expected)
    {
        // Arrange
        var tenantId = new TenantId("milvasoft_1");
        var otherTenantId = new TenantId(otherTenancyName, otherBrancNo);

        // Act
        var result = tenantId < otherTenantId;

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("milvasoft", 1, true)]
    [InlineData("milvasof", 1, true)]
    [InlineData("milvasoft", 2, false)]
    [InlineData("milvasoft", 3, false)]
    public void BiggerThanOperator_WithTenancyIds_ShouldReturnExpected(string otherTenancyName, int otherBrancNo, bool expected)
    {
        // Arrange
        var tenantId = new TenantId("milvasoft_2");
        var otherTenantId = new TenantId(otherTenancyName, otherBrancNo);

        // Act
        var result = tenantId > otherTenantId;

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("milvasoft", 1, false)]
    [InlineData("milvasof", 1, false)]
    [InlineData("milvasoft", 2, true)]
    [InlineData("milvasoft", 3, true)]
    public void LowerThanOrEqualOperator_WithTenancyIds_ShouldReturnExpected(string otherTenancyName, int otherBrancNo, bool expected)
    {
        // Arrange
        var tenantId = new TenantId("milvasoft_2");
        var otherTenantId = new TenantId(otherTenancyName, otherBrancNo);

        // Act
        var result = tenantId <= otherTenantId;

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("milvasoft", 1, true)]
    [InlineData("milvasof", 1, true)]
    [InlineData("milvasoft", 2, true)]
    [InlineData("milvasoft", 3, false)]
    public void BiggerThanOrEqualOperator_WithTenancyIds_ShouldReturnExpected(string otherTenancyName, int otherBrancNo, bool expected)
    {
        // Arrange
        var tenantId = new TenantId("milvasoft_2");
        var otherTenantId = new TenantId(otherTenancyName, otherBrancNo);

        // Act
        var result = tenantId >= otherTenantId;

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void IncrementOperator_WithTenancyIds_ShouldReturnIncrementCorrectly()
    {
        // Arrange
        var tenantId = new TenantId("milvasoft_2");
        var expectedBranchNo = 3;

        // Act
        tenantId++;

        // Assert
        tenantId.BranchNo.Should().Be(expectedBranchNo);
    }

    [Fact]
    public void DecrementOperator_WithTenancyIds_ShouldReturnDecrementCorrectly()
    {
        // Arrange
        var tenantId = new TenantId("milvasoft_2");
        var expectedBranchNo = 1;

        // Act
        tenantId--;

        // Assert
        tenantId.BranchNo.Should().Be(expectedBranchNo);
    }

    #endregion

    [Fact]
    public void Serialization_WithValidValue_ShouldSerializeCorrectly()
    {
        // Arrange
        var someObject = new TenantIdSerializationTestModel { Id = 1, Name = "MilvaSoft", TenantId = new TenantId("milvasoft_1") };
        var expectedJson = """"{"Id":1,"Name":"MilvaSoft","TenantId":"milvasoft_1"}"""";

        // Act
        var serializedObject = JsonSerializer.Serialize(someObject);

        // Assert
        serializedObject.Should().Be(expectedJson);
    }

    [Theory]
    [InlineData("null")]
    [InlineData(""" " " """)]
    [InlineData(""" "" """)]
    [InlineData(""" "invalid" """)]
    public void Deserialize_WithNullValue_ShouldDeserializeCorrectly(string tenantString)
    {
        // Arrange
        var jsonValue = """"{"Id":1,"Name":"MilvaSoft","TenantId":~}"""";
        jsonValue = jsonValue.Replace("~", tenantString);

        var expectedObject = new TenantIdSerializationTestModel { Id = 1, Name = "MilvaSoft", TenantId = TenantId.Empty };

        // Act
        var deserializedObject = JsonSerializer.Deserialize<TenantIdSerializationTestModel>(jsonValue);

        // Assert
        deserializedObject.TenantId.ToString().Should().Be("_0");
    }

    [Fact]
    public void Deserialize_WithInvalidValue_ShouldDeserializeCorrectly()
    {
        // Arrange
        var jsonValue = """"{"Id":1,"Name":"MilvaSoft","TenantId":"milva"}"""";
        var expectedObject = new TenantIdSerializationTestModel { Id = 1, Name = "MilvaSoft", TenantId = TenantId.Empty };

        // Act
        var deserializedObject = JsonSerializer.Deserialize<TenantIdSerializationTestModel>(jsonValue);

        // Assert
        deserializedObject.TenantId.ToString().Should().Be("_0");
    }

    [Fact]
    public void Deserialize_WithValidValue_ShouldDeserializeCorrectly()
    {
        // Arrange
        var jsonValue = """"{"Id":1,"Name":"MilvaSoft","TenantId":"milvasoft_1"}"""";
        var expectedObject = new TenantIdSerializationTestModel { Id = 1, Name = "MilvaSoft", TenantId = new TenantId("milvasoft_1") };

        // Act
        var deserializedObject = JsonSerializer.Deserialize<TenantIdSerializationTestModel>(jsonValue);

        // Assert
        deserializedObject.Should().BeEquivalentTo(expectedObject);
    }

    private class TenantIdSerializationTestModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TenantId TenantId { get; set; }
    }
}
