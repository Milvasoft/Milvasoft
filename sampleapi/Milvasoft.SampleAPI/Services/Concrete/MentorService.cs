using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Milvasoft.Helpers.DataAccess.EfCore.Abstract;
using Milvasoft.Helpers.DataAccess.EfCore.IncludeLibrary;
using Milvasoft.Helpers.Exceptions;
using Milvasoft.Helpers.FileOperations.Enums;
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
        private readonly string _loggedUser;
        private readonly UserManager<AppUser> _userManager;
        private readonly IBaseRepository<Mentor, Guid, EducationAppDbContext> _mentorRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Performs constructor injection for repository interfaces used in this service.
        /// </summary>
        /// <param name="mentorRepository"></param>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        public MentorService(IBaseRepository<Mentor, Guid, EducationAppDbContext> mentorRepository, UserManager<AppUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _mentorRepository = mentorRepository;
            _loggedUser = httpContextAccessor.HttpContext.User.Identity.Name;
        }

        /// <summary>
        /// Get mentors for admin.
        /// </summary>
        /// <returns></returns>
        public async Task<PaginationDTO<MentorForAdminDTO>> GetMentorsForAdminAsync(PaginationParamsWithSpec<MentorSpec> paginationParams)
        {
            Func<IIncludable<Mentor>, IIncludable> includes = i => i.Include(p => p.PublishedAnnouncements)
                                                                    .Include(s => s.Students)
                                                                    .Include(p => p.Professions);

            var (mentors, pageCount, totalDataCount) = await _mentorRepository.PreparePaginationDTO(paginationParams.PageIndex,
                                                                                                    paginationParams.RequestedItemCount,
                                                                                                    paginationParams.OrderByProperty,
                                                                                                    paginationParams.OrderByAscending,
                                                                                                    paginationParams.Spec?.ToExpression(),
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
        public async Task<MentorForMentorDTO> GetCurrentUserProfileAsync()
        {
            Func<IIncludable<Mentor>, IIncludable> includes = i => i.Include(p => p.Students)
                                                                    .Include(p => p.Professions)
                                                                    .Include(p => p.PublishedAnnouncements);


            var mentor = await _mentorRepository.GetFirstOrDefaultAsync(includes, p => p.AppUser.UserName == _loggedUser).ConfigureAwait(false);

            mentor.ThrowIfNullForGuidObject();

            return new MentorForMentorDTO
            {
                Id = mentor.Id,
                Name = mentor.Name,
                Surname = mentor.Surname,
                CVFilePath = mentor.CVFilePath,
                CreationDate = mentor.CreationDate,
                LastModificationDate = mentor.LastModificationDate,
                Professions = mentor.Professions.CheckList(i => mentor.Professions?.Select(pr => new MentorProfessionDTO
                {
                    ProfessionId = pr.ProfessionId
                })),
                PublishedAnnouncements = mentor.PublishedAnnouncements.CheckList(i => mentor.PublishedAnnouncements?.Select(pa => new AnnouncementDTO
                {
                    Title = pa.Title,
                    Description = pa.Description
                })),
                Students = mentor.Students.CheckList(i => mentor.Students?.Select(st => new StudentForMentorDTO
                {
                    Name = st.Name,
                    Surname = st.Surname,
                    Age = st.Age,
                    GraduationScore = st.GraduationScore,
                    GraduationStatus = st.GraduationStatus,
                    ProfessionId = st.ProfessionId,
                    CreationDate = st.CreationDate,
                    CurrentAssigmentDeliveryDate = st.CurrentAssigmentDeliveryDate,
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
        /// <param name="Id">Id of the to be updated mentor.</param>
        /// <returns></returns>
        public async Task UpdateMentorByAdminAsync(UpdateMentorDTO updateMentorDTO, Guid Id)
        {
            var toBeUpdatedMentor = await _mentorRepository.GetByIdAsync(Id).ConfigureAwait(false);

            toBeUpdatedMentor.ThrowIfNullForGuidObject();

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

            toBeUpdatedMentor.ThrowIfNullForGuidObject();

            toBeUpdatedMentor.Mentor.Name = updateMentorDTO.Name;
            toBeUpdatedMentor.Mentor.Surname = updateMentorDTO.Surname;

            await _mentorRepository.UpdateAsync(toBeUpdatedMentor.Mentor).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes the mentor whose id has been sent.
        /// </summary>
        /// <param name="mentorIds">Id of the mentors to be deleted.</param>
        /// <returns></returns>
        public async Task DeleteMentorsAsync(List<Guid> mentorIds)
        {
            var mentors = await _mentorRepository.GetAllAsync(i => mentorIds.Select(p => p).Contains(i.Id)).ConfigureAwait(false);

            mentors.ThrowIfListIsNullOrEmpty();

            await _mentorRepository.DeleteAsync(mentors).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates picture of student.
        /// </summary>
        /// <param name="imageUploadDTO"></param>
        /// <returns></returns>
        public async Task UpdateImageAsync(ImageUploadDTO imageUploadDTO)
        {
            var mentor = await _mentorRepository.GetByIdAsync(imageUploadDTO.UserId);

            mentor.ThrowIfNullForGuidObject();

            var formFile = imageUploadDTO.Image;

            formFile.ValidateFile(FileType.Image);

            var dto = new StudentDTO { Id = mentor.Id, Image = formFile };

            mentor.ImagePath = await formFile.SaveImageToServerAsync<StudentDTO, Guid>(dto);

            await _mentorRepository.UpdateAsync(mentor);
        }
    }
}
