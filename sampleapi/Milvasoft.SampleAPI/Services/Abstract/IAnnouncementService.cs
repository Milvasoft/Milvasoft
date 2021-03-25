using Milvasoft.SampleAPI.DTOs.AnnouncementDTOs;
using Milvasoft.SampleAPI.Spec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Services.Abstract
{
    public interface IAnnouncementService : IBaseService<AnnouncementDTO,AnnouncementSpec>
    {
    }
}
