using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Milvasoft.Attributes.Annotations;
using Milvasoft.Core.EntityBases.Abstract;
using Milvasoft.Core.EntityBases.Abstract.Auditing;
using Milvasoft.Core.EntityBases.Concrete;
using Milvasoft.Core.EntityBases.MultiTenancy;
using Milvasoft.Core.Helpers;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.Cryptography.Abstract;
using Milvasoft.Cryptography.Builder;
using Milvasoft.DataAccess.EfCore;
using Milvasoft.DataAccess.EfCore.Configuration;
using Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures;
using Milvasoft.IntegrationTests.Client.Fixtures.Persistence;
using System.Reflection;

namespace Milvasoft.IntegrationTests.DataAccessTests.EfCoreTests;

[Collection(nameof(UtcTrueDatabaseTestCollection))]
[Trait("MilvaEfExtensions Integration Tests", "Integration tests for Milvasoft.DataAccess.EfCore integration tests.")]
public class ModelBuilderExtensionsTests(CustomWebApplicationFactory factory) : DataAccessIntegrationTestBase(factory)
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

    [Fact]
    public async Task UseTenantId_WithBaseContext_ShouldAddEntity()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString());
            });

        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entity = new SomeModelBuilderTestEntityFixture
        {
            Id = 1,
            TenantId = TenantId.NewTenantId()
        };

        // Act & Assert
        await dbContext.ModelBuilderTestEntities.AddAsync(entity);
        await dbContext.SaveChangesAsync();
        var entityInDb = await dbContext.ModelBuilderTestEntities.FirstOrDefaultAsync();
        entityInDb.TenantId.Should().Be(entity.TenantId);
    }

    [Fact]
    public async Task UseUtcDateTime_WithoutUseUtcDateTime_ShouldThrowException()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).ReplaceService<IModelCustomizer, NoUseUtcDateTimeModelCustomizer>();
            });

        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entity = new SomeModelBuilderTestEntityFixture
        {
            Id = 1,
            TenantId = TenantId.NewTenantId(),
            SomeDateProp = DateTime.Now,
            SomeNullableDateProp = DateTime.Now,
            SomeNullableDateTimeOffsetProp = DateTimeOffset.Now,
            SomeDateTimeOffsetProp = DateTimeOffset.Now,
        };

        // Act
        await dbContext.ModelBuilderTestEntities.AddAsync(entity);
        Func<Task> act = async () => await dbContext.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task UseUtcDateTime_WithUseUtcDateTime_ShouldInsertAsUtc()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).ReplaceService<IModelCustomizer, UseUtcDateTimeModelCustomizer>();
            });

        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entity = new SomeModelBuilderTestEntityFixture
        {
            Id = 1,
            TenantId = TenantId.Empty,
            SomeDateProp = DateTime.Now,
            SomeNullableDateProp = DateTime.Now,
            SomeNullableDateTimeOffsetProp = DateTimeOffset.Now,
            SomeDateTimeOffsetProp = DateTimeOffset.Now,
        };

        // Act & Assert
        await dbContext.ModelBuilderTestEntities.AddAsync(entity);
        await dbContext.SaveChangesAsync();

        var entityInDb = await dbContext.Database.SqlQueryRaw<SomeModelBuilderTestKeylessEntityFixture>(""" select * from "ModelBuilderTestEntities" """).FirstOrDefaultAsync();
        entityInDb.SomeDateProp.Should().BeCloseTo(entity.SomeDateProp.ToUniversalTime(), TimeSpan.FromSeconds(2));
        entityInDb.SomeNullableDateProp.Should().BeCloseTo(entity.SomeNullableDateProp.Value.ToUniversalTime(), TimeSpan.FromSeconds(2));
        entityInDb.SomeNullableDateTimeOffsetProp.Should().BeCloseTo(entity.SomeNullableDateTimeOffsetProp.Value.ToUniversalTime(), TimeSpan.FromSeconds(2));
        entityInDb.SomeDateTimeOffsetProp.Should().BeCloseTo(entity.SomeDateTimeOffsetProp.ToUniversalTime(), TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task UseAnnotationEncryption_WithoutUseAnnotationEncryption_ShouldNotEncrypt()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).ReplaceService<IModelCustomizer, UseUtcDateTimeModelCustomizer>();
            });

        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entity = new SomeModelBuilderTestEntityFixture
        {
            Id = 1,
            SomeEncryptedStringWithAttributeProp = "notencrypted",
            SomeEncryptedStringProp = "notencrypted",
            TenantId = TenantId.NewTenantId(),
            SomeDateProp = DateTime.Now,
            SomeDateTimeOffsetProp = DateTimeOffset.Now,
        };

        // Act & Assert
        await dbContext.ModelBuilderTestEntities.AddAsync(entity);
        await dbContext.SaveChangesAsync();

        var entityInDb = await dbContext.ModelBuilderTestEntities.FirstOrDefaultAsync();
        entityInDb.SomeEncryptedStringProp.Should().Be(entity.SomeEncryptedStringProp);
        entityInDb.SomeEncryptedStringWithAttributeProp.Should().Be(entity.SomeEncryptedStringWithAttributeProp);
    }

    [Fact]
    public async Task UseAnnotationEncryption_WithUseAnnotationEncryption_ShouldEncrypt()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.AddMilvaCryptography()
                    .WithOptions(opt =>
                    {
                        opt.Key = "w!z%C*F-JaNdRgUk";
                    });

            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).ReplaceService<IModelCustomizer, UseAnnotationEncryptionModelCustomizer>();
            });

        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entity = new SomeModelBuilderTestEntityFixture
        {
            Id = 1,
            TenantId = TenantId.Empty,
            SomeEncryptedStringWithAttributeProp = "notencrypted",
            SomeEncryptedStringProp = "notencrypted",
            SomeDateProp = DateTime.Now,
            SomeNullableDateProp = DateTime.Now,
            SomeNullableDateTimeOffsetProp = DateTimeOffset.Now,
            SomeDateTimeOffsetProp = DateTimeOffset.Now,
        };
        var encryptor = _serviceProvider.GetService<IMilvaCryptographyProvider>();

        // Act & Assert
        await dbContext.ModelBuilderTestEntities.AddAsync(entity);
        await dbContext.SaveChangesAsync();

        var entityInDb = await dbContext.Database.SqlQueryRaw<SomeModelBuilderTestKeylessEntityFixture>(""" select * from "ModelBuilderTestEntities" """).FirstOrDefaultAsync();
        var entityInDb2 = await dbContext.ModelBuilderTestEntities.FirstOrDefaultAsync();
        entityInDb.SomeEncryptedStringProp.Should().Be(entity.SomeEncryptedStringProp);
        entityInDb.SomeEncryptedStringWithAttributeProp.Should().NotBe(entity.SomeEncryptedStringWithAttributeProp);
        var cipher = entityInDb.SomeEncryptedStringWithAttributeProp;
        cipher.Should().StartWith("enc::");
        var decyrptedString = await encryptor.DecryptAsync(cipher["enc::".Length..]);
        decyrptedString.Should().Be(entity.SomeEncryptedStringWithAttributeProp);
        entityInDb2.SomeEncryptedStringProp.Should().Be(entity.SomeEncryptedStringProp);
        entityInDb2.SomeEncryptedStringWithAttributeProp.Should().Be(entity.SomeEncryptedStringWithAttributeProp);
    }

    [Fact]
    public async Task UseEncryption_WithUseEncryption_ShouldEncrypt()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.AddMilvaCryptography()
                    .WithOptions(opt =>
                    {
                        opt.Key = "w!z%C*F-JaNdRgUk";
                    });

            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).ReplaceService<IModelCustomizer, UseEncryptionModelCustomizer>();
            });

        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entity = new SomeModelBuilderTestEntityFixture
        {
            Id = 1,
            TenantId = TenantId.Empty,
            SomeEncryptedStringWithAttributeProp = "notencrypted",
            SomeEncryptedStringProp = "notencrypted",
            SomeDateProp = DateTime.Now,
            SomeNullableDateProp = DateTime.Now,
            SomeNullableDateTimeOffsetProp = DateTimeOffset.Now,
            SomeDateTimeOffsetProp = DateTimeOffset.Now,
        };
        var encryptor = _serviceProvider.GetService<IMilvaCryptographyProvider>();

        // Act & Assert
        await dbContext.ModelBuilderTestEntities.AddAsync(entity);
        await dbContext.SaveChangesAsync();

        var entityInDb = await dbContext.Database.SqlQueryRaw<SomeModelBuilderTestKeylessEntityFixture>(""" select * from "ModelBuilderTestEntities" """).FirstOrDefaultAsync();
        var entityInDb2 = await dbContext.ModelBuilderTestEntities.FirstOrDefaultAsync();
        var cipher = entityInDb.SomeEncryptedStringWithAttributeProp;
        cipher.Should().StartWith("enc::");
        var cipher2 = entityInDb.SomeEncryptedStringWithAttributeProp;
        cipher.Should().StartWith("enc::");
        (await encryptor.DecryptAsync(cipher["enc::".Length..])).Should().Be(entity.SomeEncryptedStringProp);
        (await encryptor.DecryptAsync(cipher2["enc::".Length..])).Should().Be(entity.SomeEncryptedStringWithAttributeProp);
        entityInDb2.SomeEncryptedStringProp.Should().Be(entity.SomeEncryptedStringProp);
        entityInDb2.SomeEncryptedStringWithAttributeProp.Should().Be(entity.SomeEncryptedStringWithAttributeProp);
    }

    [Fact]
    public async Task UseIndexMethods_WithoutUseIndexMethods_ShouldNotIndex()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).ReplaceService<IModelCustomizer, UseUtcDateTimeModelCustomizer>();
            });

        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();

        // Act & Assert
        var entityTypes = dbContext.Model.GetEntityTypes();

        foreach (var entityType in entityTypes)
        {
            var canAssignableToCreationAuditable = entityType.ClrType.CanAssignableTo(typeof(ICreationAuditable<>));

            if (canAssignableToCreationAuditable)
            {
                var indexes = entityType.GetIndexes();
                var creationDateIndex = indexes.FirstOrDefault(i => i.Properties.Any(p => p.Name == EntityPropertyNames.CreationDate));
                creationDateIndex.Should().BeNull();
            }

            var canAssignableToSoftDeletable = entityType.ClrType.CanAssignableTo(typeof(ISoftDeletable));

            if (canAssignableToSoftDeletable)
            {
                var indexes = entityType.GetIndexes();
                var isDeletedIndex = indexes.FirstOrDefault(i => i.Properties.Any(p => p.Name == EntityPropertyNames.IsDeleted));
                isDeletedIndex.Should().BeNull();
            }

            var canAssignableToLogEntity = entityType.ClrType.CanAssignableTo(typeof(LogEntityBase<>));

            if (canAssignableToLogEntity)
            {
                var indexes = entityType.GetIndexes();
                var transactionIdIndex = indexes.FirstOrDefault(i => i.Properties.Any(p => p.Name == EntityPropertyNames.TransactionId));
                var utcLogTimeIndex = indexes.FirstOrDefault(i => i.Properties.Any(p => p.Name == EntityPropertyNames.UtcLogTime));
                var methodNameIndex = indexes.FirstOrDefault(i => i.Properties.Any(p => p.Name == EntityPropertyNames.MethodName));
                var isSuccessIndex = indexes.FirstOrDefault(i => i.Properties.Any(p => p.Name == EntityPropertyNames.IsSuccess));

                transactionIdIndex.Should().BeNull();
                utcLogTimeIndex.Should().BeNull();
                methodNameIndex.Should().BeNull();
                isSuccessIndex.Should().BeNull();
            }
        }
    }

    [Fact]
    public async Task UseIndexMethods_WithUseIndexMethods_ShouldIndex()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).ReplaceService<IModelCustomizer, UseIndexModelCustomizer>();
            });

        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();

        // Act & Assert
        var entityTypes = dbContext.Model.GetEntityTypes();

        foreach (var entityType in entityTypes)
        {
            var canAssignableToCreationAuditable = entityType.ClrType.CanAssignableTo(typeof(ICreationAuditable<>));

            if (canAssignableToCreationAuditable)
            {
                var indexes = entityType.GetIndexes();
                var creationDateIndex = indexes.FirstOrDefault(i => i.Properties.Any(p => p.Name == EntityPropertyNames.CreationDate));
                creationDateIndex.Should().NotBeNull();
            }

            var canAssignableToSoftDeletable = entityType.ClrType.CanAssignableTo(typeof(ISoftDeletable));

            if (canAssignableToSoftDeletable)
            {
                var indexes = entityType.GetIndexes();
                var isDeletedIndex = indexes.FirstOrDefault(i => i.Properties.Any(p => p.Name == EntityPropertyNames.IsDeleted));
                isDeletedIndex.Should().NotBeNull();
            }

            var canAssignableToLogEntity = entityType.ClrType.CanAssignableTo(typeof(LogEntityBase<>));

            if (canAssignableToLogEntity)
            {
                var indexes = entityType.GetIndexes();
                var transactionIdIndex = indexes.FirstOrDefault(i => i.Properties.Any(p => p.Name == EntityPropertyNames.TransactionId));
                var utcLogTimeIndex = indexes.FirstOrDefault(i => i.Properties.Any(p => p.Name == EntityPropertyNames.UtcLogTime));
                var methodNameIndex = indexes.FirstOrDefault(i => i.Properties.Any(p => p.Name == EntityPropertyNames.MethodName));
                var isSuccessIndex = indexes.FirstOrDefault(i => i.Properties.Any(p => p.Name == EntityPropertyNames.IsSuccess));

                transactionIdIndex.Should().NotBeNull();
                utcLogTimeIndex.Should().NotBeNull();
                methodNameIndex.Should().NotBeNull();
                isSuccessIndex.Should().NotBeNull();
            }
        }
    }

    [Fact]
    public async Task UseTurkishCollation_WithoutUseTurkishCollation_ShouldNotCollate()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).ReplaceService<IModelCustomizer, UseUtcDateTimeModelCustomizer>();
            });

        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();

        // Act & Assert
        var entityTypes = dbContext.GetService<IDesignTimeModel>().Model.GetEntityTypes();

        foreach (var prop in entityTypes.SelectMany(e => e.GetProperties()))
        {
            prop.GetCollation().Should().NotBe("tr-TR-x-icu");
        }
    }

    [Fact]
    public async Task UseTurkishCollation_WithUseTurkishCollation_ShouldCollateStringProps()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).ReplaceService<IModelCustomizer, UseTurkishCollationModelCustomizer>();
            });

        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();

        // Act & Assert
        var entityTypes = dbContext.GetService<IDesignTimeModel>().Model.GetEntityTypes();

        foreach (var prop in entityTypes.SelectMany(e => e.GetProperties()).Where(p => p.ClrType == typeof(string)))
        {
            prop.GetCollation().Should().Be("tr-TR-x-icu");
        }
    }

    [Fact]
    public async Task UsePrecision_WithoutUsePrecision_ShouldBeDefaultPrecision()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).ReplaceService<IModelCustomizer, UseUtcDateTimeModelCustomizer>();
            });

        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();

        // Act & Assert
        var entityTypes = dbContext.GetService<IDesignTimeModel>().Model.GetEntityTypes();

        foreach (var prop in entityTypes.SelectMany(e => e.GetProperties().Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?))))
        {
            prop.GetPrecision().Should().BeNull();
            prop.GetScale().Should().BeNull();
        }
    }

    [Fact]
    public async Task UsePrecision_WithUsePrecision_ShouldBeGivenPrecision()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).ReplaceService<IModelCustomizer, UsePrecisionModelCustomizer>();
            });

        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();

        // Act & Assert
        var entityTypes = dbContext.GetService<IDesignTimeModel>().Model.GetEntityTypes();

        foreach (var prop in entityTypes.SelectMany(e => e.GetProperties().Where(p => p.ClrType.CanAssignableTo(typeof(decimal)) || p.ClrType.CanAssignableTo(typeof(decimal?)))))
        {
            prop.GetPrecision().Should().Be(18);
            prop.GetScale().Should().Be(10);
        }
    }

    [Fact]
    public async Task UseAnnotationPrecision_WithUseAnnotationPrecision_ShouldBeGivenPrecision()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).ReplaceService<IModelCustomizer, UseAnnotationPrecisionModelCustomizer>();
            });

        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();

        // Act & Assert
        var entityTypes = dbContext.GetService<IDesignTimeModel>().Model.GetEntityTypes();

        foreach (var prop in entityTypes.SelectMany(e => e.GetProperties().Where(p => (p.PropertyInfo ?? (MemberInfo)p.FieldInfo)?.GetCustomAttribute<DecimalPrecisionAttribute>() != null
                                                                                      && p.ClrType.IsAssignableFrom(typeof(decimal))
                                                                                      || p.ClrType.CanAssignableTo(typeof(decimal?)))))
        {
            prop.GetPrecision().Should().Be(18);
            prop.GetScale().Should().Be(2);
        }
    }

    [Fact]
    public async Task UseCollationOnStringProperties_WithUseCollationOnStringProperties_ShouldCollateStringProps()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).ReplaceService<IModelCustomizer, UseCollationOnStringPropertiesModelCustomizer>();
            });

        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();

        // Act & Assert
        var entityTypes = dbContext.GetService<IDesignTimeModel>().Model.GetEntityTypes();

        foreach (var prop in entityTypes.SelectMany(e => e.GetProperties()).Where(p => p.ClrType == typeof(string)))
        {
            prop.GetCollation().Should().Be("tr-TR-x-icu");
        }
    }

    [Fact]
    public async Task UseDefaultValue_WithoutUseDefaultValue_ShouldBeTypeDefault()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).ReplaceService<IModelCustomizer, UseUtcDateTimeModelCustomizer>();
            });

        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entity = new SomeModelBuilderTestEntityFixture
        {
            Id = 1,
            SomeDateProp = DateTime.Now,
            SomeDateTimeOffsetProp = DateTimeOffset.Now,
        };

        // Act & Assert
        await dbContext.ModelBuilderTestEntities.AddAsync(entity);
        await dbContext.SaveChangesAsync();

        var entityInDb = await dbContext.ModelBuilderTestEntities.FirstOrDefaultAsync();
        entityInDb.SomeIntProp.Should().Be(0);
    }

    [Fact]
    public async Task UseDefaultValue_WithUseDefaultValue_ShouldApplyDefaultValue()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).ReplaceService<IModelCustomizer, UseDefaultValueModelCustomizer>();
            });

        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entity = new SomeModelBuilderTestEntityFixture
        {
            Id = 1,
            TenantId = TenantId.Empty,
            SomeEncryptedStringWithAttributeProp = "notencrypted",
            SomeEncryptedStringProp = "notencrypted",
            SomeDateProp = DateTime.Now,
            SomeNullableDateProp = DateTime.Now,
            SomeNullableDateTimeOffsetProp = DateTimeOffset.Now,
            SomeDateTimeOffsetProp = DateTimeOffset.Now,
        };

        // Act & Assert
        await dbContext.ModelBuilderTestEntities.AddAsync(entity);
        await dbContext.SaveChangesAsync();

        var entityInDb = await dbContext.ModelBuilderTestEntities.FirstOrDefaultAsync();
        entityInDb.SomeIntProp.Should().Be(1);
    }

    [Fact]
    public async Task UseTenantIdQueryFilter_WithoutUseTenantIdQueryFilter_ShouldReturnAll()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).ReplaceService<IModelCustomizer, UseUtcDateTimeModelCustomizer>();
            });

        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entities = new List<SomeModelBuilderTestEntityFixture>
        {
            new() {
                Id = 1,
                TenantId = TenantId.NewTenantId(),
                SomeDateProp = DateTime.Now,
                SomeDateTimeOffsetProp = DateTimeOffset.Now,
            },
            new() {
                Id = 2,
                TenantId = new TenantId("milvasoft_1"),
                SomeDateProp = DateTime.Now,
                SomeDateTimeOffsetProp = DateTimeOffset.Now,
            },
        };

        // Act & Assert
        await dbContext.ModelBuilderTestEntities.AddRangeAsync(entities);
        await dbContext.SaveChangesAsync();

        var entitiesInDb = await dbContext.ModelBuilderTestEntities.ToListAsync();
        entitiesInDb.Count.Should().Be(2);
    }

    [Fact]
    public async Task UseSoftDeleteQueryFilter_UseSoftDeleteQueryFilter_ShouldReturnAll()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = DataAccess.EfCore.Utils.Enums.SoftDeletionState.Passive
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).ReplaceService<IModelCustomizer, UseUtcDateTimeModelCustomizer>();
            });

        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entities = new List<SomeModelBuilderTestEntityFixture>
        {
            new() {
                Id = 1,
                TenantId = TenantId.NewTenantId(),
                SomeDateProp = DateTime.Now,
                SomeDateTimeOffsetProp = DateTimeOffset.Now,
            },
            new() {
                Id = 2,
                TenantId = new TenantId("milvasoft_1"),
                SomeDateProp = DateTime.Now,
                SomeDateTimeOffsetProp = DateTimeOffset.Now,
                IsDeleted = true
            },
        };

        // Act & Assert
        await dbContext.ModelBuilderTestEntities.AddRangeAsync(entities);
        await dbContext.SaveChangesAsync();

        var entitiesInDb = await dbContext.ModelBuilderTestEntities.ToListAsync();
        entitiesInDb.Count.Should().Be(2);
    }

    [Fact]
    public async Task UseSoftDeleteQueryFilter_WithUseSoftDeleteQueryFilter_ShouldReturnOnlyFiltereds()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).ReplaceService<IModelCustomizer, UseSoftDeleteQueryFilterModelCustomizer>();
            });

        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entities = new List<SomeModelBuilderTestEntityFixture>
        {
            new() {
                Id = 1,
                TenantId = TenantId.NewTenantId(),
                SomeDateProp = DateTime.Now,
                SomeDateTimeOffsetProp = DateTimeOffset.Now,
            },
            new() {
                Id = 2,
                TenantId = new TenantId("milvasoft_1"),
                SomeDateProp = DateTime.Now,
                SomeDateTimeOffsetProp = DateTimeOffset.Now,
                IsDeleted = true
            },
        };

        // Act & Assert
        await dbContext.ModelBuilderTestEntities.AddRangeAsync(entities);
        await dbContext.SaveChangesAsync();

        var entitiesInDb = await dbContext.ModelBuilderTestEntities.ToListAsync();
        entitiesInDb.Count.Should().Be(1);
    }
}
