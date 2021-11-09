using System.Xml;
using System.IO;
using System.Text;
using Ionic.Zlib;
using System.Collections.Generic;
using System.Linq;
using System;

public class DecodeData
{
  string file;
  public string info = "";
  public string data = "";
  public string snippet = "";

  public float min;
  public float max;
  public int rays;
  public int bins;
  public float angle;

  private const int EIGHT_BYTE_DIVIDER = 256;
  private const int SIXTEEN_BYTE_DIVIDER = 65536;

  public float[] values;

  public DecodeData(string file)
  {
    this.file = file;
    PrepareString();
    Decode();
  }

  void Decode()
  {
    var xmlFile = ReadXmlFile();
    string BlobXpath = $"volume/scan/slice[1]/slicedata/";

    int z_blobid = int.Parse(xmlFile.SelectSingleNode(BlobXpath + "rawdata/@blobid").Value);
    int z_depth = int.Parse(xmlFile.SelectSingleNode(BlobXpath + "rawdata/@depth").Value);
    int azi_blobid = int.Parse(xmlFile.SelectSingleNode(BlobXpath + "rayinfo/@blobid").Value);
    int azi_depth = int.Parse(xmlFile.SelectSingleNode(BlobXpath + "rayinfo/@depth").Value);

    rays = int.Parse(xmlFile.SelectSingleNode(BlobXpath + "rayinfo/@rays").Value);
    bins = int.Parse(xmlFile.SelectSingleNode(BlobXpath + "rawdata/@bins").Value);
    min = float.Parse(xmlFile.SelectSingleNode(BlobXpath + "rawdata/@min").Value.Replace('.', ','));
    max = float.Parse(xmlFile.SelectSingleNode(BlobXpath + "rawdata/@max").Value.Replace('.', ','));

    angle = DecompressData(azi_blobid, azi_depth)[0];
    angle = NormalizeAZI(azi_depth);

    values = DecompressData(z_blobid, z_depth);
    NormalizeZ(z_depth, min, max);
  }

  void NormalizeZ(int depth, float min, float max)
  {
    int divider = (depth == 8) ? EIGHT_BYTE_DIVIDER : SIXTEEN_BYTE_DIVIDER;

    for (int i = 0; i < values.Length; i++)
    {
      float value = values[i];
      value = (float)(min + value * (max - min) / divider);
      values[i] = value;
    }
  }

  float NormalizeAZI(int depth)
  {
    int divider = (depth == 8) ? EIGHT_BYTE_DIVIDER : SIXTEEN_BYTE_DIVIDER;
    return (float)(angle * rays / divider);
  }


  void PrepareString()
  {
    string stringToSearch = "<!-- END XML -->";

    int endInfoIndex = file.IndexOf(stringToSearch);
    int startBlobData = endInfoIndex + stringToSearch.Length;

    info = file.Substring(0, endInfoIndex);
    data = file.Substring(startBlobData);
  }

  XmlDocument ReadXmlFile()
  {
    XmlDocument doc = new XmlDocument();
    doc.LoadXml(info);
    return doc;
  }

  string ReadFromBlobString(int refid)
  {
    string headerSearchString = $"<BLOB blobid=\"{refid}\"";

    int headerStart = data.IndexOf(headerSearchString);
    int headerEnd = data.IndexOf(">", headerStart);

    int blobDataStart = headerEnd + 2;
    int blobDataEnd = data.IndexOf("</BLOB>", blobDataStart);
    int blobDataLen = blobDataEnd - blobDataStart - 1;

    string output = data.Substring(blobDataStart, blobDataLen);
    return output;
  }

  float[] DecompressData(int refid, int depth)
  {
    Encoding latinEncoding = Encoding.GetEncoding("ISO-8859-1");
    byte[] compressedData = latinEncoding.GetBytes(ReadFromBlobString(refid));
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
}
