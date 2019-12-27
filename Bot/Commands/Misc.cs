using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CC_Functions.Misc;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using Shared;
using Shared.Config;

namespace Bot.Commands
{
    public class Misc : BaseCommandModule
    {
        [Command("poll")]
        [Description(
            "Run a poll with reactions. WARNING: Only normal emoticons (:laughing:, :grinning:) are allowed! Special emojis (:one:, :regional_indicator_d:) might cause problems")]
        public async Task Poll(CommandContext ctx, [Description("What to ask")] string text,
            [Description("How long should the poll last.")]
            TimeSpan duration, [Description("What options should people have.")]
            params DiscordEmoji[] options)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled)
                .AND(ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Poll)))
            {
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Title = "Poll time!",
                    Description = text
                };
                DiscordMessage msg = await ctx.RespondAsync(embed: embed.Build());
                ReadOnlyCollection<PollEmoji> pollResult = await Bot.instance.Client.GetInteractivity()
                    .DoPollAsync(msg, options, timeout: duration);
                IEnumerable<string> results = pollResult.Where(xkvp => options.Contains(xkvp.Emoji))
                    .Select(xkvp => $"{xkvp.Emoji}: {xkvp.Voted.Count}");
                await ctx.RespondAsync(string.Join("\n", results));
            }
        }

        [Command("quicktype")]
        [Description("Waits for a response containing a generated code")]
        public async Task QuickType(CommandContext ctx,
            [Description("Bytes to generate. One byte equals two characters")]
            int bytes, [Description("Time before exiting")] TimeSpan time)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled)
                .AND(ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Quicktype)))
            {
                InteractivityExtension interactivity = ctx.Client.GetInteractivity();
                byte[] codebytes = new byte[bytes];
                Program.rnd.NextBytes(codebytes);
                string code = BitConverter.ToString(codebytes).ToLower().Replace("-", "");
                await ctx.RespondAsync($"The first one to type the following code gets a reward: {code.emotify()}");
                InteractivityResult<DiscordMessage> msg =
                    await interactivity.WaitForMessageAsync(xm => xm.Content.Contains(code), time);
                if (msg.TimedOut)
                    await ctx.RespondAsync("Nobody? Really?");
                else
                    await ctx.RespondAsync($"And the winner is: {msg.Result.Author.Mention}");
            }
        }

        [Command("emotify")]
        [Description("Converts your text to emoticons")]
        public async Task Emotify(CommandContext ctx, [Description("What should be converted")]
            params string[] args)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled)
                .AND(ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Emojify)))
                await ctx.RespondAsync(string.Join(" ", args.Select(s => s.emotify())));
        }

        [Command("preview")]
        [Description("Paginates a website for preview")]
        public async Task PreviewSite(CommandContext ctx, [Description("URL to paginate site from")]
            string URL)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled)
                .AND(ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.PreviewSite)))
            {
                string html;
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        html = client.DownloadString(URL);
                    }
                }
                catch
                {
                    await ctx.RespondAsync("Failed to download site");
                    return;
                }
                await ctx.RespondPaginated(HTMLProcessor.StripTags(html));
            }
        }
    }
}