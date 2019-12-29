#region

using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;

#endregion

namespace Shared
{
    public static class GuildSingleton
    {
        private static readonly Dictionary<ulong, BotGuild> Guilds = new Dictionary<ulong, BotGuild>();

        public static int Count => Guilds.Count;
        public static BotGuild getInstance(this DiscordGuild guild) => Guilds[guild.Id];

        public static BotChannel getInstance(this DiscordChannel channel) =>
            channel.Guild.getInstance().Channels[channel.Id];

        public static BotMessage getInstance(this DiscordMessage message) =>
            message.Channel.getInstance().Messages[message.Id];

        public static bool Remove(DiscordGuild guild) => Remove(guild.Id);
        public static bool Remove(ulong guildID) => Guilds.Remove(guildID);

        public static BotGuild Add(DiscordGuild guild)
        {
            BotGuild tmp = new BotGuild(guild);
            Guilds.Add(guild.Id, tmp);
            return tmp;
        }

        public static BotGuild getInstance(ulong guildID) => Guilds[guildID];

        public static BotGuild[] getAll() => Guilds.Values.ToArray();
    }
}