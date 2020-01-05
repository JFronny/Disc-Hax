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

namespace Bot.Commands
{
    [Group("money")]
    [Description("Commands to manage your money\nYou earn money by sending messages or by gambling (games)")]
    public class Money : BaseCommandModule
    {
        [Command("balance")]
        [Description("Gets the money you have/someone has")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task GetMoney(CommandContext ctx)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigManager.ENABLED)
                .AND(ConfigManager.getMethodEnabled(ctx.Channel.getInstance())))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync($"You have {ConfigManager.getMoney(ctx.Guild.getInstance(), ctx.User)} coins");
            }
        }

        [Command("balance")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task GetMoney(CommandContext ctx, [Description("User whose balance to get")]
            DiscordUser user)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigManager.ENABLED)
                .AND(ConfigManager.getMethodEnabled(ctx.Channel.getInstance())))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync(
                    $"{user.Username} has {ConfigManager.getMoney(ctx.Guild.getInstance(), user)} coins");
            }
        }

        [Command("balance")]
        [RequireUserPermissions(Permissions.Administrator)]
        [Description("Sets the money you have/someone has")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task SetMoney(CommandContext ctx, [Description("User whose balance to set")]
            DiscordUser user, [Description("New value")] decimal money)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigManager.ENABLED)
                .AND(ConfigManager.getMethodEnabled(ctx.Channel.getInstance())))
            {
                await ctx.TriggerTypingAsync();
                decimal original = ConfigManager.getMoney(ctx.Guild.getInstance(), user);
                ConfigManager.setMoney(ctx.Guild.getInstance(), user, money);
                await ctx.RespondAsync(
                    $"{user.Username} now has {ConfigManager.getMoney(ctx.Guild.getInstance(), user)} coins instead of {original}");
            }
        }

        [Command("scoreboard")]
        [RequireUserPermissions(Permissions.Administrator)]
        [Description("Gets the members with the biggest wallet")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Scoreboard(CommandContext ctx)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigManager.ENABLED)
                .AND(ConfigManager.getMethodEnabled(ctx.Channel.getInstance())))
            {
                await ctx.TriggerTypingAsync();
                KeyValuePair<ulong, decimal>[] tmp = ConfigManager.getAllMoney(ctx.Guild.getInstance())
                    .OrderByDescending(s => s.Value).ToArray();
                tmp = tmp.Length > 10 ? tmp.Where((s, i) => i < 10).ToArray() : tmp;
                await ctx.RespondAsync(
                    $"Richest members:\r\n{string.Join("\n", tmp.Select(s => $"{ctx.Guild.Members[s.Key].DisplayName}: {s.Value}"))}");
            }
        }
    }
}