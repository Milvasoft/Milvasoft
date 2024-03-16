using Milvasoft.Core.Extensions.GeoLocation.Models;

namespace Milvasoft.Core.Extensions.GeoLocation;

/// <summary>
/// Extension methods for geo location.
/// </summary>
public static class GeoLocationExtensions
{
    /// <summary>
    /// Calculates the distance in km from <paramref name="point1"/> to <paramref name="point2"/>.
    /// </summary>
    /// <param name="point1"></param>
    /// <param name="point2"></param>
    /// <returns></returns>
    public static double CalculateDistance(this GeoPoint point1, GeoPoint point2)
    {
        double tempLat1, tempLat2, tempLon1, tempLon2;

        // The math module contains a function named ToRadians which converts from degrees to radians.
        tempLat1 = ToRadians(point1.Latitude);
        tempLon1 = ToRadians(point1.Longitude);
        tempLat2 = ToRadians(point2.Latitude);
        tempLon2 = ToRadians(point2.Longitude);

        // Haversine formula
        double dlon = tempLon2 - tempLon1;
        double dlat = tempLat2 - tempLat1;

        double a = Math.Pow(Math.Sin(dlat / 2), 2) +
                   Math.Cos(tempLat1) * Math.Cos(tempLat2) *
                   Math.Pow(Math.Sin(dlon / 2), 2);

        double c = 2 * Math.Asin(Math.Sqrt(a));

        // Radius of earth in kilometers. 
        double radius = 6371;

        // Calculate the result
        return c * radius;

        // Angle in 10th of a degree
        static double ToRadians(double angleIn10thofaDegree) => angleIn10thofaDegree * Math.PI / 180;
    }
}
