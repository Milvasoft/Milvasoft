using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.DTOs.AccountDTOs
{
    /// <summary>
    /// User password processing takes place with this dto.
    /// </summary>
    public class ChangePassDTO
    {
        /// <summary>
        /// User name of the user.
        /// </summary>
        [OValidateString(2, 100)]
        public string UserName { get; set; }

        /// <summary>
        /// Old password of the user.
        /// </summary>
        [OValidateString(2, 250)]
        public string OldPassword { get; set; }

        /// <summary>
        /// New password of the user.
        /// </summary>
        [OValidateString(2, 250)]
        public string NewPassword { get; set; }
    }
}
