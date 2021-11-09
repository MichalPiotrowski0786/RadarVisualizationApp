using System;
using System.IO;
using System.Net;

public class SiteData
{
  string url;
  public string sites;

  public SiteData(string url)
  {
    this.url = url;
    sites = FetchSites();
  }

  public string FetchSites()
  {
    string file = null;

    if (url.Length > 0 && url != null)
    {
      using (WebClient client = new WebClient())
      {
        try
        {
          file = client.DownloadString(url);
        }
        catch (Exception e)
        {
          throw e;
        }
      }
    }

    return file;
  }
}
