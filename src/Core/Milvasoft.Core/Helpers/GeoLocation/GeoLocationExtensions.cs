﻿using Milvasoft.Core.Helpers.GeoLocation.Models;

namespace Milvasoft.Core.Helpers.GeoLocation;

/// <summary>
/// Contains extension methods for calculating distances between geographical points.
/// </summary>
public static class GeoLocationExtensions
{
    /// <summary>
    /// Calculates the distance in kilometers between two geographical points.
    /// </summary>
    /// <param name="point1">The first geographical point.</param>
    /// <param name="point2">The second geographical point.</param>
    /// <returns>The distance in kilometers between the two geographical points.</returns>
    public static double CalculateDistance(this GeoPoint point1, GeoPoint point2)
    {
        double tempLat1, tempLat2, tempLon1, tempLon2;

        // The math module contains a function named ToRadians which converts from degrees to radians.
        tempLat1 = ToRadians(point1.Latitude);
        tempLon1 = ToRadians(point1.Longitude);
        tempLat2 = ToRadians(point2.Latitude);
        tempLon2 = ToRadians(point2.Longitude);

        // Haversine formula
        var dlon = tempLon2 - tempLon1;
        var dlat = tempLat2 - tempLat1;

        var a = Math.Pow(Math.Sin(dlat / 2), 2) +
                   Math.Cos(tempLat1) * Math.Cos(tempLat2) *
                   Math.Pow(Math.Sin(dlon / 2), 2);

        var c = 2 * Math.Asin(Math.Sqrt(a));

        // Radius of earth in kilometers. 
        double radius = 6371;

        // Calculate the result
        return c * radius;

        // Angle in 10th of a degree
        static double ToRadians(double angleIn10thofaDegree) => angleIn10thofaDegree * Math.PI / 180;
    }
}
