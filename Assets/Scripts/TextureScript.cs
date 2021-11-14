using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextureScript : MonoBehaviour
{
  public RenderTexture[] RadarTexture;
  public Texture2D[] colormaps;
  public ComputeShader textureShader;
  public Text infoText;

  List<float[]> data = new List<float[]>();
  int datalen;

  public int rays; public int bins;
  List<float> dmin; List<float> dmax;

  [Range(0, 1)]
  public int datatype = 0;

  public float[] angles;

  void Awake()
  {
    GetData();
  }

  void Start()
  {
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
    string url = "ftp://daneradarowe.pl/";
    SiteData siteData = new SiteData(url);

    string[] sites = siteData.FetchSites();
    string site = sites[2];

    string[] scans = siteData.FetchScans(site);

    string scanZ = scans[scans.Length - 1];
    string scanV = scans[scans.Length - 2];

    string fetchedZ = siteData.FetchData(scanZ);
    string fetchedV = siteData.FetchData(scanV);

    siteData.CloseFTPConnection();

    DecodeData decodeZ = new DecodeData(fetchedZ);
    DecodeData decodeV = new DecodeData(fetchedV);
    angles = new float[] { decodeZ.angle, decodeV.angle };

    if (decodeZ.values.Length > 0) data.Add(decodeZ.values);
    if (decodeV.values.Length > 0) data.Add(decodeV.values);
    datalen = data.Count;

    dmin = new List<float>(); dmax = new List<float>();

    rays = decodeZ.rays;
    bins = decodeZ.bins;

    dmin.Add(decodeZ.min); dmin.Add(decodeV.min);
    dmax.Add(decodeZ.max); dmax.Add(decodeV.max);

    string infoString =
    $" {decodeZ.siteName} {decodeZ.scanTime}z {decodeZ.scanDate}";

    if (infoText != null) infoText.text = infoString;
  }

  RenderTexture GenerateTexture(int index)
  {
    if (textureShader != null)
    {
      RenderTexture rt = new RenderTexture(rays, bins, 0);
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

      textureShader.SetInt("rays", rays);
      textureShader.SetInt("bins", bins);
      textureShader.SetInt("type", index);

      textureShader.SetFloat("_dmin", dmin[index]);
      textureShader.SetFloat("_dmax", dmax[index]);

      textureShader.Dispatch(karnel, rays, bins, 1);
      dataBuffer.Release();

      return rt;
    }
    else
    {
      return null;
    }
  }

  void UpdateUI()
  {

  }
}