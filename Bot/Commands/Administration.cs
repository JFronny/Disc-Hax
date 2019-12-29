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
    public class Administration : BaseCommandModule
    {
        [Command("ping")]
        [Description("Responds with \"Pong\" if the bot is active")]
        public async Task Ping(CommandContext ctx)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled).TRUE())
                await Ping(ctx.Channel, (c1, c2, c3) => ctx.RespondAsync(c1, c2, c3));
        }

        public static async Task Ping(DiscordChannel Channel,
            Func<string, bool, DiscordEmbed, Task<DiscordMessage>> postMessage)
        {
            if (ConfigManager.get(Channel.getInstance(), ConfigElement.Enabled).TRUE())
                await postMessage("Pong", false, null);
        }

        [Command("config")]
        [Description("Prints or changes the DiscHax-instance config")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ConfigCmd(CommandContext ctx, [Description("Used to set a param: config [key] [true/false]")]
            params string[] args)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled)
                .AND(ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Config)))
            {
                if (args.Length == 0)
                {
                    await ctx.RespondAsync(ConfigManager.getStr(ctx.Channel.getInstance()));
                }
                else
                {
                    ConfigElement el = GenericExtensions.ParseToEnum<ConfigElement>(args[0]);
                    if (args.Length == 1)
                    {
                        await ctx.RespondAsync($"{el}: {ConfigManager.get(ctx.Channel.getInstance(), el)}");
                    }
                    else
                    {
                        bool val = bool.Parse(args[1]);
                        ConfigManager.set(ctx.Channel.getInstance(), el, val);
                        await ctx.RespondAsync($"Set {el} to {val}");
                    }
                }
            }
        }
    }
}