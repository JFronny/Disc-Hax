using System;
using Eto.Drawing;
using QRCoder;

namespace Shared.QR
{
    public class QRCode : IDisposable
    {
        protected QRCodeData QrCodeData { get; set; }

        /// <summary>
        /// Set a QRCodeData object that will be used to generate QR code. Used in COM Objects connections
        /// </summary>
        /// <param name="data">Need a QRCodeData object generated by QRCodeGenerator.CreateQrCode()</param>
        virtual public void SetQRCodeData(QRCodeData data) {
            QrCodeData = data;
        }

        public void Dispose()
        {
            QrCodeData?.Dispose();
            QrCodeData = null;
        }
        /// <summary>
        /// Constructor without params to be used in COM Objects connections
        /// </summary>
        public QRCode() { }

        public QRCode(QRCodeData data)
        {
            QrCodeData = data;}

        public Bitmap GetGraphic(int pixelsPerModule)
        {
            return GetGraphic(pixelsPerModule, Colors.Black, Colors.White, true);
        }

        private Bitmap GetGraphic(int pixelsPerModule, Color darkColor, Color lightColor, bool drawQuietZones = true)
        {
            int size = (QrCodeData.ModuleMatrix.Count - (drawQuietZones ? 0 : 8)) * pixelsPerModule;
            int offset = drawQuietZones ? 0 : 4 * pixelsPerModule;

            var bmp = new Bitmap(size, size, PixelFormat.Format32bppRgb);
            using var gfx = new Graphics(bmp);
            using var lightBrush = new SolidBrush(lightColor);
            using var darkBrush = new SolidBrush(darkColor);
            for (int x = 0; x < size + offset; x = x + pixelsPerModule)
            for (int y = 0; y < size + offset; y = y + pixelsPerModule)
            {
                bool module = QrCodeData.ModuleMatrix[(y + pixelsPerModule) / pixelsPerModule - 1][(x + pixelsPerModule) / pixelsPerModule - 1];

                gfx.FillRectangle(module ? darkBrush : lightBrush,
                    new Rectangle(x - offset, y - offset, pixelsPerModule, pixelsPerModule));
            }

            gfx.Flush();

            return bmp;
        }

        public Bitmap GetGraphic(int pixelsPerModule, Color darkColor, Color lightColor, int iconSizePercent=15, int iconBorderWidth = 6, bool drawQuietZones = true)
        {
            int size = (QrCodeData.ModuleMatrix.Count - (drawQuietZones ? 0 : 8)) * pixelsPerModule;
            int offset = drawQuietZones ? 0 : 4 * pixelsPerModule;

            Bitmap bmp = new Bitmap(size, size, PixelFormat.Format32bppRgba);

            using Graphics gfx = new Graphics(bmp);
            using SolidBrush lightBrush = new SolidBrush(lightColor);
            using SolidBrush darkBrush = new SolidBrush(darkColor);
            gfx.ImageInterpolation = ImageInterpolation.High;
            gfx.Clear(lightColor);

            GraphicsPath iconPath = null;
            float iconDestWidth = 0, iconDestHeight = 0, iconX = 0, iconY = 0;

            for (int x = 0; x < size + offset; x = x + pixelsPerModule)
            for (int y = 0; y < size + offset; y = y + pixelsPerModule)
            {
                bool module = QrCodeData.ModuleMatrix[(y + pixelsPerModule) / pixelsPerModule - 1][(x + pixelsPerModule) / pixelsPerModule - 1];

                if (module)
                {
                    Rectangle r = new Rectangle(x - offset, y - offset, pixelsPerModule, pixelsPerModule);

                    gfx.FillRectangle(darkBrush, r);
                }
                else
                    gfx.FillRectangle(lightBrush, new Rectangle(x - offset, y - offset, pixelsPerModule, pixelsPerModule));
            }

            gfx.Flush();

            return bmp;
        }

        private GraphicsPath CreateRoundedRectanglePath(RectangleF rect, int cornerRadius)
        {
            GraphicsPath roundedRect = new GraphicsPath();
            roundedRect.AddArc(rect.X, rect.Y, cornerRadius * 2, cornerRadius * 2, 180, 90);
            roundedRect.AddLine(rect.X + cornerRadius, rect.Y, rect.Right - cornerRadius * 2, rect.Y);
            roundedRect.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y, cornerRadius * 2, cornerRadius * 2, 270, 90);
            roundedRect.AddLine(rect.Right, rect.Y + cornerRadius * 2, rect.Right, rect.Y + rect.Height - cornerRadius * 2);
            roundedRect.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y + rect.Height - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0, 90);
            roundedRect.AddLine(rect.Right - cornerRadius * 2, rect.Bottom, rect.X + cornerRadius * 2, rect.Bottom);
            roundedRect.AddArc(rect.X, rect.Bottom - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 90, 90);
            roundedRect.AddLine(rect.X, rect.Bottom - cornerRadius * 2, rect.X, rect.Y + cornerRadius * 2);
            roundedRect.CloseFigure();
            return roundedRect;
        }
    }
}