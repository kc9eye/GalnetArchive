﻿using System;
using System.IO;
using System.Xml;
using System.Threading.Tasks;
using Npgsql;

namespace PGSqlUpdater
{
    class PGSqlUpdater
    {
        static async Task Main(string[] args)
        {
            string dcs = "";
            if (args.Length == 0||args.Length < 5)
            {
                Console.WriteLine("Usage: PGSqlUpdater.exe [dbhost] [dbport] [dbname] [dbuser] [dbpassword]");
                Environment.Exit(1);
            }
            else
            {
                dcs = "Host=" + args[0] + ";Port=" + args[1] + ";Database=" + args[2] + ";Username=" + args[3] + ";Password=" + args[4];
            }
            Console.WriteLine("PGSqlUpdater v0.1");
            Console.WriteLine("Loading archive...");
            XmlDocument archive = new XmlDocument();
            archive.Load(Path.Combine(Directory.GetCurrentDirectory(), "GalnetArchive.xml"));
            XmlNode root = archive.DocumentElement;
            XmlNode last = root.LastChild;
            XmlNode lastDate = last.SelectSingleNode("date");
            DateTime lastUpdated = Convert.ToDateTime(lastDate.InnerText);
            Console.WriteLine("Last archived article dated: " + lastUpdated.ToString("dd MMM yyyy"));
            try
            {
                string lastStored;
                Console.WriteLine("Trying database {0} at {1}...",args[2],args[0]);
                await using var dbh = new NpgsqlConnection(dcs);
                await dbh.OpenAsync();
                await using (var query = new NpgsqlCommand("select max(_date) from articles",dbh))
                await using (var rdr = await query.ExecuteReaderAsync())
                {
                    await rdr.ReadAsync();
                    lastStored = rdr[0].ToString();   
                }
                await dbh.CloseAsync();

                if (String.IsNullOrEmpty(lastStored))
                {
                    Console.WriteLine("Database is empty, archiving all articles...");
                    PGSqlUpdater updater = new PGSqlUpdater();
                    await updater.StoreAllArticles(dcs, archive);
                }
            }
            catch(NpgsqlException e)
            {
                Console.WriteLine(e);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
 

        }

        public async Task StoreAllArticles(string dcs, XmlDocument archive)
        {
            XmlNode root = archive.DocumentElement;
            XmlNodeList articles = root.SelectNodes("//article");
            try
            {
                await using var dbh = new NpgsqlConnection(dcs);
                await dbh.OpenAsync();

                foreach (XmlNode article in articles)
                {
                    string id = article.SelectSingleNode("id").InnerText;
                    string link = article.SelectSingleNode("link").InnerText;
                    DateTime date = Convert.ToDateTime(article.SelectSingleNode("date").InnerText);
                    string title = article.SelectSingleNode("title").InnerText;
                    string story = article.SelectSingleNode("story").InnerText;

                    await using(var insert = new NpgsqlCommand("insert into articles values (@id,@date,@link,@title,@story)",dbh))
                    {
                        insert.Parameters.AddWithValue("id", id);
                        insert.Parameters.AddWithValue("date", date);
                        insert.Parameters.AddWithValue("link", link);
                        insert.Parameters.AddWithValue("title", title);
                        insert.Parameters.AddWithValue("story", story);
                        insert.ExecuteNonQuery();
                    }
                    Console.WriteLine("Stored article: {0}", title);
                }

                await dbh.CloseAsync();
            }
            catch(NpgsqlException e)
            {
                Console.WriteLine(e);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
 
        }
    }
}
