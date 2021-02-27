using Microsoft.EntityFrameworkCore;
using Milvasoft.Helpers.DataAccess.MilvaContext;
using Milvasoft.Helpers.Encryption.Abstract;
using Milvasoft.Helpers.Encryption.Concrete;
using Milvasoft.SampleAPI.Entity;

namespace Milvasoft.SampleAPI.Data
{
    public class TodoAppDbContext : DbContext
    {
        private const string _encryptionKey = "w!z%C*F-JaNdRgUk";
        private readonly IMilvaEncryptionProvider _provider;

        public TodoAppDbContext(DbContextOptions<TodoAppDbContext> options) : base(options)
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
