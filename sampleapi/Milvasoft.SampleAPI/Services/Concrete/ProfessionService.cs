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
    public class ProfessionService : IBaseService<ProfessionDTO,ProfessionSpec>
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
        public async Task<List<ProfessionDTO>> GetEntitiesForAdmin(ProfessionSpec spec)
        {

            var professions = await _professionRepository.GetAllAsync(spec?.ToExpression()).ConfigureAwait(false);

            var professionsDTO = from profession in professions
                                 select new ProfessionDTO
                                 {
                                     Name=profession.Name,
                                     CreationDate=profession.CreationDate,
                                     CreatorUserId=profession.CreatorUserId,
                                     Id=profession.Id,
                                     LastModificationDate=profession.LastModificationDate,
                                     LastModifierUserId=profession.LastModifierUserId
                                 };

            return professionsDTO.ToList();

        }

        /// <summary>
        /// Get professions for mentor.
        /// </summary>
        /// <returns></returns>
        public async Task<List<ProfessionDTO>> GetEntitiesForMentor(ProfessionSpec spec)
        {

            var professions = await _professionRepository.GetAllAsync(spec?.ToExpression()).ConfigureAwait(false);

            var professionsDTO = from profession in professions
                                 select new ProfessionDTO
                                 {
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
        public async Task<List<ProfessionDTO>> GetEntitiesForStudent(ProfessionSpec spec)
        {

            var professions = await _professionRepository.GetAllAsync(spec?.ToExpression()).ConfigureAwait(false);

            var professionsDTO = from profession in professions
                                 select new ProfessionDTO
                                 {
                                     Name = profession.Name
                                 };

            return professionsDTO.ToList();

        }

        /// <summary>
        /// Get profession for admin by <paramref name="id"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ProfessionDTO> GetEntityForAdmin(Guid id)
        {

            var profession = await _professionRepository.GetByIdAsync(id).ConfigureAwait(false);

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
        /// Get profession for mentor by <paramref name="id"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ProfessionDTO> GetEntityForMentor(Guid id)
        {

            var profession = await _professionRepository.GetByIdAsync(id).ConfigureAwait(false);

            return new ProfessionDTO
            {
                Name = profession.Name,
                CreationDate = profession.CreationDate,
                LastModificationDate = profession.LastModificationDate
            };

        }

        /// <summary>
        /// Get profession for student by <paramref name="id"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ProfessionDTO> GetEntityForStudent(Guid id)
        {

            var profession = await _professionRepository.GetByIdAsync(id).ConfigureAwait(false);

            return new ProfessionDTO
            {
                Name = profession.Name
            };

        }
        /// <summary>
        /// Add profession to database.
        /// </summary>
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        public async Task AddEntityAsync(ProfessionDTO educationDTO)
        {

            var profession = new Profession
            {
                Name = educationDTO.Name,
                CreationDate = DateTime.Now,
                CreatorUserId = educationDTO.CreatorUserId,
            };

            await _professionRepository.AddAsync(profession).ConfigureAwait(false);

        }

        /// <summary>
        /// Update profession by <paramref name="educationDTO"/>.
        /// </summary>
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        public async Task UpdateEntityAsync(ProfessionDTO educationDTO)
        {

            var updatedProfession = await _professionRepository.GetByIdAsync(educationDTO.Id).ConfigureAwait(false);

            updatedProfession.Name = educationDTO.Name;

            updatedProfession.LastModificationDate = DateTime.Now;

            updatedProfession.LastModifierUserId = educationDTO.LastModifierUserId;

            await _professionRepository.UpdateAsync(updatedProfession).ConfigureAwait(false);

        }

        /// <summary>
        /// Delete professions by <paramref name="ids"/>.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task DeleteEntities(List<Guid> ids)
        {

            var professions = await _professionRepository.GetAllAsync(i => ids.Select(p => p).Contains(i.Id)).ConfigureAwait(false);

            await _professionRepository.DeleteAsync(professions).ConfigureAwait(false);

        }

        /// <summary>
        /// Delete profession by <paramref name="id"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteEntityAsync(Guid id)
        {

            var deletedProfession = await _professionRepository.GetByIdAsync(id).ConfigureAwait(false);

            await _professionRepository.DeleteAsync(deletedProfession).ConfigureAwait(false);

        }

    }
}
