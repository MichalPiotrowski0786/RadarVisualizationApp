using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Ionic.Zlib;

public class DecodeData
{
  public string debug = "";
  public int len = 4;

  public Scan scan;
  string file;
  string xml;
  string blobs;

  private const int EIGHT_BYTE_DIVIDER = 256;
  private const int SIXTEEN_BYTE_DIVIDER = 65536;

  public DecodeData(string file)
  {
    this.file = file;
    PrepareFile();

    Run();
  }

  void Run()
  {
    scan = new Scan();
    var xmlDoc = ReadXmlFile();
    string scanXpath = $"volume/scan";
    string sensorinfoXpath = $"volume/sensorinfo";

    string scanTime = xmlDoc.SelectSingleNode($"{scanXpath}/@time").Value;
    string scanDate = xmlDoc.SelectSingleNode($"{scanXpath}/@date").Value;
    string siteName = xmlDoc.SelectSingleNode($"{sensorinfoXpath}/@name").Value;
    float siteLat = float.Parse(xmlDoc.SelectSingleNode($"{sensorinfoXpath}/lat").InnerText.Replace('.', ','));
    float siteLon = float.Parse(xmlDoc.SelectSingleNode($"{sensorinfoXpath}/lon").InnerText.Replace('.', ','));
    scan.GetGenericInfo(scanTime, scanDate, siteName, siteLat, siteLon);

    for (int i = 0; i < len; i++)
    {
      string sliceXpath = $"{scanXpath}/slice[{i + 1}]";
      string elev = xmlDoc.SelectSingleNode($"{sliceXpath}/posangle").InnerText;

      string rayinfoXpath = $"{sliceXpath}/slicedata/rayinfo";
      string rawdataXpath = $"{sliceXpath}/slicedata/rawdata";

      int rays = int.Parse(xmlDoc.SelectSingleNode($"{rawdataXpath}/@rays").Value);
      int bins = int.Parse(xmlDoc.SelectSingleNode($"{rawdataXpath}/@bins").Value);

      int rayinfoBlobid = int.Parse(xmlDoc.SelectSingleNode($"{rayinfoXpath}/@blobid").Value);
      int rayinfoDepth = int.Parse(xmlDoc.SelectSingleNode($"{rayinfoXpath}/@depth").Value);

      int rawdataBlobid = int.Parse(xmlDoc.SelectSingleNode($"{rawdataXpath}/@blobid").Value);
      int rawdataDepth = int.Parse(xmlDoc.SelectSingleNode($"{rawdataXpath}/@depth").Value);
      float rawdataMin = float.Parse(xmlDoc.SelectSingleNode($"{rawdataXpath}/@min").Value.Replace('.', ','));
      float rawdataMax = float.Parse(xmlDoc.SelectSingleNode($"{rawdataXpath}/@max").Value.Replace('.', ','));

      float angle = DecodeBlob(rayinfoBlobid, rayinfoDepth)[0];
      angle = NormalizeAngle(angle, rays, rayinfoDepth);
      float[] rawdata = DecodeBlob(rawdataBlobid, rawdataDepth);
      rawdata = NormalizeRawdata(rawdata, rawdataDepth, rawdataMin, rawdataMax);

      Slice slice = new Slice(rawdata, angle, rays, bins, elev, rawdataMin, rawdataMax);
      scan.AddSlice(slice);
    }
  }

  float[] DecodeBlob(int blobid, int depth)
  {
    Encoding latinEncoding = Encoding.GetEncoding("ISO-8859-1");
    byte[] compressedData = latinEncoding.GetBytes(ReadBlob(blobid));
    byte[] buffer = new byte[compressedData.Length - 4];
    for (int i = 0; i < buffer.Length; i++) buffer[i] = compressedData[i + 4];

    MemoryStream memStream = new MemoryStream(buffer);
    ZlibStream zlibStream = new ZlibStream(memStream, CompressionMode.Decompress);
    MemoryStream endStream = new MemoryStream();
    zlibStream.CopyTo(endStream);

    byte[] byte_data = endStream.ToArray();
    uint value;
    int n = depth / 8;
    int size = byte_data.Length;
    float[] output = new float[size / n];

    for (int i = 0; i < size; i += n)
    {
      value = byte_data[i];
      for (int j = 0; j < n - 1; j++)
      {
        value <<= 8;
        value |= byte_data[i + 1];
      }

      output[i / n] = value;
    }
    return output;
  }

  string ReadBlob(int blobid)
  {
    string headerSearchString = $"<BLOB blobid=\"{blobid}\"";

    int headerStart = blobs.IndexOf(headerSearchString);
    int headerEnd = blobs.IndexOf(">", headerStart);

    int blobDataStart = headerEnd + 2;
    int blobDataEnd = blobs.IndexOf("</BLOB>", blobDataStart);
    int blobDataLen = blobDataEnd - blobDataStart - 1;

    string output = blobs.Substring(blobDataStart, blobDataLen);
    return output;
  }

  XmlDocument ReadXmlFile()
  {
    XmlDocument doc = new XmlDocument();
    doc.LoadXml(xml);
    return doc;
  }

  void PrepareFile()
  {
    string stringToSearch = "<!-- END XML -->";

    int endInfoIndex = file.IndexOf(stringToSearch);
    int startBlobData = endInfoIndex + stringToSearch.Length;

    xml = file.Substring(0, endInfoIndex);
    blobs = file.Substring(startBlobData);
  }

  float[] NormalizeRawdata(float[] rawdata, int depth, float min, float max)
  {
    int divider = (depth == 8) ? EIGHT_BYTE_DIVIDER : SIXTEEN_BYTE_DIVIDER;

    for (int i = 0; i < rawdata.Length; i++)
    {
      float value = rawdata[i];
      value = (float)(min + value * (max - min) / divider);
      rawdata[i] = value;
    }

    return rawdata;
  }

  float NormalizeAngle(float angle, float rays, int depth)
  {
    int divider = (depth == 8) ? EIGHT_BYTE_DIVIDER : SIXTEEN_BYTE_DIVIDER;
    return (float)(angle * rays / divider);
  }
}


public class Scan
{
  List<Slice> slices = new List<Slice>();
  public string time;
  public string date;
  public string name;
  public float lat;
  public float lon;

  public void AddSlice(Slice slice) { slices.Add(slice); }
  public void GetGenericInfo(string time, string date, string name, float lat, float lon)
  {
    this.time = time;
    this.date = date;
    this.name = name;
    this.lat = lat;
    this.lon = lon;
  }
  public Slice[] ReturnSliceArr() { return slices.ToArray(); }
}

public class Slice
{
  public float[] data;
  public float angle;
  public int rays;
  public int bins;
  public string elevation;
  public float min;
  public float max;

  public Slice(float[] data, float angle, int rays, int bins, string elevation, float min, float max)
  {
    this.data = data;
    this.angle = angle;
    this.rays = rays;
    this.bins = bins;
    this.elevation = elevation;
    this.min = min;
    this.max = max;
  }
}
