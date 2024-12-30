using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Milvasoft.Components.Rest.Request;
using Milvasoft.DataAccess.EfCore.Configuration;
using Milvasoft.DataAccess.EfCore.Utils;
using Milvasoft.IntegrationTests.Client.Fixtures;
using Milvasoft.Middlewares.ResponseTimeCalculator;
using System.Globalization;

namespace Milvasoft.IntegrationTests.DataAccessTests.EfCoreTests;

[Collection(nameof(DatabaseTestCollection))]
public class DbContextTests(CustomWebApplicationFactory factory) : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory = factory;
    private IServiceProvider _serviceProvider;

    [Theory]
    [InlineData("IOS")]
    [InlineData("ios")]
    [InlineData("ıos")]
    public async Task Create_ShouldCreateCustomer_WhenDetailsAreValid(string query)
    {
        // Arrange
        var entity = new SomeFullAuditableEntityFixture
        {
            SomeStringProp = "IOS",
            SomeDecimalProp = 123.45m,
            SomeDateProp = DateTime.Now
        };
        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        dbContext.FullAuditableEntities.Add(entity);
        await dbContext.SaveChangesAsync();

        var cu = CultureInfo.CurrentCulture;
        var cuı = CultureInfo.CurrentUICulture;

        CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
        CultureInfo.CurrentUICulture = new CultureInfo("tr-TR");

        // Act
        var createdEntity = await dbContext.FullAuditableEntities.WithFiltering(new FilterRequest
        {
            Criterias = [
                new FilterCriteria
                {
                    FilterBy = nameof(SomeFullAuditableEntityFixture),
                    Value = query,
                    Type = Components.Rest.Enums.FilterType.EqualTo
                }
            ]
        }).ToListAsync();

        // Assert
        createdEntity.FirstOrDefault().Should().NotBeNull();
    }

    public async Task InitializeAsync()
    {
        // Test ortamı için özel yapılandırma
        var waf = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<MilvaBulkDbContextFixture>>();
                services.RemoveAll<MilvaBulkDbContextFixture>();

                var dataAccessConfiguration = new DataAccessConfiguration
                {
                    DbContext = new DbContextConfiguration
                    {
                        GetCurrentUserNameMethod = _ => "testuser",
                        UseUtcForDateTime = true
                    },
                    Auditing = new AuditConfiguration
                    {
                        AuditModificationDate = false,
                        AuditModifier = true
                    }
                };

                services.AddSingleton<IDataAccessConfiguration>(dataAccessConfiguration);

                services.AddDbContext<MilvaBulkDbContextFixture>(options =>
                {
                    options.UseNpgsql(_factory.GetConnectionString());
                });
            });

            builder.Configure(app =>
            {
                app.UseMiddleware<MilvaResponseTimeCalculator>();
            });

        });

        _serviceProvider = waf.Services.CreateScope().ServiceProvider;

        await _factory.CreateRespawner();
    }

    public async Task DisposeAsync() => await _factory.ResetDatabase();
}