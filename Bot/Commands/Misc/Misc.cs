using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Bot.Converters;
using CC_Functions.Misc;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using Eto.Drawing;
using QRCoder;
using Shared;
using Shared.Config;
using Shared.QR;

namespace Bot.Commands.Misc
{
    [Group("misc")]
    [Aliases("r")]
    [Description("Random commands that didn't fit into other categories")]
    public partial class Misc : BaseCommandModule
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
                if (duration > new TimeSpan(0, 1, 0))
                    throw new ArgumentOutOfRangeException("Please choose a smaller time");
                DiscordMessage msg = await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = "Poll time!",
                    Description = text
                }.Build());
                ReadOnlyCollection<PollEmoji> pollResult = await Program.client.GetInteractivity()
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
                if (time > new TimeSpan(0, 1, 0))
                    throw new ArgumentOutOfRangeException("Please choose a smaller time");
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
                await ctx.RespondAsync(text.Emotify());
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

        /*[Command("preview")]
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
                await ctx.RespondPaginatedIfTooLong(TextProcessor.HtmlToPlainText(html));
            }
        }*/
        //Disabled until this doesn't allow access to local devices. I prefer not to be hacked

        [Command("random")]
        [Aliases("rnd", "rng", "dice", "r")]
        [Description("Generates a random number")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Random(CommandContext ctx, [Description("The inclusive minimum for the number")]
            int minimum, [Description("The inclusive maximum for the number")]
            int maximum)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                if (maximum < minimum)
                {
                    int tmp = maximum;
                    maximum = minimum;
                    minimum = tmp;
                }
                await ctx.RespondPaginatedIfTooLong(Program.Rnd.Next(minimum, maximum + 1).ToString());
            }
        }
        
        [Command("coinflip")]
        [Aliases("flip")]
        [Description("Flip a coin")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Coinflip(CommandContext ctx)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondPaginatedIfTooLong(Program.Rnd.Next(0, 2) == 0 ? "Head" : "Tail");
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
                g.DrawEllipse(new Pen(Eto.Drawing.Color.FromArgb(100, 80, 80, 80), 6), size);
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
        
        [Command("color")]
        [Aliases("col", "colour")]
        [Description("Show info about a color. Chooses a random color when none is specified")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Color(CommandContext ctx, [Description("The color")] DiscordColor? color = null)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                color ??= DiscordNamedColorConverter.ColorNames.Values.OrderBy(s => Program.Rnd.NextDouble()).First();
                //Image
                Bitmap bmp = new Bitmap(new Size(100, 100), PixelFormat.Format32bppRgb);
                Graphics g = new Graphics(bmp);
                Color col = new Color(color.Value.R, color.Value.G, color.Value.B);
                g.Clear(col);
                g.Flush();
                g.Dispose();
                await using MemoryStream str = new MemoryStream();
                bmp.Save(str, ImageFormat.Jpeg);
                str.Position = 0;
                //Data
                DiscordEmbedBuilder bld = new DiscordEmbedBuilder {Color = color.Value};
                if (DiscordNamedColorConverter.ColorNames.Values.Any(s => s.Value == color.Value.Value))
                    bld.Title = DiscordNamedColorConverter.ColorNames.First(s => s.Value.Value == color.Value.Value)
                        .Key;
                bld.AddField("RGB", $"{color.Value.R}, {color.Value.G}, {color.Value.B}", true);
                bld.AddField("Hex", $"#{color.Value.R:X2}{color.Value.G:X2}{color.Value.B:X2}", true);
                bld.AddField("RGB Int", color.Value.Value.ToString(), true);
                bld.AddField("HSB", col.ToHSB().selectO(s => $"{s.H}, {s.S}, {s.B}"), true);
                bld.AddField("HSL", col.ToHSL().selectO(s => $"{s.H}, {s.S}, {s.L}"), true);
                bld.AddField("CMYK", col.ToCMYK().selectO(s => $"{s.C}, {s.M}, {s.Y}, {s.K}"), true);
                await ctx.RespondWithFileAsync("Color.jpg", str, embed: bld.Build());
            }
        }
        
        [Command("base64")]
        [Aliases("b64", "base")]
        [Description("Encode/Decode base64 strings (you can also attach a text file)")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Base64(CommandContext ctx, [Description("Whether to encode (default) or decode")] bool decode, [Description("The text to process"), RemainingText] string text)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                if (string.IsNullOrWhiteSpace(text) && ctx.Message.Attachments.Count > 0)
                {
                    using WebClient wc = new WebClient();
                    text = wc.DownloadString(ctx.Message.Attachments[0].Url);
                }
                if (decode)
                {
                    byte[] data = Convert.FromBase64String(text);
                    try
                    {
                        await using MemoryStream ms = new MemoryStream(data);
                        using StreamReader rd = new StreamReader(ms);
                        await ctx.RespondAsync(await rd.ReadToEndAsync());
                    }
                    catch
                    {
                        await using MemoryStream ms = new MemoryStream(data);
                        await ctx.RespondWithFileAsync("blob.bin", ms, "Could not decode to string, using raw data");
                    }
                }
                else
                    await ctx.RespondAsync(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(text)));
            }
        }
        
        [Command("echo")]
        [Aliases("repeat")]
        [Description("Repeat a string back to you")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Echo(CommandContext ctx, [Description("The text to echo"), RemainingText] string text)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
                await ctx.RespondAsync(text);
        }
        
        [Command("reverse")]
        [Aliases("rev")]
        [Description("Reverse a string")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task Reverse(CommandContext ctx, [Description("The text to echo"), RemainingText] string text) => Reverse(ctx, false, text);

        [Command("reverse")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task Reverse(CommandContext ctx, bool reverseWords, [Description("The text to echo"), RemainingText] string text)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
                return ctx.RespondAsync(reverseWords ? string.Join(' ', text.Split(' ').Reverse()) : text);
            else
                return Task.CompletedTask;
        }
        
        [Command("generate-qr")]
        [Aliases("gen-qr", "mkqr")]
        [Description("Generate a QR Code from supplied data (string or attachment)")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task GenerateQR(CommandContext ctx, [Description("The text to echo"), RemainingText] string text)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                Bitmap bmp;
                if (string.IsNullOrWhiteSpace(text) && ctx.Message.Attachments.Count > 0)
                {
                    using WebClient wc = new WebClient();
                    QRCodeData data = QRGenerator.GenerateQrCode(wc.DownloadData(ctx.Message.Attachments[0].Url), QRGenerator.ECCLevel.Q);
                    QRCode code = new QRCode(data);
                    bmp = code.GetGraphic(20);
                }
                else
                {
                    QRCodeData data = QRGenerator.GenerateQrCode(text, QRGenerator.ECCLevel.Q);
                    QRCode code = new QRCode(data);
                    bmp = code.GetGraphic(20);
                }
                await using MemoryStream str = new MemoryStream();
                bmp.Save(str, ImageFormat.Jpeg);
                str.Position = 0;
                await ctx.RespondWithFileAsync("Code.jpg", str);
            }
        }
        
        [Command("hash")]
        [Aliases("gen-hash")]
        [Description("Generate a Hash Code from supplied data (string or attachment), no arguments for a list of possible algorithms")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Hash(CommandContext ctx)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync(string.Join(", ",
                    HashTypeConv.Soptions.OfType<HashTypeConv.HashType>().Select(s => s.ToString())));
            }
        }
        
        [Command("hash")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Hash(CommandContext ctx, [Description("The algorithm to use")] HashTypeConv.HashType algorithm, [Description("The text to echo"), RemainingText] string text)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                byte[] data;
                using WebClient wc = new WebClient();
                data = string.IsNullOrWhiteSpace(text) && ctx.Message.Attachments.Count > 0
                    ? wc.DownloadData(ctx.Message.Attachments[0].Url)
                    : System.Text.Encoding.UTF8.GetBytes(text);
                HashAlgorithm impl = algorithm switch
                {
                    HashTypeConv.HashType.Md5 => new MD5CryptoServiceProvider(),
                    HashTypeConv.HashType.Sha1 => new SHA1Managed(),
                    HashTypeConv.HashType.Sha256 => new SHA256Managed(),
                    _ => throw new ArgumentOutOfRangeException()
                };
                byte[] hash = impl.ComputeHash(data);
                await ctx.RespondAsync(BitConverter.ToString(hash).Replace("-", string.Empty).ToUpper());
            }
        }
    }
}