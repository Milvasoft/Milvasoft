using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Milvasoft.DataAccess.EfCore.Configuration;
using Milvasoft.IntegrationTests.Fixtures;
using Milvasoft.Middlewares.ResponseTimeCalculator;
using Xunit.Abstractions;

namespace Milvasoft.IntegrationTests;

[Collection(nameof(DatabaseTestCollection))]
public class DbContextTests(ITestOutputHelper testOutputHelper) : IAsyncLifetime
{
    private readonly SomeMilvaDbContextFixture _someMilvaDbContext;
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;
    private WebApplicationFactory<Program> _waf;
    private Func<Task> _resetDatabase;

    private readonly Faker<SomeFullAuditableEntityFixture> _entityGenerator = new Faker<SomeFullAuditableEntityFixture>()
            .RuleFor(x => x.SomeStringProp, f => f.Person.FullName)
            .RuleFor(x => x.SomeDecimalProp, f => f.Finance.Amount())
            .RuleFor(x => x.SomeDateProp, f => f.Person.DateOfBirth.Date)
            .UseSeed(1000);

    [Fact]
    public async Task Create_ShouldCreateCustomer_WhenDetailsAreValid()
    {
        // Arrange
        await _resetDatabase();
        var entity = _entityGenerator.Generate();

        _ = _someMilvaDbContext.FullAuditableEntities.AddRangeAsync(entity);
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

    public async Task DisposeAsync() => await _resetDatabase();
}
