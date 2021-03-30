using Milvasoft.SampleAPI.DTOs.ProfessionDTOs;
using Milvasoft.SampleAPI.Spec;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Services.Abstract
{
    /// <summary>
    /// Profession service interface.
    /// </summary>
    public interface IProfessionService : IBaseService<ProfessionDTO, ProfessionSpec, AddProfessionDTO, UpdateProfessionDTO>
    {
        Task TestMethod();
    }
}
