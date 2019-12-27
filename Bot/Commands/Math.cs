using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using CC_Functions.Misc;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using org.mariuszgromada.math.mxparser;
using Shared;
using Shared.Config;

namespace Bot.Commands
{
    public class Math : BaseCommandModule
    {
        private Function log;

        [Command("calc")]
        [Description(
            "Calculates a result using mathparser.org\r\nExamples: \"sin(15^2)\", \"15*(-12)\", \"solve( 2*x - 4, x, 0, 10 )\"\r\nRemember: sin() etc use radians! 2*pi radians equals 360°\r\nAlso: A result of NaN means, that you did something wrong and no result was found")]
        public async Task Calc(CommandContext ctx, [Description("Equation")] [RemainingText]
            string equation)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled)
                .AND(ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Calc)))
            {
                Expression ex = new Expression(equation);
                await ctx.RespondAsync(ex.getExpressionString() + " = " + ex.calculate());
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
                string newEq = "(" + parts[0].Trim() + ") - (" + parts[1].Trim() + ")";
                newEq = "solve(" + newEq + ", " + target + ", " + min + ", " + max + ")";
                Expression ex = new Expression(newEq);
                await ctx.RespondAsync(ex.getExpressionString() + " = " + ex.calculate());
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
                    double result = new Expression(equation, new Argument("x", x / 10d)).calculate();
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

        /*public class Logarithmm : FunctionExtension
        {
            double b;
            double x;
            public Logarithmm()
            {
                b = Double.NaN;
                x = Double.NaN;
            }
            
            public Logarithmm(double b, double x)
            {
                this.b = b;
                this.x = x;
            }
            
            public int getParametersNumber() => 2;

            public void setParameterValue(int argumentIndex, double argumentValue)
            {
                switch (argumentIndex)
                {
                    case 0:
                        b = argumentValue;
                        break;
                    case 1:
                        x = argumentValue;
                        break;
                }
            }

            public string getParameterName(int parameterIndex) => parameterIndex switch {0 => "b", 1 => "x"};

            public double calculate() => System.Math.Log(x, b);

            public FunctionExtension clone() => new Logarithmm(b, x);
        }*/
    }
}