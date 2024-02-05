﻿using Milvasoft.Attributes.Annotations;

namespace Milvasoft.Components.Rest.Response;

/// <summary>
/// Response metadata for frontend apps.
/// </summary>
public class ResponseDataMetadata
{
    //TODO Tree data https://mui.com/x/react-data-grid/tree-data/

    /// <summary>
    /// Column or property name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Column or property localized name.
    /// </summary>
    public string LocalizedName { get; set; }

    /// <summary>
    /// Column or property type.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Determines whether column or property will displayed or not.
    /// </summary>
    public bool Display { get; set; } = true;

    /// <summary>
    /// Determines whether column or property will pinned or not.
    /// </summary>
    public bool Pinned { get; set; }

    /// <summary>
    /// Determines whether the column or property will be masked or not.
    /// </summary>
    public bool Mask { get; set; }

    /// <summary>
    /// Determines whether the column will be filtered and ordered on the frontend.
    /// </summary>
    public bool Filterable { get; set; }

    /// <summary>
    /// Default value if column or property data is null.
    /// </summary>
    public object DefaultValue { get; set; } = default;

    /// <summary>
    /// If the column is of decimal type, it determines how many digits there will be after the comma.
    /// </summary>
    public DecimalPrecision DecimalPrecision { get; set; }

    /// <summary>
    /// It determines the format in which data in the table will display. Example : "{{Code}} - {{Name}}"
    /// </summary>
    public string CellDisplayFormat { get; set; }

    /// <summary>
    /// It determines what the tooltip format of a data in the table will be.
    /// </summary>
    public string CellTooltipFormat { get; set; }
}