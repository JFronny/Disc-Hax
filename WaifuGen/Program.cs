using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WaifuGen
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Random rnd = new Random();
            while (true)
            {
                using (Form f = new Form())
                {
                    f.StartPosition = FormStartPosition.CenterScreen;
                    using (WebClient c = new WebClient())
                    {
                        using (Stream s = c.OpenRead("https://www.thiswaifudoesnotexist.net/example-" + rnd.Next(6000).ToString() + ".jpg"))
                        {
                            Bitmap img = (Bitmap)Image.FromStream(s);
                            f.BackgroundImage = img;
                            f.BackgroundImageLayout = ImageLayout.Zoom;
                            SetFormSize(f, img.Size);
                        }
                    }
                    f.ShowDialog();
                }
            }
        }

        static void SetFormSize(Form f, Size s)
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
                Size tmp = f.Size;
                f.Width = Math.Min(f.Width, screen.Width);
                f.Height = (int)Math.Round((f.Width - WidthAdd) * ratio) + HeightAdd;
            };
        }
    }
}
