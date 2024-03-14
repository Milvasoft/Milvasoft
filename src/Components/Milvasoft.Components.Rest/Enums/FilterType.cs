namespace Milvasoft.Components.Rest.Enums;

/// <summary>
/// Represents the types of filtering operations supported by the application.
/// </summary>
public enum FilterType
{
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
}

