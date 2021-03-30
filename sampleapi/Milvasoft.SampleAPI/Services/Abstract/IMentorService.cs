﻿using Milvasoft.SampleAPI.DTOs.MentorDTOs;
using Milvasoft.SampleAPI.Spec;

namespace Milvasoft.SampleAPI.Services.Abstract
{
    /// <summary>
    /// Mentor service interface.
    /// </summary>
    public interface IMentorService : IBaseService<MentorDTO, MentorSpec, AddMentorDTO, UpdateMentorDTO>
    {
    }
}