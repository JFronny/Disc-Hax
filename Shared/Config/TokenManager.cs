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
        private static readonly string ContainerFile =
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Cfgs",
                $"keys.secure.{Environment.OSVersion.Platform.ToString().ToLower()}");

        public static string DiscordToken
        {
            get => LoadXe().Element("discord")?.Value;
            set
            {
                XElement el = LoadXe();
                el.Element("discord").Value = value;
                SaveXe(el);
            }
        }

        public static string CurrencyconverterapiToken
        {
            get => LoadXe().Element("currencyconverterapi")?.Value;
            set
            {
                XElement el = LoadXe();
                el.Element("currencyconverterapi").Value = value;
                SaveXe(el);
            }
        }

        public static string PerspectiveToken
        {
            get => LoadXe().Element("perspective")?.Value;
            set
            {
                XElement el = LoadXe();
                el.Element("perspective").Value = value;
                SaveXe(el);
            }
        }

        private static XElement LoadXe()
        {
            if (!Directory.Exists(Path.GetDirectoryName(ContainerFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(ContainerFile));
            if (!File.Exists(ContainerFile))
                TokenForm.Show("", "", "", true);
            byte[] bytes = HID.DecryptLocal(File.ReadAllBytes(ContainerFile));
            XElement retEl = XElement.Parse(Encoding.UTF8.GetString(bytes));
            if (retEl.Element("discord") == null)
                retEl.Add(new XElement("discord", ""));
            if (retEl.Element("currencyconverterapi") == null)
                retEl.Add(new XElement("currencyconverterapi", ""));
            if (retEl.Element("perspective") == null)
                retEl.Add(new XElement("perspective", ""));
            return retEl;
        }

        public static void SaveXe(XElement el)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(el.ToString());
            File.WriteAllBytes(ContainerFile, HID.EncryptLocal(bytes));
        }
    }
}