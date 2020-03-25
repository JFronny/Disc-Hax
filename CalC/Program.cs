using System;
using org.mariuszgromada.math.mxparser;

namespace CalC
{
    internal class Program
    {
        public static void Main()
        {
            while (true)
            {
                Console.Write("Expr: ");
                Expression ex = new Expression(Console.ReadLine());
                Console.WriteLine($"{ex.getExpressionString()}\n{ex.calculate()}");
            }
        }
    }
}