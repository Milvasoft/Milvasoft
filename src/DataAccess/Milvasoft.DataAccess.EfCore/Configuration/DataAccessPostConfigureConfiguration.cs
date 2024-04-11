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
}