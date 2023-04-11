using System;
using System.Runtime.Serialization;

namespace Webwonders.Extensions;

[DataContract(Name = "coordinate")]
public class Coordinate
{
    // DataMember names are the same as google coordinates
    [DataMember(Name = "lat")]
    public double Latitude { get; set; }

    [DataMember(Name = "lng")]
    public double Longitude { get; set; }


    public Coordinate()
    {
    }

    public Coordinate(Coordinate copyFrom)
    {
        Latitude = copyFrom.Latitude;
        Longitude = copyFrom.Longitude;
    }

    public Coordinate(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }


    /// <summary>
    /// Distance from this point to another point in meters
    /// </summary>
    /// <param name="toPoint"></param>
    /// <returns>distance to point in meters</returns>
    public double GetDistanceTo(Coordinate toPoint)
    {
        return GetDistance(Longitude, Latitude, toPoint.Longitude, toPoint.Latitude);
    }


    /// <summary>
    /// Code from .NET-frameworks GeoCoordinate class
    /// see: https://stackoverflow.com/questions/6366408/calculating-distance-between-two-latitude-and-longitude-geocoordinates
    /// because using the GeoCordinate class directly does not work
    /// </summary>
    /// <param name="longitude"></param>
    /// <param name="latitude"></param>
    /// <param name="otherLongitude"></param>
    /// <param name="otherLatitude"></param>
    /// <returns>Distance in meters</returns>
    private static double GetDistance(double longitude, double latitude, double otherLongitude, double otherLatitude)
    {
        var d1 = latitude * (Math.PI / 180.0);
        var num1 = longitude * (Math.PI / 180.0);
        var d2 = otherLatitude * (Math.PI / 180.0);
        var num2 = otherLongitude * (Math.PI / 180.0) - num1;
        var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);

        return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
    }
}



