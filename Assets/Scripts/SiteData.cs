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
    string res = null;

    if (url.Length > 0 && url != null)
    {
      try
      {
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
        request.Method = WebRequestMethods.Ftp.ListDirectory;

        var response = (FtpWebResponse)request.GetResponse();
        if (response.StatusCode == FtpStatusCode.OpeningData)
        {
          Stream responseStream = response.GetResponseStream();
          StreamReader reader = new StreamReader(responseStream);

          while (reader.Peek() > 0)
          {
            string line = reader.ReadLine();
            if (line.Contains("125")) res += $"{line};";
          }

          reader.Close();
          response.Close();
        }
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    return res;
  }
}
