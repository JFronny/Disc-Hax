using System;
using System.Net;

namespace BeeMovie
{
    internal class Program
    {
        private static readonly Random Rnd = new Random();

        private static void Main()
        {
            string[] quotes;
            using (WebClient client = new WebClient())
                quotes = client
                    .DownloadString(
                        "http://www.script-o-rama.com/movie_scripts/a1/bee-movie-script-transcript-seinfeld.html")
                    .Split(new[] {"<pre>"}, StringSplitOptions.None)[1]
                    .Split(new[] {"</pre>"}, StringSplitOptions.None)[0]
                    .Split(new[] {"\n\n  \n"}, StringSplitOptions.None);

            quotes[0] = quotes[0].Replace("  \n  \n", "");
            while (true)
            {
                Console.Clear();
                int q = Rnd.Next(quotes.Length - 2);
                Console.WriteLine((quotes[q] + "\n\n" + quotes[q + 1] + "\n\n" + quotes[q + 2]).Replace("\n", "\r\n"));
                Console.ReadKey();
            }
        }
    }
}