using Microsoft.AspNetCore.Mvc;
using Milvasoft.Helpers;
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
    [ApiController]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1.0")]
    [Route("sampleapi/[controller]")]
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
        [HttpPatch("Mentor")]
        public async Task<IActionResult> GetAnnouncementsForMentor([FromBody] PaginationParamsWithSpec<AnnouncementSpec> paginationParams)
        {
            var announcements = await _announcementService.GetAnnouncementsForMentorAsync(paginationParams).ConfigureAwait(false);
            return announcements.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the all filtered announcements datas for admin.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Admin")]
        [HttpPatch("Admin")]
        public async Task<IActionResult> GetAnnouncementsForAdmn([FromBody] PaginationParamsWithSpec<AnnouncementSpec> paginationParams)
        {
            var announcements = await _announcementService.GetAnnouncementsForAdminAsync(paginationParams).ConfigureAwait(false);
            return announcements.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the all filtered announcements datas for student.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Student")]
        [HttpPatch("Student")]
        public async Task<IActionResult> GetAnnouncementsForStudent([FromBody] PaginationParamsWithSpec<AnnouncementSpec> paginationParams)
        {
            var announcements = await _announcementService.GetAnnouncementsForStudentAsync(paginationParams).ConfigureAwait(false);
            return announcements.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the all filtered announcement datas for mentor.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Mentor")]
        [HttpPatch("Mentor/{id}")]
        public async Task<IActionResult> GetAnnouncementForMentorbyId(Guid id)
        {
            var announcement = await _announcementService.GetAnnouncementForMentorAsync(id).ConfigureAwait(false);

            return announcement.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the filtered announcement datas for admin.
        /// </summary>
        /// <returns></returns>
        ///[Authorize(Roles = "Admin")]
        [HttpPatch("Admin/{id}")]
        public async Task<IActionResult> GetAnnouncementForAdminbyId(Guid id)
        {
            var announcement = await _announcementService.GetAnnouncementForAdminAsync(id).ConfigureAwait(false);

            return announcement.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the filtered announcement datas for student.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Student")]
        [HttpPatch("Student/{id}")]
        public async Task<IActionResult> GetAnnouncementForStudentbyId(Guid id)
        {
            var announcement = await _announcementService.GetAnnouncementForStudentAsync(id).ConfigureAwait(false);

            return announcement.GetObjectResponse("Success");
        }

        /// <summary>
        /// Add <b><paramref name="addAnnouncement"/></b> data to database.
        /// </summary>
        /// <param name="addAnnouncement"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpPost("Announcement")]
        public async Task<IActionResult> AddAnnouncement([FromBody] AddAnnouncementDTO addAnnouncement)
        {
            return await _announcementService.AddAnnouncementAsync(addAnnouncement).ConfigureAwait(false).GetObjectResponseAsync<AddAnnouncementDTO>("Success").ConfigureAwait(false);
        }

        /// <summary>
        /// Update <paramref name="updateAnnouncement"/> data.
        /// </summary>
        /// <param name="updateAnnouncement"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpPut("Announcement")]
        public async Task<IActionResult> UpdateAnnouncement([FromBody] UpdateAnnouncementDTO updateAnnouncement)
        {
            return await _announcementService.UpdateAnnouncementAsync(updateAnnouncement).ConfigureAwait(false).GetObjectResponseAsync<UpdateAnnouncementDTO>("Success").ConfigureAwait(false);
        }

        /// <summary>
        /// Delete announcement data by <paramref name="ids"/>
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteAnnouncements([FromBody] List<Guid> ids)
        {
            return await _announcementService.DeleteAnnouncementsAsync(ids).ConfigureAwait(false).GetObjectResponseAsync<AnnouncementDTO, Guid>(ids, "Success").ConfigureAwait(false);
        }

    }
}
