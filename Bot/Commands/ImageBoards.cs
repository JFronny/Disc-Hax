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
    public class ImageBoards : BaseCommandModule
    {
        private static readonly Dictionary<string, Booru> booruDict = new Dictionary<string, Booru>();

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
        public async Task Chan(CommandContext ctx, [Description("Board to select image from")]
            params string[] args)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled)
                .AND(ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Chan)))
                if (args.Length == 0)
                {
                    List<string> lmsg = new List<string>();
                    List<BoardInfo> lboard = JsonDeserializer
                        .Deserialize<BoardListModel>(await Internet.DownloadString(@"https://a.4cdn.org/boards.json")
                            .ConfigureAwait(false)).boards;
                    lboard.ForEach(s => { lmsg.Add($"{s.Title} ({s.ShortName})"); });
                    await ctx.RespondAsync($"{string.Join(", ", lmsg)}\r\nUsage: !4chan <ShortName>");
                }
                else
                {
                    Board b = new Board(args[0]);
                    if (ctx.Channel.getEvaluatedNSFW())
                    {
                        Thread[] threads = b.GetThreads().ToArray();
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
        public async Task Waifu(CommandContext ctx,
            [Description("Use \"f\" to force execution, even on non-NSFW channels")]
            params string[] args)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled)
                .AND(ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Waifu)))
                if (ctx.Channel.getEvaluatedNSFW() || args.Contains("f"))
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
                        "The generated waifus might not be something you want to be looking at at work. You can override this with the \"f\"-Flag");
                }
        }

        [Command("booru")]
        [Description("Shows a random Image from your favourite *booru. See [booru list] for a full list")]
        public async Task Booru(CommandContext ctx,
            [Description("Tags for image selection, first element can be a source")]
            params string[] args)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled)
                .AND(ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Booru)))
            {
                if (args.Length == 1 && args[0].ToLower() == "list")
                {
                    await ctx.RespondAsync(string.Join("; ", booruDict.Keys));
                }
                else if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Booru).TRUE())
                {
                    Booru booru;
                    if (args.Length > 0 && booruDict.ContainsKey(args[0].ToLower()))
                    {
                        booru = booruDict[args[0]];
                        args = args.ToList().selectO(s =>
                        {
                            s.RemoveAt(0);
                            return s;
                        }).ToArray();
                    }
                    else
                    {
                        booru = ctx.Channel.getEvaluatedNSFW() ? (Booru) new Rule34() : new Gelbooru();
                    }

                    SearchResult? result = null;
                    while (result == null || result.Value.rating !=
                           (ctx.Channel.getEvaluatedNSFW() ? Rating.Explicit : Rating.Safe))
                        result = await booru.GetRandomImage(args);
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
        }
    }
}