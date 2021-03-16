using System;

namespace Milvasoft.SampleAPI.Entity
{
    public class UsefulLink : EducationEntityBase
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string Url { get; set; }

        public Guid ProfessionId { get; set; }

        public virtual Profession Profession { get; set; }

    }
}
