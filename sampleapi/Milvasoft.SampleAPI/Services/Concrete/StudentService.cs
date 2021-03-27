using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.DataAccess.IncludeLibrary;
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
        public async Task<List<StudentDTO>> GetEntitiesForAdminAsync(StudentSpec spec = null)
        {

            Func<IIncludable<Student>, IIncludable> includes = i => i.Include(md => md.Mentor);


            var students = await _studentRepository.GetAllAsync(includes, spec?.ToExpression()).ConfigureAwait(false);

            var studentDTOList = from student in students
                                 select new StudentDTO
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
                                     }),
                                     CreationDate = student.CreationDate,
                                     LastModificationDate = student.LastModificationDate,
                                 };

            return studentDTOList.ToList();

        }

        /// <summary>
        /// Get students for mentor.
        /// </summary>
        /// <returns></returns>
        public async Task<List<StudentDTO>> GetEntitiesForMentorAsync(StudentSpec spec = null)
        {

            Func<IIncludable<Student>, IIncludable> includeMentor = i => i.Include(md => md.Mentor)
                                                                        .Include(assi => assi.OldAssignments);


            var students = await _studentRepository.GetAllAsync(includeMentor, spec?.ToExpression()).ConfigureAwait(false);

            var studentDTOList = from student in students
                                 select new StudentDTO
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
                                 };

            return studentDTOList.ToList();

        }

        /// <summary>
        /// Get students for student.
        /// </summary>
        /// <returns></returns>
        public async Task<List<StudentDTO>> GetEntitiesForStudentAsync(StudentSpec spec = null)
        {

            var students = await _studentRepository.GetAllAsync(spec?.ToExpression()).ConfigureAwait(false);

            var studentDTOList = from student in students
                                 select new StudentDTO
                                 {
                                     Name = student.Name,
                                     Surname = student.Surname,
                                     University = student.University,
                                     Age = student.Age,
                                     Dream = student.Dream,
                                     HomeAddress = student.HomeAddress
                                 };

            return studentDTOList.ToList();

        }

        /// <summary>
        /// Get student information by <paramref name="id"/> 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<StudentDTO> GetEntityForStudentAsync(Guid id)
        {

            var student = await _studentRepository.GetByIdAsync(id).ConfigureAwait(false);

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
        /// Get student information for admin by <paramref name="id"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<StudentDTO> GetEntityForAdminAsync(Guid id)
        {
            Func<IIncludable<Student>, IIncludable> includes = i => i.Include(md => md.Mentor);


            Expression<Func<Student, bool>> condition = i => i.Id == id;

            var student = await _studentRepository.GetByIdAsync(id, includes).ConfigureAwait(false);

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
                LastModificationDate = student.LastModificationDate,
                LastModifierUserId = student.LastModifierUserId,
            };

        }

        /// <summary>
        /// Get student information for mentor by <paramref name="id"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<StudentDTO> GetEntityForMentorAsync(Guid id)
        {

            Func<IIncludable<Student>, IIncludable> includes = i => i.Include(md => md.Mentor)
                                                                     .Include(oa => oa.OldAssignments);

            var student = await _studentRepository.GetByIdAsync(id, includes).ConfigureAwait(false);

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
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        public async Task AddEntityAsync(AddStudentDTO educationDTO)
        {

            var newStudent = new Student
            {
                Name = educationDTO.Name,
                Surname = educationDTO.Surname,
                University = educationDTO.University,
                Age = educationDTO.Age,
                Dream = educationDTO.Dream,
                HomeAddress = educationDTO.HomeAddress,
                MentorThoughts = educationDTO.MentorThoughts,
                IsConfidentialityAgreementSigned = educationDTO.IsConfidentialityAgreementSigned,
                GraduationStatus = educationDTO.GraduationStatus,
                GraduationScore = educationDTO.GraduationScore,
                MentorGraduationThoughts = educationDTO.MentorGraduationThoughts,
                ProfessionId = educationDTO.ProfessionId,
                MentorId = educationDTO.MentorId,
                CreationDate = DateTime.Now
            };

            await _studentRepository.AddAsync(newStudent).ConfigureAwait(false);

        }

        /// <summary>
        /// Update student.
        /// </summary>
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        public async Task UpdateEntityAsync(UpdateStudentDTO educationDTO)
        {

            var updatedStudent = await _studentRepository.GetByIdAsync(educationDTO.Id).ConfigureAwait(false);

            updatedStudent.Name = educationDTO.Name;

            updatedStudent.Surname = educationDTO.Surname;

            updatedStudent.University = educationDTO.University;

            updatedStudent.Age = educationDTO.Age;

            updatedStudent.Dream = educationDTO.Dream;

            updatedStudent.HomeAddress = educationDTO.HomeAddress;

            updatedStudent.MentorThoughts = educationDTO.MentorThoughts;

            updatedStudent.IsConfidentialityAgreementSigned = educationDTO.IsConfidentialityAgreementSigned;

            updatedStudent.GraduationStatus = educationDTO.GraduationStatus;

            updatedStudent.GraduationScore = educationDTO.GraduationScore;

            updatedStudent.MentorGraduationThoughts = educationDTO.MentorGraduationThoughts;

            updatedStudent.ProfessionId = educationDTO.ProfessionId;

            updatedStudent.MentorId = educationDTO.MentorId;

            updatedStudent.LastModificationDate = DateTime.Now;

            await _studentRepository.UpdateAsync(updatedStudent).ConfigureAwait(false);

        }

        /// <summary>
        /// Delete student.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteEntityAsync(Guid id)
        {

            var student = await _studentRepository.GetByIdAsync(id).ConfigureAwait(false);

            await _studentRepository.DeleteAsync(student);

        }

        /// <summary>
        /// Delete students by <paramref name="ids"/>.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task DeleteEntitiesAsync(List<Guid> ids)
        {

            var deletedStudents = await _studentRepository.GetAllAsync(i => ids.Select(p => p).Contains(i.Id)).ConfigureAwait(false);

            await _studentRepository.DeleteAsync(deletedStudents).ConfigureAwait(false);

        }
    }
}
