using Microsoft.AspNetCore.Http;
using Milvasoft.Helpers.DataAccess.Concrete.Entity;
using Milvasoft.SampleAPI.DTOs.AnnouncementDTOs;
using Milvasoft.SampleAPI.DTOs.StudentDTOs;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Utils;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using System;
using System.Collections.Generic;

namespace Milvasoft.SampleAPI.DTOs.MentorDTOs
{

    /// <summary>
    /// Mentor DTO.
    /// </summary>
    public class MentorDTO : AuditableEntity<AppUser, Guid, Guid>
    {
        /// <summary>
        /// Mentor name.
        /// </summary>
        [OValidateString(2, 100)]
        public string Name { get; set; }

        /// <summary>
        /// Mentor surname.
        /// </summary>
        [OValidateString(2, 100)]
        public string Surname { get; set; }

        /// <summary>
        /// CV path of mentor.
        /// </summary>
        public string CVFilePath { get; set; }

        /// <summary>
        /// AppUser ID of mentor.
        /// </summary>
        public Guid AppUserId { get; set; }

        /// <summary>
        /// AppUser of mentor.
        /// </summary>
        public virtual AppUserDTO AppUser { get; set; }

        /// <summary>
        /// <para><b>EN: </b> Uploaded image of personnel.</para>
        /// <para><b>TR: </b> Yüklenen personel resmi.</para>
        /// </summary>
        public IFormFile Image { get; set; }

        /// <summary>
        /// The base64 version of the image file.
        /// </summary>
        private string _imageBase64String;

        /// <summary>
        /// <para><b>EN:</b>Image bse64 string of menu.</para>
        /// <para><b>TR:</b>Menüye ait resmin base64 string değeri.</para>
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

        /// <summary>
        /// Announcements posted by the mentor.
        /// </summary>
        public virtual List<AnnouncementDTO> PublishedAnnouncements { get; set; }

        /// <summary>
        /// Professions of a mentor.
        /// </summary>
        public virtual List<MentorProfessionDTO> Professions { get; set; }

        /// <summary>
        /// Students of a mentor.
        /// </summary>
        public virtual List<StudentDTO> Students { get; set; }
    }
}
