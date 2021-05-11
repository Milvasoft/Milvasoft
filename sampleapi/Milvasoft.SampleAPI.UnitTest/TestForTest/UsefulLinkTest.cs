using Milvasoft.Helpers;
using Milvasoft.Helpers.Exceptions;
using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.DTOs.UsefulLinkDTOs;
using Milvasoft.SampleAPI.Services.Abstract;
using Milvasoft.SampleAPI.Spec;
using Milvasoft.SampleAPI.UnitTest.TestHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Milvasoft.SampleAPI.UnitTest.TestForTest
{
    [Collection("Test")]
    public class UsefulLinkTest
    {
        private readonly IUsefulLinkService _usefulLinkService;
        public UsefulLinkTest()
        {
            var serviceCollection = new ServiceCollectionHelper();

            //Servislerde eğer httpcontext üzerinden userName bilgisi isteniyor ise bu metot çağrılıp istreğe bağlı giriş yaptırmak istediğiniz kullanıcının kullaıcı adını girebilirsin.
            serviceCollection.MockLoggedUserWithHttpContextAccessor();

            //Test edilmek istenen servis bu şekilde mocklanabilir.
            _usefulLinkService = serviceCollection.GetService<IUsefulLinkService>();

        }

        #region GetAnnouncementsForAdmin
        [Fact]
        public async Task GetUsefulLinkForAdmin_4()
        {
            var paginationParams = new PaginationParamsWithSpec<UsefulLinkSpec>
            {
                PageIndex = 1,
                RequestedItemCount = 4
            };
            var test = await _usefulLinkService.GetUsefulLinksForAdminAsync(paginationParams).ConfigureAwait(false);
            Assert.Equal(4, test.TotalDataCount);
        }

        [Fact]
        public async Task GetUsefulLinksForAdmin_SpecProfessionId_2()
        {
            var paginationParams = new PaginationParamsWithSpec<UsefulLinkSpec>
            {
                PageIndex = 1,
                RequestedItemCount = 2,
                Spec = new UsefulLinkSpec
                {
                    ProfessionId=1.ToGuid()
                }
            };
            var test = await _usefulLinkService.GetUsefulLinksForMentorAsync(paginationParams).ConfigureAwait(false);
            Assert.Equal(2, test.TotalDataCount);
        }

        [Fact]
        public async Task GetUsefulLinksForAdmin_SpecTittle_1()
        {
            var paginationParams = new PaginationParamsWithSpec<UsefulLinkSpec>
            {
                PageIndex = 1,
                RequestedItemCount = 2,
                Spec = new UsefulLinkSpec
                {
                    Title="C# Dersleri"
                }
            };
            var test = await _usefulLinkService.GetUsefulLinksForAdminAsync(paginationParams).ConfigureAwait(false);
            Assert.Equal(2, test.TotalDataCount);
        }

        #endregion

        #region GetAnnouncementsForMentor

        [Fact]
        public async Task GetUsefulLinksForMentor_GetAll_4()
        {
            var paginationParams = new PaginationParamsWithSpec<UsefulLinkSpec>
            {
                PageIndex = 1,
                RequestedItemCount = 4
            };
            var test = await _usefulLinkService.GetUsefulLinksForMentorAsync(paginationParams).ConfigureAwait(false);
            Assert.Equal(4, test.TotalDataCount);
        }

        [Fact]
        public async Task GetUsefulLinksForMentor_SpecTitle_2()
        {
            var paginationParams = new PaginationParamsWithSpec<UsefulLinkSpec>
            {
                PageIndex = 1,
                RequestedItemCount = 2,
                Spec = new UsefulLinkSpec
                {
                    Title="C# Dersleri"
                }
            };
            var test = await _usefulLinkService.GetUsefulLinksForMentorAsync(paginationParams).ConfigureAwait(false);
            Assert.Equal(2, test.TotalDataCount);
        }

        [Fact]
        public async Task GetUsefulLinksForMentor_SpecProfessionId_2()
        {
            var paginationParams = new PaginationParamsWithSpec<UsefulLinkSpec>
            {
                PageIndex = 1,
                RequestedItemCount = 2,
                Spec = new UsefulLinkSpec
                {
                    ProfessionId=2.ToGuid()
                }
            };
            var test = await _usefulLinkService.GetUsefulLinksForMentorAsync(paginationParams).ConfigureAwait(false);
            Assert.Equal(2, test.TotalDataCount);
        }

        #endregion

        #region GetAnnouncementsForStudent

        [Fact]
        public async Task GetUsefulLinksForStudent_2()
        {
            var paginationParams = new PaginationParamsWithSpec<UsefulLinkSpec>
            {
                PageIndex = 1,
                RequestedItemCount = 4
            };
            var test = await _usefulLinkService.GetUsefulLinksForStudentAsync(paginationParams).ConfigureAwait(false);
            Assert.Equal(4, test.TotalDataCount);
        }

        [Fact]
        public async Task GetUsefulLinksForStudent_SpecTitle_2()
        {
            var paginationParams = new PaginationParamsWithSpec<UsefulLinkSpec>
            {
                PageIndex = 1,
                RequestedItemCount = 1,
                Spec = new UsefulLinkSpec
                {
                    Title = "Java Dersleri"
                }
            };
            var test = await _usefulLinkService.GetUsefulLinksForStudentAsync(paginationParams).ConfigureAwait(false);
            Assert.Equal(1, test.TotalDataCount);
        }

        [Fact]
        public async Task GetUsefulLinksForStudent_SpecProfessionId_1()
        {
            var paginationParams = new PaginationParamsWithSpec<UsefulLinkSpec>
            {
                PageIndex = 1,
                RequestedItemCount = 2,
                Spec = new UsefulLinkSpec
                {
                    ProfessionId = 2.ToGuid()
                }
            };
            var test = await _usefulLinkService.GetUsefulLinksForStudentAsync(paginationParams).ConfigureAwait(false);
            Assert.Equal(2, test.TotalDataCount);
        }

        #endregion

        #region GetAnnouncementForAdmin

        [Fact]
        public async Task GetUsefulLinkForAdmin_NegativId_ThrowException()
        {
            Func<Task> act = () => _usefulLinkService.GetUsefulLinkForAdminAsync((-2).ToGuid());
            var exception = await Assert.ThrowsAsync<MilvaUserFriendlyException>(act);

            Assert.Equal("Böyle bir link bulunmamaktadır.", exception.Message);
        }

        [Fact]
        public async Task GetUsefulLinkForAdmin_PositiveNumber_1()
        {
            var test = await _usefulLinkService.GetUsefulLinkForAdminAsync(1.ToGuid()).ConfigureAwait(false);
            Assert.Equal(1.ToGuid(), test.ProfessionId);
        }

        #endregion

        #region GetAnnouncementForMentor

        [Fact]
        public async Task GetUsefulLinkForMentor_NegativId_ThrowException()
        {
            Func<Task> act = () => _usefulLinkService.GetUsefulLinkForMentorAsync((-2).ToGuid());
            var exception = await Assert.ThrowsAsync<MilvaUserFriendlyException>(act);

            Assert.Equal("Böyle bir link bulunmamaktadır.", exception.Message);
        }

        [Fact]
        public async Task GetUsefulLinkForMentor_PositiveNumber_1()
        {
            var test = await _usefulLinkService.GetUsefulLinkForMentorAsync(1.ToGuid()).ConfigureAwait(false);
            Assert.Equal(1.ToGuid(), test.ProfessionId);
        }

        #endregion

        #region GetAnnouncementForStudent

        [Fact]
        public async Task GetUsefulLinkForStudent_NegativId_ThrowException()
        {
            Func<Task> act = () => _usefulLinkService.GetUsefulLinkForStudentAsync((-2).ToGuid());
            var exception = await Assert.ThrowsAsync<MilvaUserFriendlyException>(act);

            Assert.Equal("Böyle bir link bulunmamaktadır.", exception.Message);
        }

        [Fact]
        public async Task GetUsefulLinkForStudent_PositiveNumber_1()
        {
            var test = await _usefulLinkService.GetUsefulLinkForStudentAsync(1.ToGuid()).ConfigureAwait(false);
            Assert.Equal(1.ToGuid(), test.ProfessionId);
        }
        #endregion

        [Fact]
        public async Task AddUsefulLink()
        {
            var newUsefulLinks = new AddUsefulLinkDTO
            {
                Title = "AddDeneme",
                Description = "Deneme",
                ProfessionId=1.ToGuid(),
                Url="www.deneme.com"
            };
            await _usefulLinkService.AddUsefulLinkAsync(newUsefulLinks).ConfigureAwait(false);
        }
        [Fact]
        public async Task UpdateUsefulLink()
        {
            var toBeUpdatedUsefulLink = new UpdateUsefulLinkDTO
            {
                Id = 1.ToGuid(),
                ProfessionId = 1.ToGuid(),
                Title = "C# 2 Dersleri",
                Url = "www.oguzhanbaran2.com"
            };
            await _usefulLinkService.UpdateUsefulLinkAsync(toBeUpdatedUsefulLink).ConfigureAwait(false);
        }

        [Fact]
        public async Task DeleteUsefulLink()
        {
            List<Guid> toBeDeletedList = new List<Guid>() { 1.ToGuid(), 2.ToGuid() };
            await _usefulLinkService.DeleteUsefulLinksAsync(toBeDeletedList).ConfigureAwait(false);
        }
    
    }
}
