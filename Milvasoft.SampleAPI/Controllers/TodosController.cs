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
    public class TodosController : ControllerBase
    {
        private IBaseService<TodoDTO> _todoService;


        public TodosController(IBaseService<TodoDTO> todoService)
        {
            _todoService = todoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTodosAsync()
        {
            var todos = await _todoService.GetEntitiesAsync().ConfigureAwait(false);
            return Ok(todos);
        }

        [HttpGet("Todo/{id}")]
        public async Task<IActionResult> GetTodoAsync(Guid id)
        {
            var todo = await _todoService.GetEntityAsync(id).ConfigureAwait(false);
            return Ok(todo);
        }

        [HttpPost("Todo")]
        public async Task<IActionResult> AddTodoAsync(TodoDTO todoDTO)
        {
            var todoId = await _todoService.AddEntityAsync(todoDTO).ConfigureAwait(false);

            if (todoId != Guid.Empty)
                return Ok(todoId);
            else return BadRequest("Bir hata oluştu lütfen tekrar deneyiniz.");
        }

        [HttpPut("Todo")]
        public async Task<IActionResult> UpdateTodoAsync(TodoDTO todoDTO)
        {
            await _todoService.UpdateEntityAsync(todoDTO).ConfigureAwait(false);
            return Ok("İşlem Başarılı");
        }

        [HttpDelete("Todo/{id}")]
        public async Task<IActionResult> DeleteTodoAsync(Guid id)
        {
            await _todoService.DeleteEntityAsync(id).ConfigureAwait(false);
            return Ok("İşlem Başarılı");
        }

        [HttpPut("Todo/{id}/Mark/{isFavorite}")]
        public async Task<IActionResult> MarkAsFavoriteTodoAsync(Guid id, bool isFavorite)
        {
            await _todoService.MarkAsIsFavoriteAsync(id, isFavorite).ConfigureAwait(false);

            return Ok("İşlem başarılı.");
        }

    }
}
