using Microsoft.AspNetCore.Http;
using Milvasoft.SampleAPI.Utils;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.DTOs
{
    public class ImageUploadDTO
    {
        /// <summary>
        /// Id of the user whose official will be updated
        /// </summary>
        [OValidateId]
        public Guid UserId { get; set; }

        /// <summary>
        /// <para><b>EN: </b> Uploaded image of personnel.</para>
        /// <para><b>TR: </b> Yüklenen personel resmi.</para>
        /// </summary>
        public IFormFile Image { get; set; }

        private string _imageBase64String;

        /// <summary>
        /// <para><b>EN:</b>Image bse64 string of user.</para>
        /// </summary>
        [OValidateString(1073741823)]
        public string ImageBase64String
        {
            get => _imageBase64String;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    Image = HelperExtensions.ConvertToFormFile(value);
                }
                _imageBase64String = value;
            }
        }
    }
}
