using Microsoft.AspNetCore.Mvc;
using Milvasoft.API.DTOs;
using Milvasoft.SampleAPI.DTOs.QuestionDTOs;
using Milvasoft.SampleAPI.Services.Abstract;
using Milvasoft.SampleAPI.Spec;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Controllers
{
    /// <summary>
    /// Provided Question operations.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class QuestionController : Controller
    {
        private readonly IQuestionService _questionService;

        /// <summary>
        /// Question controller.
        /// </summary>
        /// <param name="questionService"></param>
        public QuestionController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        [HttpPatch("QuestionForStudent")]
        public async Task<IActionResult> GetStudentForStudent([FromBody] PaginationParamsWithSpec<QuestionSpec> paginationParams)
        {
            var questions = await _questionService.GetEntitiesForStudentAsync(paginationParams.Spec).ConfigureAwait(false);
            return Ok(questions);
        }
        [HttpGet("WillShown")]
        public async Task<IActionResult> GetWillShownQuestions()
        {
            var questions = await _questionService.GetWillShowQuestions().ConfigureAwait(false);
            return Ok(questions);
        }
        [HttpPost("Question")]
        public async Task<IActionResult> AddQuestion([FromBody] AddQuestionDTO newQuestion)
        {
            await _questionService.AddEntityAsync(newQuestion).ConfigureAwait(false);
            return Ok();
        }
    }
}
