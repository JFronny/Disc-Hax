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
    [Aliases("$")]
    [Description("Commands to manage your money\nYou earn money by sending messages or by gambling (games)")]
    public class Money : BaseCommandModule
    {
        [Command("balance")]
        [Description("Gets the money you have/someone has")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task GetMoney(CommandContext ctx)
        {
            if (ctx.Channel.get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync($"You have {ctx.Guild.getMoney(ctx.Member)} coins");
            }
        }

        [Command("balance")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task GetMoney(CommandContext ctx, [Description("User whose balance to get")]
            DiscordMember user)
        {
            if (ctx.Channel.get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync(
                    $"{user.Username} has {ctx.Guild.getMoney(user)} coins");
            }
        }

        [Command("balance")]
        [RequireUserPermissions(Permissions.Administrator)]
        [Description("Sets the money you have/someone has")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task SetMoney(CommandContext ctx, [Description("User whose balance to set")]
            DiscordMember user, [Description("New value")] decimal money)
        {
            if (ctx.Channel.get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                decimal original = ctx.Guild.getMoney(user);
                ctx.Guild.setMoney(user, money);
                await ctx.RespondAsync(
                    $"{user.Username} now has {ctx.Guild.getMoney(user)} coins instead of {original}");
            }
        }

        [Command("scoreboard")]
        [RequireUserPermissions(Permissions.Administrator)]
        [Description("Gets the members with the biggest wallet")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Scoreboard(CommandContext ctx)
        {
            if (ctx.Channel.get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                KeyValuePair<ulong, decimal>[] tmp = ctx.Guild.getAllMoney()
                    .OrderByDescending(s => s.Value).ToArray();
                tmp = tmp.Length > 10 ? tmp.Where((s, i) => i < 10).ToArray() : tmp;
                await ctx.RespondAsync(
                    $"Richest members:\r\n{string.Join("\n", tmp.Select(s => $"{ctx.Guild.Members[s.Key].DisplayName}: {s.Value}"))}");
            }
        }

        [Command("give")]
        [Description("Give someone money")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task GiveMoney(CommandContext ctx, [Description("User to give money to")]
            DiscordMember user, [Description("Money to give")] decimal money)
        {
            if (ctx.Channel.get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                if (money > ctx.Guild.getMoney(ctx.Member) || money < 0)
                {
                    await ctx.RespondAsync("You don't have that much");
                    return;
                }
                ctx.Guild.incrementMoney(user, money);
                ctx.Guild.incrementMoney(ctx.Member, -money);
                await ctx.RespondAsync($"Gave {user.Username} {money} coins.");
            }
        }
    }
}