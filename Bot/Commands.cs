﻿using System;
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
        public async Task Chan(CommandContext ctx, [Description("Board to select image from")] params string[] args) => await Chan(args, ctx.Channel, (c1, c2, c3) => ctx.RespondAsync(c1, c2, c3));

        public static async Task Chan(string[] args, DiscordChannel Channel, Func<string, bool, DiscordEmbed, Task<DiscordMessage>> postMessage)
        {
            if (Config.data.Chan)
                if (args.Length == 0)
                {
                    List<string> lmsg = new List<string>();
                    List<BoardInfo> lboard = JsonDeserializer.Deserialize<BoardListModel>(await Internet.DownloadString(@"https://a.4cdn.org/boards.json").ConfigureAwait(false)).boards;
                    lboard.ForEach(s =>
                    {
                        lmsg.Add(s.Title + " (" + s.ShortName + ")");
                    });
                    await postMessage.Invoke(string.Join(", ", lmsg) + "\r\nUsage: !4chan <ShortName>", false, null);
                }
                else
                {
                    Board b = new Board(args[0]);
                    if (Channel.IsNSFW || Config.data.Nsfw)
                    {
                        Thread[] threads = b.GetThreads().ToArray();
                        Thread t = threads[Form.Instance.rnd.Next(threads.Length)];
                        await postMessage.Invoke("https://boards.4channel.org/" + t.Board.BoardId + "/thread/" + t.PostNumber, false, new DiscordEmbedBuilder { Title = t.Name + "#" + t.Id + ": " + (string.IsNullOrWhiteSpace(t.Subject) ? "Untitled" : t.Subject), ImageUrl = t.Image.Image.AbsoluteUri });
                    }
                    else
                        await postMessage.Invoke("Due to the way 4chan users behave, this command is only allowed in NSFW channels", false, null);
                }
        }

        [Command("waifu"), Description("Shows you a random waifu from thiswaifudoesnotexist.net")]
        public async Task Waifu(CommandContext ctx, [Description("Use \"f\" to force execution, even on non-NSFW channels")] params string[] args) => await Waifu(args, ctx.Channel, (c1, c2, c3) => ctx.RespondAsync(c1, c2, c3));

        public static async Task Waifu(string[] args, DiscordChannel Channel, Func<string, bool, DiscordEmbed, Task<DiscordMessage>> postMessage)
        {
            if (Config.data.Waifu)
                if (Channel.IsNSFW || Config.data.Nsfw || args.Contains("f"))
                    await postMessage.Invoke(null, false, new DiscordEmbedBuilder { Title = "There.", ImageUrl = "https://www.thiswaifudoesnotexist.net/example-" + Form.Instance.rnd.Next(6000).ToString() + ".jpg" });
                else
                    await postMessage.Invoke("The generated waifus might not be something you want to be looking at at work.", false, null);
        }

        [Command("play"), Description("Sends \"No.\" to the chat. For users used to RhythmBot.")]
        public async Task Play(CommandContext ctx) => await Play((c1, c2, c3) => ctx.RespondAsync(c1, c2, c3));

        public static async Task Play(Func<string, bool, DiscordEmbed, Task<DiscordMessage>> postMessage)
        {
            if (Config.data.Play)
                await postMessage.Invoke("No.", false, null);
        }

        [Command("ping"), Description("Responds with \"Pong\" if the bot is active")]
        public async Task Ping(CommandContext ctx) => await Ping((c1, c2, c3) => ctx.RespondAsync(c1, c2, c3));

        public static async Task Ping(Func<string, bool, DiscordEmbed, Task<DiscordMessage>> postMessage) => await postMessage.Invoke("Pong", false, null);


        [Command("booru"), Description("Shows a random Image from danbooru")]
        public async Task Booru(CommandContext ctx, [Description("Tags for image selection")] params string[] args) => await Booru(args, ctx.Channel, (c1, c2, c3) => ctx.RespondAsync(c1, c2, c3));

        public static async Task Booru(string[] args, DiscordChannel Channel, Func<string, bool, DiscordEmbed, Task<DiscordMessage>> postMessage)
        {

            if (Config.data.Booru)
            {
                SearchResult result = await (Channel.IsNSFW || Config.data.Nsfw ? (Booru)new Rule34() : new Gelbooru()).GetRandomImage(args);
                while (result.rating != (Channel.IsNSFW || Config.data.Nsfw ? Rating.Safe : Rating.Explicit))
                {
                    result = await new Gelbooru().GetRandomImage(args);
                }
                await postMessage.Invoke(null, false, new DiscordEmbedBuilder { Title = "There.", Description = string.Join(", ", result.tags), ImageUrl = result.fileUrl.AbsoluteUri });
            }
        }

        [Command("config"), Description("Prints the DiscHax-bot config"), RequireUserPermissions(Permissions.Administrator)]
        public async Task ConfigCmd(CommandContext ctx) => await ctx.RespondAsync(Config.data.ToString(true));

        public static async Task ConfigCmd(Func<string, bool, DiscordEmbed, Task<DiscordMessage>> postMessage)
        {
            if (Config.data.Config)
                await postMessage.Invoke(Config.data.ToString(true), false, null);
        }
    }
}
