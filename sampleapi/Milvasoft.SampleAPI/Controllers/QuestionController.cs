using Microsoft.AspNetCore.Mvc;
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
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1.0")]
    public class QuestionController : Controller
    {
        private readonly IQuestionService _questionService;

        /// <summary>
        /// Constructor of <c>QuestionController</c>
        /// </summary>
        /// <param name="questionService"></param>
        public QuestionController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        /// <summary>
        /// Gets the all filtered questions datas for mentor.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Mentor")]
        [HttpPatch("Question/Mentor")]
        public async Task<IActionResult> GetQuestionsForMentor([FromBody] PaginationParamsWithSpec<QuestionSpec> paginationParams)
        {
            var questions = await _questionService.GetEntitiesForMentorAsync(paginationParams.PageIndex,
                                                                                 paginationParams.RequestedItemCount,
                                                                                 paginationParams.OrderByProperty,
                                                                                 paginationParams.OrderByAscending,
                                                                                 paginationParams.Spec).ConfigureAwait(false);
            return Ok(questions);
        }

        /// <summary>
        /// Gets the all filtered questions datas for admin.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Admin")]
        [HttpPatch("Question/Admin")]
        public async Task<IActionResult> GetQuestionsForAdmin([FromBody] PaginationParamsWithSpec<QuestionSpec> paginationParams)
        {
            var questions = await _questionService.GetEntitiesForAdminAsync(paginationParams.PageIndex,
                                                                                 paginationParams.RequestedItemCount,
                                                                                 paginationParams.OrderByProperty,
                                                                                 paginationParams.OrderByAscending,
                                                                                 paginationParams.Spec).ConfigureAwait(false);
            return Ok(questions);
        }

        /// <summary>
        /// Gets the all filtered questions datas for student.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Student")]
        [HttpPatch("Question/Student")]
        public async Task<IActionResult> GetQuestionsForStudent([FromBody] PaginationParamsWithSpec<QuestionSpec> paginationParams)
        {
            var questions = await _questionService.GetEntitiesForStudentAsync(paginationParams.PageIndex,
                                                                                 paginationParams.RequestedItemCount,
                                                                                 paginationParams.OrderByProperty,
                                                                                 paginationParams.OrderByAscending,
                                                                                 paginationParams.Spec).ConfigureAwait(false);
            return Ok(questions);
        }

        /// <summary>
        /// Gets the all filtered question datas for mentor.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Mentor")]
        [HttpPatch("Question/Mentor/{id}")]
        public async Task<IActionResult> GetQuestionForMentorbyId([FromBody] Guid id)
        {
            var question = await _questionService.GetEntityForMentorAsync(id).ConfigureAwait(false);

            return Ok(question);
        }

        /// <summary>
        /// Gets the filtered question datas for admin.
        /// </summary>
        /// <returns></returns>
        ///[Authorize(Roles = "Admin")]
        [HttpPatch("Question/Admin/{id}")]
        public async Task<IActionResult> GetQuestionForAdminbyId([FromBody] Guid id)
        {
            var question = await _questionService.GetEntityForAdminAsync(id).ConfigureAwait(false);

            return Ok(question);
        }

        /// <summary>
        /// Gets the filtered question datas for student.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Student")]
        [HttpPatch("Question/Student/{id}")]
        public async Task<IActionResult> GetQuestionForStudentbyId([FromBody] Guid id)
        {
            var question = await _questionService.GetEntityForStudentAsync(id).ConfigureAwait(false);

            return Ok(question);
        }

        /// <summary>
        /// Add <b><paramref name="addQuestion"/></b> data to database.
        /// </summary>
        /// <param name="addQuestion"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpPost("AddQuestions")]
        public async Task<IActionResult> AddQuestion([FromBody] AddQuestionDTO addQuestion)
        {
            await _questionService.AddEntityAsync(addQuestion).ConfigureAwait(false);
            return Ok();
        }

        /// <summary>
        /// Update <paramref name="updateQuestion"/> data.
        /// </summary>
        /// <param name="updateQuestion"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpPut("UpdateQuestion")]
        public async Task<IActionResult> UpdateQuestion([FromBody] UpdateQuestionDTO updateQuestion)
        {
            await _questionService.UpdateEntityAsync(updateQuestion).ConfigureAwait(false);
            return Ok();
        }

        /// <summary>
        /// Delete question data by <paramref name="id"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpDelete("Question/Delete/{id}")]
        public async Task<IActionResult> DeleteQuestions(Guid id)
        {
            await _questionService.DeleteEntityAsync(id).ConfigureAwait(false);
            return Ok();
        }

        /// <summary>
        /// Delete professions data by <paramref name="ids"/>
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpDelete("Question/Deletes/{ids}")]
        public async Task<IActionResult> DeleteQuestions(List<Guid> ids)
        {
            await _questionService.DeleteEntitiesAsync(ids).ConfigureAwait(false);
            return Ok();
        }
    }
}
