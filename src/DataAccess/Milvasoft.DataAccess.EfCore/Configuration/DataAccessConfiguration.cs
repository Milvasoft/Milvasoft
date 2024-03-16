namespace Milvasoft.DataAccess.EfCore.Configuration;

/// <summary>
/// IOptions aware custom MilvaDbContext configurations.
/// </summary>
public class DataAccessConfiguration : IDataAccessConfiguration
{
    /// <summary>
    /// Configuration section path in configuration file.
    /// </summary>
    public static string SectionName { get; } = $"{MilvaOptionsExtensions.ParentSectionName}:DataAccess";

    /// <summary>
    /// DbContext configuration.
    /// </summary>
    public DbContextConfiguration DbContext { get; set; } = new();

    /// <summary>
    /// Audit configuration.
    /// </summary>
    public RepositoryConfiguration Repository { get; set; } = new();

    /// <summary>
    /// Audit configuration. Default is auditing only dates.
    /// </summary>
    public AuditConfiguration Auditing { get; set; } = new();
}