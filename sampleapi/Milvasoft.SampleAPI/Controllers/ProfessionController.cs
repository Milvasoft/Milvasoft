using Microsoft.AspNetCore.Mvc;
using Milvasoft.API.DTOs;
using Milvasoft.SampleAPI.DTOs.ProfessionDTOs;
using Milvasoft.SampleAPI.Services.Abstract;
using Milvasoft.SampleAPI.Spec;
using System;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Controllers
{
    /// <summary>
    /// Provided profession operations.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class ProfessionController : Controller
    {
        private readonly IProfessionService _professionService;

        /// <summary>
        /// ProfessionController concrete for injection.
        /// </summary>
        /// <param name="professionService"></param>
        public ProfessionController(IProfessionService professionService)
        {
            _professionService = professionService;
        }


        [HttpGet("Test")]
        public async Task<IActionResult> Test()
        {
            await _professionService.TestMethod().ConfigureAwait(false);
            return Ok();
        }

        /// <summary>
        /// Add profession to database.
        /// </summary>
        /// <param name="profession"></param>
        /// <returns></returns>
        [HttpPost("AddProfession")]
        public async Task<IActionResult> AddProfession([FromBody] AddProfessionDTO profession)
        {
            await _professionService.AddEntityAsync(profession).ConfigureAwait(false);
            return Ok();
        }

        /// <summary>
        /// Get all profession.
        /// </summary>
        /// <returns></returns>
        [HttpPatch("Get")]
        public async Task<IActionResult> GetProfession([FromBody] PaginationParamsWithSpec<ProfessionSpec> paginationParams)
        {
            var professions = await _professionService.GetEntitiesForMentorAsync(paginationParams.PageIndex, paginationParams.RequestedItemCount, paginationParams.OrderByProperty, paginationParams.OrderByAscending, paginationParams.Spec).ConfigureAwait(false);
            return Ok(professions);
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteProfession(Guid id)
        {
            await _professionService.DeleteEntityAsync(id).ConfigureAwait(false);
            return Ok();
        }
    }
}
