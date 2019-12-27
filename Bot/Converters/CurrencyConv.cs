using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using Shared;

namespace Bot.Converters
{
    public class CurrencyConv : IArgumentConverter<Currency>
    {
        public async Task<Optional<Currency>> ConvertAsync(string value, CommandContext ctx) =>
            CurrencyConverter.Currencies[value];
    }
}