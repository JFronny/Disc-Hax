using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace Bot.Converters
{
    public class HashTypeConv : IArgumentConverter<HashTypeConv.HashType>
    {
        public enum HashType
        {
            Sha1,
            Sha256,
            Md5
        }

        public static Array Soptions = Enum.GetValues(typeof(HashType));

        public async Task<Optional<HashType>> ConvertAsync(string value, CommandContext ctx)
        {
            string tmp1 = value.ToLower();
            return Soptions.OfType<HashType>().First(
                s =>
                {
                    string tmp2 = s.ToString().ToLower();
                    return tmp2.StartsWith(tmp1) || tmp2.EndsWith(tmp1) || tmp1.StartsWith(tmp2) || tmp1.EndsWith(tmp2);
                });
        }
    }
}