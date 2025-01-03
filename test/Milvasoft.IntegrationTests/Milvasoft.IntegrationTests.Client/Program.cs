using Microsoft.EntityFrameworkCore;
using Milvasoft.DataAccess.EfCore;
using Milvasoft.IntegrationTests.Client.Fixtures.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;

var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionString");

services.ConfigureMilvaDataAccess(opt =>
{

});

services.AddDbContext<MilvaBulkDbContextFixture>(options =>
{
    options.UseNpgsql(connectionString);
});

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