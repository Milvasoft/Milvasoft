using System.Collections.Generic;

namespace Milvasoft.SampleAPI.Entity
{
    /// <summary>
    /// Profession entity.
    /// </summary>
    public class Profession : EducationEntityBase
    {

        /// <summary>
        /// Name of profession.
        /// </summary>
        public string Name { get; set; }



        /// <summary>
        /// Assignments of profession.
        /// </summary>
        public virtual IEnumerable<Assignment> Assignments { get; set; }

        /// <summary>
        /// Mentor profession relationship.
        /// </summary>
        public virtual IEnumerable<MentorProfession> MentorProfessions { get; set; }

        /// <summary>
        /// Students of profession.
        /// </summary>
        public virtual IEnumerable<Student> Students { get; set; }

        /// <summary>
        /// Questions of profession.
        /// </summary>
        public virtual IEnumerable<Question> Questions { get; set; }

        /// <summary>
        /// Useful links of profession.
        /// </summary>
        public virtual IEnumerable<UsefulLink> UsefulLinks { get; set; }
    }
}
