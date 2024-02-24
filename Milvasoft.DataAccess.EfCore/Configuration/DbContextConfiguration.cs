using Milvasoft.DataAccess.EfCore.Utils.Enums;

namespace Milvasoft.DataAccess.EfCore.Configuration;

/// <summary>
/// IOptions aware custom MilvaDbContext configurations.
/// </summary>
public class DbContextConfiguration
{
    /// <summary>
    /// If it is true converts all saved DateTimes to UTC. Default value is false.
    /// </summary>
    public bool UseUtcForDateTimes { get; set; } = false;

    /// <summary>
    /// Soft delete state when delete operation. Default value is false.
    /// </summary>
    public SoftDeletionState DefaultSoftDeletionState { get; set; } = SoftDeletionState.Passive;

    /// <summary>
    /// Current user id getter delegate for auditing operations.
    /// </summary>
    public Func<string> GetCurrentUserNameDelegate { get; set; }
}