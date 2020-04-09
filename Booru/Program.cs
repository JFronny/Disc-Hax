using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using BooruSharp.Booru;
using BooruSharp.Search.Post;
using Eto.Drawing;
using Eto.Forms;

namespace Booru
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            Console.Write("Allow NSFW? (Y/N):");
            ABooru booru = Console.ReadKey().Key == ConsoleKey.Y ? (ABooru) new Rule34() : new Safebooru();
            Console.Write(Environment.NewLine);
            using WebClient c = new WebClient();
            using Application app = new Application();
            using ImageView view = new ImageView();
            using Form f = new Form {Content = view};
            f.Closed += (sender, e) => Environment.Exit(0);
            f.MouseDown += async (sender, e) => await Modify(f, view, c, booru, args);
            view.MouseDown += async (sender, e) => await Modify(f, view, c, booru, args);
            Modify(f, view, c, booru, args);
            app.Run(f);
        }

        private static async Task Modify(Form f, ImageView view, WebClient c, ABooru booru, string[] args)
        {
            SearchResult result = await booru.GetRandomImageAsync(args);
            Console.Clear();
            Console.WriteLine($"Image preview URL: {result.previewUrl}");
            Console.WriteLine($"Image URL: {result.fileUrl}");
            Console.WriteLine($"Image Source: {result.score}");
            Console.WriteLine($"Image rating: {result.rating}");
            Console.WriteLine($"Tags on the image: {string.Join(", ", result.tags)}");
            f.Title =
                $"{result.fileUrl} - {result.rating}";
            await using Stream s = c.OpenRead(result.fileUrl);
            view.Image = new Bitmap(s);
            SetFormSize(f, view.Image.Size);
        }

        private static void SetFormSize(Form f, SizeF s)
        {
            double ratio = (double) s.Height / s.Width;
            RectangleF screen = Screen.PrimaryScreen.WorkingArea;
            if (s.Width > screen.Width)
            {
                s.Height = s.Width / screen.Width * s.Height;
                s.Width = screen.Width;
            }

            if (s.Height > screen.Height)
            {
                s.Width = s.Height / screen.Height * s.Width;
                s.Height = screen.Height;
            }

            f.Size = new Size((int) Math.Round(s.Width), (int) Math.Round(s.Height));
            f.SizeChanged += (sender, e) =>
            {
                f.Width = (int) Math.Round(Math.Min(f.Width, screen.Width));
                f.Height = (int) Math.Round(f.Width * ratio);
            };
        }
    }
}