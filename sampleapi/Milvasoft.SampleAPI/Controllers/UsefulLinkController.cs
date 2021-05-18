using Microsoft.AspNetCore.Mvc;
using Milvasoft.Helpers;
using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.DTOs.UsefulLinkDTOs;
using Milvasoft.SampleAPI.Services.Abstract;
using Milvasoft.SampleAPI.Spec;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Controllers
{
    /// <summary>
    /// Provided link operations.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1.0")]
    public class UsefulLinkController : Controller
    {
        private readonly IUsefulLinkService _userfulLinkService;

        /// <summary>
        /// Constructor of <c>UsefulLinkController</c>
        /// </summary>
        /// <param name="usefulLinkService"></param>
        public UsefulLinkController(IUsefulLinkService usefulLinkService)
        {
            _userfulLinkService = usefulLinkService;
        }

        /// <summary>
        /// Gets the all filtered usefulLinks datas for mentor.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Mentor")]
        [HttpPatch("Mentor")]
        public async Task<IActionResult> GetUsefulLinksForMentor([FromBody] PaginationParamsWithSpec<UsefulLinkSpec> paginationParams)
        {
            var usefulLinks = await _userfulLinkService.GetUsefulLinksForMentorAsync(paginationParams).ConfigureAwait(false);
            return usefulLinks.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the all filtered usefulLinks datas for admin.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Admin")]
        [HttpPatch("Admin")]
        public async Task<IActionResult> GetUsefulLinksForAdmin([FromBody] PaginationParamsWithSpec<UsefulLinkSpec> paginationParams)
        {
            var usefulLinks = await _userfulLinkService.GetUsefulLinksForAdminAsync(paginationParams).ConfigureAwait(false);
            return usefulLinks.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the all filtered usefulLinks datas for student.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Student")]
        [HttpPatch("Student")]
        public async Task<IActionResult> GetUsefulLinksForStudent([FromBody] PaginationParamsWithSpec<UsefulLinkSpec> paginationParams)
        {
            var usefulLinks = await _userfulLinkService.GetUsefulLinksForStudentAsync(paginationParams).ConfigureAwait(false);
            return usefulLinks.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the all filtered usefulLink datas for mentor.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Mentor")]
        [HttpPatch("Mentor/{id}")]
        public async Task<IActionResult> GetUsefulLinkForMentorbyId(Guid id)
        {
            var usefulLink = await _userfulLinkService.GetUsefulLinkForMentorAsync(id).ConfigureAwait(false);

            return usefulLink.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the filtered usefulLink datas for admin.
        /// </summary>
        /// <returns></returns>
        ///[Authorize(Roles = "Admin")]
        [HttpPatch("Admin/{id}")]
        public async Task<IActionResult> GetUsefulLinkAdminbyId(Guid id)
        {
            var usefulLink = await _userfulLinkService.GetUsefulLinkForAdminAsync(id).ConfigureAwait(false);

            return usefulLink.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the filtered usefulLink datas for student.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Student")]
        [HttpPatch("Student/{id}")]
        public async Task<IActionResult> GetUsefulLinkForStudentbyId(Guid id)
        {
            var usefulLink = await _userfulLinkService.GetUsefulLinkForStudentAsync(id).ConfigureAwait(false);

            return usefulLink.GetObjectResponse("Success");
        }

        /// <summary>
        /// Add <b><paramref name="addStudent"/></b> data to database.
        /// </summary>
        /// <param name="addStudent"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpPost("UsefulLink")]
        public async Task<IActionResult> AddUsefulLink([FromBody] AddUsefulLinkDTO addStudent)
        {
            return await _userfulLinkService.AddUsefulLinkAsync(addStudent).ConfigureAwait(false).GetObjectResponseAsync<AddUsefulLinkDTO>("Success");
        }

        /// <summary>
        /// Update <paramref name="updateStudent"/> data.
        /// </summary>
        /// <param name="updateStudent"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpPut("UsefulLink")]
        public async Task<IActionResult> UpdateUsefulLink([FromBody] UpdateUsefulLinkDTO updateStudent)
        {
            return await _userfulLinkService.UpdateUsefulLinkAsync(updateStudent).ConfigureAwait(false).GetObjectResponseAsync<UpdateUsefulLinkDTO>("Success");
        }

        /// <summary>
        /// Delete usefulLink data by <paramref name="ids"/>
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteUsefulLinks([FromBody] List<Guid> ids)
        {
            return await _userfulLinkService.DeleteUsefulLinksAsync(ids).ConfigureAwait(false).GetObjectResponseAsync<UsefulLinkDTO, Guid>(ids, "Success");
        }

    }
}
