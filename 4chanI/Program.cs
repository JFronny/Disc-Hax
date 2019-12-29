using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using Chan.Net;
using Chan.Net.JsonModel;
using Thread = System.Threading.Thread;

namespace Cleverbot
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Fetching data from 4chan...");
            List<BoardInfo> m = JsonDeserializer.Deserialize<BoardListModel>(Internet
                    .DownloadString(@"https://a.4cdn.org/boards.json").ConfigureAwait(false).GetAwaiter().GetResult())
                .boards;
            Board b = new Board(args != null && args.Length > 0
                ? args[0]
                : consoleChoose("Choose a board!", m.Select(s => s.ShortName).ToArray(),
                    m.Select(s => s.Title).ToArray()));
            Thread.Sleep(1000);
            List<Chan.Net.Thread> th = b.GetThreads().ToList();
            for (int i = 0; i < th.Count; i++)
            {
                Console.WriteLine($"https://boards.4channel.org/{th[i].Board.BoardId}/thread/{th[i].PostNumber}");
                using (Form f = new Form())
                {
                    f.Text =
                        $"{th[i].Name}: {(string.IsNullOrWhiteSpace(th[i].Subject) ? "Untitled" : th[i].Subject)} - {i + 1}/{th.Count}";
                    f.StartPosition = FormStartPosition.CenterScreen;
                    using (WebClient c = new WebClient())
                    {
                        try
                        {
                            using (Stream s = c.OpenRead(th[i].Image.Image))
                            {
                                Bitmap img = (Bitmap) Image.FromStream(s);
                                f.BackgroundImage = img;
                                f.BackgroundImageLayout = ImageLayout.Zoom;
                                SetFormSize(f, img.Size);
                            }
                        }
                        catch
                        {
                            try
                            {
                                Label l = new Label();
                                l.Text = th[i].Message;
                                l.AutoSize = false;
                                l.Dock = DockStyle.Fill;
                                f.Controls.Add(l);
                                SetFormSize(f, l.Size);
                            }
                            catch
                            {
                            }
                        }
                    }

                    f.ShowDialog();
                }

                Thread.Sleep(1000);
            }
        }

        private static string consoleChoose(string prompt, string[] choices, string[] choiceDesc = null)
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
                Size tmp = f.Size;
                f.Width = Math.Min(f.Width, screen.Width);
                f.Height = (int) Math.Round((f.Width - WidthAdd) * ratio) + HeightAdd;
            };
        }
    }
}