namespace Milvasoft.Interception.Interceptors.Logging;

/// <summary>
/// Marks a property whose value must be masked when the owning method's arguments or return value
/// are serialized by <see cref="LogInterceptor"/>. The real value is never written to the log; the
/// configured <see cref="Mask"/> placeholder is written instead.
/// </summary>
/// <remarks>
/// Masking only affects the JSON produced by <see cref="LogInterceptor"/>; it does not change any other
/// serialization of the type (persistence, API responses, …). Only string properties are masked.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class LogMaskedAttribute : Attribute
{
    /// <summary>
    /// The placeholder written to the log in place of the real value. Default is "***".
    /// </summary>
    public string Mask { get; set; } = "***";
}
