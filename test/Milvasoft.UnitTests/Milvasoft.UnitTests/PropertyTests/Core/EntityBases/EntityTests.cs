using FluentAssertions;
using Milvasoft.Core.EntityBases.Abstract;
using Milvasoft.Core.EntityBases.Concrete;
using Milvasoft.Core.MultiLanguage.EntityBases.Concrete;
using Milvasoft.Storage.Models;
using Milvasoft.Storage.S3;
using Milvasoft.UnitTests.PropertyTests.Core.EntityBases.Fixtures;
using System.Linq.Expressions;

namespace Milvasoft.UnitTests.PropertyTests.Core.EntityBases;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "<Pending>")]
[Trait("Property Getter Setters Unit Tests", "Library's models property getter setter unit tests.")]
public class EntityTests
{
    public string CreateFile<TEntity>(IS3Provider s3Provider,
                                           TEntity entity,
                                           Expression<Func<TEntity, FileInformation>> filePropertySelector,
                                           string folderName,
                                           FileType fileType) where TEntity : IMilvaEntity
    {

        var q = entity.Id;

        return "signedUrl";
    }

    [Fact]
    public void EntityBase_GetUniqueIdentifier_ShouldReturnCorrectValue()
    {
        // Arrange
        EntityBase sut = new EntityFixture
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
    public void GenericEntityBase_PropertiesGetterSetter_ShouldReturnCorrectValue()
    {
        // Arrange
        EntityBase<int> sut = new EntityBaseFixture
        {
            Id = 1
        };

        // Act
        var result = sut.Id;

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void GenericEntityBase_ToString_ShouldReturnCorrectValue()
    {
        // Arrange
        EntityBase<int> sut = new EntityBaseFixture
        {
            Id = 1
        };

        // Act
        var result = sut.ToString();

        // Assert
        result.Should().Be($"[{sut.GetType().Name} {sut.Id}]");
    }

    [Fact]
    public void GenericEntityBase_GetUniqueIdentifier_ShouldReturnCorrectValue()
    {
        // Arrange
        EntityBase<int> sut = new EntityBaseFixture
        {
            Id = 1
        };

        // Act
        var result = sut.GetUniqueIdentifier();

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void BaseEntity_PropertiesGetterSetter_ShouldReturnCorrectValue()
    {
        // Arrange
        BaseEntity<int> sut = new EntityFixture
        {
            Id = 1
        };

        // Act
        var result = sut.Id;

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void BaseEntity_ToString_ShouldReturnCorrectValue()
    {
        // Arrange
        BaseEntity<int> sut = new EntityFixture
        {
            Id = 1
        };

        // Act
        var result = sut.ToString();

        // Assert
        result.Should().Be($"[{sut.GetType().Name} {sut.Id}]");
    }

    [Fact]
    public void BaseEntity_GetUniqueIdentifier_ShouldReturnCorrectValue()
    {
        // Arrange
        BaseEntity<int> sut = new EntityFixture
        {
            Id = 1
        };

        // Act
        var result = sut.GetUniqueIdentifier();

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void BaseEntity_Equals_ShouldReturnCorrectValue()
    {
        // Arrange
        BaseEntity<int> sut = new EntityFixture
        {
            Id = 1
        };

        // Act
        var result = sut.Id.Equals(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void AuditEntities_PropertiesGetterSetter_ShouldReturnCorrectValue()
    {
        // Arrange & Act
        var now = DateTime.Now;
        var id = 1;
        var username = "test";
        var sut = new EntityFixture
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
    public void WithoutUserAuditEntities_PropertiesGetterSetter_ShouldReturnCorrectValue()
    {
        // Arrange & Act
        var now = DateTime.Now;
        var id = 1;
        var sut = new EntityWithoutUserFixture
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

    [Fact]
    public void LanguageEntity_PropertiesGetterSetter_ShouldReturnCorrectValue()
    {
        // Arrange
        var code = "en-US";
        var name = "English";

        // Act
        LanguageEntity sut = new LanguageEntityFixture
        {
            Id = 1,
            Code = code,
            Name = name,
            IsDefault = true,
            Supported = true
        };

        // Assert
        sut.Id.Should().Be(1);
        sut.Code.Should().Be(code);
        sut.Name.Should().Be(name);
        sut.IsDefault.Should().BeTrue();
        sut.Supported.Should().BeTrue();
    }

    [Fact]
    public void LanguageEntity_ToString_ShouldReturnCorrectValue()
    {
        // Arrange
        LanguageEntity sut = new LanguageEntityFixture
        {
            Id = 1
        };

        // Act
        var result = sut.ToString();

        // Assert
        result.Should().Be($"[{sut.GetType().Name} {sut.Id}]");
    }

    [Fact]
    public void TranslationEntity_ToString_ShouldReturnCorrectValue()
    {
        // Arrange
        TranslationEntity<HasTranslationEntityFixture> sut = new TranslationEntityFixture
        {
            Id = 1,
        };

        // Act
        var result = sut.ToString();

        // Assert
        result.Should().Be($"[{sut.GetType().Name} {sut.Id}]");
    }

    [Fact]
    public void TranslationEntity_PropertiesGetterSetter_ShouldReturnCorrectValue()
    {
        // Arrange & Act
        TranslationEntity<HasTranslationEntityFixture> sut = new TranslationEntityFixture
        {
            Id = 1,
            Entity = new HasTranslationEntityFixture
            {
                Id = 2
            }
        };

        // Assert
        sut.Id.Should().Be(1);
        sut.Entity.Id.Should().Be(2);
    }

    [Fact]
    public void HasTranslationEntity_ToString_ShouldReturnCorrectValue()
    {
        // Arrange
        HasTranslationEntity<TranslationEntityFixture> sut = new HasTranslationEntityFixture
        {
            Id = 1,
        };

        // Act
        var result = sut.ToString();

        // Assert
        result.Should().Be($"[{sut.GetType().Name} {sut.Id}]");
    }
}
