using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.DataAccess.IncludeLibrary;
using Milvasoft.Helpers.Models;
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
        public async Task<PaginationDTO<AnnouncementDTO>> GetEntitiesForStudentAsync(int pageIndex,
                                                                                     int requestedItemCount,
                                                                                     string orderByProperty = null,
                                                                                     bool orderByAscending = false,
                                                                                     AnnouncementSpec announcementSpec = null)
        {
            Func<IIncludable<Announcement>, IIncludable> includes = i => i.Include(md => md.PublisherMentor);

            var (announcements, pageCount, totalDataCount) = await _announcementRepository.PreparePaginationDTO<IBaseRepository<Announcement, Guid, EducationAppDbContext>, Announcement, Guid>
                                                                                                                (pageIndex, requestedItemCount, orderByProperty, orderByAscending, announcementSpec?.ToExpression(), includes).ConfigureAwait(false);

            return new PaginationDTO<AnnouncementDTO>
            {
                DTOList = announcements.CheckList(i => announcements.Select(announcement => new AnnouncementDTO
                {
                    Title = announcement.Title,
                    Description = announcement.Description,
                    PublisherMentor = announcement.PublisherMentor.CheckObject(i => new MentorDTO
                    {
                        Id = i.Id
                    })
                })),
                PageCount = pageCount,
                TotalDataCount = totalDataCount
            };
        }

        /// <summary>
        /// Get all announcement for admin.
        /// </summary>
        /// <returns></returns>
        public async Task<PaginationDTO<AnnouncementDTO>> GetEntitiesForAdminAsync(int pageIndex,
                                                                                   int requestedItemCount,
                                                                                   string orderByProperty = null,
                                                                                   bool orderByAscending = false,
                                                                                   AnnouncementSpec announcementSpec = null)
        {
            Func<IIncludable<Announcement>, IIncludable> includes = i => i.Include(md => md.PublisherMentor);

            var (announcements, pageCount, totalDataCount) = await _announcementRepository.PreparePaginationDTO<IBaseRepository<Announcement, Guid, EducationAppDbContext>, Announcement, Guid>
                                                                                                                (pageIndex, requestedItemCount, orderByProperty, orderByAscending, announcementSpec?.ToExpression(), includes).ConfigureAwait(false);

            return new PaginationDTO<AnnouncementDTO>
            {
                DTOList = announcements.CheckList(i => announcements.Select(announcement => new AnnouncementDTO
                {
                    Id = announcement.Id,
                    Title = announcement.Title,
                    Description = announcement.Description,
                    IsFixed = announcement.IsFixed,
                    PublisherMentor = announcement.PublisherMentor.CheckObject(i => new MentorDTO
                    {
                        Id = i.Id
                    }),
                    CreatorUserId = announcement.MentorId
                })),
                PageCount = pageCount,
                TotalDataCount = totalDataCount
            };
        }

        /// <summary>
        /// Get all announcement for mentor.
        /// </summary>
        /// <returns></returns>
        public async Task<PaginationDTO<AnnouncementDTO>> GetEntitiesForMentorAsync(int pageIndex,
                                                                                    int requestedItemCount,
                                                                                    string orderByProperty = null,
                                                                                    bool orderByAscending = false,
                                                                                    AnnouncementSpec announcementSpec = null)
        {
            Func<IIncludable<Announcement>, IIncludable> includes = i => i.Include(md => md.PublisherMentor);

            var (announcements, pageCount, totalDataCount) = await _announcementRepository.PreparePaginationDTO<IBaseRepository<Announcement, Guid, EducationAppDbContext>, Announcement, Guid>
                                                                                                                (pageIndex, requestedItemCount, orderByProperty, orderByAscending, announcementSpec?.ToExpression(), includes).ConfigureAwait(false);

            return new PaginationDTO<AnnouncementDTO>
            {
                DTOList = announcements.CheckList(i => announcements.Select(announcement => new AnnouncementDTO
                {
                    Title = announcement.Title,
                    Description = announcement.Description,
                    IsFixed = announcement.IsFixed,
                    PublisherMentor = announcement.PublisherMentor.CheckObject(i => new MentorDTO
                    {
                        Id = i.Id
                    })
                })),
                PageCount = pageCount,
                TotalDataCount = totalDataCount
            };
        }

        /// <summary>
        /// Get announcement for admin by <paramref name="announcementId"/>.
        /// </summary>
        /// <param name="announcementId"></param>
        /// <returns></returns>
        public async Task<AnnouncementDTO> GetEntityForAdminAsync(Guid announcementId)
        {
            Func<IIncludable<Announcement>, IIncludable> includes = i => i.Include(md => md.PublisherMentor);

            var announcement = await _announcementRepository.GetByIdAsync(announcementId, includes).ConfigureAwait(false);

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
        /// Get entity for mentor by <paramref name="announcementId"/>.
        /// </summary>
        /// <param name="announcementId"></param>
        /// <returns></returns>
        public async Task<AnnouncementDTO> GetEntityForMentorAsync(Guid announcementId)
        {
            Func<IIncludable<Announcement>, IIncludable> includes = i => i.Include(md => md.PublisherMentor);

            var announcement = await _announcementRepository.GetByIdAsync(announcementId, includes).ConfigureAwait(false);

            return new AnnouncementDTO
            {
                Title = announcement.Title,
                Description = announcement.Description,
                IsFixed = announcement.IsFixed,
                PublisherMentor = announcement.PublisherMentor.CheckObject(i => new MentorDTO
                {
                    Id = i.Id
                })
            };
        }

        /// <summary>
        /// Get announcement for student by <paramref name="announcementId"/>.
        /// </summary>
        /// <param name="announcementId"></param>
        /// <returns></returns>
        public async Task<AnnouncementDTO> GetEntityForStudentAsync(Guid announcementId)
        {
            Func<IIncludable<Announcement>, IIncludable> includes = i => i.Include(md => md.PublisherMentor);

            var announcement = await _announcementRepository.GetByIdAsync(announcementId, includes).ConfigureAwait(false);

            return new AnnouncementDTO
            {
                Title = announcement.Title,
                Description = announcement.Description,
                PublisherMentor = announcement.PublisherMentor.CheckObject(i => new MentorDTO
                {
                    Id = i.Id
                }),
                CreationDate = announcement.CreationDate,
                LastModificationDate = announcement.LastModificationDate
            };
        }

        /// <summary>
        /// Add new announcement.
        /// </summary>
        /// <param name="addAnnouncementDTO"></param>
        /// <returns></returns>
        public async Task AddEntityAsync(AddAnnouncementDTO addAnnouncementDTO)
        {
            var newAnnnouncement = new Announcement
            {
                Title = addAnnouncementDTO.Title,
                Description = addAnnouncementDTO.Description,
                IsFixed = addAnnouncementDTO.IsFixed,
                MentorId = addAnnouncementDTO.MentorId
            };

            await _announcementRepository.AddAsync(newAnnnouncement).ConfigureAwait(false);
        }

        /// <summary>
        /// Update announcement.
        /// </summary>
        /// <param name="updateAnnouncementDTO"></param>
        /// <returns></returns>
        public async Task UpdateEntityAsync(UpdateAnnouncementDTO updateAnnouncementDTO)
        {
            var updatedAnnouncement = await _announcementRepository.GetByIdAsync(updateAnnouncementDTO.Id).ConfigureAwait(false);

            updatedAnnouncement.Title = updateAnnouncementDTO.Title;

            updatedAnnouncement.Description = updateAnnouncementDTO.Description;

            updatedAnnouncement.IsFixed = updateAnnouncementDTO.IsFixed;

            await _announcementRepository.UpdateAsync(updatedAnnouncement).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete announcement.
        /// </summary>
        /// <param name="announcementId"></param>
        /// <returns></returns>
        public async Task DeleteEntityAsync(Guid announcementId)
        {
            var deletedAnnouncement = await _announcementRepository.GetByIdAsync(announcementId).ConfigureAwait(false);

            await _announcementRepository.DeleteAsync(deletedAnnouncement).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete multiple announcement by <paramref name="announcementIds"/>.
        /// </summary>
        /// <param name="announcementIds"></param>
        /// <returns></returns>
        public async Task DeleteEntitiesAsync(List<Guid> announcementIds)
        {
            var deletedAnnouncement = await _announcementRepository.GetAllAsync(i => announcementIds.Select(p => p).Contains(i.Id)).ConfigureAwait(false);

            await _announcementRepository.DeleteAsync(deletedAnnouncement).ConfigureAwait(false);
        }


    }
}
