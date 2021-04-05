using Microsoft.AspNetCore.Mvc;
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
        [HttpPatch("Link/Mentor")]
        public async Task<IActionResult> GetUsefulLinksForMentor([FromBody] PaginationParamsWithSpec<UsefulLinkSpec> paginationParams)
        {
            var usefulLinks = await _userfulLinkService.GetEntitiesForMentorAsync(paginationParams.PageIndex,
                                                                                 paginationParams.RequestedItemCount,
                                                                                 paginationParams.OrderByProperty,
                                                                                 paginationParams.OrderByAscending,
                                                                                 paginationParams.Spec).ConfigureAwait(false);
            return Ok(usefulLinks);
        }

        /// <summary>
        /// Gets the all filtered usefulLinks datas for admin.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Admin")]
        [HttpPatch("Link/Admin")]
        public async Task<IActionResult> GetUsefulLinksForAdmin([FromBody] PaginationParamsWithSpec<UsefulLinkSpec> paginationParams)
        {
            var usefulLinks = await _userfulLinkService.GetEntitiesForAdminAsync(paginationParams.PageIndex,
                                                                                 paginationParams.RequestedItemCount,
                                                                                 paginationParams.OrderByProperty,
                                                                                 paginationParams.OrderByAscending,
                                                                                 paginationParams.Spec).ConfigureAwait(false);
            return Ok(usefulLinks);
        }

        /// <summary>
        /// Gets the all filtered usefulLinks datas for student.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Student")]
        [HttpPatch("Link/Student")]
        public async Task<IActionResult> GetUsefulLinksForStudent([FromBody] PaginationParamsWithSpec<UsefulLinkSpec> paginationParams)
        {
            var usefulLinks = await _userfulLinkService.GetEntitiesForStudentAsync(paginationParams.PageIndex,
                                                                                 paginationParams.RequestedItemCount,
                                                                                 paginationParams.OrderByProperty,
                                                                                 paginationParams.OrderByAscending,
                                                                                 paginationParams.Spec).ConfigureAwait(false);
            return Ok(usefulLinks);
        }

        /// <summary>
        /// Gets the all filtered usefulLink datas for mentor.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Mentor")]
        [HttpPatch("Link/Mentor/{id}")]
        public async Task<IActionResult> GetUsefulLinkForMentorbyId([FromBody] Guid id)
        {
            var usefulLink = await _userfulLinkService.GetEntityForMentorAsync(id).ConfigureAwait(false);

            return Ok(usefulLink);
        }

        /// <summary>
        /// Gets the filtered usefulLink datas for admin.
        /// </summary>
        /// <returns></returns>
        ///[Authorize(Roles = "Admin")]
        [HttpPatch("Link/Admin/{id}")]
        public async Task<IActionResult> GetUsefulLinkAdminbyId([FromBody] Guid id)
        {
            var usefulLink = await _userfulLinkService.GetEntityForAdminAsync(id).ConfigureAwait(false);

            return Ok(usefulLink);
        }

        /// <summary>
        /// Gets the filtered usefulLink datas for student.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Student")]
        [HttpPatch("Link/Student/{id}")]
        public async Task<IActionResult> GetUsefulLinkForStudentbyId([FromBody] Guid id)
        {
            var usefulLink = await _userfulLinkService.GetEntityForStudentAsync(id).ConfigureAwait(false);

            return Ok(usefulLink);
        }

        /// <summary>
        /// Add <b><paramref name="addStudent"/></b> data to database.
        /// </summary>
        /// <param name="addStudent"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpPost("AddUsefulLink")]
        public async Task<IActionResult> AddUsefulLink([FromBody] AddUsefulLinkDTO addStudent)
        {
            await _userfulLinkService.AddEntityAsync(addStudent).ConfigureAwait(false);
            return Ok();
        }

        /// <summary>
        /// Update <paramref name="updateStudent"/> data.
        /// </summary>
        /// <param name="updateStudent"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpPut("UpdateUsefulLink")]
        public async Task<IActionResult> UpdateUsefulLink([FromBody] UpdateUsefulLinkDTO updateStudent)
        {
            await _userfulLinkService.UpdateEntityAsync(updateStudent).ConfigureAwait(false);
            return Ok();
        }

        /// <summary>
        /// Delete usefulLink data by <paramref name="id"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpDelete("Link/Delete/{id}")]
        public async Task<IActionResult> DeleteUsefulLink(Guid id)
        {
            await _userfulLinkService.DeleteEntityAsync(id).ConfigureAwait(false);
            return Ok();
        }

        /// <summary>
        /// Delete usefulLink data by <paramref name="ids"/>
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpDelete("Link/Deletes/{ids}")]
        public async Task<IActionResult> DeleteUsefulLinks(List<Guid> ids)
        {
            await _userfulLinkService.DeleteEntitiesAsync(ids).ConfigureAwait(false);
            return Ok();
        }

    }
}
