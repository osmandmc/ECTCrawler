
using HtmlAgilityPack;
using HtmlToOpenXml;
using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xceed.Words.NET;

namespace EÇTCrawler
{
    public class EvrenselCrawler
    {
        public void DoJob(DocX doc, DateTime startDate, DateTime endDate)
        {
            Console.WriteLine("Evrensel indirilmeye başlıyor");
            Logger.WriteLog("Evrensel başladı");
            int i = 1;
            bool isOver = false;
            do
            {
                try
                {
                    var pageContent = Utility.GetPageContent("https://www.evrensel.net/kategori/2/isci-sendika/s/" + i);
                    var urls = FindUrl(pageContent);
                    int j = 0;
                    while (j < urls.Length && !isOver)
                    {
                        try
                        {
                            Logger.WriteLog(urls[j]);
                            var newsContent = Utility.GetPageContent(urls[j]);
                            var newsDate = FindDate(newsContent);
                            if (newsDate.Date >= startDate && newsDate.Date <= endDate)
                            {
                                AddNewsItemAsync(doc, newsContent, urls[j]);
                            }
                            if (newsDate.Date < startDate)
                                isOver = true;
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteLog(ex);                            
                        }
                        finally
                        {
                            j++;
                        }
                    }
                }
                catch(Exception ex)
                {
                    Logger.WriteLog(ex);
                    Console.WriteLine(ex.Message);
                    isOver = true;
                }
                i++;
            }
            while (!isOver);
           
        }

        public DateTime FindDate(string content)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(content);
            string dateStr = string.Empty;
            if (doc.DocumentNode.SelectNodes("//div[@class = 'web-tv-black']") != null)
                dateStr = doc.DocumentNode.SelectSingleNode("//div[@class='articledate-webtv']")
                    .Descendants("span").First().InnerText;
            else
                dateStr = doc.DocumentNode.SelectSingleNode("//div[@class='articledate']")
            .Descendants("span").First().InnerText;
            var date = DateTime.Parse(dateStr);
            return date;
        }
       
        public string[] FindUrl(string content)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(content);
            return doc.DocumentNode.SelectNodes("//div[@class='item-header']")
                .Descendants("a")
                .Select(node => node.Attributes["href"].Value.Trim())
                .Where(n => n.StartsWith("https://www.evrensel.net/haber/"))
                .ToArray();
        }
        public void AddNewsItemAsync(DocX doc, string newsContent, string url)
        {
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(newsContent);

            var mainContent = html.DocumentNode.SelectSingleNode("//div[@class='shortcode-content']");
            List<HtmlNode> nodesToExclude = GetNodesToExclude(html);
            AddHtml(doc, mainContent, nodesToExclude);
            doc.InsertParagraph(url);
        }
        public void AddHtml(DocX doc, HtmlNode node, List<HtmlNode> nodesToExclude)
        {
            var referenceNode = node.ParentNode;
            while (node != referenceNode)
            {
                if (nodesToExclude.Contains(node))
                {
                    node = node.ParentNode;
                    node.FirstChild.Remove();
                }
                if (node.FirstChild == null)
                {
                    AddNodeToDoc(doc, node);
                    node = node.ParentNode;
                    node.FirstChild.Remove();
                }
                else
                {
                    node = node.FirstChild;
                }
            }
        }
        public void AddNodeToDoc(DocX doc, HtmlNode node)
        {
            if (node.Name == "img")
            {
                try
                {
                    var p = doc.InsertParagraph();
                    var localFileName = @"C:\ECT\images\" + Guid.NewGuid() + ".jpg";
                    Utility.DownloadRemoteImageFile(node.Attributes["src"].Value, localFileName);
                    Image image = doc.AddImage(localFileName);
                    Picture picture = image.CreatePicture();
                    p.InsertPicture(picture);
                }
                catch (Exception)
                {

                    
                }
            
                // File.Delete(localFileName);
            }
            else if (node.Name == "#text" && !String.IsNullOrEmpty(node.InnerText.Trim()) && node.ParentNode.Name != "script")
            {
                var p = doc.InsertParagraph();
                p.AppendLine(node.InnerText);
                if (node.ParentNode.Name == "h1")
                    p.Heading(HeadingType.Heading1);
            }
        }
        public List<HtmlNode> GetNodesToExclude(HtmlDocument html)
        {
            var nodesToExlude = new List<HtmlNode>();
            if (html.DocumentNode.Descendants("figure") != null)
                nodesToExlude.AddRange(html.DocumentNode.Descendants("figure").ToList());
            if (html.DocumentNode.SelectNodes("//div[@class = 'webtv-mobile-reklam']") != null)
                nodesToExlude.AddRange(html.DocumentNode.SelectNodes("//div[@class = 'webtv-mobile-reklam']").ToList());
            if (html.DocumentNode.SelectNodes("//div[@class = 'about-author-webtv']") != null)
                nodesToExlude.AddRange(html.DocumentNode.SelectNodes("//div[@class = 'about-author-webtv']").ToList());
            if (html.DocumentNode.SelectNodes("//div[@class = 'fontresize-webtv']") != null)
                nodesToExlude.AddRange(html.DocumentNode.SelectNodes("//div[@class = 'fontresize-webtv']").ToList());
            if (html.DocumentNode.SelectNodes("//div[@class = 'webtv']") != null)
                nodesToExlude.AddRange(html.DocumentNode.SelectNodes("//div[@class = 'webtv']").ToList());
            if (html.DocumentNode.SelectNodes("//div[@class = 'ilgili_haber']") != null)
                nodesToExlude.AddRange(html.DocumentNode.SelectNodes("//div[@class = 'ilgili_haber']").ToList());
            if (html.DocumentNode.SelectNodes("//div[@class = 'author-content']") != null)
                nodesToExlude.AddRange(html.DocumentNode.SelectNodes("//div[@class = 'author-content']").ToList());
            if (html.DocumentNode.SelectNodes("//div[@class = 'tags-cats']") != null)
                nodesToExlude.AddRange(html.DocumentNode.SelectNodes("//div[@class = 'tags-cats']").ToList());
            return nodesToExlude;
        }



        //public void DoJobByRank(int startPageNumber, int startNewsRank, int endPageNumber, int endNewsRank)
        //{
        //    string fileName = @"C:\ECT\news.docx";
        //    DocX doc;
        //    if (!File.Exists(fileName))
        //        doc = DocX.Create(fileName);
        //    else
        //        doc = DocX.Load(fileName);
        //    int i = startPageNumber;
        //    do
        //    {
        //        var pageContent = Utility.GetPageContent("https://www.evrensel.net/kategori/2/isci-sendika/s/" + i);
        //        var urls = FindUrl(pageContent);
        //        int j = startNewsRank;
        //        var endingRank = i == endPageNumber ? endNewsRank : 18;
        //        while (j < endingRank)
        //        {
        //            var newsContent = Utility.GetPageContent(urls[j]);
        //            AddNewsItemAsync(doc, newsContent, urls[j]);
        //            j++;
        //        }
        //        i++;

        //    }
        //    while (i <= endPageNumber);

        //    doc.Save();
        //}
       
        //public List<News> FindNews(string pageContent)
        //{
        //    HtmlDocument doc = new HtmlDocument();
        //    doc.LoadHtml(pageContent);
        //    var news = doc.DocumentNode.SelectNodes("//div[@class='article articletype-0']")
        //        .Select(node => new News
        //        {
        //            Url = node.Descendants().Where(n => n.HasClass("news-img-wrap")).First()
        //            .Element("a")
        //            .Attributes["href"].Value,
        //            Date = DateTime.Parse(node.Descendants().Where(n => n.HasClass("news-list-date"))
        //            .First().InnerText.Trim().Substring(0, 10))
        //        }).ToList();

        //    return news;
        //}
    }
}
