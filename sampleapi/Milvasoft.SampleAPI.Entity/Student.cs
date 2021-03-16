using Milvasoft.Helpers.DataAccess.Concrete.Entity;
using Milvasoft.SampleAPI.Entity.Enum;
using System;
using System.Collections.Generic;

namespace Milvasoft.SampleAPI.Entity
{
    public class Student : FullAuditableEntity<Guid>
    {
        public string Name { get; set; }

        public string Surname { get; set; }

        public string University { get; set; }

        public int Age { get; set; }

        public string Dream { get; set; }

        public string HomeAddress { get; set; }

        public string MentorThoughts { get; set; }

        public bool IsConfidentialityAgreementSigned { get; set; }

        public EducationStatus GraduationStatus { get; set; }

        public int GraduationScore { get; set; }

        public string MentorGraduationThoughts { get; set; }

        public DateTime CurrentAssigmentDeliveryDate { get; set; }


        public Guid AppUserId { get; set; }

        public virtual AppUser AppUser { get; set; }


        public Guid ProfessionId { get; set; }

        public virtual Profession Profession { get; set; }


        public Guid MentorId { get; set; }

        public virtual Mentor Mentor { get; set; }


        public virtual IEnumerable<StudentAssigment> OldAssignments { get; set; }

    }
}
