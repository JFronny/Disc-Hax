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
        public static ChannelConfig getCh(ulong Channel)
        {
            string XMLPath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Cfgs\";
            if (!Directory.Exists(XMLPath))
                Directory.CreateDirectory(XMLPath);
            XMLPath += Channel.ToString() + ".xml";
            if (!File.Exists(XMLPath))
                setCh(Channel, new ChannelConfig(() => { }));
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
            tmp.Save = () => setCh(Channel, tmp);
            GC.Collect();
            return tmp;
        }

        public static bool getGl(ulong Guild)
        {
            if (!File.Exists(Path.GetDirectoryName(Application.ExecutablePath) + @"\Cfgs\" + Guild.ToString() + ".xml"))
                setGl(Guild, false);
            return bool.Parse(XElement.Load(Path.GetDirectoryName(Application.ExecutablePath) + @"\Cfgs\" + Guild.ToString() + ".xml").Element("enabled").Value);
        }

        public static void setGl(ulong Guild, bool val) => new XElement("guild", new XElement("enabled", val.ToString())).Save(Path.GetDirectoryName(Application.ExecutablePath) + @"\Cfgs\" + Guild.ToString() + ".xml");

        public static void setCh(ulong Channel, ChannelConfig cfg)
        {
            new XElement("channel",
                new XElement("chan", cfg.Chan),
                new XElement("play", cfg.Play),
                new XElement("waifu", cfg.Waifu),
                new XElement("booru", cfg.Booru),
                new XElement("nsfw", cfg.Nsfw),
                new XElement("bees", cfg.Bees),
                new XElement("config", cfg.Config),
                new XElement("enabled", cfg.Enabled)).Save(Path.GetDirectoryName(Application.ExecutablePath) + @"\Cfgs\" + Channel.ToString() + ".xml");
        }
    }
}
