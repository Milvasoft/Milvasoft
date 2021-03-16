using Milvasoft.Helpers.DataAccess.Concrete.Entity;
using System;

namespace Milvasoft.SampleAPI.Entity
{
    public class MentorProfession : AuditableEntity<AppUser, Guid, Guid>
    {
        public Guid MentorId { get; set; }

        public virtual Mentor Mentor { get; set; }


        public Guid ProfessionId { get; set; }

        public virtual Profession Profession { get; set; }
    }
}
