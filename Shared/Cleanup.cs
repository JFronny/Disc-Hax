using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Shared.Config;

namespace Shared
{
    public static class Cleanup
    {
        public static void Clean(IEnumerable<string> commands, IEnumerable<Tuple<string, IEnumerable<string>>> guilds,
            IEnumerable<string> channelIds)
        {
            string[] allowedNames = {"common.xml"};
            foreach (string cfg in Directory.GetFiles(Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                @"Cfgs")).Where(s => !Path.GetFileName(s).StartsWith("keys.secure")))
            {
                //Clean files
                XDocument el;
                try
                {
                    el = XDocument.Load(cfg);
                }
                catch
                {
                    el = new XDocument();
                }

                string file = Path.GetFileName(cfg);
                string id = Path.GetFileNameWithoutExtension(file);
                if (el.Element("guild") != null)
                {
                    if (!guilds.Any(s => s.Item1 == id))
                        File.Delete(cfg);
                }
                else if (el.Element("channel") != null)
                {
                    if (!channelIds.Contains(id))
                        File.Delete(cfg);
                }
                else
                {
                    if (!allowedNames.Contains(file))
                        File.Delete(cfg);
                }
                //Clean contents
                if (!File.Exists(cfg)) continue;
                XElement root = el.Element(ConfigManager.Guild) ??
                                el.Element(ConfigManager.Channel) ?? el.Element("common");
                List<string> allowedVars = commands.ToList();
                allowedVars.AddRange(new[] {ConfigManager.Prefix, ConfigManager.Enabled});
                if (cfg != allowedNames[0])
                    allowedVars.AddRange(new[] {"upperType", "upper", ConfigManager.Users, ConfigManager.Bans});
                List<XElement> filteredELs =
                    root.Elements().Where(s => allowedVars.Contains(s.Name.LocalName)).ToList();
                root.RemoveAll();
                filteredELs.ForEach(s => root.Add(s));
                if (root.Name.LocalName == ConfigManager.Guild)
                {
                    IEnumerable<string> users = guilds.First(s => s.Item1 == id).Item2;
                    if (root.Element(ConfigManager.Users) != null)
                        foreach (XElement element in root.Element(ConfigManager.Users).Elements())
                            if (users.All(s => s != element.Name.LocalName.Replace("user", "")))
                                element.Remove();
                }
                el.Save(cfg);
            }
        }
    }
}