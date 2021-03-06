using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CC_Functions.Misc;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Eto.Drawing;
using org.mariuszgromada.math.mxparser;
using Shared;
using Shared.Config;

namespace Bot.Commands.Misc
{
    [Group("Math")]
    [Aliases("m", "+")]
    [Description("Commands for calculating. Also includes money conversion")]
    public class Math : BaseCommandModule
    {
        [Command("calc")]
        [Description(
            "Calculates a result using mathparser.org. Example: \"sin(15^2)\", \"15 * (-12)\", \"solve( 2 * x - 4, x, 0, 10 )\", \"log(4, 2)\" - Please note: sin() etc use radians! 2*pi radians equals 360°")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Calc(CommandContext ctx, [Description("Equation")] [RemainingText]
            string equation)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .And(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                Expression ex = new Expression(equation);
#if DEBUG
                ex.setVerboseMode();
#endif
                double result = ex.calculate();
                await ctx.RespondAsyncFix(double.IsNaN(result)
                    ? ex.getErrorMessage()
                    : $"{ex.getExpressionString()} = {result}");
            }
        }

        [Command("solve")]
        [Description(
            "Solve a mathematical function. See calc for extra help. Example: \"solve x -100 100 3 * x * 2 = 15 * x\"")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Solve(CommandContext ctx, [Description("Variable to calculate")] string target,
            [Description("Minimum value for result")]
            string min, [Description("Maximum value for result")]
            string max, [Description("Equation")] [RemainingText]
            string equation)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .And(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                string[] parts = equation.Split('=');
                string newEq = $"({parts[0].Trim()}) - ({parts[1].Trim()})";
                newEq = $"solve({newEq}, {target}, {min}, {max})";
                Expression ex = new Expression(newEq);
                await ctx.RespondAsyncFix($"{ex.getExpressionString()} = {ex.calculate()}");
            }
        }

        [Command("graph")]
        [Description(
            "Generates a x-based graph (variable x will be set), see \"calc\" for syntax. Example: graph x + 15")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Graph(CommandContext ctx, [Description("Equation")] [RemainingText]
            string equation)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .And(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                Bitmap bmp = new Bitmap(200, 200, PixelFormat.Format24bppRgb);
                Graphics g = new Graphics(bmp);
                g.Clear(Colors.White);
                Pen grid = Pens.LightGrey;
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
                    double result = new Expression(equation, new Argument("x", x / 10d)).calculate();
                    result *= 10;
                    if (!double.IsNaN(result) && result <= 200 && result >= -200)
                        points.Add(new PointF(x + 100, 100 - Convert.ToSingle(result)));
                }
                g.DrawLines(line, points.ToArray());
                g.Flush();
                using MemoryStream memoryStream = new MemoryStream();
                bmp.Save(memoryStream, ImageFormat.Jpeg);
                memoryStream.Position = 0;
                await ctx.RespondWithFileAsync("EquationResult.jpg", memoryStream);
            }
        }

        [Command("currency")]
        [Description("Transforms currencies")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Currency(CommandContext ctx, [Description("Input currency in ISO")] Currency inCurrency,
            [Description("Output currency in ISO")]
            Currency outCurrency, [Description("Amount to convert")] double amount)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .And(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync(
                    $"{amount} {inCurrency.CurrencyName} equals {CurrencyConverter.Convert(amount, inCurrency, outCurrency)} {outCurrency.CurrencyName}");
            }
        }

        [Command("currency")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Currency(CommandContext ctx)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .And(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondPaginatedIfTooLong(string.Join(", ",
                    CurrencyConverter.Currencies.Values.Select(s => $"{s.CurrencyName}/{s.CurrencySymbol} ({s.Id})")
                        .OrderBy(s => s)));
            }
        }
    }
}