using Milvasoft.Helpers.DataAccess.Concrete.Entity;
using Milvasoft.SampleAPI.DTOs.MentorDTOs;
using Milvasoft.SampleAPI.DTOs.StudentDTOs;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using Milvasoft.SampleAPI.Utils.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.DTOs.QuestionDTOs
{
    /// <summary>
    /// Question entities for student.
    /// </summary>
    public class QuestionForStudentDTO : AuditableEntity<AppUser, Guid, Guid>
    {
        /// <summary>
        /// Tittle of question.
        /// </summary>
        [OValidateString(2000)]
        public string Title { get; set; }

        /// <summary>
        /// Content of question.
        /// </summary>
        [OValidateString(2000)]
        public string QuestionContent { get; set; }

        /// <summary>
        /// The mentor's answer to the question.
        /// </summary>
        [OValidateString(2000)]
        public string MentorReply { get; set; }

        /// <summary>
        /// Profession ıd of the question.
        /// </summary>
        [OValidateId]
        public Guid ProfessionId { get; set; }

        /// <summary>
        /// Student of the asked question.
        /// </summary>
        [SwaggerExclude]
        public virtual StudentDTO Student { get; set; }

        /// <summary>
        /// The mentor answering the question
        /// </summary>
        [SwaggerExclude]
        public virtual MentorDTO Mentor { get; set; }
    }
}
