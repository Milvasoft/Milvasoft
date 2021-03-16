using Milvasoft.Helpers.DataAccess.Concrete.Entity;
using System;
using System.Collections.Generic;

namespace Milvasoft.SampleAPI.Entity
{
    public class Mentor : FullAuditableEntity<Guid>
    {
        public string Name { get; set; }

        public string Surname { get; set; }

        public string CVFilePath { get; set; }

        public Guid AppUserId { get; set; }

        public virtual AppUser AppUser { get; set; }

        public virtual IEnumerable<Announcement> PublishedAnnouncements { get; set; }

        public virtual IEnumerable<MentorProfession> Professions { get; set; }

        public virtual IEnumerable<Student> Students { get; set; }
    }
}