using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.SampleAPI.Data;
using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.DTOs.UsefulLinkDTOs;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Services.Abstract;
using Milvasoft.SampleAPI.Spec;
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
        public async Task<List<UsefulLinkDTO>> GetEntitiesForStudentAsync(UsefulLinkSpec usefulLinkSpec)
        {
            var links = await _usefulLinkRepository.GetAllAsync(usefulLinkSpec?.ToExpression()).ConfigureAwait(false);

            return (from link in links
                              select new UsefulLinkDTO
                              {
                                  Title = link.Title,
                                  Description = link.Description,
                                  Url = link.Url,
                                  ProfessionId = link.ProfessionId,
                                  CreatorUser = link.CreatorUser
                              }).ToList();
        }

        /// <summary>
        /// Get links for admin.
        /// </summary>
        /// <returns></returns>
        public async Task<List<UsefulLinkDTO>> GetEntitiesForAdminAsync(UsefulLinkSpec usefulLinkSpec)
        {
            var links = await _usefulLinkRepository.GetAllAsync(usefulLinkSpec?.ToExpression()).ConfigureAwait(false);

            return (from link in links
                              select new UsefulLinkDTO
                              {
                                  Id = link.Id,
                                  Title = link.Title,
                                  Description = link.Description,
                                  Url = link.Url,
                                  ProfessionId = link.ProfessionId,
                                  CreatorUser = link.CreatorUser
                              }).ToList();
        }

        /// <summary>
        /// Get link for mentor.
        /// </summary>
        /// <returns></returns>
        public async Task<List<UsefulLinkDTO>> GetEntitiesForMentorAsync(UsefulLinkSpec usefulLinkSpec)
        {
            var links = await _usefulLinkRepository.GetAllAsync(usefulLinkSpec?.ToExpression()).ConfigureAwait(false);

            return (from link in links
                              select new UsefulLinkDTO
                              {
                                  Title = link.Title,
                                  Description = link.Description,
                                  Url = link.Url,
                                  ProfessionId = link.ProfessionId,
                              }).ToList();
        }

        /// <summary>
        /// Get link by <paramref name="usefulLinkId"/>   
        /// </summary>
        /// <param name="usefulLinkId"></param>
        /// <returns></returns>
        public async Task<UsefulLinkDTO> GetEntityForStudentAsync(Guid usefulLinkId)
        {
            var link = await _usefulLinkRepository.GetByIdAsync(usefulLinkId).ConfigureAwait(false);

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
        public async Task<UsefulLinkDTO> GetEntityForAdminAsync(Guid usefulLinkId)
        {
            var link = await _usefulLinkRepository.GetByIdAsync(usefulLinkId).ConfigureAwait(false);

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
        public async Task<UsefulLinkDTO> GetEntityForMentorAsync(Guid usefulLinkId)
        {
            var link = await _usefulLinkRepository.GetByIdAsync(usefulLinkId).ConfigureAwait(false);

            return new UsefulLinkDTO()
            {
                Title = link.Title,
                Description = link.Description,
                Url = link.Url,
                ProfessionId = link.ProfessionId,

            };
        }

        /// <summary>
        /// Add link.
        /// </summary>
        /// <param name="addUsefulLinkDTO"></param>
        /// <returns></returns>
        public async Task AddEntityAsync(AddUsefulLinkDTO addUsefulLinkDTO)
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
        /// Update link.
        /// </summary>
        /// <param name="addUsefulLinkDTO"></param>
        /// <returns></returns>
        public async Task UpdateEntityAsync(UpdateUsefulLinkDTO addUsefulLinkDTO)
        {
            var updatedLink = await _usefulLinkRepository.GetByIdAsync(addUsefulLinkDTO.Id).ConfigureAwait(false);

            updatedLink.Title = addUsefulLinkDTO.Title;

            updatedLink.Description = addUsefulLinkDTO.Description;

            updatedLink.Url = addUsefulLinkDTO.Url;

            await _usefulLinkRepository.UpdateAsync(updatedLink).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete link.
        /// </summary>
        /// <param name="usefulLinkId"></param>
        /// <returns></returns>
        public async Task DeleteEntityAsync(Guid usefulLinkId)
        {
            var deletedLink = await _usefulLinkRepository.GetByIdAsync(usefulLinkId).ConfigureAwait(false);

            await _usefulLinkRepository.DeleteAsync(deletedLink).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete links by <paramref name="usefulLinkIds"/>
        /// </summary>
        /// <param name="usefulLinkIds"></param>
        /// <returns></returns>
        public async Task DeleteEntitiesAsync(List<Guid> usefulLinkIds)
        {
            var deletedLinks = await _usefulLinkRepository.GetAllAsync(i => usefulLinkIds.Select(p => p).Contains(i.Id)).ConfigureAwait(false);
            await _usefulLinkRepository.DeleteAsync(deletedLinks);
        }
    }
}
