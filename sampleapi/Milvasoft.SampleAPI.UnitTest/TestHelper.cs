using Milvasoft.Helpers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

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
