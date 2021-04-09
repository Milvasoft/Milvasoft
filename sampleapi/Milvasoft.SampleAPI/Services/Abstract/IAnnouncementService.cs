using Milvasoft.SampleAPI.DTOs.AnnouncementDTOs;
using Milvasoft.SampleAPI.Spec;

namespace Milvasoft.SampleAPI.Services.Abstract
{
    /// <summary>
    /// Announcement service interface.
    /// </summary>
    public interface IAnnouncementService : IBaseService<AnnouncementSpec, AddAnnouncementDTO, UpdateAnnouncementDTO, AnnouncementForStudentDTO, AnnouncementForMentorDTO, AnnouncementForAdminDTO>
    {
    }
}
