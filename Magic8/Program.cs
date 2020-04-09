using System;
using Eto.Drawing;
using Eto.Forms;

namespace Magic8
{
    internal static class Program
    {
        private static readonly string[] AnswerList =
        {
            "IT IS\nCERTAIN",
            "IT IS\nDECIDEDLY\nSO",
            "YES\nDEFINITELY",
            "YOU\nMAY RELY\nON IT",
            "AS I\nSEE IT,\nYES",
            "MOST\nLIKELY",
            "YES",
            "REPLY HAZY,\nTRY AGAIN",
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

        private static readonly Random Rnd = new Random();

        private static Bitmap Generate(IDisposable prev)
        {
            prev?.Dispose();
            Rectangle size = new Rectangle(0, 0, 400, 400);
            Bitmap bmp = new Bitmap(size.Size, PixelFormat.Format32bppRgb);
            Graphics g = new Graphics(bmp);
            //Background
            g.Clear(Colors.White);
            //Main Circle
            g.FillEllipse(Brushes.Black, size);
            //Center circle
            size.Width /= 2;
            size.Height /= 2;
            size.X = size.Width / 2;
            size.Y = size.Height / 2;
            g.DrawEllipse(new Pen(Color.FromArgb(100, 80, 80, 80), 6), size);
            //Triangle
            PointF center = new PointF(size.X + size.Width / 2, size.Y + size.Height / 2);
            float radius = size.Width / 2f;
            g.FillPolygon(Brushes.Blue, new PointF(center.X - 0.866f * radius, center.Y - 0.5f * radius),
                new PointF(center.X + 0.866f * radius, center.Y - 0.5f * radius),
                new PointF(center.X, center.Y + radius));
            Font font = SystemFonts.Default();
            font = new Font(font.Family, font.Size * (180f / g.MeasureString(font, "QWERTBTESTSTR").Width));
            string answer = AnswerList[Rnd.Next(AnswerList.Length)];
            size.Top = (int) Math.Round(size.Center.Y - g.MeasureString(font, answer).Height / 2);
            g.DrawText(font, Brushes.White, size, answer, FormattedTextWrapMode.Word, FormattedTextAlignment.Center);
            g.Flush();
            g.Dispose();
            return bmp;
        }

        public static void Main()
        {
            using Application app = new Application();
            using ImageView view = new ImageView();
            using Form f = new Form {Size = new Size(400, 410), Content = view};
            view.Image = Generate(null);
            f.MouseDown += (sender, e) => view.Image = Generate(view.Image);
            view.MouseDown += (sender, e) => view.Image = Generate(view.Image);
            f.Closed += (sender, e) => Environment.Exit(0);
            app.Run(f);
        }
    }
}