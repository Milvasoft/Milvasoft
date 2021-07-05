namespace Milvasoft.Helpers.MilvaTest.Integration.Enums
{
    /// <summary>
    /// A enum that determines how to create url information.
    /// </summary>
    public enum UrlTypeEnum
    {
        /// <summary>
        /// Used without processing incoming url information.
        /// </summary>
        Specific,

        /// <summary>
        /// Uses the name of the class in which the test method is used as a url.
        /// </summary>
        InController
    }
}
