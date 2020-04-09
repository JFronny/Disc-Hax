using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using CC_Functions.Misc;
using DSharpPlus.Entities;

namespace Shared.Config
{
    public static class ConfigManager
    {
        public delegate void ConfigUpdateEvent(object sender, string id, string element);

        public const string Channel = "channel";
        public const string Guild = "guild";
        public const string Enabled = "enabled";
        public const string Nsfw = "nsfw";
        public const string Users = "Users";
        public const string Bans = "Bans";

        public static ConfigUpdateEvent ConfigUpdate;

        public static XElement GetXml(string id, string elName, out string xmlPath)
        {
            xmlPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Cfgs");
            if (!Directory.Exists(xmlPath))
                Directory.CreateDirectory(xmlPath);
            xmlPath = Path.Combine(xmlPath, $"{id}.xml");
            if (!File.Exists(xmlPath))
                new XElement(elName).Save(xmlPath);
            try
            {
                return XElement.Load(xmlPath);
            }
            catch
            {
                return new XElement(elName);
            }
        }

        public static bool? Get(this SnowflakeObject id, string element, bool? defaultVal = true) => GenericExtensions.ParseBool(id.Get(element, defaultVal.GetString()));

        public static string? Get(this SnowflakeObject id, string element, string? defaultVal)
        {
            return id switch
            {
                DiscordChannel _ => Get(id.Id.ToString(), element, Channel, defaultVal),
                DiscordGuild _ => Get(id.Id.ToString(), element, Guild, defaultVal),
                _ => throw new ArgumentException("Invalid Snowflake! (Supports Guilds and Channels)")
            };
        }

        private static string? Get(string id, string element, string configType, string? defaultVal)
        {
            while (true)
            {
                XElement el = GetXml(id, configType, out _);
                XElement self = el.Element(element.ToLower());
                if (self == null)
                {
                    Set(id, element, null, true, configType);
                    continue;
                }
                if (!string.IsNullOrEmpty(self.Value))
                    return self.Value;
                if (el.Element("upper") != null && !string.IsNullOrEmpty(el.Element("upper").Value) &&
                    el.Element("upperType") != null && !string.IsNullOrEmpty(el.Element("upperType").Value))
                {
                    id = el.Element("upper").Value;
                    configType = el.Element("upperType").Value;
                    continue;
                }
                Set(id, element, defaultVal, true, "common");
            }
        }

        public static string GetStr(this SnowflakeObject id, string[] allowed)
        {
            string configType = id switch
            {
                DiscordChannel _ => Channel,
                DiscordGuild _ => Guild,
                _ => throw new ArgumentException("Invalid Snowflake! (Supports Guilds and Channels)")
            };
            return string.Join("\r\n", allowed.Select(element => $"{element}: {Get(id, element, true)}"));
        }

        public static void Set(this SnowflakeObject id, string element, string? val, bool disableFormChecks = false)
        {
            switch (id)
            {
                case DiscordChannel channel:
                    Set(channel.Id.ToString(), element, val, disableFormChecks, Channel,
                        new[] {new ValueTuple<string, string>(channel.GuildId.ToString(), Guild)});
                    break;
                case DiscordGuild guild:
                    Set(guild.Id.ToString(), element, val, disableFormChecks, Guild,
                        new[] {new ValueTuple<string, string>("common", "common")});
                    break;
                default:
                    throw new ArgumentException("Invalid Snowflake! (Supports Guilds and Channels)");
            }
        }

        private static void Set(string id, string element, string? val, bool disableFormChecks, string configType,
            IList<(string, string)> upper = null)
        {
            XElement xml = GetXml(id, configType, out string xmlPath);
            bool changed = false;
            string el = element.ToLower();
            if (!xml.Elements(el).Any())
            {
                xml.Add(val is null ? new XElement(el) : new XElement(el, val));
                changed = true;
            }
            else if (xml.Element(el).Value != val && !(val is null))
            {
                xml.Element(el).Value = val;
                changed = true;
            }

            if (!changed) return;
            if (upper != null)
            {
                XElement tmpXml = xml;
                for (int i = 0; i < upper.Count; i++)
                {
                    if (!tmpXml.Elements("upper").Any())
                        tmpXml.Add(new XElement("upper", upper[i].Item1));
                    else
                        tmpXml.Element("upper").Value = upper[i].Item1;
                    if (!tmpXml.Elements("upperType").Any())
                        tmpXml.Add(new XElement("upperType", upper[i].Item2));
                    else
                        tmpXml.Element("upperType").Value = upper[i].Item2;
                    tmpXml = GetXml(tmpXml.Element("upper").Value, tmpXml.Element("upperType").Value, out _);
                }
            }
            if (!disableFormChecks)
                ConfigUpdate?.Invoke(null, id, element);
            xml.Save(xmlPath);
        }

        public static bool? GetMethodEnabled(this DiscordChannel id, bool? defaultval = true,
            [CallerMemberName] string method = "") => getMethodEnabled_ext(id, defaultval, method);

        public static bool? getMethodEnabled_ext(this DiscordChannel id, bool? defaultval = true, string method = "") =>
            GetMethodEnabled(id.Id.ToString(), Channel, defaultval, method);

        private static bool? GetMethodEnabled(string id, string configType, bool? defaultval, string callerName)
        {
            string tmp1 = CommandComparer.GetName(callerName);
            bool? tmp = GenericExtensions.ParseBool(Get(id, tmp1, configType,
                defaultval.GetString()));
            Console.WriteLine($"CommandComparer.GetName({callerName})={tmp1}");
            Console.WriteLine($"cfg({callerName})={tmp}");
            return tmp;
        }

        public static decimal GetMoney(this DiscordGuild id, DiscordMember user)
        {
            XElement el = GetXml(id.Id.ToString(), Guild, out string xmlPath);
            if (el.Element(Users) == null)
                el.Add(new XElement(Users));
            if (el.Element(Users).Element($"user{user.Id}") == null)
                el.Element(Users)
                    .Add(new XElement($"user{user.Id}", decimal.Zero.ToString(CultureInfo.InvariantCulture)));
            el.Save(xmlPath);
            return decimal.Parse(el.Element(Users).Element($"user{user.Id}").Value);
        }

        public static void SetMoney(this DiscordGuild id, DiscordMember user, decimal money)
        {
            XElement el = GetXml(id.Id.ToString(), Guild, out string xmlPath);
            if (el.Element(Users) == null)
                el.Add(new XElement(Users));
            if (el.Element(Users).Element($"user{user.Id}") == null)
                el.Element(Users).Add(new XElement($"user{user.Id}"));
            el.Element(Users).Element($"user{user.Id}").Value = money.ToString(CultureInfo.InvariantCulture);
            el.Save(xmlPath);
        }

        public static void IncrementMoney(this DiscordGuild id, DiscordMember user, decimal amount) =>
            SetMoney(id, user, GetMoney(id, user) + amount);

        public static Dictionary<ulong, decimal> GetAllMoney(this DiscordGuild id)
        {
            return id.Members.Where(s => !s.Value.IsBot && id.Members.ContainsKey(s.Key))
                .ToDictionary(s => s.Key, s => GetMoney(id, s.Value));
        }
        
#if !NO_TIMED_BAN
        public static void AddTimedBan(this DiscordGuild id, DiscordMember user, TimeSpan span)
        {
            XElement el = GetXml(id.Id.ToString(), Guild, out string xmlPath);
            if (el.Element(Bans) == null)
                el.Add(new XElement(Bans));
            if (el.Element(Bans).Element($"user{user.Id}") == null)
                el.Element(Bans).Add(new XElement($"user{user.Id}", TimeSpan.Zero.ToString()));
            XElement element = el.Element(Bans);
            element = element.Element($"user{user.Id}");
            element.Value = (DateTime.Now + span).ToString(CultureInfo.InvariantCulture);
            el.Save(xmlPath);
        }

        public static void EvalBans(this DiscordGuild id)
        {
            XElement el = GetXml(id.Id.ToString(), Guild, out string xmlPath);
            if (el.Element(Bans) == null)
                el.Add(new XElement(Bans));
            foreach (KeyValuePair<ulong, DateTime> v in el.Element(Bans).Elements().Select(s =>
                new KeyValuePair<ulong, DateTime>(
                    ulong.Parse(s.Name.LocalName.Replace("user", "")),
                    DateTime.Parse(s.Value))))
            {
                if (id.GetBansAsync().GetAwaiter().GetResult().Count(s => s.User.Id == v.Key) == 0)
                {
                    el.Element(Bans).Element($"user{v.Key}").Remove();
                    continue;
                }
                if (v.Value > DateTime.Now) continue;
                id.UnbanMemberAsync(v.Key, "Timed ban timed out");
                el.Element(Bans).Element($"user{v.Key}").Remove();
            }
            el.Save(xmlPath);
        }

        public static bool IsUserTimeBanned(this DiscordGuild id, ulong user)
        {
            XElement el = GetXml(id.Id.ToString(), Guild, out string xmlPath);
            if (el.Element(Bans) == null)
                el.Add(new XElement(Bans));
            bool output = el.Element(Bans).Element($"user{user}") != null;
            el.Save(xmlPath);
            return output;
        }

        public static TimeSpan GetBanTimeLeft(this DiscordGuild id, ulong user)
        {
            XElement el = GetXml(id.Id.ToString(), Guild, out string xmlPath);
            if (el.Element(Bans) == null)
                el.Add(new XElement(Bans));
            TimeSpan output = TimeSpan.Zero;
            if (el.Element(Bans).Element($"user{user}") != null)
                output = DateTime.Parse(el.Element(Bans).Element($"user{user}").Value) - DateTime.Now;
            el.Save(xmlPath);
            return output;
        }

        public static void UnbanUserIfBanned(this DiscordGuild id, ulong user)
        {
            XElement el = GetXml(id.Id.ToString(), Guild, out string xmlPath);
            if (el.Element(Bans) == null)
                el.Add(new XElement(Bans));
            if (el.Element(Bans).Element($"user{user}") != null)
                el.Element(Bans).Element($"user{user}").Remove();
            el.Save(xmlPath);
        }
#endif
    }
}