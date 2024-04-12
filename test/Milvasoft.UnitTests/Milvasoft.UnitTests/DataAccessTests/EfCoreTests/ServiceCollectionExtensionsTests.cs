using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.DataAccess.EfCore;
using Milvasoft.DataAccess.EfCore.Configuration;

namespace Milvasoft.UnitTests.DataAccessTests.EfCoreTests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void ConfigureMilvaDataAccess_WithNullAction_ShouldAddDefault()
    {
        // Arrange
        var services = new ServiceCollection();
        services.ConfigureMilvaDataAccess(dataAccessConfiguration: null);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var configuration = serviceProvider.GetService<IDataAccessConfiguration>();

        // Assert
        configuration.Auditing.AuditDeletionDate.Should().BeTrue();
        configuration.DbContext.GetCurrentUserNameMethod.Should().BeNull();
    }

    [Fact]
    public void ConfigureMilvaDataAccess_WithAction_ShouldAddCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.ConfigureMilvaDataAccess(opt =>
        {
            opt.DbContext.GetCurrentUserNameMethod = (sp) => "test";
        });
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var configuration = serviceProvider.GetService<IDataAccessConfiguration>();

        // Assert
        configuration.Auditing.AuditDeletionDate.Should().BeTrue();
        configuration.DbContext.GetCurrentUserNameMethod.Invoke(serviceProvider).Should().Be("test");
    }
}
