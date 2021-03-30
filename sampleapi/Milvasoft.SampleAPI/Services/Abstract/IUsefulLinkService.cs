﻿using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.DTOs.UsefulLinkDTOs;
using Milvasoft.SampleAPI.Spec;

namespace Milvasoft.SampleAPI.Services.Abstract
{
    /// <summary>
    /// UsefulLink service inteface.
    /// </summary>
    public interface IUsefulLinkService : IBaseService<UsefulLinkDTO, UsefulLinkSpec, AddUsefulLinkDTO, UpdateUsefulLinkDTO>
    {
    }
}