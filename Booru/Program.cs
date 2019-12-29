#region

using System;
using BooruSharp.Booru;
using BooruSharp.Search.Post;

#endregion

namespace BooruT
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.Write("Allow NSFW? (Y/N):");
            Booru booru = Console.ReadKey().Key == ConsoleKey.Y ? (Booru) new Rule34() : new Gelbooru();
            Console.Write(Environment.NewLine);
            SearchResult result = booru.GetRandomImage(args).GetAwaiter().GetResult();

            Console.WriteLine(
                $"Image preview URL: {result.previewUrl}{Environment.NewLine}Image URL: {result.fileUrl}{Environment.NewLine}Image Source: {result.score}{Environment.NewLine}Image rating: {result.rating}{Environment.NewLine}Tags on the image: {string.Join(", ", result.tags)}");
            Console.ReadKey();
        }
    }
}