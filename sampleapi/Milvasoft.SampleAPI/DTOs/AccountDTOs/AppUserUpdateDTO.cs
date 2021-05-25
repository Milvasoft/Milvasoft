using Microsoft.AspNetCore.Identity;
using Milvasoft.Helpers.Attributes.Validation;
using Milvasoft.Helpers.DataAccess.Abstract.Entity;
using Milvasoft.SampleAPI.Localization;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.DTOs.AccountDTOs
{
    /// <summary>
    /// DTO to be user in creation proccess.
    /// </summary>
    public class AppUserUpdateDTO : IdentityUser<Guid>
    {
        /// <summary>
        /// Name of to be created user by admin.
        /// </summary>
        [OValidateString(3, 25, MemberNameLocalizerKey = "LocalizedName")]
        public string NewName { get; set; }

        /// <summary>
        /// Surname of to be created user by admin.
        /// </summary>
        [OValidateString(3, 25, MemberNameLocalizerKey = "LocalizedSurname")]
        public string NewSurname { get; set; }

        /// <summary>
        /// Phone number which can be entered in initial update process after registeration process.
        /// </summary>
        [OValidateString(0, 16)]
        [MilvaRegex(typeof(SharedResource), IsRequired = false)]
        public string NewPhoneNumber { get; set; }
    }
}
