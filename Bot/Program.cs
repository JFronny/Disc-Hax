using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DSharpPlus;
using Microsoft.VisualBasic;

namespace Bot
{
    class Program
    {
        public static DiscordConfiguration cfg;
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            cfg = null;
            while (cfg == null)
            {
                try
                {
                    cfg = new DiscordConfiguration { Token = Interaction.InputBox("Token"), TokenType = TokenType.Bot, AutoReconnect = true, LogLevel = LogLevel.Debug, UseInternalLogHandler = false };
                }
                catch
                {
                }
            }
            Application.Run(new Form());
        }
    }
}
