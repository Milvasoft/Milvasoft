using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.Models
{
    public interface IFileEntity
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }
}
