using System.Xml;

public class DecodeData
{
  string file;
  public string snippet = "";

  public DecodeData(string file)
  {
    this.file = file;
    Decode();
  }

  void Decode()
  {
    PrepareData();
    var data = ReadXmlFile();
    snippet = data.GetElementsByTagName("scan")[0].Attributes[0].Value;
  }

  void PrepareData()
  {
    string stringToSearch = "<!-- END XML -->";
    int endXmlIndex = file.IndexOf(stringToSearch) + stringToSearch.Length;
    file = file.Remove(endXmlIndex);
  }

  XmlDocument ReadXmlFile()
  {
    XmlDocument doc = new XmlDocument();
    doc.LoadXml(file);
    return doc;
  }
}
