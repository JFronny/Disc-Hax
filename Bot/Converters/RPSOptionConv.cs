using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace Bot.Converters
{
    public class RPSOptionConv : IArgumentConverter<RPSOptionConv.RPSOption>
    {
        public enum RPSOption
        {
            Rock,
            Paper,
            Scissor
        }

        public static Array soptions = Enum.GetValues(typeof(RPSOption));

        public async Task<Optional<RPSOption>> ConvertAsync(string value, CommandContext ctx)
        {
            string tmp1 = value.ToLower();
            return soptions.OfType<RPSOption>().First(
                s =>
                {
                    string tmp2 = s.ToString().ToLower();
                    return tmp2.StartsWith(tmp1) || tmp2.EndsWith(tmp1) || tmp1.StartsWith(tmp2) || tmp1.EndsWith(tmp2);
                });
        }
    }
}