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
        public async Task<List<UsefulLinkDTO>> GetEntitiesForStudentAsync(UsefulLinkSpec spec)
        {

            var links = await _usefulLinkRepository.GetAllAsync(spec?.ToExpression()).ConfigureAwait(false);

            var linkDTOList = from link in links
                              select new UsefulLinkDTO
                              {
                                  Title = link.Title,
                                  Description = link.Description,
                                  Url = link.Url,
                                  ProfessionId = link.ProfessionId,
                                  CreatorUser = link.CreatorUser
                              };

            return linkDTOList.ToList();

        }

        /// <summary>
        /// Get links for admin.
        /// </summary>
        /// <returns></returns>
        public async Task<List<UsefulLinkDTO>> GetEntitiesForAdminAsync(UsefulLinkSpec spec)
        {

            var links = await _usefulLinkRepository.GetAllAsync(spec?.ToExpression()).ConfigureAwait(false);

            var linkDTOList = from link in links
                              select new UsefulLinkDTO
                              {
                                  Id = link.Id,
                                  Title = link.Title,
                                  Description = link.Description,
                                  Url = link.Url,
                                  ProfessionId = link.ProfessionId,
                                  CreationDate = link.CreationDate,
                                  CreatorUser = link.CreatorUser,
                                  LastModificationDate = link.LastModificationDate
                              };

            return linkDTOList.ToList();

        }

        /// <summary>
        /// Get link for mentor.
        /// </summary>
        /// <returns></returns>
        public async Task<List<UsefulLinkDTO>> GetEntitiesForMentorAsync(UsefulLinkSpec spec)
        {

            var links = await _usefulLinkRepository.GetAllAsync(spec?.ToExpression()).ConfigureAwait(false);

            var linkDTOList = from link in links
                              select new UsefulLinkDTO
                              {
                                  Title = link.Title,
                                  Description = link.Description,
                                  Url = link.Url,
                                  ProfessionId = link.ProfessionId,
                              };

            return linkDTOList.ToList();

        }

        /// <summary>
        /// Get link by <paramref name="id"/>   
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<UsefulLinkDTO> GetEntityForStudentAsync(Guid id)
        {

            var link = await _usefulLinkRepository.GetByIdAsync(id).ConfigureAwait(false);

            return new UsefulLinkDTO()
            {
                Title = link.Title,
                Description = link.Description,
                Url = link.Url,
                ProfessionId = link.ProfessionId
            };

        }

        /// <summary>
        /// Get useful link for admin by <paramref name="id"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<UsefulLinkDTO> GetEntityForAdminAsync(Guid id)
        {
            var link = await _usefulLinkRepository.GetByIdAsync(id).ConfigureAwait(false);

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
        /// Get useful link for mentor by <paramref name="id"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<UsefulLinkDTO> GetEntityForMentorAsync(Guid id)
        {
            var link = await _usefulLinkRepository.GetByIdAsync(id).ConfigureAwait(false);

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
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        public async Task AddEntityAsync(AddUsefulLinkDTO educationDTO)
        {

            var usefullink = new UsefulLink
            {
                Title = educationDTO.Title,
                Description = educationDTO.Description,
                Url = educationDTO.Url,
                ProfessionId = educationDTO.ProfessionId
            };

            await _usefulLinkRepository.AddAsync(usefullink).ConfigureAwait(false);

        }

        /// <summary>
        /// Update link.
        /// </summary>
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        public async Task UpdateEntityAsync(UpdateUsefulLinkDTO educationDTO)
        {

            var updatedLink = await _usefulLinkRepository.GetByIdAsync(educationDTO.Id).ConfigureAwait(false);

            updatedLink.Title = educationDTO.Title;

            updatedLink.Description = educationDTO.Description;

            updatedLink.Url = educationDTO.Url;

            await _usefulLinkRepository.UpdateAsync(updatedLink).ConfigureAwait(false);

        }

        /// <summary>
        /// Delete link.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteEntityAsync(Guid id)
        {

            var deletedLink = await _usefulLinkRepository.GetByIdAsync(id).ConfigureAwait(false);

            await _usefulLinkRepository.DeleteAsync(deletedLink).ConfigureAwait(false);

        }

        /// <summary>
        /// Delete links by <paramref name="ids"/>
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task DeleteEntitiesAsync(List<Guid> ids)
        {
            var deletedLinks = await _usefulLinkRepository.GetAllAsync(i => ids.Select(p => p).Contains(i.Id)).ConfigureAwait(false);
            await _usefulLinkRepository.DeleteAsync(deletedLinks);
        }
    }
}
