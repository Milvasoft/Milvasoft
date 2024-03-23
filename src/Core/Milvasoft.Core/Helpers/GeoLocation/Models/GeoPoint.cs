namespace Milvasoft.Core.Helpers.GeoLocation.Models;

/// <summary>
/// Represents a location point with latitude and longitude coordinates.
/// </summary>
/// <remarks>
/// Use this class to initialize a new instance of <see cref="GeoPoint"/> with latitude and longitude values.
/// </remarks>
/// <param name="latitude">The latitude coordinate of the point.</param>
/// <param name="longitude">The longitude coordinate of the point.</param>
public class GeoPoint(double latitude, double longitude)
{
    /// <summary>
    /// Gets or sets the latitude coordinate of the point.
    /// </summary>
    public double Latitude { get; set; } = latitude;

    /// <summary>
    /// Gets or sets the longitude coordinate of the point.
    /// </summary>
    public double Longitude { get; set; } = longitude;
}
