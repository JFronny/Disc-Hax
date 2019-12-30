using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using BooruSharp.Booru;
using BooruSharp.Search.Post;
using CC_Functions.Misc;
using Chan.Net;
using Chan.Net.JsonModel;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Shared;
using Shared.Config;

namespace Bot.Commands
{
    [Group("board")]
    [Description("Commands to get random images from image-boards around the interwebz")]
    public class ImageBoards : BaseCommandModule
    {
        public static Dictionary<string, Booru> booruDict;

        public ImageBoards()
        {
            Console.Write("Instantiating Boorus...");
            booruDict = Assembly.GetAssembly(typeof(Booru)).GetTypes()
                .Where(s => s.Namespace == "BooruSharp.Booru" && s.IsClass && !s.IsAbstract &&
                            s.IsSubclassOf(typeof(Booru)))
                .Select(s => (Booru) Activator.CreateInstance(s, new object[] {null}))
                .OrderBy(s => s.ToString().Split('.')[2].ToLower()).ToList()
                .ToDictionary(s => s.ToString().Split('.')[2].ToLower(), s => s);
            Console.WriteLine(" Finished.");
        }

        [Command("4chan")]
        [Description(
            "Sends a random image from the board. If no board is specified, a list of boards will be displayed.")]
        public async Task Chan(CommandContext ctx)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled)
                .AND(ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Chan)))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync(
                    $"{string.Join(", ", JsonDeserializer.Deserialize<BoardListModel>(await Internet.DownloadString(@"https://a.4cdn.org/boards.json")).boards.Select(s => $"{s.Title} ({s.ShortName})"))}\r\nUsage: !4chan <ShortName>");
            }
        }

        [Command("4chan")]
        public async Task Chan(CommandContext ctx, [Description("Board to select image from")]
            Board board)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled)
                .AND(ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Chan)))
            {
                await ctx.TriggerTypingAsync();
                if (ctx.Channel.getEvaluatedNSFW())
                {
                    Thread[] threads = board.GetThreads().ToArray();
                    Thread t = threads[Program.rnd.Next(threads.Length)];
                    using (WebClient wClient = new WebClient())
                    {
                        await ctx.RespondWithFileAsync($"{t.PostNumber}.jpg",
                            wClient.OpenRead(t.Image.Image),
                            $"https://boards.4channel.org/{t.Board.BoardId}/thread/{t.PostNumber}",
                            embed: new DiscordEmbedBuilder
                            {
                                Author = new DiscordEmbedBuilder.EmbedAuthor {Name = t.Name},
                                Timestamp = t.TimeCreated,
                                Title = string.IsNullOrWhiteSpace(t.Subject) ? "Untitled" : t.Subject,
                                Description = t.Message
                            }.Build());
                    }
                }
                else
                {
                    await ctx.RespondAsync(
                        "Due to the way 4chan users behave, this command is only allowed in NSFW channels");
                }
            }
        }

        [Command("waifu")]
        [Description("Shows you a random waifu from thiswaifudoesnotexist.net")]
        public async Task Waifu(CommandContext ctx)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled)
                .AND(ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Waifu)))
                await Waifu(ctx, false);
        }

        [Command("waifu")]
        public async Task Waifu(CommandContext ctx,
            [Description("Set to true to force execution, even on non-NSFW channels")]
            bool forceExecution)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled)
                .AND(ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Waifu)))
            {
                await ctx.TriggerTypingAsync();
                if (ctx.Channel.getEvaluatedNSFW() || forceExecution)
                {
                    int img = Program.rnd.Next(6000);
                    using (WebClient wClient = new WebClient())
                    {
                        await ctx.RespondWithFileAsync($"{img}.jpg",
                            wClient.OpenRead($"https://www.thiswaifudoesnotexist.net/example-{img}.jpg"),
                            "There.");
                    }
                }
                else
                {
                    await ctx.RespondAsync(
                        "The generated waifus might not be something you want to be looking at at work. You can override this.");
                }
            }
        }

        [Command("booru")]
        [Description("Shows a random Image from your favourite *booru. See \"booru\" for a full list")]
        public async Task Booru(CommandContext ctx, [Description("Include questionable content?")]
            bool qcont)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled)
                .AND(ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Booru)))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync(qcont
                    ? string.Join("; ", booruDict.Keys)
                    : string.Join("; ", booruDict.Keys.Where(s => booruDict[s].IsSafe())));
            }
        }

        [Command("booru")]
        public async Task Booru(CommandContext ctx, [Description("Booru to select image from")]
            Booru booru,
            [Description("Tags for image selection")]
            params string[] tags)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled)
                .AND(ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Booru)))
            {
                await ctx.TriggerTypingAsync();
                SearchResult result = await booru.GetRandomImage(tags);
                int triesLeft = 10;
                while (result.rating != (ctx.Channel.getEvaluatedNSFW() ? Rating.Explicit : Rating.Safe) &&
                       !booru.IsSafe())
                {
                    if (triesLeft == 0)
                        throw new Exception("Failed to find image in a reasonable amount of tries");
                    result = await booru.GetRandomImage(tags);
                    triesLeft--;
                }
                string val = Program.rnd.Next(10000, 99999).ToString();
                using (WebClient wClient = new WebClient())
                {
                    await ctx.RespondWithFileAsync($"{val}_img.jpg",
                        wClient.OpenRead(result.fileUrl), embed: new DiscordEmbedBuilder
                        {
                            Description = "Tags: " + string.Join(", ", result.tags),
                            Title = result.source ?? "Unknown source",
                            Url = result.fileUrl.ToString()
                        }.Build());
                }
            }
        }

        [Command("booru")]
        public async Task Booru(CommandContext ctx,
            [Description("Tags for image selection")]
            params string[] tags) =>
            await Booru(ctx, ctx.Channel.getEvaluatedNSFW() ? (Booru) new Rule34() : new Safebooru(), tags);
    }
}