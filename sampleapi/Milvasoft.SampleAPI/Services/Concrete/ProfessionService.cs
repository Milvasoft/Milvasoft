using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.SampleAPI.Data;
using Milvasoft.SampleAPI.DTOs.ProfessionDTOs;
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
        /// Get professions for admin.
        /// </summary>
        /// <returns></returns>
        public async Task<List<ProfessionDTO>> GetEntitiesForAdminAsync(ProfessionSpec professionSpec)
        {

            var professions = await _professionRepository.GetAllAsync(professionSpec?.ToExpression()).ConfigureAwait(false);

            var professionsDTO = from profession in professions
                                 select new ProfessionDTO
                                 {
                                     Name = profession.Name,
                                     CreationDate = profession.CreationDate,
                                     CreatorUserId = profession.CreatorUserId,
                                     Id = profession.Id,
                                     LastModificationDate = profession.LastModificationDate,
                                     LastModifierUserId = profession.LastModifierUserId
                                 };

            return professionsDTO.ToList();

        }

        /// <summary>
        /// Get professions for mentor.
        /// </summary>
        /// <returns></returns>
        public async Task<List<ProfessionDTO>> GetEntitiesForMentorAsync(ProfessionSpec professionSpec)
        {

            var professions = await _professionRepository.GetAllAsync(professionSpec?.ToExpression()).ConfigureAwait(false);

            var professionsDTO = from profession in professions
                                 select new ProfessionDTO
                                 {
                                     Id = profession.Id,
                                     Name = profession.Name,
                                     CreationDate = profession.CreationDate,
                                     LastModificationDate = profession.LastModificationDate
                                 };

            return professionsDTO.ToList();

        }

        /// <summary>
        /// Get professions for student.
        /// </summary>
        /// <returns></returns>
        public async Task<List<ProfessionDTO>> GetEntitiesForStudentAsync(ProfessionSpec professionSpec)
        {

            var professions = await _professionRepository.GetAllAsync(professionSpec?.ToExpression()).ConfigureAwait(false);

            var professionsDTO = from profession in professions
                                 select new ProfessionDTO
                                 {
                                     Name = profession.Name
                                 };

            return professionsDTO.ToList();

        }

        /// <summary>
        /// Get profession for admin by <paramref name="professionId"/>.
        /// </summary>
        /// <param name="professionId"></param>
        /// <returns></returns>
        public async Task<ProfessionDTO> GetEntityForAdminAsync(Guid professionId)
        {

            var profession = await _professionRepository.GetByIdAsync(professionId).ConfigureAwait(false);

            return new ProfessionDTO
            {
                Name = profession.Name,
                CreationDate = profession.CreationDate,
                CreatorUserId = profession.CreatorUserId,
                Id = profession.Id,
                LastModificationDate = profession.LastModificationDate,
                LastModifierUserId = profession.LastModifierUserId
            };

        }

        /// <summary>
        /// Get profession for mentor by <paramref name="professionId"/>
        /// </summary>
        /// <param name="professionId"></param>
        /// <returns></returns>
        public async Task<ProfessionDTO> GetEntityForMentorAsync(Guid professionId)
        {

            var profession = await _professionRepository.GetByIdAsync(professionId).ConfigureAwait(false);

            return new ProfessionDTO
            {
                Name = profession.Name,
                CreationDate = profession.CreationDate,
                LastModificationDate = profession.LastModificationDate
            };

        }

        /// <summary>
        /// Get profession for student by <paramref name="professionId"/>
        /// </summary>
        /// <param name="professionId"></param>
        /// <returns></returns>
        public async Task<ProfessionDTO> GetEntityForStudentAsync(Guid professionId)
        {

            var profession = await _professionRepository.GetByIdAsync(professionId).ConfigureAwait(false);

            return new ProfessionDTO
            {
                Name = profession.Name
            };

        }
        /// <summary>
        /// Add profession to database.
        /// </summary>
        /// <param name="addProfessionDTO"></param>
        /// <returns></returns>
        public async Task AddEntityAsync(AddProfessionDTO addProfessionDTO)
        {

            var profession = new Profession
            {
                Name = addProfessionDTO.Name,
            };

            await _professionRepository.AddAsync(profession).ConfigureAwait(false);

        }

        /// <summary>
        /// Update profession by <paramref name="addProfessionDTO"/>.
        /// </summary>
        /// <param name="addProfessionDTO"></param>
        /// <returns></returns>
        public async Task UpdateEntityAsync(UpdateProfessionDTO addProfessionDTO)
        {

            var updatedProfession = await _professionRepository.GetByIdAsync(addProfessionDTO.Id).ConfigureAwait(false);

            updatedProfession.Name = addProfessionDTO.Name;

            await _professionRepository.UpdateAsync(updatedProfession).ConfigureAwait(false);

        }

        /// <summary>
        /// Delete professions by <paramref name="professionIds"/>.
        /// </summary>
        /// <param name="professionIds"></param>
        /// <returns></returns>
        public async Task DeleteEntitiesAsync(List<Guid> professionIds)
        {

            var professions = await _professionRepository.GetAllAsync(i => professionIds.Select(p => p).Contains(i.Id)).ConfigureAwait(false);

            await _professionRepository.DeleteAsync(professions).ConfigureAwait(false);

        }

        /// <summary>
        /// Delete profession by <paramref name="professionId"/>.
        /// </summary>
        /// <param name="professionId"></param>
        /// <returns></returns>
        public async Task DeleteEntityAsync(Guid professionId)
        {

            var deletedProfession = await _professionRepository.GetByIdAsync(professionId).ConfigureAwait(false);

            await _professionRepository.DeleteAsync(deletedProfession).ConfigureAwait(false);

        }

    }
}
