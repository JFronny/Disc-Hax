using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BooruSharp.Booru;
using BooruSharp.Search.Post;
using CC_Functions.Misc;
using Chan.Net;
using Chan.Net.JsonModel;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;
using Shared;
using Shared.Config;

namespace Bot.Commands
{
    [Group("board")]
    [Aliases("b")]
    [Description("Commands to get random images from image-boards around the interwebz")]
    public class ImageBoards : BaseCommandModule
    {
        public static Dictionary<string, ABooru> booruDict;

        public ImageBoards()
        {
            Console.Write("Instantiating Boorus...");
            booruDict = Assembly.GetAssembly(typeof(ABooru)).GetTypes()
                .Where(s => s.Namespace == "BooruSharp.Booru" && s.IsClass && !s.IsAbstract &&
                            s.IsSubclassOf(typeof(ABooru)))
                .Select(s => (ABooru) Activator.CreateInstance(s, new object?[] {null}))
                .OrderBy(s => s.ToString().Split('.')[2].ToLower()).ToList()
                .ToDictionary(s => s.ToString().Split('.')[2].ToLower(), s => s);
            Console.WriteLine(" Finished.");
        }

        [Command("4chan")]
        [Aliases("4", "chan")]
        [Description(
            "Sends a random image from the board. If no board is specified, a list of boards will be displayed.")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Chan(CommandContext ctx)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync(
                    $"{string.Join(", ", JsonDeserializer.Deserialize<BoardListModel>(await Internet.DownloadString(@"https://a.4cdn.org/boards.json")).boards.Select(s => $"{s.Title} ({s.ShortName})"))}\r\nUsage: !4chan <ShortName>");
            }
        }

        [Command("4chan")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Chan(CommandContext ctx, [Description("Board to select image from")]
            Board board)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                if (ctx.Channel.getEvaluatedNSFW())
                {
                    Thread[] threads = board.GetThreads().ToArray();
                    Thread t = threads[Program.Rnd.Next(threads.Length)];
                    using WebClient wClient = new WebClient();
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
                else
                {
                    await ctx.RespondAsync(
                        "Due to the way 4chan users behave, this command is only allowed in NSFW channels");
                }
            }
        }

        [Command("waifu")]
        [Aliases("w")]
        [Description("Shows you a random waifu from thiswaifudoesnotexist.net")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Waifu(CommandContext ctx)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
                await Waifu(ctx, false);
        }

        [Command("waifu")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Waifu(CommandContext ctx,
            [Description("Set to true to force execution, even on non-NSFW channels")]
            bool forceExecution)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                if (ctx.Channel.getEvaluatedNSFW() || forceExecution)
                {
                    int img = Program.Rnd.Next(6000);
                    using WebClient wClient = new WebClient();
                    await ctx.RespondWithFileAsync($"{img}.jpg",
                        wClient.OpenRead($"https://www.thiswaifudoesnotexist.net/example-{img}.jpg"),
                        "There.");
                }
                else
                {
                    await ctx.RespondAsync(
                        "The generated waifus might not be something you want to be looking at at work. You can override this.");
                }
            }
        }

        [Command("booru")]
        [Aliases("b")]
        [Description("Shows a random Image from your favourite *booru. See \"booru\" for a full list")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Booru(CommandContext ctx, [Description("Include questionable content?")]
            bool qcont)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync(qcont
                    ? string.Join("; ", booruDict.Keys)
                    : string.Join("; ", booruDict.Keys.Where(s => booruDict[s].IsSafe())));
            }
        }

        [Command("booru")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Booru(CommandContext ctx, [Description("Booru to select image from")]
            ABooru booru,
            [Description("Tags for image selection")]
            params string[] tags)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                SearchResult result = await booru.GetRandomImageAsync(tags);
                int triesLeft = 10;
                while (result.rating != (ctx.Channel.getEvaluatedNSFW() ? Rating.Explicit : Rating.Safe) &&
                       !booru.IsSafe())
                {
                    if (triesLeft == 0)
                        throw new Exception("Failed to find image in a reasonable amount of tries");
                    result = await booru.GetRandomImageAsync(tags);
                    triesLeft--;
                }
                string val = Program.Rnd.Next(10000, 99999).ToString();
                using WebClient wClient = new WebClient();
                await ctx.RespondWithFileAsync($"{val}_img.jpg",
                    wClient.OpenRead(result.fileUrl), embed: new DiscordEmbedBuilder
                    {
                        Description = "Tags: " + string.Join(", ", result.tags),
                        Title = result.source ?? "Unknown source",
                        Url = result.fileUrl.ToString()
                    }.Build());
            }
        }

        [Command("booru")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Booru(CommandContext ctx,
            [Description("Tags for image selection")]
            params string[] tags) =>
            await Booru(ctx, ctx.Channel.getEvaluatedNSFW() ? (ABooru) new Rule34() : new Safebooru(), tags);

        [Command("reddit")]
        [Aliases("r")]
        [Description("Shows a post from reddit")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Reddit(CommandContext ctx,
            [Description("Shows top posts of the subreddit instead of random ones")]
            bool topPost = false, [Description("The subreddit to select a post from. Leave empty for a random one")]
            string subreddit = "random")
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                using WebClient client = new WebClient();
                string res =
                    client.DownloadString($"https://www.reddit.com/r/{subreddit}/{(topPost ? "top" : "random")}/.json");
                JToken jToken = (topPost ? JObject.Parse(res) : JArray.Parse(res)[0])["data"]["children"][0]["data"];
                while (ctx.Channel.getEvaluatedNSFW() != jToken["over_18"].Value<bool>())
                {
                    res = client.DownloadString(
                        $"https://www.reddit.com/r/{subreddit}/{(topPost ? "top" : "random")}/.json");
                    jToken = (topPost ? JObject.Parse(res) : JArray.Parse(res)[0])["data"]["children"][0]["data"];
                }
                string content =
                    $"{jToken["author"].Value<string>()} on {jToken["subreddit_name_prefixed"].Value<string>()}";
                DiscordEmbedBuilder builder = new DiscordEmbedBuilder
                {
                    Title = jToken["title"].Value<string>(),
                    Url = $"https://www.reddit.com{jToken["permalink"].Value<string>()}",
                    Description = jToken["selftext"].Value<string>()
                };
                try
                {
                    string address = jToken["url"].Value<string>();
                    using Stream s = client.OpenRead(address);
                    await ctx.RespondWithFileAsync(Path.GetFileName(address), s, content, embed: builder.Build());
                }
                catch
                {
                    try
                    {
                        string str = jToken["media"]["reddit_video"]["fallback_url"].Value<string>();
                        using Stream s = client.OpenRead(str);
                        await ctx.RespondWithFileAsync(Path.GetFileName(str), s, content, embed: builder.Build());
                    }
                    catch
                    {
                        await ctx.RespondAsync(content, embed: builder.Build());
                    }
                }
            }
        }

        [Command("inspirobot")]
        [Aliases("i")]
        [Description("Shows a random Image from your favourite *booru. See \"booru\" for a full list")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Inspirobot(CommandContext ctx, [Description("Set to true for christmas quotes")]
            bool xmas = false)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                using WebClient client = new WebClient();
                string page = client.DownloadString(
                    $"http://inspirobot.me/api?generate=true{(xmas ? "&season=xmas" : "")}");
                await ctx.RespondWithFileAsync(Path.GetFileName(page), client.OpenRead(page));
            }
        }
    }
}