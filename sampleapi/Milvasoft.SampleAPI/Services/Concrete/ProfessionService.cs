using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.Models;
using Milvasoft.SampleAPI.Data;
using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.DTOs.ProfessionDTOs;
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
    /// Profession service.
    /// </summary>
    public class ProfessionService : IProfessionService
    {
        private readonly IBaseRepository<Profession, Guid, EducationAppDbContext> _professionRepository;

        /// <summary>
        /// Performs constructor injection for repository interfaces used in this service.
        /// </summary>
        /// <param name="professionRepository"></param>
        public ProfessionService(IBaseRepository<Profession, Guid, EducationAppDbContext> professionRepository)
        {
            _professionRepository = professionRepository;
        }

        /// <summary>
        /// Get all filtered professions by <paramref name="paginationParams"/>
        /// </summary>
        /// <returns>Returns the filtered profession.</returns>
        public async Task<PaginationDTO<ProfessionDTO>> GetProfessionsAsync(PaginationParamsWithSpec<ProfessionSpec> paginationParams)
        {
            var (professions, pageCount, totalDataCount) = await _professionRepository.PreparePaginationDTO(paginationParams.PageIndex,
                                                                                                            paginationParams.RequestedItemCount,
                                                                                                            paginationParams.OrderByProperty,
                                                                                                            paginationParams.OrderByAscending,
                                                                                                            paginationParams.Spec?.ToExpression()).ConfigureAwait(false);

            return new PaginationDTO<ProfessionDTO>
            {
                DTOList = professions.CheckList(i => professions.Select(profession => new ProfessionDTO
                {
                    Name = profession.Name,
                    Id = profession.Id
                })),
                PageCount = pageCount,
                TotalDataCount = totalDataCount
            };
        }

        /// <summary>
        /// Get profession for admin by <paramref name="professionId"/>.
        /// </summary>
        /// <param name="professionId">Profession id to display.</param>
        /// <returns>Returns the profession to which the student sent her/his id.</returns>
        public async Task<ProfessionDTO> GetProfessionForAdminAsync(Guid professionId)
        {
            var profession = await _professionRepository.GetByIdAsync(professionId).ConfigureAwait(false);

            profession.ThrowIfNullForGuidObject();

            return new ProfessionDTO
            {
                Name = profession.Name,
                CreationDate = profession.CreationDate,
                Id = profession.Id,
                LastModificationDate = profession.LastModificationDate,
                LastModifierUserId = profession.LastModifierUserId
            };
        }

        /// <summary>
        /// Get profession for mentor by <paramref name="professionId"/>
        /// </summary>
        /// <param name="professionId">Profession id to display.</param>
        /// <returns>Returns the profession to which the student sent her/his id.</returns>
        public async Task<ProfessionDTO> GetProfessionForMentorAsync(Guid professionId)
        {
            var profession = await _professionRepository.GetByIdAsync(professionId).ConfigureAwait(false);

            profession.ThrowIfNullForGuidObject();

            return new ProfessionDTO
            {
                Name = profession.Name,
                CreationDate = profession.CreationDate,
                Id = profession.Id,
                LastModificationDate = profession.LastModificationDate
            };
        }

        /// <summary>
        /// Get profession for student by <paramref name="professionId"/>
        /// </summary>
        /// <param name="professionId">Profession id to display.</param>
        /// <returns>Returns the profession to which the student sent her/his id.</returns>
        public async Task<ProfessionDTO> GetProfessionForStudentAsync(Guid professionId)
        {
            var profession = await _professionRepository.GetByIdAsync(professionId).ConfigureAwait(false);

            profession.ThrowIfNullForGuidObject();

            return new ProfessionDTO
            {
                Name = profession.Name
            };
        }

        /// <summary>
        /// Maps <paramref name="addProfessionDTO"/> to <c><b>Profession</b></c>  object and adds that product to repository.
        /// </summary>
        /// <param name="addProfessionDTO">Profession to be added.</param>
        public async Task AddProfessionAsync(AddProfessionDTO addProfessionDTO)
        {
            var profession = new Profession
            {
                Name = addProfessionDTO.Name,
            };

            await _professionRepository.AddAsync(profession).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates single profession which that equals <paramref name="updateProfessionDTO"/> in repository by <paramref name="updateProfessionDTO"/>'s properties.
        /// </summary>
        /// <param name="updateProfessionDTO">Profession to be updated.</param>
        public async Task UpdateProfessionAsync(UpdateProfessionDTO updateProfessionDTO)
        {
            var toBeUpdatedProfession = await _professionRepository.GetByIdAsync(updateProfessionDTO.Id).ConfigureAwait(false);

            toBeUpdatedProfession.ThrowIfNullForGuidObject();

            toBeUpdatedProfession.Name = updateProfessionDTO.Name;

            await _professionRepository.UpdateAsync(toBeUpdatedProfession).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes the professions whose id has been sent.
        /// </summary>
        /// <param name="professionIds">Professions to be deleted.</param>
        public async Task DeleteProfessionsAsync(List<Guid> professionIds)
        {
            var professions = await _professionRepository.GetAllAsync(i => professionIds.Select(p => p).Contains(i.Id)).ConfigureAwait(false);

            professions.ThrowIfListIsNullOrEmpty();

            await _professionRepository.DeleteAsync(professions).ConfigureAwait(false);
        }
    }
}
