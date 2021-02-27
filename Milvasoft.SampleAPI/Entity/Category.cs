using Milvasoft.Helpers.DataAccess.Attributes;
using System.Collections.Generic;

namespace Milvasoft.SampleAPI.Entity
{
    public class Category : BaseEntity
    {
        [MilvaEncrypted]
        public string Name { get; set; }

        public List<Todo> Todos { get; set; }
    }
}
