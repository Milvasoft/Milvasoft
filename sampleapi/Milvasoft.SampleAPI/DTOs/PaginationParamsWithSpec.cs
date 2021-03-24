namespace Milvasoft.API.DTOs
{
    /// <summary>
    /// Pagination parameters with specification object(<typeparamref name="TSpec"/>).
    /// </summary>
    /// <typeparam name="TSpec"></typeparam>
    public class PaginationParamsWithSpec<TSpec> : PaginationParams
    {
        /// <summary>
        /// Specification object.
        /// </summary>
        public TSpec Spec { get; set; } = default(TSpec);
    }
}
