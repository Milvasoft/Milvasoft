using Microsoft.AspNetCore.Mvc;
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
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1.0")]
    public class ProfessionController : Controller
    {
        private readonly IProfessionService _professionService;

        /// <summary>
        /// Constructor of <c>ProfessionController</c>.
        /// </summary>
        /// <param name="professionService"></param>
        public ProfessionController(IProfessionService professionService)
        {
            _professionService = professionService;
        }

        /// <summary>
        /// Test controller.
        /// </summary>
        /// <returns></returns>
        [HttpGet("Test")]
        public async Task<IActionResult> Test()
        {
            await _professionService.TestMethod().ConfigureAwait(false);
            return Ok("Ok");
        }

        /// <summary>
        /// Gets the all filtered professions datas for mentor.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Mentor")]
        [HttpPatch("Mentor")]
        public async Task<IActionResult> GetProfessionsForMentor([FromBody] PaginationParamsWithSpec<ProfessionSpec> paginationParams)
        {
            var professions = await _professionService.GetProfessionsForMentorAsync(paginationParams).ConfigureAwait(false);
            return professions.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the all filtered professions datas for admin.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Admin")]
        [HttpPatch("Admin")]
        public async Task<IActionResult> GetProfessionsForAdmin([FromBody] PaginationParamsWithSpec<ProfessionSpec> paginationParams)
        {
            var professions = await _professionService.GetProfessionsForAdminAsync(paginationParams).ConfigureAwait(false);
            return professions.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the all filtered professions datas for student.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Student")]
        [HttpPatch("Student")]
        public async Task<IActionResult> GetProfessionsForStudent([FromBody] PaginationParamsWithSpec<ProfessionSpec> paginationParams)
        {
            var professions = await _professionService.GetProfessionsForStudentAsync(paginationParams).ConfigureAwait(false);
            return professions.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the all filtered profession datas for mentor.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Mentor")]
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
        //[Authorize(Roles = "Student")]
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
       // [Authorize(Roles = "Admin")]
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
       // [Authorize(Roles = "Admin")]
        [HttpPut("Profession")]
        public async Task<IActionResult> UpdateProfession([FromBody] UpdateProfessionDTO updateProfession)
        {
             return  await _professionService.UpdateProfessionAsync(updateProfession).ConfigureAwait(false).GetObjectResponseAsync<UpdateProfessionDTO>("Success"); 
        }

        /// <summary>
        /// Delete professions data by <paramref name="ids"/>
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteProfession( [FromBody] List<Guid> ids)
        {
           return await _professionService.DeleteProfessionsAsync(ids).ConfigureAwait(false).GetObjectResponseAsync<UpdateProfessionDTO,Guid>(ids,"Success"); 
        }
    }
}
