﻿using System.Collections.Generic;
using Milvasoft.Components.Rest.Enums;

namespace Milvasoft.Components.Rest.Request;

/// <summary>
/// Represents criteria used for filtering data.
/// </summary>
public class FilterCriteria
{
    /// <summary>
    /// Gets or sets the name of the column on which the filter is applied.
    /// </summary>
    public string ColumnName { get; set; }

    /// <summary>
    /// Gets or sets the value to filter by.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Gets or sets the type of filter operation to apply.
    /// </summary>
    public FilterType FilterType { get; set; }
}
