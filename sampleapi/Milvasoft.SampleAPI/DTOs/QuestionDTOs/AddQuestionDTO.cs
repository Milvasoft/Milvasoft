﻿using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using System;

namespace Milvasoft.SampleAPI.DTOs.QuestionDTOs
{
    /// <summary>
    /// AddQuestionDTO for add question operations.
    /// </summary>
    public class AddQuestionDTO
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
        [OValidateId]
        public Guid ProfessionId { get; set; }

        /// <summary>
        /// Id of the student who asked the question.
        /// </summary>
        [OValidateId]
        public Guid StudentId { get; set; }
    }
}
