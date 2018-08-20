using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EÇTCrawler
{
    public class Utility
    {
        public static string GetPageContent(string url)
        {
            Console.WriteLine("Şu sayfaya istek gönderildi: " + url);
            try
            {
                WebRequest wr = WebRequest.Create(url);
                WebResponse ws = wr.GetResponse();
                StreamReader sr = new StreamReader(ws.GetResponseStream(), Encoding.UTF8);

                string response = WebUtility.HtmlDecode(sr.ReadToEnd());
                ws.Close();
                sr.Close();
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(url + " linki hata verdi", ex);
            }



        }
        public static void DownloadRemoteImageFile(string uri, string fileName)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                // Check that the remote file was found. The ContentType
                // check is performed since a request for a non-existent
                // image file might be redirected to a 404-page, which would
                // yield the StatusCode "OK", even though the image was not
                // found.
                if ((response.StatusCode == HttpStatusCode.OK ||
                    response.StatusCode == HttpStatusCode.Moved ||
                    response.StatusCode == HttpStatusCode.Redirect) &&
                    response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
                {

                    // if the remote file was found, download oit
                    using (Stream inputStream = response.GetResponseStream())
                    using (Stream outputStream = File.OpenWrite(fileName))
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead;
                        do
                        {
                            bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                            outputStream.Write(buffer, 0, bytesRead);
                        } while (bytesRead != 0);
                    }
                }
            }
            catch (Exception)
            {
                Logger.WriteLog(uri + " indirme sırasında başarısız oldu");
            }
        }
    }
}
