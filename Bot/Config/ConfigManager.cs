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

        public static bool? get(ulong Channel, ConfigElement element)
        {
            XElement el = getXML(Channel, "channel").Element(element.ToString().ToLower());
            if (el == null)
            {
                set(Channel, element, false);
                return get(Channel, element);
            }
            else
                return Extensions.ParseBool(el.Value);
        }

        public static string getStr(ulong Channel) => string.Join("\r\n", Enum.GetValues(typeof(ConfigElement)).OfType<ConfigElement>().Select(s => s.ToString() + ": " + get(Channel, s)));

        public static void set(ulong ID, ConfigElement element, bool? val, bool disableFormChecks = false, string configType = "channel")
        {
            XElement XML = getXML(ID, configType, out string XMLPath);
            string el = element.ToString().ToLower();
            if (XML.Elements(el).Count() == 0)
                XML.Add(new XElement(el, val.ToString()));
            else
                XML.Element(el).Value = val.ToString();
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
                    f.Invoke((MethodInvoker)delegate () { f.updateChecking(); });
            }
            XML.Save(XMLPath);
        }
    }
}
