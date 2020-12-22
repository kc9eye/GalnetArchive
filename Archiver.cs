using HtmlAgilityPack;
using System;
using System.IO;
using System.Xml;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace GalnetArchiver
{
    class Archiver { 
 
        static void Main(string[] args)
        {
            string baseUrl = "https://community.elitedangerous.com/en/galnet";

            Archiver app = new Archiver();
            XmlDocument archive = new XmlDocument();

            Console.WriteLine("Galnet Archiver v0.12");
            Console.WriteLine("Loading archive...");
            archive.Load(Path.Combine(Directory.GetCurrentDirectory(),"GalnetArchive.xml"));

            XmlNode root = archive.DocumentElement;
            XmlNode last = root.LastChild;
            XmlNode lastDate = last.SelectSingleNode("date");
            DateTime lastUpdated = Convert.ToDateTime(lastDate.InnerText);
            DateTime currentDate = Convert.ToDateTime(DateTime.UtcNow.AddYears(1286));

            Console.WriteLine("Last archived article dated: " + lastUpdated.ToString("dd MMM yyyy"));

            List<XmlNode> newArticles = new List<XmlNode>();

            foreach (DateTime day in app.EachDay(lastUpdated, currentDate))
            {     
                if (day.ToString("dd MMM yyyy") != lastUpdated.ToString("dd MMM yyyy"))
                {
                    Console.Write("Checking for articles on: " + day.ToString("dd MMM yyyy") + "...");
                    HtmlWeb web = new HtmlWeb();
                    var doc = web.Load(baseUrl + "/" + day.ToString("dd-MMM-yyyy"));
                    HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[@class='article']");

                    if (nodes != null) 
                    {
                        Console.WriteLine(" Found articles");
                        foreach(HtmlNode node in nodes)
                        {                           
                            String id = Guid.NewGuid().ToString();
                            String link = node.SelectSingleNode("h3/a").GetAttributeValue("href", "");
                            String date = node.SelectSingleNode("//p[@class='small']").InnerText;
                            String title = node.SelectSingleNode("h3").InnerText;
                            String story = node.SelectSingleNode("p").InnerText;

                            Console.Write("Archiving new article: " + title +"...");

                            XmlNode article = archive.CreateElement("article");

                            XmlNode Id = archive.CreateElement("id");
                            Id.AppendChild(archive.CreateTextNode(id));
                            article.AppendChild(Id);

                            XmlNode Link = archive.CreateElement("link");
                            Link.AppendChild(archive.CreateTextNode(link));
                            article.AppendChild(Link);

                            XmlNode Title = archive.CreateElement("title");
                            Title.AppendChild(archive.CreateTextNode(title));
                            article.AppendChild(Title);

                            XmlNode Date = archive.CreateElement("date");
                            Date.AppendChild(archive.CreateTextNode(date));
                            article.AppendChild(Date);

                            XmlNode Story = archive.CreateElement("story");
                            Story.AppendChild(archive.CreateTextNode(story));
                            article.AppendChild(Story);

                            newArticles.Add(article);
                            
                            //Console.WriteLine(" archived!");
                            
                        }

                        foreach(XmlNode article in newArticles)
                        {
                            root.AppendChild(article);
                        }

                        archive.Save(Path.Combine(Directory.GetCurrentDirectory(), "GalnetArchive.xml"));
                    }
                    else
                    {
                        Console.WriteLine(" no articles found.");
                    }
                    Thread.Sleep(10000);
                }
                
            }
            Console.WriteLine("Completed");
        }

        public IEnumerable EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }
    }
}
