using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Milvasoft.DataAccess.EfCore.Configuration;
using Milvasoft.IntegrationTests;
using Milvasoft.Middlewares.ResponseTimeCalculator;
using Milvasoft.UnitTests.DataAccessTests.EfCoreTests.Fixtures;
using Xunit.Abstractions;

namespace Customers.Api.Tests.Integration;

[Collection(nameof(DatabaseTestCollection))]
public class DbContextTests : IAsyncLifetime
{
    private SomeMilvaDbContextFixture _someMilvaDbContext;
    private readonly ITestOutputHelper _testOutputHelper;
    private WebApplicationFactory<Program> _waf;
    private Func<Task> _resetDatabase;

    private readonly Faker<SomeFullAuditableEntityFixture> _entityGenerator = new Faker<SomeFullAuditableEntityFixture>()
            .RuleFor(x => x.SomeStringProp, f => f.Person.FullName)
            .RuleFor(x => x.SomeDecimalProp, f => f.Finance.Amount())
            .RuleFor(x => x.SomeDateProp, f => f.Person.DateOfBirth.Date)
            .UseSeed(1000);

    public DbContextTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task Create_ShouldCreateCustomer_WhenDetailsAreValid()
    {
        // Arrange
        await _resetDatabase();
        var entity = _entityGenerator.Generate();

        var request = _someMilvaDbContext.FullAuditableEntities.AddRangeAsync(entity);
        await _someMilvaDbContext.SaveChangesAsync();

        // Act
        var count = _someMilvaDbContext.FullAuditableEntities.CountAsync();

        // Assert
        count.Should().Be(1);
    }

    public async Task InitializeAsync()
    {
        var customWaf = new CustomWebApplicationFactory();

        var waf = customWaf.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<SomeMilvaDbContextFixture>>();
                services.RemoveAll<SomeMilvaDbContextFixture>();

                var dataAccessConfiguration = new DataAccessConfiguration
                {
                    DbContext = new DbContextConfiguration
                    {
                        GetCurrentUserNameMethod = (sp) => "testuser"
                    },
                    Auditing = new AuditConfiguration
                    {
                        AuditModificationDate = false,
                        AuditModifier = true
                    }
                };

                services.AddSingleton<IDataAccessConfiguration>(dataAccessConfiguration);

                services.AddDbContext<SomeMilvaDbContextFixture>(x => x.UseNpgsql(customWaf.GetConnectionString()));
            });

        });

        _waf = waf;
        _resetDatabase = customWaf.ResetDatabase;
        customWaf.SetAppBuilderAction(app =>
        {
            app.UseMiddleware<MilvaResponseTimeCalculator>();
            return app;
        });
    }

    public async Task DisposeAsync()
    {
        await _resetDatabase();
    }
}
