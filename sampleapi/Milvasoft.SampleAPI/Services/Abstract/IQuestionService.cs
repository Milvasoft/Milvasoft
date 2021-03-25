using Milvasoft.SampleAPI.DTOs.QuestionDTOs;
using Milvasoft.SampleAPI.Spec;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Services.Abstract
{
    public interface IQuestionService : IBaseService<QuestionDTO, QuestionSpec>
    {
        Task<List<QuestionDTO>> GetWillShowQuestions();

    }
}
