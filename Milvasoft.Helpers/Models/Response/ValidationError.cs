using Newtonsoft.Json;
using System.Collections.Generic;

namespace Milvasoft.Helpers.Models.Response
{
    /// <summary>
    /// Validation error model for property.
    /// </summary>
    public class ValidationError
    {
        /// <value>
        /// Property where validation error occurred.
        /// </value>
        [JsonProperty("validationFieldName")]
        public string ValidationFieldName { get; set; }

        /// <value>
        /// Property's validation error messages.
        /// </value>
        [JsonProperty("errorMessageList")]
        public List<string> ErrorMessageList { get; set; }
    }
}
