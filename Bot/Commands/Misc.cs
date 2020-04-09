using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CC_Functions.Misc;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using Eto.Drawing;
using Shared;
using Shared.Config;

namespace Bot.Commands
{
    [Group("misc")]
    [Aliases("r")]
    [Description("Random commands that didn't fit into other categories")]
    public class Misc : BaseCommandModule
    {
        private static readonly string[] AnswerList =
        {
            "IT IS\nCERTAIN",
            "IT IS\nDECIDEDLY\nSO",
            "YES\nDEFINITELY",
            "YOU\nMAY RELY\nON IT",
            "AS I\nSEE IT,\nYES",
            "MOST\nLIKELY",
            "YES",
            "REPLY HAZY,\nTRY AGAIN",
            "ASK\nAGAIN\nLATER",
            "DON'T\nCOUNT\nON IT",
            "VERY\nDOUBTFUL",
            "WITHOUT A\nDOUBT",
            "OUTLOOK\nGOOD",
            "SIGNS\nPOINT TO\nYES",
            "BETTER\nNOT TELL\nYOU NOW",
            "CANNOT\nPREDICT\nNOW",
            "CONCENTRATE\nAND ASK\nAGAIN",
            "MY REPLY\nIS NO",
            "MY SOURCES\nSAY NO",
            "OUTLOOK\nNOT SO\nGOOD"
        };

        [Command("poll")]
        [Description(
            "Run a poll with reactions")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Poll(CommandContext ctx, [Description("What to ask")] string text,
            [Description("How long should the poll last.")]
            TimeSpan duration, [Description("What options should people have.")]
            params DiscordEmoji[] options)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                DiscordMessage msg = await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = "Poll time!",
                    Description = text
                }.Build());
                ReadOnlyCollection<PollEmoji> pollResult = await Program.Bot.GetInteractivity()
                    .DoPollAsync(msg, options, timeout: duration);
                IEnumerable<string> results = pollResult.Where(xkvp => options.Contains(xkvp.Emoji))
                    .Select(xkvp => $"{xkvp.Emoji}: {xkvp.Voted.Count}");
                await ctx.RespondAsync(string.Join("\n", results));
            }
        }

        [Command("quicktype")]
        [Description("Waits for a response containing a generated code")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task QuickType(CommandContext ctx,
            [Description("Bytes to generate. One byte equals two characters")]
            int bytes, [Description("Time before exiting")] TimeSpan time)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                InteractivityExtension interactivity = ctx.Client.GetInteractivity();
                byte[] codeBytes = new byte[bytes];
                Program.Rnd.NextBytes(codeBytes);
                string code = BitConverter.ToString(codeBytes).ToLower().Replace("-", "");
                await ctx.RespondAsync($"The first one to type the following code gets a reward: {code.Emotify()}");
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
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Emotify(CommandContext ctx, [Description("What should be converted")] [RemainingText]
            string text)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsyncFix(text.Emotify());
            }
        }

        [Command("leetify")]
        [Aliases("1337ify")]
        [Description("Leetifies your text")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Leetify(CommandContext ctx, [Description("What should be leetified")] [RemainingText]
            string text)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsyncFix(text.Leetify());
            }
        }

        [Command("preview")]
        [Description("Paginates a website for preview")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task PreviewSite(CommandContext ctx, [Description("URL to paginate site from")] [RemainingText]
            Uri url)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                if (url.IsLocal())
                    throw new WebException("Error: NameResolutionFailure");
                string html;
                try
                {
                    using WebClient client = new WebClient();
                    html = client.DownloadString(url);
                }
                catch
                {
                    await ctx.RespondAsync("Failed to download site");
                    return;
                }
                await ctx.RespondPaginatedIfTooLong(HtmlProcessor.ToPlainText(html));
            }
        }

        [Command("magic8")]
        [Description("The answer to your questions")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Magic8(CommandContext ctx, [Description("Question to answer")] [RemainingText]
            string question)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                Rectangle size = new Rectangle(0, 0, 400, 400);
                Bitmap bmp = new Bitmap(size.Size, PixelFormat.Format32bppRgb);
                Graphics g = new Graphics(bmp);
                //Background
                g.Clear(Colors.White);
                //Main Circle
                g.FillEllipse(Brushes.Black, size);
                //Center circle
                size.Width /= 2;
                size.Height /= 2;
                size.X = size.Width / 2;
                size.Y = size.Height / 2;
                g.DrawEllipse(new Pen(Color.FromArgb(100, 80, 80, 80), 6), size);
                //Triangle
                PointF center = new PointF(size.X + size.Width / 2, size.Y + size.Height / 2);
                float radius = size.Width / 2f;
                g.FillPolygon(Brushes.Blue, new PointF(center.X - 0.866f * radius, center.Y - 0.5f * radius),
                    new PointF(center.X + 0.866f * radius, center.Y - 0.5f * radius),
                    new PointF(center.X, center.Y + radius));
                Font font = SystemFonts.Default();
                font = new Font(font.Family, font.Size * (180f / g.MeasureString(font, "QWERTBTESTSTR").Width));
                string answer = AnswerList[Program.Rnd.Next(AnswerList.Length)];
                size.Top = (int) System.Math.Round(size.Center.Y - g.MeasureString(font, answer).Height / 2);
                g.DrawText(font, Brushes.White, size, answer, FormattedTextWrapMode.Word,
                    FormattedTextAlignment.Center);
                g.Flush();
                g.Dispose();
                await using MemoryStream str = new MemoryStream();
                bmp.Save(str, ImageFormat.Jpeg);
                str.Position = 0;
                await ctx.RespondWithFileAsync("Magic8.jpg", str);
            }
        }

        [Command("unshorten")]
        [Description("Unshorten a fishy URL")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Unshorten(CommandContext ctx, [Description("URL to unshorten")] Uri url)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsyncFix($"Response is: {url.Unshorten().AbsoluteUri}");
            }
        }

        [Command("toxicity")]
        [Aliases("tox")]
        [Description("Calculate the specified users toxicity")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Toxicity(CommandContext ctx, [Description("Member to calculate")] DiscordMember member)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                IReadOnlyList<DiscordMessage> msg = await ctx.Channel.GetMessagesAsync();
                IEnumerable<string?> messages = msg.Where(s => s.Author.Id == member.Id).Select(x => x.Content);
                IEnumerable<DiscordEmbed> embeds = msg.Where(s => s.Embeds != null).SelectMany(s => s.Embeds);
                messages = messages.Concat(embeds
                    .SelectMany(s => new[] {s.Title, s.Description}
                        .Concat((s.Fields ?? new DiscordEmbedField[0]).SelectMany(a => new[] {a.Name, a.Value})))
                );
                messages = messages.Where(s => !string.IsNullOrWhiteSpace(s));
                string str = string.Join("\n", messages);
                await ctx.RespondAsyncFix(
                    $"Toxicity: {(await Program.Perspective.RequestAnalysis(str)).AttributeScores.First().Value.SummaryScore.Value}");
            }
        }

        [Command("minecraft")]
        [Aliases("mc")]
        [Description("Gets the status of a minecraft server")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Minecraft(CommandContext ctx, [Description("IP of the server")] string ip)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                string[] parsedIp = ip.Split(':');
                ushort port = parsedIp.Length == 1 ? (ushort) 25565 : ushort.Parse(parsedIp[1]);
                await ctx.RespondAsyncFix(StatReader.ReadStat(parsedIp[0], port));
            }
        }

        [Command("quote")]
        [Aliases("q")]
        [Description("Quote a message")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Quote(CommandContext ctx)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                DiscordMessage msg = (await ctx.Channel.GetMessagesAsync()).Skip(1).First(s => !s.Author.IsBot);
                await msg.Quote(ctx);
            }
        }

        [Command("quote")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Quote(CommandContext ctx, [Description("The messages ID")] ulong id)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                DiscordMessage msg = await ctx.Channel.GetMessageAsync(id);
                await msg.Quote(ctx);
            }
        }
    }
}