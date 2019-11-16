using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booru
{
    class Program
    {
        static void Main(string[] args)
        {
            BooruSharp.Booru.Gelbooru booru = new BooruSharp.Booru.Gelbooru();
            BooruSharp.Search.Post.SearchResult result = booru.GetRandomImage("hibiki_(kantai_collection)", "school_swimsuit").GetAwaiter().GetResult();

            Console.WriteLine("Image preview URL: " + result.previewUrl + Environment.NewLine +
                              "Image URL: " + result.fileUrl + Environment.NewLine +
                              "Image is safe: " + (result.rating == BooruSharp.Search.Post.Rating.Safe) + Environment.NewLine +
                              "Tags on the image: " + String.Join(", ", result.tags));
            Console.ReadKey();
        }
    }
}
