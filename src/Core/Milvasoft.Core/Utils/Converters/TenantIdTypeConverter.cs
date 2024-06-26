﻿using Milvasoft.Core.EntityBases.MultiTenancy;
using System.ComponentModel;
using System.Globalization;

namespace Milvasoft.Core.Utils.Converters;

/// <summary>
/// Provides a type converter to convert string objects to and from <see cref="TenantId"/>.
/// </summary>
public sealed class TenantIdTypeConverter : StringConverter
{
    /// <summary>
    /// Determines <paramref name="sourceType"/> can convert from string.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="sourceType"></param>
    /// <returns></returns>
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        if (sourceType == typeof(string))
            return true;

        return base.CanConvertFrom(context, sourceType);
    }

    /// <summary>
    /// Converts <paramref name="value"/>(<see cref="string"/>) to <see cref="TenantId"/>.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="culture"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        if (value is string stringValue)
            return TenantId.Parse(stringValue);

        return base.ConvertFrom(context, culture, value);
    }

    /// <summary>
    /// Determines <paramref name="destinationType"/> can convert to string.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="destinationType"></param>
    /// <returns></returns>
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    {
        if (destinationType == typeof(string))
            return true;

        return base.CanConvertTo(context, destinationType);
    }

    /// <summary>
    /// Converts the given value object to the specified type, using the specified context and culture information.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="culture"></param>
    /// <param name="value"></param>
    /// <param name="destinationType"></param>
    /// <returns></returns>
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        => destinationType == typeof(string) && value is TenantId tenantId
            ? tenantId.ToString()
            : base.ConvertTo(context, culture, value, destinationType);
}
