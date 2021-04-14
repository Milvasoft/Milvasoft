using Milvasoft.Helpers.Models;
using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.DTOs.AnnouncementDTOs;
using Milvasoft.SampleAPI.Spec;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Services.Abstract
{
    /// <summary>
    /// Announcement service interface.
    /// </summary>
    public interface IAnnouncementService
    {
        /// <summary>
        /// Get all announcement for admin from database.
        /// </summary>
        /// <returns></returns>
        Task<PaginationDTO<AnnouncementForAdminDTO>> GetAnnouncementForAdminAsync(PaginationParamsWithSpec<AnnouncementSpec> paginationParams);

        /// <summary>
        /// Get all announcement for mentor from database.
        /// </summary>
        /// <returns></returns>
        Task<PaginationDTO<AnnouncementForMentorDTO>> GetAnnouncementForMentorAsync(PaginationParamsWithSpec<AnnouncementSpec> paginationParams);

        /// <summary>
        /// Get all announcement for mentor from database.
        /// </summary>
        /// <returns></returns>
        Task<PaginationDTO<AnnouncementForStudentDTO>> GetAnnouncementForStudentAsync(PaginationParamsWithSpec<AnnouncementSpec> paginationParams);

        /// <summary>
        /// Fetches filtered announcement by id for student.
        /// </summary>
        /// <param name="id">Student id.</param>
        /// <returns></returns>
        Task<AnnouncementForStudentDTO> GetAnnouncementForStudentAsync(Guid id);

        /// <summary>
        /// Fetches filtered announcement by id for admin.
        /// </summary>
        /// <param name="id">Student id.</param>
        /// <returns></returns>
        Task<AnnouncementForAdminDTO> GetAnnouncementForAdminAsync(Guid id);

        /// <summary>
        /// Fetches filtered announcement by id for mentor.
        /// </summary>
        /// <param name="id">Student id.</param>
        /// <returns></returns>
        Task<AnnouncementForMentorDTO> GetAnnouncementForMentorAsync(Guid id);

        /// <summary>
        /// Add announcement to database.
        /// </summary>
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        Task AddAnnouncementAsync(AddAnnouncementDTO educationDTO);

        /// <summary>
        /// Updates single entity in database.
        /// </summary>
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        Task UpdateAnnouncementAsync(UpdateAnnouncementDTO educationDTO);

        /// <summary>
        /// Delete entities by <paramref name="ids"/>
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task DeleteAnnouncementsAsync(List<Guid> ids);
    }
}
