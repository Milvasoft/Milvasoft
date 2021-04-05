using Microsoft.AspNetCore.Mvc;
using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.DTOs.AnnouncementDTOs;
using Milvasoft.SampleAPI.Services.Abstract;
using Milvasoft.SampleAPI.Spec;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Controllers
{
    /// <summary>
    /// Provided announcement operations.
    /// </summary>
    public class AnnouncementController : Controller
    {
        private readonly IAnnouncementService _announcementService;

        /// <summary>
        /// Constructor of <c>AnnouncementController</c>.
        /// </summary>
        /// <param name="announcementService"></param>
        public AnnouncementController(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        /// <summary>
        /// Gets the all filtered announcements datas for mentor.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Mentor")]
        [HttpPatch("Announcement/Mentor")]
        public async Task<IActionResult> GetAnnouncementsForMentor([FromBody] PaginationParamsWithSpec<AnnouncementSpec> paginationParams)
        {
            var announcements = await _announcementService.GetEntitiesForMentorAsync(paginationParams.PageIndex,
                                                                                     paginationParams.RequestedItemCount,
                                                                                     paginationParams.OrderByProperty,
                                                                                     paginationParams.OrderByAscending,
                                                                                     paginationParams.Spec).ConfigureAwait(false);
            return Ok(announcements);
        }

        /// <summary>
        /// Gets the all filtered announcements datas for admin.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Admin")]
        [HttpPatch("Announcement/Admin")]
        public async Task<IActionResult> GetAnnouncementsForAdmn([FromBody] PaginationParamsWithSpec<AnnouncementSpec> paginationParams)
        {
            var announcements = await _announcementService.GetEntitiesForAdminAsync(paginationParams.PageIndex,
                                                                                 paginationParams.RequestedItemCount,
                                                                                 paginationParams.OrderByProperty,
                                                                                 paginationParams.OrderByAscending,
                                                                                 paginationParams.Spec).ConfigureAwait(false);
            return Ok(announcements);
        }

        /// <summary>
        /// Gets the all filtered announcements datas for student.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Student")]
        [HttpPatch("Announcement/Student")]
        public async Task<IActionResult> GetAnnouncementsForStudent([FromBody] PaginationParamsWithSpec<AnnouncementSpec> paginationParams)
        {
            var announcements = await _announcementService.GetEntitiesForStudentAsync(paginationParams.PageIndex,
                                                                                 paginationParams.RequestedItemCount,
                                                                                 paginationParams.OrderByProperty,
                                                                                 paginationParams.OrderByAscending,
                                                                                 paginationParams.Spec).ConfigureAwait(false);
            return Ok(announcements);
        }

        /// <summary>
        /// Gets the all filtered announcement datas for mentor.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Mentor")]
        [HttpPatch("Announcement/Mentor/{id}")]
        public async Task<IActionResult> GetAnnouncementForMentorbyId([FromBody] Guid id)
        {
            var announcement = await _announcementService.GetEntityForMentorAsync(id).ConfigureAwait(false);

            return Ok(announcement);
        }

        /// <summary>
        /// Gets the filtered announcement datas for admin.
        /// </summary>
        /// <returns></returns>
        ///[Authorize(Roles = "Admin")]
        [HttpPatch("Announcement/Admin/{id}")]
        public async Task<IActionResult> GetAnnouncementForAdminbyId([FromBody] Guid id)
        {
            var announcement = await _announcementService.GetEntityForAdminAsync(id).ConfigureAwait(false);

            return Ok(announcement);
        }

        /// <summary>
        /// Gets the filtered announcement datas for student.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Student")]
        [HttpPatch("Announcement/Student/{id}")]
        public async Task<IActionResult> GetAnnouncementForStudentbyId([FromBody] Guid id)
        {
            var announcement = await _announcementService.GetEntityForStudentAsync(id).ConfigureAwait(false);

            return Ok(announcement);
        }

        /// <summary>
        /// Add <b><paramref name="addAnnouncement"/></b> data to database.
        /// </summary>
        /// <param name="addAnnouncement"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpPost("AddAnnouncements")]
        public async Task<IActionResult> AddAnnouncement([FromBody] AddAnnouncementDTO addAnnouncement)
        {
            await _announcementService.AddEntityAsync(addAnnouncement).ConfigureAwait(false);
            return Ok();
        }

        /// <summary>
        /// Update <paramref name="updateAnnouncement"/> data.
        /// </summary>
        /// <param name="updateAnnouncement"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpPut("UpdateAnnouncement")]
        public async Task<IActionResult> UpdateAnnouncement([FromBody] UpdateAnnouncementDTO updateAnnouncement)
        {
            await _announcementService.UpdateEntityAsync(updateAnnouncement).ConfigureAwait(false);
            return Ok();
        }

        /// <summary>
        /// Delete announcement data by <paramref name="id"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpDelete("Announcement/Delete/{id}")]
        public async Task<IActionResult> DeleteAnnouncement(Guid id)
        {
            await _announcementService.DeleteEntityAsync(id).ConfigureAwait(false);
            return Ok();
        }

        /// <summary>
        /// Delete announcement data by <paramref name="ids"/>
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpDelete("Announcement/Deletes/{ids}")]
        public async Task<IActionResult> DeleteAnnouncements(List<Guid> ids)
        {
            await _announcementService.DeleteEntitiesAsync(ids).ConfigureAwait(false);
            return Ok();
        }
    }
}
