using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.Services.Abstract;
using System;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1.0")]
    [Route("sampleapi/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private IBaseService<CategoryDTO> _categoryService;

        public CategoriesController(IBaseService<CategoryDTO> categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Gets all categories.
        /// </summary>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        public async Task<IActionResult> GetEntitiesAsync()
        {
            var categories = await _categoryService.GetEntitiesAsync().ConfigureAwait(false);
            return Ok(categories);
        }

        [HttpGet("Category/{id}")]
        public async Task<IActionResult> GetEntityAsync(Guid id)
        {
            var category = await _categoryService.GetEntityAsync(id).ConfigureAwait(false);
            return Ok(category);
        }

        [HttpPost("Category")]
        public async Task<IActionResult> AddEntityAsync(CategoryDTO categoryDTO)
        {
            var categoryId = await _categoryService.AddEntityAsync(categoryDTO).ConfigureAwait(false);

            if (categoryId != Guid.Empty)
                return Ok(categoryId);
            else return BadRequest("Bir hata oluştu lütfen tekrar deneyiniz.");
        }

        [HttpPut("Category")]
        public async Task<IActionResult> UpdateEntityAsync(CategoryDTO categoryDTO)
        {
            await _categoryService.UpdateEntityAsync(categoryDTO).ConfigureAwait(false);
            return Ok("İşlem Başarılı");
        }

        [HttpDelete("Category/{id}")]
        public async Task<IActionResult> DeleteEntityAsync(Guid id)
        {
            await _categoryService.DeleteEntityAsync(id).ConfigureAwait(false);
            return Ok("İşlem Başarılı");
        }

    }
}
