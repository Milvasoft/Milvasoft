﻿using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.DataAccess.IncludeLibrary;
using Milvasoft.Helpers.Models;
using Milvasoft.SampleAPI.Data;
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

        private readonly IBaseRepository<Student, Guid, EducationAppDbContext> _studentRepository;

        /// <summary>
        /// Performs constructor injection for repository interfaces used in this service.
        /// </summary>
        /// <param name="studentRepository"></param>
        public StudentService(IBaseRepository<Student, Guid, EducationAppDbContext> studentRepository)
        {

            _studentRepository = studentRepository;

        }

        /// <summary>
        /// Get students for admin.
        /// </summary>
        /// <returns></returns>
        public async Task<PaginationDTO<StudentDTO>> GetEntitiesForAdminAsync(int pageIndex,
                                                                               int requestedItemCount,
                                                                               string orderByProperty = null,
                                                                               bool orderByAscending = false, 
                                                                               StudentSpec studentSpec = null)
        {
            Func<IIncludable<Student>, IIncludable> includes = i => i.Include(md => md.Mentor);

            var (students, pageCount, totalDataCount) = await _studentRepository.PreparePaginationDTO<IBaseRepository<Student, Guid, EducationAppDbContext>, Student, Guid>
                                                                                                                (pageIndex, requestedItemCount, orderByProperty, orderByAscending, studentSpec?.ToExpression(),includes).ConfigureAwait(false);

            return new PaginationDTO<StudentDTO>
            {
                DTOList = students.CheckList(i => students.Select(student => new StudentDTO
                {
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
        public async Task<PaginationDTO<StudentDTO>> GetEntitiesForMentorAsync(int pageIndex,
                                                                               int requestedItemCount,
                                                                               string orderByProperty = null,
                                                                               bool orderByAscending = false,
                                                                               StudentSpec studentSpec = null)
        {
            Func<IIncludable<Student>, IIncludable> includes = i => i.Include(md => md.Mentor)
                                                                        .Include(assi => assi.OldAssignments);


            var (students, pageCount, totalDataCount) = await _studentRepository.PreparePaginationDTO<IBaseRepository<Student, Guid, EducationAppDbContext>, Student, Guid>
                                                                                                                (pageIndex, requestedItemCount, orderByProperty, orderByAscending, studentSpec?.ToExpression(),includes).ConfigureAwait(false);

            return new PaginationDTO<StudentDTO>
            {
                DTOList = students.CheckList(i => students.Select(student => new StudentDTO
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
        /// Get students for student.
        /// </summary>
        /// <returns></returns>
        public async Task<PaginationDTO<StudentDTO>> GetEntitiesForStudentAsync(int pageIndex,
                                                                               int requestedItemCount,
                                                                               string orderByProperty = null,
                                                                               bool orderByAscending = false,
                                                                               StudentSpec studentSpec = null)
        {
            var (students, pageCount, totalDataCount) = await _studentRepository.PreparePaginationDTO<IBaseRepository<Student, Guid, EducationAppDbContext>, Student, Guid>
                                                                                                                (pageIndex, requestedItemCount, orderByProperty, orderByAscending, studentSpec?.ToExpression()).ConfigureAwait(false);

            return new PaginationDTO<StudentDTO>
            {
                DTOList = students.CheckList(i => students.Select(student => new StudentDTO
                {
                    Name = student.Name,
                    Surname = student.Surname,
                    University = student.University,
                    Age = student.Age,
                    Dream = student.Dream,
                    HomeAddress = student.HomeAddress
                })),
                PageCount = pageCount,
                TotalDataCount = totalDataCount
            };
        }

        /// <summary>
        /// Get student information by <paramref name="studenId"/> 
        /// </summary>
        /// <param name="studenId"></param>
        /// <returns></returns>
        public async Task<StudentDTO> GetEntityForStudentAsync(Guid studenId)
        {
            var student = await _studentRepository.GetByIdAsync(studenId).ConfigureAwait(false);

            return new StudentDTO
            {
                Name = student.Name,
                Surname = student.Surname,
                University = student.University,
                Age = student.Age,
                Dream = student.Dream,
                HomeAddress = student.HomeAddress
            };
        }

        /// <summary>
        /// Get student information for admin by <paramref name="studenId"/>
        /// </summary>
        /// <param name="studenId"></param>
        /// <returns></returns>
        public async Task<StudentDTO> GetEntityForAdminAsync(Guid studenId)
        {
            Func<IIncludable<Student>, IIncludable> includes = i => i.Include(md => md.Mentor);

            Expression<Func<Student, bool>> condition = i => i.Id == studenId;

            var student = await _studentRepository.GetByIdAsync(studenId, includes).ConfigureAwait(false);

            return new StudentDTO
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
        /// Get student information for mentor by <paramref name="studenId"/>.
        /// </summary>
        /// <param name="studenId"></param>
        /// <returns></returns>
        public async Task<StudentDTO> GetEntityForMentorAsync(Guid studenId)
        {
            Func<IIncludable<Student>, IIncludable> includes = i => i.Include(md => md.Mentor)
                                                                     .Include(oa => oa.OldAssignments);

            var student = await _studentRepository.GetByIdAsync(studenId, includes).ConfigureAwait(false);

            return new StudentDTO
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
        /// Add student to database.
        /// </summary>
        /// <param name="addStudentDTO"></param>
        /// <returns></returns>
        public async Task AddEntityAsync(AddStudentDTO addStudentDTO)
        {
            var newStudent = new Student
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
            };

            await _studentRepository.AddAsync(newStudent).ConfigureAwait(false);
        }

        /// <summary>
        /// Update student.
        /// </summary>
        /// <param name="updateStudentDTO"></param>
        /// <returns></returns>
        public async Task UpdateEntityAsync(UpdateStudentDTO updateStudentDTO)
        {
            var updatedStudent = await _studentRepository.GetByIdAsync(updateStudentDTO.Id).ConfigureAwait(false);

            updatedStudent.Name = updateStudentDTO.Name;
            updatedStudent.Surname = updateStudentDTO.Surname;
            updatedStudent.University = updateStudentDTO.University;
            updatedStudent.Age = updateStudentDTO.Age;
            updatedStudent.Dream = updateStudentDTO.Dream;
            updatedStudent.HomeAddress = updateStudentDTO.HomeAddress;
            updatedStudent.MentorThoughts = updateStudentDTO.MentorThoughts;
            updatedStudent.IsConfidentialityAgreementSigned = updateStudentDTO.IsConfidentialityAgreementSigned;
            updatedStudent.GraduationStatus = updateStudentDTO.GraduationStatus;
            updatedStudent.GraduationScore = updateStudentDTO.GraduationScore;
            updatedStudent.MentorGraduationThoughts = updateStudentDTO.MentorGraduationThoughts;
            updatedStudent.ProfessionId = updateStudentDTO.ProfessionId;
            updatedStudent.MentorId = updateStudentDTO.MentorId;

            await _studentRepository.UpdateAsync(updatedStudent).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete student.
        /// </summary>
        /// <param name="studentId"></param>
        /// <returns></returns>
        public async Task DeleteEntityAsync(Guid studentId)
        {
            var student = await _studentRepository.GetByIdAsync(studentId).ConfigureAwait(false);

            await _studentRepository.DeleteAsync(student);
        }

        /// <summary>
        /// Delete students by <paramref name="studentIds"/>.
        /// </summary>
        /// <param name="studentIds"></param>
        /// <returns></returns>
        public async Task DeleteEntitiesAsync(List<Guid> studentIds)
        {
            var deletedStudents = await _studentRepository.GetAllAsync(i => studentIds.Select(p => p).Contains(i.Id)).ConfigureAwait(false);

            await _studentRepository.DeleteAsync(deletedStudents).ConfigureAwait(false);
        }
    }
}
