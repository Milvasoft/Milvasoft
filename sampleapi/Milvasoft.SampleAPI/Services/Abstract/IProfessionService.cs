using Milvasoft.SampleAPI.DTOs.ProfessionDTOs;
using Milvasoft.SampleAPI.Spec;

namespace Milvasoft.SampleAPI.Services.Abstract
{
    /// <summary>
    /// Profession service interface.
    /// </summary>
    public interface IProfessionService : IBaseService<ProfessionDTO, ProfessionSpec, AddProfessionDTO, UpdateProfessionDTO>
    {
    }
}
