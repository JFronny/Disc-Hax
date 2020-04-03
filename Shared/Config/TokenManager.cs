using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using CC_Functions.Misc;

namespace Shared.Config
{
    public static class TokenManager
    {
        private static readonly string containerFile =
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Cfgs",
                $"keys.secure.{Environment.OSVersion.Platform.ToString().ToLower()}");

        public static string DiscordToken
        {
            get => LoadXE().Element("discord")?.Value;
            set
            {
                XElement el = LoadXE();
                el.Element("discord").Value = value;
                SaveXE(el);
            }
        }

        public static string CurrencyconverterapiToken
        {
            get => LoadXE().Element("currencyconverterapi")?.Value;
            set
            {
                XElement el = LoadXE();
                el.Element("currencyconverterapi").Value = value;
                SaveXE(el);
            }
        }

        public static string PerspectiveToken
        {
            get => LoadXE().Element("perspective")?.Value;
            set
            {
                XElement el = LoadXE();
                el.Element("perspective").Value = value;
                SaveXE(el);
            }
        }

        private static XElement LoadXE()
        {
            if (!Directory.Exists(Path.GetDirectoryName(containerFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(containerFile));
            if (!File.Exists(containerFile))
                TokenForm.Show("", "", "", true);
            byte[] bytes = HID.DecryptLocal(File.ReadAllBytes(containerFile));
            XElement retEl = XElement.Parse(Encoding.UTF8.GetString(bytes));
            if (retEl.Element("discord") == null)
                retEl.Add(new XElement("discord", ""));
            if (retEl.Element("currencyconverterapi") == null)
                retEl.Add(new XElement("currencyconverterapi", ""));
            if (retEl.Element("perspective") == null)
                retEl.Add(new XElement("perspective", ""));
            return retEl;
        }

        public static void SaveXE(XElement el)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(el.ToString());
            File.WriteAllBytes(containerFile, HID.EncryptLocal(bytes));
        }
    }
}