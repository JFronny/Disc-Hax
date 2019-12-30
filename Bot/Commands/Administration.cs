using System;
using System.Threading.Tasks;
using CC_Functions.Misc;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Shared;
using Shared.Config;

namespace Bot.Commands
{
    [Group("admin")]
    [Description("Commands for administration and debugging")]
    public class Administration : BaseCommandModule
    {
        [Command("ping")]
        [Aliases("pong")]
        [Description("Responds with \"Pong\" if the bot is active")]
        public async Task Ping(CommandContext ctx) =>
            await Ping(ctx.Channel, (c1, c2, c3) => ctx.RespondAsync(c1, c2, c3));

        public static async Task Ping(DiscordChannel Channel,
            Func<string, bool, DiscordEmbed, Task<DiscordMessage>> postMessage)
        {
            if (ConfigManager.get(Channel.getInstance(), ConfigElement.Enabled).TRUE())
                await postMessage($"Pong! ({Program.Bot.Client.Ping}ms)", false, null);
        }

        [Command("config")]
        [RequireUserPermissions(Permissions.Administrator)]
        [Description("Prints or changes the DiscHax-instance config")]
        public async Task ConfigCmd(CommandContext ctx)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled)
                .AND(ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Config)))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync(ConfigManager.getStr(ctx.Channel.getInstance()));
            }
        }

        [Command("config")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ConfigCmd(CommandContext ctx, [Description("Config element to print")]
            ConfigElement element)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled)
                .AND(ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Config)))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync($"{element}: {ConfigManager.get(ctx.Channel.getInstance(), element)}");
            }
        }

        [Command("config")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ConfigCmd(CommandContext ctx, [Description("Config element to change")]
            ConfigElement element, [Description("New value")] bool value)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled)
                .AND(ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Config)))
            {
                await ctx.TriggerTypingAsync();
                ConfigManager.set(ctx.Channel.getInstance(), element, value);
                await ctx.RespondAsync($"Set {element} to {value}");
            }
        }
    }
}