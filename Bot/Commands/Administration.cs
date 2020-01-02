using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CC_Functions.Misc;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Shared;
using Shared.Config;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

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
            if (ConfigManager.get(Channel.getInstance(), ConfigManager.ENABLED).TRUE())
                await postMessage($"Pong! ({Program.Bot.Client.Ping}ms)", false, null);
        }

        [Command("config")]
        [RequireUserPermissions(Permissions.Administrator)]
        [Description("Prints or changes the DiscHax-instance config")]
        public async Task ConfigCmd(CommandContext ctx)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigManager.ENABLED)
                .AND(ConfigManager.getMethodEnabled(ctx.Channel.getInstance())))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync(ConfigManager.getStr(ctx.Channel.getInstance()));
            }
        }

        [Command("config")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ConfigCmd(CommandContext ctx, [Description("Config element to print")]
            string element)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigManager.ENABLED)
                .AND(ConfigManager.getMethodEnabled(ctx.Channel.getInstance())))
            {
                await ctx.TriggerTypingAsync();
                if (!CommandArr.getC().Contains(element))
                    throw new ArgumentException($"Element ({element}) not in CommandArr");
                await ctx.RespondAsync($"{element}: {ConfigManager.get(ctx.Channel.getInstance(), element)}");
            }
        }

        [Command("config")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ConfigCmd(CommandContext ctx, [Description("Config element to change")]
            string element, [Description("New value")] bool value)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigManager.ENABLED)
                .AND(ConfigManager.getMethodEnabled(ctx.Channel.getInstance())))
            {
                await ctx.TriggerTypingAsync();
                if (!CommandArr.getC().Contains(element))
                    throw new ArgumentException($"Element ({element}) not in CommandArr");
                ConfigManager.set(ctx.Channel.getInstance(), element, value);
                await ctx.RespondAsync($"Set {element} to {value}");
            }
        }

        [Command("avatar")]
        [Aliases("icon")]
        public async Task Avatar(CommandContext ctx, [Description("User to get icon from")] DiscordUser user)
        {
            await ctx.TriggerTypingAsync();
            WebClient wc = new WebClient();
            Image bmp = Image.FromStream(wc.OpenRead(user.AvatarUrl));
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Jpeg);
            ms.Position = 0;
            await ctx.RespondWithFileAsync("avatar.jpg", ms, $"Avatar of {user.Username}");
            wc.Dispose();
        }
    }
}