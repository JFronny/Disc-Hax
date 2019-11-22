using DSharpPlus.Entities;
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
    static class ConfigManager
    {
        static XElement getXML(ulong ID, string ElName, out string XMLPath)
        {
            XMLPath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Cfgs\";
            if (!Directory.Exists(XMLPath))
                Directory.CreateDirectory(XMLPath);
            XMLPath += ID.ToString() + ".xml";
            if (!File.Exists(XMLPath))
                new XElement(ElName).Save(XMLPath);
            return XElement.Load(XMLPath);
        }
        static XElement getXML(ulong ID, string ElName) => getXML(ID, ElName, out string leltisnotused);
        public static bool? get(ulong ID, ConfigElement element, string configType = "channel", bool? defaultVal = false)
        {
            XElement el = getXML(ID, configType);
            if (el.Element("upper") != null && el.Element("upper").Value != null
                && el.Element("upperType") != null && el.Element("upperType").Value != null)
            {
                bool? tmp = get(ulong.Parse(el.Element("upper").Value), element, el.Element("upperType").Value, null);
                if (tmp.HasValue)
                    return tmp;
            }
            el = el.Element(element.ToString().ToLower());
            if (el == null)
            {
                set(ID, element, defaultVal, true);
                return get(ID, element, configType, defaultVal);
            }
            else
                return Extensions.ParseBool(el.Value);
        }

        public static string getStr(ulong ID) => string.Join("\r\n", Enum.GetValues(typeof(ConfigElement)).OfType<ConfigElement>().Select(s => s.ToString() + ": " + get(ID, s)));
        public static void set(DiscordChannel channel, ConfigElement element, bool? val, bool disableFormChecks = false) => set(channel.Id, element, val, disableFormChecks, "channel", new ValueTuple<ulong, string>[] { new ValueTuple<ulong, string>(channel.GuildId, "guild") });
        public static void set(ulong ID, ConfigElement element, bool? val, bool disableFormChecks = false, string configType = "channel", ValueTuple<ulong, string>[] upper = null)
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
                        tmpXML.Add(new XElement("upper", upper[i].Item1.ToString()));
                    else
                        tmpXML.Element("upper").Value = upper[i].Item1.ToString();
                    if (tmpXML.Elements("upperType").Count() == 0)
                        tmpXML.Add(new XElement("upperType", upper[i].Item2));
                    else
                        tmpXML.Element("upperType").Value = upper[i].Item2;
                    tmpXML = getXML(ulong.Parse(tmpXML.Element("upper").Value), tmpXML.Element("upperType").Value);
                }
            }
            if (!disableFormChecks)
            {
                MainForm f = MainForm.Instance;
                if (f.ChannelDefined && f.SelectedChannel.Id == ID)
                    f.Invoke((MethodInvoker)delegate ()
                    {
                        f.channelTree_AfterSelect(null, new TreeViewEventArgs(
                            f.channelTree.Nodes[0].Nodes.OfType<TreeNode>()
                            .SelectMany(s => s.Nodes.OfType<TreeNode>())
                            .First(s => ((BotChannel)s.Tag).Id == ID)));
                    });
                if (element == ConfigElement.Enabled)
                    f.InvokeAction((MethodInvoker)delegate () { f.updateChecking(); });
            }
            XML.Save(XMLPath);
        }
    }
}
