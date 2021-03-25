using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.DataAccess.IncludeLibrary;
using Milvasoft.SampleAPI.Data;
using Milvasoft.SampleAPI.DTOs.AnnouncementDTOs;
using Milvasoft.SampleAPI.DTOs.MentorDTOs;
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
    /// Announcement service.
    /// </summary>
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IBaseRepository<Announcement, Guid, EducationAppDbContext> _announcementRepository;

        /// <summary>
        /// Performs constructor injection for repository interfaces used in this service.
        /// </summary>
        /// <param name="announcementRepository"></param>
        public AnnouncementService(IBaseRepository<Announcement, Guid, EducationAppDbContext> announcementRepository)
        {

            _announcementRepository = announcementRepository;

        }

        /// <summary>
        /// Get all announcement for student.
        /// </summary>
        /// <returns></returns>
        public async Task<List<AnnouncementDTO>> GetEntitiesForStudent(AnnouncementSpec spec = null)
        {

            Func<IIncludable<Announcement>, IIncludable> includes = i => i.Include(md => md.PublisherMentor);


            var announcements = await _announcementRepository.GetAllAsync(includes, spec?.ToExpression()).ConfigureAwait(false);

            var annoncemementDTOList = from announcement in announcements
                                       select new AnnouncementDTO
                                       {
                                           Title = announcement.Title,
                                           Description = announcement.Description,
                                           PublisherMentor = announcement.PublisherMentor.CheckObject(i => new MentorDTO
                                           {
                                               Id = i.Id
                                           }),
                                           CreationDate = announcement.CreationDate,
                                       };

            return annoncemementDTOList.ToList();

        }

        /// <summary>
        /// Get all announcement for admin.
        /// </summary>
        /// <returns></returns>
        public async Task<List<AnnouncementDTO>> GetEntitiesForAdmin(AnnouncementSpec spec = null)
        {

            Func<IIncludable<Announcement>, IIncludable> includes = i => i.Include(md => md.PublisherMentor);

            var announcements = await _announcementRepository.GetAllAsync(includes, spec?.ToExpression()).ConfigureAwait(false);

            var annoncemementDTOList = from announcement in announcements
                                       select new AnnouncementDTO
                                       {
                                           Id = announcement.Id,
                                           Title = announcement.Title,
                                           Description = announcement.Description,
                                           IsFixed = announcement.IsFixed,
                                           PublisherMentor = announcement.PublisherMentor.CheckObject(i => new MentorDTO
                                           {
                                               Id = i.Id
                                           }),
                                           CreationDate = announcement.CreationDate,
                                           CreatorUserId = announcement.MentorId,
                                           LastModificationDate = announcement.LastModificationDate
                                       };

            return annoncemementDTOList.ToList();
        }

        /// <summary>
        /// Get all announcement for mentor.
        /// </summary>
        /// <returns></returns>
        public async Task<List<AnnouncementDTO>> GetEntitiesForMentor(AnnouncementSpec spec = null)
        {

            Func<IIncludable<Announcement>, IIncludable> includes = i => i.Include(md => md.PublisherMentor);

            var announcements = await _announcementRepository.GetAllAsync(includes, spec?.ToExpression()).ConfigureAwait(false);

            var annoncemementDTOList = from announcement in announcements
                                       select new AnnouncementDTO
                                       {
                                           Title = announcement.Title,
                                           Description = announcement.Description,
                                           IsFixed = announcement.IsFixed,
                                           PublisherMentor = announcement.PublisherMentor.CheckObject(i => new MentorDTO
                                           {
                                               Id = i.Id
                                           }),
                                           CreationDate = announcement.CreationDate
                                       };

            return annoncemementDTOList.ToList();
        }

        /// <summary>
        /// Get announcement for admin by <paramref name="id"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<AnnouncementDTO> GetEntityForAdmin(Guid id)
        {

            Func<IIncludable<Announcement>, IIncludable> includes = i => i.Include(md => md.PublisherMentor);

            var announcement = await _announcementRepository.GetByIdAsync(id, includes).ConfigureAwait(false);

            return new AnnouncementDTO
            {
                Id = announcement.Id,
                Title = announcement.Title,
                Description = announcement.Description,
                IsFixed = announcement.IsFixed,
                PublisherMentor = announcement.PublisherMentor.CheckObject(i => new MentorDTO
                {
                    Id = i.Id
                }),
                CreationDate = announcement.CreationDate,
                CreatorUserId = announcement.MentorId,
                LastModificationDate = announcement.LastModificationDate
            };
        }

        /// <summary>
        /// Get entity for mentor by <paramref name="id"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<AnnouncementDTO> GetEntityForMentor(Guid id)
        {

            Func<IIncludable<Announcement>, IIncludable> includes = i => i.Include(md => md.PublisherMentor);

            var announcement = await _announcementRepository.GetByIdAsync(id, includes).ConfigureAwait(false);

            return new AnnouncementDTO
            {
                Title = announcement.Title,
                Description = announcement.Description,
                IsFixed = announcement.IsFixed,
                PublisherMentor = announcement.PublisherMentor.CheckObject(i => new MentorDTO
                {
                    Id = i.Id
                }),
                CreationDate = announcement.CreationDate
            };
        }

        /// <summary>
        /// Get announcement for student by <paramref name="id"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<AnnouncementDTO> GetEntityForStudent(Guid id)
        {

            Func<IIncludable<Announcement>, IIncludable> includes = i => i.Include(md => md.PublisherMentor);

            var announcement = await _announcementRepository.GetByIdAsync(id, includes).ConfigureAwait(false);

            return new AnnouncementDTO
            {
                Title = announcement.Title,
                Description = announcement.Description,
                PublisherMentor = announcement.PublisherMentor.CheckObject(i => new MentorDTO
                {
                    Id = i.Id
                }),
                CreationDate = announcement.CreationDate,
            };
        }

        /// <summary>
        /// Add new announcement.
        /// </summary>
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        public async Task AddEntityAsync(AnnouncementDTO educationDTO)
        {

            var newAnnnouncement = new Announcement
            {
                Title = educationDTO.Title,
                Description = educationDTO.Description,
                IsFixed = educationDTO.IsFixed,
                MentorId = educationDTO.MentorId,
                CreationDate = DateTime.Now,
                CreatorUserId = educationDTO.MentorId
            };

            await _announcementRepository.AddAsync(newAnnnouncement).ConfigureAwait(false);

        }

        /// <summary>
        /// Update announcement.
        /// </summary>
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        public async Task UpdateEntityAsync(AnnouncementDTO educationDTO)
        {

            var updatedAnnouncement = await _announcementRepository.GetByIdAsync(educationDTO.Id).ConfigureAwait(false);

            updatedAnnouncement.Title = educationDTO.Title;

            updatedAnnouncement.Description = educationDTO.Description;

            updatedAnnouncement.IsFixed = educationDTO.IsFixed;

            updatedAnnouncement.LastModificationDate = DateTime.Now;

            await _announcementRepository.UpdateAsync(updatedAnnouncement).ConfigureAwait(false);

        }

        /// <summary>
        /// Delete announcement.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteEntityAsync(Guid id)
        {

            var deletedAnnouncement = await _announcementRepository.GetByIdAsync(id).ConfigureAwait(false);

            await _announcementRepository.DeleteAsync(deletedAnnouncement).ConfigureAwait(false);

        }

        /// <summary>
        /// Delete multiple announcement by <paramref name="ids"/>.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task DeleteEntities(List<Guid> ids)
        {
            var deletedAnnouncement = await _announcementRepository.GetAllAsync(i => ids.Select(p => p).Contains(i.Id)).ConfigureAwait(false);
            await _announcementRepository.DeleteAsync(deletedAnnouncement).ConfigureAwait(false);
        }


    }
}
