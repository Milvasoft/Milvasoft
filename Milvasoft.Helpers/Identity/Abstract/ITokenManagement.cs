namespace Milvasoft.Helpers.Identity.Concrete
{
    /// <summary>
    /// Token management DTO for DI.
    /// </summary>
    public interface ITokenManagement
    {
        /// <summary>
        /// Gets or sets Secret key of token.
        /// </summary>
        public string Secret { get; set; }

        /// <summary>
        /// Gets or sets Authorization issuer.
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// Gets or sets Authorization audience.
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// Gets or sets Authorization issuer.
        /// </summary>
        public string LoginProvider { get; set; }

        /// <summary>
        /// Gets or sets Authorization audience.
        /// </summary>
        public string TokenName { get; set; }

        /// <summary>
        /// Gets or sets AccessExpiration of token.
        /// </summary>
        public int AccessExpiration { get; set; }

        /// <summary>
        /// Gets or sets RefreshExpiration of token.
        /// </summary>
        public int RefreshExpiration { get; set; }

    }
}
