using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CC_Functions.Misc;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Shared;
using Shared.Config;

namespace Bot.Commands.Administration
{
    public partial class Administration
    {
        [Command("ban")]
        [Aliases("b")]
        [RequireUserPermissions(Permissions.BanMembers)]
        [Description("Bans the selected user")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Ban(CommandContext ctx, DiscordMember member,
            [Description("Reason for the ban")] [RemainingText]
            string reason)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await member.BanAsync(reason: reason);
                await ctx.RespondAsync($"Banned {member.DisplayName}.");
            }
        }

#if !NO_TIMED_BAN
        [Command("ban")]
        [RequireUserPermissions(Permissions.BanMembers)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Ban(CommandContext ctx, DiscordMember member,
            [Description("Period of time to ban the member for")]
            TimeSpan time, [Description("Reason for the ban")] [RemainingText]
            string reason)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.Guild.BanMemberAsync(member, reason: $"{reason}\nBanned until {DateTime.Now + time:g}");
                ctx.Guild.AddTimedBan(member, time);
                await ctx.RespondAsync($"Banned {member.DisplayName}.");
            }
        }
#endif

        [Command("unban")]
        [Aliases("u", "ub", "uban", "deban", "rmban")]
        [RequireUserPermissions(Permissions.BanMembers)]
        [Description("Unbans the selected user")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Unban(CommandContext ctx, DiscordUser user,
            [Description("Reason for the unban")] [RemainingText]
            string reason)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.Guild.UnbanMemberAsync(user, reason);
#if !NO_TIMED_BAN
                ctx.Guild.UnbanUserIfBanned(user.Id);
#endif
                await ctx.RespondAsync($"Unbanned {user.Username}.");
            }
        }

        [Command("bans")]
        [Aliases("lb", "listbans", "lsb", "lsbans")]
        [RequireUserPermissions(Permissions.BanMembers)]
        [Description("Lists all banned users")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Bans(CommandContext ctx)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
#if NO_TIMED_BAN
                IEnumerable<string> bans = (await ctx.Guild.GetBansAsync())
                    .Select(s =>
                        $"User: {s.User.Username}\nReason: {s.Reason}");
#else
                IEnumerable<string> bans = (await ctx.Guild.GetBansAsync())
                    .Select(s =>
                        $"User: {s.User.Username}\nReason: {s.Reason}\nTimed: {(ctx.Guild.IsUserTimeBanned(s.User.Id) ? $"Yes, {ctx.Guild.GetBanTimeLeft(s.User.Id):g} left" : "No")}");
#endif
                await ctx.RespondPaginatedIfTooLong($"Banned Users:\n{string.Join("\n\n", bans)}");
            }
        }

        [Command("softban")]
        [Aliases("sb", "sban")]
        [RequireUserPermissions(Permissions.BanMembers)]
        [Description("Kicks the member and deletes their messages")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Softban(CommandContext ctx, [Description("Member to softban")] DiscordMember member,
            [Description("Reason for the softban")] [RemainingText]
            string reason)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await member.BanAsync(7, reason);
                await member.UnbanAsync();
                await ctx.RespondAsync($"Softbanned {member.Username}.");
            }
        }

        [Command("kick")]
        [Aliases("k")]
        [RequireUserPermissions(Permissions.KickMembers)]
        [Description("Kicks the member")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Kick(CommandContext ctx, [Description("Member to softban")] DiscordMember member,
            [Description("Reason for the softban")] [RemainingText]
            string reason)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await member.RemoveAsync(reason);
                await ctx.RespondAsync($"Kicked {member.Username}.");
            }
        }

        [Command("nick")]
        [Aliases("n")]
        [Description("Gives the member a new nickname")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Nick(CommandContext ctx, [Description("New nickname")] string nickname,
            [Description("Member to softban")] DiscordMember? member = null)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                if (member == null)
                    member = ctx.Member;
                Permissions userPer = ctx.Member.PermissionsIn(ctx.Channel);
                if (member.Id == ctx.Member.Id && userPer.HasPermission(Permissions.ChangeNickname) ||
                    userPer.HasPermission(Permissions.ManageNicknames))
                {
                    await member.ModifyAsync(s => s.Nickname = nickname);
                    await ctx.RespondAsync($"Set the nickname of {member.Username} to {nickname}.");
                }
                else
                    throw new Exception("I cannot allow you to do that.");
            }
        }

        [Command("mute")]
        [Aliases("m", "silent", "quiet")]
        [RequireUserPermissions(Permissions.MuteMembers)]
        [Description("(Un)Mutes the member")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Mute(CommandContext ctx, [Description("Member to (Un)Mute")] DiscordMember member,
            [Description("Set to \"true\" to mute, false to undo")]
            bool mute,
            [Description("Reason for the (Un)Mute")] [RemainingText]
            string reason)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await member.SetMuteAsync(mute, reason);
                await ctx.RespondAsync($"{(mute ? "Muted" : "Unmuted")} {member.Username}.");
            }
        }

        [Command("deaf")]
        [Aliases("d")]
        [RequireUserPermissions(Permissions.DeafenMembers)]
        [Description("(Un)Deafen the member")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Deaf(CommandContext ctx, [Description("Member to (Un)Deafen")] DiscordMember member,
            [Description("Set to \"true\" to deafen, false to undo")]
            bool deafen,
            [Description("Reason for the (Un)Deafen")] [RemainingText]
            string reason)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await member.SetDeafAsync(deafen, reason);
                await ctx.RespondAsync($"{(deafen ? "Deafened" : "Undeafened")} {member.Username}.");
            }
        }
    }
}