namespace Milvasoft.Core.GeoLocation.Models;

/// <summary>
/// Location point.
/// </summary>
public class GeoPoint
{
    /// <summary>
    /// Latitude of point.
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Longitude of point.
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// Initialize new instance of <see cref="GeoPoint"/> with lat and lon.
    /// </summary>
    /// <param name="latitude"></param>
    /// <param name="longitude"></param>
    public GeoPoint(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }
}
