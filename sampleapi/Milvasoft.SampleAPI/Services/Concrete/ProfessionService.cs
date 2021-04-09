using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.Models;
using Milvasoft.Helpers.MultiTenancy.Accessor;
using Milvasoft.Helpers.MultiTenancy.EntityBase;
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
        private readonly IBaseRepository<TestEntity, TenantId, EducationAppDbContext> _testRepository;

        /// <summary>
        /// Performs constructor injection for repository interfaces used in this service.
        /// </summary>
        /// <param name="professionRepository"></param>
        /// <param name="testRepository"></param>
        /// <param name="tenantAccessor"></param>
        public ProfessionService(IBaseRepository<Profession, Guid, EducationAppDbContext> professionRepository,
                                 IBaseRepository<TestEntity, TenantId, EducationAppDbContext> testRepository,
                                 ITenantAccessor<CachedTenant, TenantId> tenantAccessor) 
        {

            _professionRepository = professionRepository;
            _testRepository = testRepository;
        }

        public async Task TestMethod()
        {
            var tenantId = new TenantId("milvasoft", 4);

            var entities = await _testRepository.GetAllAsync(i => i.TenancyName == tenantId.TenancyName);
        }

        /// <summary>
        /// Get professions for admin.
        /// </summary>
        /// <returns></returns>
        public async Task<PaginationDTO<ProfessionDTO>> GetEntitiesForAdminAsync(PaginationParamsWithSpec<ProfessionSpec> professionPaginationParams)
        {
            var (professions, pageCount, totalDataCount) = await _professionRepository.PreparePaginationDTO<IBaseRepository<Profession, Guid, EducationAppDbContext>, Profession, Guid>
                                                                                                                (professionPaginationParams.PageIndex,
                                                                                                                professionPaginationParams.RequestedItemCount,
                                                                                                                professionPaginationParams.OrderByProperty = null,
                                                                                                                professionPaginationParams.OrderByAscending = false,
                                                                                                                professionPaginationParams.Spec?.ToExpression()).ConfigureAwait(false);

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
        /// Get professions for mentor.
        /// </summary>
        /// <returns></returns>
        public async Task<PaginationDTO<ProfessionDTO>> GetEntitiesForMentorAsync(PaginationParamsWithSpec<ProfessionSpec> professionPaginationParams)
        {
            var (professions, pageCount, totalDataCount) = await _professionRepository.PreparePaginationDTO<IBaseRepository<Profession, Guid, EducationAppDbContext>, Profession, Guid>
                                                                                                                (professionPaginationParams.PageIndex,
                                                                                                                professionPaginationParams.RequestedItemCount,
                                                                                                                professionPaginationParams.OrderByProperty = null,
                                                                                                                professionPaginationParams.OrderByAscending = false,
                                                                                                                professionPaginationParams.Spec?.ToExpression()).ConfigureAwait(false);

            return new PaginationDTO<ProfessionDTO>
            {
                DTOList = professions.CheckList(i => professions.Select(profession => new ProfessionDTO
                {
                    Id = profession.Id,
                    Name = profession.Name
                })),
                PageCount = pageCount,
                TotalDataCount = totalDataCount
            };
        }

        /// <summary>
        /// Get professions for student.
        /// </summary>
        /// <returns></returns>
        public async Task<PaginationDTO<ProfessionDTO>> GetEntitiesForStudentAsync(PaginationParamsWithSpec<ProfessionSpec> professionPaginationParams)
        {
            var (professions, pageCount, totalDataCount) = await _professionRepository.PreparePaginationDTO<IBaseRepository<Profession, Guid, EducationAppDbContext>, Profession, Guid>
                                                                                                                (professionPaginationParams.PageIndex,
                                                                                                                professionPaginationParams.RequestedItemCount,
                                                                                                                professionPaginationParams.OrderByProperty = null,
                                                                                                                professionPaginationParams.OrderByAscending = false,
                                                                                                                professionPaginationParams.Spec?.ToExpression()).ConfigureAwait(false);

            return new PaginationDTO<ProfessionDTO>
            {

                DTOList = professions.CheckList(i => professions.Select(profession => new ProfessionDTO
                {
                    Id = profession.Id,
                    Name = profession.Name
                })),
                PageCount = pageCount,
                TotalDataCount = totalDataCount
            };
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
        /// Update profession by <paramref name="updateProfessionDTO"/>.
        /// </summary>
        /// <param name="updateProfessionDTO"></param>
        /// <returns></returns>
        public async Task UpdateEntityAsync(UpdateProfessionDTO updateProfessionDTO)
        {
            var updatedProfession = await _professionRepository.GetByIdAsync(updateProfessionDTO.Id).ConfigureAwait(false);

            updatedProfession.Name = updateProfessionDTO.Name;

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
