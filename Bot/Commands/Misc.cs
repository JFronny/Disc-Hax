using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
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
using Shared;
using Shared.Config;

namespace Bot.Commands
{
    [Group("misc")]
    [Aliases("r")]
    [Description("Random commands that didn't fit into other categories")]
    public class Misc : BaseCommandModule
    {
        private static readonly string[] answerList =
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
            if (ctx.Channel.get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                DiscordMessage msg = await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = "Poll time!",
                    Description = text
                }.Build());
                ReadOnlyCollection<PollEmoji> pollResult = await Bot.instance.Client.GetInteractivity()
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
            if (ctx.Channel.get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
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
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Emotify(CommandContext ctx, [Description("What should be converted")] [RemainingText]
            string text)
        {
            if (ctx.Channel.get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsyncFix(text.emotify());
            }
        }

        [Command("leetify")]
        [Aliases("1337ify")]
        [Description("Leetifies your text")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Leetify(CommandContext ctx, [Description("What should be leetified")] [RemainingText]
            string text)
        {
            if (ctx.Channel.get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsyncFix(text.leetify());
            }
        }

        [Command("preview")]
        [Description("Paginates a website for preview")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task PreviewSite(CommandContext ctx, [Description("URL to paginate site from")] [RemainingText]
            Uri URL)
        {
            if (ctx.Channel.get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                if (URL.IsLocal())
                    throw new WebException("Error: NameResolutionFailure");
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
                await ctx.RespondPaginated(HTMLProcessor.ToPlainText(html));
            }
        }

        [Command("magic8")]
        [Description("The answer to your questions")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Magic8(CommandContext ctx, [Description("Question to answer")] [RemainingText]
            string question)
        {
            if (ctx.Channel.get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                Rectangle size = new Rectangle(0, 0, 400, 400);
                Bitmap bmp = new Bitmap(size.Width, size.Height);
                Graphics g = Graphics.FromImage(bmp);
                //Background
                g.Clear(Color.White);
                //Main Circle
                g.FillEllipse(Brushes.Black, size);
                //Center circle
                size.Width /= 2;
                size.Height /= 2;
                size.X = size.Width / 2;
                size.Y = size.Height / 2;
                g.DrawEllipse(new Pen(Color.FromArgb(100, 80, 80, 80), 6), size);
                //Triangle
                PointF center = new PointF(size.X + (size.Width / 2), size.Y + (size.Height / 2));
                float radius = size.Width / 2;
                g.FillPolygon(Brushes.Blue, new[]
                {
                    new PointF(center.X - (0.866f * radius), center.Y - (0.5f * radius)),
                    new PointF(center.X + (0.866f * radius), center.Y - (0.5f * radius)),
                    new PointF(center.X, center.Y + radius)
                });
                //Get text scale
                Font font = SystemFonts.DefaultFont;
                font = new Font(font.FontFamily, font.Size * (180f / g.MeasureString("QWERTBTESTSTR", font).Width));
                //Text
                g.DrawString(answerList[Program.rnd.Next(answerList.Length)], font, Brushes.White, size,
                    new StringFormat
                    {
                        LineAlignment = StringAlignment.Center,
                        Alignment = StringAlignment.Center
                    });
                //Save
                g.Flush();
                g.Dispose();
                using (MemoryStream str = new MemoryStream())
                {
                    bmp.Save(str, ImageFormat.Jpeg);
                    str.Position = 0;
                    await ctx.RespondWithFileAsync("Magic8.jpg", str);
                }
            }
        }

        [Command("unshorten")]
        [Description("Unshorten a fishy URL")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Unshorten(CommandContext ctx, [Description("URL to unshorten")] Uri url)
        {
            if (ctx.Channel.get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.AllowAutoRedirect = true;
                req.MaximumAutomaticRedirections = 100;
                WebResponse resp = req.GetResponse();
                await ctx.RespondAsyncFix($"Response is: {resp.ResponseUri}");
            }
        }
    }
}