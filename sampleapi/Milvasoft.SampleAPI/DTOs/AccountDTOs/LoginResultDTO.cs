using Microsoft.AspNetCore.Identity;
using Milvasoft.Helpers.Identity.Concrete;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using System.Collections.Generic;

namespace Milvasoft.SampleAPI.DTOs.AccountDTOs
{
    public class LoginResultDTO : ILoginResultDTO
    {
        public List<IdentityError> ErrorMessages { get; set; }

        [OValidateString(5000)]
        public string Token { get; set; }
    }
}