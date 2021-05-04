using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.DTOs
{
    /// <summary>
    /// DTO is required to submit assignments.
    /// </summary>
    public class SubmitAssignmentDTO
    {
        /// <summary>
        /// Id of the homework to be sent
        /// </summary>
        public Guid AssigmentId{ get; set; }
        /// <summary>
        /// Assignment file.
        /// </summary>
        public IFormFile Assignment { get; set; }

        /// <summary>
        /// Assignment description to mentor.
        /// </summary>
        public string Description { get; set; }
    }
}
