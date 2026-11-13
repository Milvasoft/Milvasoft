namespace ExpressionBuilder.Common;

/// <summary>
/// Defines how a filter statement is joined with the statement that follows it.
/// </summary>
public enum Connector
{
    /// <summary>
    /// Both statements must be satisfied.
    /// </summary>
    And = 0,

    /// <summary>
    /// At least one of the statements must be satisfied.
    /// </summary>
    Or = 1
}
