using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;

namespace Milvasoft.BulkExtensions.Internal;

/// <summary>
/// How one column is read from, and written back to, entities of one entity type.
/// </summary>
internal sealed class ColumnBinding
{
    private readonly Lazy<Action<object, object>> _setter;
    private readonly ValueComparer _comparer;
    private Func<DbDataReader, int, object> _providerReader;

    public ColumnBinding(ColumnModel column, IProperty property, IPropertyBase[] hops, RelationalTypeMapping typeMapping, object discriminatorValue, bool isDiscriminator)
    {
        Column = column;
        Property = property;
        Hops = hops;
        HasComplexHop = hops.Any(h => h is IComplexProperty);
        Path = string.Join('.', hops.Select(h => h.Name).Append(property.Name));
        Converter = typeMapping.Converter;
        ProviderClrType = Unwrap(Converter?.ProviderClrType ?? typeMapping.ClrType);
        IsDiscriminator = isDiscriminator;
        DiscriminatorValue = discriminatorValue;
        IsShadow = property.IsShadowProperty();
        Getter = IsShadow ? null : property.GetGetter();
        HopGetters = [.. hops.Select(h => h.GetGetter())];
        BeforeSave = property.GetBeforeSaveBehavior();
        AfterSave = property.GetAfterSaveBehavior();
        IsGeneratedOnAdd = property.ValueGenerated is ValueGenerated.OnAdd or ValueGenerated.OnAddOrUpdate;
        IsGeneratedOnUpdate = property.ValueGenerated is ValueGenerated.OnUpdate or ValueGenerated.OnAddOrUpdate or ValueGenerated.OnUpdateSometimes;
        _comparer = property.GetValueComparer();
        RequiresValueGenerator = (IsGeneratedOnAdd && property.IsKey() && !property.IsForeignKey()) || property.GetValueGeneratorFactory() is not null;
        IsForeignKey = property.IsForeignKey();
        _setter = new Lazy<Action<object, object>>(() => CreateSetter(property));
    }

    public ColumnModel Column { get; }

    public IProperty Property { get; }

    /// <summary>
    /// Owned navigations and complex properties to follow from the entity to the object that declares <see cref="Property"/>.
    /// </summary>
    public IPropertyBase[] Hops { get; }

    public bool HasComplexHop { get; }

    /// <summary>
    /// Dotted property path from the entity, used to match names in <see cref="BulkConfig"/>.
    /// </summary>
    public string Path { get; }

    public ValueConverter Converter { get; }

    /// <summary>
    /// CLR type of the value sent to and read from the database.
    /// </summary>
    public Type ProviderClrType { get; }

    public bool IsDiscriminator { get; }

    /// <summary>
    /// Model value of the discriminator for the entity type of this binding.
    /// </summary>
    public object DiscriminatorValue { get; }

    public bool IsShadow { get; }

    public IClrPropertyGetter Getter { get; }

    public IClrPropertyGetter[] HopGetters { get; }

    public PropertySaveBehavior BeforeSave { get; }

    public PropertySaveBehavior AfterSave { get; }

    public bool IsGeneratedOnAdd { get; }

    public bool IsGeneratedOnUpdate { get; }

    /// <summary>
    /// Whether EF Core uses a value generator for this property when an entity is added.
    /// The generator decides whether the value is made on the client or by the database.
    /// </summary>
    public bool RequiresValueGenerator { get; }

    public bool IsForeignKey { get; }

    /// <summary>
    /// Sets the model value on the object that declares the property, or <see langword="null"/> when the property cannot be set from outside EF Core.
    /// </summary>
    public Action<object, object> Setter => _setter.Value;

    /// <summary>
    /// Whether <paramref name="modelValue"/> is the sentinel of the property, meaning "not set". Compared like EF Core does, with the value comparer.
    /// </summary>
    public bool IsSentinel(object modelValue) => _comparer.Equals(modelValue, Property.Sentinel);

    /// <summary>
    /// Reads a provider value of this column from <paramref name="reader"/>; <see langword="null"/> for database NULL.
    /// </summary>
    public object ReadProviderValue(DbDataReader reader, int ordinal)
        => reader.IsDBNull(ordinal) ? null : (_providerReader ??= TypedReader.Create(ProviderClrType))(reader, ordinal);

    public object ToProvider(object modelValue)
    {
        if (Converter is null)
            return modelValue;

        if (modelValue is null && !Converter.ConvertsNulls)
            return null;

        return Converter.ConvertToProvider(modelValue);
    }

    public object FromProvider(object providerValue)
    {
        if (Converter is null)
            return providerValue;

        if (providerValue is null && !Converter.ConvertsNulls)
            return null;

        return Converter.ConvertFromProvider(providerValue);
    }

    public override string ToString() => $"{Column.Name} <- {Path}";

    private static Type Unwrap(Type type) => Nullable.GetUnderlyingType(type) ?? type;

    private static Action<object, object> CreateSetter(IProperty property)
    {
        if (property.IsShadowProperty() || property.DeclaringType.ClrType.IsValueType)
            return null;

        var instance = Expression.Parameter(typeof(object), "instance");
        var value = Expression.Parameter(typeof(object), "value");
        var target = Expression.Convert(instance, property.DeclaringType.ClrType);

        Expression member;

        if (property.IsIndexerProperty() && property.PropertyInfo is { } indexer)
            member = Expression.MakeIndex(target, indexer, [Expression.Constant(property.Name)]);
        else if (property.FieldInfo is { } field && !field.IsInitOnly)
            member = Expression.Field(field.IsStatic ? null : target, field);
        else if (property.PropertyInfo is { SetMethod: not null } propertyInfo)
            member = Expression.Property(target, propertyInfo);
        else
            return null;

        var memberType = member.Type;

        Expression converted = memberType.IsValueType && Nullable.GetUnderlyingType(memberType) is null
            ? Expression.Condition(Expression.Equal(value, Expression.Constant(null)), Expression.Default(memberType), Expression.Convert(value, memberType))
            : Expression.Convert(value, memberType);

        var assign = Expression.Assign(member, converted);

        return Expression.Lambda<Action<object, object>>(assign, instance, value).Compile();
    }
}