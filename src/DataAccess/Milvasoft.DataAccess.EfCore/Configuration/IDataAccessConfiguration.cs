namespace Milvasoft.DataAccess.EfCore.Configuration;

/// <summary>
/// IOptions aware custom MilvaDbContext configurations.
/// </summary>
public interface IDataAccessConfiguration : IMilvaOptions
{
    /// <summary>
    /// DbContext configuration.
    /// </summary>
    public DbContextConfiguration DbContext { get; set; }

    /// <summary>
    /// Audit configuration.
    /// </summary>
    public RepositoryConfiguration Repository { get; set; }

    /// <summary>
    /// Audit configuration.
    /// </summary>
    public AuditConfiguration Auditing { get; set; }
}
