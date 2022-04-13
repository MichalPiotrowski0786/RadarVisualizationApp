using System;
using System.IO;
using UnityEngine;

public class GetCitiesScript
{
    public double d = 0;
    public City[] cities;

    public GetCitiesScript()
    {
        ReadFile();
    }

    void ReadFile()
    {
        string fileName = @$"{Application.dataPath}/Front/UI/mapa_miast.txt";
        string[] lines = File.ReadAllLines(fileName);
        cities = new City[lines.Length];
        int index = 0;

        foreach (string line in lines)
        {
            string[] line_arr = line.Split(',');

            string city_name = line_arr[0];
            float city_lat = float.Parse(line_arr[2].Replace('.', ','));
            float city_lon = float.Parse(line_arr[1].Replace('.', ','));

            cities[index] = new City(city_name, city_lat, city_lon);
            index++;
        }
    }

    public double GetDistance(double longitude, double latitude, double otherLongitude, double otherLatitude)
    {
        var d1 = latitude * (Math.PI / 180.0);
        var num1 = longitude * (Math.PI / 180.0);
        var d2 = otherLatitude * (Math.PI / 180.0);
        var num2 = otherLongitude * (Math.PI / 180.0) - num1;
        var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);

        return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
    }
}

public class City
{
    public string name;
    public float lat, lon;

    public City(string name, float lat, float lon)
    {
        this.name = name;
        this.lat = lat; this.lon = lon;
    }

}
