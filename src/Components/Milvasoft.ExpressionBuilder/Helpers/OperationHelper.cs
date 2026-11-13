using ExpressionBuilder.Common;
using System.Collections.ObjectModel;

namespace ExpressionBuilder.Helpers;

/// <summary>
/// Type information used to decide which operations a property supports.
/// </summary>
public static class OperationHelper
{
    /// <summary>
    /// Types of each <see cref="TypeGroup"/>, in a stable order. Nullable types belong to the group of their underlying type.
    /// </summary>
    public static IReadOnlyDictionary<TypeGroup, IReadOnlySet<Type>> TypeGroups { get; } = new ReadOnlyDictionary<TypeGroup, IReadOnlySet<Type>>(new Dictionary<TypeGroup, IReadOnlySet<Type>>
    {
        [TypeGroup.Text] = new HashSet<Type> { typeof(string), typeof(char) },
        [TypeGroup.Number] = new HashSet<Type>
        {
            typeof(int), typeof(uint), typeof(byte), typeof(sbyte), typeof(short), typeof(ushort),
            typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)
        },
        [TypeGroup.Boolean] = new HashSet<Type> { typeof(bool) },
        [TypeGroup.Date] = new HashSet<Type> { typeof(DateTime), typeof(DateTimeOffset) },
    });

    /// <summary>
    /// Gets the type group of <paramref name="type"/>. Nullable types use their underlying type.
    /// </summary>
    /// <param name="type">Property type.</param>
    /// <returns>The group that contains the type, or <see cref="TypeGroup.Default"/>.</returns>
    public static TypeGroup GetTypeGroup(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        type = Nullable.GetUnderlyingType(type) ?? type;

        foreach (var (group, types) in TypeGroups)
            if (types.Contains(type))
                return group;

        return TypeGroup.Default;
    }
}
