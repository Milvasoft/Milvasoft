using System;

namespace Milvasoft.SampleAPI.Entity
{
    public class Announcement : EducationEntityBase
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public bool IsFixed { get; set; }

        public Guid MentorId { get; set; }

        public virtual Mentor PublisherMentor { get; set; }

    }
}
