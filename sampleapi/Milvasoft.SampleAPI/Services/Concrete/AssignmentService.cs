using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.SampleAPI.Data;
using Milvasoft.SampleAPI.DTOs.AssignmentDTOs;
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
        /// Get all assignment by <paramref name="spec"/>
        /// </summary>
        /// <returns></returns>
        public async Task<List<AssignmentDTO>> GetEntitiesForStudent(AssignmentSpec spec = null)
        {
            var assignments = await _assignmentRepository.GetAllAsync(spec?.ToExpression()).ConfigureAwait(false);

            var assignmentDTOList = from assignment in assignments
                                    select new AssignmentDTO
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

            return assignmentDTOList.ToList();

        }

        /// <summary>
        /// Get assignments for admin by <paramref name="spec"/>
        /// </summary>
        /// <returns></returns>
        public async Task<List<AssignmentDTO>> GetEntitiesForAdmin(AssignmentSpec spec = null)
        {
            var assignments = await _assignmentRepository.GetAllAsync(spec?.ToExpression()).ConfigureAwait(false);

            var assignmentDTOList = from assignment in assignments
                                    select new AssignmentDTO
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

            return assignmentDTOList.ToList();

        }

        /// <summary>
        /// Get assignments for mentor by <paramref name="spec"/>
        /// </summary>
        /// <returns></returns>
        public async Task<List<AssignmentDTO>> GetEntitiesForMentor(AssignmentSpec spec = null)
        {
            var assignments = await _assignmentRepository.GetAllAsync(spec?.ToExpression()).ConfigureAwait(false);

            var assignmentDTOList = from assignment in assignments
                                    select new AssignmentDTO
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

            return assignmentDTOList.ToList();

        }

        /// <summary>
        /// Get assignment for student by <paramref name="id"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<AssignmentDTO> GetEntityForStudent(Guid id)
        {

            var assignment = await _assignmentRepository.GetByIdAsync(id).ConfigureAwait(false);

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
        /// Get assignment for admin by <paramref name="id"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<AssignmentDTO> GetEntityForAdmin(Guid id)
        {
            var assignment = await _assignmentRepository.GetByIdAsync(id).ConfigureAwait(false);

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
        /// Get assignment for mentor by <paramref name="id"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<AssignmentDTO> GetEntityForMentor(Guid id)
        {
            var assignment = await _assignmentRepository.GetByIdAsync(id).ConfigureAwait(false);

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
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        public async Task AddEntityAsync(AssignmentDTO educationDTO)
        {

            var assignment = new Assignment
            {
                Id = educationDTO.Id,
                Title = educationDTO.Title,
                Description = educationDTO.Description,
                RemarksToStudent = educationDTO.RemarksToStudent,
                RemarksToMentor = educationDTO.RemarksToMentor,
                Level = educationDTO.Level,
                Rules = educationDTO.Rules,
                MaxDeliveryDay = educationDTO.MaxDeliveryDay,
                ProfessionId = educationDTO.ProfessionId,
                CreationDate = DateTime.Now
            };

            await _assignmentRepository.AddAsync(assignment).ConfigureAwait(false);

        }

        /// <summary>
        /// Update assignment.
        /// </summary>
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        public async Task UpdateEntityAsync(AssignmentDTO educationDTO)
        {

            var updatedAssignment = await _assignmentRepository.GetByIdAsync(educationDTO.Id).ConfigureAwait(false);

            updatedAssignment.LastModificationDate = DateTime.Now;

            updatedAssignment.Title = educationDTO.Title;

            updatedAssignment.Description = educationDTO.Description;

            updatedAssignment.Level = educationDTO.Level;

            updatedAssignment.MaxDeliveryDay = educationDTO.MaxDeliveryDay;

            updatedAssignment.ProfessionId = educationDTO.ProfessionId;

            updatedAssignment.RemarksToMentor = educationDTO.RemarksToMentor;

            updatedAssignment.RemarksToStudent = educationDTO.RemarksToStudent;

            await _assignmentRepository.UpdateAsync(updatedAssignment).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete assignment.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteEntityAsync(Guid id)
        {

            var deletedAssignment = await _assignmentRepository.GetByIdAsync(id).ConfigureAwait(false);

            await _assignmentRepository.DeleteAsync(deletedAssignment);

        }

        /// <summary>
        /// Delete assignments by <paramref name="ids"/>
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task DeleteEntities(List<Guid> ids)
        {
            var assignments = await _assignmentRepository.GetAllAsync(i => ids.Select(p => p).Contains(i.Id)).ConfigureAwait(false);
            await _assignmentRepository.DeleteAsync(assignments).ConfigureAwait(false);
        }
    }
}
