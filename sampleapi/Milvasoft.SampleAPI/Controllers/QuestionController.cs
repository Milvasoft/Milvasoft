using Microsoft.AspNetCore.Mvc;
using Milvasoft.API.DTOs;
using Milvasoft.SampleAPI.DTOs.QuestionDTOs;
using Milvasoft.SampleAPI.Services.Abstract;
using Milvasoft.SampleAPI.Spec;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Controllers
{
    public class QuestionController : Controller
    {
        private readonly IQuestionService _questionService;

        public QuestionController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        [HttpGet("QuestionForStudent")]
        public async Task<IActionResult> GetStudentForStudent([FromBody] PaginationParamsWithSpec<QuestionSpec> paginationParams)
        {
            var questions = await _questionService.GetEntitiesForStudent(paginationParams.Spec).ConfigureAwait(false);
            return Ok(questions);
        }
        [HttpGet("WillShown")]
        public async Task<IActionResult> GetWillShownQuestions()
        {
            var questions = await _questionService.GetWillShowQuestions().ConfigureAwait(false);
            return Ok(questions);
        }
        [HttpPost("Question")]
        public async Task<IActionResult> AddQuestion(QuestionDTO newQuestion)
        {
            return Ok();
        }
    }
}
