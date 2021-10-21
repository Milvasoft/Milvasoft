using System;

namespace Milvasoft.SampleAPI.Utils.Swagger
{
    /// <summary>
    /// Excludes property from swagger documentation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class)]
    public class SwaggerExcludeAttribute : Attribute
    {
    }
}
