using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CC_Functions.Misc;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Shared;
using Shared.Config;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace Bot.Commands
{
    [Group("admin")]
    [Aliases("a")]
    [Description("Commands for administration and debugging")]
    public class Administration : BaseCommandModule
    {
        [Command("ping")]
        [Aliases("pong")]
        [Description("Responds with \"Pong\" if the bot is active")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Ping(CommandContext ctx) =>
            await Ping(ctx.Channel, (c1, c2, c3) => ctx.RespondAsync(c1, c2, c3));

        public static async Task Ping(DiscordChannel Channel,
            Func<string, bool, DiscordEmbed, Task<DiscordMessage>> postMessage)
        {
            if (Channel.get(ConfigManager.ENABLED).TRUE())
                await postMessage($"Pong! ({Program.Bot.Ping}ms)", false, null);
        }

        [Command("config")]
        [RequireUserPermissions(Permissions.Administrator)]
        [Description("Prints or changes the DiscHax-instance config")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task ConfigCmd(CommandContext ctx)
        {
            if (ctx.Channel.get(ConfigManager.ENABLED).TRUE())
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync(ctx.Channel.getStr());
            }
        }

        [Command("config")]
        [RequireUserPermissions(Permissions.Administrator)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task ConfigCmd(CommandContext ctx, [Description("Config element to print")]
            string element)
        {
            if (ctx.Channel.get(ConfigManager.ENABLED).TRUE())
            {
                await ctx.TriggerTypingAsync();
                if (!CommandArr.getC().Contains(element))
                    throw new ArgumentException($"Element ({element}) not in CommandArr");
                await ctx.RespondAsync($"{element}: {ctx.Channel.get(element)}");
            }
        }

        [Command("config")]
        [RequireUserPermissions(Permissions.Administrator)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task ConfigCmd(CommandContext ctx, [Description("Config element to change")]
            string element, [Description("New value")] bool value)
        {
            if (ctx.Channel.get(ConfigManager.ENABLED).TRUE())
            {
                await ctx.TriggerTypingAsync();
                if (!CommandArr.getC().Contains(element))
                    throw new ArgumentException($"Element ({element}) not in CommandArr");
                ctx.Channel.set(element, value);
                await ctx.RespondAsync($"Set {element} to {value}");
            }
        }

        [Command("ban")]
        [RequireUserPermissions(Permissions.BanMembers)]
        [Description("Bans the selected user")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Ban(CommandContext ctx, DiscordMember member,
            [Description("Reason for the ban")] [RemainingText]
            string reason)
        {
            if (ctx.Channel.get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await member.BanAsync(reason: reason);
                await ctx.RespondAsync($"Banned {member.DisplayName}.");
            }
        }

        [Command("ban")]
        [RequireUserPermissions(Permissions.BanMembers)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Ban(CommandContext ctx, DiscordMember member,
            [Description("Period of time to ban the member for")]
            TimeSpan time, [Description("Reason for the ban")] [RemainingText]
            string reason)
        {
            if (ctx.Channel.get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.Guild.BanMemberAsync(member, reason: $"{reason}\nBanned until {DateTime.Now + time:g}");
                ctx.Guild.addTimedBan(member, time);
                await ctx.RespondAsync($"Banned {member.DisplayName}.");
            }
        }

        [Command("unban")]
        [RequireUserPermissions(Permissions.BanMembers)]
        [Description("Unbans the selected user")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Unban(CommandContext ctx, DiscordUser user,
            [Description("Reason for the unban")] [RemainingText]
            string reason)
        {
            if (ctx.Channel.get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.Guild.UnbanMemberAsync(user, reason);
                ctx.Guild.unbanUserIfBanned(user.Id);
                await ctx.RespondAsync($"Unbanned {user.Username}.");
            }
        }

        [Command("bans")]
        [RequireUserPermissions(Permissions.BanMembers)]
        [Description("Lists all banned users")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Bans(CommandContext ctx)
        {
            if (ctx.Channel.get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                IEnumerable<string> bans = (await ctx.Guild.GetBansAsync())
                    .Select(s =>
                        $"User: {s.User.Username}\nReason: {s.Reason}\nTimed: {(ctx.Guild.isUserTimeBanned(s.User.Id) ? $"Yes, {ctx.Guild.getBanTimeLeft(s.User.Id):g} left" : "No")}");
                await ctx.RespondPaginated($"Banned Users:\n{string.Join("\n\n", bans)}");
            }
        }

        [Command("softban")]
        [RequireUserPermissions(Permissions.BanMembers)]
        [Description("Kicks the member and deletes their messages")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Softban(CommandContext ctx, [Description("Member to softban")] DiscordMember member,
            [Description("Reason for the softban")] [RemainingText]
            string reason)
        {
            if (ctx.Channel.get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await member.BanAsync(7, reason);
                await member.UnbanAsync();
                await ctx.RespondAsync($"Softbanned {member.Username}.");
            }
        }

        [Command("kick")]
        [RequireUserPermissions(Permissions.KickMembers)]
        [Description("Kicks the member")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Kick(CommandContext ctx, [Description("Member to softban")] DiscordMember member,
            [Description("Reason for the softban")] [RemainingText]
            string reason)
        {
            if (ctx.Channel.get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await member.RemoveAsync(reason);
                await ctx.RespondAsync($"Kicked {member.Username}.");
            }
        }

        [Command("nick")]
        [Description("Gives the member a new nickname")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Nick(CommandContext ctx, [Description("New nickname")] string Nickname,
            [Description("Member to softban")] DiscordMember member = null)
        {
            if (ctx.Channel.get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                if (member == null)
                    member = ctx.Member;
                Permissions userPer = ctx.Member.PermissionsIn(ctx.Channel);
                if ((member.Id == ctx.Member.Id && userPer.HasPermission(Permissions.ChangeNickname)) ||
                    userPer.HasPermission(Permissions.ManageNicknames))
                {
                    await member.ModifyAsync(s => s.Nickname = Nickname);
                    await ctx.RespondAsync($"Set the nickname of {member.Username} to {Nickname}.");
                }
                else
                {
                    throw new Exception("I cannot allow you to do that.");
                }
            }
        }

        [Command("mute")]
        [RequireUserPermissions(Permissions.MuteMembers)]
        [Description("(Un)Mutes the member")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Mute(CommandContext ctx, [Description("Member to (Un)Mute")] DiscordMember member,
            [Description("Set to \"true\" to mute, false to undo")]
            bool mute,
            [Description("Reason for the (Un)Mute")] [RemainingText]
            string reason)
        {
            if (ctx.Channel.get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await member.SetMuteAsync(mute, reason);
                await ctx.RespondAsync($"{(mute ? "Muted" : "Unmuted")} {member.Username}.");
            }
        }

        [Command("deaf")]
        [RequireUserPermissions(Permissions.DeafenMembers)]
        [Description("(Un)Deafen the member")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Deaf(CommandContext ctx, [Description("Member to (Un)Deafen")] DiscordMember member,
            [Description("Set to \"true\" to deafen, false to undo")]
            bool deafen,
            [Description("Reason for the (Un)Deafen")] [RemainingText]
            string reason)
        {
            if (ctx.Channel.get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await member.SetDeafAsync(deafen, reason);
                await ctx.RespondAsync($"{(deafen ? "Deafened" : "Undeafened")} {member.Username}.");
            }
        }

        [Command("cooldown")]
        [RequireUserPermissions(Permissions.DeafenMembers)]
        [Description("Sets a custom cooldown for this channel")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Cooldown(CommandContext ctx, int cooldown)
        {
            if (ctx.Channel.get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                if (cooldown <= 21600 && cooldown >= 0)
                {
                    await ctx.Channel.ModifyAsync(x => x.PerUserRateLimit = cooldown);
                    await ctx.RespondAsync($"Set cooldown to {cooldown} seconds.");
                    return;
                }
                await ctx.RespondAsync($"Invalid cooldown: {cooldown}");
            }
        }

        [Command("avatar")]
        [Aliases("icon")]
        [Description("Gets the avatar of the specified user")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Avatar(CommandContext ctx, [Description("User to get icon from")] DiscordUser user)
        {
            await ctx.TriggerTypingAsync();
            WebClient wc = new WebClient();
            Image bmp = Image.FromStream(wc.OpenRead(user.AvatarUrl));
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Jpeg);
            ms.Position = 0;
            await ctx.RespondWithFileAsync("avatar.jpg", ms, $"Avatar of {user.Username}");
            wc.Dispose();
        }

        [Command("sudo")]
        [Hidden]
        [RequireOwner]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task SudoAsync(CommandContext ctx, [Description("Member to sudo")] DiscordMember m,
            [Description("Command to sudo")] [RemainingText]
            string command)
        {
            await ctx.TriggerTypingAsync();
            if (ctx.Client.CurrentApplication.Owners.All(x => x.Id != ctx.User.Id))
            {
                await ctx.RespondAsync("You do not have permission to use this command!");
                return;
            }
            await ctx.CommandsNext.ExecuteCommandAsync(ctx.CommandsNext.CreateFakeContext(m, ctx.Channel, command,
                Common.prefix, ctx.CommandsNext.FindCommand(command, out string _)));
        }

        [Command("purge")]
        [Description("Purge commands by user or regex")]
        [RequirePermissions(Permissions.Administrator)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Purge(CommandContext ctx, [Description("User to delete posts from")]
            DiscordMember member, [Description("Amount of messages to search")]
            int span = 50, [Description("Reason for deletion")] string reason = null)
        {
            if (ctx.Channel.get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                IEnumerable<DiscordMessage> messages = await ctx.Channel.GetMessagesAsync(span);
                messages = messages.Where(s => ctx.Guild.Members.First(a => a.Key == s.Author.Id).Value == member);
                await ctx.Channel.DeleteMessagesAsync(messages, reason);
                await ctx.RespondAsync($"Deleted {messages.Count()} messages");
            }
        }

        [Command("purge")]
        [RequirePermissions(Permissions.Administrator)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Purge(CommandContext ctx, [Description("Regex for messages")] string regex,
            [Description("Amount of messages to search")]
            int span, [Description("Reason for deletion")] string reason = null)
        {
            if (ctx.Channel.get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                IEnumerable<DiscordMessage> messages = await ctx.Channel.GetMessagesAsync(span);
                Regex rex = new Regex(regex);
                messages = messages.Where(s => rex.IsMatch(s.Content));
                await ctx.Channel.DeleteMessagesAsync(messages, reason);
                await ctx.RespondAsync($"Deleted {messages.Count()} messages");
            }
        }
    }
}