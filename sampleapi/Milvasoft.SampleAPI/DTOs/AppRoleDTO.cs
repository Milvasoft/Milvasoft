using Microsoft.AspNetCore.Identity;
using Milvasoft.Helpers.DataAccess.Abstract.Entity;
using Milvasoft.SampleAPI.DTOs.MentorDTOs;
using Milvasoft.SampleAPI.DTOs.StudentDTOs;
using System;

namespace Milvasoft.SampleAPI.DTOs
{

    /// <summary>
    /// Role for all users.
    /// </summary>
    public class AppRoleDTO : IdentityRole<Guid>, IFullAuditable<Guid>
    {

        /// <summary>
        /// Photo path of users.
        /// </summary>
        public string PhotoPath { get; set; }

        /// <summary>
        /// Is the user confirmed?
        /// </summary>
        public bool IsConfirmedUser { get; set; }

        /// <summary>
        /// Is the user a mentor?
        /// </summary>
        public bool IsMentor { get; set; }

        /// <summary>
        /// Last modification date of user information.
        /// </summary>
        public DateTime? LastModificationDate { get; set; }

        /// <summary>
        /// User creation date.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Id of the user who created the user.
        /// </summary>
        public Guid? CreatorUserId { get; set; }

        /// <summary>
        /// Id of last modified user.
        /// </summary>
        public Guid? LastModifierUserId { get; set; }

        /// <summary>
        /// Deletion date.
        /// </summary>
        public DateTime? DeletionDate { get; set; }

        /// <summary>
        /// Can it be deleted?
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Id of the user who deleted.
        /// </summary>
        public Guid? DeleterUserId { get; set; }

        /// <summary>
        /// Is the user mentor?
        /// </summary>
        public virtual MentorDTO Mentor { get; set; }

        /// <summary>
        /// Is the user student?
        /// </summary>
        public virtual StudentDTO Student { get; set; }
    }
}
