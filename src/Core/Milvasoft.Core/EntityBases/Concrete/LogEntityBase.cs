using System.Diagnostics;

namespace Milvasoft.Core.EntityBases.Concrete;

/// <summary>
/// Entity for logging parameters.
/// </summary>
/// <typeparam name="TKey"> Id property type.</typeparam>
public abstract class LogEntityBase<TKey> : EntityBase<TKey>
{
    /// <summary>
    /// Gets or sets the unique id for the transaction. This id is unique within the scope of the <see cref="Activity"/> if an activity interceptor is used, allowing multiple logs to have the same transaction id.
    /// </summary>
    public string TransactionId { get; set; }

    /// <summary>
    /// Gets or sets the namespace of the class where the method is located.
    /// </summary>
    public string Namespace { get; set; }

    /// <summary>
    /// Gets or sets the class name where the method is located.
    /// </summary>
    public string ClassName { get; set; }

    /// <summary>
    /// Gets or sets the intercepted method name.
    /// </summary>
    public string MethodName { get; set; }

    /// <summary>
    /// Gets or sets the intercepted method parameters as JSON (as an object array).
    /// </summary>
    public string MethodParams { get; set; }

    /// <summary>
    /// Gets or sets the method result as JSON.
    /// </summary>
    public string MethodResult { get; set; }

    /// <summary>
    /// Gets or sets the time spent running the method in milliseconds.
    /// </summary>
    public int ElapsedMs { get; set; }

    /// <summary>
    /// Gets or sets the method run time in UTC.
    /// </summary>
    public DateTime UtcLogTime { get; set; }

    /// <summary>
    /// Gets or sets the information about whether the method result came from the cache interceptor or not.
    /// </summary>
    public string CacheInfo { get; set; }

    /// <summary>
    /// Gets or sets the information about any exception thrown while the method is running, represented as JSON.
    /// </summary>
    public string Exception { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the method worked successfully or not.
    /// </summary>
    public bool IsSuccess { get; set; }
}
