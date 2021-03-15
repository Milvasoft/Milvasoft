using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.DataAccess.IncludeLibrary;
using Milvasoft.Helpers.Exceptions;
using Milvasoft.Helpers.Extensions;
using Milvasoft.SampleAPI.Data;
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
    /// Provides category task crud operations. 
    /// </summary>
    public class CategoryService : IBaseService<CategoryDTO>
    {
        private readonly IBaseRepository<Category, Guid, TodoAppDbContext> _categoryRepository;

        /// <summary>
        /// Constructor of category service.
        /// </summary>
        /// <param name="categoryRepository"></param>
        public CategoryService(IBaseRepository<Category, Guid, TodoAppDbContext> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        /// <summary>
        /// Get all categories from database.
        /// </summary>
        /// <returns></returns>
        public async Task<List<CategoryDTO>> GetEntitiesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync(i => i.Include(i=>i.Todos)).ConfigureAwait(false);

            var categoryDTOList = from category in categories
                                  select new CategoryDTO
                                  {
                                      Id = category.Id,
                                      Name = category.Name,
                                      TodoCount = category.Todos?.Count ?? 0,
                                      CreationDate = category.CreationDate,
                                      LastModificationDate = category.LastModificationDate
                                  };

            return categoryDTOList.ToList();
        }

        /// <summary>
        /// Get one category by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<CategoryDTO> GetEntityAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id, i => i.Include(i => i.Todos)).ConfigureAwait(false);

            if (category == null)
                throw new MilvaUserFriendlyException("Veritabanında varolmayan bir kayda erişmeye çalışıyorsunuz.");

            return new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                TodoCount = category.Todos?.Count ?? 0,
                Todos = !category.Todos.IsNullOrEmpty()
                        ? (from todo in category.Todos
                           select new TodoDTO
                           {
                               Id = todo.Id,
                               Content = todo.Content,
                               ReminMeDate = todo.ReminMeDate,
                               DueDate = todo.DueDate,
                               CreationDate = todo.CreationDate,
                               LastModificationDate = todo.LastModificationDate
                           }).ToList()
                        : null,
                CreationDate = category.CreationDate,
                LastModificationDate = category.LastModificationDate
            };
        }

        /// <summary>
        /// Addd <paramref name="categoryDTO"/> to database.
        /// </summary>
        /// <param name="categoryDTO"></param>
        /// <returns></returns>
        public async Task<Guid> AddEntityAsync(CategoryDTO categoryDTO)
        {
            var category = new Category
            {
                Id = categoryDTO.Id,
                Name = categoryDTO.Name,
                CreationDate = DateTime.Now
            };
            return Guid.Empty;
            //return await _categoryRepository.AddAsync(category).ConfigureAwait(false);

        }

        /// <summary>
        /// Updates requested <paramref name="categoryDTO"/> in database.
        /// </summary>
        /// <param name="categoryDTO"></param>
        /// <returns></returns>
        public async Task UpdateEntityAsync(CategoryDTO categoryDTO)
        {
            var toBeUpdatedCategory = await _categoryRepository.GetByIdAsync(categoryDTO.Id).ConfigureAwait(false);

            if (toBeUpdatedCategory == null)
                throw new MilvaUserFriendlyException("Veritabanında varolmayan bir kaydı güncellemeye çalışıyorsunuz.");

            toBeUpdatedCategory.Id = categoryDTO.Id;
            toBeUpdatedCategory.Name = categoryDTO.Name;
            toBeUpdatedCategory.LastModificationDate = DateTime.Now;

            await _categoryRepository.UpdateAsync(toBeUpdatedCategory).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes requested category whoose id equals to <paramref name="id"/> from database.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteEntityAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new MilvaUserFriendlyException("Veritabanında varolmayan bir kaydı silmeye çalışıyorsunuz.");

            var toBeDeletedCategory = await _categoryRepository.GetByIdAsync(id).ConfigureAwait(false);

            if (toBeDeletedCategory == null)
                throw new MilvaUserFriendlyException("Veritabanında varolmayan bir kaydı güncellemeye çalışıyorsunuz.");

            await _categoryRepository.DeleteAsync(toBeDeletedCategory).ConfigureAwait(false);
        }

        public Task MarkAsIsFavoriteAsync(Guid id, bool isFavorite)
        {
            throw new NotImplementedException();
        }
    }
}
