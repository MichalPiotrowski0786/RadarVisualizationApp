using System;
using System.Collections.Generic;
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

  public string[] FetchSites()
  {
    var list = client.GetListing();
    List<string> res = new List<string>();

    foreach (FtpListItem item in client.GetListing())
    {
      if (item.FullName.Contains("125")) res.Add(item.FullName);
    }

    return res.ToArray();
  }

  public string[] FetchScans(string site)
  {
    // LONGEST TASK, TRY REFACTORING
    var list = client.GetListing(site);
    List<string> res = new List<string>();

    foreach (FtpListItem item in list)
    {
      res.Add(item.FullName);
    }

    return res.ToArray();
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
