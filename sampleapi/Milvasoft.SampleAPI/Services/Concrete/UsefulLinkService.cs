using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.Models;
using Milvasoft.SampleAPI.Data;
using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.DTOs.UsefulLinkDTOs;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Services.Abstract;
using Milvasoft.SampleAPI.Spec;
using Milvasoft.SampleAPI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Services.Concrete
{
    /// <summary>
    /// UsefulLink service.
    /// </summary>
    public class UsefulLinkService : IUsefulLinkService
    {
        private readonly IBaseRepository<UsefulLink, Guid, EducationAppDbContext> _usefulLinkRepository;

        /// <summary>
        /// Performs constructor injection for repository interfaces used in this service.
        /// </summary>
        /// <param name="usefulLinkRepository"></param>
        public UsefulLinkService(IBaseRepository<UsefulLink, Guid, EducationAppDbContext> usefulLinkRepository)
        {
            _usefulLinkRepository = usefulLinkRepository;
        }

        /// <summary>
        /// Get links for student.
        /// </summary>
        /// <returns></returns>
        public async Task<PaginationDTO<UsefulLinkDTO>> GetUsefulLinksForStudentAsync(PaginationParamsWithSpec<UsefulLinkSpec> pagiantionParams)
        {
            pagiantionParams.ThrowIfParameterIsNull();

            var (usefulLinks, pageCount, totalDataCount) = await _usefulLinkRepository.PreparePaginationDTO<UsefulLink, Guid>(pagiantionParams.PageIndex,
                                                                                                                              pagiantionParams.RequestedItemCount,
                                                                                                                              pagiantionParams.OrderByProperty,
                                                                                                                              pagiantionParams.OrderByAscending,
                                                                                                                              pagiantionParams.Spec?.ToExpression()).ConfigureAwait(false);

            return new PaginationDTO<UsefulLinkDTO>
            {
                DTOList = usefulLinks.CheckList(i => usefulLinks.Select(link => new UsefulLinkDTO
                {
                    Id = link.Id,
                    Title = link.Title,
                    Description = link.Description,
                    Url = link.Url,
                    ProfessionId = link.ProfessionId,
                    CreatorUser = link.CreatorUser
                })),
                PageCount = pageCount,
                TotalDataCount = totalDataCount
            };
        }

        /// <summary>
        /// Get links for admin.
        /// </summary>
        /// <returns></returns>
        public async Task<PaginationDTO<UsefulLinkDTO>> GetUsefulLinksForAdminAsync(PaginationParamsWithSpec<UsefulLinkSpec> pagiantionParams)
        {
            var (usefulLinks, pageCount, totalDataCount) = await _usefulLinkRepository.PreparePaginationDTO<UsefulLink, Guid>(pagiantionParams.PageIndex,
                                                                                                                              pagiantionParams.RequestedItemCount,
                                                                                                                              pagiantionParams.OrderByProperty,
                                                                                                                              pagiantionParams.OrderByAscending,
                                                                                                                              pagiantionParams.Spec?.ToExpression()).ConfigureAwait(false);
            return new PaginationDTO<UsefulLinkDTO>
            {
                DTOList = usefulLinks.CheckList(i => usefulLinks.Select(link => new UsefulLinkDTO
                {
                    Id = link.Id,
                    Title = link.Title,
                    Description = link.Description,
                    Url = link.Url,
                    ProfessionId = link.ProfessionId,
                    CreatorUser = link.CreatorUser
                })),
                PageCount = pageCount,
                TotalDataCount = totalDataCount
            };
        }

        /// <summary>
        /// Get link for mentor.
        /// </summary>
        /// <returns></returns>
        public async Task<PaginationDTO<UsefulLinkDTO>> GetUsefulLinksForMentorAsync(PaginationParamsWithSpec<UsefulLinkSpec> pagiantionParams)
        {
            var (usefulLinks, pageCount, totalDataCount) = await _usefulLinkRepository.PreparePaginationDTO<UsefulLink, Guid>(pagiantionParams.PageIndex,
                                                                                                                              pagiantionParams.RequestedItemCount,
                                                                                                                              pagiantionParams.OrderByProperty,
                                                                                                                              pagiantionParams.OrderByAscending,
                                                                                                                              pagiantionParams.Spec?.ToExpression()).ConfigureAwait(false);

            return new PaginationDTO<UsefulLinkDTO>
            {
                DTOList = usefulLinks.CheckList(i => usefulLinks.Select(link => new UsefulLinkDTO
                {
                    Id = link.Id,
                    Title = link.Title,
                    Description = link.Description,
                    Url = link.Url,
                    ProfessionId = link.ProfessionId,
                })),
                PageCount = pageCount,
                TotalDataCount = totalDataCount
            };
        }

        /// <summary>
        /// Get link by <paramref name="usefulLinkId"/>   
        /// </summary>
        /// <param name="usefulLinkId"></param>
        /// <returns></returns>
        public async Task<UsefulLinkDTO> GetUsefulLinkForStudentAsync(Guid usefulLinkId)
        {
            var link = await _usefulLinkRepository.GetByIdAsync(usefulLinkId).ConfigureAwait(false);

            link.ThrowIfNullForGuidObject("Böyle bir link bulunmamaktadır.");

            return new UsefulLinkDTO()
            {
                Title = link.Title,
                Description = link.Description,
                Url = link.Url,
                ProfessionId = link.ProfessionId
            };

        }

        /// <summary>
        /// Get useful link for admin by <paramref name="usefulLinkId"/>
        /// </summary>
        /// <param name="usefulLinkId"></param>
        /// <returns></returns>
        public async Task<UsefulLinkDTO> GetUsefulLinkForAdminAsync(Guid usefulLinkId)
        {
            var link = await _usefulLinkRepository.GetByIdAsync(usefulLinkId).ConfigureAwait(false);

            link.ThrowIfNullForGuidObject("Böyle bir link bulunmamaktadır.");

            return new UsefulLinkDTO()
            {
                Id = link.Id,
                Title = link.Title,
                Description = link.Description,
                Url = link.Url,
                ProfessionId = link.ProfessionId
            };
        }

        /// <summary>
        /// Get useful link for mentor by <paramref name="usefulLinkId"/>
        /// </summary>
        /// <param name="usefulLinkId"></param>
        /// <returns></returns>
        public async Task<UsefulLinkDTO> GetUsefulLinkForMentorAsync(Guid usefulLinkId)
        {
            var link = await _usefulLinkRepository.GetByIdAsync(usefulLinkId).ConfigureAwait(false);

            link.ThrowIfNullForGuidObject("Böyle bir link bulunmamaktadır.");

            return new UsefulLinkDTO()
            {
                Title = link.Title,
                Description = link.Description,
                Url = link.Url,
                ProfessionId = link.ProfessionId,

            };
        }

        /// <summary>
        /// Maps <paramref name="addUsefulLinkDTO"/> to <c><b>Useful Link</b></c>  object and adds that product to repository.
        /// </summary>
        /// <param name="addUsefulLinkDTO">Useful Link to be added.</param>
        /// <returns></returns>
        public async Task AddUsefulLinkAsync(AddUsefulLinkDTO addUsefulLinkDTO)
        {
            var usefullink = new UsefulLink
            {
                Title = addUsefulLinkDTO.Title,
                Description = addUsefulLinkDTO.Description,
                Url = addUsefulLinkDTO.Url,
                ProfessionId = addUsefulLinkDTO.ProfessionId
            };

            await _usefulLinkRepository.AddAsync(usefullink).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates single link which that equals <paramref name="addUsefulLinkDTO"/> in repository by <paramref name="addUsefulLinkDTO"/>'s properties.
        /// </summary>
        /// <param name="addUsefulLinkDTO">Link to be updated.</param>
        /// <returns></returns>
        public async Task UpdateUsefulLinkAsync(UpdateUsefulLinkDTO addUsefulLinkDTO)
        {
            var toBeUpdatedLink = await _usefulLinkRepository.GetByIdAsync(addUsefulLinkDTO.Id).ConfigureAwait(false);

            toBeUpdatedLink.Title = addUsefulLinkDTO.Title;

            toBeUpdatedLink.Description = addUsefulLinkDTO.Description;

            toBeUpdatedLink.Url = addUsefulLinkDTO.Url;

            await _usefulLinkRepository.UpdateAsync(toBeUpdatedLink).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete links by <paramref name="usefulLinkIds"/>
        /// </summary>
        /// <param name="usefulLinkIds"></param>
        /// <returns></returns>
        public async Task DeleteUsefulLinksAsync(List<Guid> usefulLinkIds)
        {
            var deletedLinks = await _usefulLinkRepository.GetAllAsync(i => usefulLinkIds.Select(p => p).Contains(i.Id)).ConfigureAwait(false);
            await _usefulLinkRepository.DeleteAsync(deletedLinks);
        }
    }
}
