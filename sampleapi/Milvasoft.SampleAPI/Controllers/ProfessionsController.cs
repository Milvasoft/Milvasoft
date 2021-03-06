﻿using Microsoft.AspNetCore.Mvc;
using Milvasoft.Helpers;
using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.DTOs.ProfessionDTOs;
using Milvasoft.SampleAPI.Services.Abstract;
using Milvasoft.SampleAPI.Spec;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Controllers
{
    /// <summary>
    /// Provided profession operations.
    /// </summary>
    [Route("sampleapi/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1.0")]
    public class ProfessionsController : Controller
    {
        private readonly IProfessionService _professionService;

        /// <summary>
        /// Constructor of <c>ProfessionController</c>.
        /// </summary>
        /// <param name="professionService"></param>
        public ProfessionsController(IProfessionService professionService)
        {
            _professionService = professionService;
        }

        /// <summary>
        /// Gets the all filtered professions datas for mentor.
        /// </summary>
        /// <returns></returns>
        [HttpPatch("Mentor")]
        public async Task<IActionResult> GetProfessions([FromBody] PaginationParamsWithSpec<ProfessionSpec> paginationParams)
        {
            var professions = await _professionService.GetProfessionsAsync(paginationParams).ConfigureAwait(false);

            return professions.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the all filtered profession datas for mentor.
        /// </summary>
        /// <returns></returns>
        [HttpPatch("Mentor/{id}")]
        public async Task<IActionResult> GetProfessionForMentorbyId(Guid id)
        {
            var professions = await _professionService.GetProfessionForMentorAsync(id).ConfigureAwait(false);

            return professions.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the filtered profession datas for admin.
        /// </summary>
        /// <returns></returns>
        ///[Authorize(Roles = "Admin")]
        [HttpPatch("Admin/{id}")]
        public async Task<IActionResult> GetProfessionForAdminbyId(Guid id)
        {
            var professions = await _professionService.GetProfessionForAdminAsync(id).ConfigureAwait(false);

            return professions.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the filtered profession datas for student.
        /// </summary>
        /// <returns></returns>
        [HttpPatch("Student/{id}")]
        public async Task<IActionResult> GetProfessionForStudentbyId(Guid id)
        {
            var professions = await _professionService.GetProfessionForStudentAsync(id).ConfigureAwait(false);

            return professions.GetObjectResponse("Success");
        }

        /// <summary>
        /// Add <b><paramref name="addProfession"/></b> data to database.
        /// </summary>
        /// <param name="addProfession"></param>
        /// <returns></returns>
        [HttpPost("Profession")]
        public async Task<IActionResult> AddProfession([FromBody] AddProfessionDTO addProfession)
        {
            return await _professionService.AddProfessionAsync(addProfession).ConfigureAwait(false).GetObjectResponseAsync<AddProfessionDTO>("Success");
        }

        /// <summary>
        /// Update <paramref name="updateProfession"/> data.
        /// </summary>
        /// <param name="updateProfession"></param>
        /// <returns></returns>
        [HttpPut("Profession")]
        public async Task<IActionResult> UpdateProfession([FromBody] UpdateProfessionDTO updateProfession)
        {
            return await _professionService.UpdateProfessionAsync(updateProfession).ConfigureAwait(false).GetObjectResponseAsync<UpdateProfessionDTO>("Success");
        }

        /// <summary>
        /// Delete professions data by <paramref name="ids"/>
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteProfession([FromBody] List<Guid> ids)
        {
            return await _professionService.DeleteProfessionsAsync(ids).ConfigureAwait(false).GetObjectResponseAsync<UpdateProfessionDTO, Guid>(ids, "Success");
        }
    }
}
