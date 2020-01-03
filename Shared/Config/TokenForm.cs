using System;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Shared.Config
{
    public partial class TokenForm : Form
    {
        private readonly bool overwriteXML;

        public TokenForm(string DiscordToken, string CurrencyconverterapiToken, bool overwriteXML = false)
        {
            InitializeComponent();
            discordBox.Text = DiscordToken;
            currConvBox.Text = CurrencyconverterapiToken;
            this.overwriteXML = overwriteXML;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (overwriteXML)
            {
                TokenManager.SaveXE(new XElement("container",
                    new XElement("discord", discordBox.Text),
                    new XElement("currencyconverterapi", currConvBox.Text)));
            }
            else
            {
                TokenManager.DiscordToken = discordBox.Text;
                TokenManager.CurrencyconverterapiToken = currConvBox.Text;
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}