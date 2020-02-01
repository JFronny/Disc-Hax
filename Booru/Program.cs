using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;
using BooruSharp.Booru;
using BooruSharp.Search.Post;

namespace BooruT
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.Write("Allow NSFW? (Y/N):");
            Booru booru = Console.ReadKey().Key == ConsoleKey.Y ? (Booru) new Rule34() : new Gelbooru();
            Console.Write(Environment.NewLine);
            while (true)
            {
                SearchResult result = booru.GetRandomImage(args).GetAwaiter().GetResult();
                Console.Clear();
                Console.WriteLine($"Image preview URL: {result.previewUrl}");
                Console.WriteLine($"Image URL: {result.fileUrl}");
                Console.WriteLine($"Image Source: {result.score}");
                Console.WriteLine($"Image rating: {result.rating}");
                Console.WriteLine($"Tags on the image: {string.Join(", ", result.tags)}");
                using Form f = new Form();
                f.Text =
                    $"{result.fileUrl} - {result.rating}";
                f.StartPosition = FormStartPosition.CenterScreen;
                using WebClient c = new WebClient();
                using Stream s = c.OpenRead(result.fileUrl);
                using Bitmap img = (Bitmap) Image.FromStream(s);
                f.BackgroundImage = img;
                f.BackgroundImageLayout = ImageLayout.Zoom;
                SetFormSize(f, img.Size);
                f.ShowDialog();
            }
        }

        private static void SetFormSize(Form f, Size s)
        {
            double ratio = (double)s.Height / s.Width;
            Rectangle screen = Screen.PrimaryScreen.WorkingArea;
            if (s.Width > screen.Width)
            {
                s.Height = (s.Width / screen.Width) * s.Height;
                s.Width = screen.Width;
            }

            if (s.Height > screen.Height)
            {
                s.Width = (s.Height / screen.Height) * s.Width;
                s.Height = screen.Height;
            }

            int WidthAdd = 16;
            int HeightAdd = 39;
            s.Width += WidthAdd;
            s.Height += HeightAdd;
            f.Size = s;
            f.SizeChanged += (sender, e) =>
            {
                f.Width = Math.Min(f.Width, screen.Width);
                f.Height = (int)Math.Round((f.Width - WidthAdd) * ratio) + HeightAdd;
            };
        }
    }
}