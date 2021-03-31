using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.Models
{
    /// <summary>
    ///  Abstraction for multiple file upload process.
    /// </summary>
    public interface IFileEntity
    {
        /// <summary>
        /// Name of file.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Path of file.
        /// </summary>
        public string FilePath { get; set; }
    }
}
