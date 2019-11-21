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
using Bot.Config;
using System.Net;
using DSharpPlus.Interactivity;

namespace Bot
{
    public class Commands
    {
        [Command("4chan"), Description("Sends a random image from the board. If no board is specified, a list of boards will be displayed.")]
        public async Task Chan(CommandContext ctx, [Description("Board to select image from")] params string[] args) => await Chan(args, ctx.Channel, (c1, c2, c3) => ctx.RespondAsync(c1, c2, c3));

        public static async Task Chan(string[] args, DiscordChannel Channel, Func<string, bool, DiscordEmbed, Task<DiscordMessage>> postMessage)
        {
            if (ChCfgMgr.getCh(Channel.Id, ConfigElement.Enabled) && ChCfgMgr.getCh(Channel.Id, ConfigElement.Chan))
                if (args.Length == 0)
                {
                    List<string> lmsg = new List<string>();
                    List<BoardInfo> lboard = JsonDeserializer.Deserialize<BoardListModel>(await Internet.DownloadString(@"https://a.4cdn.org/boards.json").ConfigureAwait(false)).boards;
                    lboard.ForEach(s =>
                    {
                        lmsg.Add(s.Title + " (" + s.ShortName + ")");
                    });
                    await postMessage(string.Join(", ", lmsg) + "\r\nUsage: !4chan <ShortName>", false, null);
                }
                else
                {
                    Board b = new Board(args[0]);
                    if (Channel.getEvaluatedNSFW())
                    {
                        Thread[] threads = b.GetThreads().ToArray();
                        Thread t = threads[MainForm.Instance.rnd.Next(threads.Length)];
                        await postMessage("https://boards.4channel.org/" + t.Board.BoardId + "/thread/" + t.PostNumber, false, new DiscordEmbedBuilder { Title = t.Name + "#" + t.Id + ": " + (string.IsNullOrWhiteSpace(t.Subject) ? "Untitled" : t.Subject), ImageUrl = t.Image.Image.AbsoluteUri });
                    }
                    else
                        await postMessage("Due to the way 4chan users behave, this command is only allowed in NSFW channels", false, null);
                }
        }

        [Command("waifu"), Description("Shows you a random waifu from thiswaifudoesnotexist.net")]
        public async Task Waifu(CommandContext ctx, [Description("Use \"f\" to force execution, even on non-NSFW channels")] params string[] args) => await Waifu(args, ctx.Channel, (c1, c2, c3) => ctx.RespondAsync(c1, c2, c3));

        public static async Task Waifu(string[] args, DiscordChannel Channel, Func<string, bool, DiscordEmbed, Task<DiscordMessage>> postMessage)
        {
            if (ChCfgMgr.getCh(Channel.Id, ConfigElement.Enabled) && ChCfgMgr.getCh(Channel.Id, ConfigElement.Waifu))
                if (Channel.getEvaluatedNSFW() || args.Contains("f"))
                    await postMessage(null, false, new DiscordEmbedBuilder { Title = "There.", ImageUrl = "https://www.thiswaifudoesnotexist.net/example-" + MainForm.Instance.rnd.Next(6000).ToString() + ".jpg" });
                else
                    await postMessage("The generated waifus might not be something you want to be looking at at work. You can override this with the \"f\"-Flag", false, null);
        }

        [Command("play"), Description("Sends \"No.\" to the chat. For users used to RhythmBot.")]
        public async Task Play(CommandContext ctx) => await Play(ctx.Channel, (c1, c2, c3) => ctx.RespondAsync(c1, c2, c3));

        public static async Task Play(DiscordChannel Channel, Func<string, bool, DiscordEmbed, Task<DiscordMessage>> postMessage)
        {
            if (ChCfgMgr.getCh(Channel.Id, ConfigElement.Enabled) && ChCfgMgr.getCh(Channel.Id, ConfigElement.Play))
                await postMessage("No.", false, null);
        }

        [Command("beemovie"), Description("Sends a quote from the bee movie script as TTS")]
        public async Task Bees(CommandContext ctx) => await Bees(ctx.Channel, (c1, c2, c3) => ctx.RespondAsync(c1, c2, c3));

        public static async Task Bees(DiscordChannel Channel, Func<string, bool, DiscordEmbed, Task<DiscordMessage>> postMessage)
        {
            if (ChCfgMgr.getCh(Channel.Id, ConfigElement.Enabled) && ChCfgMgr.getCh(Channel.Id, ConfigElement.Bees))
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
                await postMessage((_beequotes[q] + "\n\n" + _beequotes[q + 1] + "\n\n" + _beequotes[q + 2]).Replace("\n", "\r\n"), true, null);
            }
        }

        static string[] _beequotes;

        [Command("ping"), Description("Responds with \"Pong\" if the bot is active")]
        public async Task Ping(CommandContext ctx) => await Ping(ctx.Channel, (c1, c2, c3) => ctx.RespondAsync(c1, c2, c3));

        public static async Task Ping(DiscordChannel Channel, Func<string, bool, DiscordEmbed, Task<DiscordMessage>> postMessage)
        {
            if (ChCfgMgr.getCh(Channel.Id, ConfigElement.Enabled))
                await postMessage("Pong", false, null);
        }

        [Command("booru"), Description("Shows a random Image from your favourite *booru. See [booru list] for a full list")]
        public async Task Booru(CommandContext ctx, [Description("Tags for image selection, first element can be a source")] params string[] args) => await Booru(args, ctx.Channel, (c1, c2, c3) => ctx.RespondAsync(c1, c2, c3));

        public static async Task Booru(string[] args, DiscordChannel Channel, Func<string, bool, DiscordEmbed, Task<DiscordMessage>> postMessage)
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
            if (ChCfgMgr.getCh(Channel.Id, ConfigElement.Enabled) && ChCfgMgr.getCh(Channel.Id, ConfigElement.Booru))
            {
                if (args.Length == 1 && args[0].ToLower() == "list")
                {
                    await postMessage(string.Join("; ", booruDict.Keys), false, null);
                }
                else if (ChCfgMgr.getCh(Channel.Id, ConfigElement.Booru))
                {
                    Booru booru = (args.Length > 0 && booruDict.ContainsKey(args[0].ToLower())) ? booruDict[args[0]]
                        : (Channel.getEvaluatedNSFW() ? (Booru)new Rule34() : new Gelbooru());
                    args = args.ToList().mod(s => { s.RemoveAt(0); return s; }).ToArray();
                    SearchResult? result = null;
                    while (result == null || result.Value.rating != (Channel.getEvaluatedNSFW() ? Rating.Explicit : Rating.Safe))
                    {
                        result = await booru.GetRandomImage(args);
                    }
                    await postMessage(null, false, new DiscordEmbedBuilder { Title = result.Value.source, Description = string.Join(", ", result.Value.tags), ImageUrl = result.Value.fileUrl.AbsoluteUri });
                }
            }
        }

        static Dictionary<string, Booru> booruDict = new Dictionary<string, Booru>();

        [Command("config"), Description("Prints or changes the DiscHax-bot config"), RequireUserPermissions(Permissions.Administrator)]
        public async Task ConfigCmd(CommandContext ctx, [Description("Used to set a param: config [key] [true/false]")] string[] args) => await ConfigCmd(args, ctx.Channel, (c1, c2, c3) => ctx.RespondAsync(c1, c2, c3));

        public static async Task ConfigCmd(string[] args, DiscordChannel Channel, Func<string, bool, DiscordEmbed, Task<DiscordMessage>> postMessage)
        {
            if (ChCfgMgr.getCh(Channel.Id, ConfigElement.Enabled) && ChCfgMgr.getCh(Channel.Id, ConfigElement.Config))
            {
                if (args.Length == 0)
                    await postMessage(ChCfgMgr.getChStr(Channel.Id), false, null);
                else if (args.Length == 1)
                {
                    if (Enum.TryParse(args[0].ToLower(), out ConfigElement el))
                        await postMessage(ChCfgMgr.getCh(Channel.Id, el).ToString(), false, null);
                    else
                        await postMessage("Config key not found", false, null);
                }
                else
                {
                    if (Enum.TryParse(args[0].ToLower(), out ConfigElement el) && bool.TryParse(args[1], out bool val))
                    {
                        ChCfgMgr.setCh(Channel.Id, el, val);
                        await postMessage("Set " + el.ToString() + " to " + val.ToString(), false, null);
                    }
                    else
                        await postMessage("Not found or invalid value", false, null);
                }
            }
        }

        [Command("poll"), Description("Run a poll with reactions")]
        public async Task Poll(CommandContext ctx, [Description("How long should the poll last.")] TimeSpan duration, [Description("What options should people have.")] params DiscordEmoji[] options) => await Poll(ctx.Channel, (c1, c2, c3) => ctx.RespondAsync(c1, c2, c3), duration, options);

        public async Task Poll(DiscordChannel Channel, Func<string, bool, DiscordEmbed, Task<DiscordMessage>> postMessage, TimeSpan duration, params DiscordEmoji[] options)
        {
            IEnumerable<string> poll_options = options.Select(xe => xe.ToString());
            var embed = new DiscordEmbedBuilder
            {
                Title = "Poll time!",
                Description = string.Join(" ", poll_options)
            };
            var msg = await postMessage(null, false, embed);
            for (var i = 0; i < options.Length; i++)
                await msg.CreateReactionAsync(options[i]);
            var poll_result = await Bot.instance.Client.GetInteractivityModule().CollectReactionsAsync(msg, duration);
            var results = poll_result.Reactions.Where(xkvp => options.Contains(xkvp.Key))
                .Select(xkvp => $"{xkvp.Key}: {xkvp.Value}");
            await postMessage(string.Join("\n", results), false, null);
        }
    }
}
