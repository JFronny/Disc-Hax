#region

using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using CC_Functions.Misc;
using Microsoft.VisualBasic;

#endregion

namespace Shared.Config
{
    public static class TokenManager
    {
        private static readonly string containerFile =
            Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Cfgs", "keys.secure");

        public static string DiscordToken
        {
            get => LoadXE().Element("discord").Value;
            set
            {
                XElement el = LoadXE();
                el.Element("discord").Value = value;
                SaveXE(el);
            }
        }

        public static string CurrencyconverterapiToken
        {
            get => LoadXE().Element("currencyconverterapi").Value;
            set
            {
                XElement el = LoadXE();
                el.Element("currencyconverterapi").Value = value;
                SaveXE(el);
            }
        }

        private static XElement LoadXE()
        {
            if (!Directory.Exists(Path.GetDirectoryName(containerFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(containerFile));
            if (!File.Exists(containerFile))
                SaveXE(new XElement("container",
                    new XElement("discord", Interaction.InputBox("Please enter your discord Token")),
                    new XElement("currencyconverterapi",
                        Interaction.InputBox("Please enter your currencyconverterapi.com Token"))
                ));
            byte[] bytes = HID.DecryptLocal(File.ReadAllBytes(containerFile));
            return XElement.Parse(Encoding.UTF8.GetString(bytes));
        }

        private static void SaveXE(XElement el)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(el.ToString());
            File.WriteAllBytes(containerFile, HID.EncryptLocal(bytes));
        }
    }
}