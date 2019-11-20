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
                return common.Element("prefix").Value;
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
