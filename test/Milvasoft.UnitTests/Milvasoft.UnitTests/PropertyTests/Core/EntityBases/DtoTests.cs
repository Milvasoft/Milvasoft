using FluentAssertions;
using Milvasoft.Core.EntityBases.Concrete;
using Milvasoft.UnitTests.PropertyTests.Core.EntityBases.Fixtures;

namespace Milvasoft.UnitTests.PropertyTests.Core.EntityBases;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "<Pending>")]
[Trait("Property Getter Setters Unit Tests", "Library's models property getter setter unit tests.")]
public class DtoTests
{
    [Fact]
    public void DtoBase_GetUniqueIdentifier_ShouldReturnCorrectValue()
    {
        // Arrange
        DtoBase sut = new DtoFixture
        {
            Id = 1,
            CreationDate = DateTime.Now,
            CreatorUserName = "test",
            LastModificationDate = DateTime.Now,
            LastModifierUserName = "test",
            DeletionDate = DateTime.Now,
            DeleterUserName = "test",
            IsDeleted = false,
        };

        // Act
        var result = sut.GetUniqueIdentifier();

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void GenericDtoBase_PropertiesGetterSetter_ShouldReturnCorrectValue()
    {
        // Arrange
        DtoBase<int> sut = new DtoBaseFixture
        {
            Id = 1
        };

        // Act
        var result = sut.Id;

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void GenericDtoBase_ToString_ShouldReturnCorrectValue()
    {
        // Arrange
        DtoBase<int> sut = new DtoBaseFixture
        {
            Id = 1
        };

        // Act
        var result = sut.ToString();

        // Assert
        result.Should().Be($"[{sut.GetType().Name} {sut.Id}]");
    }

    [Fact]
    public void GenericDtoBase_GetUniqueIdentifier_ShouldReturnCorrectValue()
    {
        // Arrange
        DtoBase<int> sut = new DtoBaseFixture
        {
            Id = 1
        };

        // Act
        var result = sut.GetUniqueIdentifier();

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void BaseDto_PropertiesGetterSetter_ShouldReturnCorrectValue()
    {
        // Arrange
        BaseDto<int> sut = new DtoFixture
        {
            Id = 1
        };

        // Act
        var result = sut.Id;

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void BaseDto_ToString_ShouldReturnCorrectValue()
    {
        // Arrange
        BaseDto<int> sut = new DtoFixture
        {
            Id = 1
        };

        // Act
        var result = sut.ToString();

        // Assert
        result.Should().Be($"[{sut.GetType().Name} {sut.Id}]");
    }

    [Fact]
    public void BaseDto_GetUniqueIdentifier_ShouldReturnCorrectValue()
    {
        // Arrange
        BaseDto<int> sut = new DtoFixture
        {
            Id = 1
        };

        // Act
        var result = sut.GetUniqueIdentifier();

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void BaseDto_Equals_ShouldReturnCorrectValue()
    {
        // Arrange
        BaseDto<int> sut = new DtoFixture
        {
            Id = 1
        };

        // Act
        var result = sut.Id.Equals(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void AuditDtos_PropertiesSetterGetter_ShouldReturnCorrectValue()
    {
        // Arrange & Act
        var now = DateTime.Now;
        var id = 1;
        var username = "test";
        var sut = new DtoFixture
        {
            Id = id,
            CreationDate = now,
            CreatorUserName = username,
            LastModificationDate = now,
            LastModifierUserName = username,
            DeletionDate = now,
            DeleterUserName = username,
            IsDeleted = true,
        };

        // Assert
        sut.Id.Should().Be(id);
        sut.CreationDate.Should().Be(now);
        sut.CreatorUserName.Should().Be(username);
        sut.LastModificationDate.Should().Be(now);
        sut.LastModifierUserName.Should().Be(username);
        sut.DeletionDate.Should().Be(now);
        sut.DeleterUserName.Should().Be(username);
        sut.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void WithoutUserAuditDtos_PropertiesSetterGetter_ShouldReturnCorrectValue()
    {
        // Arrange & Act
        var now = DateTime.Now;
        var id = 1;
        var sut = new DtoWithoutUserFixture
        {
            Id = id,
            CreationDate = now,
            LastModificationDate = now,
            DeletionDate = now,
            IsDeleted = true,
        };

        // Assert
        sut.Id.Should().Be(id);
        sut.CreationDate.Should().Be(now);
        sut.LastModificationDate.Should().Be(now);
        sut.DeletionDate.Should().Be(now);
        sut.IsDeleted.Should().BeTrue();
    }
}
