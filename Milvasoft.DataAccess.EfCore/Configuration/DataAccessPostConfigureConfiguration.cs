namespace Milvasoft.DataAccess.EfCore.Configuration;

/// <summary>
/// IOptions aware custom MilvaDbContext configurations.
/// </summary>
public class DataAccessPostConfigureConfiguration
{
    /// <summary>
    /// Current user id getter method for auditing operations.
    /// </summary>
    public Func<IServiceProvider, string> GetCurrentUserNameMethod { get; set; }

    /// <summary>
    /// Current language id getter method for multi language operations.
    /// </summary>
    public Func<IServiceProvider, object> GetCurrentLanguageIdMethod { get; set; }

    /// <summary>
    /// Current language id getter method for multi language operations.
    /// </summary>
    public Func<IServiceProvider, object> GetDefaultLanguageIdMethod { get; set; }
}