using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Words.NET;

namespace EÇTCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Haberleri hangi tarihten itibaren indirmeye başlamak istersin? Şu formatta gir: dd.MM.yyyy");
            var startDateInput = Console.ReadLine();
            DateTime startDate = DateTime.MinValue;
            DateTime.TryParse(startDateInput, out startDate);

            Console.WriteLine("Haberleri hangi tarihe kadar indirelim? Şu formatta gir: dd.MM.yyyy");
            var endDateInput = Console.ReadLine();
            DateTime endDate = DateTime.MinValue;
            DateTime.TryParse(endDateInput, out endDate);
            try
            {
                string imgFolderPath = @"C:\ECT\images";
                if (!Directory.Exists(imgFolderPath))
                    Directory.CreateDirectory(imgFolderPath);
                string fileName = $@"C:\ECT\news{startDate.Date.Month}_{startDate.Date.Year}.docx";
                DocX doc;
                if (!File.Exists(fileName))
                    doc = DocX.Create(fileName);
                else
                    doc = DocX.Load(fileName);

                new EvrenselCrawler().DoJob(doc, startDate, endDate);
                doc.Save();
                Logger.WriteLog("Evrensel Tamamlandı");
                Console.WriteLine("İndirme başarıyla sona erdi. Kontrol edebilirsin.");
                new KızılBayrakCrawler().DoJob(doc, startDate, endDate);
                doc.Save();
                Logger.WriteLog("Kızılbayrak Tamamlandı");
                Console.WriteLine("Kızılbayrak başarıyla sona erdi. Kontrol edebilirsin.");
                if (!Directory.Exists(imgFolderPath))
                    Directory.Delete(imgFolderPath);
            }
            catch(Exception ex)
            {
                Logger.WriteLog(ex);
                Console.WriteLine("Bir hata oldu, olur böyle şeyler. Hata detayı şurda, yazılımcı arkadaşa ilet bi zahmet: " + ex.Message);
                Console.ReadLine();
            }
            
        }
    }
}
