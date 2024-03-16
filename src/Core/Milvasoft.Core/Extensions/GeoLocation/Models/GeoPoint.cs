namespace Milvasoft.Core.Extensions.GeoLocation.Models;

/// <summary>
/// Location point.
/// </summary>
/// <remarks>
/// Initialize new instance of <see cref="GeoPoint"/> with lat and lon.
/// </remarks>
/// <param name="latitude"></param>
/// <param name="longitude"></param>
public class GeoPoint(double latitude, double longitude)
{
    /// <summary>
    /// Latitude of point.
    /// </summary>
    public double Latitude { get; set; } = latitude;

    /// <summary>
    /// Longitude of point.
    /// </summary>
    public double Longitude { get; set; } = longitude;
}
