using Milvasoft.SampleAPI.DTOs.ProfessionDTOs;
using Milvasoft.SampleAPI.Spec;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Services.Abstract
{
    /// <summary>
    /// Profession service interface.
    /// </summary>
    public interface IProfessionService : IBaseService<ProfessionSpec, AddProfessionDTO, UpdateProfessionDTO, ProfessionDTO, ProfessionDTO, ProfessionDTO>
    {
        Task TestMethod();
    }
}
