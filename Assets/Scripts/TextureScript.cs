using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TextureScript : MonoBehaviour
{
  public RenderTexture[] RadarTexture;
  public Texture2D[] colormaps;
  public ComputeShader textureShader;

  List<float[]> data = new List<float[]>();
  int datalen;

  public List<int> rays; public List<int> bins;
  List<float> dmin; List<float> dmax;

  [Range(0, 1)]
  public int datatype = 0;

  public float angle;

  void Awake()
  {
    GetData();
    SendToTextureArray();
  }

  void Update()
  {
    GetComponent<MeshScript>().material.mainTexture = RadarTexture[datatype];
  }

  void SendToTextureArray()
  {
    RadarTexture = new RenderTexture[datalen];
    for (int i = 0; i < datalen; i++)
    {
      RadarTexture[i] = GenerateTexture(i);
    }
  }

  void GetData()
  {
    string site = "GDA_125_ZVW";
    string scan = "2021110902241400";
    string type = "dBZ";

    SiteData siteData = new SiteData("ftp://daneradarowe.pl/");
    Debug.Log(siteData.sites);
    FetchData fetchData = new FetchData($"ftp://daneradarowe.pl/{site}.vol/{scan}{type}.vol");
    string fetchedFile = fetchData.SendRequest();
    DecodeData decodeData = new DecodeData(fetchedFile);
    angle = decodeData.angle;

    if (decodeData.values.Length > 0) data.Add(decodeData.values);
    datalen = data.Count;

    rays = new List<int>(); bins = new List<int>();
    dmin = new List<float>(); dmax = new List<float>();

    rays.Add(decodeData.rays);
    bins.Add(decodeData.bins);
    dmin.Add(decodeData.min);
    dmax.Add(decodeData.max);
  }

  RenderTexture GenerateTexture(int index)
  {
    if (textureShader != null)
    {
      RenderTexture rt = new RenderTexture(rays[index], bins[index], 0);
      rt.enableRandomWrite = true;
      rt.filterMode = FilterMode.Point;
      rt.anisoLevel = 0;
      rt.Create();

      int karnel = textureShader.FindKernel("Main");
      textureShader.SetTexture(karnel, "tex2d", rt);
      textureShader.SetTexture(karnel, "cmap", colormaps[index]);

      ComputeBuffer dataBuffer = new ComputeBuffer(data[index].Length, sizeof(float));
      dataBuffer.SetData(data[index]);
      textureShader.SetBuffer(karnel, "_data", dataBuffer);

      textureShader.SetInt("rays", rays[index]);
      textureShader.SetInt("bins", bins[index]);
      textureShader.SetInt("type", index);

      textureShader.SetFloat("_dmin", dmin[index]);
      textureShader.SetFloat("_dmax", dmax[index]);

      textureShader.Dispatch(karnel, rays[index], bins[index], 1);
      dataBuffer.Release();

      return rt;
    }
    else
    {
      return null;
    }
  }
}