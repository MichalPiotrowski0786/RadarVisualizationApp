using System.IO;
using UnityEngine;

public class DataReader
{
  public int rays;
  public int bins;

  public string path;
  float[] data;

  public DataReader(string path)
  {
    this.path = path;
    if (!File.Exists(this.path)) Debug.Log("File not found");

    Read();
  }

  private void Read()
  {
    string[] file = File.ReadAllLines(path);
    rays = file.Length;
    bins = file[0].Split(';').Length;

    int size = rays * bins;
    data = new float[size];
    int i = 0;
    foreach (string i_line in file)
    {
      string[] values = i_line.Split(';');
      foreach (string i_value in values)
      {
        string value = i_value.Replace('.', ',');
        if (value.Length > 0) data[i] = float.Parse(value);
        i++;
      }
    }
  }
  public float[] ReturnData()
  {
    if (data == null || data.Length == 0) return null;
    return data;
  }
}
