using System;
using System.IO;
using System.Net;
using Eto.Drawing;
using Eto.Forms;

namespace WaifuGen
{
    internal static class Program
    {
        private static readonly Random Rnd = new Random();

        [STAThread]
        private static void Main()
        {
            using Application app = new Application();
            using ImageView view = new ImageView();
            using Form f = new Form {Content = view};
            using WebClient c = new WebClient();
            f.Closed += (sender, e) => Environment.Exit(0);
            f.MouseDown += (sender, e) => Update(f, view, c);
            view.MouseDown += (sender, e) => Update(f, view, c);
            Update(f, view, c);
            app.Run(f);
        }

        private static void Update(Form f, ImageView view, WebClient c)
        {
            int image = -1;
            string address = "";
            try
            {
                try
                {
                    view.Image?.Dispose();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                image = Rnd.Next(6000);
                address = $"https://www.thiswaifudoesnotexist.net/example-{image}.jpg";
                using Stream s = c.OpenRead(address);
                f.Title = $"{image}/6000";
                view.Image = new Bitmap(s);
                SetFormSize(f, view.Image.Size);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to load {image} ({address})");
                Console.WriteLine(e);
                Update(f, view, c);
            }
        }

        private static void SetFormSize(Form f, SizeF s)
        {
            double ratio = (double) s.Height / s.Width;
            RectangleF screen = Screen.PrimaryScreen.WorkingArea;
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

            f.Size = new Size((int) Math.Round(s.Width), (int) Math.Round(s.Height));
            f.SizeChanged += (sender, e) =>
            {
                f.Width = (int) Math.Round(Math.Min(f.Width, screen.Width));
                f.Height = (int) Math.Round(f.Width * ratio);
            };
        }
    }
}