using System.Threading.Tasks;
using BooruSharp.Booru;
using Bot.Commands;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace Bot.Converters
{
    public class BooruConv : IArgumentConverter<ABooru>
    {
        public async Task<Optional<ABooru>> ConvertAsync(string value, CommandContext ctx) =>
            ImageBoards.booruDict[value.ToLower()];
    }
}