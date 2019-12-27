using System;
using System.Drawing;
using System.Windows.Forms;

namespace Magic8
{
    internal class Program
    {
        private static readonly string[] answerList =
        {
            "IT IS\nCERTAIN",
            "IT IS\nDECIDEDLY\nSO",
            "YES\nDEFINITELY",
            "YOU\nMAY RELY\nON IT",
            "AS I\nSEE IT,\nYES",
            "MOST\nLIKELY",
            "YES",
            "REPLY\nHAZY,\nTRY AGAIN",
            "ASK\nAGAIN\nLATER",
            "DON'T\nCOUNT\nON IT",
            "VERY\nDOUBTFUL",
            "WITHOUT A\nDOUBT",
            "OUTLOOK\nGOOD",
            "SIGNS\nPOINT TO\nYES",
            "BETTER\nNOT TELL\nYOU NOW",
            "CANNOT\nPREDICT\nNOW",
            "CONCENTRATE\nAND ASK\nAGAIN",
            "MY REPLY\nIS NO",
            "MY SOURCES\nSAY NO",
            "OUTLOOK\nNOT SO\nGOOD"
        };

        public static void Main(string[] args)
        {
            while (true)
            {
                Rectangle size = new Rectangle(0, 0, 400, 400);
                Bitmap bmp = new Bitmap(size.Width, size.Height);
                Graphics g = Graphics.FromImage(bmp);
                Random rnd = new Random();
                //Background
                g.Clear(Color.White);
                //Main Circle
                g.FillEllipse(Brushes.Black, size);
                //Center circle
                size.Width /= 2;
                size.Height /= 2;
                size.X = size.Width / 2;
                size.Y = size.Height / 2;
                g.DrawEllipse(new Pen(Color.FromArgb(100, 80, 80, 80), 6), size);
                //Triangle
                PointF center = new PointF(size.X + (size.Width / 2), size.Y + (size.Height / 2));
                float radius = size.Width / 2;
                g.FillPolygon(Brushes.Blue, new[]
                {
                    new PointF(center.X - (0.866f * radius), center.Y - (0.5f * radius)),
                    new PointF(center.X + (0.866f * radius), center.Y - (0.5f * radius)),
                    new PointF(center.X, center.Y + radius)
                });
                //Text
                g.DrawString(answerList[rnd.Next(answerList.Length)], SystemFonts.DefaultFont, Brushes.White, size,
                    new StringFormat
                    {
                        LineAlignment = StringAlignment.Center,
                        Alignment = StringAlignment.Center
                    });
                //Save
                g.Flush();
                g.Dispose();
                Form f = new Form();
                f.BackgroundImage = bmp;
                f.BackgroundImageLayout = ImageLayout.Center;
                f.Size = new Size(400, 410);
                f.ShowDialog();
                bmp.Dispose();
            }
        }
    }
}