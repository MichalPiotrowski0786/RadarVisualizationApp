using System;
using System.IO;
using System.Net;
using System.Text;

public class SiteData
{
  string url;

  public SiteData(string url)
  {
    this.url = url;
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
        request.KeepAlive = false;
        request.UsePassive = false;

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

  public string FetchScans(string site)
  {
    string res = null;

    if (url.Length > 0 && url != null)
    {
      try
      {
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"{url}{site}/");
        request.Method = WebRequestMethods.Ftp.ListDirectory;
        request.KeepAlive = false;
        request.UsePassive = false;

        var response = (FtpWebResponse)request.GetResponse();
        if (response.StatusCode == FtpStatusCode.OpeningData)
        {
          Stream responseStream = response.GetResponseStream();
          StreamReader reader = new StreamReader(responseStream);

          while (reader.Peek() > 0)
          {
            string line = reader.ReadLine();
            res += $"{line};";
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

  public string FetchData(string site, string scan)
  {
    string res = "";

    if (url.Length > 0 && url != null)
    {
      using (WebClient client = new WebClient())
      {
        client.Encoding = Encoding.GetEncoding("ISO-8859-1");
        try
        {
          res = client.DownloadString($"{url}{site}/{scan}");
        }
        catch (Exception e)
        {
          res = e.Message;
        }
      }
    }

    return res;
  }
}
