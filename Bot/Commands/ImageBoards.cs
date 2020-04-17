using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using BooruSharp.Booru;
using BooruSharp.Search.Post;
using Bot.Converters;
using CC_Functions.Misc;
using Chan.Net;
using Chan.Net.JsonModel;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;
using NHentaiSharp.Core;
using NHentaiSharp.Exception;
using NHentaiSharp.Search;
using Shared;
using Shared.Config;
using SearchResult = BooruSharp.Search.Post.SearchResult;

namespace Bot.Commands
{
    [Group("board")]
    [Aliases("b")]
    [Description("Commands to get random images from image-boards around the interwebz")]
    public class ImageBoards : BaseCommandModule
    {
        public static Dictionary<string, ABooru> BooruDict;

        public ImageBoards()
        {
            Console.Write("Instantiating Boorus...");
            BooruDict = Assembly.GetAssembly(typeof(ABooru)).GetTypes()
                .Where(s => s.Namespace == "BooruSharp.Booru" && s.IsClass && !s.IsAbstract &&
                            s.IsSubclassOf(typeof(ABooru)))
                .Select(s => (ABooru) Activator.CreateInstance(s, new object?[] {null}))
                .OrderBy(s => s.ToString().Split('.')[2].ToLower()).ToList()
                .ToDictionary(s => s.ToString().Split('.')[2].ToLower(), s => s);
            Console.WriteLine(" Finished.");
/*#if !NO_NSFW
            Console.Write("Instantiating JavMostCategories...");
            JavMostCategories.Add("censor");
            JavMostCategories.Add("uncensor");
            int page = 1;
            try
            {
                using HttpClient hc = new HttpClient {Timeout = TimeSpan.FromSeconds(5.0)};
                List<string> newTags;
                do
                {
                    string html = hc.GetStringAsync($"https://www5.javmost.com/allcategory/{page}").Result;
                    newTags = Regex
                        .Matches(html, "<a href=\"https:\\/\\/www5\\.javmost\\.com\\/category\\/([^\\/]+)\\/\">")
                        .Select(m => m.Groups[1].Value.Trim().ToLower())
                        .Where(content => !JavMostCategories.Contains(content)).ToList();
                    JavMostCategories.AddRange(newTags);
                    page++;
                } while (newTags.Count > 0);
            }
            catch (HttpRequestException)
            {
                if (!Debugger.IsAttached)
                    throw;
            }
            catch (TaskCanceledException)
            {
            }
            Console.WriteLine(" Finished.");
#endif*/
        }

        [Command("booru")]
        [Aliases("b")]
        [Description("Shows a random Image from your favourite *booru. See \"booru\" for a full list")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Booru(CommandContext ctx
#if !NO_NSFW
            , [Description("Include questionable content?")]
            bool qcont
#endif
        )
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync(
#if !NO_NSFW
                    qcont
                        ? string.Join("; ", BooruDict.Keys)
                        :
#endif
                        string.Join("; ", BooruDict.Keys.Where(s => BooruDict[s].IsSafe())));
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
                SearchResult result;
                int triesLeft = 10;
                do
                {
                    if (triesLeft == 0)
                        throw new Exception("Failed to find image in a reasonable amount of tries");
                    result = await booru.GetRandomImageAsync(tags);
                    triesLeft--;
                } while (result.rating != (
#if !NO_NSFW
                    ctx.Channel.GetEvaluatedNsfw()
                        ? Rating.Explicit
                        :
#endif
                        Rating.Safe));
                string val = Program.Rnd.Next(10000, 99999).ToString();
                using WebClient wClient = new WebClient();
                await ctx.RespondWithFileAsync($"{val}_img.jpg",
                    wClient.OpenRead(result.fileUrl ?? result.previewUrl), embed: new DiscordEmbedBuilder
                    {
                        Description = $"Tags: {string.Join(", ", result.tags)}",
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
            await Booru(ctx,
#if !NO_NSFW
                ctx.Channel.GetEvaluatedNsfw()
                    ? (ABooru) new Rule34()
                    :
#endif
                    new Safebooru(), tags);

#if !NO_NSFW
        [Command("nonbooru")]
        [Aliases("d")]
        [Description("Shows a random non-booru from your favourite source. See \"doujinshi ls\" for a full list")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task NonBooru(CommandContext ctx, [Description("Source to select image from (ls for a list)")]
            DoujinEnumConv.DoujinEnum source,
            [Description("Tags for image selection")]
            params string[] tags)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                if (ctx.Channel.GetEvaluatedNsfw())
                {
                    using WebClient wClient = new WebClient();
                    string val = Program.Rnd.Next(10000, 99999).ToString();
                    string html;
                    string url;
                    switch (source)
                    {
                        case DoujinEnumConv.DoujinEnum.Nhentai:
                        {
                            NHentaiSharp.Search.SearchResult result;
                            try
                            {
                                result = await (tags.Length == 0
                                    ? SearchClient.SearchAsync()
                                    : SearchClient.SearchWithTagsAsync(tags));
                            }
                            catch (InvalidArgumentException)
                            {
                                await ctx.RespondAsync("Not found");
                                break;
                            }
                            int page = Program.Rnd.Next(0, result.numPages) + 1;
                            result = await (tags.Length == 0
                                ? SearchClient.SearchAsync(page)
                                : SearchClient.SearchWithTagsAsync(tags, page));
                            GalleryElement doujinshi = result.elements[Program.Rnd.Next(0, result.elements.Length)];
                            await ctx.RespondWithFileAsync($"{val}_img.jpg",
                                wClient.OpenRead(doujinshi.cover.imageUrl.Unshorten()), embed: new DiscordEmbedBuilder
                                {
                                    Description = $"Tags: {string.Join(", ", doujinshi.tags.Select(s => s.name))}",
                                    Title = $"{doujinshi.japaneseTitle} ({doujinshi.prettyTitle})",
                                    Url = doujinshi.url.ToString(),
                                    Footer = new DiscordEmbedBuilder.EmbedFooter
                                    {
                                        Text = "Click on the title to access the doujin page."
                                    }
                                }.Build());
                            break;
                        }
                        case DoujinEnumConv.DoujinEnum.EHentai:
                            url =
                                $"https://e-hentai.org/?f_cats=959&f_search={Uri.EscapeDataString(string.Join(" ", tags))}";
                            int randomDoujinshi;
                            string imageUrl;
                            List<string> allTags = new List<string>();
                            string finalUrl;
                            using (HttpClient hc = new HttpClient())
                            {
                                html = await hc.GetStringAsync(url);
                                Match m = Regex.Match(html, "Showing ([0-9,]+) result");
                                if (!m.Success)
                                {
                                    await ctx.RespondAsync("Not found");
                                    break;
                                }
                                randomDoujinshi = Program.Rnd.Next(0, int.Parse(m.Groups[1].Value.Replace(",", "")));
                                html = await hc.GetStringAsync($"{url}&page={randomDoujinshi / 25}");
                                finalUrl =
                                    Regex.Matches(html, "<a href=\"(https:\\/\\/e-hentai\\.org\\/g\\/[^\"]+)\"")[
                                        randomDoujinshi % 25].Groups[1].Value;
                                html = await hc.GetStringAsync(finalUrl);
                                string htmlTags = html.Split(new[] {"taglist"}, StringSplitOptions.None)[1]
                                    .Split(new[] {"Showing"}, StringSplitOptions.None)[0];
                                foreach (Match match in Regex.Matches(htmlTags, ">([^<]+)<\\/a><\\/div>"))
                                    allTags.Add(match.Groups[1].Value);
                                string htmlCover = await hc.GetStringAsync(Regex
                                    .Match(html, "<a href=\"([^\"]+)\"><img alt=\"0*1\"").Groups[1].Value);
                                imageUrl = Regex.Match(htmlCover, "<img id=\"img\" src=\"([^\"]+)\"").Groups[1].Value;
                            }
                            await ctx.RespondWithFileAsync($"{val}_img.jpg",
                                wClient.OpenRead(imageUrl), embed: new DiscordEmbedBuilder
                                {
                                    Description = $"Tags: {string.Join(", ", allTags.ToArray())}",
                                    Title = HttpUtility.HtmlDecode(Regex
                                        .Match(html, "<title>(.+) - E-Hentai Galleries<\\/title>").Groups[1].Value),
                                    Url = finalUrl,
                                    Footer = new DiscordEmbedBuilder.EmbedFooter
                                    {
                                        Text = "Click on the title to access the doujin page."
                                    }
                                }.Build());
                            break;
                        /*case DoujinEnumConv.DoujinEnum.JavMost:
                            string tag = tags.Length > 0 ? string.Join(" ", tags).ToLower() : "";
                            if (tags.Length > 0 && !JavMostCategories.Contains(tag))
                            {
                                await ctx.RespondAsync("Not found");
                                break;
                            }
                            if (tag == "")
                                tag = "all";
                            int perPage;
                            int total;
                            url = $"https://www5.javmost.com/category/{tag}";
                            using (HttpClient hc = new HttpClient())
                            {
                                html = await hc.GetStringAsync(url);
                                perPage = Regex.Matches(html, "<!-- begin card -->").Count;
                                total = int.Parse(Regex.Match(html,
                                        "<input type=\"hidden\" id=\"page_total\" value=\"([0-9]+)\" \\/>").Groups[1]
                                    .Value);
                            }
                            Match videoMatch;
                            string[] videoTags = null;
                            string previewUrl = "";
                            int nbTry = 0;
                            do
                            {
                                int video = Program.Rnd.Next(0, total);
                                int pageNumber = video / perPage;
                                int pageIndex = video % perPage;
                                if (pageNumber > 0)
                                {
                                    using HttpClient hc = new HttpClient();
                                    html = await hc.GetStringAsync($"{url}/page/{pageNumber + 1}");
                                }
                                int index = pageIndex + 1;
                                string[] arr = html.Split(new[] {"<!-- begin card -->"}, StringSplitOptions.None);
                                if (index >= arr.Length)
                                {
                                    videoMatch = Regex.Match("", "a");
                                    continue;
                                }
                                string videoHtml = arr[index];
                                videoMatch = Regex.Match(videoHtml,
                                    "<a href=\"(https:\\/\\/www5\\.javmost\\.com\\/([^\\/]+)\\/)\"");
                                previewUrl = Regex.Match(videoHtml, "data-src=\"([^\"]+)\"").Groups[1].Value;
                                if (previewUrl.StartsWith("//"))
                                    previewUrl = $"https:{previewUrl}";
                                videoTags = Regex
                                    .Matches(videoHtml,
                                        "<a href=\"https:\\/\\/www5\\.javmost\\.com\\/category\\/([^\\/]+)\\/\"")
                                    .Select(x => x.Groups[1].Value).ToArray();
                                nbTry++;
                                if (nbTry <= 10) continue;
                                await ctx.RespondAsync("Not found");
                                break;
                            } while (!videoMatch.Success);
                            await ctx.RespondWithFileAsync($"{val}_img.jpg",
                                wClient.OpenRead(previewUrl), embed: new DiscordEmbedBuilder
                                {
                                    Description =
                                        $"Tags: {string.Join(", ", videoTags ?? throw new Exception("Unexpected internal val"))}",
                                    Title = videoMatch.Groups[2].Value,
                                    Url = videoMatch.Groups[1].Value,
                                    Footer = new DiscordEmbedBuilder.EmbedFooter
                                    {
                                        Text = "Click on the title to access the doujin page."
                                    }
                                }.Build());
                            break;*/
                        case DoujinEnumConv.DoujinEnum.Ls:
                            await ctx.RespondAsync(string.Join("; ", Enum.GetNames(typeof(DoujinEnumConv.DoujinEnum))));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(source), source, null);
                    }
                }
                else
                    await ctx.RespondAsync("NSFW Channels only!");
            }
        }
#endif

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
                while (
#if !NO_NSFW
                    ctx.Channel.GetEvaluatedNsfw() !=
#endif
                    jToken["over_18"].Value<bool>())
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
                    await using Stream s = client.OpenRead(address);
                    await ctx.RespondWithFileAsync(Path.GetFileName(address), s, content, embed: builder.Build());
                }
                catch
                {
                    try
                    {
                        string str = jToken["media"]["reddit_video"]["fallback_url"].Value<string>();
                        await using Stream s = client.OpenRead(str);
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
        [Description("Gets a random image from inspirobot")]
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

        [Command("xkcd")]
        [Aliases("x")]
        [Description("Gets a random image from xkcd")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Xkcd(CommandContext ctx)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                using WebClient client = new WebClient();
                int c = Program.Rnd.Next(1,
                    JObject.Parse(client.DownloadString("https://xkcd.com/info.0.json")).Value<int>("num") + 1);
                JObject comic = JObject.Parse(client.DownloadString($"https://xkcd.com/{c}/info.0.json"));
                string val = Program.Rnd.Next(10000, 99999).ToString();
                await ctx.RespondWithFileAsync($"{val}_img.jpg",
                    client.OpenRead(comic.Value<string>("img")), embed: new DiscordEmbedBuilder
                    {
                        Description = $"Transcript: {comic.Value<string>("alt")}",
                        Title = comic.Value<string>("safe_title"),
                        Url = $"https://xkcd.com/{c}/",
                        Timestamp = new DateTime(int.Parse(comic.Value<string>("year")),
                            int.Parse(comic.Value<string>("month")), int.Parse(comic.Value<string>("day")))
                    }.Build());
            }
        }
#if !NO_NSFW
        //private static readonly List<string> JavMostCategories = new List<string>();
#endif
#if !NO_NSFW
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
                if (ctx.Channel.GetEvaluatedNsfw())
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
                    await ctx.RespondAsync(
                        "Due to the way 4chan users behave, this command is only allowed in NSFW channels");
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
                if (ctx.Channel.GetEvaluatedNsfw() || forceExecution)
                {
                    int img = Program.Rnd.Next(6000);
                    using WebClient wClient = new WebClient();
                    await ctx.RespondWithFileAsync($"{img}.jpg",
                        wClient.OpenRead($"https://www.thiswaifudoesnotexist.net/example-{img}.jpg"),
                        "There.");
                }
                else
                    await ctx.RespondAsync(
                        "The generated waifus might not be something you want to be looking at at work. You can override this.");
            }
        }
#endif

#if !NO_NSFW

        [Command("sauce")]
        [Aliases("s", "source")]
        [Description("Gets the source for an image (provided as an attachment or url)")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Sauce(CommandContext ctx)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                if (ctx.Channel.GetEvaluatedNsfw())
                {
                    if (ctx.Message.Attachments.Count > 0)
                        await Sauce(ctx, ctx.Message.Attachments[0].Url);
                    else
                        await ctx.RespondAsync("You must provide a link or attachment!");
                }
                else
                    await ctx.RespondAsync("NSFW Channels only!");
            }
        }

        [Command("sauce")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Sauce(CommandContext ctx, [Description("The images url")] string url)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                if (ctx.Channel.GetEvaluatedNsfw())
                {
                    string html;
                    using (HttpClient hc = new HttpClient())
                        html = await hc.GetStringAsync(
                            $"https://saucenao.com/search.php?db=999&url={Uri.EscapeDataString(url)}");
                    if (!html.Contains("<div id=\"middle\">"))
                    {
                        await ctx.RespondAsync("Not found");
                        return;
                    }
                    string fullHtml = html;
                    html = html.Split(new[] {"<td class=\"resulttablecontent\">"}, StringSplitOptions.None)[1];
                    float certitude =
                        float.Parse(
                            Regex.Match(html, "<div class=\"resultsimilarityinfo\">([0-9]{2,3}\\.[0-9]{1,2})%<\\/div>")
                                .Groups[1].Value, CultureInfo.InvariantCulture);
                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = TextProcessor.HtmlToPlainText(
                            html.Split(new[] {"<div class=\"resultcontentcolumn\">"}, StringSplitOptions.None)[1]
                                .Split(new[] {"</div>"}, StringSplitOptions.None)[0]),
                        ImageUrl = Regex.Match(fullHtml,
                                "<img title=\"Index #[^\"]+\"( raw-rating=\"[^\"]+\") src=\"(https:\\/\\/img[0-9]+.saucenao.com\\/[^\"]+)\"")
                            .Groups[2].Value,
                        Color = certitude > 80 ? DiscordColor.Green :
                            certitude > 50 ? DiscordColor.Orange : DiscordColor.Red,
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Certitude: {certitude}%"
                        }
                    }.Build());
                }
                else
                    await ctx.RespondAsync("NSFW Channels only!");
            }
        }
#endif
    }
}