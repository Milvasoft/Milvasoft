using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.MultiTenancy.EntityBase
{
    public sealed class TenantIdStringConverter : ValueConverter<TenantId, string>
    {
        /// <summary>
        /// Creates a new <see cref="TenantIdStringConverter"/> instance.
        /// </summary>
        /// <param name="mappingHints">Entity Framework mapping hints</param>
        public TenantIdStringConverter(ConverterMappingHints mappingHints = null)
            : base(to => to.ToString(), from => TenantId.Parse(from), mappingHints)
        {
        }
    }
}
