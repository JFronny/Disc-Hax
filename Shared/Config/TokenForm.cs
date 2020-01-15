using System;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Shared.Config
{
    public partial class TokenForm : Form
    {
        private readonly bool _overwriteXml;

        public TokenForm(string discordToken, string currencyconverterapiToken, string perspectiveToken,
            bool overwriteXml = false)
        {
            InitializeComponent();
            discordBox.Text = discordToken;
            currConvBox.Text = currencyconverterapiToken;
            perspectiveBox.Text = perspectiveToken;
            _overwriteXml = overwriteXml;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (_overwriteXml)
            {
                TokenManager.SaveXE(new XElement("container",
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
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}