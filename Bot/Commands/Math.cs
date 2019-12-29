#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CC_Functions.Misc;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using org.mariuszgromada.math.mxparser;
using Shared;
using Shared.Config;

#endregion

namespace Bot.Commands
{
    public class Math : BaseCommandModule
    {
        private readonly Function log;

        public Math()
        {
            Console.Write("Creating extra math functions...");
            log = new Function("log", new Logarithm());
            Console.WriteLine(" Finished.");
        }

        [Command("calc")]
        [Description(
            "Calculates a result using mathparser.org\r\nExamples: \"sin(15^2)\", \"15 * (-12)\", \"solve( 2 * x - 4, x, 0, 10 )\", \"log(4, 2)\"\r\nRemember: sin() etc use radians! 2*pi radians equals 360°\r\nAlso: A result of NaN means, that you did something wrong and no result was found")]
        public async Task Calc(CommandContext ctx, [Description("Equation")] [RemainingText]
            string equation)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled)
                .AND(ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Calc)))
            {
                Expression ex = new Expression(equation, log);
                #if DEBUG
                ex.setVerboseMode();
                #endif
                double result = ex.calculate();
                await ctx.RespondAsyncFix(double.IsNaN(result) ? ex.getErrorMessage() : $"{ex.getExpressionString()} = {result}");
            }
        }

        [Command("solve")]
        [Description(
            "Solve a mathematical function. See calc for extra help. Format is: \"solve x -100 100 3 * x * 2 = 15 * x\"")]
        public async Task Solve(CommandContext ctx, [Description("Variable to get function from")]
            string target, [Description("Minimum value for result")]
            string min, [Description("Maximum value for result")]
            string max, [Description("Equation")] [RemainingText]
            string equation)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled)
                .AND(ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Calc)))
            {
                string[] parts = equation.Split('=');
                string newEq = $"({parts[0].Trim()}) - ({parts[1].Trim()})";
                newEq = $"solve({newEq}, {target}, {min}, {max})";
                Expression ex = new Expression(newEq, log);
                await ctx.RespondAsyncFix($"{ex.getExpressionString()} = {ex.calculate()}");
            }
        }

        [Command("graph")]
        [Description(
            "Generates a x-based graph (variable x will be set), see \"calc\" for syntax\r\nExample: graph x + 15")]
        public async Task Graph(CommandContext ctx, [Description("Equation")] [RemainingText]
            string equation)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled)
                .AND(ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Graph)))
            {
                Bitmap bmp = new Bitmap(200, 200);
                Graphics g = Graphics.FromImage(bmp);
                g.Clear(Color.White);
                Pen grid = Pens.LightGray;
                Pen gridZero = Pens.Gray;
                Pen line = Pens.Red;
                for (int i = -100; i < 100; i += 10)
                {
                    g.DrawLine(i == 0 ? gridZero : grid, i + 100, 200, i + 100, 0);
                    g.DrawLine(i == 0 ? gridZero : grid, 200, 100 - i, 0, 100 - i);
                }
                List<PointF> points = new List<PointF>();
                for (int x = -100; x < 100; x++)
                {
                    double result = new Expression(equation, new Argument("x", x / 10d), log).calculate();
                    result *= 10;
                    if (!double.IsNaN(result) && result <= 200 && result >= -200)
                        points.Add(new PointF(x + 100, 100 - Convert.ToSingle(result)));
                }
                g.DrawLines(line, points.ToArray());
                g.Flush();
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    bmp.Save(memoryStream, ImageFormat.Jpeg);
                    memoryStream.Position = 0;
                    await ctx.RespondWithFileAsync("EquationResult.jpg", memoryStream);
                }
            }
        }

        [Command("currency")]
        [Description("Transforms currencies")]
        public async Task Currency(CommandContext ctx, [Description("Input currency in ISO")] Currency inCurrency,
            [Description("Output currency in ISO")]
            Currency outCurrency, [Description("Amount to convert")] double amount)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled)
                .AND(ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Currency)))
                await ctx.RespondAsync(
                    $"{amount} {inCurrency.currencyName} equals {CurrencyConverter.Convert(amount, inCurrency, outCurrency)} {outCurrency.currencyName}");
        }

        [Command("currency")]
        public async Task Currency(CommandContext ctx)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled)
                .AND(ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Currency)))
                await ctx.RespondPaginated(string.Join(", ",
                    CurrencyConverter.Currencies.Values.Select(s => $"{s.currencyName}/{s.currencySymbol} ({s.id})")
                        .OrderBy(s => s)));
        }
    }

    public class Logarithm : FunctionExtension
    {
        private double b;
        private double x;

        public Logarithm()
        {
            x = double.NaN;
            b = double.NaN;
        }

        public Logarithm(double x, double b)
        {
            this.x = x;
            this.b = b;
        }

        public int getParametersNumber() => 2;

        public void setParameterValue(int argumentIndex, double argumentValue)
        {
            switch (argumentIndex)
            {
                case 0:
                    x = argumentValue;
                    break;
                case 1:
                    b = argumentValue;
                    break;
            }
        }

        public string getParameterName(int parameterIndex) => parameterIndex switch {0 => "x", 1 => "b"};

        public double calculate() => System.Math.Log(x, b);

        public FunctionExtension clone() => new Logarithm(x, b);
    }
}