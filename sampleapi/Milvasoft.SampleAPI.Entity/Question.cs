using System;

namespace Milvasoft.SampleAPI.Entity
{
    public class Question : EducationEntityBase
    {
        public string Title { get; set; }

        public string QuestionContent { get; set; }

        public string MentorReply { get; set; }

        public bool IsUseful { get; set; }

        public bool WillShown { get; set; }

        public Guid ProfessionId { get; set; }

        public virtual Profession Profession { get; set; }

        public Guid StudentId { get; set; }

        public virtual Student Student { get; set; }

        public Guid? MentorId { get; set; }

        public virtual Mentor Mentor { get; set; }

    }
}
