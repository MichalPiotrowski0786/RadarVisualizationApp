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

  [Range(0, 2)]
  public int datatype = 0;

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
    FetchData fetchData = new FetchData("ftp://daneradarowe.pl/GDA_125_ZVW.vol/2021110423342100dBZ.vol");
    string fetchedFile = fetchData.SendRequest();
    DecodeData decodeData = new DecodeData(fetchedFile);
    Debug.Log(decodeData.snippet);

    string path = Application.dataPath + "/Temp/";

    DataReader dbz_dr = null;
    DataReader vel_dr = null;
    DataReader rho_dr = null;

    if (File.Exists(path + "dBZ")) dbz_dr = new DataReader(path + "dBZ");
    if (File.Exists(path + "V")) vel_dr = new DataReader(path + "V");
    if (File.Exists(path + "RhoHV")) rho_dr = new DataReader(path + "RhoHV");

    if (dbz_dr != null) data.Add(dbz_dr.ReturnData());
    if (vel_dr != null) data.Add(vel_dr.ReturnData());
    if (rho_dr != null) data.Add(rho_dr.ReturnData());

    datalen = data.Count;

    rays = new List<int>(); bins = new List<int>();
    dmin = new List<float>(); dmax = new List<float>();

    if (dbz_dr != null)
    {
      rays.Add(dbz_dr.rays);
      bins.Add(dbz_dr.bins);
      dmin.Add(-31.5f);
      dmax.Add(95.5f);
    }

    if (vel_dr != null)
    {
      rays.Add(vel_dr.rays);
      bins.Add(vel_dr.bins);
      dmin.Add(-47.7f);
      dmax.Add(47.7f);
    }

    if (rho_dr != null)
    {
      rays.Add(rho_dr.rays);
      bins.Add(rho_dr.bins);
      dmin.Add(0f);
      dmax.Add(1f);
    }

    //if (dbz_dr != null && File.Exists(dbz_dr.path)) File.Delete(dbz_dr.path);
    //if (vel_dr != null && File.Exists(vel_dr.path)) File.Delete(vel_dr.path);
    //if (rho_dr != null && File.Exists(rho_dr.path)) File.Delete(rho_dr.path);
  }

  RenderTexture GenerateTexture(int data_index)
  {
    if (textureShader != null)
    {
      RenderTexture rt = new RenderTexture(rays[data_index], bins[data_index], 0);
      rt.enableRandomWrite = true;
      rt.filterMode = FilterMode.Point;
      rt.anisoLevel = 0;
      rt.Create();

      int karnel = textureShader.FindKernel("Main");
      textureShader.SetTexture(karnel, "tex2d", rt);
      textureShader.SetTexture(karnel, "cmap", colormaps[data_index]);

      ComputeBuffer dataBuffer = new ComputeBuffer(data[data_index].Length, sizeof(float));
      dataBuffer.SetData(data[data_index]);
      textureShader.SetBuffer(karnel, "_data", dataBuffer);

      textureShader.SetInt("r", rays[data_index]);
      textureShader.SetInt("b", bins[data_index]);
      textureShader.SetInt("type", data_index);

      textureShader.SetFloat("_dmin", dmin[data_index]);
      textureShader.SetFloat("_dmax", dmax[data_index]);

      textureShader.Dispatch(karnel, rays[data_index], bins[data_index], 1);
      dataBuffer.Release();

      return rt;
    }
    else
    {
      return null;
    }
  }
}