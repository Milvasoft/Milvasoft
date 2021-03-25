using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.DataAccess.IncludeLibrary;
using Milvasoft.SampleAPI.Data;
using Milvasoft.SampleAPI.DTOs.AnnouncementDTOs;
using Milvasoft.SampleAPI.DTOs.MentorDTOs;
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
    /// Mentor service. 
    /// </summary>
    public class MentorService : IMentorService
    {

        private readonly IBaseRepository<Mentor, Guid, EducationAppDbContext> _mentorRepository;

        /// <summary>
        /// Performs constructor injection for repository interfaces used in this service.
        /// </summary>
        /// <param name="mentorRepository"></param>
        public MentorService(IBaseRepository<Mentor, Guid, EducationAppDbContext> mentorRepository)
        {
            _mentorRepository = mentorRepository;
        }

        /// <summary>
        /// Get mentors for admin.
        /// </summary>
        /// <returns></returns>
        public async Task<List<MentorDTO>> GetEntitiesForAdmin(MentorSpec spec = null)
        {

            Func<IIncludable<Mentor>, IIncludable> includes = i => i.Include(p => p.PublishedAnnouncements)
                                                                     .Include(s => s.Students)
                                                                     .Include(p => p.Professions);

            var mentors = await _mentorRepository.GetAllAsync(includes, spec?.ToExpression()).ConfigureAwait(false);

            var mentorsDTO = from mentor in mentors
                             select new MentorDTO
                             {
                                 Name = mentor.Name,
                                 Surname = mentor.Surname,
                                 CVFilePath = mentor.CVFilePath,
                                 AppUserId = mentor.Id,
                                 CreationDate = mentor.CreationDate,
                                 LastModificationDate = mentor.LastModificationDate,
                                 Professions = mentor.Professions.CheckList(i => mentor.Professions?.Select(pr => new MentorProfessionDTO
                                 {
                                     Id = pr.ProfessionId
                                 })),
                                 PublishedAnnouncements = mentor.PublishedAnnouncements.CheckList(i => mentor.PublishedAnnouncements?.Select(pa => new AnnouncementDTO
                                 {
                                     Id = pa.Id
                                 })),
                                 Students = mentor.Students.CheckList(i => mentor.Students?.Select(st => new StudentDTO
                                 {
                                     Id = st.Id
                                 }))
                             };
            return mentorsDTO.ToList();
        }

        /// <summary>
        /// Get mentors for mentor.
        /// </summary>
        /// <returns></returns>
        public async Task<List<MentorDTO>> GetEntitiesForMentor(MentorSpec spec = null)
        {

            Func<IIncludable<Mentor>, IIncludable> includes = i => i.Include(p => p.PublishedAnnouncements)
                                                                     .Include(s => s.Students)
                                                                     .Include(p => p.Professions);

            var mentors = await _mentorRepository.GetAllAsync(includes, spec?.ToExpression()).ConfigureAwait(false);

            var mentorsDTO = from mentor in mentors
                             select new MentorDTO
                             {
                                 Name = mentor.Name,
                                 Surname = mentor.Surname,
                                 CreationDate = mentor.CreationDate,
                                 Professions = mentor.Professions.CheckList(i => mentor.Professions?.Select(pr => new MentorProfessionDTO
                                 {
                                     Id = pr.ProfessionId
                                 })),
                                 PublishedAnnouncements = mentor.PublishedAnnouncements.CheckList(i => mentor.PublishedAnnouncements?.Select(pa => new AnnouncementDTO
                                 {
                                     Id = pa.Id
                                 })),
                                 Students = mentor.Students.CheckList(i => mentor.Students?.Select(st => new StudentDTO
                                 {
                                     Id = st.Id
                                 }))
                             };
            return mentorsDTO.ToList();
        }

        /// <summary>
        /// Get mentors for student.
        /// </summary>
        /// <returns></returns>
        public async Task<List<MentorDTO>> GetEntitiesForStudent(MentorSpec spec = null)
        {

            Func<IIncludable<Mentor>, IIncludable> includes = i => i.Include(p => p.PublishedAnnouncements)
                                                                     .Include(p => p.Professions);

            var mentors = await _mentorRepository.GetAllAsync(includes, spec?.ToExpression()).ConfigureAwait(false);
            var mentorsDTO = from mentor in mentors
                             select new MentorDTO
                             {
                                 Name = mentor.Name,
                                 Surname = mentor.Surname,
                                 Professions = mentor.Professions.CheckList(i => mentor.Professions?.Select(pr => new MentorProfessionDTO
                                 {
                                     Id = pr.ProfessionId
                                 })),
                                 PublishedAnnouncements = mentor.PublishedAnnouncements.CheckList(i => mentor.PublishedAnnouncements?.Select(pa => new AnnouncementDTO
                                 {
                                     Id = pa.Id
                                 }))
                             };
            return mentorsDTO.ToList();
        }

        /// <summary>
        /// Get mentor for admin by <paramref name="id"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<MentorDTO> GetEntityForAdmin(Guid id)
        {

            Func<IIncludable<Mentor>, IIncludable> includes = i => i.Include(p => p.PublishedAnnouncements)
                                                                     .Include(s => s.Students)
                                                                     .Include(p => p.Professions);

            var mentor = await _mentorRepository.GetByIdAsync(id, includes).ConfigureAwait(false);
            return new MentorDTO
            {
                Name = mentor.Name,
                Surname = mentor.Surname,
                CVFilePath = mentor.CVFilePath,
                CreationDate = mentor.CreationDate,
                LastModificationDate = mentor.LastModificationDate,
                Professions = mentor.Professions.CheckList(i => mentor.Professions?.Select(pr => new MentorProfessionDTO
                {
                    Id = pr.ProfessionId
                })),
                PublishedAnnouncements = mentor.PublishedAnnouncements.CheckList(i => mentor.PublishedAnnouncements?.Select(pa => new AnnouncementDTO
                {
                    Id = pa.Id
                })),
                Students = mentor.Students.CheckList(i => mentor.Students?.Select(st => new StudentDTO
                {
                    Id = st.Id
                }))
            };
        }

        /// <summary>
        /// Get mentor for mentor by <paramref name="id"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<MentorDTO> GetEntityForMentor(Guid id)
        {

            Func<IIncludable<Mentor>, IIncludable> includes = i => i.Include(p => p.PublishedAnnouncements)
                                                                     .Include(s => s.Students)
                                                                     .Include(p => p.Professions);

            var mentor = await _mentorRepository.GetByIdAsync(id, includes).ConfigureAwait(false);
            return new MentorDTO
            {
                Name = mentor.Name,
                Surname = mentor.Surname,
                CreationDate = mentor.CreationDate,
                Professions = mentor.Professions.CheckList(i => mentor.Professions?.Select(pr => new MentorProfessionDTO
                {
                    Id = pr.ProfessionId
                })),
                PublishedAnnouncements = mentor.PublishedAnnouncements.CheckList(i => mentor.PublishedAnnouncements?.Select(pa => new AnnouncementDTO
                {
                    Id = pa.Id
                })),
                Students = mentor.Students.CheckList(i => mentor.Students?.Select(st => new StudentDTO
                {
                    Id = st.Id
                }))
            };
        }

        /// <summary>
        /// Get mentor for student by <paramref name="id"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<MentorDTO> GetEntityForStudent(Guid id)
        {

            Func<IIncludable<Mentor>, IIncludable> includes = i => i.Include(p => p.PublishedAnnouncements)
                                                                    .Include(p => p.Professions);

            var mentor = await _mentorRepository.GetByIdAsync(id, includes).ConfigureAwait(false);
            return new MentorDTO
            {
                Name = mentor.Name,
                Surname = mentor.Surname,
                Professions = mentor.Professions.CheckList(i => mentor.Professions?.Select(pr => new MentorProfessionDTO
                {
                    Id = pr.ProfessionId
                })),
                PublishedAnnouncements = mentor.PublishedAnnouncements.CheckList(i => mentor.PublishedAnnouncements?.Select(pa => new AnnouncementDTO
                {
                    Id = pa.Id
                }))
            };
        }

        /// <summary>
        /// Add mentor.
        /// </summary>
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        public async Task AddEntityAsync(MentorDTO educationDTO)
        {
            var mentor = new Mentor
            {
                Name = educationDTO.Name,
                Surname = educationDTO.Surname,
                CVFilePath = educationDTO.CVFilePath,
                CreationDate = educationDTO.CreationDate,
                LastModificationDate = educationDTO.LastModificationDate
            };
            await _mentorRepository.AddAsync(mentor).ConfigureAwait(false);
        }

        /// <summary>
        /// Update mentor.
        /// </summary>
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        public async Task UpdateEntityAsync(MentorDTO educationDTO)
        {
            var updatedMentor = await _mentorRepository.GetByIdAsync(educationDTO.Id).ConfigureAwait(false);

            updatedMentor.Name = educationDTO.Name;

            updatedMentor.Surname = educationDTO.Surname;

            updatedMentor.LastModificationDate = DateTime.Now;

            updatedMentor.LastModifierUserId = educationDTO.Id;

            await _mentorRepository.UpdateAsync(updatedMentor).ConfigureAwait(false);

        }

        /// <summary>
        /// Delete mentors.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task DeleteEntities(List<Guid> ids)
        {

            var mentors = await _mentorRepository.GetAllAsync(i => ids.Select(p => p).Contains(i.Id)).ConfigureAwait(false);

            await _mentorRepository.DeleteAsync(mentors).ConfigureAwait(false);

        }

        /// <summary>
        /// Delete mentor.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteEntityAsync(Guid id)
        {

            var deletedMentor = await _mentorRepository.GetByIdAsync(id).ConfigureAwait(false);

            await _mentorRepository.DeleteAsync(deletedMentor).ConfigureAwait(false);

        }

    }
}
