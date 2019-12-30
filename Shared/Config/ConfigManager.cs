using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using CC_Functions.Misc;

namespace Shared.Config
{
    public static class ConfigManager
    {
        public delegate void ConfigUpdateEvent(object sender, string ID, ConfigElement element);

        private static readonly string CHANNEL = "channel";
        private static readonly string GUILD = "guild";

        public static ConfigUpdateEvent configUpdate;
        private static string getTypeStr(this IBotStruct self) => self.tryCast(out BotGuild guild) ? GUILD : CHANNEL;

        private static XElement getXML(string ID, string ElName, out string XMLPath)
        {
            XMLPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Cfgs");
            if (!Directory.Exists(XMLPath))
                Directory.CreateDirectory(XMLPath);
            XMLPath = Path.Combine(XMLPath, $"{ID}.xml");
            if (!File.Exists(XMLPath))
                new XElement(ElName).Save(XMLPath);
            return XElement.Load(XMLPath);
        }

        public static bool? get(IBotStruct ID, ConfigElement element, bool? defaultVal = false) =>
            get(ID.Id.ToString(), element, ID.getTypeStr(), defaultVal);

        public static bool? get(string ID, ConfigElement element, string configType,
            bool? defaultVal = false)
        {
            XElement el = getXML(ID, configType, out _);
            XElement self = el.Element(element.ToString().ToLower());
            if (self == null)
            {
                set(ID, element, null, true, configType);
                return get(ID, element, configType, defaultVal);
            }
            if (string.IsNullOrEmpty(self.Value))
            {
                if (el.Element("upper") != null && !string.IsNullOrEmpty(el.Element("upper").Value) &&
                    el.Element("upperType") != null && !string.IsNullOrEmpty(el.Element("upperType").Value))
                    return get(el.Element("upper").Value, element, el.Element("upperType").Value, defaultVal);
                set(ID, element, defaultVal, true, "common");
                return get(ID, element, configType, defaultVal);
            }
            return GenericExtensions.ParseBool(self.Value);
        }

        public static string getStr(IBotStruct ID) => getStr(ID.Id.ToString(), ID.getTypeStr());

        public static string getStr(string ID, string configType) => string.Join("\r\n",
            Enum.GetValues(typeof(ConfigElement)).OfType<ConfigElement>()
                .Select(s => $"{s}: {get(ID, s, configType)}"));

        public static void set(BotChannel channel, ConfigElement element, bool? val, bool disableFormChecks = false)
        {
            set(channel.Id.ToString(), element, val, disableFormChecks, CHANNEL,
                new[] {new ValueTuple<string, string>(channel.Channel.GuildId.ToString(), GUILD)});
        }

        public static void set(BotGuild guild, ConfigElement element, bool? val, bool disableFormChecks = false)
        {
            set(guild.Id.ToString(), element, val, disableFormChecks, GUILD,
                new[] {new ValueTuple<string, string>("common", "common")});
        }

        public static void set(string ID, ConfigElement element, bool? val, bool disableFormChecks, string configType,
            ValueTuple<string, string>[] upper = null)
        {
            XElement XML = getXML(ID, configType, out string XMLPath);
            string el = element.ToString().ToLower();
            if (XML.Elements(el).Count() == 0)
                XML.Add(new XElement(el, val.ToString()));
            else
                XML.Element(el).Value = val.ToString();
            if (upper != null)
            {
                XElement tmpXML = XML;
                for (int i = 0; i < upper.Length; i++)
                {
                    if (tmpXML.Elements("upper").Count() == 0)
                        tmpXML.Add(new XElement("upper", upper[i].Item1));
                    else
                        tmpXML.Element("upper").Value = upper[i].Item1;
                    if (tmpXML.Elements("upperType").Count() == 0)
                        tmpXML.Add(new XElement("upperType", upper[i].Item2));
                    else
                        tmpXML.Element("upperType").Value = upper[i].Item2;
                    tmpXML = getXML(tmpXML.Element("upper").Value, tmpXML.Element("upperType").Value, out _);
                }
            }
            if (!disableFormChecks)
                configUpdate?.Invoke(null, ID, element);
            XML.Save(XMLPath);
        }
    }
}