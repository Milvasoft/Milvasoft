namespace Milvasoft.Core.Helpers.GeoLocation.Models;

/// <summary>
/// Represents a location point with latitude and longitude coordinates.
/// </summary>
/// <remarks>
/// Use this class to initialize a new instance of <see cref="GeoPoint"/> with latitude and longitude values.
/// </remarks>
public class GeoPoint
{
    /// <summary>
    /// Gets or sets the latitude coordinate of the point.
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude coordinate of the point.
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GeoPoint"/> class.
    /// </summary>
    public GeoPoint() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="GeoPoint"/> class with specified latitude and longitude.
    /// </summary>
    /// <param name="latitude"></param>
    /// <param name="longitude"></param>
    public GeoPoint(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }
}
