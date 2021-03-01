using Milvasoft.Helpers.Exceptions;
using Milvasoft.SampleAPI.Data.Abstract;
using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Services.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Services.Concrete
{
    /// <summary>
    /// Provides todo task crud operations. 
    /// </summary>
    public class TodoService : IBaseService<TodoDTO>
    {
        private readonly IGenericRepository<Todo> _todoRepository;

        /// <summary>
        /// Constructor of todo service.
        /// </summary>
        /// <param name="todoRepository"></param>
        public TodoService(IGenericRepository<Todo> todoRepository)
        {
            _todoRepository = todoRepository;
        }

        /// <summary>
        /// Get all todos from database.
        /// </summary>
        /// <returns></returns>
        public async Task<List<TodoDTO>> GetEntitiesAsync()
        {
            var todos = await _todoRepository.GetEntitiesAsync().ConfigureAwait(false);

            var todoDTOList = from todo in todos
                              select new TodoDTO
                              {
                                  Id = todo.Id,
                                  Content = todo.Content,
                                  ReminMeDate = todo.ReminMeDate,
                                  DueDate = todo.DueDate,
                                  IsFavorite = todo.IsFavorite,
                                  CreationDate = todo.CreationDate,
                                  LastModificationDate = todo.LastModificationDate
                              };

            return todoDTOList.ToList();
        }

        /// <summary>
        /// Get one todo by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TodoDTO> GetEntityAsync(Guid id)
        {
            var todo = await _todoRepository.GetEntityAsync(id, i => i.Category).ConfigureAwait(false);

            return new TodoDTO
            {
                Id = todo.Id,
                Content = todo.Content,
                ReminMeDate = todo.ReminMeDate,
                CategoryId = todo.CategoryId,
                CategoryName = todo.Category?.Name,
                DueDate = todo.DueDate,
                IsFavorite = todo.IsFavorite,
                CreationDate = todo.CreationDate,
                LastModificationDate = todo.LastModificationDate
            };
        }

        /// <summary>
        /// Addd <paramref name="todoDTO"/> to database.
        /// </summary>
        /// <param name="todoDTO"></param>
        /// <returns></returns>
        public async Task<Guid> AddEntityAsync(TodoDTO todoDTO)
        {
            var todo = new Todo
            {
                Id = todoDTO.Id,
                Content = todoDTO.Content,
                ReminMeDate = todoDTO.ReminMeDate,
                CategoryId = todoDTO.CategoryId,
                DueDate = todoDTO.DueDate,
                CreationDate = DateTime.Now,
                IsFavorite = todoDTO.IsFavorite
            };

            return await _todoRepository.AddAsync(todo).ConfigureAwait(false);

        }

        /// <summary>
        /// Updates requested <paramref name="todoDTO"/> in database.
        /// </summary>
        /// <param name="todoDTO"></param>
        /// <returns></returns>
        public async Task UpdateEntityAsync(TodoDTO todoDTO)
        {
            var toBeUpdatedTodo = await _todoRepository.GetEntityAsync(todoDTO.Id).ConfigureAwait(false);

            if (toBeUpdatedTodo == null)
                throw new MilvaUserFriendlyException("Veritabanında varolmayan bir kaydı güncellemeye çalışıyorsunuz.");

            toBeUpdatedTodo.Id = todoDTO.Id;
            toBeUpdatedTodo.Content = todoDTO.Content;
            toBeUpdatedTodo.ReminMeDate = todoDTO.ReminMeDate;
            toBeUpdatedTodo.CategoryId = todoDTO.CategoryId;
            toBeUpdatedTodo.DueDate = todoDTO.DueDate;
            toBeUpdatedTodo.IsFavorite = todoDTO.IsFavorite;
            toBeUpdatedTodo.LastModificationDate = DateTime.Now;

            await _todoRepository.UpdateAsync(toBeUpdatedTodo).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes requested todo whoose id equals to <paramref name="id"/> from database.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteEntityAsync(Guid id)
        {
            if(id == Guid.Empty)
                throw new MilvaUserFriendlyException("Veritabanında varolmayan bir kaydı silmeye çalışıyorsunuz.");

            var toBeDeletedTodo = await _todoRepository.GetEntityAsync(id).ConfigureAwait(false);

            if (toBeDeletedTodo == null)
                throw new MilvaUserFriendlyException("Veritabanında varolmayan bir kaydı güncellemeye çalışıyorsunuz.");

            await _todoRepository.DeleteAsync(toBeDeletedTodo).ConfigureAwait(false);
        }

        /// <summary>
        /// Sended favorites as mark.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isFavorite"></param>
        /// <returns></returns>
        public async Task MarkAsIsFavoriteAsync(Guid id, bool isFavorite)
        {
            var todo = await _todoRepository.GetEntityAsync(id).ConfigureAwait(false);

            todo.IsFavorite = isFavorite;

            await _todoRepository.UpdateAsync(todo).ConfigureAwait(false);
        }
    }
}
