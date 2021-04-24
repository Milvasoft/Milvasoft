﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.Models;
using Milvasoft.SampleAPI.Data;
using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.DTOs.AssignmentDTOs;
using Milvasoft.SampleAPI.DTOs.StudentAssignmentDTOs;
using Milvasoft.SampleAPI.DTOs.StudentDTOs;
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
        private readonly string _loggedUser;
        private readonly UserManager<AppUser> _userManager;
        private readonly IBaseRepository<Assignment, Guid, EducationAppDbContext> _assignmentRepository;
        private readonly IBaseRepository<StudentAssigment, Guid, EducationAppDbContext> _stuudentAssignmentRepository;


        /// <summary>
        /// Performs constructor injection for repository interfaces used in this service.
        /// </summary>
        /// <param name="assignmentRepository"></param>
        public AssignmentService(IBaseRepository<Assignment, Guid, EducationAppDbContext> assignmentRepository, UserManager<AppUser> userManager, IHttpContextAccessor httpContextAccessor, IBaseRepository<StudentAssigment, Guid, EducationAppDbContext> studentAssignmentRepository)
        {
            _stuudentAssignmentRepository = studentAssignmentRepository;
            _userManager = userManager;
            _loggedUser = httpContextAccessor.HttpContext.User.Identity.Name;
            _assignmentRepository = assignmentRepository;
        }

        /// <summary>
        /// Get all assignment by <paramref name="pagiantionParams"/>
        /// </summary>
        /// <returns></returns>
        public async Task<PaginationDTO<AssignmentForStudentDTO>> GetAssignmentForStudentAsync(PaginationParamsWithSpec<AssignmentSpec> pagiantionParams)
        {
            var (asssignments, pageCount, totalDataCount) = await _assignmentRepository.PreparePaginationDTO<IBaseRepository<Assignment, Guid, EducationAppDbContext>, Assignment, Guid>
                                                                                                                (pagiantionParams.PageIndex,
                                                                                                                pagiantionParams.RequestedItemCount,
                                                                                                                pagiantionParams.OrderByProperty = null,
                                                                                                                pagiantionParams.OrderByAscending = false,
                                                                                                                pagiantionParams.Spec?.ToExpression()).ConfigureAwait(false);

            return new PaginationDTO<AssignmentForStudentDTO>
            {
                DTOList = asssignments.CheckList(i => asssignments.Select(assignment => new AssignmentForStudentDTO
                {
                    Id = assignment.Id,
                    Title = assignment.Title,
                    Description = assignment.Description,
                    RemarksToStudent = assignment.RemarksToStudent,
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
        /// Get assignments for admin by <paramref name="pagiantionParams"/>
        /// </summary>
        /// <returns></returns>
        public async Task<PaginationDTO<AssignmentForAdminDTO>> GetAssignmentForAdminAsync(PaginationParamsWithSpec<AssignmentSpec> pagiantionParams)
        {
            var (asssignments, pageCount, totalDataCount) = await _assignmentRepository.PreparePaginationDTO<IBaseRepository<Assignment, Guid, EducationAppDbContext>, Assignment, Guid>
                                                                                                                 (pagiantionParams.PageIndex,
                                                                                                                pagiantionParams.RequestedItemCount,
                                                                                                                pagiantionParams.OrderByProperty = null,
                                                                                                                pagiantionParams.OrderByAscending = false,
                                                                                                                pagiantionParams.Spec?.ToExpression()).ConfigureAwait(false);

            return new PaginationDTO<AssignmentForAdminDTO>
            {
                DTOList = asssignments.CheckList(i => asssignments.Select(assignment => new AssignmentForAdminDTO
                {
                    Id = assignment.Id,
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
        /// Get assignments for mentor by <paramref name="pagiantionParams"/>
        /// </summary>
        /// <returns></returns>
        public async Task<PaginationDTO<AssignmentForMentorDTO>> GetAssignmentForMentorAsync(PaginationParamsWithSpec<AssignmentSpec> pagiantionParams)
        {
            var (asssignments, pageCount, totalDataCount) = await _assignmentRepository.PreparePaginationDTO<IBaseRepository<Assignment, Guid, EducationAppDbContext>, Assignment, Guid>
                                                                                                                  (pagiantionParams.PageIndex,
                                                                                                                pagiantionParams.RequestedItemCount,
                                                                                                                pagiantionParams.OrderByProperty = null,
                                                                                                                pagiantionParams.OrderByAscending = false,
                                                                                                                pagiantionParams.Spec?.ToExpression()).ConfigureAwait(false);

            return new PaginationDTO<AssignmentForMentorDTO>
            {
                DTOList = asssignments.CheckList(i => asssignments.Select(assignment => new AssignmentForMentorDTO
                {
                    Id = assignment.Id,
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
        public async Task<AssignmentForStudentDTO> GetAssignmentForStudentAsync(Guid assignmentId)
        {
            var assignment = await _assignmentRepository.GetByIdAsync(assignmentId).ConfigureAwait(false);

            assignment.ThrowIfNullForGuidObject();

            return new AssignmentForStudentDTO
            {
                Title = assignment.Title,
                Description = assignment.Description,
                RemarksToStudent = assignment.RemarksToStudent,
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
        public async Task<AssignmentForAdminDTO> GetAssignmentForAdminAsync(Guid assignmentId)
        {
            var assignment = await _assignmentRepository.GetByIdAsync(assignmentId).ConfigureAwait(false);

            assignment.ThrowIfNullForGuidObject();

            return new AssignmentForAdminDTO
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
        public async Task<AssignmentForMentorDTO> GetAssignmentForMentorAsync(Guid assignmentId)
        {
            var assignment = await _assignmentRepository.GetByIdAsync(assignmentId).ConfigureAwait(false);

            assignment.ThrowIfNullForGuidObject();

            return new AssignmentForMentorDTO
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
        /// Maps <paramref name="addAssignmentDTO"/> to <c><b>Assignment</b></c>  object and adds that product to repository.
        /// </summary>
        /// <param name="addAssignmentDTO">Assignment to be added.</param>
        /// <returns></returns>
        public async Task AddAssignmentAsync(AddAssignmentDTO addAssignmentDTO)
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
        /// Updates single assignment which that equals <paramref name="updateAssignmentDTO"/> in repository by <paramref name="updateAssignmentDTO"/>'s properties.
        /// </summary>
        /// <param name="updateAssignmentDTO">Assignment to be updated.</param>
        /// <returns></returns>
        public async Task UpdateAssignmentAsync(UpdateAssignmentDTO updateAssignmentDTO)
        {
            var toBeUpdatedAssignment = await _assignmentRepository.GetByIdAsync(updateAssignmentDTO.Id).ConfigureAwait(false);

            toBeUpdatedAssignment.Title = updateAssignmentDTO.Title;

            toBeUpdatedAssignment.Description = updateAssignmentDTO.Description;

            toBeUpdatedAssignment.Level = updateAssignmentDTO.Level;

            toBeUpdatedAssignment.MaxDeliveryDay = updateAssignmentDTO.MaxDeliveryDay;

            toBeUpdatedAssignment.ProfessionId = updateAssignmentDTO.ProfessionId;

            toBeUpdatedAssignment.RemarksToMentor = updateAssignmentDTO.RemarksToMentor;

            toBeUpdatedAssignment.RemarksToStudent = updateAssignmentDTO.RemarksToStudent;

            await _assignmentRepository.UpdateAsync(toBeUpdatedAssignment).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete assignments by <paramref name="assignmentIds"/>
        /// </summary>
        /// <param name="assignmentIds"></param>
        /// <returns></returns>
        public async Task DeleteAssignmentAsync(List<Guid> assignmentIds)
        {
            var assignments = await _assignmentRepository.GetAllAsync(i => assignmentIds.Select(p => p).Contains(i.Id)).ConfigureAwait(false);
            await _assignmentRepository.DeleteAsync(assignments).ConfigureAwait(false);
        }

        /// <summary>
        /// Brings homework suitable for the student's level.
        /// </summary>
        /// <returns> Returns the appropriate assignment to the student.</returns>
        public async Task<AssignmentForStudentDTO> GetAvaibleAssignmentForCurrentStudent()
        {
            var currentStudent = await _userManager.FindByNameAsync(_loggedUser).ConfigureAwait(false);

            currentStudent.Student.ThrowIfNullForGuidObject("User is not student.");

            int level = currentStudent.Student.Level;

            Guid professionId = currentStudent.Student.ProfessionId;

            var assignment = await _assignmentRepository.GetFirstOrDefaultAsync(i => i.Level == level && i.ProfessionId == professionId).ConfigureAwait(false);

            return new AssignmentForStudentDTO
            {
                Id = assignment.Id,
                Title = assignment.Title,
                Description = assignment.Description,
                RemarksToStudent = assignment.RemarksToStudent,
                Level = assignment.Level,
                Rules = assignment.Rules,
                MaxDeliveryDay = assignment.MaxDeliveryDay,
                ProfessionId = assignment.ProfessionId
            };
        }

        /// <summary>
        ///  The student takes the next assignment.
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="newAssignment"></param>
        /// <returns></returns>
        public async Task TakeAssignment(Guid Id,AddStudentAssignmentDTO newAssignment)
        {
            var currentStudent = await _userManager.FindByNameAsync(_loggedUser).ConfigureAwait(false);

            currentStudent.Student.ThrowIfNullForGuidObject("User is not student.");

            var toBeTakeAssignment = await _assignmentRepository.GetByIdAsync(Id).ConfigureAwait(false);

            toBeTakeAssignment.ThrowIfNullForGuidObject();

            var studentAssignment = new StudentAssigment
            {
                IsActive = false,
                AssigmentId = toBeTakeAssignment.Id,
                StudentId = currentStudent.Student.Id,
                AdditionalTime=newAssignment.AdditionalTime,
                AdditionalTimeDescription= newAssignment.AdditionalTimeDescription
            };

            await _stuudentAssignmentRepository.AddAsync(studentAssignment).ConfigureAwait(false);
        }

        
    }
}
