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
    static class ChCfgMgr
    {
        static Dictionary<ulong, ChannelConfig> Cfgs = new Dictionary<ulong, ChannelConfig>();
        static Dictionary<ulong, bool> GldE = new Dictionary<ulong, bool>();
        public static ChannelConfig getCh(ulong Channel)
        {
            if (!Cfgs.ContainsKey(Channel))
            {
                string XMLPath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Cfgs\";
                if (!Directory.Exists(XMLPath))
                    Directory.CreateDirectory(XMLPath);
                XMLPath += Channel.ToString() + ".xml";
                if (!File.Exists(XMLPath))
                    SaveChCfg(Channel, new ChannelConfig(() => { }));
                XElement el = XElement.Load(XMLPath);
                ChannelConfig tmp = new ChannelConfig
                {
                    Save = () => { },
                    Bees = bool.Parse(el.Element("bees").Value),
                    Booru = bool.Parse(el.Element("booru").Value),
                    Chan = bool.Parse(el.Element("chan").Value),
                    Config = bool.Parse(el.Element("config").Value),
                    Enabled = bool.Parse(el.Element("enabled").Value),
                    Nsfw = bool.Parse(el.Element("nsfw").Value),
                    Play = bool.Parse(el.Element("play").Value),
                    Waifu = bool.Parse(el.Element("waifu").Value)
                };
                tmp.Save = () => SaveChCfg(Channel);
                Cfgs[Channel] = tmp;
                GC.Collect();
            }
            return Cfgs[Channel];
        }

        public static void setCh(ulong Channel, ChannelConfig config)
        {
            Cfgs[Channel] = config;
            SaveChCfg(Channel, Cfgs[Channel]);
        }

        public static bool getGl(ulong Guild)
        {
            if (!GldE.ContainsKey(Guild))
            {
                if (File.Exists(Path.GetDirectoryName(Application.ExecutablePath) + @"\Cfgs\" + Guild.ToString() + ".xml"))
                    GldE[Guild] = bool.Parse(XElement.Load(Path.GetDirectoryName(Application.ExecutablePath) + @"\Cfgs\" + Guild.ToString() + ".xml").Element("enabled").Value);
                else
                    setGl(Guild, false);
            }
            return GldE[Guild];
        }

        public static void setGl(ulong Guild, bool val)
        {
            GldE[Guild] = val;
            new XElement("guild", new XElement("enabled", val.ToString())).Save(Path.GetDirectoryName(Application.ExecutablePath) + @"\Cfgs\" + Guild.ToString() + ".xml");
        }

        static void SaveChCfg(ulong Channel, ChannelConfig? cfg = null)
        {
            ChannelConfig tmp = (cfg == null) ? getCh(Channel) : cfg.Value;
            new XElement("channel",
                new XElement("chan", tmp.Chan),
                new XElement("play", tmp.Play),
                new XElement("waifu", tmp.Waifu),
                new XElement("booru", tmp.Booru),
                new XElement("nsfw", tmp.Nsfw),
                new XElement("bees", tmp.Bees),
                new XElement("config", tmp.Config),
                new XElement("enabled", tmp.Enabled)).Save(Path.GetDirectoryName(Application.ExecutablePath) + @"\Cfgs\" + Channel.ToString() + ".xml");
        }
    }
}
