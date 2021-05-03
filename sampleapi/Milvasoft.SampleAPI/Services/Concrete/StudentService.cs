using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.DataAccess.IncludeLibrary;
using Milvasoft.Helpers.Exceptions;
using Milvasoft.Helpers.Models;
using Milvasoft.SampleAPI.Data;
using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.DTOs.AssignmentDTOs;
using Milvasoft.SampleAPI.DTOs.MentorDTOs;
using Milvasoft.SampleAPI.DTOs.StudentAssignmentDTOs;
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
        #region Fields

        private readonly IHttpContextAccessor _httpContextAccessor ;
        private readonly UserManager<AppUser> _userManager;
        private readonly IBaseRepository<Student, Guid, EducationAppDbContext> _studentRepository;
        private readonly IBaseRepository<Mentor, Guid, EducationAppDbContext> _mentorRepository;

        #endregion

        /// <summary>
        /// Performs constructor injection for repository interfaces used in this service.
        /// </summary>
        /// <param name="studentRepository"></param>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="mentorRepository"></param>
        public StudentService(IBaseRepository<Student, Guid, EducationAppDbContext> studentRepository, UserManager<AppUser> userManager, IBaseRepository<Mentor, Guid, EducationAppDbContext> mentorRepository, IHttpContextAccessor httpContextAccessor)
        {
            _mentorRepository = mentorRepository;
            _userManager = userManager;
            _studentRepository = studentRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        #region CRUD Operations

        /// <summary>
        /// It will filter students according to the parameters sent in <paramref name="pagiantionParams"/>
        /// </summary>
        /// <exception cref="ArgumentNullException">If <paramref name="pagiantionParams"/> is null, it is thrown.</exception>
        /// <param name="pagiantionParams">Filtering object.</param>
        /// <returns>Student information that can be seen by admin.</returns>
        public async Task<PaginationDTO<StudentForAdminDTO>> GetStudentsForAdminAsync(PaginationParamsWithSpec<StudentSpec> pagiantionParams)
        {
            pagiantionParams.ThrowIfParameterIsNull();

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
                    Mentor = student.Mentor.CheckObject(i => new MentorForAdminDTO
                    {
                        Name=i.Name,
                        Surname=i.Surname,
                        Id = i.Id
                    })
                })),
                PageCount = pageCount,
                TotalDataCount = totalDataCount
            };
        }

        /// <summary>
        /// Brings the students of the mentor who has entered.
        /// </summary>
        ///  <exception cref="ArgumentNullException">If the mentor does not enter, it is thrown..</exception>
        /// <param name="pagiantionParams">Filtering object.</param>
        /// <returns>Brings the students for whom the mentor is responsible.</returns>
        public async Task<PaginationDTO<StudentForMentorDTO>> GetStudentsForCurrentMentorAsync(PaginationParamsWithSpec<StudentSpec> pagiantionParams)
        {
            var username = _httpContextAccessor.HttpContext.User.Identity.Name;

            var currentMentor = await _mentorRepository.GetFirstOrDefaultAsync(i => i.AppUser.UserName == username).ConfigureAwait(false);

            Func<IIncludable<Student>, IIncludable> includes = i => i.Include(md => md.Mentor)
                                                                        .Include(oa => oa.OldAssignments);

            var (students, pageCount, totalDataCount) = await _studentRepository.PreparePaginationDTO<IBaseRepository<Student, Guid, EducationAppDbContext>, Student, Guid>
                                                                                                                (pagiantionParams.PageIndex,
                                                                                                                pagiantionParams.RequestedItemCount,
                                                                                                                pagiantionParams.OrderByProperty = null,
                                                                                                                pagiantionParams.OrderByAscending = false,
                                                                                                                pagiantionParams.Spec?.ToExpression(),
                                                                                                                includes).ConfigureAwait(false);


            return new PaginationDTO<StudentForMentorDTO>
            {
                DTOList = students.CheckList(i => students.Select(students => students.MentorId==currentMentor.Id ? new StudentForMentorDTO
                {
                    Id = students.Id,
                    Name = students.Name,
                    Surname = students.Surname,
                    University = students.University,
                    Age = students.Age,
                    Dream = students.Dream,
                    HomeAddress = students.HomeAddress,
                    MentorThoughts = students.MentorThoughts,
                    GraduationStatus = students.GraduationStatus,
                    GraduationScore = students.GraduationScore,
                    MentorGraduationThoughts = students.MentorGraduationThoughts,
                    ProfessionId = students.ProfessionId,
                    Mentor = students.Mentor.CheckObject(i => new MentorDTO
                    {
                        AppUserId = i.AppUserId,
                        Name = i.Name,
                        Id = i.Id
                    }),
                    CurrentAssigmentDeliveryDate = students.CurrentAssigmentDeliveryDate,
                    OldAssignments = students.OldAssignments.CheckList(f =>students.OldAssignments.Select(oa => oa.Status!=Entity.Enum.EducationStatus.InProgress ? new StudentAssignmentDTO
                    {
                        Id = oa.Id,
                    }:null))
                }:null)),
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
                Mentor = student.Mentor.CheckObject(i => new MentorForAdminDTO
                {
                    Name=i.Name,
                    Surname=i.Surname
                }),
                CreationDate = student.CreationDate,
                LastModificationDate = student.LastModificationDate
            };

        }

        /// <summary>
        /// Brings the student who gave the <paramref name="studentId"/> of the logged in mentor..
        /// </summary>
        /// <param name="studentId"></param>
        /// <returns></returns>
        public async Task<StudentForMentorDTO> GetStudentForMentorAsync(Guid studentId)
        {
            var username = _httpContextAccessor.HttpContext.User.Identity.Name;

            var currentMentor = await _mentorRepository.GetFirstOrDefaultAsync(i => i.AppUser.UserName == username).ConfigureAwait(false);

            Func<IIncludable<Student>, IIncludable> includes = i => i.Include(md => md.Mentor)
                                                                     .Include(oa => oa.OldAssignments);

            var student = await _studentRepository.GetByIdAsync(studentId, includes, i => i.MentorId == currentMentor.Id).ConfigureAwait(false);

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
                OldAssignments = student.OldAssignments.CheckList(f => student.OldAssignments?.Select(oa => new StudentAssignmentDTO
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
            var username = _httpContextAccessor.HttpContext.User.Identity.Name;

            var currentStudent = await _studentRepository.GetFirstOrDefaultAsync(i => i.AppUser.UserName == username).ConfigureAwait(false);

            currentStudent.ThrowIfNullForGuidObject();

            return new StudentForMentorDTO
            {
                Name = currentStudent.Name,
                Surname = currentStudent.Surname,
                University = currentStudent.University,
                Age = currentStudent.Age,
                Dream = currentStudent.Dream,
                HomeAddress = currentStudent.HomeAddress,
                MentorThoughts = currentStudent.MentorThoughts,
                GraduationStatus = currentStudent.GraduationStatus,
                GraduationScore = currentStudent.GraduationScore,
                MentorGraduationThoughts = currentStudent.MentorGraduationThoughts,
                ProfessionId = currentStudent.ProfessionId,
                Mentor = currentStudent.Mentor.CheckObject(i => new MentorDTO
                {
                    Name=i.Name,
                    Surname=i.Surname
                }),
                CurrentAssigmentDeliveryDate = currentStudent.CurrentAssigmentDeliveryDate,
                OldAssignments = currentStudent.OldAssignments.CheckList(f => currentStudent.OldAssignments?.Select(oa => new StudentAssignmentDTO
                {
                    AssigmentId = oa.Assigment.Id  
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


            var result = await _userManager.CreateAsync(appUser, addStudentDTO.Password);

            if (!result.Succeeded)
                throw new MilvaUserFriendlyException(string.Join("~", result.Errors.Select(m => m.Description)));
        }

        /// <summary>
        /// Updates single student which that equals <paramref name="updateStudentDTO"/> in repository by <paramref name="updateStudentDTO"/>'s properties by mentor.
        /// </summary>
        /// <param name="updateStudentDTO">Student to be updated.</param>
        /// <param name="Id">Id of student to be updated.</param>
        /// <returns></returns>
        public async Task UpdateStudentByAdminAsync(UpdateStudentByAdminDTO updateStudentDTO, Guid Id)
        {
            var toBeUpdatedStudent = await _studentRepository.GetByIdAsync(Id).ConfigureAwait(false);

            toBeUpdatedStudent.Name = updateStudentDTO.Name;
            toBeUpdatedStudent.Surname = updateStudentDTO.Surname;
            toBeUpdatedStudent.University = updateStudentDTO.University;
            toBeUpdatedStudent.Age = updateStudentDTO.Age;
            toBeUpdatedStudent.IsConfidentialityAgreementSigned = updateStudentDTO.IsConfidentialityAgreementSigned;
            toBeUpdatedStudent.MentorId = updateStudentDTO.MentorId;

            await _studentRepository.UpdateAsync(toBeUpdatedStudent).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates single student which that equals <paramref name="updateStudentDTO"/> in repository by <paramref name="updateStudentDTO"/>'s properties by mentor.
        /// </summary>
        /// <param name="updateStudentDTO">Student to be updated.</param>
        /// <param name="Id">Id of student to be updated.</param>
        /// <returns></returns>
        public async Task UpdateStudentByMentorAsync(UpdateStudentByMentorDTO updateStudentDTO,Guid Id)
        {
            var toBeUpdatedStudent = await _studentRepository.GetByIdAsync(Id).ConfigureAwait(false);

            toBeUpdatedStudent.Name = updateStudentDTO.Name;
            toBeUpdatedStudent.Surname = updateStudentDTO.Surname;
            toBeUpdatedStudent.Level = updateStudentDTO.Level;
            toBeUpdatedStudent.University = updateStudentDTO.University;
            toBeUpdatedStudent.Age = updateStudentDTO.Age;
            toBeUpdatedStudent.HomeAddress = updateStudentDTO.HomeAddress;
            toBeUpdatedStudent.MentorThoughts = updateStudentDTO.MentorThoughts;
            toBeUpdatedStudent.GraduationStatus = updateStudentDTO.GraduationStatus;
            toBeUpdatedStudent.CurrentAssigmentDeliveryDate = updateStudentDTO.CurrentAssigmentDeliveryDate;
            toBeUpdatedStudent.GraduationScore = updateStudentDTO.GraduationScore;
            toBeUpdatedStudent.MentorGraduationThoughts = updateStudentDTO.MentorGraduationThoughts;
            toBeUpdatedStudent.ProfessionId = updateStudentDTO.ProfessionId;

            await _studentRepository.UpdateAsync(toBeUpdatedStudent).ConfigureAwait(false);
        }

        /// <summary>
        /// The student can update himself.
        /// </summary>
        /// <param name="updateStudentDTO"></param>
        /// <returns></returns>
        public async Task UpdateCurrentStudentAsync(UpdateStudentDTO updateStudentDTO)
        {
            var username = _httpContextAccessor.HttpContext.User.Identity.Name;

            var currentStudent = await _userManager.FindByNameAsync(username).ConfigureAwait(false);

            currentStudent.Student.Name = updateStudentDTO.Name;
            currentStudent.Student.Surname = updateStudentDTO.Surname;
            currentStudent.Student.University = updateStudentDTO.University;
            currentStudent.Student.Age = updateStudentDTO.Age;
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

        #endregion 
    }
}
