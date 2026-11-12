namespace Milvasoft.Interception.Interceptors.Logging;

/// <summary>
/// Marks a property or a method parameter whose value must be masked when the owning method's arguments or return value
/// are serialized by <see cref="LogInterceptor"/>. The real value is never written to the log; the
/// configured <see cref="Mask"/> placeholder is written instead (null values stay null).
/// </summary>
/// <remarks>
/// <para>Masking only affects the JSON produced by <see cref="LogInterceptor"/>; it does not change any other
/// serialization of the type (persistence, API responses, …). Properties of any type are masked, and the attribute
/// is honored on overriding properties when it is declared on the base property.</para>
/// <para>Values from <see cref="ILogInterceptionOptions.ExtraLoggingPropertiesSelector"/> are serialized with the same rules.
/// Exception messages are logged as they are and are not masked.</para>
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class LogMaskedAttribute : Attribute
{
    /// <summary>
    /// The placeholder written to the log in place of the real value. Default is "***".
    /// </summary>
    public string Mask { get; set; } = "***";
}
