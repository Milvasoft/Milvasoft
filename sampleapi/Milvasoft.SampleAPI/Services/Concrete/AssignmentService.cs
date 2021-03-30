using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.Models;
using Milvasoft.SampleAPI.Data;
using Milvasoft.SampleAPI.DTOs.AssignmentDTOs;
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
    /// Assignment service.
    /// </summary>
    public class AssignmentService : IAssignmentService
    {
        private readonly IBaseRepository<Assignment, Guid, EducationAppDbContext> _assignmentRepository;

        /// <summary>
        /// Performs constructor injection for repository interfaces used in this service.
        /// </summary>
        /// <param name="assignmentRepository"></param>
        public AssignmentService(IBaseRepository<Assignment, Guid, EducationAppDbContext> assignmentRepository)
        {
            _assignmentRepository = assignmentRepository;
        }

        /// <summary>
        /// Get all assignment by <paramref name="assignmentSpec"/>
        /// </summary>
        /// <returns></returns>
        public async Task<PaginationDTO<AssignmentDTO>> GetEntitiesForStudentAsync(int pageIndex,
                                                                                     int requestedItemCount,
                                                                                     string orderByProperty = null,
                                                                                     bool orderByAscending = false,
                                                                                      AssignmentSpec assignmentSpec = null)
        {
            var (asssignments, pageCount, totalDataCount) = await _assignmentRepository.PreparePaginationDTO<IBaseRepository<Assignment, Guid, EducationAppDbContext>, Assignment, Guid>
                                                                                                                (pageIndex, requestedItemCount, orderByProperty, orderByAscending, assignmentSpec?.ToExpression()).ConfigureAwait(false);

            return new PaginationDTO<AssignmentDTO>
            {
                DTOList = asssignments.CheckList(i => asssignments.Select(assignment => new AssignmentDTO
                {
                    Title = assignment.Title,
                    Description = assignment.Description,
                    RemarksToStudent = assignment.RemarksToStudent,
                    RemarksToMentor = assignment.RemarksToMentor,
                    Level = assignment.Level,
                    Rules = assignment.Rules,
                    MaxDeliveryDay = assignment.MaxDeliveryDay,
                    ProfessionId = assignment.ProfessionId,
                    CreatorUser = assignment.CreatorUser
                })),
                PageCount = pageCount,
                TotalDataCount = totalDataCount
            };
        }

        /// <summary>
        /// Get assignments for admin by <paramref name="assignmentSpec"/>
        /// </summary>
        /// <returns></returns>
        public async Task<PaginationDTO<AssignmentDTO>> GetEntitiesForAdminAsync(int pageIndex,
                                                                                     int requestedItemCount,
                                                                                     string orderByProperty = null,
                                                                                     bool orderByAscending = false,
                                                                                      AssignmentSpec assignmentSpec = null)
        {
            var (asssignments, pageCount, totalDataCount) = await _assignmentRepository.PreparePaginationDTO<IBaseRepository<Assignment, Guid, EducationAppDbContext>, Assignment, Guid>
                                                                                                                 (pageIndex, requestedItemCount, orderByProperty, orderByAscending, assignmentSpec?.ToExpression()).ConfigureAwait(false);

            return new PaginationDTO<AssignmentDTO>
            {
                DTOList = asssignments.CheckList(i => asssignments.Select(assignment => new AssignmentDTO
                {
                    Title = assignment.Title,
                    Description = assignment.Description,
                    RemarksToStudent = assignment.RemarksToStudent,
                    RemarksToMentor = assignment.RemarksToMentor,
                    Level = assignment.Level,
                    Rules = assignment.Rules,
                    MaxDeliveryDay = assignment.MaxDeliveryDay,
                    ProfessionId = assignment.ProfessionId,
                    CreatorUser = assignment.CreatorUser,
                    LastModifierUser = assignment.LastModifierUser
                })),
                PageCount = pageCount,
                TotalDataCount = totalDataCount
            };
        }

        /// <summary>
        /// Get assignments for mentor by <paramref name="assignmentSpec"/>
        /// </summary>
        /// <returns></returns>
        public async Task<PaginationDTO<AssignmentDTO>> GetEntitiesForMentorAsync(int pageIndex,
                                                                                     int requestedItemCount,
                                                                                     string orderByProperty = null,
                                                                                     bool orderByAscending = false,
                                                                                      AssignmentSpec assignmentSpec = null)
        {
            var (asssignments, pageCount, totalDataCount) = await _assignmentRepository.PreparePaginationDTO<IBaseRepository<Assignment, Guid, EducationAppDbContext>, Assignment, Guid>
                                                                                                                 (pageIndex, requestedItemCount, orderByProperty, orderByAscending, assignmentSpec?.ToExpression()).ConfigureAwait(false);

            return new PaginationDTO<AssignmentDTO>
            {
                DTOList = asssignments.CheckList(i => asssignments.Select(assignment => new AssignmentDTO
                {
                    Title = assignment.Title,
                    Description = assignment.Description,
                    RemarksToStudent = assignment.RemarksToStudent,
                    RemarksToMentor = assignment.RemarksToMentor,
                    Level = assignment.Level,
                    Rules = assignment.Rules,
                    MaxDeliveryDay = assignment.MaxDeliveryDay,
                    ProfessionId = assignment.ProfessionId,
                    CreatorUser = assignment.CreatorUser
                })),
                PageCount = pageCount,
                TotalDataCount = totalDataCount
            };
        }

        /// <summary>
        /// Get assignment for student by <paramref name="assignmentId"/>
        /// </summary>
        /// <param name="assignmentId"></param>
        /// <returns></returns>
        public async Task<AssignmentDTO> GetEntityForStudentAsync(Guid assignmentId)
        {
            var assignment = await _assignmentRepository.GetByIdAsync(assignmentId).ConfigureAwait(false);

            return new AssignmentDTO
            {
                Title = assignment.Title,
                Description = assignment.Description,
                RemarksToStudent = assignment.RemarksToStudent,
                RemarksToMentor = assignment.RemarksToMentor,
                Level = assignment.Level,
                Rules = assignment.Rules,
                MaxDeliveryDay = assignment.MaxDeliveryDay,
                ProfessionId = assignment.ProfessionId,
                CreatorUser = assignment.CreatorUser
            };
        }

        /// <summary>
        /// Get assignment for admin by <paramref name="assignmentId"/>
        /// </summary>
        /// <param name="assignmentId"></param>
        /// <returns></returns>
        public async Task<AssignmentDTO> GetEntityForAdminAsync(Guid assignmentId)
        {
            var assignment = await _assignmentRepository.GetByIdAsync(assignmentId).ConfigureAwait(false);

            return new AssignmentDTO
            {
                Title = assignment.Title,
                Description = assignment.Description,
                RemarksToStudent = assignment.RemarksToStudent,
                RemarksToMentor = assignment.RemarksToMentor,
                Level = assignment.Level,
                Rules = assignment.Rules,
                MaxDeliveryDay = assignment.MaxDeliveryDay,
                ProfessionId = assignment.ProfessionId,
                CreationDate = assignment.CreationDate,
                LastModificationDate = assignment.LastModificationDate,
                CreatorUser = assignment.CreatorUser,
                LastModifierUser = assignment.LastModifierUser
            };
        }

        /// <summary>
        /// Get assignment for mentor by <paramref name="assignmentId"/>
        /// </summary>
        /// <param name="assignmentId"></param>
        /// <returns></returns>
        public async Task<AssignmentDTO> GetEntityForMentorAsync(Guid assignmentId)
        {
            var assignment = await _assignmentRepository.GetByIdAsync(assignmentId).ConfigureAwait(false);

            return new AssignmentDTO
            {
                Title = assignment.Title,
                Description = assignment.Description,
                RemarksToStudent = assignment.RemarksToStudent,
                RemarksToMentor = assignment.RemarksToMentor,
                Level = assignment.Level,
                Rules = assignment.Rules,
                MaxDeliveryDay = assignment.MaxDeliveryDay,
                ProfessionId = assignment.ProfessionId,
                CreationDate = assignment.CreationDate,
                CreatorUser = assignment.CreatorUser,
                LastModifierUser = assignment.LastModifierUser
            };
        }

        /// <summary>
        /// Add assignment.
        /// </summary>
        /// <param name="addAssignmentDTO"></param>
        /// <returns></returns>
        public async Task AddEntityAsync(AddAssignmentDTO addAssignmentDTO)
        {
            var assignment = new Assignment
            {
                Title = addAssignmentDTO.Title,
                Description = addAssignmentDTO.Description,
                RemarksToStudent = addAssignmentDTO.RemarksToStudent,
                RemarksToMentor = addAssignmentDTO.RemarksToMentor,
                Level = addAssignmentDTO.Level,
                Rules = addAssignmentDTO.Rules,
                MaxDeliveryDay = addAssignmentDTO.MaxDeliveryDay,
                ProfessionId = addAssignmentDTO.ProfessionId,
            };

            await _assignmentRepository.AddAsync(assignment).ConfigureAwait(false);
        }

        /// <summary>
        /// Update assignment.
        /// </summary>
        /// <param name="updateAssignmentDTO"></param>
        /// <returns></returns>
        public async Task UpdateEntityAsync(UpdateAssignmentDTO updateAssignmentDTO)
        {
            var updatedAssignment = await _assignmentRepository.GetByIdAsync(updateAssignmentDTO.Id).ConfigureAwait(false);

            updatedAssignment.Title = updateAssignmentDTO.Title;

            updatedAssignment.Description = updateAssignmentDTO.Description;

            updatedAssignment.Level = updateAssignmentDTO.Level;

            updatedAssignment.MaxDeliveryDay = updateAssignmentDTO.MaxDeliveryDay;

            updatedAssignment.ProfessionId = updateAssignmentDTO.ProfessionId;

            updatedAssignment.RemarksToMentor = updateAssignmentDTO.RemarksToMentor;

            updatedAssignment.RemarksToStudent = updateAssignmentDTO.RemarksToStudent;

            await _assignmentRepository.UpdateAsync(updatedAssignment).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete assignment.
        /// </summary>
        /// <param name="assignementId"></param>
        /// <returns></returns>
        public async Task DeleteEntityAsync(Guid assignementId)
        {
            var deletedAssignment = await _assignmentRepository.GetByIdAsync(assignementId).ConfigureAwait(false);

            await _assignmentRepository.DeleteAsync(deletedAssignment);
        }

        /// <summary>
        /// Delete assignments by <paramref name="assignmentIds"/>
        /// </summary>
        /// <param name="assignmentIds"></param>
        /// <returns></returns>
        public async Task DeleteEntitiesAsync(List<Guid> assignmentIds)
        {
            var assignments = await _assignmentRepository.GetAllAsync(i => assignmentIds.Select(p => p).Contains(i.Id)).ConfigureAwait(false);
            await _assignmentRepository.DeleteAsync(assignments).ConfigureAwait(false);
        }
    }
}
