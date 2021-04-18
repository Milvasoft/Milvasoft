using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.DataAccess.IncludeLibrary;
using Milvasoft.Helpers.Exceptions;
using Milvasoft.Helpers.Identity.Concrete;
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
        private readonly string _loggedUser;

        /// <summary>
        /// Performs constructor injection for repository interfaces used in this service.
        /// </summary>
        /// <param name="mentorRepository"></param>
        /// <param name="userManager"></param>
        public MentorService(IBaseRepository<Mentor, Guid, EducationAppDbContext> mentorRepository, UserManager<AppUser> userManager,IHttpContextAccessor httpContextAccessor)
        {
            _loggedUser = httpContextAccessor.HttpContext.User.Identity.Name;
            _userManager = userManager;
            _mentorRepository = mentorRepository;
        }

        /// <summary>
        /// Get mentors for admin.
        /// </summary>
        /// <returns></returns>
        public async Task<PaginationDTO<MentorForAdminDTO>> GetMentorsForAdminAsync(PaginationParamsWithSpec<MentorSpec> pagiantionParams)
        {
            Func<IIncludable<Mentor>, IIncludable> includes = i => i.Include(p => p.PublishedAnnouncements)
                                                                     .Include(s => s.Students)
                                                                     .Include(p => p.Professions);

            var (mentors, pageCount, totalDataCount) = await _mentorRepository.PreparePaginationDTO<IBaseRepository<Mentor, Guid, EducationAppDbContext>, Mentor, Guid>
                                                                                                               (pagiantionParams.PageIndex,
                                                                                                                pagiantionParams.RequestedItemCount,
                                                                                                                pagiantionParams.OrderByProperty = null,
                                                                                                                pagiantionParams.OrderByAscending = false,
                                                                                                                pagiantionParams.Spec?.ToExpression(),
                                                                                                                includes).ConfigureAwait(false);

            return new PaginationDTO<MentorForAdminDTO>
            {
                DTOList = mentors.CheckList(i => mentors.Select(mentor => new MentorForAdminDTO
                {
                    Id = mentor.Id,
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
        /// Get mentor for admin by <paramref name="mentorId"/>.
        /// </summary>
        /// <param name="mentorId"></param>
        /// <returns></returns>
        public async Task<MentorForAdminDTO> GetMentorForAdminAsync(Guid mentorId)
        {
            Func<IIncludable<Mentor>, IIncludable> includes = i => i.Include(p => p.PublishedAnnouncements)
                                                                     .Include(s => s.Students)
                                                                     .Include(p => p.Professions);

            var mentor = await _mentorRepository.GetByIdAsync(mentorId, includes).ConfigureAwait(false);

            mentor.ThrowIfNullForGuidObject();

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
                    ProfessionId = pr.ProfessionId,
                    MentorId = pr.MentorId,
                    Id = pr.Id
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
        /// Brings instant user's profile information.
        /// </summary>
        /// <returns></returns>
        public async Task<MentorForAdminDTO> GetCurrentUserProfile()
        {
            var currentMentor = await _userManager.FindByNameAsync(_loggedUser).ConfigureAwait(false);

            return new MentorForAdminDTO
            {
                Name = currentMentor.Mentor.Name,
                Surname = currentMentor.Mentor.Surname,
                CVFilePath = currentMentor.Mentor.CVFilePath,
                CreationDate = currentMentor.Mentor.CreationDate,
                LastModificationDate = currentMentor.Mentor.LastModificationDate,
                Professions = currentMentor.Mentor.Professions.CheckList(i => currentMentor.Mentor.Professions?.Select(pr => new MentorProfessionDTO
                {
                    ProfessionId = pr.ProfessionId,
                    MentorId = pr.MentorId,
                    Id = pr.Id
                })),
                PublishedAnnouncements = currentMentor.Mentor.PublishedAnnouncements.CheckList(i => currentMentor.Mentor.PublishedAnnouncements?.Select(pa => new AnnouncementDTO
                {
                    Id = pa.Id
                })),
                Students = currentMentor.Mentor.Students.CheckList(i => currentMentor.Mentor.Students?.Select(st => new StudentDTO
                {
                    Id = st.Id
                }))

            };
        }

        /// <summary>
        /// Maps <paramref name="addMentorDTO"/> to <c><b>Mentor</b></c>  object and adds that product to repository.
        /// </summary>
        /// <param name="addMentorDTO">Mentor to be added.</param>
        /// <returns></returns>
        public async Task AddMentorAsync(AddMentorDTO addMentorDTO)
        {
            var appUser = new AppUser
            {
                UserName = addMentorDTO.UserName,
                Email = addMentorDTO.Email,
                PhoneNumber = addMentorDTO.PhoneNumber,
                Mentor = new Mentor
                {
                    Name = addMentorDTO.Name,
                    Surname = addMentorDTO.Surname,
                    CVFilePath = addMentorDTO.CVFilePath,
                    Professions = addMentorDTO.Professions != null ?
                                     (from pp in addMentorDTO.Professions
                                      select new MentorProfession
                                      {
                                          ProfessionId = pp
                                      }).ToList() : null,
                }
            };

            var result = await _userManager.CreateAsync(appUser, addMentorDTO.Password);

            if (!result.Succeeded)
                throw new MilvaUserFriendlyException(result.DescriptionJoin());
        }

        /// <summary>
        /// Updates single mentor which that equals <paramref name="updateMentorDTO"/> in repository by <paramref name="updateMentorDTO"/>'s properties.
        /// </summary>
        /// <param name="updateMentorDTO">Mentor to be updated.</param>
        /// <returns></returns>
        public async Task UpdateMentorAsync(UpdateMentorDTO updateMentorDTO)
        {
            
            var toBeUpdatedMentor = await _mentorRepository.GetByIdAsync(updateMentorDTO.Id).ConfigureAwait(false);

            toBeUpdatedMentor.Name = updateMentorDTO.Name;

            toBeUpdatedMentor.Surname = updateMentorDTO.Surname;

            await _mentorRepository.UpdateAsync(toBeUpdatedMentor).ConfigureAwait(false);
        }

        /// <summary>
        /// The mentor can update himself.
        /// </summary>
        /// <param name="updateMentorDTO"></param>
        /// <returns></returns>
        public async Task UpdateCurrentMentorAsync(UpdateMentorDTO updateMentorDTO)
        {
            var toBeUpdatedMentor = await _userManager.FindByNameAsync(_loggedUser).ConfigureAwait(false) ?? throw new MilvaUserFriendlyException("CannotFindUserWithThisToken");

            toBeUpdatedMentor.Mentor.Name = updateMentorDTO.Name;
            toBeUpdatedMentor.Mentor.Surname = updateMentorDTO.Surname;

            await _mentorRepository.UpdateAsync(toBeUpdatedMentor.Mentor).ConfigureAwait(false);

        }

        /// <summary>
        /// Delete mentors.
        /// </summary>
        /// <param name="mentorIds"></param>
        /// <returns></returns>
        public async Task DeleteMentorsAsync(List<Guid> mentorIds)
        {
            //TODO OGZ SUMMARY DÜZELTİLECEK.
            //TODO OGZ mentorIdList.IsNullOrEmpty() throw

            var mentors = await _mentorRepository.GetAllAsync(i => mentorIds.Select(p => p).Contains(i.Id)).ConfigureAwait(false);

            //TODO OGZ mentors.IsNullOrEmpty() 
            await _mentorRepository.DeleteAsync(mentors).ConfigureAwait(false);
        }

        #region Private Methods
        //TODO OGZ PRİVATE METHODLAR BURAYA YAZILACAK.
        #endregion
    }
}
