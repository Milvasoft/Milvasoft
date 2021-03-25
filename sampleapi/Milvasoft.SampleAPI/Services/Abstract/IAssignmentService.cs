using Milvasoft.SampleAPI.DTOs.AssignmentDTOs;
using Milvasoft.SampleAPI.Spec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Services.Abstract
{
    interface IAssignmentService : IBaseService<AssignmentDTO,AssignmentSpec>
    {
    }
}
