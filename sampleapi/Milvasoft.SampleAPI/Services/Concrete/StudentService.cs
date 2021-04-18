using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.DataAccess.IncludeLibrary;
using Milvasoft.Helpers.Exceptions;
using Milvasoft.Helpers.Models;
using Milvasoft.SampleAPI.Data;
using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.DTOs.MentorDTOs;
using Milvasoft.SampleAPI.DTOs.StudentDTOs;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Services.Abstract;
using Milvasoft.SampleAPI.Spec;
using Milvasoft.SampleAPI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Services.Concrete
{

    /// <summary>
    /// Student service.
    /// </summary>
    public class StudentService : IStudentService
    {
        private readonly string _loggedUser;
        private readonly UserManager<AppUser> _userManager;
        private readonly IBaseRepository<Student, Guid, EducationAppDbContext> _studentRepository;

        /// <summary>
        /// Performs constructor injection for repository interfaces used in this service.
        /// </summary>
        /// <param name="studentRepository"></param>
        /// <param name="userManager"></param>
        public StudentService(IBaseRepository<Student, Guid, EducationAppDbContext> studentRepository, UserManager<AppUser> userManager,IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _studentRepository = studentRepository;
            _loggedUser = httpContextAccessor.HttpContext.User.Identity.Name;
        }

        /// <summary>
        /// Get students for admin.
        /// </summary>
        /// <returns></returns>
        public async Task<PaginationDTO<StudentForAdminDTO>> GetStudentsForAdminAsync(PaginationParamsWithSpec<StudentSpec> pagiantionParams)
        {
            Func<IIncludable<Student>, IIncludable> includes = i => i.Include(md => md.Mentor);

            var (students, pageCount, totalDataCount) = await _studentRepository.PreparePaginationDTO<IBaseRepository<Student, Guid, EducationAppDbContext>, Student, Guid>
                                                                                                                (pagiantionParams.PageIndex,
                                                                                                                pagiantionParams.RequestedItemCount,
                                                                                                                pagiantionParams.OrderByProperty = null,
                                                                                                                pagiantionParams.OrderByAscending = false,
                                                                                                                pagiantionParams.Spec?.ToExpression(),
                                                                                                                includes).ConfigureAwait(false);

            return new PaginationDTO<StudentForAdminDTO>
            {
                DTOList = students.CheckList(i => students.Select(student => new StudentForAdminDTO
                {
                    Id = student.Id,
                    Name = student.Name,
                    Surname = student.Surname,
                    University = student.University,
                    Age = student.Age,
                    HomeAddress = student.HomeAddress,
                    MentorThoughts = student.MentorThoughts,
                    IsConfidentialityAgreementSigned = student.IsConfidentialityAgreementSigned,
                    GraduationStatus = student.GraduationStatus,
                    GraduationScore = student.GraduationScore,
                    MentorGraduationThoughts = student.MentorGraduationThoughts,
                    ProfessionId = student.ProfessionId,
                    Mentor = student.Mentor.CheckObject(i => new MentorDTO
                    {
                        Id = i.Id
                    })
                })),
                PageCount = pageCount,
                TotalDataCount = totalDataCount
            };
        }

        /// <summary>
        /// Get students for mentor.
        /// </summary>
        /// <returns></returns>
        public async Task<PaginationDTO<StudentForMentorDTO>> GetStudentsForCurrentMentorAsync(PaginationParamsWithSpec<StudentSpec> pagiantionParams)
        {
            var mentor = await _userManager.FindByNameAsync(_loggedUser).ConfigureAwait(false);

            Func<IIncludable<Student>, IIncludable> includes = i => i.Include(md => md.Mentor==mentor.Mentor)
                                                                        .Include(assi => assi.OldAssignments);


            var (students, pageCount, totalDataCount) = await _studentRepository.PreparePaginationDTO<IBaseRepository<Student, Guid, EducationAppDbContext>, Student, Guid>
                                                                                                                (pagiantionParams.PageIndex,
                                                                                                                pagiantionParams.RequestedItemCount,
                                                                                                                pagiantionParams.OrderByProperty = null,
                                                                                                                pagiantionParams.OrderByAscending = false,
                                                                                                                pagiantionParams.Spec?.ToExpression(),
                                                                                                                includes).ConfigureAwait(false);

            return new PaginationDTO<StudentForMentorDTO>
            {
                DTOList = students.CheckList(i => students.Select(student => new StudentForMentorDTO
                {
                    Id = student.Id,
                    Name = student.Name,
                    Surname = student.Surname,
                    University = student.University,
                    Age = student.Age,
                    Dream = student.Dream,
                    HomeAddress = student.HomeAddress,
                    MentorThoughts = student.MentorThoughts,
                    GraduationStatus = student.GraduationStatus,
                    GraduationScore = student.GraduationScore,
                    MentorGraduationThoughts = student.MentorGraduationThoughts,
                    ProfessionId = student.ProfessionId,
                    Mentor = student.Mentor.CheckObject(i => new MentorDTO
                    {
                        AppUserId = i.AppUserId,
                        Name = i.Name,
                        Id = i.Id
                    }),
                    CurrentAssigmentDeliveryDate = student.CurrentAssigmentDeliveryDate,
                    OldAssignments = student.OldAssignments.CheckList(f => student.OldAssignments?.Select(oa => new StudentAssigmentDTO
                    {
                        AssigmentId = oa.Assigment.Id,
                    }))
                })),
                PageCount = pageCount,
                TotalDataCount = totalDataCount
            };
        }


        /// <summary>
        /// Get student information for admin by <paramref name="studentId"/>
        /// </summary>
        /// <param name="studentId"></param>
        /// <returns></returns>
        public async Task<StudentForAdminDTO> GetStudentForAdminAsync(Guid studentId)
        {
            Func<IIncludable<Student>, IIncludable> includes = i => i.Include(md => md.Mentor);

            Expression<Func<Student, bool>> condition = i => i.Id == studentId;

            var student = await _studentRepository.GetByIdAsync(studentId, includes).ConfigureAwait(false);

            student.ThrowIfNullForGuidObject();

            return new StudentForAdminDTO
            {
                Id = student.Id,
                Name = student.Name,
                Surname = student.Surname,
                University = student.University,
                Age = student.Age,
                HomeAddress = student.HomeAddress,
                MentorThoughts = student.MentorThoughts,
                IsConfidentialityAgreementSigned = student.IsConfidentialityAgreementSigned,
                GraduationStatus = student.GraduationStatus,
                GraduationScore = student.GraduationScore,
                MentorGraduationThoughts = student.MentorGraduationThoughts,
                ProfessionId = student.ProfessionId,
                Mentor = student.Mentor.CheckObject(i => new MentorDTO
                {
                    Id = student.MentorId
                }),
                CreationDate = student.CreationDate,
                LastModificationDate = student.LastModificationDate
            };

        }

        /// <summary>
        /// Get student information for mentor by <paramref name="studentId"/>.
        /// </summary>
        /// <param name="studentId"></param>
        /// <returns></returns>
        public async Task<StudentForMentorDTO> GetStudentForMentorAsync(Guid studentId)
        {
            Func<IIncludable<Student>, IIncludable> includes = i => i.Include(md => md.Mentor)
                                                                     .Include(oa => oa.OldAssignments);

            var student = await _studentRepository.GetByIdAsync(studentId, includes).ConfigureAwait(false);

            student.ThrowIfNullForGuidObject();

            return new StudentForMentorDTO
            {
                Name = student.Name,
                Surname = student.Surname,
                University = student.University,
                Age = student.Age,
                Dream = student.Dream,
                HomeAddress = student.HomeAddress,
                MentorThoughts = student.MentorThoughts,
                GraduationStatus = student.GraduationStatus,
                GraduationScore = student.GraduationScore,
                MentorGraduationThoughts = student.MentorGraduationThoughts,
                ProfessionId = student.ProfessionId,
                Mentor = student.Mentor.CheckObject(i => new MentorDTO
                {
                    Id = student.MentorId
                }),
                CurrentAssigmentDeliveryDate = student.CurrentAssigmentDeliveryDate,
                OldAssignments = student.OldAssignments.CheckList(f => student.OldAssignments?.Select(oa => new StudentAssigmentDTO
                {
                    AssigmentId = oa.Assigment.Id,
                }))
            };
        }

        /// <summary>
        /// Brings instant user's profile information.
        /// </summary>
        /// <returns></returns>
        public async Task<StudentForMentorDTO> GetCurrentUserProfile()
        {
            var currentStudent = await _userManager.FindByNameAsync(_loggedUser).ConfigureAwait(false);

            currentStudent.ThrowIfNullForGuidObject();

            return new StudentForMentorDTO
            {
                Name = currentStudent.Student.Name,
                Surname = currentStudent.Student.Surname,
                University = currentStudent.Student.University,
                Age = currentStudent.Student.Age,
                Dream = currentStudent.Student.Dream,
                HomeAddress = currentStudent.Student.HomeAddress,
                MentorThoughts = currentStudent.Student.MentorThoughts,
                GraduationStatus = currentStudent.Student.GraduationStatus,
                GraduationScore = currentStudent.Student.GraduationScore,
                MentorGraduationThoughts = currentStudent.Student.MentorGraduationThoughts,
                ProfessionId = currentStudent.Student.ProfessionId,
                Mentor = currentStudent.Student.Mentor.CheckObject(i => new MentorDTO
                {
                    Id = currentStudent.Student.MentorId
                }),
                CurrentAssigmentDeliveryDate = currentStudent.Student.CurrentAssigmentDeliveryDate,
                OldAssignments = currentStudent.Student.OldAssignments.CheckList(f => currentStudent.Student.OldAssignments?.Select(oa => new StudentAssigmentDTO
                {
                    AssigmentId = oa.Assigment.Id,
                }))
            };
        }

        /// <summary>
        /// Maps <paramref name="addStudentDTO"/> to <c><b>Student</b></c>  object and adds that product to repository.
        /// </summary>
        /// <param name="addStudentDTO">Student to be added.</param>
        /// <returns></returns>
        public async Task AddStudentAsync(AddStudentDTO addStudentDTO)
        {
            var appUser = new AppUser
            {
                UserName = addStudentDTO.UserName,
                Email = addStudentDTO.Email,
                PhoneNumber = addStudentDTO.PhoneNumber,
                Student = new Student
                {
                    Name = addStudentDTO.Name,
                    Surname = addStudentDTO.Surname,
                    University = addStudentDTO.University,
                    Age = addStudentDTO.Age,
                    Dream = addStudentDTO.Dream,
                    HomeAddress = addStudentDTO.HomeAddress,
                    MentorThoughts = addStudentDTO.MentorThoughts,
                    IsConfidentialityAgreementSigned = addStudentDTO.IsConfidentialityAgreementSigned,
                    GraduationStatus = addStudentDTO.GraduationStatus,
                    GraduationScore = addStudentDTO.GraduationScore,
                    MentorGraduationThoughts = addStudentDTO.MentorGraduationThoughts,
                    ProfessionId = addStudentDTO.ProfessionId,
                    MentorId = addStudentDTO.MentorId
                }
            };

            appUser.Mentor = null;

            var result = await _userManager.CreateAsync(appUser, addStudentDTO.Password);

            if (!result.Succeeded)
                throw new MilvaUserFriendlyException(string.Join("~", result.Errors.Select(m => m.Description)));
        }

        /// <summary>
        /// Updates single student which that equals <paramref name="updateStudentDTO"/> in repository by <paramref name="updateStudentDTO"/>'s properties.
        /// </summary>
        /// <param name="updateStudentDTO">Student to be updated.</param>
        /// <returns></returns>
        public async Task UpdateStudentAsync(UpdateStudentDTO updateStudentDTO)
        {
            var toBeUpdatedStudent = await _studentRepository.GetByIdAsync(updateStudentDTO.Id).ConfigureAwait(false);

            toBeUpdatedStudent.Name = updateStudentDTO.Name;
            toBeUpdatedStudent.Surname = updateStudentDTO.Surname;
            toBeUpdatedStudent.University = updateStudentDTO.University;
            toBeUpdatedStudent.Age = updateStudentDTO.Age;
            toBeUpdatedStudent.Dream = updateStudentDTO.Dream;
            toBeUpdatedStudent.HomeAddress = updateStudentDTO.HomeAddress;
            toBeUpdatedStudent.MentorThoughts = updateStudentDTO.MentorThoughts;
            toBeUpdatedStudent.IsConfidentialityAgreementSigned = updateStudentDTO.IsConfidentialityAgreementSigned;
            toBeUpdatedStudent.GraduationStatus = updateStudentDTO.GraduationStatus;
            toBeUpdatedStudent.GraduationScore = updateStudentDTO.GraduationScore;
            toBeUpdatedStudent.MentorGraduationThoughts = updateStudentDTO.MentorGraduationThoughts;
            toBeUpdatedStudent.ProfessionId = updateStudentDTO.ProfessionId;
            toBeUpdatedStudent.MentorId = updateStudentDTO.MentorId;

            await _studentRepository.UpdateAsync(toBeUpdatedStudent).ConfigureAwait(false);
        }

        /// <summary>
        /// The student can update himself.
        /// </summary>
        /// <param name="updateStudentDTO"></param>
        /// <returns></returns>
        public async Task UpdateCurrentStudentAsync(UpdateStudentDTO updateStudentDTO)
        {
            var currentStudent = await _userManager.FindByNameAsync(_loggedUser).ConfigureAwait(false);

            currentStudent.Student.Name = updateStudentDTO.Name;
            currentStudent.Student.Surname = updateStudentDTO.Surname;
            currentStudent.Student.ProfessionId = updateStudentDTO.ProfessionId;
            currentStudent.Student.Dream = updateStudentDTO.Dream;
            currentStudent.Student.HomeAddress = updateStudentDTO.HomeAddress;

            await _studentRepository.UpdateAsync(currentStudent.Student).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete students by <paramref name="studentIds"/>.
        /// </summary>
        /// <param name="studentIds"></param>
        /// <returns></returns>
        public async Task DeleteStudentsAsync(List<Guid> studentIds)
        {
            var deletedStudents = await _studentRepository.GetAllAsync(i => studentIds.Select(p => p).Contains(i.Id)).ConfigureAwait(false);

            await _studentRepository.DeleteAsync(deletedStudents).ConfigureAwait(false);
        }
    }
}
