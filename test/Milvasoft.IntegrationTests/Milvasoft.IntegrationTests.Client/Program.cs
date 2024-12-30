using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Milvasoft.DataAccess.EfCore;
using Milvasoft.IntegrationTests.Client.Fixtures;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionString");

services.RemoveAll<DbContextOptions<MilvaBulkDbContextFixture>>();
services.RemoveAll<MilvaBulkDbContextFixture>();

services.ConfigureMilvaDataAccess(opt =>
{

});

services.AddDbContext<MilvaBulkDbContextFixture>(options =>
{
    options.UseNpgsql(connectionString);
});

// Add services to the container.

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

    if ((await dbContext.Database.GetPendingMigrationsAsync()).Any())
        await dbContext.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.

await app.RunAsync();

public partial class Program;