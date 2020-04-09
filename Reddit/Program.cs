using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using Eto.Drawing;
using Eto.Forms;
using Newtonsoft.Json.Linq;

namespace Reddit
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Subreddit (empty for \"random\"):");
            string subreddit = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(subreddit))
                subreddit = "random";
            Console.WriteLine("Use top instead of random? (y/n)");
            bool top = Console.ReadKey().KeyChar == 'y';
            Console.WriteLine("NSFW? (y/n)");
            bool nsfw = Console.ReadKey().KeyChar == 'y';
            using Application app = new Application();
            using WebClient c = new WebClient();
            using Form f = new Form();
            using ImageView view = new ImageView();
            using Label l = new Label();
            f.MouseDown += (sender, e) => Update(f, view, l, c, subreddit, top, nsfw);
            view.MouseDown += (sender, e) => Update(f, view, l, c, subreddit, top, nsfw);
            l.MouseDown += (sender, e) => Update(f, view, l, c, subreddit, top, nsfw);
            f.Closed += (sender, e) => Environment.Exit(0);
            Update(f, view, l, c, subreddit, top, nsfw);
            app.Run(f);
        }

        private static void Update(Form f, ImageView view, Label l, WebClient c, string subreddit, bool top, bool nsfw)
        {
            try
            {
                bool found = false;
                JToken jToken = null;
                while (!found)
                {
                    string res =
                        c.DownloadString($"https://www.reddit.com/r/{subreddit}/{(top ? "top" : "random")}/.json");
                    jToken = (top ? JObject.Parse(res) : JArray.Parse(res)[0])["data"]["children"][0]["data"];
                    Console.WriteLine($"https://www.reddit.com{jToken["permalink"].Value<string>()}");
                    if (!jToken["over_18"].Value<bool>() || nsfw)
                        found = true;
                }
                f.Title =
                    $"{jToken["author"].Value<string>()}: {jToken["title"].Value<string>()} - Score: {jToken["score"].Value<int>()}";
                try
                {
                    using Stream s = c.OpenRead(jToken["url"].Value<string>());
                    Bitmap img = new Bitmap(s);
                    view.Image = img;
                    f.Content = view;
                    SetFormSize(f, img.Size);
                }
                catch
                {
                    try
                    {
                        string str = jToken["media"]["reddit_video"]["fallback_url"].Value<string>();
                        l.TextColor = SystemColors.LinkText;
                        l.Text = str;
                        l.MouseDown += (sender, e) =>
                        {
                            try
                            {
                                Process.Start(str);
                            }
                            catch
                            {
                            }
                        };
                        f.Content = l;
                        SetFormSize(f, l.Size);
                    }
                    catch
                    {
                        l.TextColor = SystemColors.ControlText;
                        l.Text = jToken["selftext"].Value<string>();
                        l.MouseDown += (sender, e) => { };
                        f.Content = l;
                        SetFormSize(f, l.Size);
                    }
                }
            }
            catch
            {
                Update(f, view, l, c, subreddit, top, nsfw);
            }
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