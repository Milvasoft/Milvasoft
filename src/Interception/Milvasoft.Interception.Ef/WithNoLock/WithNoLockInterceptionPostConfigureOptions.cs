namespace Milvasoft.Interception.Ef.WithNoLock;

/// <summary>
/// If options are made from the configuration file, the class that allows options that cannot be made from the configuration file.
/// </summary>
public class WithNoLockInterceptionPostConfigureOptions
{
    /// <summary>
    /// DbContext type. For example typeof(SomeContext)
    /// </summary>
    public Type DbContextType { get; set; }
}