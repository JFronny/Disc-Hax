using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bot.Properties;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Net.WebSocket;
using Octokit;
using Shared;
using Shared.Config;
using Application = System.Windows.Forms.Application;

namespace Bot
{
    internal static class Program
    {
        public static Random rnd = new Random();
        public static Task BotThread;
        public static Bot Bot;
        public static CancellationTokenSource TokenSource;
        public static MainForm form;
        private static NotifyIcon notifyIcon;
        private static ApplicationContext ctx;
        public static GitHubClient cli;

        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string appGuid = ((GuidAttribute) Assembly.GetExecutingAssembly()
                .GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value;
            string mutexId = string.Format("Global\\{{{0}}}", appGuid);
            bool createdNew;
            MutexAccessRule allowEveryoneRule = new MutexAccessRule(
                new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl,
                AccessControlType.Allow);
            MutexSecurity securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);
            using (Mutex mutex = new Mutex(false, mutexId, out createdNew, securitySettings))
            {
                bool hasHandle = false;
                try
                {
                    try
                    {
                        hasHandle = mutex.WaitOne(5000, false);
                        if (hasHandle == false)
                            throw new TimeoutException("Timeout waiting for exclusive access");
                    }
                    catch (AbandonedMutexException)
                    {
#if DEBUG
                        Console.WriteLine("Mutex abandoned");
#endif
                        hasHandle = true;
                    }
                    Console.WriteLine("Initializing");
                    notifyIcon = new NotifyIcon();
                    MenuItem formItem = new MenuItem("Show");
                    MenuItem exitItem = new MenuItem("Exit");
                    ContextMenu contextMenu = new ContextMenu(new[] {formItem, exitItem});
                    formItem.Index = 0;
                    formItem.Click += (sender, e) =>
                    {
                        if (form == null)
                            form = new MainForm();
                        form.Show();
                    };
                    exitItem.Index = 1;
                    exitItem.Click += (sender, e) => { ctx.ExitThread(); };
                    notifyIcon.Text = "DiscHax";
                    notifyIcon.Icon = Resources.TextTemplate;
                    notifyIcon.ContextMenu = contextMenu;
                    notifyIcon.Visible = true;
                    cli = new GitHubClient(new ProductHeaderValue("DiscHax"));
                    DiscordConfiguration cfg = new DiscordConfiguration
                    {
                        Token = TokenManager.Token,
                        TokenType = TokenType.Bot,
                        AutoReconnect = true,
#if DEBUG
                        LogLevel = LogLevel.Debug,
#else
                        LogLevel = LogLevel.Info,
#endif
                        UseInternalLogHandler = false
                    };
                    if (Type.GetType("Mono.Runtime") != null)
                        cfg.WebSocketClientFactory = WebSocketSharpClient.CreateNew;
                    Bot = new Bot(cfg);
                    Bot.Client.Ready += Bot_Ready;
                    Bot.Client.GuildAvailable += Bot_GuildAvailable;
                    Bot.Client.GuildCreated += Bot_GuildCreated;
                    Bot.Client.GuildUnavailable += Bot_GuildUnavailable;
                    Bot.Client.GuildDeleted += Bot_GuildDeleted;
                    Bot.Client.MessageCreated += Bot_MessageCreated;
                    Bot.Client.ClientErrored += Bot_ClientErrored;
                    TokenSource = new CancellationTokenSource();
                    BotThread = Task.Run(BotThreadCallback);
                    if (args == null || args.Length == 0 || args.Contains("form"))
                    {
                        form = new MainForm();
                        form.Show();
                    }

                    ctx = new ApplicationContext();
                    Application.Run(ctx);
                    TokenSource.Cancel();
                    notifyIcon.Visible = false;
                }
                finally
                {
                    if (hasHandle)
                        mutex.ReleaseMutex();
                }
            }
        }

        private static async Task BotThreadCallback()
        {
            await Bot.StartAsync().ConfigureAwait(false);
            try
            {
                await Task.Delay(-1, TokenSource.Token).ConfigureAwait(false);
            }
            catch
            {
            }

            await Bot.StopAsync().ConfigureAwait(false);
            if (form != null)
            {
                form.SetProperty(x => x.Text, "DiscHax Bot Menu");
                form.channelTree.InvokeAction(new Action(form.channelTree.Nodes.Clear));
                form.SelectedGuild = default;
                form.SelectedChannel = default;
            }

            Bot = null;
            TokenSource = null;
            BotThread = null;
        }

        private static Task Bot_Ready(ReadyEventArgs e)
        {
            form?.SetProperty(xf => xf.Text, "DiscHax Bot Menu (connected)");
            Bot.Client.DebugLogger.LogMessage(LogLevel.Info, "DiscHax",
                "Your invite Link: https://discordapp.com/oauth2/authorize?client_id=" +
                e.Client.CurrentApplication.Id + "&scope=bot&permissions=8", DateTime.Now);
            return Task.CompletedTask;
        }

        private static Task Bot_GuildAvailable(GuildCreateEventArgs e)
        {
            form?.channelTree.InvokeAction(new Action<BotGuild>(form.AddGuild), new BotGuild(e.Guild));
            return Task.CompletedTask;
        }

        private static Task Bot_GuildCreated(GuildCreateEventArgs e)
        {
            form?.channelTree.InvokeAction(new Action<BotGuild>(form.AddGuild), new BotGuild(e.Guild));
            return Task.CompletedTask;
        }

        private static Task Bot_GuildUnavailable(GuildDeleteEventArgs e)
        {
            form?.channelTree.InvokeAction(new Action<ulong>(form.RemoveGuild), e.Guild.Id);
            return Task.CompletedTask;
        }

        private static Task Bot_GuildDeleted(GuildDeleteEventArgs e)
        {
            form?.channelTree.InvokeAction(new Action<ulong>(form.RemoveGuild), e.Guild.Id);
            return Task.CompletedTask;
        }

        private static Task Bot_MessageCreated(MessageCreateEventArgs e)
        {
            form?.channelTree.InvokeAction(new Action<BotMessage, BotChannel>(form.AddMessage),
                new BotMessage(e.Message), new BotChannel(e.Channel));
            return Task.CompletedTask;
        }

        private static Task Bot_ClientErrored(ClientErrorEventArgs e)
        {
            Bot.Client.DebugLogger.LogMessage(LogLevel.Error, "DiscHax", $"Exception in {e.EventName}: {e.Exception}",
                DateTime.Now);
            return Task.CompletedTask;
        }
    }
}