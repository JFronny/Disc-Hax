using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Shared;
using Shared.Config;
using System;
using System.Threading.Tasks;

namespace Bot.Commands
{
    public class Administration
    {
        [Command("ping"), Description("Responds with \"Pong\" if the bot is active")]
        public async Task Ping(CommandContext ctx)
        {
            if (ConfigManager.get(ctx.Channel.Id, ConfigElement.Enabled).TRUE())
                await Ping(ctx.Channel, (c1, c2, c3) => ctx.RespondAsync(c1, c2, c3));
        }

        public static async Task Ping(DiscordChannel Channel, Func<string, bool, DiscordEmbed, Task<DiscordMessage>> postMessage)
        {
            if (ConfigManager.get(Channel.Id, ConfigElement.Enabled).TRUE())
                await postMessage("Pong", false, null);
        }

        [Command("config"), Description("Prints or changes the DiscHax-instance config"), RequireUserPermissions(Permissions.Administrator)]
        public async Task ConfigCmd(CommandContext ctx, [Description("Used to set a param: config [key] [true/false]")] params string[] args)
        {
            if (ConfigManager.get(ctx.Channel.Id, ConfigElement.Enabled).AND(ConfigManager.get(ctx.Channel.Id, ConfigElement.Config)))
            {
                if (args.Length == 0)
                    await ctx.RespondAsync(ConfigManager.getStr(ctx.Channel.Id), false, null);
                else
                {
                    ConfigElement el = ClassExtensions.ParseToEnum<ConfigElement>(args[0]);
                    if (args.Length == 1)
                    {
                        await ctx.RespondAsync(el.ToString() + ": " + ConfigManager.get(ctx.Channel.Id, el).ToString(), false, null);
                    }
                    else
                    {
                        bool val = bool.Parse(args[1]);
                        ConfigManager.set(ctx.Channel.Id, el, val);
                        await ctx.RespondAsync("Set " + el.ToString() + " to " + val.ToString(), false, null);
                    }
                }
            }
        }
    }
}