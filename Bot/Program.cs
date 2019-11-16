using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DSharpPlus;
using Microsoft.VisualBasic;

namespace Bot
{
    static class Program
    {
        public static DiscordConfiguration cfg;
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string inp = (args != null && args.Length > 0) ? args[0] : File.Exists("Token.txt") ? File.ReadAllText("Token.txt") : null;
            cfg = null;
            int i = 0;
            while (cfg == null)
            {
                try
                {
                    if (i > 0 || inp == null)
                        inp = Interaction.InputBox("Token");
                    if (string.IsNullOrWhiteSpace(inp)) Environment.Exit(0);
                    cfg = new DiscordConfiguration { Token = inp, TokenType = TokenType.Bot, AutoReconnect = true, LogLevel = LogLevel.Debug, UseInternalLogHandler = false };
                }
                catch
                {
                }
                i++;
            }
            Application.Run(new Form());
        }
    }
}
