using Microsoft.AspNetCore.Mvc;
using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.DTOs.MentorDTOs;
using Milvasoft.SampleAPI.Services.Abstract;
using Milvasoft.SampleAPI.Spec;
using Milvasoft.SampleAPI.Utils.Attributes.ActionFilters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Controllers
{
    /// <summary>
    /// Provided Mentor operations.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1.0")]
    public class MentorController : Controller
    {
        private readonly IMentorService _mentorService;

        /// <summary>
        /// Mentor constructor method for injection.
        /// </summary>
        /// <param name="mentorService"></param>
        public MentorController(IMentorService mentorService)
        {
            _mentorService = mentorService;
        }

        /// <summary>
        /// Gets the all filtered mentors datas for mentor.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Mentor")]
        [HttpPatch("Mentor")]
        public async Task<IActionResult> GetMentorsForMentor([FromBody] PaginationParamsWithSpec<MentorSpec> paginationParams)
        {
            var mentors = await _mentorService.GetEntitiesForMentorAsync(paginationParams).ConfigureAwait(false);
            return Ok(mentors);
        }

        /// <summary>
        /// Gets the all filtered mentors datas for admin.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Admin")]
        [HttpPatch("Admin")]
        public async Task<IActionResult> GetMentorsForAdmn([FromBody] PaginationParamsWithSpec<MentorSpec> paginationParams)
        {
            var mentors = await _mentorService.GetEntitiesForAdminAsync(paginationParams).ConfigureAwait(false);
            return Ok(mentors);
        }

        /// <summary>
        /// Gets the all filtered mentors datas for student.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Student")]
        [HttpPatch("Student")]
        public async Task<IActionResult> GetMentorsForStudent([FromBody] PaginationParamsWithSpec<MentorSpec> paginationParams)
        {
            var mentors = await _mentorService.GetEntitiesForStudentAsync(paginationParams).ConfigureAwait(false);
            return Ok(mentors);
        }

        /// <summary>
        /// Gets the all filtered mentor datas for mentor.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Mentor")]
        [HttpPatch("Mentor/{id}")]
        public async Task<IActionResult> GetMentorForMentorbyId(Guid id)
        {
            var mentor = await _mentorService.GetEntityForMentorAsync(id).ConfigureAwait(false);

            return Ok(mentor);
        }

        /// <summary>
        /// Gets the filtered mentor datas for admin.
        /// </summary>
        /// <returns></returns>
        ///[Authorize(Roles = "Admin")]
        [HttpPatch("Admin/{id}")]
        public async Task<IActionResult> GetMentorForAdminbyId([FromBody] Guid id)
        {
            var mentor = await _mentorService.GetEntityForAdminAsync(id).ConfigureAwait(false);

            return Ok(mentor);
        }

        /// <summary>
        /// Gets the filtered mentor datas for student.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Student")]
        [HttpPatch("Mentor/{id}/Students")]
        public async Task<IActionResult> GetMentorForStudentbyId([FromBody] Guid id)
        {
            var mentor = await _mentorService.GetEntityForStudentAsync(id).ConfigureAwait(false);

            return Ok(mentor);
        }

        /// <summary>
        /// Add <b><paramref name="addMentor"/></b> data to database.
        /// </summary>
        /// <param name="addMentor"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpPost("Mentor")]
        [OValidationFilter]
        public async Task<IActionResult> AddMentor([FromBody] AddMentorDTO addMentor)
        {
            await _mentorService.AddEntityAsync(addMentor).ConfigureAwait(false);
            return Ok();
        }

        /// <summary>
        /// Update <paramref name="updateMentor"/> data.
        /// </summary>
        /// <param name="updateMentor"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpPut("Mentor")]
        public async Task<IActionResult> UpdateMentor([FromBody] UpdateMentorDTO updateMentor)
        {
            await _mentorService.UpdateEntityAsync(updateMentor).ConfigureAwait(false);
            return Ok();
        }

        /// <summary>
        /// Delete mentor data by <paramref name="id"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMentor(Guid id)
        {
            await _mentorService.DeleteEntityAsync(id).ConfigureAwait(false);
            return Ok();
        }

        /// <summary>
        /// Delete mentor data by <paramref name="ids"/>
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpDelete]
        public async Task<IActionResult> DeleteMentors([FromBody] List<Guid> ids)
        {
            await _mentorService.DeleteEntitiesAsync(ids).ConfigureAwait(false);
            return Ok();
        }
    }
}
