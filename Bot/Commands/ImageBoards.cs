using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    public class ImageBoards : BaseCommandModule
    {
        public static readonly Dictionary<string, Booru> booruDict = new Dictionary<string, Booru>();

        public ImageBoards()
        {
            Console.Write("Instantiating Boorus...");
            List<Booru> boorus = new List<Booru>
            {
                new Atfbooru(), new DanbooruDonmai(), new E621(), new E926(),
                new Furrybooru(), new Konachan(), new Lolibooru(), new Realbooru(), new Rule34(),
                new Safebooru(), new Sakugabooru(), new Xbooru(), new Yandere()
            };
            boorus.Sort((x, y) =>
                x.ToString().Split('.')[2].ToLower().CompareTo(y.ToString().Split('.')[2].ToLower()));
            boorus.ToList().ForEach(s => { booruDict.Add(s.ToString().Split('.')[2].ToLower(), s); });
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
        public async Task Booru(CommandContext ctx, [Description("Include VERY questionable content?")]
            bool qcont)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled)
                .AND(ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Booru)))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync(qcont ? string.Join("; ", booruDict.Keys) : "safebooru");
            }
        }

        [Command("booru")]
        public async Task Booru(CommandContext ctx, [Description("Booru to select image from")]
            Booru booru,
            [Description("Tags for image selection")]
            params string[] tags)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled)
                    .AND(ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Booru))
                && (booru == booruDict["safebooru"] || ctx.Channel.getEvaluatedNSFW()))
            {
                await ctx.TriggerTypingAsync();
                SearchResult? result = null;
                while (result == null || result.Value.rating !=
                       (ctx.Channel.getEvaluatedNSFW() ? Rating.Explicit : Rating.Safe))
                    result = await booru.GetRandomImage(tags);
                string val = Program.rnd.Next(10000, 99999).ToString();
                using (WebClient wClient = new WebClient())
                {
                    await ctx.RespondWithFileAsync($"{val}_img.jpg",
                        wClient.OpenRead(result.Value.fileUrl), result.Value.source, embed: new DiscordEmbedBuilder
                        {
                            Description = string.Join(", ", result.Value.tags)
                        }.Build());
                }
            }
        }

        [Command("booru")]
        public async Task Booru(CommandContext ctx,
            [Description("Tags for image selection")]
            params string[] tags) =>
            Booru(ctx, ctx.Channel.getEvaluatedNSFW() ? (Booru) new Rule34() : new Gelbooru(), tags);
    }
}