using System;
using System.IO;
using System.Net;
using System.Text;
using FluentFTP;

public class SiteData
{
  FtpClient client;
  string url;

  public SiteData(string url)
  {
    this.url = url;

    client = new FtpClient(url);
    client.AutoConnect();
  }

  public string FetchSites()
  {
    string res = null;
    foreach (FtpListItem item in client.GetListing())
    {
      if (item.FullName.Contains("125")) res += $"{item.FullName};";
    }
    return res;
  }

  public string FetchScans(string site)
  {
    // LONGEST TASK, TRY REFACTORING
    string res = null;
    foreach (FtpListItem item in client.GetListing(site))
    {
      res += $"{item.FullName};";
    }
    return res;
  }

  public string FetchData(string scan)
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
