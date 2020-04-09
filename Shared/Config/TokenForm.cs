using System.Xml.Linq;
using Eto.Drawing;
using Eto.Forms;

namespace Shared.Config
{
    public static class TokenForm
    {
        public static void Show(string discordToken, string currConvToken, string perspectiveToken,
            bool overwriteXml = false)
        {
            using PasswordBox discordBox = new PasswordBox {PasswordChar = '●', Text = discordToken};
            using PasswordBox currConvBox = new PasswordBox {PasswordChar = '●', Text = currConvToken};
            using PasswordBox perspectiveBox = new PasswordBox {PasswordChar = '●', Text = perspectiveToken};
            using Dialog dlg = new Dialog
            {
                Content = new TableLayout
                {
                    Spacing = new Size(5, 5),
                    Padding = new Padding(10, 10, 10, 10),
                    Rows =
                    {
                        new TableRow(
                            new Label { Text = "Discord:" },
                            discordBox
                        ),
                        new TableRow(
                            new Label { Text = "CurrConv:" },
                            currConvBox
                        ),
                        new TableRow(
                            new Label { Text = "Perspective:" },
                            perspectiveBox
                        )
                    }
                }
            };
            dlg.ShowModal();
            if (overwriteXml)
            {
                TokenManager.SaveXe(new XElement("container",
                    new XElement("discord", discordBox.Text),
                    new XElement("currencyconverterapi", currConvBox.Text),
                    new XElement("perspective", perspectiveBox.Text)));
            }
            else
            {
                TokenManager.DiscordToken = discordBox.Text;
                TokenManager.CurrencyconverterapiToken = currConvBox.Text;
                TokenManager.PerspectiveToken = perspectiveBox.Text;
            }
        }
    }
}