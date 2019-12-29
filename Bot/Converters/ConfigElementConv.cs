using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using Shared.Config;

namespace Bot.Converters
{
    public class ConfigElementConv : IArgumentConverter<ConfigElement>
    {
        public async Task<Optional<ConfigElement>> ConvertAsync(string value, CommandContext ctx) =>
            (ConfigElement) Enum.Parse(typeof(ConfigElement), value, true);
    }
}