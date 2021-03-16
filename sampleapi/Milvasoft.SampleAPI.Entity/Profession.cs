using System.Collections.Generic;

namespace Milvasoft.SampleAPI.Entity
{
    public class Profession : EducationEntityBase
    {
        public string Name { get; set; }

        public virtual IEnumerable<Assignment> Assignments { get; set; }
        public virtual IEnumerable<MentorProfession> MentorProfessions { get; set; }
        public virtual IEnumerable<Student> Students { get; set; }
        public virtual IEnumerable<Question> Questions{ get; set; }
        public virtual IEnumerable<UsefulLink> UsefulLinks { get; set; }
    }
}
