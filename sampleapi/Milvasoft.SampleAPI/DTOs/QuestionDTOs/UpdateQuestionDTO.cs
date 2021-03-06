﻿using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using System;

namespace Milvasoft.SampleAPI.DTOs.QuestionDTOs
{
    /// <summary>
    /// UpdateQuestionDTO for update question operations.
    /// </summary>
    public class UpdateQuestionDTO
    {
        /// <summary>
        /// Id of to be updated question.
        /// </summary>
        public Guid Id { get; set; }

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
    }
}
