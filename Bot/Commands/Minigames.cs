using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Bot.Converters;
using CC_Functions.Misc;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Shared;
using Shared.Config;

namespace Bot.Commands
{
    [Group("game")]
    [Description("Simple games")]
    public class Minigames : BaseCommandModule
    {
        [Command("rps")]
        [Description("Play Rock-Paper-Scissors")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Currency(CommandContext ctx, [Description("Input (=Rock/Paper/Scissor)")]
            RPSOptionConv.RPSOption Option)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigManager.ENABLED)
                .AND(ConfigManager.getMethodEnabled(ctx.Channel.getInstance())))
            {
                await ctx.TriggerTypingAsync();
                string output;
                RPSOptionConv.RPSOption rpsOption =
                    (RPSOptionConv.RPSOption) RPSOptionConv.soptions.GetValue(
                        Program.rnd.Next(RPSOptionConv.soptions.Length));
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
    }
}