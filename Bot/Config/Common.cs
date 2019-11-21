using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Bot.Config
{
    static class Common
    {
        static string path = Path.GetDirectoryName(Application.ExecutablePath) + @"\Cfgs\common.xml";
        static XElement common;
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

        static void getXE()
        {
            if (!File.Exists(path))
                new XElement("common", new XElement("prefix", "!")).Save(path);
            common = XElement.Load(path);
        }

        static void saveXE() => common.Save(path);
    }
}
