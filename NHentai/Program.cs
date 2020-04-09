using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using NHentaiSharp.Search;

namespace NHentai
{
    class Program
    {
        private static async Task Main(string[] args)
        {
            Random rnd = new Random();
            while (true)
            {
                Console.WriteLine("Loading");
                GalleryElement doujinshi;
                if (args.Length == 0)
                {
                    SearchResult res = await NHentaiSharp.Core.SearchClient.SearchAsync(rnd.Next(0, 20));
                    doujinshi = res.elements[rnd.Next(0, res.elements.Length)];
                }
                else
                {
                    SearchResult res = await NHentaiSharp.Core.SearchClient.SearchWithTagsAsync(args);
                    Console.WriteLine("Loaded res1");
                    res = await NHentaiSharp.Core.SearchClient.SearchWithTagsAsync(args, rnd.Next(res.numPages) + 1);
                    Console.WriteLine("Loaded res2");
                    doujinshi = res.elements[rnd.Next(0, res.elements.Length)];
                    Console.WriteLine("Loaded res3");
                }
                Console.Clear();
                Console.WriteLine(doujinshi.url);
                Console.WriteLine(doujinshi.uploadDate);
                Console.WriteLine($"{doujinshi.japaneseTitle} - ({doujinshi.prettyTitle})");
                Console.WriteLine(doujinshi.thumbnail.imageUrl);
                Console.ReadKey();
            }
        }
    }
}