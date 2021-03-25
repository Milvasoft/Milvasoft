using Milvasoft.SampleAPI.DTOs.QuestionDTOs;
using Milvasoft.SampleAPI.Spec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Services.Abstract
{
    public interface IQuestionService : IBaseService<QuestionDTO,QuestionSpec>
    {
        Task<List<QuestionDTO>> GetWillShowQuestions();
        
    }
}
