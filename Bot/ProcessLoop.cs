using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DSharpPlus;
using DSharpPlus.Entities;
using Shared.Config;

namespace Bot
{
    public class ProcessLoop
    {
        public static bool RunIteration(DiscordClient client)
        {
            try
            {
                if (Console.KeyAvailable && Console.Read() == 'x') return true;
                Thread.Sleep(5000);
#if !NO_TIMED_BAN
                if (client == null) return true;
                foreach (KeyValuePair<ulong, DiscordGuild> guild in client.Guilds)
                    guild.Value.EvalBans();
#endif
            }
            catch (Exception e)
            {
                client.DebugLogger.LogMessage(LogLevel.Error, "DiscHax", $"A crash occured in the main Thread: {e}", DateTime.Now, e);
            }
            finally
            {
                try
                {
                    EvaluateRR(client);
                }
                catch (Exception e)
                {
                    client.DebugLogger.LogMessage(LogLevel.Error, "DiscHax",
                        $"A crash occured in the reaction-roles Thread: {e}", DateTime.Now, e);
                }
            }
            return false;
        }

        private static void EvaluateRR(DiscordClient client)
        {
            foreach (KeyValuePair<ulong, DiscordGuild> guild in client.Guilds)
            {
                Dictionary<string, ulong> roles = guild.Value.GetReactionRoles();
                if (roles.Values.Count(s => guild.Value.Roles.ContainsKey(s)) != roles.Count)
                {
                    guild.Value.SetReactionRoles(roles.Where(s => guild.Value.Roles.ContainsKey(s.Value))
                        .ToDictionary(s => s.Key, s => s.Value));
                    roles = guild.Value.GetReactionRoles();
                }
                if (roles.GroupBy(s => s.Value).Count() != roles.Count)
                {
                    guild.Value.SetReactionRoles(roles.GroupBy(s => s.Value).Select(s => s.First())
                        .ToDictionary(s => s.Key, s => s.Value));
                    roles = guild.Value.GetReactionRoles();
                }
                if (roles.Count > 20)
                {
                    guild.Value.SetReactionRoles(roles.Take(20)
                        .ToDictionary(s => s.Key, s => s.Value));
                    roles = guild.Value.GetReactionRoles();
                }
                (ulong? channel, ulong? message) =
                    guild.Value.GetReactionRoleMessage() ?? new Tuple<ulong?, ulong?>(0, 0);
                if (channel == 0 || message == 0) continue;
                DiscordMessage msg =
                    client.GetChannelAsync(channel.Value).Result.GetMessageAsync(message.Value).Result;
                foreach (DiscordEmoji disallowed in msg.Reactions.Where(s => !s.IsMe).Select(s => s.Emoji))
                    msg.DeleteReactionsEmojiAsync(disallowed);
                foreach (KeyValuePair<string, ulong> role in roles)
                {
                    Func<DiscordReaction, bool> selector = s =>
                        s.IsMe && s.Emoji.GetDiscordName() == role.Key;
                    DiscordEmoji emoji = DiscordEmoji.FromName(client, role.Key);
                    if (!msg.Reactions.Any(selector)) msg.CreateReactionAsync(emoji);
                    IReadOnlyList<DiscordUser> reactions = msg.GetReactionsAsync(emoji).Result;
                    DiscordRole dRole = guild.Value.GetRole(role.Value);
                    foreach (KeyValuePair<ulong, DiscordMember> member in guild.Value.Members)
                    {
                        bool inGroup = member.Value.Roles.Contains(dRole);
                        bool shouldBeInGroup = reactions.Contains(member.Value);
                        if (inGroup == shouldBeInGroup) continue;
                        if (shouldBeInGroup)
                            member.Value.GrantRoleAsync(dRole);
                        else
                            member.Value.RevokeRoleAsync(dRole);
                    }
                }
            }
        }
    }
}