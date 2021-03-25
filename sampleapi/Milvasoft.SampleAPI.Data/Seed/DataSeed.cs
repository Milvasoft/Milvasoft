using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Helpers.DataAccess.Abstract.Entity;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Entity.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Data.Seed
{

    /// <summary>
    /// <para><b>EN: </b>Defines the <see cref="DataSeed" />.</para>
    /// <para><b>TR: </b><See cref = "DataSeed" /> 'ı tanımlar.</para>
    /// </summary>
    public static class DataSeed
    {
        /// <summary>
        /// <para><b>EN: </b>Resets the tables data even if table is not empty when any method called.</para>
        /// <para><b>TR: </b>Herhangi bir metot çağrıldığında tablo boş olmasa bile tablo verilerini sıfırlar.</para>
        /// </summary>
        public static bool ResetAnyway { get; set; } = false;
        /// <summary>
        /// <para><b>EN: </b>Contains dipendency injection services.</para>
        /// <para><b>TR: </b>Dipendency injection servislerini icerir.</para>
        /// </summary>
        public static IServiceProvider Services { get; set; }

        /// <summary>
        /// <para><b>EN: </b>Resets the data of type of T. This method is only calls from Reset extension method of indelible tables.</para>
        /// <para><b>TR: </b>T tipindeki verileri sıfırlar. Bu yöntem yalnızca silinmez depoların Uzatma sıfırlama yönteminden gelen çağrılardır.</para>
        /// </summary>
        /// <param name="entityList">The entityList<see cref="List{TEntity}"/>.</param>
        private static async Task InitializeTableAsync<TEntity, TKey>(List<TEntity> entityList) where TEntity : class, IBaseEntity<TKey>
                                                                                                where TKey : struct, IEquatable<TKey>
        {
            var dbContext = Services.GetRequiredService<EducationAppDbContext>();
            if (typeof(Student) != typeof(TEntity))
            {
                if (!ResetAnyway && dbContext.Set<TEntity>().Any())
                    return;
            }
            foreach (var entity in dbContext.Set<TEntity>().AsNoTracking().IgnoreQueryFilters())
                dbContext.Entry(entity).State = EntityState.Detached;
            foreach (var entity in dbContext.Set<TEntity>().AsNoTracking().IgnoreQueryFilters())
                dbContext.Remove(entity);
            EducationAppDbContext.IgnoreSoftDeleteForNextProcess();
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            foreach (var entity in dbContext.Set<TEntity>().AsNoTracking().IgnoreQueryFilters())
                dbContext.Entry(entity).State = EntityState.Detached;
            await dbContext.Set<TEntity>().AddRangeAsync(entityList).ConfigureAwait(false);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public static async Task ConfigureAutoIncrementStartIndex()
        {
            var dbContext = Services.GetRequiredService<EducationAppDbContext>();
            var entities = Assembly.GetAssembly(typeof(Student)).GetTypes();

            sbyte a = 1;
            foreach (var entity in entities)
            {
                if (entity.GetProperty("Id").PropertyType.Name == "Int32")
                    await dbContext.Database.ExecuteSqlRawAsync($"ALTER TABLE {entity.Name} AUTO_INCREMENT = 50;").ConfigureAwait(false);
                else if (entity.GetProperty("Id").PropertyType.Name == a.GetType().Name)
                {
                    var qs = entity.GetProperty("Id").PropertyType.Name;
                    await dbContext.Database.ExecuteSqlRawAsync($"ALTER TABLE {entity.Name} AUTO_INCREMENT = 20;").ConfigureAwait(false);

                }
            }
        }

        public static async Task AddProfessionForTest()
        {
            var ProfessionList = new List<Profession>()
            {
                new Profession
                {
                    Id=Guid.Parse("1"),
                    Name="Back-end"
                },
                new Profession
                {
                    Id=Guid.Parse("2"),
                    Name="Front-end"
                }
            };
        }

        public static async Task AddAssignmentForTest()
        {
            var assignmentList = new List<Assignment>()
            {
                new Assignment
                {
                    Id=Guid.Parse("1"),
                    Title="Bilgi yarışması WEB API",
                    Description="Çocuklar için 5 şıklı 20 sorulu bilgi yarışması.",
                    RemarksToStudent="Ödevi yaparken dikkatli ol gerekli kontrolleri sağla.",
                    RemarksToMentor="Projedeki kontrolleri dikkatli kontrol et.",
                    Level=1,
                    Rules="Sorular 5 şıklı olacak,En az 20 soru olacak,Veritabanı kullanılmayacak.",
                    MaxDeliveryDay=15,
                    ProfessionId=Guid.Parse("1")
                },
                new Assignment
                {
                    Id=Guid.Parse("2"),
                    Title="Bilgi yarışması arayüzü",
                    Description="Çocuklar için 5 şıklı 20 sorulu bilgi yarışması için arayüz.",
                    RemarksToStudent="Ödevi yaparken dikkatli ol.",
                    RemarksToMentor="Projedeki kontrolleri dikkatli kontrol et.",
                    Level=1,
                    Rules="React kullanılacak.",
                    MaxDeliveryDay=15,
                    ProfessionId=Guid.Parse("2")
                },
                 new Assignment
                {
                    Id=Guid.Parse("3"),
                    Title="Todo App for Education",
                    Description="Yapılacaklar listesi uygulaması.",
                    RemarksToStudent="Ödevi yaparken mimarisine dikkat et.",
                    RemarksToMentor="Tasarım desenlerinin uygulandığına dikkat edilmeli.",
                    Level=2,
                    Rules="Dependency injection kullanılmalı.",
                    MaxDeliveryDay=15,
                    ProfessionId=Guid.Parse("1")
                },
                new Assignment
                {
                    Id=Guid.Parse("4"),
                    Title="Todo App for Education arayüzü",
                    Description="Yapılacaklar listesi uygulaması arayüzü.",
                    RemarksToStudent="Uygun css kullanılmalı.",
                    RemarksToMentor="Uygun cssler kullanılmalı.",
                    Level=2,
                    Rules="Css'ler uygun olmalı.",
                    MaxDeliveryDay=15,
                    ProfessionId=Guid.Parse("2")
                }
            };
        }

        public static async Task AddMentorForTest()
        {
            var mentorList = new List<Mentor>()
            {
                new Mentor
                {
                    Name="Ahmet Buğra",
                    Surname="Kösen",

                }
            };
        }
        

    }
}
