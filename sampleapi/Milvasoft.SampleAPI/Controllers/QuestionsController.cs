using Microsoft.AspNetCore.Mvc;
using Milvasoft.Helpers;
using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.DTOs.QuestionDTOs;
using Milvasoft.SampleAPI.Services.Abstract;
using Milvasoft.SampleAPI.Spec;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Controllers
{
    /// <summary>
    /// Provided Question operations.
    /// </summary>
    [Route("sampleapi/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1.0")]
    public class QuestionsController : Controller
    {
        private readonly IQuestionService _questionService;

        /// <summary>
        /// Constructor of <c>QuestionController</c>
        /// </summary>
        /// <param name="questionService"></param>
        public QuestionsController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        /// <summary>
        /// Gets the all filtered questions datas for mentor.
        /// </summary>
        /// <returns></returns>
        [HttpPatch("Mentor")]
        public async Task<IActionResult> GetQuestionsForMentor([FromBody] PaginationParamsWithSpec<QuestionSpec> paginationParams)
        {
            var questions = await _questionService.GetQuestionsForMentorAsync(paginationParams).ConfigureAwait(false);

            return questions.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the all filtered questions datas for admin.
        /// </summary>
        /// <returns></returns>
        [HttpPatch("Admin")]
        public async Task<IActionResult> GetQuestionsForAdmin([FromBody] PaginationParamsWithSpec<QuestionSpec> paginationParams)
        {
            var questions = await _questionService.GetQuestionsForAdminAsync(paginationParams).ConfigureAwait(false);

            return questions.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the all filtered questions datas for student.
        /// </summary>
        /// <returns></returns>
        [HttpPatch("Student")]
        public async Task<IActionResult> GetQuestionsForStudent([FromBody] PaginationParamsWithSpec<QuestionSpec> paginationParams)
        {
            var questions = await _questionService.GetQuestionsForStudentAsync(paginationParams).ConfigureAwait(false);

            return questions.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the all filtered question datas for mentor.
        /// </summary>
        /// <returns></returns>
        [HttpPatch("Mentor/{id}")]
        public async Task<IActionResult> GetQuestionForMentorbyId(Guid id)
        {
            var question = await _questionService.GetQuestionForMentorAsync(id).ConfigureAwait(false);

            return question.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the filtered question datas for admin.
        /// </summary>
        /// <returns></returns>
        ///[Authorize(Roles = "Admin")]
        [HttpPatch("Admin/{id}")]
        public async Task<IActionResult> GetQuestionForAdminbyId(Guid id)
        {
            var question = await _questionService.GetQuestionForAdminAsync(id).ConfigureAwait(false);

            return question.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the filtered question datas for student.
        /// </summary>
        /// <returns></returns>
        [HttpPatch("Student/{id}")]
        public async Task<IActionResult> GetQuestionForStudentbyId(Guid id)
        {
            var question = await _questionService.GetQuestionForStudentAsync(id).ConfigureAwait(false);

            return question.GetObjectResponse("Success");
        }

        /// <summary>
        /// Add <b><paramref name="addQuestion"/></b> data to database.
        /// </summary>
        /// <param name="addQuestion"></param>
        /// <returns></returns>
        [HttpPost("Question")]
        public async Task<IActionResult> AddQuestion([FromBody] AddQuestionDTO addQuestion)
        {
            return await _questionService.AddQuestionAsync(addQuestion).ConfigureAwait(false).GetObjectResponseAsync<AddQuestionDTO>("Success").ConfigureAwait(false);
        }

        /// <summary>
        /// Update <paramref name="updateQuestion"/> data.
        /// </summary>
        /// <param name="updateQuestion"></param>
        /// <returns></returns>
        [HttpPut("Question")]
        public async Task<IActionResult> UpdateQuestion([FromBody] UpdateQuestionDTO updateQuestion)
        {
            return await _questionService.UpdateQuestionAsync(updateQuestion).ConfigureAwait(false).GetObjectResponseAsync<UpdateQuestionDTO>("Success").ConfigureAwait(false);
        }

        /// <summary>
        /// Delete professions data by <paramref name="ids"/>
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteQuestions([FromBody] List<Guid> ids)
        {
            return await _questionService.DeleteQuestionsAsync(ids).ConfigureAwait(false).GetObjectResponseAsync<QuestionDTO, Guid>(ids, "Success");
        }
    }
}
