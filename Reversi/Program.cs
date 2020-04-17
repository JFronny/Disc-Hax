using System;

namespace Reversi
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Board b = new Board();
            while (true)
            {
                bool playing = true;
                b.SetForNewGame();
                bool isWhite = true;
                while (playing)
                {
                    isWhite = !isWhite;
                    Board.Color player = isWhite ? Board.Color.White : Board.Color.Black;
                    Console.Clear();
                    Console.Write(" ");
                    for (int i = 0; i < 8; i++)
                        Console.Write($" {i + 1}");
                    Console.Write(" X");
                    Console.WriteLine();
                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 8; x++)
                        {
                            Console.Write(x == 0 ? $"{y + 1} " : " ");
                            Console.ForegroundColor =
                                b.IsValidMove(player, y, x) ? ConsoleColor.Green : ConsoleColor.Red;
                            Console.Write(GetChar(b.GetSquareContents(y, x)));
                        }
                        Console.ResetColor();
                        Console.WriteLine();
                    }
                    Console.WriteLine("Y");
                    if (!b.HasAnyValidMove(player))
                    {
                        if (!b.HasAnyValidMove(Board.Invert(player)))
                        {
                            Console.WriteLine(b.WhiteCount == b.BlackCount
                                ? "Tie"
                                : $"{(b.WhiteCount > b.BlackCount ? "White" : "Black")} won");
                            Console.ReadKey();
                            playing = false;
                        }
                        continue;
                    }
                    Console.WriteLine($"Current player: {(isWhite ? "White (+)" : "Black (-)")}");
                    Console.WriteLine($"{b.GetValidMoveCount(player)} moves possible");
                    int nX;
                    int nY;
                    bool first = true;
                    do
                    {
                        if (!first)
                            Console.WriteLine("Invalid move");
                        first = false;
                        Console.Write("x> ");
                        nX = int.Parse(Console.ReadKey().KeyChar.ToString()) - 1;
                        Console.WriteLine();
                        Console.Write("y> ");
                        nY = int.Parse(Console.ReadKey().KeyChar.ToString()) - 1;
                        Console.WriteLine();
                    } while (!b.IsValidMove(player, nY, nX));
                    b.MakeMove(player, nY, nX);
                }
            }
        }

        private static char GetChar(Board.Color square)
        {
            return square switch
            {
                Board.Color.Black => '-',
                Board.Color.Empty => '.',
                Board.Color.White => '+',
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}