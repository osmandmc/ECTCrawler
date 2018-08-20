using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using Xceed.Words.NET;

namespace EÇTCrawler
{
    public interface ICrawler
    {
        void DoJob(DateTime startDate, DateTime endDate);
        void AddNewsItemAsync(DocX doc, string newsContent);
        void AddHtml(DocX doc, HtmlNode node, List<HtmlNode> nodesToExclude);
        void AddNodeToDoc(DocX doc, HtmlNode node);
        string[] FindUrl(string content);
        List<HtmlNode> GetNodesToExclude(HtmlDocument html);
    }
}

