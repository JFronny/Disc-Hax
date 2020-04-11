using System;
using System.Linq;

namespace Minesweeper
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Random rnd = new Random();
            while (true)
            {
                Console.Clear();
                char[,] field = GenField(10, 10, 5, rnd);
                Console.WriteLine("Gen complete");
                for (int y = 0; y < 10; y++)
                {
                    for (int x = 0; x < 10; x++)
                    {
                        Console.Write(field[x, y]);
                        if (x < 9)
                            Console.Write(".");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine("Field complete");
                Console.ReadKey();
            }
        }

        private static char[,] GenField(int width, int height, int mineCount, Random rnd)
        {
            bool[,] field = new bool[width, height];
            for (int i = 0; i < mineCount; i++)
            {
                int x;
                int y;
                do
                {
                    x = rnd.Next(width);
                    y = rnd.Next(height);
                } while (field[x, y]);
                field[x, y] = true;
            }
            char[,] resField = new char[width, height];
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                if (field[x, y])
                    resField[x, y] = 'X';
                else
                {
                    int tmp = new[] {x - 1, x, x + 1}
                        .SelectMany(oX => new[] {y - 1, y, y + 1}, (oX, oY) => new {oX, oY})
                        .Where(t => t.oX >= 0 && t.oX < width && t.oY >= 0 && t.oY < height)
                        .Count(s => field[s.oX, s.oY]);
                    resField[x, y] = tmp == 0 ? ' ' : tmp.ToString().ToCharArray()[0];
                }
            return resField;
        }
    }
}