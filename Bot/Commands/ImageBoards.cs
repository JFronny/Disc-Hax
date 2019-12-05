using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chan.Net;
using Chan.Net.JsonModel;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using BooruSharp;
using BooruSharp.Booru;
using BooruSharp.Search.Post;
using System.Net;
using DSharpPlus.Interactivity;
using Shared.Config;
using Shared;

namespace Bot.Commands
{
    public class ImageBoards
    {
        [Command("4chan"), Description("Sends a random image from the board. If no board is specified, a list of boards will be displayed.")]
        public async Task Chan(CommandContext ctx, [Description("Board to select image from")] params string[] args)
        {
            if (ConfigManager.get(ctx.Channel.Id, ConfigElement.Enabled).AND(ConfigManager.get(ctx.Channel.Id, ConfigElement.Chan)))
                if (args.Length == 0)
                {
                    List<string> lmsg = new List<string>();
                    List<BoardInfo> lboard = JsonDeserializer.Deserialize<BoardListModel>(await Internet.DownloadString(@"https://a.4cdn.org/boards.json").ConfigureAwait(false)).boards;
                    lboard.ForEach(s =>
                    {
                        lmsg.Add(s.Title + " (" + s.ShortName + ")");
                    });
                    await ctx.RespondAsync(string.Join(", ", lmsg) + "\r\nUsage: !4chan <ShortName>");
                }
                else
                {
                    Board b = new Board(args[0]);
                    if (ctx.Channel.getEvaluatedNSFW())
                    {
                        Thread[] threads = b.GetThreads().ToArray();
                        Thread t = threads[MainForm.Instance.rnd.Next(threads.Length)];
                        await ctx.RespondAsync("https://boards.4channel.org/" + t.Board.BoardId + "/thread/" + t.PostNumber, embed: new DiscordEmbedBuilder { Title = t.Name + "#" + t.Id + ": " + (string.IsNullOrWhiteSpace(t.Subject) ? "Untitled" : t.Subject), ImageUrl = t.Image.Image.AbsoluteUri });
                    }
                    else
                        await ctx.RespondAsync("Due to the way 4chan users behave, this command is only allowed in NSFW channels");
                }
        }

        [Command("waifu"), Description("Shows you a random waifu from thiswaifudoesnotexist.net")]
        public async Task Waifu(CommandContext ctx, [Description("Use \"f\" to force execution, even on non-NSFW channels")] params string[] args)
        {
            if (ConfigManager.get(ctx.Channel.Id, ConfigElement.Enabled).AND(ConfigManager.get(ctx.Channel.Id, ConfigElement.Waifu)))
                if (ctx.Channel.getEvaluatedNSFW() || args.Contains("f"))
                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder { Title = "There.", ImageUrl = "https://www.thiswaifudoesnotexist.net/example-" + MainForm.Instance.rnd.Next(6000).ToString() + ".jpg" });
                else
                    await ctx.RespondAsync("The generated waifus might not be something you want to be looking at at work. You can override this with the \"f\"-Flag");
        }

        [Command("beemovie"), Description("Sends a quote from the bee movie script as TTS")]
        public async Task Bees(CommandContext ctx)
        {
            if (ConfigManager.get(ctx.Channel.Id, ConfigElement.Enabled).AND(ConfigManager.get(ctx.Channel.Id, ConfigElement.Bees)))
            {
                if (_beequotes == null)
                    using (WebClient client = new WebClient())
                    {
                        _beequotes = client.DownloadString("http://www.script-o-rama.com/movie_scripts/a1/bee-movie-script-transcript-seinfeld.html")
                            .Split(new string[] { "<pre>" }, StringSplitOptions.None)[1]
                            .Split(new string[] { "</pre>" }, StringSplitOptions.None)[0]
                            .Split(new string[] { "\n\n  \n" }, StringSplitOptions.None);
                        _beequotes[0] = _beequotes[0].Replace("  \n  \n", "");
                    }
                int q = MainForm.Instance.rnd.Next(_beequotes.Length - 2);
                await ctx.RespondAsync((_beequotes[q] + "\n\n" + _beequotes[q + 1] + "\n\n" + _beequotes[q + 2]).Replace("\n", "\r\n"), true, null);
            }
        }

        static string[] _beequotes;

        [Command("booru"), Description("Shows a random Image from your favourite *booru. See [booru list] for a full list")]
        public async Task Booru(CommandContext ctx, [Description("Tags for image selection, first element can be a source")] params string[] args)
        {
            if (booruDict.Count == 0)
            {
                List<Booru> boorus = new List<Booru> { new Atfbooru(), new DanbooruDonmai(), new E621(), new E926(),
                    new Furrybooru(), new Konachan(), new Lolibooru(), new Realbooru(), new Rule34(),
                    new Safebooru(), new Sakugabooru(), new Xbooru(), new Yandere() };
                boorus.Sort((x, y) => x.ToString().Split('.')[2].ToLower().CompareTo(y.ToString().Split('.')[2].ToLower()));
                boorus.ToList().ForEach(s => {
                    booruDict.Add(s.ToString().Split('.')[2].ToLower(), s);
                });
            }
            if (ConfigManager.get(ctx.Channel.Id, ConfigElement.Enabled).AND(ConfigManager.get(ctx.Channel.Id, ConfigElement.Booru)))
            {
                if (args.Length == 1 && args[0].ToLower() == "list")
                {
                    await ctx.RespondAsync(string.Join("; ", booruDict.Keys), false, null);
                }
                else if (ConfigManager.get(ctx.Channel.Id, ConfigElement.Booru).TRUE())
                {
                    Booru booru;
                    if (args.Length > 0 && booruDict.ContainsKey(args[0].ToLower()))
                    {
                        booru = booruDict[args[0]];
                        args = args.ToList().mod(s => { s.RemoveAt(0); return s; }).ToArray();
                    }
                    else
                        booru = (ctx.Channel.getEvaluatedNSFW() ? (Booru)new Rule34() : new Gelbooru());
                    SearchResult? result = null;
                    while (result == null || result.Value.rating != (ctx.Channel.getEvaluatedNSFW() ? Rating.Explicit : Rating.Safe))
                    {
                        result = await booru.GetRandomImage(args);
                    }
                    await ctx.RespondAsync(null, false, new DiscordEmbedBuilder { Title = result.Value.source, Description = string.Join(", ", result.Value.tags), ImageUrl = result.Value.fileUrl.AbsoluteUri });
                }
            }
        }

        static Dictionary<string, Booru> booruDict = new Dictionary<string, Booru>();
    }
}
