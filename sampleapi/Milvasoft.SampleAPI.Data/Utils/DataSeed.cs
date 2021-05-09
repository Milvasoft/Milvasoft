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
            await InitializeMentor().ConfigureAwait(false);
            await InitializeAnnouncement().ConfigureAwait(false);
            await InitializeUsefulLink().ConfigureAwait(false);

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
                    Id=2.ToGuid(),
                    Name = "Frontend"
                },
            };
            await InitializeDataAsync<Profession, Guid>(testEntities).ConfigureAwait(false);
        }

        private static async Task InitializeAnnouncement()
        {
            var testEntities = new List<Announcement>()
            {
                new Announcement
                {
                    Id=1.ToGuid(),
                    Title="Stajyerler hakkında",
                    Description="Stajyerler verilen ödevleri yapmalıdır.",
                    IsFixed=true,
                    MentorId=1.ToGuid()
                },
                new Announcement
                {
                    Id=2.ToGuid(),
                    Title="Stajyerler hakkında",
                    Description="Stajyerler kurallar uymak zorundadır.",
                    IsFixed=false,
                    MentorId=2.ToGuid()
                },
                new Announcement
                {
                    Id=3.ToGuid(),
                    Title="Şirket kuralları",
                    Description="Stajyerler arasında ilişki olamaz.",
                    IsFixed=false,
                    MentorId=1.ToGuid()
                }
            };
            await InitializeDataAsync<Announcement, Guid>(testEntities).ConfigureAwait(false);
        }

        private static async Task InitializeUsefulLink()
        {
            var testEntities = new List<UsefulLink>()
            {
                new UsefulLink
                {
                    Id=1.ToGuid(),
                    ProfessionId=1.ToGuid(),
                    Title="C# Dersleri",
                    Url="www.oguzhanbaran.com"
                },
                new UsefulLink
                {
                    Id=2.ToGuid(),
                    ProfessionId=2.ToGuid(),
                    Title="C# Dersleri",
                    Url="www.oguzhanbaranc#.com"
                },new UsefulLink
                {
                    Id=3.ToGuid(),
                    ProfessionId=2.ToGuid(),
                    Title="HTML Dersleri",
                    Url="www.oguzhanbaranhtml.com"
                },new UsefulLink
                {
                    Id=4.ToGuid(),
                    ProfessionId=1.ToGuid(),
                    Title="Java Dersleri",
                    Url="www.oguzhanbaran.com"
                }
            };
            await InitializeDataAsync<UsefulLink, Guid>(testEntities).ConfigureAwait(false);
        }

        private static async Task InitializeStudent()
        {
            var testEntities = new List<AppUser>
            {
                new AppUser
                {
                    UserName="burakakbay",
                    Email="burakakbay@gmail.com",
                    PhoneNumber="507 664 05 44",
                        Student=new Student
                        {
                            Id=1.ToGuid(),
                            Name="Burak",
                            Surname="Akbay",
                            ProfessionId=1.ToGuid(),
                            IsConfidentialityAgreementSigned=true,
                            Level=1,
                            Age=18,
                            Dream="Yazılımcı olmak.",
                            HomeAddress="Ankara",
                            University="Selçuk Üniversitesi",
                            MentorId=1.ToGuid()
                        }
                },
                new AppUser
                {
                    UserName="mehmetbayburt",
                    Email="mehmetbayburt@gmail.com",
                    PhoneNumber="507 661 05 44",
                        Student=new Student
                        {
                            Id=2.ToGuid(),
                            Name="Mehmet",
                            Surname="Bayburt",
                            ProfessionId=2.ToGuid(),
                            IsConfidentialityAgreementSigned=true,
                            Level=2,
                            Age=21,
                            Dream="Makina mühendisi olmak.",
                            HomeAddress="istanbul",
                            University="Selçuk Üniversitesi",
                            MentorId=2.ToGuid()
                        }
                }
            };
            await InitializeDataAsync<AppUser, Guid>(testEntities).ConfigureAwait(false);
        }
        
        private static async Task InitializeMentor()
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
                        }
                    }

                },
                new AppUser
                {

                    UserName="bugrakosen",
                    Email="bugrakosen@gmail.com",
                    Mentor=new Mentor
                    {
                        Name="Buğra Ahmet",
                        Surname="Kösen",
                        Id=2.ToGuid(),
                        Professions=new List<MentorProfession>
                        {
                            new MentorProfession
                            {
                                ProfessionId=2.ToGuid()
                            }
                        }
                    }

                }

            };
            await InitializeDataAsync<AppUser, Guid>(testEntities).ConfigureAwait(false);
        }
    }
}
