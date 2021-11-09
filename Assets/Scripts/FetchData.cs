using System;
using System.Net;
using System.Text;

public class FetchData
{
  public string url;

  public FetchData(string url)
  {
    this.url = url;
  }

  public string SendRequest()
  {
    string file = "";

    if (url.Length > 0 && url != null)
    {
      using (WebClient client = new WebClient())
      {
        client.Encoding = Encoding.GetEncoding("ISO-8859-1");
        try
        {
          file = client.DownloadString(url);
        }
        catch (Exception e)
        {
          file = e.Message;
        }
      }
    }

    return file;
  }

  //   void SaveFile()
  //   {
  //     if (file.Length > 0 && file is not null)
  //     {

  //     }
  //   }
}
