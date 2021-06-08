using Microsoft.AspNetCore.Mvc;
using Milvasoft.Helpers;
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
    [ApiController]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1.0")]
    [Route("sampleapi/[controller]")]
    public class MentorsController : Controller
    {
        private readonly IMentorService _mentorService;

        /// <summary>
        /// Mentor constructor method for injection.
        /// </summary>
        /// <param name="mentorService"></param>
        public MentorsController(IMentorService mentorService)
        {
            _mentorService = mentorService;
        }

        /// <summary>
        /// Gets the all filtered mentors datas for admin.
        /// </summary>
        /// <returns></returns>
        [HttpPatch("Admin")]
        public async Task<IActionResult> GetMentorsForAdmin([FromBody] PaginationParamsWithSpec<MentorSpec> paginationParams)
        {
            var mentors = await _mentorService.GetMentorsForAdminAsync(paginationParams).ConfigureAwait(false);

            return mentors.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the filtered mentor datas for admin.
        /// </summary>
        /// <returns></returns>
        ///[Authorize(Roles = "Admin")]
        [HttpPatch("Admin/{id}")]
        public async Task<IActionResult> GetMentorForAdminbyId(Guid id)
        {
            var mentor = await _mentorService.GetMentorForAdminAsync(id).ConfigureAwait(false);

            return mentor.GetObjectResponse("Success");
        }

        /// <summary>
        /// It brings the information of the mentor logged in.
        /// </summary>
        /// <returns></returns>
        [HttpPatch("Mentor")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var mentor = await _mentorService.GetCurrentUserProfileAsync().ConfigureAwait(false);

            return mentor.GetObjectResponse("Success");
        }

        /// <summary>
        /// Add <b><paramref name="addMentor"/></b> data to database.
        /// </summary>
        /// <param name="addMentor"></param>
        /// <returns></returns>
        [HttpPost("Mentor")]
        [OValidationFilter]
        public async Task<IActionResult> AddMentor([FromBody] AddMentorDTO addMentor)
        {
            return await _mentorService.AddMentorAsync(addMentor).ConfigureAwait(false).GetObjectResponseAsync<AddMentorDTO>("Success");
        }

        /// <summary>
        /// The mentor can be updated by admin with <paramref name="Id"/>
        /// </summary>
        /// <param name="updateMentor"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPut("Mentor")]
        public async Task<IActionResult> UpdateMentorbyAdmin([FromBody] UpdateMentorDTO updateMentor, Guid Id)
        {
            return await _mentorService.UpdateMentorByAdminAsync(updateMentor, Id).ConfigureAwait(false).GetObjectResponseAsync<UpdateMentorDTO>("Success"); ;
        }

        /// <summary>
        ///  It allows the logged in mentor to update his / her information.
        /// </summary>
        /// <param name="updateMentor"></param>
        /// <returns></returns>
        [HttpPut("CurrentMentor")]
        public async Task<IActionResult> UpdateCurrentMentor([FromBody] UpdateMentorDTO updateMentor)
        {
            return await _mentorService.UpdateCurrentMentorAsync(updateMentor).ConfigureAwait(false).GetObjectResponseAsync<UpdateMentorDTO>("Success");
        }

        /// <summary>
        /// Delete mentor data by <paramref name="ids"/>
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteMentors([FromBody] List<Guid> ids)
        {
            return await _mentorService.DeleteMentorsAsync(ids).ConfigureAwait(false).GetObjectResponseAsync<MentorDTO, Guid>(ids, "Success");
        }
    }
}
