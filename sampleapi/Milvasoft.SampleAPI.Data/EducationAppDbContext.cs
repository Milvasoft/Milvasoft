using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Milvasoft.Helpers.DataAccess.MilvaContext;
using Milvasoft.Helpers.DependencyInjection;
using Milvasoft.Helpers.Encryption.Abstract;
using Milvasoft.Helpers.Encryption.Concrete;
using Milvasoft.SampleAPI.Entity;
using System;

namespace Milvasoft.SampleAPI.Data
{
    public class EducationAppDbContext : MilvaDbContextBase<AppUser, AppRole, Guid>
    {
        private const string _encryptionKey = "w!z%C*F-JaNdRgUk";
        private readonly IMilvaEncryptionProvider _provider;

        public EducationAppDbContext(DbContextOptions<EducationAppDbContext> options,
                                     IHttpContextAccessor httpContextAccessor,
                                     IAuditConfiguration auditConfiguration) : base(options, httpContextAccessor, auditConfiguration)
        {
            _provider = new MilvaEncryptionProvider(_encryptionKey);
        }

        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Mentor> Mentors { get; set; }
        public DbSet<MentorProfession> MentorProfessions { get; set; }
        public DbSet<Profession> Professions { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<StudentAssigmentDTO> StudentAssigments { get; set; }
        public DbSet<UsefulLink> UsefulLinks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseEncryption(_provider);
            base.OnModelCreating(modelBuilder);
        }

    }
}
