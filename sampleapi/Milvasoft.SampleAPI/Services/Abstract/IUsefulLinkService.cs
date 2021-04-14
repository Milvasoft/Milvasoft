using Milvasoft.Helpers.Models;
using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.DTOs.UsefulLinkDTOs;
using Milvasoft.SampleAPI.Spec;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Services.Abstract
{
    /// <summary>
    /// UsefulLink service inteface.
    /// </summary>
    public interface IUsefulLinkService
    {
        /// <summary>
        /// Get all useful links for student from database.
        /// </summary>
        /// <returns></returns>
        Task<PaginationDTO<UsefulLinkDTO>> GetUsefulLinksForStudentAsync(PaginationParamsWithSpec<UsefulLinkSpec> paginationParams);

        /// <summary>
        /// Get all useful links for admin from database.
        /// </summary>
        /// <returns></returns>
        Task<PaginationDTO<UsefulLinkDTO>> GetUsefulLinksForAdminAsync(PaginationParamsWithSpec<UsefulLinkSpec> paginationParams);

        /// <summary>
        /// Get all useful links for mentor from database.
        /// </summary>
        /// <returns></returns>
        Task<PaginationDTO<UsefulLinkDTO>> GetUsefulLinksForMentorAsync(PaginationParamsWithSpec<UsefulLinkSpec> paginationParams);

        /// <summary>
        /// Fetches filtered useful link by id for student.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<UsefulLinkDTO> GetUsefulLinkForStudentAsync(Guid id);

        /// <summary>
        /// Fetches filtered useful link by id for admin.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<UsefulLinkDTO> GetUsefulLinkForAdminAsync(Guid id);

        /// <summary>
        /// Fetches filtered useful link by id for mentor.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<UsefulLinkDTO> GetUsefulLinkForMentorAsync(Guid id);

        /// <summary>
        /// Add useful link to database.
        /// </summary>
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        Task AddUsefulLinkAsync(AddUsefulLinkDTO educationDTO);

        /// <summary>
        /// Updates useful link in database.
        /// </summary>
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        Task UpdateUsefulLinkAsync(UpdateUsefulLinkDTO educationDTO);

        /// <summary>
        /// Delete useful links by <paramref name="ids"/>
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task DeleteUsefulLinksAsync(List<Guid> ids);
    }
}
