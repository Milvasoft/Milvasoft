using Milvasoft.SampleAPI.DTOs.MentorDTOs;
using Milvasoft.SampleAPI.DTOs.ProfessionDTOs;
using Milvasoft.SampleAPI.DTOs.StudentDTOs;
using Milvasoft.SampleAPI.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.DTOs.QuestionDTOs
{

    /// <summary>
    /// Questions asked by students.
    /// </summary>
    public class QuestionDTO : EducationEntityBase
    {

        /// <summary>
        /// Tittle of question.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Content of question.
        /// </summary>
        public string QuestionContent { get; set; }

        /// <summary>
        /// The mentor's answer to the question.
        /// </summary>
        public string MentorReply { get; set; }

        /// <summary>
        /// Is the question useful?
        /// </summary>
        public bool IsUseful { get; set; }

        /// <summary>
        /// Will the question be shown as a useful question?
        /// </summary>
        public bool WillShown { get; set; }

        /// <summary>
        /// Profession ıd of the question.
        /// </summary>
        public Guid ProfessionId { get; set; }

        /// <summary>
        /// Profession of the question.
        /// </summary>
        public virtual ProfessionDTO Profession { get; set; }

        /// <summary>
        /// Id of the student who asked the question.
        /// </summary>
        public Guid StudentId { get; set; }

        /// <summary>
        /// Student of the asked question.
        /// </summary>
        public virtual StudentDTO Student { get; set; }

        /// <summary>
        /// Id of the mentor answering the question.
        /// </summary>
        public Guid? MentorId { get; set; }

        /// <summary>
        /// The mentor answering the question
        /// </summary>
        public virtual MentorDTO Mentor { get; set; }
    }
}
