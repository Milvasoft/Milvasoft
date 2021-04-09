using Milvasoft.SampleAPI.DTOs.QuestionDTOs;
using Milvasoft.SampleAPI.Spec;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Services.Abstract
{
    /// <summary>
    /// Question service inteface.
    /// </summary>
    public interface IQuestionService : IBaseService<QuestionSpec, AddQuestionDTO, UpdateQuestionDTO, QuestionForStudentDTO, QuestionForMentorDTO, QuestionForAdminDTO>
    {
        /// <summary>
        ///  Returns questions to display
        /// </summary>
        /// <returns></returns>
        Task<List<QuestionDTO>> GetWillShowQuestions();

    }
}
