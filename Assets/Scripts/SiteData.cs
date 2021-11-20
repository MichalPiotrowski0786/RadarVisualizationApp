using System;
using System.Linq;
using System.Net;
using System.Text;
using FluentFTP;

public class SiteData
{
  FtpClient client;
  public string url;

  public SiteData(string url)
  {
    this.url = url;

    client = new FtpClient(url);
    client.AutoConnect();
  }

  public string[] FetchSiteList()
  {
    return client.GetListing()
      .Where((x) => x.FullName.Contains("125"))
      .Select((x) => x.FullName)
      .ToArray();
  }

  public string[] FetchScanList(string site)
  {
    // LONGEST TASK, TRY REFACTORING
    // EDIT 13.11.2021: WAY BETTER PERFORMANCE NOW, BUT NOT IDEAL
    // EDIT 20.11.2021: CHANGED TO LINQ QUERRY, SLIGHT IMPROVEMENT

    return client.GetListing(site)
      .Select((x) => x.Name.Substring(0, 16))
      .ToArray();
  }

  public string FetchScan(string scan)
  {
    string res = "";
    if (url.Length > 0 && url != null)
    {
      using (WebClient client = new WebClient())
      {
        client.Encoding = Encoding.GetEncoding("ISO-8859-1");
        try
        {
          res = client.DownloadString($"{url}{scan}");
        }
        catch (Exception e)
        {
          res = e.Message;
        }
      }
    }

    return res;
  }

  public void CloseFTPConnection() { client.Disconnect(); }
}
