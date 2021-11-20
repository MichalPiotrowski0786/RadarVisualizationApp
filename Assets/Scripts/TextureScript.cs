using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextureScript : MonoBehaviour
{
  public RenderTexture[] RadarTexture;
  public Texture2D[] colormaps;
  public ComputeShader textureShader;

  public Text infoText;
  public Text maxValueText;
  public Text minValueText;
  public Dropdown sitesDropdown;
  public Image loadingSpinner;
  public RawImage colormapImage;
  public Button[] buttons;

  List<float[]> data;
  int siteDatatypes;

  public int rays; public int bins;
  List<float> dmin; List<float> dmax;

  public int datatype = 0;
  public float[] angles;

  void Start()
  {
    GetData(0); // hardcoded start at Brzuchania, might refactor later
    sitesDropdown.onValueChanged.AddListener(delegate { GetData(sitesDropdown.value); });
  }

  void Update()
  {
    if (RadarTexture.Length > 0) GetComponent<MeshScript>().material.mainTexture = RadarTexture[datatype];
  }

  void ButtonsLogic()
  {
    if (buttons.Length > 0)
    {
      UpdateColormapImageTexture(datatype);
      for (int i = 0; i < siteDatatypes; i++)
      {
        int x = i; // lol
        buttons[i].interactable = true;
        buttons[i].onClick.AddListener(() =>
        {
          datatype = x;
          UpdateColormapImageTexture(x);
        });
      }
    }
  }

  void SendToTextureArray()
  {
    RadarTexture = new RenderTexture[siteDatatypes];

    for (int i = 0; i < siteDatatypes; i++)
    {
      RadarTexture[i] = GenerateTexture(i);
    }
  }

  void UpdateColormapImageTexture(int index)
  {
    if (colormapImage != null)
    {
      colormapImage.texture = colormaps[index];
      maxValueText.text = dmax[index].ToString().Replace(',', '.');
      minValueText.text = dmin[index].ToString().Replace(',', '.');
    }
  }

  void GetData(int siteIndex)
  {
    string url = "ftp://daneradarowe.pl/";
    SiteData siteData = new SiteData(url);

    string[] sites = siteData.FetchSiteList();
    string site = sites[siteIndex];
    string[] scans = siteData.FetchScanList(site);

    siteDatatypes = 2;
    if (siteIndex == 3 || siteIndex == 5 || siteIndex == 6) siteDatatypes = 3;
    string[] dataName = new string[3] { "dBZ", "V", "RhoHV" };

    RadarTexture = null;
    data = new List<float[]>();
    angles = new float[siteDatatypes];
    dmin = new List<float>();
    dmax = new List<float>();

    for (int i = 0; i < siteDatatypes; i++)
    {
      string scanName = $"{site}/{scans[scans.Length - 1]}{dataName[i]}.vol";
      string scan = siteData.FetchScan(scanName);

      DecodeData decoded = new DecodeData(scan);
      angles[i] = decoded.angle;

      if (decoded.values.Length > 0) data.Add(decoded.values);
      dmin.Add(decoded.min);
      dmax.Add(decoded.max);

      if (i == 0)
      {
        rays = decoded.rays - 1; // no idea why, but subtracting one from rays fixes mesh and texture problems
        bins = decoded.bins;

        string infoString = $"{decoded.siteName}\n{decoded.scanTime}z\n{decoded.scanDate}";
        if (infoText != null) infoText.text = infoString;
      }
    }

    siteData.CloseFTPConnection();

    ButtonsLogic();
    SendToTextureArray();
  }

  RenderTexture GenerateTexture(int index)
  {
    if (textureShader != null)
    {
      RenderTexture rt = new RenderTexture(rays, bins, 0);
      rt.format = RenderTextureFormat.ARGB32;
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
}