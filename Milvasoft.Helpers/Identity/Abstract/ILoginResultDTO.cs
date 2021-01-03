using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Milvasoft.Helpers.Identity.Concrete
{
    public interface ILoginResultDTO
    {
        public List<IdentityError> ErrorMessages { get; set; }
        public string Token { get; set; }
    }
}