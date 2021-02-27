
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Milvasoft.SampleAPI.Data;
using Milvasoft.SampleAPI.Entity;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Seed
{
    public static class SeedData
    {
        private const QueryTrackingBehavior NO_TRACKING = QueryTrackingBehavior.NoTracking;

        public static async Task ResetDatas()
        {
            await ResetCategories().ConfigureAwait(false);
            await ResetTodos().ConfigureAwait(false);
        }

        /// <summary>
        /// This method return int value to guid value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Guid ToGuid(this int value)
        {
            byte[] bytes = new byte[16];
            BitConverter.GetBytes(value).CopyTo(bytes, 0);
            return new Guid(bytes);
        }

        private static async Task ResetTable<TEntity>(List<TEntity> entities) where TEntity : BaseEntity
        {
            IConfiguration configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.Development.json")
           .Build();

            var connectionString = configuration.GetConnectionString("MySqlConnectionTest");

            var optionsBuilder = new DbContextOptionsBuilder<TodoAppDbContext>()
                              .UseMySql(connectionString,
                                     new MySqlServerVersion(new Version(8, 0, 21)),
                                     mySqlOptionsAction: b =>
                                     {
                                         b.SchemaBehavior(MySqlSchemaBehavior.Ignore);
                                     }).UseQueryTrackingBehavior(NO_TRACKING);

            using (var context = new TodoAppDbContext(optionsBuilder.Options))
            {
                await context.Database.MigrateAsync().ConfigureAwait(false);

                var dbSet = context.Set<TEntity>();

                var allEntities = await dbSet.ToListAsync().ConfigureAwait(false);
                dbSet.RemoveRange(allEntities);
                //await context.SaveChangesAsync().ConfigureAwait(false);

                await dbSet.AddRangeAsync(entities).ConfigureAwait(false);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private static async Task ResetCategories()
        {
            var categories = new List<Category>
            {
                new Category
                {
                    Id = 1.ToGuid(),
                    CreationDate = DateTime.Now,
                    Name = "Okul için"
                },
                new Category
                {
                    Id = 2.ToGuid(),
                    CreationDate = DateTime.Now,
                    Name = "İş için"
                },
                new Category
                {
                    Id = 3.ToGuid(),
                    CreationDate = DateTime.Now,
                    Name = "Yapılacaklar"
                }
            };

            await ResetTable(categories).ConfigureAwait(false);
        }

        private static async Task ResetTodos()
        {
            var todos = new List<Todo>
            {
                new Todo
                {
                    Id = 1.ToGuid(),
                    CreationDate = DateTime.Now,
                    CategoryId = 1.ToGuid(),
                    DueDate = DateTime.Now.AddDays(1),
                    ReminMeDate = DateTime.Now.AddDays(2),
                    IsFavorite = true,
                    Content = ".Net core dependency injection yapısı çalışılacak"
                },
                new Todo
                {
                    Id = 2.ToGuid(),
                    CreationDate = DateTime.Now,
                    CategoryId = 2.ToGuid(),
                    DueDate = DateTime.Now.AddDays(1),
                    ReminMeDate = DateTime.Now.AddDays(2),
                    IsFavorite = false,
                    Content = "Yarın toplantı var."
                },
                new Todo
                {
                    Id = 3.ToGuid(),
                    CreationDate = DateTime.Now,
                    CategoryId = 3.ToGuid(),
                    DueDate = DateTime.Now.AddDays(1),
                    ReminMeDate = DateTime.Now.AddDays(2),
                    IsFavorite = false,
                    Content = "Eve giderke ekmek al"
                },
                new Todo
                {
                    Id = 4.ToGuid(),
                    CreationDate = DateTime.Now,
                    CategoryId = 1.ToGuid(),
                    DueDate = DateTime.Now.AddDays(1),
                    ReminMeDate = DateTime.Now.AddDays(2),
                    IsFavorite = true,
                    Content = "Haftaya vizeler başlıyor"
                },
                new Todo
                {
                    Id = 5.ToGuid(),
                    CreationDate = DateTime.Now,
                    CategoryId = 2.ToGuid(),
                    DueDate = DateTime.Now.AddDays(1),
                    ReminMeDate = DateTime.Now.AddDays(2),
                    IsFavorite = false,
                    Content = "yarın saat 5 de toplantı var"
                },
                new Todo
                {
                    Id = 6.ToGuid(),
                    CreationDate = DateTime.Now,
                    CategoryId = 3.ToGuid(),
                    DueDate = DateTime.Now.AddDays(1),
                    ReminMeDate = DateTime.Now.AddDays(2),
                    IsFavorite = false,
                    Content = "Yazıcının kartuşunu değiştir."
                },
                new Todo
                {
                    Id = 7.ToGuid(),
                    CreationDate = DateTime.Now,
                    CategoryId = 1.ToGuid(),
                    DueDate = DateTime.Now.AddDays(1),
                    ReminMeDate = DateTime.Now.AddDays(2),
                    IsFavorite = false,
                    Content = "Murat hocanın yanına uğra"
                },
                new Todo
                {
                    Id = 8.ToGuid(),
                    CreationDate = DateTime.Now,
                    CategoryId = 2.ToGuid(),
                    DueDate = DateTime.Now.AddDays(1),
                    ReminMeDate = DateTime.Now.AddDays(2),
                    IsFavorite = false,
                    Content = "Müşteri evraklrini hazırla"
                },
                new Todo
                {
                    Id = 9.ToGuid(),
                    CreationDate = DateTime.Now,
                    CategoryId = 3.ToGuid(),
                    DueDate = DateTime.Now.AddDays(1),
                    ReminMeDate = DateTime.Now.AddDays(2),
                    IsFavorite = false,
                    Content = "Erdem hocayla bu mevzuyu konuş sen hatırlarsın."
                },
                new Todo
                {
                    Id = 10.ToGuid(),
                    CreationDate = DateTime.Now,
                    CategoryId = 1.ToGuid(),
                    DueDate = DateTime.Now.AddDays(1),
                    ReminMeDate = DateTime.Now.AddDays(2),
                    IsFavorite = true,
                    Content = "Krdeş için kalem defter al"
                },

            };

            await ResetTable(todos).ConfigureAwait(false);
        }
    }
}
