﻿using Microsoft.AspNetCore.Identity;
using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.DataAccess.IncludeLibrary;
using Milvasoft.Helpers.Exceptions;
using Milvasoft.Helpers.Models;
using Milvasoft.SampleAPI.Data;
using Milvasoft.SampleAPI.DTOs;
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
        private readonly UserManager<AppUser> _userManager;
        private readonly IBaseRepository<Mentor, Guid, EducationAppDbContext> _mentorRepository;

        /// <summary>
        /// Performs constructor injection for repository interfaces used in this service.
        /// </summary>
        /// <param name="mentorRepository"></param>
        /// <param name="userManager"></param>
        public MentorService(IBaseRepository<Mentor, Guid, EducationAppDbContext> mentorRepository, UserManager<AppUser> userManager)
        {
            _userManager = userManager;
            _mentorRepository = mentorRepository;
        }

        /// <summary>
        /// Get mentors for admin.
        /// </summary>
        /// <returns></returns>
        public async Task<PaginationDTO<MentorForAdminDTO>> GetEntitiesForAdminAsync(PaginationParamsWithSpec<MentorSpec> mentorPaginationParams)
        {
            Func<IIncludable<Mentor>, IIncludable> includes = i => i.Include(p => p.PublishedAnnouncements)
                                                                     .Include(s => s.Students)
                                                                     .Include(p => p.Professions);

            var (mentors, pageCount, totalDataCount) = await _mentorRepository.PreparePaginationDTO<IBaseRepository<Mentor, Guid, EducationAppDbContext>, Mentor, Guid>
                                                                                                               (mentorPaginationParams.PageIndex,
                                                                                                                mentorPaginationParams.RequestedItemCount,
                                                                                                                mentorPaginationParams.OrderByProperty = null,
                                                                                                                mentorPaginationParams.OrderByAscending = false,
                                                                                                                mentorPaginationParams.Spec?.ToExpression(),
                                                                                                                includes).ConfigureAwait(false);

            return new PaginationDTO<MentorForAdminDTO>
            {
                DTOList = mentors.CheckList(i => mentors.Select(mentor => new MentorForAdminDTO
                {
                    Id=mentor.Id,
                    Name = mentor.Name,
                    Surname = mentor.Surname,
                    CVFilePath = mentor.CVFilePath,
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
                })),
                PageCount = pageCount,
                TotalDataCount = totalDataCount
            };
        }

        /// <summary>
        /// Get mentors for mentor.
        /// </summary>
        /// <returns></returns>
        public async Task<PaginationDTO<MentorForMentorDTO>> GetEntitiesForMentorAsync(PaginationParamsWithSpec<MentorSpec> mentorPaginationParams)
        {
            Func<IIncludable<Mentor>, IIncludable> includes = i => i.Include(p => p.PublishedAnnouncements)
                                                                     .Include(s => s.Students)
                                                                     .Include(p => p.Professions);

            var (mentors, pageCount, totalDataCount) = await _mentorRepository.PreparePaginationDTO<IBaseRepository<Mentor, Guid, EducationAppDbContext>, Mentor, Guid>
                                                                                                                (mentorPaginationParams.PageIndex,
                                                                                                                mentorPaginationParams.RequestedItemCount,
                                                                                                                mentorPaginationParams.OrderByProperty = null,
                                                                                                                mentorPaginationParams.OrderByAscending = false,
                                                                                                                mentorPaginationParams.Spec?.ToExpression(),
                                                                                                                includes).ConfigureAwait(false);

            return new PaginationDTO<MentorForMentorDTO>
            {
                DTOList = mentors.CheckList(i => mentors.Select(mentor => new MentorForMentorDTO
                {
                    Id = mentor.Id,
                    Name = mentor.Name,
                    Surname = mentor.Surname,
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
                })),
                PageCount = pageCount,
                TotalDataCount = totalDataCount
            };
        }

        /// <summary>
        /// Get mentors for student.
        /// </summary>
        /// <returns></returns>
        public async Task<PaginationDTO<MentorForStudentDTO>> GetEntitiesForStudentAsync(PaginationParamsWithSpec<MentorSpec> mentorPaginationParams)
        {
            Func<IIncludable<Mentor>, IIncludable> includes = i => i.Include(p => p.PublishedAnnouncements)
                                                                     .Include(p => p.Professions);

            var (mentors, pageCount, totalDataCount) = await _mentorRepository.PreparePaginationDTO<IBaseRepository<Mentor, Guid, EducationAppDbContext>, Mentor, Guid>
                                                                                                                (mentorPaginationParams.PageIndex,
                                                                                                                mentorPaginationParams.RequestedItemCount,
                                                                                                                mentorPaginationParams.OrderByProperty = null,
                                                                                                                mentorPaginationParams.OrderByAscending = false,
                                                                                                                mentorPaginationParams.Spec?.ToExpression(),
                                                                                                                includes).ConfigureAwait(false);

            return new PaginationDTO<MentorForStudentDTO>
            {
                DTOList = mentors.CheckList(i => mentors.Select(mentor => new MentorForStudentDTO
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
                })),
                PageCount = pageCount,
                TotalDataCount = totalDataCount
            };
        }

        /// <summary>
        /// Get mentor for admin by <paramref name="mentorId"/>.
        /// </summary>
        /// <param name="mentorId"></param>
        /// <returns></returns>
        public async Task<MentorForAdminDTO> GetEntityForAdminAsync(Guid mentorId)
        {
            Func<IIncludable<Mentor>, IIncludable> includes = i => i.Include(p => p.PublishedAnnouncements)
                                                                     .Include(s => s.Students)
                                                                     .Include(p => p.Professions);

            var mentor = await _mentorRepository.GetByIdAsync(mentorId, includes).ConfigureAwait(false);
            return new MentorForAdminDTO
            {
                Id = mentor.Id,
                Name = mentor.Name,
                Surname = mentor.Surname,
                CVFilePath = mentor.CVFilePath,
                CreationDate = mentor.CreationDate,
                LastModificationDate = mentor.LastModificationDate,
                Professions = mentor.Professions.CheckList(i => mentor.Professions?.Select(pr => new MentorProfessionDTO
                {
                    ProfessionId= pr.ProfessionId,
                    MentorId=pr.MentorId,
                    Id=pr.Id
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
        /// Get mentor for mentor by <paramref name="mentorId"/>
        /// </summary>
        /// <param name="mentorId"></param>
        /// <returns></returns>
        public async Task<MentorForMentorDTO> GetEntityForMentorAsync(Guid mentorId)
        {

            Func<IIncludable<Mentor>, IIncludable> includes = i => i.Include(p => p.PublishedAnnouncements)
                                                                     .Include(s => s.Students)
                                                                     .Include(p => p.Professions);

            var mentor = await _mentorRepository.GetByIdAsync(mentorId, includes).ConfigureAwait(false);
            return new MentorForMentorDTO
            {
                Name = mentor.Name,
                Surname = mentor.Surname,
                Professions = mentor.Professions.CheckList(i => mentor.Professions?.Select(pr => new MentorProfessionDTO
                {
                    ProfessionId = pr.ProfessionId
                    
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
        /// Get mentor for student by <paramref name="mentorId"/>
        /// </summary>
        /// <param name="mentorId"></param>
        /// <returns></returns>
        public async Task<MentorForStudentDTO> GetEntityForStudentAsync(Guid mentorId)
        {
            Func<IIncludable<Mentor>, IIncludable> includes = i => i.Include(p => p.PublishedAnnouncements)
                                                                    .Include(p => p.Professions);

            var mentor = await _mentorRepository.GetByIdAsync(mentorId, includes).ConfigureAwait(false);
            return new MentorForStudentDTO
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
        /// <param name="addMentorDTO"></param>
        /// <returns></returns>
        public async Task AddEntityAsync(AddMentorDTO addMentorDTO)
        {
            var mentor = new Mentor
            {
                Name = addMentorDTO.Name,
                Surname = addMentorDTO.Surname,
                CVFilePath = addMentorDTO.CVFilePath,
                Professions = addMentorDTO.Professions != null ?
                                     (from pp in addMentorDTO.Professions
                                      select new MentorProfession
                                      {
                                          ProfessionId=pp
                                      }).ToList() : null,
            };
            var appUser = new AppUser
            {
                UserName = addMentorDTO.UserName,
                Email = addMentorDTO.Email,
                PhoneNumber = addMentorDTO.PhoneNumber,
                Mentor = mentor
            };

            await AddAsync(appUser, addMentorDTO.Password).ConfigureAwait(false);
            


        }

        /// <summary>
        /// Add appuser with mentor.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private async Task AddAsync(AppUser user, string password)
        {
            if (user.Mentor == null)
                throw new MilvaUserFriendlyException("PleaseEnterPersonnelInformation");

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
                throw new MilvaUserFriendlyException(string.Join("~", result.Errors.Select(m => m.Description)));
        }

        /// <summary>
        /// Update mentor.
        /// </summary>
        /// <param name="updateMentorDTO"></param>
        /// <returns></returns>
        public async Task UpdateEntityAsync(UpdateMentorDTO updateMentorDTO)
        {
            var updatedMentor = await _mentorRepository.GetByIdAsync(updateMentorDTO.Id).ConfigureAwait(false);

            updatedMentor.Name = updateMentorDTO.Name;

            updatedMentor.Surname = updateMentorDTO.Surname;


            await _mentorRepository.UpdateAsync(updatedMentor).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete mentor.
        /// </summary>
        /// <param name="mentorId"></param>
        /// <returns></returns>
        public async Task DeleteEntityAsync(Guid mentorId)
        {
            var deletedMentor = await _mentorRepository.GetByIdAsync(mentorId, i => i.Include(a => a.Professions)).ConfigureAwait(false);

            await _mentorRepository.DeleteAsync(deletedMentor).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete mentors.
        /// </summary>
        /// <param name="mentorIds"></param>
        /// <returns></returns>
        public async Task DeleteEntitiesAsync(List<Guid> mentorIds)
        {
            var mentors = await _mentorRepository.GetAllAsync(i => mentorIds.Select(p => p).Contains(i.Id)).ConfigureAwait(false);

            await _mentorRepository.DeleteAsync(mentors).ConfigureAwait(false);
        }
    }
}
