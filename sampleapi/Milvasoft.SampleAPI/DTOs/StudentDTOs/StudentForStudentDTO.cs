﻿using Milvasoft.Helpers.DataAccess.Concrete.Entity;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using System;

namespace Milvasoft.SampleAPI.DTOs.StudentDTOs
{
    /// <summary>
    /// Student entities for other student.
    /// </summary>
    public class StudentForStudentDTO : AuditableEntity<AppUser, Guid, Guid>
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
        /// Student's university.
        /// </summary>
        [OValidateString(2, 200)]
        public string University { get; set; }

        /// <summary>
        /// Age of student.
        /// </summary>
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


    }
}