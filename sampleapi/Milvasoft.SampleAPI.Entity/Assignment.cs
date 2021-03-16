using System;

namespace Milvasoft.SampleAPI.Entity
{
    public class Assignment : EducationEntityBase
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string RemarksToStudent { get; set; }

        public string RemarksToMentor { get; set; }

        public int Level { get; set; }

        public string Rules { get; set; }

        public int MaxDeliveryDay { get; set; }

        public Guid ProfessionId { get; set; }

        public virtual Profession Profession { get; set; }

    }
}
