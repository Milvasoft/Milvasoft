using System;

namespace Milvasoft.SampleAPI.Entity
{

    /// <summary>
    /// Questions asked by students.
    /// </summary>
    public class Question : EducationEntityBase
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
        public virtual Profession Profession { get; set; }

        /// <summary>
        /// Id of the student who asked the question.
        /// </summary>
        public Guid StudentId { get; set; }

        /// <summary>
        /// Student of the asked question.
        /// </summary>
        public virtual Student Student { get; set; }

        /// <summary>
        /// Id of the mentor answering the question.
        /// </summary>
        public Guid? MentorId { get; set; }

        /// <summary>
        /// The mentor answering the question
        /// </summary>
        public virtual Mentor Mentor { get; set; }

    }
}
