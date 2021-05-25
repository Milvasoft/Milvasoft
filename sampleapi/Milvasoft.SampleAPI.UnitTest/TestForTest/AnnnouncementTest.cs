//using Milvasoft.Helpers;
//using Milvasoft.Helpers.Exceptions;
//using Milvasoft.SampleAPI.DTOs;
//using Milvasoft.SampleAPI.DTOs.AnnouncementDTOs;
//using Milvasoft.SampleAPI.Services.Abstract;
//using Milvasoft.SampleAPI.Spec;
//using Milvasoft.SampleAPI.UnitTest.TestHelpers;
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Xunit;

//namespace Milvasoft.SampleAPI.UnitTest.TestForTest
//{
//    [Collection("Test")]
//    public class AnnnouncementTest
//    {
//        private readonly IAnnouncementService _announcementService;
//        public AnnnouncementTest()
//        {
//            var serviceCollection = new ServiceCollectionHelper();

//            //Servislerde eğer httpcontext üzerinden userName bilgisi isteniyor ise bu metot çağrılıp istreğe bağlı giriş yaptırmak istediğiniz kullanıcının kullaıcı adını girebilirsin.
//            serviceCollection.MockLoggedUserWithHttpContextAccessor();

//            //Test edilmek istenen servis bu şekilde mocklanabilir.
//            _announcementService = serviceCollection.GetService<IAnnouncementService>();

//        }

//        #region GetAnnouncementsForAdmin
//        [Fact]
//        public async Task GetAnnouncementsForAdmin_2()
//        {
//            var paginationParams = new PaginationParamsWithSpec<AnnouncementSpec>
//            {
//                PageIndex = 1,
//                RequestedItemCount = 3
//            };
//            var test = await _announcementService.GetAnnouncementsForAdminAsync(paginationParams).ConfigureAwait(false);
//            Assert.Equal(3, test.TotalDataCount);
//        }

//        [Fact]
//        public async Task GetAnnouncementsForAdmin_SpecIsFixed_2()
//        {
//            var paginationParams = new PaginationParamsWithSpec<AnnouncementSpec>
//            {
//                PageIndex = 1,
//                RequestedItemCount = 2,
//                Spec = new AnnouncementSpec
//                {
//                    IsFixed = false
//                }
//            };
//            var test = await _announcementService.GetAnnouncementsForAdminAsync(paginationParams).ConfigureAwait(false);
//            Assert.Equal(2, test.TotalDataCount);
//        }

//        [Fact]
//        public async Task GetAnnouncementsForAdmin_SpecMentorId_1()
//        {
//            var paginationParams = new PaginationParamsWithSpec<AnnouncementSpec>
//            {
//                PageIndex = 1,
//                RequestedItemCount = 2,
//                Spec = new AnnouncementSpec
//                {
//                    MentorId = 1.ToGuid()
//                }
//            };
//            var test = await _announcementService.GetAnnouncementsForAdminAsync(paginationParams).ConfigureAwait(false);
//            Assert.Equal(2, test.TotalDataCount);
//        }

//        [Fact]
//        public async Task GetAnnouncementsForAdmin_SpecTitle_2()
//        {
//            var paginationParams = new PaginationParamsWithSpec<AnnouncementSpec>
//            {
//                PageIndex = 1,
//                RequestedItemCount = 2,
//                Spec = new AnnouncementSpec
//                {
//                    Title = "Stajyerler hakkinda."
//                }
//            };
//            var test = await _announcementService.GetAnnouncementsForAdminAsync(paginationParams).ConfigureAwait(false);
//            Assert.Equal(2, test.TotalDataCount);
//        }

//        [Fact]
//        public async Task GetAnnouncementsForAdmin_SpecDescription_1()
//        {
//            var paginationParams = new PaginationParamsWithSpec<AnnouncementSpec>
//            {
//                PageIndex = 1,
//                RequestedItemCount = 2,
//                Spec = new AnnouncementSpec
//                {
//                    Description = "Stajyerler kurallara uymak zorundadir."
//                }
//            };
//            var test = await _announcementService.GetAnnouncementsForAdminAsync(paginationParams).ConfigureAwait(false);
//            Assert.Equal(1, test.TotalDataCount);
//        }

//        #endregion

//        #region GetAnnouncementsForMentor

//        [Fact]
//        public async Task GetAnnouncementsForMentor_2()
//        {
//            var paginationParams = new PaginationParamsWithSpec<AnnouncementSpec>
//            {
//                PageIndex = 1,
//                RequestedItemCount = 3
//            };
//            var test = await _announcementService.GetAnnouncementsForMentorAsync(paginationParams).ConfigureAwait(false);
//            Assert.Equal(3, test.TotalDataCount);
//        }

//        [Fact]
//        public async Task GetAnnouncementsForMentor_SpecIsFixed_2()
//        {
//            var paginationParams = new PaginationParamsWithSpec<AnnouncementSpec>
//            {
//                PageIndex = 1,
//                RequestedItemCount = 2,
//                Spec = new AnnouncementSpec
//                {
//                    IsFixed = false
//                }
//            };
//            var test = await _announcementService.GetAnnouncementsForMentorAsync(paginationParams).ConfigureAwait(false);
//            Assert.Equal(2, test.TotalDataCount);
//        }

//        [Fact]
//        public async Task GetAnnouncementsForMentor_SpecMentorId_1()
//        {
//            var paginationParams = new PaginationParamsWithSpec<AnnouncementSpec>
//            {
//                PageIndex = 1,
//                RequestedItemCount = 2,
//                Spec = new AnnouncementSpec
//                {
//                    MentorId = 2.ToGuid()
//                }
//            };
//            var test = await _announcementService.GetAnnouncementsForMentorAsync(paginationParams).ConfigureAwait(false);
//            Assert.Equal(1, test.TotalDataCount);
//        }

//        [Fact]
//        public async Task GetAnnouncementsForMentor_SpecTitle_2()
//        {
//            var paginationParams = new PaginationParamsWithSpec<AnnouncementSpec>
//            {
//                PageIndex = 1,
//                RequestedItemCount = 2,
//                Spec = new AnnouncementSpec
//                {
//                    Title = "Stajyerler hakkinda."
//                }
//            };
//            var test = await _announcementService.GetAnnouncementsForMentorAsync(paginationParams).ConfigureAwait(false);
//            Assert.Equal(2, test.TotalDataCount);
//        }

//        #endregion

//        #region GetAnnouncementsForStudent

//        [Fact]
//        public async Task GetAnnouncementsForStudent_2()
//        {
//            var paginationParams = new PaginationParamsWithSpec<AnnouncementSpec>
//            {
//                PageIndex = 1,
//                RequestedItemCount = 3
//            };
//            var test = await _announcementService.GetAnnouncementsForStudentAsync(paginationParams).ConfigureAwait(false);
//            Assert.Equal(3, test.TotalDataCount);
//        }

//        [Fact]
//        public async Task GetAnnouncementsForStudent_SpecIsFixed_2()
//        {
//            var paginationParams = new PaginationParamsWithSpec<AnnouncementSpec>
//            {
//                PageIndex = 1,
//                RequestedItemCount = 2,
//                Spec = new AnnouncementSpec
//                {
//                    IsFixed = false
//                }
//            };
//            var test = await _announcementService.GetAnnouncementsForStudentAsync(paginationParams).ConfigureAwait(false);
//            Assert.Equal(2, test.TotalDataCount);
//        }

//        [Fact]
//        public async Task GetAnnouncementsForStudent_SpecMentorId_1()
//        {
//            var paginationParams = new PaginationParamsWithSpec<AnnouncementSpec>
//            {
//                PageIndex = 1,
//                RequestedItemCount = 2,
//                Spec = new AnnouncementSpec
//                {
//                    MentorId = 2.ToGuid()
//                }
//            };
//            var test = await _announcementService.GetAnnouncementsForStudentAsync(paginationParams).ConfigureAwait(false);
//            Assert.Equal(1, test.TotalDataCount);
//        }

//        [Fact]
//        public async Task GetAnnouncementsForStudent_SpecTitle_2()
//        {
//            var paginationParams = new PaginationParamsWithSpec<AnnouncementSpec>
//            {
//                PageIndex = 1,
//                RequestedItemCount = 2,
//                Spec = new AnnouncementSpec
//                {
//                    Title = "Stajyerler hakkinda."
//                }
//            };
//            var test = await _announcementService.GetAnnouncementsForStudentAsync(paginationParams).ConfigureAwait(false);
//            Assert.Equal(2, test.TotalDataCount);
//        }

//        #endregion

//        #region GetAnnouncementForAdmin

//        [Fact]
//        public async Task GetAnnouncementForAdmin_NegativId_ThrowException()
//        {
//            Func<Task> act = () => _announcementService.GetAnnouncementForAdminAsync((-2).ToGuid());
//            var exception = await Assert.ThrowsAsync<MilvaUserFriendlyException>(act);

//            Assert.Equal("Böyle bir duyuru bulunmamaktadır.", exception.Message);
//        }

//        [Fact]
//        public async Task GetAnnouncementForAdmin_PositiveNumber_1()
//        {
//            var test = await _announcementService.GetAnnouncementForAdminAsync(1.ToGuid()).ConfigureAwait(false);
//            Assert.Equal("Oğuzhan", test.PublisherMentor.Name);
//        }

//        #endregion

//        #region GetAnnouncementForMentor

//        [Fact]
//        public async Task GetAnnouncementForMentor_NegativId_ThrowException()
//        {
//            Func<Task> act = () => _announcementService.GetAnnouncementForMentorAsync((-2).ToGuid());
//            var exception = await Assert.ThrowsAsync<MilvaUserFriendlyException>(act);

//            Assert.Equal("Böyle bir duyuru bulunmamaktadır.", exception.Message);
//        }

//        [Fact]
//        public async Task GetAnnouncementForMentor_PositiveNumber_1()
//        {
//            var test = await _announcementService.GetAnnouncementForMentorAsync(1.ToGuid()).ConfigureAwait(false);
//            Assert.Equal("Oğuzhan", test.PublisherMentor.Name);
//        }

//        #endregion

//        #region GetAnnouncementForStudent

//        [Fact]
//        public async Task GetAnnouncementForStudent_NegativId_ThrowException()
//        {
//            Func<Task> act = () => _announcementService.GetAnnouncementForStudentAsync((-2).ToGuid());
//            var exception = await Assert.ThrowsAsync<MilvaUserFriendlyException>(act);

//            Assert.Equal("Böyle bir duyuru bulunmamaktadır.", exception.Message);
//        }

//        [Fact]
//        public async Task GetAnnouncementForStudent_PositiveNumber_1()
//        {
//            var test = await _announcementService.GetAnnouncementForStudentAsync(1.ToGuid()).ConfigureAwait(false);
//            Assert.Equal("Oğuzhan", test.PublisherMentor.Name);
//        }
//        #endregion

//        [Fact]
//        public async Task AddAnnouncement()
//        {
//            var newAnnnouncement = new AddAnnouncementDTO
//            {
//                Title = "AddDeneme",
//                Description = "Deneme",
//                IsFixed = true,
//                MentorId = 1.ToGuid()
//            };
//            await _announcementService.AddAnnouncementAsync(newAnnnouncement).ConfigureAwait(false);
//        }
//        [Fact]
//        public async Task UpdateAnnouncement()
//        {
//            var newAnnnouncement = new UpdateAnnouncementDTO
//            {
//                Id = 1.ToGuid(),
//                Title = "Stajyerler hakkında",
//                Description = "Stajyerler verilen ödevleri yapmamalıdır.",
//                IsFixed = true
//            };
//            await _announcementService.UpdateAnnouncementAsync(newAnnnouncement).ConfigureAwait(false);
//        }
//        [Fact]
//        public async Task DeleteAnnouncement()
//        {
//            List<Guid> toBeDeletedList = new List<Guid>() { 1.ToGuid(), 2.ToGuid() };
//            await _announcementService.DeleteAnnouncementsAsync(toBeDeletedList).ConfigureAwait(false);
//        }
//    }
//}
