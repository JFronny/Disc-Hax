using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace Shared.Config
{
    public static class Common
    {
        private static readonly string Path = System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Cfgs",
            "common.xml");

        private static XElement _common;

        public static string Prefix
        {
            get
            {
                GetXe();
                if (_common.Element(ConfigManager.Prefix) == null)
                {
                    _common.Add(new XElement(ConfigManager.Prefix), "!");
                    SaveXe();
                }

                return _common.Element(ConfigManager.Prefix).Value;
            }
        }

        public static bool GuildsBox
        {
            get
            {
                GetXe();
                if (_common.Element("guildsBox") == null ||
                    !bool.TryParse(_common.Element("guildsBox").Value, out bool t))
                {
                    _common.Add(new XElement("guildsBox", bool.FalseString));
                    SaveXe();
                }

                return bool.Parse(_common.Element("guildsBox").Value);
            }
            set
            {
                GetXe();
                if (_common.Element("guildsBox") == null)
                    _common.Add(new XElement("guildsBox", value.ToString()));
                else
                    _common.Element("guildsBox").Value = value.ToString();
                SaveXe();
            }
        }

        private static void GetXe()
        {
            if (!Directory.Exists(System.IO.Path.GetDirectoryName(Path)))
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
            if (!File.Exists(Path))
                new XElement("common", new XElement(ConfigManager.Prefix, "!"),
                    new XElement("guildsBox", false.ToString()),
                    new XElement("stash")).Save(Path);
            _common = XElement.Load(Path);
        }

        private static void SaveXe() => _common.Save(Path);
    }
}