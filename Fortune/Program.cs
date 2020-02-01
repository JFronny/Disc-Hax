using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Octokit;

namespace Fortune
{
    internal class Program
    {
        private static readonly Random rnd = new Random();

        private static void Main()
        {
            Console.Write("off? (y/n): ");
            bool off = Console.ReadKey().Key == ConsoleKey.Y;
            Console.WriteLine();
            GitHubClient cli = new GitHubClient(new ProductHeaderValue("DiscHax"));
            IEnumerable<RepositoryContent> files = cli.Repository.Content.GetAllContents("shlomif", "fortune-mod",
                off ? "fortune-mod/datfiles/off/unrotated" : "fortune-mod/datfiles").GetAwaiter().GetResult();
            IEnumerable<string> disallowednames = new[] { "CMakeLists.txt", null };
            IEnumerable<RepositoryContent> filteredFiles =
                files.Where(s => s.Type == ContentType.File && !disallowednames.Contains(s.Name));
            IEnumerable<string> cookies =
                filteredFiles.Where(s => !disallowednames.Contains(s.Name)).Select(s => s.DownloadUrl);
            IEnumerable<string> contents;
            using (WebClient client = new WebClient())
            {
                contents = cookies.Select(s =>
                {
                    Console.WriteLine($"Downloading: {s}");
                    return client.DownloadString(s);
                });
            }
            string[] quotes = contents.SelectMany(s => s.Split(new[] { "\n%\n" }, StringSplitOptions.None)).ToArray();
            while (true)
            {
                Console.Clear();
                Console.WriteLine(quotes[rnd.Next(quotes.Length)]);
                Console.ReadKey();
            }
        }
    }
}