using FluentAssertions;
using Milvasoft.Core.EntityBases.MultiTenancy;
using Milvasoft.UnitTests.PropertyTests.Core.EntityBases.Fixtures;

namespace Milvasoft.UnitTests.PropertyTests.Core.EntityBases;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "<Pending>")]
[Trait("Property Getter Setters Unit Tests", "Library's models property getter setter unit tests.")]
public class TenantTests
{
    [Fact]
    public void MilvaTenant_GetUniqueIdentifier_ShouldReturnCorrectValue()
    {
        // Arrange
        var tenancyName = "milvasoft";
        var id = new TenantId(tenancyName, 1);
        MilvaTenant sut = new MilvaTenantFixture(tenancyName, 1);

        // Act
        var result = sut.GetUniqueIdentifier();

        // Assert
        result.Should().Be(id);
    }

    [Fact]
    public void MilvaTenant_PropertiesGetterSetter_ShouldReturnCorrectValue()
    {
        // Arrange & Act
        var tenancyName = "milvasoft";
        var id = new TenantId(tenancyName, 1);
        var now = DateTime.Now;
        var username = "test";
        var someString = "someString";
        MilvaTenant sut = new MilvaTenantFixture(tenancyName, 1)
        {
            CreationDate = now,
            CreatorUserName = username,
            LastModificationDate = now,
            LastModifierUserName = username,
            DeletionDate = now,
            DeleterUserName = username,
            IsDeleted = true,
            IsActive = true,
            ConnectionString = someString,
            Name = someString,
            SubscriptionExpireDate = now,
        };

        // Assert
        sut.Id.Should().Be(id);
        sut.BranchNo.Should().Be(1);
        sut.CreationDate.Should().Be(now);
        sut.CreatorUserName.Should().Be(username);
        sut.LastModificationDate.Should().Be(now);
        sut.LastModifierUserName.Should().Be(username);
        sut.DeletionDate.Should().Be(now);
        sut.DeleterUserName.Should().Be(username);
        sut.IsDeleted.Should().BeTrue();
        sut.IsActive.Should().BeTrue();
        sut.ConnectionString.Should().Be(someString);
        sut.SubscriptionExpireDate.Should().Be(now);
        sut.TenancyName.Should().Be(tenancyName);
        var sutAsMilvaBaseTenant = sut as MilvaBaseTenant<TenantId>;
        sutAsMilvaBaseTenant.TenancyName.Should().Be(tenancyName);
    }

    [Fact]
    public void MilvaTenant_Constructor_ShouldReturnCorrectValue()
    {
        // Arrange & Act
        MilvaTenant sut = new MilvaTenantFixture();

        // Assert
        sut.Id.BranchNo.Should().Be(1);
        sut.BranchNo.Should().Be(1);
        sut.TenancyName.Should().NotBeNull();
        sut.IsActive.Should().BeTrue();
    }
}
