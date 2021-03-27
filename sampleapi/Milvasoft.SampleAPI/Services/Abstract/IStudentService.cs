using Milvasoft.SampleAPI.DTOs.StudentDTOs;
using Milvasoft.SampleAPI.Spec;

namespace Milvasoft.SampleAPI.Services.Abstract
{
    /// <summary>
    /// Student service inteface.
    /// </summary>
    public interface IStudentService : IBaseService<StudentDTO, StudentSpec,AddStudentDTO,UpdateStudentDTO>
    {

    }
}
