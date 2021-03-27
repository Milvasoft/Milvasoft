using Milvasoft.SampleAPI.DTOs.AssignmentDTOs;
using Milvasoft.SampleAPI.Spec;

namespace Milvasoft.SampleAPI.Services.Abstract
{
    /// <summary>
    /// Assignment service interface.
    /// </summary>
    interface IAssignmentService : IBaseService<AssignmentDTO, AssignmentSpec, AddAssignmentDTO, UpdateAssignmentDTO>
    {
    }
}
