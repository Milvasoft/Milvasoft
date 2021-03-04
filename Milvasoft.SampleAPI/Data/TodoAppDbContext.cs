using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Milvasoft.Helpers.DataAccess.MilvaContext;
using Milvasoft.Helpers.DependencyInjection;
using Milvasoft.Helpers.Encryption.Abstract;
using Milvasoft.Helpers.Encryption.Concrete;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Entity.Identity;
using System;

namespace Milvasoft.SampleAPI.Data
{
    public class TodoAppDbContext : MilvaDbContextBase<MilvaUser, MilvaRole, Guid>
    {
        private const string _encryptionKey = "w!z%C*F-JaNdRgUk";
        private readonly IMilvaEncryptionProvider _provider;

        public TodoAppDbContext(DbContextOptions<TodoAppDbContext> options, IHttpContextAccessor httpContextAccessor,
                                IAuditConfiguration auditConfiguration) : base(options, httpContextAccessor, auditConfiguration)
        {
            _provider = new MilvaEncryptionProvider(_encryptionKey);
        }

        public DbSet<Category> Todos { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseEncryption(_provider);
            base.OnModelCreating(modelBuilder);
        }

    }
}
