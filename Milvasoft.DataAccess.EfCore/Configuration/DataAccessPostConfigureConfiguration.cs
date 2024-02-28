namespace Milvasoft.DataAccess.EfCore.Configuration;

/// <summary>
/// IOptions aware custom MilvaDbContext configurations.
/// </summary>
public class DataAccessPostConfigureConfiguration
{
    /// <summary>
    /// Current user id getter delegate for auditing operations.
    /// </summary>
    public Func<string> GetCurrentUserNameDelegate { get; set; }
}