using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace Bot.Converters
{
    public class DoujinEnumConv : IArgumentConverter<DoujinEnumConv.DoujinEnum>
    {
        public enum DoujinEnum
        {
            JavMost,
            EHentai,
            Nhentai,
            Ls
        }

        public static Array Soptions = Enum.GetValues(typeof(DoujinEnum));

        public async Task<Optional<DoujinEnum>> ConvertAsync(string value, CommandContext ctx)
        {
            string tmp1 = value.ToLower();
            return Soptions.OfType<DoujinEnum>().First(
                s =>
                {
                    string tmp2 = s.ToString().ToLower();
                    return tmp2.StartsWith(tmp1) || tmp2.EndsWith(tmp1) || tmp1.StartsWith(tmp2) || tmp1.EndsWith(tmp2);
                });
        }
    }
}