using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Xml.Linq;
using CC_Functions.Misc;
using DSharpPlus.Entities;

namespace Shared.Config
{
    public static class ConfigManager
    {
        public delegate void ConfigUpdateEvent(object sender, string ID, string element);

        public const string CHANNEL = "channel";
        public const string GUILD = "guild";
        public const string ENABLED = "enabled";
        public const string NSFW = "nsfw";
        public const string USERS = "Users";
        public const string BANS = "Bans";

        public static ConfigUpdateEvent configUpdate;

        public static XElement getXML(string ID, string ElName, out string XMLPath)
        {
            XMLPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Cfgs");
            if (!Directory.Exists(XMLPath))
                Directory.CreateDirectory(XMLPath);
            XMLPath = Path.Combine(XMLPath, $"{ID}.xml");
            if (!File.Exists(XMLPath))
                new XElement(ElName).Save(XMLPath);
            try
            {
                return XElement.Load(XMLPath);
            }
            catch
            {
                return new XElement(ElName);
            }
        }

        public static bool? get(this DiscordGuild ID, string element, bool? defaultVal = true) =>
            get(ID.Id.ToString(), element, GUILD, defaultVal);

        public static bool? get(this DiscordChannel ID, string element, bool? defaultVal = true) =>
            get(ID.Id.ToString(), element, CHANNEL, defaultVal);

        public static bool? get(string ID, string element, string configType,
            bool? defaultVal = true)
        {
            XElement el = getXML(ID, configType, out _);
            XElement self = el.Element(element.ToLower());
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

        public static string getStr(this DiscordChannel ID) => getStr(ID.Id.ToString(), CHANNEL);
        public static string getStr(this DiscordGuild ID) => getStr(ID.Id.ToString(), GUILD);

        public static string getStr(string ID, string configType) => string.Join("\r\n",
            getXML(ID, configType, out _).Elements()
                .Where(s => !string.IsNullOrWhiteSpace(s.Value) && s.Value.Length <= 5)
                .Select(s => $"{s.Name}: {s.Value}"));

        public static void set(this DiscordChannel channel, string element, bool? val, bool disableFormChecks = false)
        {
            set(channel.Id.ToString(), element, val, disableFormChecks, CHANNEL,
                new[] {new ValueTuple<string, string>(channel.GuildId.ToString(), GUILD)});
        }

        public static void set(this DiscordGuild guild, string element, bool? val, bool disableFormChecks = false)
        {
            set(guild.Id.ToString(), element, val, disableFormChecks, GUILD,
                new[] {new ValueTuple<string, string>("common", "common")});
        }

        public static void set(string ID, string element, bool? val, bool disableFormChecks, string configType,
            ValueTuple<string, string>[] upper = null)
        {
            XElement XML = getXML(ID, configType, out string XMLPath);
            string el = element.ToLower();
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

        public static bool? getMethodEnabled(this DiscordChannel ID, bool? defaultval = true,
            [CallerMemberName] string method = "") => getMethodEnabled_ext(ID, defaultval, method);

        public static bool? getMethodEnabled_ext(this DiscordChannel ID, bool? defaultval = true, string method = "") =>
            getMethodEnabled(ID.Id.ToString(), CHANNEL, defaultval, method);

        private static bool? getMethodEnabled(string ID, string configType, bool? defaultval, string callerName)
        {
            string tmp1 = CommandComparer.GetName(callerName);
            bool? tmp = get(ID, tmp1, configType,
                defaultval);
            Console.WriteLine($"CommandComparer.GetName({callerName})={tmp1}");
            Console.WriteLine($"cfg({callerName})={tmp}");
            return tmp;
        }

        public static decimal getMoney(this DiscordGuild ID, DiscordMember user)
        {
            XElement el = getXML(ID.Id.ToString(), GUILD, out string XMLPath);
            if (el.Element(USERS) == null)
                el.Add(new XElement(USERS));
            if (el.Element(USERS).Element("user" + user.Id) == null)
                el.Element(USERS).Add(new XElement("user" + user.Id, decimal.Zero.ToString()));
            el.Save(XMLPath);
            return decimal.Parse(el.Element(USERS).Element("user" + user.Id).Value);
        }

        public static void setMoney(this DiscordGuild ID, DiscordMember user, decimal money)
        {
            XElement el = getXML(ID.Id.ToString(), GUILD, out string XMLPath);
            if (el.Element(USERS) == null)
                el.Add(new XElement(USERS));
            if (el.Element(USERS).Element("user" + user.Id) == null)
                el.Element(USERS).Add(new XElement("user" + user.Id));
            el.Element(USERS).Element("user" + user.Id).Value = money.ToString();
            el.Save(XMLPath);
        }

        public static void incrementMoney(this DiscordGuild ID, DiscordMember user, decimal amount) =>
            setMoney(ID, user, getMoney(ID, user) + amount);

        public static Dictionary<ulong, decimal> getAllMoney(this DiscordGuild ID)
        {
            return ID.Members.Where(s => !s.Value.IsBot && ID.Members.ContainsKey(s.Key))
                .ToDictionary(s => s.Key, s => getMoney(ID, s.Value));
        }

        public static void addTimedBan(this DiscordGuild ID, DiscordMember user, TimeSpan span)
        {
            XElement el = getXML(ID.Id.ToString(), GUILD, out string XMLPath);
            if (el.Element(BANS) == null)
                el.Add(new XElement(BANS));
            if (el.Element(BANS).Element("user" + user.Id) == null)
                el.Element(BANS).Add(new XElement("user" + user.Id, TimeSpan.Zero.ToString()));
            XElement element = el.Element(BANS);
            element = element.Element("user" + user.Id);
            element.Value = (DateTime.Now + span).ToString();
            el.Save(XMLPath);
        }

        public static void evalBans(this DiscordGuild ID)
        {
            XElement el = getXML(ID.Id.ToString(), GUILD, out string XMLPath);
            if (el.Element(BANS) == null)
                el.Add(new XElement(BANS));
            foreach (KeyValuePair<ulong, DateTime> v in el.Element(BANS).Elements().Select(s =>
                new KeyValuePair<ulong, DateTime>(
                    ulong.Parse(s.Name.LocalName.Replace("user", "")),
                    DateTime.Parse(s.Value))))
            {
                if (ID.GetBansAsync().GetAwaiter().GetResult().Count(s => s.User.Id == v.Key) == 0)
                {
                    el.Element(BANS).Element("user" + v.Key).Remove();
                    continue;
                }
                if (v.Value > DateTime.Now) continue;
                ID.UnbanMemberAsync(v.Key, "Timed ban timed out");
                el.Element(BANS).Element("user" + v.Key).Remove();
            }
            el.Save(XMLPath);
        }

        public static bool isUserTimeBanned(this DiscordGuild ID, ulong User)
        {
            XElement el = getXML(ID.Id.ToString(), GUILD, out string XMLPath);
            if (el.Element(BANS) == null)
                el.Add(new XElement(BANS));
            bool output = el.Element(BANS).Element("user" + User) != null;
            el.Save(XMLPath);
            return output;
        }

        public static TimeSpan getBanTimeLeft(this DiscordGuild ID, ulong User)
        {
            XElement el = getXML(ID.Id.ToString(), GUILD, out string XMLPath);
            if (el.Element(BANS) == null)
                el.Add(new XElement(BANS));
            TimeSpan output = TimeSpan.Zero;
            if (el.Element(BANS).Element("user" + User) != null)
                output = DateTime.Parse(el.Element(BANS).Element("user" + User).Value) - DateTime.Now;
            el.Save(XMLPath);
            return output;
        }

        public static void unbanUserIfBanned(this DiscordGuild ID, ulong User)
        {
            XElement el = getXML(ID.Id.ToString(), GUILD, out string XMLPath);
            if (el.Element(BANS) == null)
                el.Add(new XElement(BANS));
            if (el.Element(BANS).Element("user" + User) != null)
                el.Element(BANS).Element("user" + User).Remove();
            el.Save(XMLPath);
        }
    }
}