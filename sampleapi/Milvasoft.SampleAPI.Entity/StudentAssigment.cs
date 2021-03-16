using Milvasoft.SampleAPI.Entity.Enum;
using System;

namespace Milvasoft.SampleAPI.Entity
{
    public class StudentAssigment : EducationEntityBase
    {
        public Guid StudentId { get; set; }

        public virtual Student Student { get; set; }


        public Guid AssigmentId { get; set; }

        public virtual Assignment Assigment { get; set; }



        public EducationStatus Status { get; set; }

        public int MentorScore { get; set; }

        public string MentorDescription { get; set; }

        public string AssigmentFilePath { get; set; }

        public DateTime ShouldDeliveryDate { get; set; }

        public DateTime StartedDate { get; set; }

        public DateTime FinishedDate { get; set; }
    }
}
