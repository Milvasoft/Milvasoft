using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Helpers;
using Milvasoft.Helpers.DataAccess.Abstract.Entity;
using Milvasoft.Helpers.MultiTenancy.EntityBase;
using Milvasoft.SampleAPI.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Data.Utils
{
    public static class DataSeed
    {
        private static EducationAppDbContext _dbContext;

        public static async Task ResetDatabaseAsync(this IApplicationBuilder applicationBuilder)
        {
            _dbContext = applicationBuilder.ApplicationServices.GetRequiredService<EducationAppDbContext>();
            await _dbContext.Database.EnsureDeletedAsync().ConfigureAwait(false);
            await _dbContext.Database.MigrateAsync().ConfigureAwait(false);
            await InitializeTestEntities().ConfigureAwait(false);
            await InitializeProfession().ConfigureAwait(false);
            //await InitializeMentor().ConfigureAwait(false);
        }

        private static async Task InitializeDataAsync<TEntity, TKey>(List<TEntity> entities) where TEntity : class, IBaseEntity<TKey> where TKey : struct, IEquatable<TKey>
        {
            var deletingEntities = await _dbContext.Set<TEntity>().AsNoTracking().ToListAsync().ConfigureAwait(false);

            EducationAppDbContext.IgnoreSoftDeleteForNextProcess();

            _dbContext.Set<TEntity>().RemoveRange(deletingEntities);

            await _dbContext.Set<TEntity>().AddRangeAsync(entities).ConfigureAwait(false);

            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        private static async Task InitializeTestEntities()
        {
            var testEntities = new List<TestEntity>(){
                new TestEntity("milvasoft",1) {
                    Name = "Milvasoft KAFE"
                },
                new TestEntity("milvasoft",2) {
                    Name = "Inforce Restourant"
                },
            };
            await InitializeDataAsync<TestEntity, TenantId>(testEntities).ConfigureAwait(false);
        }

        private static async Task InitializeProfession()
        {
            var testEntities = new List<Profession>(){
                new Profession {
                    Id=1.ToGuid(),
                    Name = "Backend"
                },
                new Profession {
                    Name = "Frontend",
                    Id=2.ToGuid()
                },
            };
            await InitializeDataAsync<Profession, Guid>(testEntities).ConfigureAwait(false);
        }

        /*private static async Task InitializeMentor()
        {
            var testEntities = new List<AppUser>()
            {
                new AppUser
                {
                    
                    UserName="oguzhanbaran",
                    Email="oguzhan.baran96@gmail.com",
                    Mentor=new Mentor
                    {
                        Name="Oğuzhan",
                        Surname="Baran",
                        Id=1.ToGuid(),
                        Professions=new List<MentorProfession>
                        {
                            new MentorProfession
                            {
                                ProfessionId=1.ToGuid()
                            }
                        },
                    }

                }

            };
            await InitializeDataAsync<AppUser, Guid>(testEntities).ConfigureAwait(false);
        }*/
    }
}
