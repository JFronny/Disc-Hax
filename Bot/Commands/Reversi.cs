using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CC_Functions.Misc;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using Shared.Config;

namespace Bot.Commands
{
    public partial class Minigames
    {
        [Command("reversi")]
        [Aliases("rev")]
        [Description("Play Reversi")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Reversi(CommandContext ctx, [Description("Person to play with")] DiscordMember player2,
            [Description("Doesn't delete messages. Speeds up game greatly but increases spam")]
            bool keepMsgs = false)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                DiscordMessage invite = await ctx.RespondAsync("Waiting 1 minute for Player 2 to react with :ok:...");
                await invite.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":ok:"));
                InteractivityResult<MessageReactionAddEventArgs> res =
                    await invite.WaitForReactionAsync(player2, new TimeSpan(0, 1, 0));
                if (res.TimedOut) await invite.ModifyAsync("Timed out.");
                else
                {
                    if (!keepMsgs)
                        await invite.DeleteAsync();
                    await ctx.TriggerTypingAsync();
                    DiscordMember player1 = await ctx.Guild.GetMemberAsync(ctx.Message.Author.Id);
                    (DiscordMember? winner, string renderedBoard) =
                        await PlayReversiRound(ctx, player1, player2, keepMsgs);
                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = "Reversi",
                        Description = winner == null
                            ? "Tie!"
                            : $"{(winner == player1 ? player1.DisplayName : player2.DisplayName)} won"
                    }.AddField("Board", renderedBoard).Build());
                }
            }
        }

        [Command("reversi")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Reversi(CommandContext ctx, [Description("Person to play with")] DiscordMember player2,
            [Description("Amount of coinst to bet")]
            decimal bet,
            [Description("Doesn't delete messages. Speeds up game greatly but increases spam")]
            bool keepMsgs = false)
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
                if (bet > ctx.Guild.GetMoney(player2) || bet < 0)
                {
                    await ctx.RespondAsync($"{player2.DisplayName} doesn't have that much");
                    return;
                }
                DiscordMessage invite = await ctx.RespondAsync("Waiting 1 minute for Player 2 to react with :ok:...");
                await invite.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":ok:"));
                InteractivityResult<MessageReactionAddEventArgs> res =
                    await invite.WaitForReactionAsync(player2, new TimeSpan(0, 1, 0));
                if (res.TimedOut) await invite.ModifyAsync("Timed out.");
                else
                {
                    if (!keepMsgs)
                        await invite.DeleteAsync();
                    await ctx.TriggerTypingAsync();
                    DiscordMember player1 = await ctx.Guild.GetMemberAsync(ctx.Message.Author.Id);
                    (DiscordMember? winner, string renderedBoard) =
                        await PlayReversiRound(ctx, player1, player2, keepMsgs);
                    if (winner is null)
                    {
                        ctx.Guild.IncrementMoney(player1, -bet);
                        ctx.Guild.IncrementMoney(player2, -bet);
                    }
                    else
                    {
                        DiscordMember nonWinner = winner == player1 ? player2 : player1;
                        ctx.Guild.IncrementMoney(winner, bet);
                        ctx.Guild.IncrementMoney(nonWinner, -bet);
                    }
                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = "Reversi",
                        Description = winner == null
                            ? "The bank won"
                            : $"{(winner == player1 ? player1.DisplayName : player2.DisplayName)} won {bet}"
                    }.AddField("Board", renderedBoard).Build());
                }
            }
        }

        private async Task<Tuple<DiscordMember?, string>> PlayReversiRound(CommandContext ctx, DiscordMember player1,
            DiscordMember player2, bool keepMsgs)
        {
            ReversiBoard b = new ReversiBoard();
            b.SetForNewGame();
            bool currentPlayerIs1 = true;
            while (true)
            {
                currentPlayerIs1 = !currentPlayerIs1;
                ReversiBoard.Color player = currentPlayerIs1 ? ReversiBoard.Color.White : ReversiBoard.Color.Black;
                string renderedBoard = "";
                renderedBoard += @"```
    A B C D E F G H
    ┌─┬─┬─┬─┬─┬─┬─┬─┐";
                for (int y = 0; y < 8; y++)
                {
                    if (y != 0)
                        renderedBoard += "\n ├─┼─┼─┼─┼─┼─┼─┼─┤";
                    renderedBoard += $"\n{y + 1}│";
                    for (int x = 0; x < 8; x++)
                        renderedBoard += b.GetSquareContents(y, x) switch
                        {
                            ReversiBoard.Color.Black => "O",
                            ReversiBoard.Color.Empty => b.IsValidMove(player, y, x) ? "-" : " ",
                            ReversiBoard.Color.White => "X",
                            _ => throw new ArgumentOutOfRangeException()
                        } + "│";
                }
                renderedBoard += @"
    └─┴─┴─┴─┴─┴─┴─┴─┘
    ```";
                if (!b.HasAnyValidMove(player))
                {
                    if (!b.HasAnyValidMove(ReversiBoard.Invert(player)))
                    {
                        await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = "Reversi",
                            Description = b.WhiteCount == b.BlackCount
                                ? "Tie!"
                                : $"{(b.WhiteCount > b.BlackCount ? player1.DisplayName : player2.DisplayName)} won"
                        }.AddField("Board", renderedBoard).Build());
                        DiscordMember? winner = b.WhiteCount == b.BlackCount
                            ? null
                            : b.WhiteCount > b.BlackCount
                                ? player1
                                : player2;
                        return new Tuple<DiscordMember?, string>(winner, renderedBoard);
                    }
                    continue;
                }
                DiscordMessage board = await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = "Reversi",
                        Description = $"Current turn: {(currentPlayerIs1 ? player1.DisplayName : player2.DisplayName)}"
                    }.AddField("Board", renderedBoard).AddField("Tutorial",
                        "Just respond with your column and row within one minute when it is your turn\n(eg: \"A5\")")
                    .Build());
                bool reading = true;
                while (reading)
                {
                    InteractivityResult<DiscordMessage> response =
                        await ctx.Channel.GetNextMessageAsync(currentPlayerIs1 ? player1 : player2,
                            new TimeSpan(0, 1, 0));
                    if (response.TimedOut)
                    {
                        await ctx.RespondAsync("Timed out.");
                        return null;
                    }
                    int[] text = response.Result.Content.ToLower().Select(s => (int) s).ToArray();
                    if (text.Length != 2 || text[0] < 97 || text[0] > 104 || text[1] < 49 || text[1] > 56)
                        await ctx.RespondAsync("Invalid.");
                    else
                    {
                        int x = text[0] - 97;
                        int y = text[1] - 49;
                        if (b.IsValidMove(player, y, x))
                        {
                            if (!keepMsgs)
                                board.DeleteAsync();
                            b.MakeMove(player, y, x);
                            reading = false;
                        }
                        else
                            await ctx.RespondAsync("Invalid move.");
                    }
                }
            }
        }

        private class ReversiBoard
        {
            public enum Color
            {
                Black = -1,
                Empty = 0,
                White = 1
            }

            private readonly bool[,] safeDiscs;

            private readonly Color[,] squares;

            public ReversiBoard()
            {
                squares = new Color[8, 8];
                safeDiscs = new bool[8, 8];

                int i, j;
                for (i = 0; i < 8; i++)
                for (j = 0; j < 8; j++)
                {
                    squares[i, j] = Color.Empty;
                    safeDiscs[i, j] = false;
                }

                UpdateCounts();
            }

            public int BlackCount { get; private set; }

            public int WhiteCount { get; private set; }

            public int EmptyCount { get; private set; }

            public int BlackFrontierCount { get; private set; }

            public int WhiteFrontierCount { get; private set; }

            public int BlackSafeCount { get; private set; }

            public int WhiteSafeCount { get; private set; }

            public static Color Invert(Color color) => (Color) (-(int) color);

            public void SetForNewGame()
            {
                int i, j;
                for (i = 0; i < 8; i++)
                for (j = 0; j < 8; j++)
                {
                    squares[i, j] = Color.Empty;
                    safeDiscs[i, j] = false;
                }
                squares[3, 3] = Color.White;
                squares[3, 4] = Color.Black;
                squares[4, 3] = Color.Black;
                squares[4, 4] = Color.White;
                UpdateCounts();
            }

            public Color GetSquareContents(int row, int col) => squares[row, col];

            public void MakeMove(Color color, int row, int col)
            {
                squares[row, col] = color;
                int dr, dc;
                for (dr = -1; dr <= 1; dr++)
                for (dc = -1; dc <= 1; dc++)
                    if (!(dr == 0 && dc == 0) && IsOutflanking(color, row, col, dr, dc))
                    {
                        int r = row + dr;
                        int c = col + dc;
                        while (squares[r, c] == Invert(color))
                        {
                            squares[r, c] = color;
                            r += dr;
                            c += dc;
                        }
                    }
                UpdateCounts();
            }

            public bool HasAnyValidMove(Color color)
            {
                int r, c;
                for (r = 0; r < 8; r++)
                for (c = 0; c < 8; c++)
                    if (IsValidMove(color, r, c))
                        return true;
                return false;
            }

            public bool IsValidMove(Color color, int row, int col)
            {
                if (squares[row, col] != Color.Empty)
                    return false;
                int dr, dc;
                for (dr = -1; dr <= 1; dr++)
                for (dc = -1; dc <= 1; dc++)
                    if (!(dr == 0 && dc == 0) && IsOutflanking(color, row, col, dr, dc))
                        return true;
                return false;
            }

            public int GetValidMoveCount(Color color)
            {
                int n = 0;
                int i, j;
                for (i = 0; i < 8; i++)
                for (j = 0; j < 8; j++)
                    if (IsValidMove(color, i, j))
                        n++;
                return n;
            }

            private bool IsOutflanking(Color color, int row, int col, int dr, int dc)
            {
                int r = row + dr;
                int c = col + dc;
                while (r >= 0 && r < 8 && c >= 0 && c < 8 && squares[r, c] == Invert(color))
                {
                    r += dr;
                    c += dc;
                }
                if (r < 0 || r > 7 || c < 0 || c > 7 || r - dr == row && c - dc == col || squares[r, c] != color)
                    return false;
                return true;
            }

            private void UpdateCounts()
            {
                BlackCount = 0;
                WhiteCount = 0;
                EmptyCount = 0;
                BlackFrontierCount = 0;
                WhiteFrontierCount = 0;
                WhiteSafeCount = 0;
                BlackSafeCount = 0;
                int i, j;
                bool statusChanged = true;
                while (statusChanged)
                {
                    statusChanged = false;
                    for (i = 0; i < 8; i++)
                    for (j = 0; j < 8; j++)
                        if (squares[i, j] != Color.Empty && !safeDiscs[i, j] && !IsOutflankable(i, j))
                        {
                            safeDiscs[i, j] = true;
                            statusChanged = true;
                        }
                }
                for (i = 0; i < 8; i++)
                for (j = 0; j < 8; j++)
                {
                    bool isFrontier = false;
                    if (squares[i, j] != Color.Empty)
                    {
                        int dr;
                        for (dr = -1; dr <= 1; dr++)
                        {
                            int dc;
                            for (dc = -1; dc <= 1; dc++)
                                if (!(dr == 0 && dc == 0) && i + dr >= 0 && i + dr < 8 && j + dc >= 0 && j + dc < 8 &&
                                    squares[i + dr, j + dc] == Color.Empty)
                                    isFrontier = true;
                        }
                    }
                    if (squares[i, j] == Color.Black)
                    {
                        BlackCount++;
                        if (isFrontier)
                            BlackFrontierCount++;
                        if (safeDiscs[i, j])
                            BlackSafeCount++;
                    }
                    else if (squares[i, j] == Color.White)
                    {
                        WhiteCount++;
                        if (isFrontier)
                            WhiteFrontierCount++;
                        if (safeDiscs[i, j])
                            WhiteSafeCount++;
                    }
                    else
                        EmptyCount++;
                }
            }

            private bool IsOutflankable(int row, int col)
            {
                Color color = squares[row, col];
                int i, j;
                bool hasSpaceSide1 = false;
                bool hasUnsafeSide1 = false;
                bool hasSpaceSide2 = false;
                bool hasUnsafeSide2 = false;
                for (j = 0; j < col && !hasSpaceSide1; j++)
                    if (squares[row, j] == Color.Empty)
                        hasSpaceSide1 = true;
                    else if (squares[row, j] != color || !safeDiscs[row, j])
                        hasUnsafeSide1 = true;
                for (j = col + 1; j < 8 && !hasSpaceSide2; j++)
                    if (squares[row, j] == Color.Empty)
                        hasSpaceSide2 = true;
                    else if (squares[row, j] != color || !safeDiscs[row, j])
                        hasUnsafeSide2 = true;
                if (hasSpaceSide1 && hasSpaceSide2 ||
                    hasSpaceSide1 && hasUnsafeSide2 ||
                    hasUnsafeSide1 && hasSpaceSide2)
                    return true;
                hasSpaceSide1 = false;
                hasSpaceSide2 = false;
                hasUnsafeSide1 = false;
                hasUnsafeSide2 = false;
                for (i = 0; i < row && !hasSpaceSide1; i++)
                    if (squares[i, col] == Color.Empty)
                        hasSpaceSide1 = true;
                    else if (squares[i, col] != color || !safeDiscs[i, col])
                        hasUnsafeSide1 = true;
                for (i = row + 1; i < 8 && !hasSpaceSide2; i++)
                    if (squares[i, col] == Color.Empty)
                        hasSpaceSide2 = true;
                    else if (squares[i, col] != color || !safeDiscs[i, col])
                        hasUnsafeSide2 = true;
                if (hasSpaceSide1 && hasSpaceSide2 ||
                    hasSpaceSide1 && hasUnsafeSide2 ||
                    hasUnsafeSide1 && hasSpaceSide2)
                    return true;
                hasSpaceSide1 = false;
                hasSpaceSide2 = false;
                hasUnsafeSide1 = false;
                hasUnsafeSide2 = false;
                i = row - 1;
                j = col - 1;
                while (i >= 0 && j >= 0 && !hasSpaceSide1)
                {
                    if (squares[i, j] == Color.Empty)
                        hasSpaceSide1 = true;
                    else if (squares[i, j] != color || !safeDiscs[i, j])
                        hasUnsafeSide1 = true;
                    i--;
                    j--;
                }
                i = row + 1;
                j = col + 1;
                while (i < 8 && j < 8 && !hasSpaceSide2)
                {
                    if (squares[i, j] == Color.Empty)
                        hasSpaceSide2 = true;
                    else if (squares[i, j] != color || !safeDiscs[i, j])
                        hasUnsafeSide2 = true;
                    i++;
                    j++;
                }
                if (hasSpaceSide1 && hasSpaceSide2 ||
                    hasSpaceSide1 && hasUnsafeSide2 ||
                    hasUnsafeSide1 && hasSpaceSide2)
                    return true;
                hasSpaceSide1 = false;
                hasSpaceSide2 = false;
                hasUnsafeSide1 = false;
                hasUnsafeSide2 = false;
                i = row - 1;
                j = col + 1;
                while (i >= 0 && j < 8 && !hasSpaceSide1)
                {
                    if (squares[i, j] == Color.Empty)
                        hasSpaceSide1 = true;
                    else if (squares[i, j] != color || !safeDiscs[i, j])
                        hasUnsafeSide1 = true;
                    i--;
                    j++;
                }
                i = row + 1;
                j = col - 1;
                while (i < 8 && j >= 0 && !hasSpaceSide2)
                {
                    if (squares[i, j] == Color.Empty)
                        hasSpaceSide2 = true;
                    else if (squares[i, j] != color || !safeDiscs[i, j])
                        hasUnsafeSide2 = true;
                    i++;
                    j--;
                }
                if (hasSpaceSide1 && hasSpaceSide2 ||
                    hasSpaceSide1 && hasUnsafeSide2 ||
                    hasUnsafeSide1 && hasSpaceSide2)
                    return true;
                return false;
            }
        }
    }
}