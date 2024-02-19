namespace Milvasoft.Core.Abstractions;

/// <summary>
/// Dummy interface for options implementations.
/// </summary>
public interface IMilvaOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public static virtual string SectionName { get; }
}