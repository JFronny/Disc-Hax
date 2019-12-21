using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Shared.Config
{
    public static class Common
    {
        private static string path = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Cfgs", "common.xml");
        private static XElement common;

        public static string prefix
        {
            get {
                getXE();
                if (common.Element("prefix") == null)
                {
                    common.Add(new XElement("prefix"), "!");
                    saveXE();
                }
                return common.Element("prefix").Value;
            }
        }

        public static bool guildsBox
        {
            get {
                getXE();
                if (common.Element("guildsBox") == null || !bool.TryParse(common.Element("guildsBox").Value, out bool t))
                {
                    common.Add(new XElement("guildsBox", bool.FalseString));
                    saveXE();
                }
                return bool.Parse(common.Element("guildsBox").Value);
            }
            set {
                getXE();
                if (common.Element("guildsBox") == null)
                    common.Add(new XElement("guildsBox", value.ToString()));
                else
                    common.Element("guildsBox").Value = value.ToString();
                saveXE();
            }
        }

        private static void getXE()
        {
            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            if (!File.Exists(path))
                new XElement("common", new XElement("prefix", "!")).Save(path);
            common = XElement.Load(path);
        }

        private static void saveXE() => common.Save(path);
    }
}