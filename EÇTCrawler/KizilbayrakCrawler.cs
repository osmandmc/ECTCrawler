
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
    public class KızılBayrakCrawler
    {
        public void DoJob(DocX doc, DateTime startDate, DateTime endDate)
        {
            Logger.WriteLog("Kızılbayrak çalışmaya başladı");
            Console.WriteLine("Kızılbayrak çalışmaya başladı");

            int i = 1;
            bool isOver = false;
            do
            {
                var pageContent = Utility.GetPageContent("http://www.kizilbayrak40.net/ana-sayfa/sinif/sayfa/" + i);
                var news = FindNews(pageContent);
                int j = 0;
                while (j < news.Count && !isOver)
                {
                    try
                    {
                        if (news[j].Date.Date >= startDate && news[j].Date.Date <= endDate)
                        {
                            var absolutePath = "http://www.kizilbayrak40.net/" + news[j].Url.Trim();
                            var newsContent = Utility.GetPageContent(absolutePath);
                            Console.WriteLine("Parsing");
                            AddNewsItemAsync(doc, newsContent, absolutePath);

                        }
                        if (news[j].Date.Date < startDate)
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
                i++;
            }
            while (!isOver);
            doc.Save();
          
        }
        public List<News> FindNews(string pageContent)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(pageContent);
            var news = doc.DocumentNode.SelectNodes("//div[@class='article articletype-0']")
                .Select(node => new News
                {
                    Url = node.Descendants().Where(n => n.HasClass("header")).FirstOrDefault()
                    .Descendants("a").First()
                    .Attributes["href"].Value,
                    Date = DateTime.Parse(node.Descendants().Where(n => n.HasClass("news-list-date"))
                    .First().InnerText.Trim().Substring(0, 10))
                }).ToList();

            return news;
        }
        public void AddNewsItemAsync(DocX doc, string newsContent, string url)
        {
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(newsContent);

            var mainContent = html.DocumentNode.SelectSingleNode("//div[@class='article']");
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
                if (node.FirstChild == null || (node.FirstChild != null &&
                    (node.FirstChild.Name == "i" || node.FirstChild.Name == "strong")))
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
                var p = doc.InsertParagraph();
                var localFileName = @"C:\ECT\images\" + Guid.NewGuid() + ".jpg";
                Utility.DownloadRemoteImageFile("http://kizilbayrak40.net/" + node.Attributes["src"].Value, localFileName);
                Image image = doc.AddImage(localFileName);
                Picture picture = image.CreatePicture();
                p.InsertPicture(picture);
                // File.Delete(localFileName);
            }
            else if (node.Name == "#text" &&
                !String.IsNullOrEmpty(node.InnerText.Trim()) &&
                node.ParentNode.Name != "script" &&
                node.InnerText != "&nbsp;")
            {
                var p = doc.InsertParagraph();
                p.AppendLine(node.InnerText);
                if (node.ParentNode.Name == "h2")
                    p.Heading(HeadingType.Heading1);

            }
        }
        public string[] FindUrl(string content)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(content);
            return doc.DocumentNode.SelectNodes("//div[@class='article articletype-0']")
                .Descendants("a")
                .Select(node => node.Attributes["href"].Value)
                .ToArray();
        }
        public List<HtmlNode> GetNodesToExclude(HtmlDocument html)
        {
            var nodesToExlude = new List<HtmlNode>();
            if (html.DocumentNode.SelectNodes("//div[contains(@class, 'footer-news')]") != null)
                nodesToExlude.AddRange(html.DocumentNode.SelectNodes("//span[contains(@class, 'news-list-category')]").ToList());
            if (html.DocumentNode.SelectNodes("//div[contains(@class, 'foto-manset')]") != null)
                nodesToExlude.AddRange(html.DocumentNode.SelectNodes("//div[contains(@class, 'foto-manset')]").ToList());
            if (html.DocumentNode.SelectNodes("//div[@class = 'facebook']") != null)
                nodesToExlude.AddRange(html.DocumentNode.SelectNodes("//div[@class = 'facebook']").ToList());
            if (html.DocumentNode.SelectNodes("//div[@class = 'news-related-wrap']") != null)
                nodesToExlude.AddRange(html.DocumentNode.SelectNodes("//div[@class = 'news-related-wrap']").ToList());
            return nodesToExlude;
        }
    }
}
