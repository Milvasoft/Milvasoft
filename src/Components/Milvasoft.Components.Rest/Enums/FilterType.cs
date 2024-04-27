namespace Milvasoft.Components.Rest.Enums;

/// <summary>
/// Represents the types of filtering operations supported by the application.
/// </summary>
public enum FilterType
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    Between,
    Contains,
    DoesNotContain,
    StartsWith,
    EndsWith,
    EqualTo,
    NotEqualTo,
    GreaterThan,
    GreaterThanOrEqualTo,
    LessThan,
    LessThanOrEqualTo,
    IsEmpty,
    IsNotEmpty,
    IsNull,
    IsNotNull,
    IsNullOrWhiteSpace,
    IsNotNullNorWhiteSpace,
    In,
    NotIn
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

