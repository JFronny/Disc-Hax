using System;
using org.mariuszgromada.math.mxparser;

namespace CalC
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            /*string[] equations = {"sin(15^2)", "sin(2*pi)", "solve( 2*x - 4, x, 0, 10 )", "2*π"};
            for (int i = 0; i < equations.Length; i++)
            {
                Expression ex = new Expression(equations[i]);
                Console.WriteLine($"{ex.getExpressionString()}\n{ex.calculate()}");
            }*/
            while (true)
            {
                Console.Write("Expr: ");
                Expression ex = new Expression(Console.ReadLine());
                Console.WriteLine($"{ex.getExpressionString()}\n{ex.calculate()}");
            }
        }
    }
}