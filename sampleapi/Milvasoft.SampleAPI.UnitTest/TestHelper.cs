using Milvasoft.Helpers.Models;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.UnitTest
{
    public static class TestHelper
    {
        public static async Task<bool> isTrue<TDTO>(this PaginationDTO<TDTO> paginationDTO)
        {
            return true;
        }
    }
}
