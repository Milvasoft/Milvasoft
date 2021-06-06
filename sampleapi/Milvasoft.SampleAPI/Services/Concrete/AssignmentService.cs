using Microsoft.AspNetCore.Http;
using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.DataAccess.IncludeLibrary;
using Milvasoft.Helpers.Exceptions;
using Milvasoft.Helpers.FileOperations.Concrete;
using Milvasoft.Helpers.FileOperations.Enums;
using Milvasoft.Helpers.Mail;
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
        #region Fields

        private readonly string _loggedUser;
        private readonly IBaseRepository<Assignment, Guid, EducationAppDbContext> _assignmentRepository;
        private readonly IBaseRepository<StudentAssigment, Guid, EducationAppDbContext> _studentAssignmentRepository;
        private readonly IBaseRepository<Student, Guid, EducationAppDbContext> _studentRepository;
        private readonly IBaseRepository<Mentor, Guid, EducationAppDbContext> _mentorRepository;
        private readonly IMilvaMailSender _milvaMailSender;

        #endregion

        /// <summary>
        /// Performs constructor injection for repository interfaces used in this service.
        /// </summary>
        /// <param name="assignmentRepository"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="studentAssignmentRepository"></param>
        /// <param name="studentRepository"></param>
        /// <param name="mentorRepository"></param>
        /// <param name="milvaMailSender"></param>
        public AssignmentService(IBaseRepository<Assignment, Guid, EducationAppDbContext> assignmentRepository,
            IHttpContextAccessor httpContextAccessor,
            IBaseRepository<StudentAssigment, Guid, EducationAppDbContext> studentAssignmentRepository,
            IBaseRepository<Student, Guid, EducationAppDbContext> studentRepository,
            IBaseRepository<Mentor, Guid, EducationAppDbContext> mentorRepository,
            IMilvaMailSender milvaMailSender)
        {
            _milvaMailSender = milvaMailSender;
            _mentorRepository = mentorRepository;
            _studentRepository = studentRepository;
            _studentAssignmentRepository = studentAssignmentRepository;
            _loggedUser = httpContextAccessor.HttpContext.User.Identity.Name;
            _assignmentRepository = assignmentRepository;
        }

        /// <summary>
        /// Get all assignment for student by <paramref name="paginationParams"/>
        /// </summary>
        /// <returns>The assignments is put in the form of an AnnouncementForStudentDTO.</returns>
        public async Task<PaginationDTO<AssignmentForStudentDTO>> GetAssignmentsForStudentAsync(PaginationParamsWithSpec<AssignmentSpec> paginationParams)
        {
            var (assignments, pageCount, totalDataCount) = await _assignmentRepository.PreparePaginationDTO(paginationParams.PageIndex,
                                                                                                            paginationParams.RequestedItemCount,
                                                                                                            paginationParams.OrderByProperty,
                                                                                                            paginationParams.OrderByAscending,
                                                                                                            paginationParams.Spec?.ToExpression()).ConfigureAwait(false);

            return new PaginationDTO<AssignmentForStudentDTO>
            {
                DTOList = assignments.CheckList(i => assignments.Select(assignment => new AssignmentForStudentDTO
                {
                    Id = assignment.Id,
                    Title = assignment.Title,
                    Description = assignment.Description,
                    RemarksToStudent = assignment.RemarksToStudent,
                    Level = assignment.Level,
                    Rules = assignment.Rules,
                    MaxDeliveryDay = assignment.MaxDeliveryDay,
                    ProfessionId = assignment.ProfessionId
                })),
                PageCount = pageCount,
                TotalDataCount = totalDataCount
            };
        }

        /// <summary>
        /// Get assignments for admin by <paramref name="paginationParams"/>
        /// </summary>
        /// <returns>The assignments is put in the form of an AnnouncementForAdminDTO.</returns>
        public async Task<PaginationDTO<AssignmentForAdminDTO>> GetAssignmentsForAdminAsync(PaginationParamsWithSpec<AssignmentSpec> paginationParams)
        {
            var (assignments, pageCount, totalDataCount) = await _assignmentRepository.PreparePaginationDTO(paginationParams.PageIndex,
                                                                                                            paginationParams.RequestedItemCount,
                                                                                                            paginationParams.OrderByProperty,
                                                                                                            paginationParams.OrderByAscending,
                                                                                                            paginationParams.Spec?.ToExpression()).ConfigureAwait(false);

            return new PaginationDTO<AssignmentForAdminDTO>
            {
                DTOList = assignments.CheckList(i => assignments.Select(assignment => new AssignmentForAdminDTO
                {
                    Id = assignment.Id,
                    Title = assignment.Title,
                    Description = assignment.Description,
                    RemarksToStudent = assignment.RemarksToStudent,
                    RemarksToMentor = assignment.RemarksToMentor,
                    Level = assignment.Level,
                    Rules = assignment.Rules,
                    MaxDeliveryDay = assignment.MaxDeliveryDay,
                    ProfessionId = assignment.ProfessionId
                })),
                PageCount = pageCount,
                TotalDataCount = totalDataCount
            };
        }

        /// <summary>
        /// Get assignments for mentor by <paramref name="paginationParams"/>
        /// </summary>
        /// <returns>The assignments is put in the form of an AnnouncementForMentorDTO.</returns>
        public async Task<PaginationDTO<AssignmentForMentorDTO>> GetAssignmentsForMentorAsync(PaginationParamsWithSpec<AssignmentSpec> paginationParams)
        {
            var (assignments, pageCount, totalDataCount) = await _assignmentRepository.PreparePaginationDTO(paginationParams.PageIndex,
                                                                                                            paginationParams.RequestedItemCount,
                                                                                                            paginationParams.OrderByProperty,
                                                                                                            paginationParams.OrderByAscending,
                                                                                                            paginationParams.Spec?.ToExpression()).ConfigureAwait(false);

            return new PaginationDTO<AssignmentForMentorDTO>
            {
                DTOList = assignments.CheckList(i => assignments.Select(assignment => new AssignmentForMentorDTO
                {
                    Id = assignment.Id,
                    Title = assignment.Title,
                    Description = assignment.Description,
                    RemarksToStudent = assignment.RemarksToStudent,
                    RemarksToMentor = assignment.RemarksToMentor,
                    Level = assignment.Level,
                    Rules = assignment.Rules,
                    MaxDeliveryDay = assignment.MaxDeliveryDay,
                    ProfessionId = assignment.ProfessionId
                })),
                PageCount = pageCount,
                TotalDataCount = totalDataCount
            };
        }

        /// <summary>
        /// Get assignment for student by <paramref name="assignmentId"/>
        /// </summary>
        /// <param name="assignmentId"> Id of the student to be shown.</param>
        /// <returns>The assignment is put in the form of an AnnouncementForStudentDTO.</returns>
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
                ProfessionId = assignment.ProfessionId
            };
        }

        /// <summary>
        /// Get assignment for admin by <paramref name="assignmentId"/>
        /// </summary>
        /// <param name="assignmentId">Id of the student to be shown.</param>
        /// <returns>The assignment is put in the form of an AnnouncementForAdminDTO.</returns>
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
                ProfessionId = assignment.ProfessionId
            };
        }

        /// <summary>
        /// Get assignment for mentor by <paramref name="assignmentId"/>
        /// </summary>
        /// <param name="assignmentId">Id of the student to be shown.</param>
        /// <returns>The assignment is put in the form of an AnnouncementForMentorDTO.</returns>
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
                ProfessionId = assignment.ProfessionId
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

            toBeUpdatedAssignment.ThrowIfNullForGuidObject();

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
        /// <param name="assignmentIds"> Id of assignments to be deleted.</param>
        /// <returns></returns>
        public async Task DeleteAssignmentAsync(List<Guid> assignmentIds)
        {
            var assignments = await _assignmentRepository.GetAllAsync(i => assignmentIds.Select(p => p).Contains(i.Id)).ConfigureAwait(false);

            assignments.ThrowIfListIsNullOrEmpty();

            await _assignmentRepository.DeleteAsync(assignments).ConfigureAwait(false);
        }

        #region Students

        /// <summary>
        /// Brings homework suitable for the student's level.
        /// </summary>
        /// <returns> Returns the appropriate assignment to the student.</returns>
        public async Task<AssignmentForStudentDTO> GetTakenAssignmentForCurrentStudentAsync()
        {
            var currentStudent = await _studentRepository.GetFirstOrDefaultAsync(i => i.AppUser.UserName == _loggedUser).ConfigureAwait(false);

            currentStudent.ThrowIfNullForGuidObject("IdentityInvalidToken");

            int level = currentStudent.Level;

            Guid professionId = currentStudent.ProfessionId;

            var assignment = await _assignmentRepository.GetFirstOrDefaultAsync(i => i.Level == level && i.ProfessionId == professionId).ConfigureAwait(false);

            assignment.ThrowIfNullForGuidObject("NoTaskFound");

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
        /// Get current assignment for logged student.
        /// </summary>
        /// <returns></returns>
        public async Task<AssignmentForStudentDTO> GetCurrentActiveAssignmentAsync()
        {
            Func<IIncludable<Student>, IIncludable> include = i => i.Include(p => p.OldAssignments);

            var currentUser= await _studentRepository.GetAllAsync(include, i=> i.AppUser.UserName == _loggedUser).ConfigureAwait(false);

            var currentStudent = currentUser.First();

            var currentAssignment = currentStudent.OldAssignments.Where(i=>i.IsApproved).First();

            currentAssignment.ThrowIfNullForGuidObject();

            var assignment = await _assignmentRepository.GetByIdAsync(currentAssignment.AssigmentId).ConfigureAwait(false);

            return new AssignmentForStudentDTO
            {
                Title = assignment.Title,
                Level = assignment.Level,
                Description = assignment.Description,
                RemarksToStudent = assignment.RemarksToStudent,
                MaxDeliveryDay = assignment.MaxDeliveryDay,
                Rules = assignment.Rules,
                ProfessionId = assignment.ProfessionId
            };
        }

        /// <summary>
        ///  The student takes the next assignment.
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="newAssignment"></param>
        /// <returns></returns>
        public async Task TakeAssignmentAsync(Guid Id, AddStudentAssignmentDTO newAssignment)
        {
            Func<IIncludable<Student>, IIncludable> include = i => i.Include(p => p.OldAssignments);

            var currentUser = await _studentRepository.GetAllAsync(include,i => i.AppUser.UserName == _loggedUser).ConfigureAwait(false);

            var currentStudent = currentUser.First();

            currentStudent.ThrowIfNullForGuidObject();

            var lastAssignment = currentStudent.OldAssignments.Where(i=>i.FinishedDate>=DateTime.Now);

            if (lastAssignment.Count() > 0)
            {
                throw new MilvaUserFriendlyException("UndeliveredHomework");
            }

            var toBeTakeAssignment = await _assignmentRepository.GetByIdAsync(Id,i=>i.Level==currentStudent.Level).ConfigureAwait(false);

            toBeTakeAssignment.ThrowIfNullForGuidObject();

            var studentAssignment = new StudentAssigment
            {
                IsApproved = false,
                AssigmentId = toBeTakeAssignment.Id,
                StudentId = currentStudent.Id,
                AdditionalTime = newAssignment.AdditionalTime,
                AdditionalTimeDescription = newAssignment.AdditionalTimeDescription,
                Status = Entity.Enum.EducationStatus.InProgress
            };

            await _studentAssignmentRepository.AddAsync(studentAssignment).ConfigureAwait(false);

            await _milvaMailSender.MilvaSendMailAsync(currentStudent.Mentor.AppUser.Email, Helpers.Enums.MailSubject.Information, currentStudent.Name + currentStudent.Surname + " isimli öğrencinin yeni bir ödev isteği var.");
        }

        /// <summary>
        /// Allows the student to turn in the assignment.
        /// </summary>
        /// <param name="submitAssignment"></param>
        /// <returns></returns>
        public async Task<string> SubmitAssignmentAsync(SubmitAssignmentDTO submitAssignment)
        {
            var currentStudent = await _studentRepository.GetFirstOrDefaultAsync(i => i.AppUser.UserName == _loggedUser).ConfigureAwait(false);

            string basePath = GlobalConstants.DocumentLibraryPath;

            FormFileOperations.FilesFolderNameCreator assignmentFolderNameCreator = (Type type) => type.Name + "Assignment";

            var maxFileLength = 140000000;

            var allowedFileExtensions = GlobalConstants.AllowedFileExtensions.Find(i => i.FileType == FileType.Compressed.ToString()).AllowedExtensions;

            var validationResult = submitAssignment.Assignment.ValidateFile(maxFileLength, allowedFileExtensions, FileType.Compressed);

            var path = await submitAssignment.Assignment.SaveFileToPathAsync(submitAssignment, basePath, assignmentFolderNameCreator, "Id").ConfigureAwait(false);

            await submitAssignment.Assignment.OpenReadStream().DisposeAsync().ConfigureAwait(false);

            await _milvaMailSender.MilvaSendMailAsync(currentStudent.Mentor.AppUser.Email, Helpers.Enums.MailSubject.Information, currentStudent.Name + currentStudent.Surname + " isimli öğrencinin ödevini gönderdi.");

            return path;
        }
        #endregion

        #region Mentors

        /// <summary>
        /// Brings the unapproved assignments of the students of the mentor logged in.
        /// </summary>
        /// <returns></returns>
        public async Task<List<StudentAssignmentDTO>> GetUnconfirmedAssignmentsAsync()
        {
            Func<IIncludable<StudentAssigment>, IIncludable> includes = i => i.Include(p => p.Assigment)
                                                                              .Include(s => s.Student)
                                                                                    .ThenInclude(m => m.Mentor);


            var currentMentor = await _mentorRepository.GetFirstOrDefaultAsync(i => i.AppUser.UserName == _loggedUser).ConfigureAwait(false);

            var unconfirmedAssignments = await _studentAssignmentRepository.GetAllAsync(includes, i => i.IsApproved == false && i.Student.Mentor.Id == currentMentor.Id).ConfigureAwait(false);

            return unconfirmedAssignments.CheckList(i => unconfirmedAssignments.Select(assignment => new StudentAssignmentDTO
            {
                Id = assignment.Id,
                IsApproved = assignment.IsApproved,
                AdditionalTime = assignment.AdditionalTime,
                AdditionalTimeDescription = assignment.AdditionalTimeDescription,
                Student = new StudentDTO
                {
                    Name = assignment.Student.Name,
                    Surname = assignment.Student.Surname,
                    Id = assignment.Student.Id
                }
            }));
        }

        /// <summary>
        /// The mentor approves the homework request sent by the student.
        /// </summary>
        /// <param name="toBeUpdated"></param>
        /// <returns></returns>
        public async Task ConfirmAssignmentAsync(StudentAssignmentDTO toBeUpdated)
        {
            var toBeUpdatedAssignment = await _studentAssignmentRepository.GetByIdAsync(toBeUpdated.Id).ConfigureAwait(false);

            toBeUpdatedAssignment.ThrowIfNullForGuidObject();

            var student = await _studentRepository.GetByIdAsync(toBeUpdatedAssignment.StudentId).ConfigureAwait(false);

            if (toBeUpdatedAssignment.IsApproved == true) throw new MilvaUserFriendlyException("AssignmentActive");

            toBeUpdatedAssignment.IsApproved = true;
            toBeUpdatedAssignment.FinishedDate = toBeUpdated.FinishedDate;

            student.CurrentAssigmentDeliveryDate = toBeUpdatedAssignment.FinishedDate;
        }
        #endregion
    }
}
