using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.Models
{
    /// <summary>
    /// Abstraction for multiple file upload process.
    /// </summary>
    public interface IFileDTO
    {
        /// <summary>
        /// Name of file.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// File.
        /// </summary>
        public IFormFile File { get; set; }
    }
}
