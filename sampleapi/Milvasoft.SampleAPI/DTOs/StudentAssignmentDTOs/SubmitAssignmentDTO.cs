using Microsoft.AspNetCore.Http;
using Milvasoft.SampleAPI.Utils;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using System;

namespace Milvasoft.SampleAPI.DTOs
{
    /// <summary>
    /// DTO is required to submit assignments.
    /// </summary>
    public class SubmitAssignmentDTO
    {
        /// <summary>
        /// Id of the homework to be sent
        /// </summary>
        public Guid AssigmentId { get; set; }
        /// <summary>
        /// Assignment file.
        /// </summary>
        public IFormFile Assignment { get; set; }

        /// <summary>
        /// File base64string.
        /// </summary>
        public string _fileBase64String;

        /// <summary>
        /// <para><b>EN:</b>File bse64 string of assignment.</para>
        /// </summary>
        [OValidateString(1073741823)]
        public string ImageBase64String
        {
            get => _fileBase64String;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    Assignment = HelperExtensions.ConvertToFormFile(value);
                }
                _fileBase64String = value;
            }
        }

        /// <summary>
        /// Assignment description to mentor.
        /// </summary>
        public string Description { get; set; }
    }
}
