using Milvasoft.SampleAPI.DTOs.AssignmentDTOs;
using Milvasoft.SampleAPI.DTOs.MentorDTOs;
using Milvasoft.SampleAPI.DTOs.QuestionDTOs;
using Milvasoft.SampleAPI.DTOs.StudentDTOs;
using Milvasoft.SampleAPI.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.DTOs.ProfessionDTOs
{

    /// <summary>
    /// Profession entity.
    /// </summary>
    public class ProfessionDTO : EducationEntityBase
    {

        /// <summary>
        /// Name of profession.
        /// </summary>
        public string Name { get; set; }



        /// <summary>
        /// Assignments of profession.
        /// </summary>
        public virtual List<AssignmentDTO> Assignments { get; set; }

        /// <summary>
        /// Mentor profession relationship.
        /// </summary>
        public virtual List<MentorProfessionDTO> MentorProfessions { get; set; }

        /// <summary>
        /// Students of profession.
        /// </summary>
        public virtual List<StudentDTO> Students { get; set; }

        /// <summary>
        /// Questions of profession.
        /// </summary>
        public virtual List<QuestionDTO> Questions { get; set; }

        /// <summary>
        /// Useful links of profession.
        /// </summary>
        public virtual List<UsefulLinkDTO> UsefulLinks { get; set; }
    }
}
