
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;

namespace Milvasoft.API.DTOs
{
    /// <summary>
    /// Paginatination params
    /// </summary>
    public class PaginationParams
    {
        /// <summary>
        /// Requested page number.
        /// </summary>
        [OValidateDecimal(0, LocalizerKey = "InvalidPageIndexMessage", FullMessage = true)]
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// Requested item count in requested page.
        /// </summary>
        [OValidateDecimal(0, LocalizerKey = "InvalidRequestedItemCountMessage", FullMessage = true)]
        public int RequestedItemCount { get; set; } = 20;

        /// <summary>
        /// If order by column requested then Property name of entity.
        /// </summary>
        [OValidateString(0, 200)]
        public string OrderByProperty { get; set; } = "";

        /// <summary>
        /// If order by column requested then ascending or descending.
        /// </summary>
        public bool OrderByAscending { get; set; } = false;
    }
}
