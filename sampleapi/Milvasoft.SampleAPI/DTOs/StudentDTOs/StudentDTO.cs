using Microsoft.AspNetCore.Http;
using Milvasoft.Helpers.DataAccess.Concrete.Entity;
using Milvasoft.SampleAPI.DTOs.MentorDTOs;
using Milvasoft.SampleAPI.DTOs.ProfessionDTOs;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Entity.Enum;
using Milvasoft.SampleAPI.Utils;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using Milvasoft.SampleAPI.Utils.Swagger;
using System;
using System.Collections.Generic;

namespace Milvasoft.SampleAPI.DTOs.StudentDTOs
{
    /// <summary>
    /// Student DTO.
    /// </summary>
    public class StudentDTO : AuditableEntity<AppUser, Guid, Guid>
    {
        /// <summary>
        /// Student's name.
        /// </summary>
        [OValidateString(2, 200)]
        public string Name { get; set; }

        /// <summary>
        /// Student's surname.
        /// </summary>
        [OValidateString(2, 200)]
        public string Surname { get; set; }

        /// <summary>
        /// The student's starting level of homework.
        /// </summary>
        [OValidateDecimal(20)]
        public int Level { get; set; }

        /// <summary>
        /// Student's university.
        /// </summary>
        [OValidateString(2, 200)]
        public string University { get; set; }

        /// <summary>
        /// Age of student.
        /// </summary>
        [OValidateDecimal(30)]
        public int Age { get; set; }

        /// <summary>
        /// Dream of student.
        /// </summary>
        [OValidateString(2000)]
        public string Dream { get; set; }

        /// <summary>
        /// Home adress of student.
        /// </summary>
        [OValidateString(2000)]
        public string HomeAddress { get; set; }

        /// <summary>
        /// The mentor's thoughts about the student.
        /// </summary>
        [OValidateString(2000)]
        public string MentorThoughts { get; set; }

        /// <summary>
        /// Did the student sign the contract?
        /// </summary>
        public bool IsConfidentialityAgreementSigned { get; set; }

        /// <summary>
        /// Education status of student.
        /// </summary>
        public EducationStatus GraduationStatus { get; set; }

        /// <summary>
        /// Gradution score of student.
        /// </summary>
        [OValidateDecimal(100)]
        public int GraduationScore { get; set; }

        /// <summary>
        /// The mentor's graduation thoughts of student.
        /// </summary>
        [OValidateString(2000)]
        public string MentorGraduationThoughts { get; set; }

        /// <summary>
        /// Due date of current assignment.
        /// </summary>
        public DateTime CurrentAssigmentDeliveryDate { get; set; }

        /// <summary>
        /// <para><b>EN: </b> Uploaded image of personnel.</para>
        /// <para><b>TR: </b> Yüklenen personel resmi.</para>
        /// </summary>
        public IFormFile Image { get; set; }

        /// <summary>
        /// base64 version of the image file.
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
        /// AppUser id.
        /// </summary>
        [OValidateId]
        public Guid AppUserId { get; set; }

        /// <summary>
        /// AppUser of student.
        /// </summary>
        [SwaggerExclude]
        public virtual AppUserDTO AppUser { get; set; }

        /// <summary>
        /// Profession id of student.
        /// </summary>
        [OValidateId]
        public Guid ProfessionId { get; set; }

        /// <summary>
        /// Profesion of student.
        /// </summary>
        [SwaggerExclude]
        public virtual ProfessionDTO Profession { get; set; }

        /// <summary>
        /// Mentor ıd of student.
        /// </summary>
        [OValidateId]
        public Guid MentorId { get; set; }

        /// <summary>
        /// Mentor of student.
        /// </summary>
        [SwaggerExclude]
        public virtual MentorDTO Mentor { get; set; }

        /// <summary>
        /// Old assignments of student.
        /// </summary>
        public virtual List<StudentAssigment> OldAssignments { get; set; }
    }
}
