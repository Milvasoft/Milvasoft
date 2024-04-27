namespace Milvasoft.Types.Structs;

/// <summary>
/// Represents a generic updatable property. 
/// </summary>
/// <typeparam name="T">The type of the property value.</typeparam>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Bug", "S2328:\"GetHashCode\" should not reference mutable fields", Justification = "<Pending>")]
public struct UpdateProperty<T> : IUpdateProperty, IEquatable<UpdateProperty<T>>
{
    private T _value;
    private bool _isUpdated = false;

    /// <summary>
    /// Gets or sets the value of the property.
    /// </summary>
    public T Value
    {
        readonly get => _value;
        set
        {
            _value = value;
            _isUpdated = true;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the property has been updated.
    /// </summary>
    public bool IsUpdated { readonly get => _isUpdated; set => _isUpdated = value; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateProperty{T}"/> struct.
    /// </summary>
    public UpdateProperty()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateProperty{T}"/> struct with the specified value.
    /// </summary>
    /// <param name="value">The initial value of the property.</param>
    public UpdateProperty(T value)
    {
        _value = value;
        _isUpdated = true;
    }

    /// <summary>
    /// Gets the value of the property as an object.
    /// </summary>
    /// <returns>The value of the property.</returns>
    public readonly object GetValue() => _value;

    /// <summary>
    /// Combines Tenancy Name and BranchNo into a hash code.
    /// </summary>
    /// <returns></returns>
    public override readonly int GetHashCode() => HashCode.Combine(_value, _isUpdated);

    /// <summary>
    /// Compares two <see cref="UpdateProperty{T}"/> objects for equality.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns> If both hash are equals returns true, othewise false. </returns>
    public override readonly bool Equals(object obj)
    {
        UpdateProperty<T> otherUpdateProp;

        // Check that other is a UpdateProperty<T> first
        if (obj is not UpdateProperty<T> prop)
            return false;
        else
            otherUpdateProp = prop;

        if (otherUpdateProp != this)
            return false;

        return true;
    }

    /// <summary>
    /// Compares two <see cref="UpdateProperty{T}"/> objects for equality.
    /// </summary>
    /// <param name="other"></param>
    /// <returns> If both hash are equals returns true, othewise false. </returns>
    public readonly bool Equals(UpdateProperty<T> other) => _value?.Equals(other._value) ?? false;

    /// <summary>
    /// Implicitly converts an <see cref="UpdateProperty{T}"/> to its underlying value.
    /// </summary>
    /// <param name="s">The <see cref="UpdateProperty{T}"/> to convert.</param>
    public static implicit operator T(UpdateProperty<T> s) => s._value;

    /// <summary>
    /// Implicitly converts a value of type <typeparamref name="T"/> to an <see cref="UpdateProperty{T}"/>.
    /// </summary>
    /// <param name="s">The value to convert.</param>
    public static implicit operator UpdateProperty<T>(T s) => new(s);

    /// <summary>
    /// Compares two <see cref="UpdateProperty{T}"/> objects for equality.
    /// </summary>
    /// <param name="a">The first <see cref="UpdateProperty{T}"/> to compare.</param>
    /// <param name="b">The second <see cref="UpdateProperty{T}"/> to compare.</param>
    /// <returns>True if the two <see cref="UpdateProperty{T}"/> objects are equal; otherwise, false.</returns>
    public static bool operator ==(UpdateProperty<T> a, UpdateProperty<T> b)
    {
        if (a._value == null && b._value == null)
            return true;

        return a._value?.Equals(b._value) ?? false;
    }

    /// <summary>
    /// Compares two <see cref="UpdateProperty{T}"/> objects for equality.
    /// </summary>
    /// <param name="a">The first <see cref="UpdateProperty{T}"/> to compare.</param>
    /// <param name="b">The second <see cref="UpdateProperty{T}"/> to compare.</param>
    /// <returns>True if the two <see cref="UpdateProperty{T}"/> objects are equal; otherwise, false.</returns>
    public static bool operator !=(UpdateProperty<T> a, UpdateProperty<T> b) => !(a == b);

    /// <summary>
    /// Returns a string that represents the current <see cref="UpdateProperty{T}"/> object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override readonly string ToString() => $"Value : {_value} / Is Updated : {_isUpdated}";
}

/// <summary>
/// Represents an updatable property.
/// </summary>
public interface IUpdateProperty
{
    /// <summary>
    /// Gets a value indicating whether the property has been updated.
    /// </summary>
    bool IsUpdated { get; }

    /// <summary>
    /// Gets the value of the property as an object.
    /// </summary>
    /// <returns>The value of the property.</returns>
    object GetValue();
}
