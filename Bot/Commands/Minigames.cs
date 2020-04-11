using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BooruSharp.Booru;
using BooruSharp.Search.Post;
using Bot.Converters;
using CC_Functions.Misc;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Shared;
using Shared.Config;
using static System.Math;

namespace Bot.Commands
{
    [Group("game")]
    [Aliases("g")]
    [Description("Simple games")]
    public class Minigames : BaseCommandModule
    {
        [Command("rps")]
        [Description("Play Rock-Paper-Scissors")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Rps(CommandContext ctx, [Description("Input (=Rock/Paper/Scissor)")]
            RpsOptionConv.RpsOption option)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                string output;
                RpsOptionConv.RpsOption rpsOption =
                    (RpsOptionConv.RpsOption) RpsOptionConv.Soptions.GetValue(
                        Program.Rnd.Next(RpsOptionConv.Soptions.Length));
                output = $"You chose: {option}, I chose {rpsOption}. This means ";
                int diff = (int) rpsOption - (int) option;
                diff = diff switch {-2 => 1, 2 => -1, _ => diff};
                output += diff switch
                {
                    -1 => "You've",
                    0 => "No-one has",
                    1 => "I've",
                    _ => throw new Exception($"This should not happen! (diff={diff})")
                };
                output += " won";
                await ctx.RespondAsync(output);
            }
        }

        [Command("rps")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Rps(CommandContext ctx, [Description("Input (=Rock/Paper/Scissor)")]
            RpsOptionConv.RpsOption option, [Description("Amount of coinst to bet")]
            decimal bet)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                if (bet > ctx.Guild.GetMoney(ctx.Member) || bet < 0)
                {
                    await ctx.RespondAsync("You don't have that much");
                    return;
                }
                string output;
                RpsOptionConv.RpsOption rpsOption =
                    (RpsOptionConv.RpsOption) RpsOptionConv.Soptions.GetValue(
                        Program.Rnd.Next(RpsOptionConv.Soptions.Length));
                output = $"You chose: {option}, I chose {rpsOption}. This means ";
                int diff = (int) rpsOption - (int) option;
                diff = diff switch {-2 => 1, 2 => -1, _ => diff};
                output += diff switch
                {
                    -1 => "You've",
                    0 => "No-one has",
                    1 => "I've",
                    _ => throw new Exception($"This should not happen! (diff={diff})")
                };
                output += " won";
                ctx.Guild.IncrementMoney(ctx.Member, -bet * diff);
                await ctx.RespondAsync(output);
            }
        }

        [Command("slots")]
        [Description("Play Slots. No arguments for values")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Slots(CommandContext ctx)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync(@":first_place::first_place::grey_question: - 0.5x
:gem::gem::grey_question: - 2x
:100::100::grey_question: - 2x
:first_place::first_place::first_place: - 2.5x
:gem::gem::gem: - 3x
:dollar::dollar::grey_question: - 3.5x
:100::100::100: - 4x
:moneybag::moneybag::grey_question: - 7x
:dollar::dollar::dollar: - 7x
:moneybag::moneybag::moneybag: - 15x");
            }
        }

        [Command("slots")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Slots(CommandContext ctx, [Description("Amount of coinst to bet")]
            decimal bet) => await Slots(ctx, bet, false);

        [Command("slots")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Slots(CommandContext ctx, [Description("Amount of coinst to bet")]
            decimal bet, [Description("Whether to skip the animation")]
            bool fast)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                if (bet > ctx.Guild.GetMoney(ctx.Member) || bet < 0)
                {
                    await ctx.RespondAsync("You don't have that much");
                    return;
                }
                int[] pool = {Program.Rnd.Next(5), Program.Rnd.Next(5), Program.Rnd.Next(5)};
                DiscordMessage msg = await ctx.RespondAsync("| :grey_question: | :grey_question: | :grey_question: |");
                if (!fast)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        await msg.ModifyAsync(
                            $"| {GetSlot(Program.Rnd.Next(5))} | :grey_question: | :grey_question: |");
                        await Task.Delay(1000);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        await msg.ModifyAsync(
                            $"| {GetSlot(pool[0])} | {GetSlot(Program.Rnd.Next(5))} | :grey_question: |");
                        await Task.Delay(1000);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        await msg.ModifyAsync(
                            $"| {GetSlot(pool[0])} | {GetSlot(pool[1])} | {GetSlot(Program.Rnd.Next(5))} |");
                        await Task.Delay(1000);
                    }
                    await msg.ModifyAsync($"| {GetSlot(pool[0])} | {GetSlot(pool[1])} | {GetSlot(pool[2])} |");
                }
                pool = pool.OrderBy(s => s).ToArray();
                decimal winnings = 0;
                if (pool[0] == pool[1] || pool[1] == pool[2])
                {
                    if (pool[0] != pool[2])
                        winnings = bet * (decimal) (pool[1] switch
                        {
                            0 => 0.5,
                            1 => 2,
                            2 => 2,
                            3 => 3.5,
                            4 => 7
                        });
                    else
                        winnings = (decimal) ((double) bet * pool[1] switch
                        {
                            0 => 2.5,
                            1 => 3,
                            2 => 4,
                            3 => 7,
                            4 => 15
                        });
                }
                winnings -= bet;
                await msg.ModifyAsync(
                    $"| {GetSlot(pool[0])} | {GetSlot(pool[1])} | {GetSlot(pool[2])} |\nYou {(winnings > 0 ? "won" : "lost")} {Abs((double) winnings)} coins.");
                ctx.Guild.IncrementMoney(ctx.Member, winnings);
            }
        }

        private string GetSlot(int val) => val switch
        {
            0 => ":first_place:",
            1 => ":gem:",
            2 => ":100:",
            3 => ":dollar:",
            4 => ":moneybag:",
            _ => throw new ArgumentOutOfRangeException()
        };

        [Command("sweeper")]
        [Aliases("mine", "minesweeper")]
        [Description("Generate a minesweeper field")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Minesweeper(CommandContext ctx, [Description("The height of the field")]
            int height = 10, [Description("The width of the field")]
            int width = 10, [Description("The amount of mines to place")]
            int mineCount = 5)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                if (height * width < mineCount * 2 || mineCount > 200)
                {
                    await ctx.RespondAsync("Too many mines!");
                    return;
                }
                if (mineCount < 1)
                {
                    await ctx.RespondAsync("Too few mines!");
                    return;
                }
                if (height > 100 || width > 100)
                {
                    await ctx.RespondAsync("Field too big!");
                    return;
                }
                if (height < 3 || width < 3)
                {
                    await ctx.RespondAsync("Field too small!");
                    return;
                }
                bool[,] field = new bool[width, height];
                for (int i = 0; i < mineCount; i++)
                {
                    int x;
                    int y;
                    do
                    {
                        x = Program.Rnd.Next(width);
                        y = Program.Rnd.Next(height);
                    } while (field[x, y]);
                    field[x, y] = true;
                }
                string message = "There you go!\n";
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        message += "||";
                        if (field[x, y])
                            message += ":boom:";
                        else
                        {
                            int tmp = new[] {x - 1, x, x + 1}
                                .SelectMany(oX => new[] {y - 1, y, y + 1}, (oX, oY) => new {oX, oY})
                                .Where(t => t.oX >= 0 && t.oX < width && t.oY >= 0 && t.oY < height)
                                .Count(s => field[s.oX, s.oY]);
                            message += tmp.ToString()[0].ToString().Emotify();
                        }
                        message += "|| ";
                    }
                    message += '\n';
                }
                await ctx.RespondAsync(message);
            }
        }

        [Command("tag-guesser")]
        [Aliases("tag", "guesser", "tagguesser", "guess")]
        [Description("Generate a minesweeper field")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task TagGuesser(CommandContext ctx, [Description("The amount of time the game will run for")] TimeSpan? gameTime = null)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                if (gameTime == null) gameTime = new TimeSpan(0, 1, 0);
                if (gameTime > new TimeSpan(0, 3, 0))
                    throw new ArgumentOutOfRangeException("Please choose a smaller time");
                InteractivityExtension ext = ctx.Client.GetInteractivity();
                ABooru booru = ImageBoards.BooruDict.Select(s => s.Value)
                    .Where(s => s.IsSafe() == !ctx.Channel.GetEvaluatedNsfw())
                    .OrderBy(s => Program.Rnd.NextDouble()).First();
                SearchResult result = new SearchResult();
                int triesLeft = 10;
                do
                {
                    if (triesLeft == 0)
                        throw new Exception("Failed to find image in a reasonable amount of tries");
                    try
                    {
                        result = await booru.GetRandomImageAsync();
                    }
                    catch
                    {
                        // ignored
                    }
                    triesLeft--;
                } while (result.rating != (ctx.Channel.GetEvaluatedNsfw() ? Rating.Explicit : Rating.Safe));
                string val = Program.Rnd.Next(10000, 99999).ToString();
                using WebClient wClient = new WebClient();
                List<string> found = new List<string>();
                Dictionary<DiscordUser, int> scores = new Dictionary<DiscordUser, int>();
                await ctx.RespondWithFileAsync($"{val}_img.jpg",
                    wClient.OpenRead(result.fileUrl ?? result.previewUrl), embed: new DiscordEmbedBuilder
                    {
                        Title = "Guess tags!"
                    }.Build());
                string[] tags = result.tags;
                DateTime tmp = DateTime.Now;
                do
                {
                    InteractivityResult<DiscordMessage> res = await ext.WaitForMessageAsync(s => true, tmp + gameTime - DateTime.Now);
                    if (res.TimedOut) continue;
                    if (found.Contains(res.Result.Content.ToLower()) || !tags.Any(s =>
                        string.Equals(s, res.Result.Content, StringComparison.CurrentCultureIgnoreCase))) continue;
                    found.Add(res.Result.Content.ToLower());
                    if (!scores.ContainsKey(res.Result.Author))
                        scores.Add(res.Result.Author, 0);
                    scores[res.Result.Author]++;
                    await ctx.RespondAsync($"+1 for {res.Result.Author.Username}");
                } while (DateTime.Now - tmp < gameTime);
                IOrderedEnumerable<KeyValuePair<DiscordUser, int>> orderedScore = scores.OrderByDescending(s => s.Value);
                if (scores.Count > 3)
                    await ctx.RespondAsync(string.Join("\n", orderedScore.Take(3).Select(s => $"{s.Key.Username}: {s.Value.ToString().Emotify()}")));
                else
                    await ctx.RespondAsync(string.Join("\n", orderedScore.Select(s => $"{s.Key.Username}: {s.Value.ToString().Emotify()}")));
            }
        }
    }
}