namespace Milvasoft.Components.Rest.Enums;

/// <summary>
/// Represents the types of filtering operations supported by the application.
/// </summary>
public enum FilterType
{
    /// <summary>
    /// Indicates equality comparison.
    /// </summary>
    Equal,

    /// <summary>
    /// Indicates inequality comparison.
    /// </summary>
    NotEqual,

    /// <summary>
    /// Indicates greater-than comparison.
    /// </summary>
    Greater,

    /// <summary>
    /// Indicates greater-than-or-equal-to comparison.
    /// </summary>
    GreaterEqual,

    /// <summary>
    /// Indicates less-than comparison.
    /// </summary>
    Less,

    /// <summary>
    /// Indicates less-than-or-equal-to comparison.
    /// </summary>
    LessEqual,

    /// <summary>
    /// Indicates partial match containing the specified value.
    /// </summary>
    Contains,

    /// <summary>
    /// Indicates partial match not containing the specified value.
    /// </summary>
    NotContains,

    /// <summary>
    /// Indicates partial match starting with the specified value.
    /// </summary>
    StartsWith,

    /// <summary>
    /// Indicates partial match ending with the specified value.
    /// </summary>
    EndsWith
}

