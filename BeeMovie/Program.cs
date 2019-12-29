#region

using System;
using System.Net;

#endregion

namespace BeeMovie
{
    internal class Program
    {
        private static readonly Random rnd = new Random();

        private static void Main(string[] args)
        {
            string[] Quotes;
            using (WebClient client = new WebClient())
            {
                Quotes = client
                    .DownloadString(
                        "http://www.script-o-rama.com/movie_scripts/a1/bee-movie-script-transcript-seinfeld.html")
                    .Split(new[] {"<pre>"}, StringSplitOptions.None)[1]
                    .Split(new[] {"</pre>"}, StringSplitOptions.None)[0]
                    .Split(new[] {"\n\n  \n"}, StringSplitOptions.None);
            }

            Quotes[0] = Quotes[0].Replace("  \n  \n", "");
            while (true)
            {
                Console.Clear();
                int q = rnd.Next(Quotes.Length - 2);
                Console.WriteLine((Quotes[q] + "\n\n" + Quotes[q + 1] + "\n\n" + Quotes[q + 2]).Replace("\n", "\r\n"));
                Console.ReadKey();
            }
        }
    }
}