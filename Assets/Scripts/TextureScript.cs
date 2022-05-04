using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class TextureScript : MonoBehaviour
{
    public RenderTexture[] RadarTextures;
    public Texture2D[] colormaps;
    public ComputeShader textureShader;

    public Text infoText;
    public Text minValueText;
    public Text maxValueText;
    public Dropdown sitesDropdown;
    public Dropdown elevationsDropdown;
    public Image loadingSpinner;
    public RawImage colormapImage;
    public Button[] buttons;

    List<Scan> ScanList;
    float[] anglesForCorrection;
    int siteDatatypes;
    int elevations;

    public int datatype = 0;
    public int elevation = 0;

    public int radarX = 0;
    public int radarY = 0;

    private string[] _minValues = new string[3] { "-31.5", "-47.7", "0%" };
    private string[] _maxValues = new string[3] { "95.5", "47.7", "100%" };

    void Start()
    {
        GetData(0); // hardcoded start at Brzuchania, might refactor later
        FixAngleOnMeshObject(0, 0);
        sitesDropdown.onValueChanged.AddListener((x) =>
        {
            GetData(x);
            FixAngleOnMeshObject(datatype, elevation);
        });
        elevationsDropdown.onValueChanged.AddListener((x) =>
        {
            elevation = x;

            FixAngleOnMeshObject(datatype, x);
        });
    }

    void Update()
    {
        int radarTextureIndex = (datatype * elevations) + elevation;
        if (RadarTextures.Length > 0) GetComponent<MeshScript>().material.mainTexture = RadarTextures[radarTextureIndex];
    }

    void ButtonsLogic()
    {
        if (buttons.Length > 0)
        {
            foreach (Button b in buttons) b.interactable = false;

            UpdateColormapElement(datatype);
            UpdateInfoText(datatype);

            for (int i = 0; i < siteDatatypes; i++)
            {
                int x = i; // lol
                buttons[i].interactable = true;
                buttons[i].onClick.AddListener(() =>
                {
                    datatype = x;

                    UpdateColormapElement(x);
                    FixAngleOnMeshObject(x, elevation);
                    UpdateInfoText(x);
                });
            }
        }
    }

    void UpdateColormapElement(int index)
    {
        colormapImage.texture = colormaps[index];
        minValueText.text = _minValues[index];
        maxValueText.text = _maxValues[index];
    }

    void FixAngleOnMeshObject(int dataIndex, int elevIndex)
    {
        int index = (dataIndex * elevations) + elevIndex;
        var radarMeshGameObject = GameObject.FindGameObjectWithTag("radar");

        if (radarMeshGameObject != null)
        {
            Vector3 meshRotation = new Vector3(anglesForCorrection[index] - 180f, 90f, -90f);
            radarMeshGameObject.transform.rotation = Quaternion.Euler(meshRotation.x, meshRotation.y, meshRotation.z);
        }
    }

    void UpdateInfoText(int scanIndex)
    {
        string dataName = "";
        if (scanIndex == 0) dataName = "Reflectivity";
        else if (scanIndex == 1) dataName = "Velocity";
        else dataName = "Correlation Coefficent";

        string infoString = $"{dataName}\n{ScanList[scanIndex].name}\n{ScanList[scanIndex].time}z\n{ScanList[scanIndex].date}";
        infoText.text = infoString;
    }

    void GetData(int siteIndex)
    {
        string url = "daneradarowe.pl";
        SiteData siteData = new SiteData(url);

        string[] sites = siteData.FetchSiteList();
        string site = sites[siteIndex];
        string[] scans = siteData.FetchScanList(site);

        siteDatatypes = 2;
        if (siteIndex == 3 || siteIndex == 5 || siteIndex == 6) siteDatatypes = 3;
        string[] dataName = new string[3] { "dBZ", "V", "RhoHV" };

        elevationsDropdown.ClearOptions();

        ScanList = new List<Scan>();
        var RadarTexturesList = new List<RenderTexture>();
        var anglesForCorrectionList = new List<float>();
        for (int i = 0; i < siteDatatypes; i++)
        {
            string scanName = $"{site}/{scans[scans.Length - 1]}{dataName[i]}.vol";
            string scan = siteData.FetchScan(scanName);
            //string scan = File.ReadAllText(@$"C:\Users\MichalPiotrowski\Desktop\Moje\ciekawostki_radarowe\2021062416234800{dataName[i]}.vol", Encoding.GetEncoding("ISO-8859-1"));

            DecodeData data = new DecodeData(scan);
            ScanList.Add(data.scan);
            elevations = data.len;

            foreach (Slice slice in data.scan.ReturnSliceArr())
            {
                RadarTexturesList.Add(GenerateTexture(i, slice.data, slice.rays - 1, slice.bins, slice.max, slice.min));
                anglesForCorrectionList.Add(slice.angle);

                if (i == 0)
                {
                    var dropdownOption = new Dropdown.OptionData();
                    dropdownOption.text = $"{slice.elevation}°";
                    elevationsDropdown.options.Add(dropdownOption);
                }
            }
        }
        siteData.CloseFTPConnection();

        anglesForCorrection = anglesForCorrectionList.ToArray();

        elevationsDropdown.value = elevation;
        elevationsDropdown.captionText.text = elevationsDropdown.options[elevation].text;

        RadarTextures = RadarTexturesList.ToArray();
        ButtonsLogic();
    }

    RenderTexture GenerateTexture(int index, float[] data, int rays, int bins, float max, float min)
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

            ComputeBuffer dataBuffer = new ComputeBuffer(data.Length, sizeof(float));
            dataBuffer.SetData(data);
            textureShader.SetBuffer(karnel, "_data", dataBuffer);

            textureShader.SetInt("rays", rays);
            textureShader.SetInt("bins", bins);
            textureShader.SetInt("type", index);

            textureShader.SetFloat("_dmin", min);
            textureShader.SetFloat("_dmax", max);

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