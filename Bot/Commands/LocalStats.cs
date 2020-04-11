using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CC_Functions.Misc;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Shared;
using Shared.Config;

namespace Bot.Commands
{
    [Group("info")]
    [Aliases("i")]
    [Description("Information that is unique to this server")]
    public class LocalStats : BaseCommandModule
    {
        [Command("user")]
        [Aliases("u")]
        [Description("Prints out information about the specified user")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task User(CommandContext ctx, [Description("User to print")] DiscordUser user)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondPaginatedIfTooLong(string.Join("\n",
                    typeof(DiscordUser).GetProperties()
                        .Select(s =>
                        {
                            try
                            {
                                return $"{s.Name}: {s.GetValue(user)}";
                            }
                            catch
                            {
                                return $"Could not read {s.Name}";
                            }
                        })));
            }
        }

        [Command("member")]
        [Aliases("m")]
        [Description("Prints out information about the specified member")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Member(CommandContext ctx, [Description("Member to print")] DiscordMember member)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondPaginatedIfTooLong(string.Join("\n",
                    typeof(DiscordMember).GetProperties()
                        .Select(s =>
                        {
                            try
                            {
                                return $"{s.Name}: {s.GetValue(member)}";
                            }
                            catch
                            {
                                return $"Could not read {s.Name}";
                            }
                        })));
            }
        }

        [Command("role")]
        [Aliases("r")]
        [Description("Prints out information about the specified role")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Role(CommandContext ctx, [Description("Role to print")] DiscordRole role)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondPaginatedIfTooLong(string.Join("\n",
                    typeof(DiscordRole).GetProperties()
                        .Select(s =>
                        {
                            try
                            {
                                return $"{s.Name}: {s.GetValue(role)}";
                            }
                            catch
                            {
                                return $"Could not read {s.Name}";
                            }
                        })));
            }
        }

        [Command("channel")]
        [Aliases("c")]
        [Description("Prints out information about the specified channel")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Member(CommandContext ctx, [Description("Channel to print, leave empty for current")]
            DiscordChannel? channel = null)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                if (channel == null)
                    channel = ctx.Channel;
                await ctx.RespondPaginatedIfTooLong(
                    $"Config:\n{channel.GetStr(CommandArr.GetCommandNames())}\n\nProperties:\n{string.Join("\n", typeof(DiscordChannel).GetProperties().Select(s => { try { return $"{s.Name}: {s.GetValue(channel)}"; } catch { return $"Could not read {s.Name}"; } }))}");
            }
        }

        [Command("guild")]
        [Aliases("g")]
        [Description("Prints out information about the current guild")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Guild(CommandContext ctx)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondPaginatedIfTooLong(
                    $"Config:\n{ctx.Guild.GetStr(CommandArr.GetCommandNames())}\n\nProperties:\n{string.Join("\n", typeof(DiscordGuild).GetProperties().Select(s => { try { return $"{s.Name}: {s.GetValue(ctx.Guild)}"; } catch { return $"Could not read {s.Name}"; } }))}");
            }
        }
    }
}