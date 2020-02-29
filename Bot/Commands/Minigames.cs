using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Bot.Converters;
using CC_Functions.Misc;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Shared.Config;
using static System.Math;

namespace Bot.Commands
{
    [Group("game")]
    [Aliases("g")]
    [Description("Simple games")]
    public class Minigames : BaseCommandModule
    {
        [Command("rps")]
        [Description("Play Rock-Paper-Scissors")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task RPS(CommandContext ctx, [Description("Input (=Rock/Paper/Scissor)")]
            RPSOptionConv.RPSOption Option)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                string output;
                RPSOptionConv.RPSOption rpsOption =
                    (RPSOptionConv.RPSOption) RPSOptionConv.soptions.GetValue(
                        Program.Rnd.Next(RPSOptionConv.soptions.Length));
                output = $"You chose: {Option}, I chose {rpsOption}. This means ";
                int diff = (int) rpsOption - (int) Option;
                diff = diff switch {-2 => 1, 2 => -1, _ => diff};
                output += diff switch
                {
                    -1 => "You've",
                    0 => "No-one has",
                    1 => "I've",
                    _ => throw new Exception($"This should not happen! (diff={diff})")
                };
                output += " won";
                await ctx.RespondAsync(output);
            }
        }

        [Command("rps")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task RPS(CommandContext ctx, [Description("Input (=Rock/Paper/Scissor)")]
            RPSOptionConv.RPSOption Option, [Description("Amount of coinst to bet")]
            decimal bet)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                if (bet > ctx.Guild.GetMoney(ctx.Member) || bet < 0)
                {
                    await ctx.RespondAsync("You don't have that much");
                    return;
                }
                string output;
                RPSOptionConv.RPSOption rpsOption =
                    (RPSOptionConv.RPSOption) RPSOptionConv.soptions.GetValue(
                        Program.Rnd.Next(RPSOptionConv.soptions.Length));
                output = $"You chose: {Option}, I chose {rpsOption}. This means ";
                int diff = (int) rpsOption - (int) Option;
                diff = diff switch {-2 => 1, 2 => -1, _ => diff};
                output += diff switch
                {
                    -1 => "You've",
                    0 => "No-one has",
                    1 => "I've",
                    _ => throw new Exception($"This should not happen! (diff={diff})")
                };
                output += " won";
                ctx.Guild.IncrementMoney(ctx.Member, -bet * diff);
                await ctx.RespondAsync(output);
            }
        }

        [Command("slots")]
        [Description("Play Slots. No arguments for values")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Slots(CommandContext ctx)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync(@":first_place::first_place::grey_question: - 0.5x
:gem::gem::grey_question: - 2x
:100::100::grey_question: - 2x
:first_place::first_place::first_place: - 2.5x
:gem::gem::gem: - 3x
:dollar::dollar::grey_question: - 3.5x
:100::100::100: - 4x
:moneybag::moneybag::grey_question: - 7x
:dollar::dollar::dollar: - 7x
:moneybag::moneybag::moneybag: - 15x");
            }
        }

        [Command("slots")]
        [Description("Play Slots.")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Slots(CommandContext ctx, [Description("Amount of coinst to bet")]
            decimal bet) => await Slots(ctx, bet, false);

        [Command("slots")]
        [Description("Play Slots.")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Slots(CommandContext ctx, [Description("Amount of coinst to bet")]
            decimal bet, [Description("Whether to skip the animation")]
            bool fast)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                if (bet > ctx.Guild.GetMoney(ctx.Member) || bet < 0)
                {
                    await ctx.RespondAsync("You don't have that much");
                    return;
                }
                int[] pool = {Program.Rnd.Next(5), Program.Rnd.Next(5), Program.Rnd.Next(5)};
                DiscordMessage msg = await ctx.RespondAsync("| :grey_question: | :grey_question: | :grey_question: |");
                if (!fast)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        await msg.ModifyAsync(
                            $"| {getSlot(Program.Rnd.Next(5))} | :grey_question: | :grey_question: |");
                        await Task.Delay(1000);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        await msg.ModifyAsync(
                            $"| {getSlot(pool[0])} | {getSlot(Program.Rnd.Next(5))} | :grey_question: |");
                        await Task.Delay(1000);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        await msg.ModifyAsync(
                            $"| {getSlot(pool[0])} | {getSlot(pool[1])} | {getSlot(Program.Rnd.Next(5))} |");
                        await Task.Delay(1000);
                    }
                    await msg.ModifyAsync($"| {getSlot(pool[0])} | {getSlot(pool[1])} | {getSlot(pool[2])} |");
                }
                pool = pool.OrderBy(s => s).ToArray();
                decimal winnings = 0;
                if (pool[0] == pool[1] || pool[1] == pool[2])
                {
                    if (pool[0] != pool[2])
                        winnings = bet * (decimal) (pool[1] switch
                        {
                            0 => 0.5,
                            1 => 2,
                            2 => 2,
                            3 => 3.5,
                            4 => 7
                        });
                    else
                        winnings = (decimal) ((double) bet * pool[1] switch
                        {
                            0 => 2.5,
                            1 => 3,
                            2 => 4,
                            3 => 7,
                            4 => 15
                        });
                }
                winnings -= bet;
                await msg.ModifyAsync(
                    $"| {getSlot(pool[0])} | {getSlot(pool[1])} | {getSlot(pool[2])} |\nYou {(winnings > 0 ? "won" : "lost")} {Abs((double) winnings)} coins.");
                ctx.Guild.IncrementMoney(ctx.Member, winnings);
            }
        }

        private string getSlot(int val) => val switch
        {
            0 => ":first_place:",
            1 => ":gem:",
            2 => ":100:",
            3 => ":dollar:",
            4 => ":moneybag:"
        };
    }
}