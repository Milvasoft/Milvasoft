using Milvasoft.SampleAPI.DTOs.AccountDTOs;
using Milvasoft.SampleAPI.Services.Abstract;
using Milvasoft.SampleAPI.UnitTest.TestHelpers;
using System.Threading.Tasks;
using Xunit;

namespace Milvasoft.SampleAPI.UnitTest.TestForTest
{
    public class Test
    {
        //private readonly IAccountService _accountService;
            
        //public Test()
        //{
        //    var serviceCollection = new ServiceCollectionHelper();

        //    //Servislerde eğer httpcontext üzerinden userName bilgisi isteniyor ise bu metot çağrılıp istreğe bağlı giriş yaptırmak istediğiniz kullanıcının kullaıcı adını girebilirsin.
        //    serviceCollection.MockLoggedUserWithHttpContextAccessor();

        //    //Test edilmek istenen servis bu şekilde mocklanabilir.
        //    _accountService = serviceCollection.GetService<IAccountService>();


        //}

        //[Fact]
        //public async Task Deneme()
        //{
        //    var a = new SignUpDTO
        //    {
        //        Email = "emresever0507@gmial.com",
        //        Name = "Emre",
        //        Password = "Es+124",
        //        PhoneNumber = "0 505 010 79 85",
        //        Surname = "Sever",
        //        UserName = "emreSever"
        //    };

        //    await _accountService.SignUpAsync(a).ConfigureAwait(false);
        //}
    }
}
