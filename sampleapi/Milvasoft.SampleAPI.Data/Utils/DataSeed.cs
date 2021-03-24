using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Helpers;
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

            await InitializeProfession().ConfigureAwait(false);
        }

        private static async Task InitializeDataAsync<TEntity>(List<TEntity> entities) where TEntity : EducationEntityBase
        {
            var deletingEntities = await _dbContext.Set<TEntity>().AsNoTracking().ToListAsync().ConfigureAwait(false);

            EducationAppDbContext.IgnoreSoftDeleteForNextProcess();

            _dbContext.Set<TEntity>().RemoveRange(deletingEntities);

            await _dbContext.Set<TEntity>().AddRangeAsync(entities).ConfigureAwait(false);

            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        private static async Task InitializeProfession()
        {
            var mentor = new List<Profession>
            {
                new Profession
                {
                    Id = 1.ToGuid(),
                    Name = "Reset Deneme",
                    CreationDate = DateTime.Now
                }
            };

            await InitializeDataAsync(mentor).ConfigureAwait(false);
        }
    }
}
