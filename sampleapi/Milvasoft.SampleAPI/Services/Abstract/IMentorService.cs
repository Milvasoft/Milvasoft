using Milvasoft.Helpers.Models;
using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.DTOs.MentorDTOs;
using Milvasoft.SampleAPI.Spec;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Services.Abstract
{
    /// <summary>
    /// Mentor service interface.
    /// </summary>
    public interface IMentorService
    {
        /// <summary>
        /// Get all assignment for admin from database.
        /// </summary>
        /// <returns></returns>
        Task<PaginationDTO<MentorForAdminDTO>> GetMentorsForAdminAsync(PaginationParamsWithSpec<MentorSpec> paginationParams);

        /// <summary>
        /// Fetches filtered assignment by id for admin.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<MentorForAdminDTO> GetMentorForAdminAsync(Guid id);

        /// <summary>
        /// Brings instant user's profile information.
        /// </summary>
        /// <returns></returns>
        Task<MentorForMentorDTO> GetCurrentUserProfile();

        /// <summary>
        /// Add assignment to database.
        /// </summary>
        /// <param name="addMentorDTO"></param>
        /// <returns></returns>
        Task AddMentorAsync(AddMentorDTO addMentorDTO);

        /// <summary>
        /// Updates assignment in database.
        /// </summary>
        /// <param name="updateMentorDTO"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        Task UpdateMentorByAdminAsync(UpdateMentorDTO updateMentorDTO, Guid Id);

        /// <summary>
        /// The mentor can update himself.
        /// </summary>
        /// <param name="updateMentorDTO"></param>
        /// <returns></returns>
        Task UpdateCurrentMentorAsync(UpdateMentorDTO updateMentorDTO);


        /// <summary>
        /// Delete assignment by <paramref name="ids"/>
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task DeleteMentorsAsync(List<Guid> ids);
    }
}
