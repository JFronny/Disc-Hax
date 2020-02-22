using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CC_Functions.Misc;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
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
            if (ctx.Channel.get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync(string.Join("\n",
                    typeof(DiscordUser).GetProperties().Select(s => $"{s.Name}: {s.GetValue(user)}")));
            }
        }

        [Command("member")]
        [Aliases("m")]
        [Description("Prints out information about the specified member")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Member(CommandContext ctx, [Description("Member to print")] DiscordMember member)
        {
            if (ctx.Channel.get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync(string.Join("\n",
                    typeof(DiscordMember).GetProperties().Select(s => $"{s.Name}: {s.GetValue(member)}")));
            }
        }

        [Command("role")]
        [Aliases("r")]
        [Description("Prints out information about the specified role")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Role(CommandContext ctx, [Description("Role to print")] DiscordRole role)
        {
            if (ctx.Channel.get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync(string.Join("\n",
                    typeof(DiscordRole).GetProperties().Select(s => $"{s.Name}: {s.GetValue(role)}")));
            }
        }

        [Command("channel")]
        [Aliases("c")]
        [Description("Prints out information about the current channel")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Member(CommandContext ctx)
        {
            if (ctx.Channel.get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync("Config:\n" + ctx.Channel.getStr() + "\n\nProperties:\n" + string.Join("\n",
                    typeof(DiscordChannel).GetProperties()
                        .Select(s => $"{s.Name}: {s.GetValue(ctx.Channel)}")));
            }
        }

        [Command("guild")]
        [Aliases("g")]
        [Description("Prints out information about the current guild")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Guild(CommandContext ctx)
        {
            if (ctx.Channel.get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync("Config:\n" + ctx.Guild.getStr() + "\n\nProperties:\n" + string.Join("\n",
                    typeof(DiscordGuild).GetProperties()
                        .Select(s => $"{s.Name}: {s.GetValue(ctx.Guild)}")));
            }
        }
    }
}