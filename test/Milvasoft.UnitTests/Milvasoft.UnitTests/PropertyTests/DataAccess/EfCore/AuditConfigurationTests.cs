using Milvasoft.DataAccess.EfCore.Configuration;

namespace Milvasoft.UnitTests.PropertyTests.DataAccess.EfCore;

[Trait("Property Getter Setters Unit Tests", "Library's models property getter setter unit tests.")]
public class AuditConfigurationTests
{
    [Fact]
    public void DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var config = new AuditConfiguration();

        // Assert
        Assert.True(config.AuditCreationDate);
        Assert.False(config.AuditCreator);
        Assert.True(config.AuditModificationDate);
        Assert.False(config.AuditModifier);
        Assert.True(config.AuditDeletionDate);
        Assert.False(config.AuditDeleter);
    }

    [Theory]
    [InlineData(true, true, true, true, true, true)]
    [InlineData(false, false, false, false, false, false)]
    [InlineData(true, false, true, false, true, false)]
    public void ParameterizedConstructor_ShouldSetValuesCorrectly(bool auditCreationDate,
                                                                  bool auditCreator,
                                                                  bool auditModificationDate,
                                                                  bool auditModifier,
                                                                  bool auditDeletionDate,
                                                                  bool auditDeleter)
    {
        // Act
        var config = new AuditConfiguration(auditCreationDate,
                                            auditCreator,
                                            auditModificationDate,
                                            auditModifier,
                                            auditDeletionDate,
                                            auditDeleter);

        // Assert
        Assert.Equal(auditCreationDate, config.AuditCreationDate);
        Assert.Equal(auditCreator, config.AuditCreator);
        Assert.Equal(auditModificationDate, config.AuditModificationDate);
        Assert.Equal(auditModifier, config.AuditModifier);
        Assert.Equal(auditDeletionDate, config.AuditDeletionDate);
        Assert.Equal(auditDeleter, config.AuditDeleter);
    }

    [Fact]
    public void PropertySetters_ShouldSetValuesCorrectly()
    {
        // Arrange & Act
        var config = new AuditConfiguration
        {
            AuditCreationDate = false,
            AuditCreator = true,
            AuditModificationDate = false,
            AuditModifier = true,
            AuditDeletionDate = false,
            AuditDeleter = true
        };

        // Assert
        Assert.False(config.AuditCreationDate);
        Assert.True(config.AuditCreator);
        Assert.False(config.AuditModificationDate);
        Assert.True(config.AuditModifier);
        Assert.False(config.AuditDeletionDate);
        Assert.True(config.AuditDeleter);
    }
}
