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

namespace Bot
{
    public class Commands
    {
        [Command("4chan"), Description("Sends a random image from the board. If no board is specified, a list of boards will be displayed.")]
        public async Task Chan(CommandContext ctx, [Description("Board to select image from")] params string[] args)
        {
            if (Form.Instance.chanBox.Checked)
            {
                if (args.Length == 0)
                {
                    List<string> lmsg = new List<string>();
                    List<BoardInfo> lboard = JsonDeserializer.Deserialize<BoardListModel>(Internet.DownloadString(@"https://a.4cdn.org/boards.json").ConfigureAwait(false).GetAwaiter().GetResult()).boards;
                    lboard.ForEach(s =>
                    {
                        lmsg.Add(s.ShortName + new string(' ', 8 - s.ShortName.Length) + s.Title);
                    });
                    await ctx.RespondAsync(string.Join("\r\n", lmsg) + "\r\nUsage: !4chan <ShortName>");
                }
                else
                {
                    Board b = new Board(args[0]);
                    if (ctx.Channel.IsNSFW)
                    {
                        Thread[] threads = b.GetThreads().ToArray();
                        Thread t = threads[Form.Instance.rnd.Next(threads.Length)];
                        await ctx.RespondAsync(embed: new DiscordEmbedBuilder { Title = t.Name + "#" + t.Id + ": " + (string.IsNullOrWhiteSpace(t.Subject) ? "Untitled" : t.Subject), ImageUrl = t.Image.Image.AbsoluteUri });
                    }
                    else
                        await ctx.RespondAsync("Due to the way 4chan users behave, this command is only allowed in NSFW channels");
                }
            }
        }

        [Command("waifu"), Description("Shows you a random waifu from thiswaifudoesnotexist.net")]
        public async Task Waifu(CommandContext ctx, [Description("Use \"f\" to force execution, even on non-NSFW channels")] params string[] args)
        {
            if (Form.Instance.waifuBox.Checked)
            {
                if (ctx.Channel.IsNSFW || args.Contains("f"))
                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder { Title = "There.", ImageUrl = "https://www.thiswaifudoesnotexist.net/example-" + Form.Instance.rnd.Next(6000).ToString() + ".jpg" });
                else
                    await ctx.RespondAsync("The generated waifus might not be something you want to be looking at at work.");
            }
        }

        [Command("play"), Description("Sends \"No.\" to the chat. For users used to RhythmBot.")]
        public async Task Play(CommandContext ctx)
        {
            if (Form.Instance.playBox.Checked)
                await ctx.RespondAsync("No.");
        }

        [Command("ping"), Description("Responds with \"Pong\" if the bot is active")]
        public async Task Ping(CommandContext ctx) => await ctx.RespondAsync("Pong");


        [Command("booru"), Description("Shows a random Image from danbooru")]
        public async Task Booru(CommandContext ctx, [Description("Tags for image selection")] params string[] args)
        {
            if (Form.Instance.booruBox.Checked)
            {
                SearchResult result = (ctx.Channel.IsNSFW ? (Booru)new Rule34() : new Gelbooru()).GetRandomImage(args).GetAwaiter().GetResult();
                while (result.rating != (ctx.Channel.IsNSFW ? Rating.Safe : Rating.Explicit))
                {
                    result = new Gelbooru().GetRandomImage(args).GetAwaiter().GetResult();
                }
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder { Title = "There.", Description = string.Join(", ", result.tags), ImageUrl = result.fileUrl.AbsoluteUri });
            }
        }
    }
}
