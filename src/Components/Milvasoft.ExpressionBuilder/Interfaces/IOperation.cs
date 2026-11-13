namespace ExpressionBuilder.Interfaces;

/// <summary>
/// A filter operation such as <see cref="Operations.EqualTo"/> or <see cref="Operations.Between"/>.
/// </summary>
/// <remarks>
/// Only the operations in the <see cref="Operations"/> namespace can be passed to <see cref="Generics.Filter{TClass}"/>.
/// </remarks>
public interface IOperation
{
    /// <summary>
    /// Name of the operation.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Number of values the operation takes: 0, 1 or 2.
    /// </summary>
    int NumberOfValues { get; }
}
