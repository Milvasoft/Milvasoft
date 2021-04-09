using Milvasoft.SampleAPI.DTOs.AssignmentDTOs;
using Milvasoft.SampleAPI.Spec;

namespace Milvasoft.SampleAPI.Services.Abstract
{
    /// <summary>
    /// Assignment service interface.
    /// </summary>
    public interface IAssignmentService : IBaseService<AssignmentSpec, AddAssignmentDTO, UpdateAssignmentDTO, AssignmentForStudentDTO, AssignmentForMentorDTO, AssignmentForAdminDTO>
    {
    }
}
