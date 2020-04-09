using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Chan.Net;
using Chan.Net.JsonModel;
using Eto.Drawing;
using Eto.Forms;

namespace chanI
{
    internal static class Program
    {
        private static int _index;
        private static List<Thread> _th;

        private static void Main(string[] args)
        {
            Application app = new Application();
            Console.WriteLine("Fetching data from 4chan...");
            List<BoardInfo> m = JsonDeserializer.Deserialize<BoardListModel>(Internet
                    .DownloadString(@"https://a.4cdn.org/boards.json").ConfigureAwait(false).GetAwaiter().GetResult())
                .boards;
            Board b = new Board(args != null && args.Length > 0
                ? args[0]
                : ConsoleChoose("Choose a board!", m.Select(s => s.ShortName).ToArray(),
                    m.Select(s => s.Title).ToArray()));
            System.Threading.Thread.Sleep(1000);
            _th = b.GetThreads().ToList();
            using Form f = new Form();
            using WebClient c = new WebClient();
            using Label l = new Label();
            using ImageView view = new ImageView();
            f.MouseDown += (sender, e) => LoadNextImage(c, f, l, view);
            l.MouseDown += (sender, e) => LoadNextImage(c, f, l, view);
            view.MouseDown += (sender, e) => LoadNextImage(c, f, l, view);
            LoadNextImage(c, f, l, view);
            app.Run(f);
            System.Threading.Thread.Sleep(1000);
        }

        private static void LoadNextImage(WebClient c, Form f, Label l, ImageView view)
        {
            if (_index >= _th.Count)
                f.Close();
            else
            {
                l.Text = "Loading content...";
                f.Content = l;
                Console.WriteLine(
                    $"https://boards.4channel.org/{_th[_index].Board.BoardId}/thread/{_th[_index].PostNumber}");

                f.Title =
                    $"{_th[_index].Name}: {(string.IsNullOrWhiteSpace(_th[_index].Subject) ? "Untitled" : _th[_index].Subject)} - {_index + 1}/{_th.Count}";
                try
                {
                    using Stream s = c.OpenRead(_th[_index].Image.Image);
                    Bitmap img = new Bitmap(s);
                    view.Image = img;
                    f.Content = view;
                    SetFormSize(f, img.Size);
                }
                catch
                {
                    try
                    {
                        l.Size = new Size(200, 100);
                        l.Text = _th[_index].Message;

                        SetFormSize(f, l.Size);
                    }
                    catch
                    {
                        LoadNextImage(c, f, l, view);
                    }
                }
                _index++;
            }
        }

        private static string ConsoleChoose(string prompt, string[] choices, string[]? choiceDesc = null)
        {
            if (choiceDesc != null && choiceDesc.Length != choices.Length)
                throw new ArgumentOutOfRangeException(
                    "choiceDesc[] needs to be of the same Length as choices[] or null");
            Console.WriteLine(prompt);
            for (int i = 0; i < choices.Length; i++)
                Console.WriteLine(choices[i] +
                                  (choiceDesc == null ? "" : new string(' ', 8 - choices[i].Length) + choiceDesc[i]));
            string choice = "";
            Console.Write("\r\n");
            while (!choices.Contains(choice))
            {
                Console.CursorTop--;
                Console.WriteLine(new string(' ', 10));
                Console.CursorTop--;
                Console.Write("-> ");
                choice = Console.ReadLine();
            }

            return choice;
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