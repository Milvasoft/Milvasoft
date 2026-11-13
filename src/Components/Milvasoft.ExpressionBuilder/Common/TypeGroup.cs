namespace ExpressionBuilder.Common;

/// <summary>
/// Groups property types so callers can decide which operations a property supports.
/// </summary>
/// <remarks>
/// Types that are not listed in <see cref="Helpers.OperationHelper.TypeGroups"/> (enums, <see cref="Guid"/> and so on) fall into <see cref="Default"/>.
/// </remarks>
public enum TypeGroup
{
    /// <summary>
    /// Any type that is not listed in another group.
    /// </summary>
    Default = -1,

    /// <summary>
    /// <see cref="string"/> and <see cref="char"/>.
    /// </summary>
    Text = 1,

    /// <summary>
    /// Integral and floating point numbers.
    /// </summary>
    Number = 2,

    /// <summary>
    /// <see cref="bool"/>.
    /// </summary>
    Boolean = 4,

    /// <summary>
    /// <see cref="DateTime"/> and <see cref="DateTimeOffset"/>.
    /// </summary>
    Date = 8
}
