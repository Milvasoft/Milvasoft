using EFCore.BulkExtensions;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Components.Rest.Request;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.DataAccess.EfCore;
using Milvasoft.DataAccess.EfCore.Configuration;
using Milvasoft.DataAccess.EfCore.Utils;
using Milvasoft.DataAccess.EfCore.Utils.Enums;
using Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures;
using Milvasoft.IntegrationTests.Client.Fixtures.EnumFixtures;
using Milvasoft.IntegrationTests.Client.Fixtures.Persistence;
using Milvasoft.IntegrationTests.DataAccessTests.Fixtures;
using System.Linq.Expressions;
using System.Net;

namespace Milvasoft.IntegrationTests.DataAccessTests.EfCoreTests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1042:The member referenced by the MemberData attribute returns untyped data rows", Justification = "<Pending>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "<Pending>")]
[Collection(nameof(UtcTrueDatabaseTestCollection))]
[Trait("MilvaEfExtensions Integration Tests", "Integration tests for Milvasoft.DataAccess.EfCore integration tests.")]
public class MilvaEfExtensionsTests(CustomWebApplicationFactory factory) : DataAccessIntegrationTestBase(factory)
{
    public override Task InitializeAsync(Action<IServiceCollection> configureServices = null, Action<IApplicationBuilder> configureApp = null)
    {
        var waf = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IDataAccessConfiguration>();
                services.RemoveAll<DbContextOptions<MilvaBulkDbContextFixture>>();
                services.RemoveAll<MilvaBulkDbContextFixture>();

                services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                {
                    x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                    x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
                });

                configureServices?.Invoke(services);
            });

            builder.Configure(app =>
            {
                configureApp?.Invoke(app);
            });
        });

        _serviceProvider = waf.Services.CreateScope().ServiceProvider;

        return _factory.CreateRespawner();
    }

    #region WithFiltering

    [Fact]
    public async Task WithFiltering_ForEfSource_WithEmptyQueryable_ShouldReturnEmpty()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = SoftDeletionState.Active,
                    GetCurrentUserNameMethod = (sp) => "testuser"
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        var filterRequest = new FilterRequest
        {
            Criterias =
            [
                new FilterCriteria
                {
                    FilterBy = nameof(RestTestEntityFixture.Name),
                    Type = FilterType.IsNull
                }
            ]
        };

        // Act
        var result = await dbContext.RestTestEntities.WithFiltering(filterRequest).ToListAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(ValidListSourceForBuildFilterExpressionMethodData))]
    public async Task WithFiltering_ForEfSource_WithValidFilterRequest_ShouldReturnCorrectResult(List<RestTestEntityFixture> source, FilterRequest filterRequest, List<int> expectedIdList)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = SoftDeletionState.Active,
                    GetCurrentUserNameMethod = (sp) => "testuser"
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();
        await dbContext.RestTestEntities.AddRangeAsync(source);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await dbContext.RestTestEntities.WithFiltering(filterRequest).ToListAsync();

        // Assert
        result.Should().HaveCount(expectedIdList.Count);
        result.Should().AllSatisfy(i => expectedIdList.Should().Contain(i.Id));
    }

    [Fact]
    public async Task WithFiltering_ForListRequestOverload_ForEfSource_WithNullListRequest_ShouldReturnInputQuery()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = SoftDeletionState.Active,
                    GetCurrentUserNameMethod = (sp) => "testuser"
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();
        await dbContext.RestTestEntities.AddAsync(new RestTestEntityFixture { Id = 1, Name = "john" });
        await dbContext.SaveChangesAsync();
        ListRequest listRequest = null;

        // Act
        var result = await dbContext.RestTestEntities.WithFiltering(listRequest).ToListAsync();

        // Assert
        result.Should().HaveCount(await dbContext.RestTestEntities.CountAsync());
    }

    [Theory]
    [ClassData(typeof(ValidListSourceForBuildFilterExpressionMethodData))]
    public async Task WithFiltering_ForListRequestOverload_ForEfSource_WithValidListRequest_ShouldReturnCorrectResult(List<RestTestEntityFixture> source, FilterRequest filterRequest, List<int> expectedIdList)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = SoftDeletionState.Active,
                    GetCurrentUserNameMethod = (sp) => "testuser"
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();
        await dbContext.RestTestEntities.AddRangeAsync(source);
        await dbContext.SaveChangesAsync();
        var listRequest = new ListRequest
        {
            Filtering = filterRequest
        };

        // Act
        var result = await dbContext.RestTestEntities.WithFiltering(listRequest).ToListAsync();

        // Assert
        result.Should().HaveCount(expectedIdList.Count);
        result.Should().AllSatisfy(i => expectedIdList.Should().Contain(i.Id));
    }

    #endregion

    #region WithSorting

    [Fact]
    public async Task WithSorting_ForEfSource_WithEmptyQueryable_ShouldReturnEmpty()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = SoftDeletionState.Active,
                    GetCurrentUserNameMethod = (sp) => "testuser"
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();
        var sortRequest = new SortRequest { SortBy = nameof(RestTestEntityFixture.Name), Type = SortType.Asc };

        // Act
        var result = await dbContext.RestTestEntities.WithSorting(sortRequest).ToListAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(ValidAscendingSortRequestForBuildPropertySelectorExpressionMethodData))]
    public async Task WithSorting_ForEfSource_WithAscendingSortRequest_ShouldReturnCorrectResult(List<RestTestEntityFixture> source, SortRequest sortRequest, Expression<Func<RestTestEntityFixture, object>> expectedExpression)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = SoftDeletionState.Active,
                    GetCurrentUserNameMethod = (sp) => "testuser"
                };
            });
        });
        // null first
        var expectedSorted = source
            .OrderBy(x => expectedExpression.Compile()(x) == null ? 0 : 1)
            .ThenBy(expectedExpression.Compile())
            .ToList();

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();
        await dbContext.RestTestEntities.AddRangeAsync(source);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await dbContext.RestTestEntities.WithSorting(sortRequest).ToListAsync();

        // Assert
        result.Should().BeEquivalentTo(expectedSorted, options => options.WithStrictOrderingFor(expectedExpression).Using<DateTime>(ctx =>
        {
            ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(2));
        }).WhenTypeIs<DateTime>());

    }

    [Theory]
    [ClassData(typeof(ValidDescendingSortRequestForBuildPropertySelectorExpressionMethodData))]
    public async Task WithSorting_ForEfSource_WithDescendingSortRequest_ShouldReturnCorrectResult(List<RestTestEntityFixture> source, SortRequest sortRequest, Expression<Func<RestTestEntityFixture, object>> expectedExpression)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = SoftDeletionState.Active,
                    GetCurrentUserNameMethod = (sp) => "testuser"
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();
        await dbContext.RestTestEntities.AddRangeAsync(source);
        await dbContext.SaveChangesAsync();

        // null first
        var expectedSorted = source
            .OrderByDescending(x => expectedExpression.Compile()(x) == null ? 0 : 1)
            .ThenByDescending(expectedExpression.Compile())
            .ToList();

        // Act
        var result = await dbContext.RestTestEntities.WithSorting(sortRequest).ToListAsync();

        // Assert
        result.Should().BeEquivalentTo(expectedSorted, options => options.WithStrictOrderingFor(expectedExpression).Using<DateTime>(ctx =>
        {
            ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(2));
        }).WhenTypeIs<DateTime>());
    }

    [Fact]
    public async Task WithSorting_ForListRequestOverload_ForEfSource_WithNullListRequest_ShouldReturnInputQuery()
    {
        // Arrange
        ListRequest listRequest = null;

        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = SoftDeletionState.Active,
                    GetCurrentUserNameMethod = (sp) => "testuser"
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();
        await dbContext.RestTestEntities.AddAsync(new RestTestEntityFixture { Id = 1, Name = "john" });
        await dbContext.SaveChangesAsync();

        // Act
        var result = await dbContext.RestTestEntities.WithSorting(listRequest).ToListAsync();

        // Assert
        result.Should().HaveCount(await dbContext.RestTestEntities.CountAsync());
    }

    [Theory]
    [ClassData(typeof(ValidAscendingSortRequestForBuildPropertySelectorExpressionMethodData))]
    public async Task WithSorting_ForListRequestOverload_ForEfSource_WithAscendingListRequest_ShouldReturnCorrectResult(List<RestTestEntityFixture> source, SortRequest sortRequest, Expression<Func<RestTestEntityFixture, object>> expectedExpression)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = SoftDeletionState.Active,
                    GetCurrentUserNameMethod = (sp) => "testuser"
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();
        await dbContext.RestTestEntities.AddRangeAsync(source);
        await dbContext.SaveChangesAsync();

        var listRequest = new ListRequest
        {
            Sorting = sortRequest
        };

        // null first
        var expectedSorted = source
            .OrderBy(x => expectedExpression.Compile()(x) == null ? 0 : 1)
            .ThenBy(expectedExpression.Compile())
            .ToList();

        // Act
        var result = await dbContext.RestTestEntities.WithSorting(listRequest).ToListAsync();

        // Assert
        result.Should().BeEquivalentTo(expectedSorted, options => options.WithStrictOrderingFor(expectedExpression).Using<DateTime>(ctx =>
        {
            ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(2));
        }).WhenTypeIs<DateTime>());
    }

    [Theory]
    [ClassData(typeof(ValidDescendingSortRequestForBuildPropertySelectorExpressionMethodData))]
    public async Task WithSorting_ForListRequestOverload_ForEfSource_WithDescendingListRequest_ShouldReturnCorrectResult(List<RestTestEntityFixture> source, SortRequest sortRequest, Expression<Func<RestTestEntityFixture, object>> expectedExpression)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = SoftDeletionState.Active,
                    GetCurrentUserNameMethod = (sp) => "testuser"
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();
        await dbContext.RestTestEntities.AddRangeAsync(source);
        await dbContext.SaveChangesAsync();
        // null first
        var expectedSorted = source
            .OrderByDescending(x => expectedExpression.Compile()(x) == null ? 0 : 1)
            .ThenByDescending(expectedExpression.Compile())
            .ToList();

        var listRequest = new ListRequest
        {
            Sorting = sortRequest
        };

        // Act
        var result = await dbContext.RestTestEntities.WithSorting(listRequest).ToListAsync();

        // Assert
        result.Should().BeEquivalentTo(expectedSorted, options => options.WithStrictOrderingFor(expectedExpression).Using<DateTime>(ctx =>
        {
            ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(2));
        }).WhenTypeIs<DateTime>());
    }

    #endregion

    #region WithFilteringAndSorting

    [Fact]
    public async Task WithFilteringAndSorting_ForEfSource_WithNullListRequest_ShouldReturnInputQuery()
    {
        // Arrange
        List<RestTestEntityFixture> list =
        [
            new() {
                Id = 1,
                Name = "John",
                Count = 1,
                Price = 10M,
                Number = RestTestEnumFixture.Two,
                IsActive = true,
                InsertDate = DateTime.Now,
                UpdateDate = DateTime.Now.AddDays(1),
            },
            new() {
                Id = 2,
                Name = "Elise",
                Count = 10,
                Price = 15.5M,
                Number = RestTestEnumFixture.One,
                IsActive = false,
                InsertDate = DateTime.Now.AddDays(4),
                UpdateDate = DateTime.Now.AddDays(3),
            },
            new() {
                Id = 3,
                Name = "Jack",
                Count = 90,
                Price = 0.50M,
                Number = RestTestEnumFixture.Zero,
                IsActive = false,
                InsertDate = DateTime.Now,
                UpdateDate = null
            },
            new() {
                Id = 4,
                Name = "Mary",
                Count = 101,
                Price = 4M,
                Number = RestTestEnumFixture.One,
                IsActive = true,
                InsertDate = DateTime.Now,
                UpdateDate = null
            },
            new() {
                Id = 5,
                Name = null,
                Count = 999,
                Price = 999M,
                Number = RestTestEnumFixture.Zero,
                IsActive = null,
                InsertDate = DateTime.Now.AddMonths(-5),
                UpdateDate = null
            },
        ];

        ListRequest listRequest = null;

        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = SoftDeletionState.Active,
                    GetCurrentUserNameMethod = (sp) => "testuser"
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();
        await dbContext.RestTestEntities.AddRangeAsync(list);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await dbContext.RestTestEntities.WithSorting(listRequest).ToListAsync();

        // Assert
        result.Should().HaveCount(await dbContext.RestTestEntities.CountAsync());
    }

    [Fact]
    public async Task WithFilteringAndSorting_ForEfSource_WithFilteringAndSortingRequested_ShouldReturnCorrectResult()
    {
        // Arrange
        List<RestTestEntityFixture> list =
        [
            new() {
                Id = 1,
                Name = "John",
                Count = 1,
                Price = 10M,
                Number = RestTestEnumFixture.Two,
                IsActive = true,
                InsertDate = DateTime.Now,
                UpdateDate = DateTime.Now.AddDays(1),
            },
            new() {
                Id = 2,
                Name = "Elise",
                Count = 10,
                Price = 15.5M,
                Number = RestTestEnumFixture.One,
                IsActive = false,
                InsertDate = DateTime.Now.AddDays(4),
                UpdateDate = DateTime.Now.AddDays(3),
            },
            new() {
                Id = 3,
                Name = "Jack",
                Count = 90,
                Price = 0.50M,
                Number = RestTestEnumFixture.Zero,
                IsActive = false,
                InsertDate = DateTime.Now,
                UpdateDate = null
            },
            new() {
                Id = 4,
                Name = "Mary",
                Count = 101,
                Price = 4M,
                Number = RestTestEnumFixture.One,
                IsActive = true,
                InsertDate = DateTime.Now,
                UpdateDate = null
            },
            new() {
                Id = 5,
                Name = null,
                Count = 999,
                Price = 999M,
                Number = RestTestEnumFixture.Zero,
                IsActive = null,
                InsertDate = DateTime.Now.AddMonths(-5),
                UpdateDate = null
            },
        ];

        ListRequest listRequest = new()
        {
            Filtering = new FilterRequest
            {
                Criterias =
                [
                    new FilterCriteria
                    {
                        FilterBy = nameof(RestTestEntityFixture.Name),
                        Type = FilterType.IsNotNull
                    }
                ]
            },
            Sorting = new SortRequest { SortBy = nameof(RestTestEntityFixture.Id), Type = SortType.Desc }
        };

        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = SoftDeletionState.Active,
                    GetCurrentUserNameMethod = (sp) => "testuser"
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();
        await dbContext.RestTestEntities.AddRangeAsync(list);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await dbContext.RestTestEntities.WithFilteringAndSorting(listRequest).ToListAsync();

        // Assert
        result.Should().HaveCount(4);
        result.Should().BeInDescendingOrder(i => i.Id);
    }

    #endregion

    #region ToListResponseAsync

    /// <summary>
    /// source, list request, expected list response
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> ValidToListResponseMethodData()
    {
        List<RestTestEntityFixture> validList =
        [
            new() {
                Id = 1,
                Name = "John",
                Count = 1,
                Price = 10M,
                Number = RestTestEnumFixture.Two,
                IsActive = true,
                InsertDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow.AddDays(1)
            },
            new() {
                Id = 2,
                Name = "Elise",
                Count = 10,
                Price = 15.5M,
                Number = RestTestEnumFixture.One,
                IsActive = false,
                InsertDate = DateTime.UtcNow.AddDays(4),
                UpdateDate = DateTime.UtcNow.AddDays(3)
            },
            new() {
                Id = 3,
                Name = "Jack",
                Count = 90,
                Price = 0.50M,
                Number = RestTestEnumFixture.Zero,
                IsActive = false,
                InsertDate = DateTime.UtcNow,
                UpdateDate = null
            },
            new() {
                Id = 4,
                Name = "Mary",
                Count = 101,
                Price = 4M,
                Number = RestTestEnumFixture.One,
                IsActive = true,
                InsertDate = DateTime.UtcNow,
                UpdateDate = null
            },
            new() {
                Id = 5,
                Name = null,
                Count = 999,
                Price = 999M,
                Number = RestTestEnumFixture.Zero,
                IsActive = null,
                InsertDate = DateTime.UtcNow.AddMonths(-5),
                UpdateDate = null
            },
        ];

        //1 - list request is null
        var response1 = ListResponse<RestTestEntityFixture>.Success([.. validList], LocalizerKeys.Successful, null, null, null);
        yield return new object[]
        {
            validList,
            null,
            response1
        };

        //2 - page number exists but row count not exists
        var response2 = ListResponse<RestTestEntityFixture>.Success([.. validList], LocalizerKeys.Successful, 1, null, null);
        yield return new object[]
        {
            validList,
            new ListRequest
            {
               PageNumber = 1,
               RowCount = null
            },
            response2
        };

        //3 - row count exists but page count not exists
        var response3 = ListResponse<RestTestEntityFixture>.Success([.. validList], LocalizerKeys.Successful, null, null, null);
        yield return new object[]
        {
            validList,
            new ListRequest
            {
               PageNumber = null,
               RowCount = 2
            },
            response3
        };

        //4 - valid page number and row count
        var response4 = ListResponse<RestTestEntityFixture>.Success([.. validList.Skip(0).Take(2)], LocalizerKeys.Successful, 1, 3, 5);
        yield return new object[]
        {
            validList,
            new ListRequest
            {
               PageNumber = 1,
               RowCount = 2
            },
            response4
        };

        //5 - aggregation, filtering, sorting requested
        var response5Data = validList.ToList();
        response5Data.RemoveAll(i => i.Name == null);
        response5Data = [.. response5Data.OrderByDescending(i => i.Id)];
        var response5 = ListResponse<RestTestEntityFixture>.Success([.. response5Data], LocalizerKeys.Successful, null, null, null);
        response5.AggregationResults =
        [
            new AggregationResult { AggregatedBy = nameof(RestTestEntityFixture.Price), Type = AggregationType.Sum, Result = 30M },
        ];
        yield return new object[]
        {
            validList,
            new ListRequest
            {
               Aggregation = new AggregationRequest{ Criterias = [ new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Price), Type = AggregationType.Sum}] },
               Filtering =  new FilterRequest { Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.Name), Type = FilterType.IsNotNull }]},
               Sorting = new SortRequest { SortBy = nameof(RestTestEntityFixture.Id), Type = SortType.Desc }
            },
            response5
        };

        //6 - aggregation, filtering, sorting and pagination requested
        var response6Data = validList.ToList();
        response6Data.RemoveAll(i => i.Name == null);
        response6Data = [.. response6Data.OrderByDescending(i => i.Id).Skip(0).Take(2)];
        var response6 = ListResponse<RestTestEntityFixture>.Success([.. response6Data], LocalizerKeys.Successful, 1, 2, 4);
        response6.AggregationResults =
        [
            new AggregationResult { AggregatedBy = nameof(RestTestEntityFixture.Price), Type = AggregationType.Sum, Result = 30M },
        ];
        yield return new object[]
        {
            validList,
            new ListRequest
            {
               PageNumber = 1,
               RowCount = 2,
               Aggregation = new AggregationRequest{ Criterias = [ new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Price), Type = AggregationType.Sum}] },
               Filtering =  new FilterRequest { Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.Name), Type = FilterType.IsNotNull }]},
               Sorting = new SortRequest { SortBy = nameof(RestTestEntityFixture.Id), Type = SortType.Desc }
            },
            response6
        };
    }

    [Fact]
    public async Task ToListResponseAsync_WithNullQuerySource_ShouldReturnSuccessResponse()
    {
        // Arrange
        var listRequest = new ListRequest
        {
            PageNumber = 1,
            RowCount = 2,
            Aggregation = new AggregationRequest { Criterias = [new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Price), Type = AggregationType.Sum }] },
            Filtering = new FilterRequest { Criterias = [new FilterCriteria { FilterBy = nameof(RestTestEntityFixture.Name), Type = FilterType.IsNotNull }] },
            Sorting = new SortRequest { SortBy = nameof(RestTestEntityFixture.Id), Type = SortType.Desc }
        };

        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = SoftDeletionState.Active,
                    GetCurrentUserNameMethod = (sp) => "testuser"
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();
        dbContext.RestTestEntities = null;
        await dbContext.SaveChangesAsync();

        // Act
        var result = await dbContext.RestTestEntities.ToListResponseAsync(listRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Messages.Should().HaveCount(1);
        result.Messages[0].Message.Should().Be(LocalizerKeys.Successful);
    }

    [Theory]
    [MemberData(nameof(ValidToListResponseMethodData))]
    public async Task ToListResponseAsync_WithFilteringAndSortingRequested_ShouldReturnCorrectResult(List<RestTestEntityFixture> source, ListRequest listRequest, ListResponse<RestTestEntityFixture> expectedResponse)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = SoftDeletionState.Active,
                    GetCurrentUserNameMethod = (sp) => "testuser"
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();
        await dbContext.RestTestEntities.AddRangeAsync(source);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await dbContext.RestTestEntities.ToListResponseAsync(listRequest);

        // Assert
        result.Should().BeEquivalentTo(expectedResponse, opt => opt.RespectingRuntimeTypes().Using<DateTime>(ctx =>
        {
            ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(2));
        }).WhenTypeIs<DateTime>());
    }

    #endregion

    #region ToListResponse

    [Fact]
    public async Task ToListResponse_ForEfSource_WithNullQuerySource_ShouldReturnSuccessResponse()
    {
        // Arrange
        var listRequest = new ListRequest
        {
            PageNumber = 1,
            RowCount = 2,
            Aggregation = new AggregationRequest { Criterias = [new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Price), Type = AggregationType.Sum }] },
            Filtering = new FilterRequest { Criterias = [new FilterCriteria { FilterBy = nameof(RestTestEntityFixture.Name), Type = FilterType.IsNotNull }] },
            Sorting = new SortRequest { SortBy = nameof(RestTestEntityFixture.Id), Type = SortType.Desc }
        };

        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = SoftDeletionState.Active,
                    GetCurrentUserNameMethod = (sp) => "testuser"
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();
        dbContext.RestTestEntities = null;
        await dbContext.SaveChangesAsync();

        // Act
        var result = dbContext.RestTestEntities.ToListResponse(listRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Messages.Should().HaveCount(1);
        result.Messages[0].Message.Should().Be(LocalizerKeys.Successful);
    }

    [Theory]
    [MemberData(nameof(ValidToListResponseMethodData))]
    public async Task ToListResponse_ForEfSourceWithFilteringAndSortingRequested_ShouldReturnCorrectResult(List<RestTestEntityFixture> source, ListRequest listRequest, ListResponse<RestTestEntityFixture> expectedResponse)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = SoftDeletionState.Active,
                    GetCurrentUserNameMethod = (sp) => "testuser"
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        await dbContext.BulkInsertAsync(source);

        // Act
        var result = await dbContext.RestTestEntities.ToListResponseAsync(listRequest);

        // Assert
        result.Should().BeEquivalentTo(expectedResponse, opt => opt.RespectingRuntimeTypes().Using<DateTime>(ctx =>
        {
            ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(2));
        }).WhenTypeIs<DateTime>().IgnoringCyclicReferences());
    }

    #endregion

    #region IncludeAll

    [Fact]
    public async Task IncludeAll_WithValidEntitiesAndRecursiveAndWhenMaxDepthIsTwo_ShouldIncludeNavigations()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = SoftDeletionState.Active,
                    GetCurrentUserNameMethod = (sp) => "testuser"
                };
            });

            services.RemoveAll<DbContextOptions<MilvaBulkDbContextFixture>>();
            services.RemoveAll<MilvaBulkDbContextFixture>();

            services.AddSingleton<IModelCustomizer, TranslationRelationsModelCustomizer>();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings =>
                {
                    warnings.Log(RelationalEventId.PendingModelChangesWarning);
                    warnings.Ignore(CoreEventId.NavigationBaseIncludeIgnored);
                });
                x.UseNpgsql(_factory.GetConnectionString())
                 .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
            });
        });

        var entities = new List<SomeFullAuditableEntityFixture>
        {
            new() {
                Id = 1,
                SomeDateProp = DateTime.Now.AddYears(1),
                SomeDecimalProp = 10M,
                SomeStringProp = "somestring1",
                ManyToOneEntities =
                [
                    new() { Id = 1, SomeStringProp = "somestring1", SomeEntity = new SomeEntityFixture{ SomeDecimalProp = 10 , RelatedEntities = [ new SomeRelatedEntityFixture { Id = 1, SomeStringProp = "relatedentity"}]} },
                    new() { Id = 2, SomeStringProp = "somestring2", IsDeleted = true, SomeEntity = new SomeEntityFixture{ SomeDecimalProp = 20 } },
                ]
            },
            new() {
                Id = 2,
                SomeDateProp = DateTime.Now.AddYears(2),
                SomeDecimalProp = 20M,
                SomeStringProp = "somestring2"
            },
            new() {
                Id = 3,
                SomeDateProp = DateTime.Now.AddYears(3),
                SomeDecimalProp = 30M,
                SomeStringProp = "somestring3"
            },
            new() {
                Id = 4,
                SomeDateProp = DateTime.Now.AddYears(4),
                SomeDecimalProp = 40M,
                SomeStringProp = "somestring4",
                DeletionDate = DateTime.Now.AddDays(4).AddYears(4),
                IsDeleted = true,
            }
        };

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();
        await dbContext.FullAuditableEntities.AddRangeAsync(entities);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await dbContext.FullAuditableEntities.IncludeAll(dbContext).ToListAsync();

        // Assert
        result.Should().HaveCount(entities.Count);
        result[0].ManyToOneEntities.Count.Should().Be(2);
        result[0].ManyToOneEntities[0].SomeEntity.Should().NotBeNull();
        result[0].ManyToOneEntities[0].SomeEntity.RelatedEntities.Should().BeNull();
    }

    [Fact]
    public async Task IncludeAll_WithValidEntitiesAndRecursiveAndWhenMaxDepthIsThree_ShouldIncludeNavigations()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = SoftDeletionState.Active,
                    GetCurrentUserNameMethod = (sp) => "testuser"
                };
            });

            services.RemoveAll<DbContextOptions<MilvaBulkDbContextFixture>>();
            services.RemoveAll<MilvaBulkDbContextFixture>();

            services.AddSingleton<IModelCustomizer, TranslationRelationsModelCustomizer>();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings =>
                {
                    warnings.Log(RelationalEventId.PendingModelChangesWarning);
                    warnings.Ignore(CoreEventId.NavigationBaseIncludeIgnored);
                });
                x.UseNpgsql(_factory.GetConnectionString())
                 .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
            });
        });

        var entities = new List<SomeFullAuditableEntityFixture>
        {
            new() {
                Id = 1,
                SomeDateProp = DateTime.Now.AddYears(1),
                SomeDecimalProp = 10M,
                SomeStringProp = "somestring1",
                ManyToOneEntities =
                [
                    new() { Id = 1, SomeStringProp = "somestring1", SomeEntity = new SomeEntityFixture{ SomeDecimalProp = 10 , RelatedEntities = [ new SomeRelatedEntityFixture { Id = 1, SomeStringProp = "relatedentity"}]} },
                    new() { Id = 2, SomeStringProp = "somestring2", IsDeleted = true, SomeEntity = new SomeEntityFixture{ SomeDecimalProp = 20 } },
                ]
            },
            new() {
                Id = 2,
                SomeDateProp = DateTime.Now.AddYears(2),
                SomeDecimalProp = 20M,
                SomeStringProp = "somestring2"
            },
            new() {
                Id = 3,
                SomeDateProp = DateTime.Now.AddYears(3),
                SomeDecimalProp = 30M,
                SomeStringProp = "somestring3"
            },
            new() {
                Id = 4,
                SomeDateProp = DateTime.Now.AddYears(4),
                SomeDecimalProp = 40M,
                SomeStringProp = "somestring4",
                DeletionDate = DateTime.Now.AddDays(4).AddYears(4),
                IsDeleted = true,
            }
        };

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();
        await dbContext.FullAuditableEntities.AddRangeAsync(entities);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await dbContext.FullAuditableEntities.IncludeAll(dbContext, maxDepth: 3).ToListAsync();

        // Assert
        result.Should().HaveCount(entities.Count);
        result[0].ManyToOneEntities.Count.Should().Be(2);
        result[0].ManyToOneEntities[0].SomeEntity.Should().NotBeNull();
        result[0].ManyToOneEntities[0].SomeEntity.RelatedEntities[0].Should().NotBeNull();
    }

    #endregion

    #region IncludeTranslations

    [Fact]
    public async Task IncludeTranslations_WithInvalidEntities_ShouldNotIncludeTranslations()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = SoftDeletionState.Active,
                    GetCurrentUserNameMethod = (sp) => "testuser"
                };
            });
        });

        var entities = new List<HasTranslationEntityFixture>
        {
            new() {
                Id = 1,
            },
            new() {
                Id = 2,
            },
        };

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();
        await dbContext.HasTranslationEntities.AddRangeAsync(entities);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await dbContext.HasTranslationEntities.IncludeTranslations(dbContext).ToListAsync();

        // Assert
        result.Should().HaveCount(entities.Count);
        result[0].Translations.Should().BeNull();
    }

    [Fact]
    public async Task IncludeTranslations_WithValidEntitiesButWithoutTranslationRelations_ShouldNotIncludeTranslations()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = SoftDeletionState.Active,
                    GetCurrentUserNameMethod = (sp) => "testuser"
                };
            });
        });

        var entities = new List<HasTranslationEntityFixture>
        {
            new() {
                Id = 1,
                Translations =
                [
                       new() { Id = 1, LanguageId = 1 , Name = "trname1" },
                       new() { Id = 2, LanguageId = 2 , Name = "enname1" },
                ]
            },
            new() {
                Id = 2,
                Translations =
                [
                       new() { Id = 3, LanguageId = 1 , Name = "trname2" },
                       new() { Id = 4, LanguageId = 2 , Name = "enname2" },
                ]
            },
        };

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();
        await dbContext.HasTranslationEntities.AddRangeAsync(entities);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await dbContext.HasTranslationEntities.IncludeTranslations(dbContext).ToListAsync();

        // Assert
        result.Should().HaveCount(entities.Count);
        result[0].Translations.Should().BeNull();
    }

    [Fact]
    public async Task IncludeTranslations_WithValidEntitiesAndWithTranslationRelations_ShouldIncludeTranslations()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = SoftDeletionState.Active,
                    GetCurrentUserNameMethod = (sp) => "testuser"
                };
            });

            services.RemoveAll<DbContextOptions<MilvaBulkDbContextFixture>>();
            services.RemoveAll<MilvaBulkDbContextFixture>();

            services.AddSingleton<IModelCustomizer, TranslationRelationsModelCustomizer>();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString())
                 .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution)
                 .ReplaceService<IModelCustomizer, TranslationRelationsModelCustomizer>();
            });
        });

        var entities = new List<HasTranslationEntityFixture>
        {
            new() {
                Id = 1,
                Translations =
                [
                       new() { Id = 1, LanguageId = 1 , Name = "trname1" },
                       new() { Id = 2, LanguageId = 2 , Name = "enname1" },
                ]
            },
            new() {
                Id = 2,
                Translations =
                [
                       new() { Id = 3, LanguageId = 1 , Name = "trname2" },
                       new() { Id = 4, LanguageId = 2 , Name = "enname2" },
                ]
            },
        };

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();
        await dbContext.HasTranslationEntities.AddRangeAsync(entities);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await dbContext.HasTranslationEntities.IncludeTranslations(dbContext).ToListAsync();

        // Assert
        result.Should().HaveCount(entities.Count);
        result[0].Translations.Count.Should().Be(2);
    }

    #endregion
}