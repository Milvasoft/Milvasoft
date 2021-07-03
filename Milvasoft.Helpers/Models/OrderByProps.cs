namespace Milvasoft.Helpers.Models
{
    /// <summary>
    /// Order by properties for multiple ordey by in database.
    /// </summary>
    public class OrderByProps
    {
        /// <summary>
        /// Determines order by Property name of entity.
        /// </summary>
        public string PropName { get; set; }

        /// <summary>
        /// Determines order by ascending or descending.
        /// </summary>
        public bool Ascending { get; set; }
    }
}
