using Milvasoft.SampleAPI.Entity;
using System.Collections.Generic;

namespace Milvasoft.SampleAPI.DTOs
{
    public class CategoryDTO : BaseEntity
    {
        public string Name { get; set; }
        public int TodoCount { get; set; }

        public List<TodoDTO> Todos { get; set; }
    }
}
