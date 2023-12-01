using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Milvasoft.Core.Abstractions;
public interface ILocalizationOptions
{
    /// <summary>
    /// Localization manager service lifetime.
    /// </summary>
    public ServiceLifetime ManagerLifetime { get; set; }

    /// <summary>
    /// Key format
    /// </summary>
    public string KeyFormat { get; set; }

    /// <summary>
    /// Fortmatted key creator delegate.
    /// </summary>
    public Func<string, string> KeyFormatDelegate { get; set; }
}
