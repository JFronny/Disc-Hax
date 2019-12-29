using System.Threading.Tasks;
using Chan.Net;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace Bot.Converters
{
    public class BoardConv : IArgumentConverter<Board>
    {
        public async Task<Optional<Board>> ConvertAsync(string value, CommandContext ctx) => new Board(value);
    }
}