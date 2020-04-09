using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace Bot.Converters
{
    public class RpsOptionConv : IArgumentConverter<RpsOptionConv.RpsOption>
    {
        public enum RpsOption
        {
            Rock,
            Paper,
            Scissor
        }

        public static Array Soptions = Enum.GetValues(typeof(RpsOption));

        public async Task<Optional<RpsOption>> ConvertAsync(string value, CommandContext ctx)
        {
            string tmp1 = value.ToLower();
            return Soptions.OfType<RpsOption>().First(
                s =>
                {
                    string tmp2 = s.ToString().ToLower();
                    return tmp2.StartsWith(tmp1) || tmp2.EndsWith(tmp1) || tmp1.StartsWith(tmp2) || tmp1.EndsWith(tmp2);
                });
        }
    }
}