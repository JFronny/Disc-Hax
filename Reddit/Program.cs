using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Reddit
{
    internal class Program
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
            using WebClient client = new WebClient();
            while (true)
            {
                string res =
                    client.DownloadString($"https://www.reddit.com/r/{subreddit}/{(top ? "top" : "random")}/.json");
                JToken jToken = (top ? JObject.Parse(res) : JArray.Parse(res)[0])["data"]["children"][0]["data"];
                Console.WriteLine($"https://www.reddit.com{jToken["permalink"].Value<string>()}");
                if (jToken["over_18"].Value<bool>() && !nsfw)
                    continue;
                using Form f = new Form
                {
                    Text =
                        $"{jToken["author"].Value<string>()}: {jToken["title"].Value<string>()} - Score: {jToken["score"].Value<int>()}",
                    StartPosition = FormStartPosition.CenterScreen
                };
                try
                {
                    using Stream s = client.OpenRead(jToken["url"].Value<string>());
                    Bitmap img = (Bitmap) Image.FromStream(s);
                    f.BackgroundImage = img;
                    f.BackgroundImageLayout = ImageLayout.Zoom;
                    SetFormSize(f, img.Size);
                }
                catch
                {
                    try
                    {
                        string str = jToken["media"]["reddit_video"]["fallback_url"].Value<string>();
                        LinkLabel l = new LinkLabel
                        {
                            Text = str, AutoSize = false, Dock = DockStyle.Fill,
                            Links = {new LinkLabel.Link(0, str.Length, str)}
                        };
                        l.Click += (sender, e) => { Process.Start(str); };
                        f.Controls.Add(l);
                        SetFormSize(f, l.Size);
                    }
                    catch
                    {
                        Label l = new Label
                            {Text = jToken["selftext"].Value<string>(), AutoSize = false, Dock = DockStyle.Fill};
                        f.Controls.Add(l);
                        SetFormSize(f, l.Size);
                    }
                }
                f.ShowDialog();
            }
        }

        private static void SetFormSize(Form f, Size s)
        {
            double ratio = (double) s.Height / s.Width;
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
                f.Height = (int) Math.Round((f.Width - WidthAdd) * ratio) + HeightAdd;
            };
        }
    }
}