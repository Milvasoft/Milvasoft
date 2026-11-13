namespace Milvasoft.DataAccess.EfCore.Configuration;

/// <summary>
/// IOptions aware custom MilvaDbContext configurations.
/// </summary>
public class DbContextConfiguration
{
    /// <summary>
    /// If it is true, <see cref="DateTime"/> and <see cref="DateTimeOffset"/> values are stored in UTC (see <see cref="DbContextBase.ModelBuilderExtensions.UseUtcDateTime"/>)
    /// and audit dates are set with <see cref="DateTime.UtcNow"/>. Default value is true.
    /// </summary>
    /// <remarks>
    /// In UTC mode <see cref="DateTimeKind.Unspecified"/> values are treated as UTC, <see cref="DateTimeKind.Local"/> values are converted to UTC.
    /// Set it to false to keep local times (e.g. for <c>timestamp without time zone</c> columns); then dates are not converted and audit dates use <see cref="DateTime.Now"/>.
    /// </remarks>
    public bool UseUtcForDateTime { get; set; } = true;

    /// <summary>
    /// Soft delete state when delete operation. Default value is false.
    /// </summary>
    public SoftDeletionState DefaultSoftDeletionState { get; set; } = SoftDeletionState.Passive;

    /// <summary>
    /// Soft delete state reset after every operation. Default value is true.
    /// </summary>
    public bool ResetSoftDeleteStateAfterEveryOperation { get; set; } = true;

    /// <summary>
    /// Current user id getter method for auditing operations.
    /// </summary>
    public Func<IServiceProvider, string> GetCurrentUserNameMethod { get; set; }

    /// <summary>
    /// Dynamic data fetch configuration.
    /// </summary>
    public DynamicFetchConfiguration DynamicFetch { get; set; } = new();

    /// <summary>
    /// Invokes <see cref="GetCurrentUserNameMethod"/>.
    /// </summary>
    /// <param name="sp"></param>
    /// <returns></returns>
    public string InvokeGetCurrentUserMethod(IServiceProvider sp) => GetCurrentUserNameMethod?.Invoke(sp) ?? null;
}