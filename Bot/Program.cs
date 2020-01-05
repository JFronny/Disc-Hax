using System;
using System.Collections.Generic;
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
using DSharpPlus.Entities;
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
            args = args == null || args.Length == 0
                ? new[] {"form"}
                : args.Select(s => s.Trim('-', '\\')).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            bool showForm = args.Contains("form") || args.Contains("form-show");
            bool formKey = args.Contains("form") || args.Contains("form-key");
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
                    notifyIcon = new NotifyIcon
                    {
                        Text = "DiscHax", Icon = Resources.TextTemplate, Visible = true, ContextMenu = new ContextMenu()
                    };
                    if (formKey)
                    {
                        MenuItem formItem = new MenuItem("Show");
                        formItem.Index = 0;
                        formItem.Click += (sender, e) =>
                        {
                            if (form != null && !form.IsDisposed)
                                form.Dispose();
                            form = new MainForm();
                            form.Show();
                        };
                        notifyIcon.ContextMenu.MenuItems.Add(formItem);
                    }
                    MenuItem exitItem = new MenuItem("Exit");
                    exitItem.Click += (sender, e) =>
                    {
                        Bot.Client.DisconnectAsync();
                        Bot.Client.Dispose();
                        ctx.ExitThread();
                    };
                    notifyIcon.ContextMenu.MenuItems.Add(exitItem);
                    cli = new GitHubClient(new ProductHeaderValue("DiscHax"));
                    DiscordConfiguration cfg = new DiscordConfiguration
                    {
                        Token = TokenManager.DiscordToken,
                        TokenType = TokenType.Bot,
                        AutoReconnect = true,
#if DEBUG
                        LogLevel = LogLevel.Debug,
#else
                        LogLevel = LogLevel.Info,
#endif
                        UseInternalLogHandler = false
                    };
                    TokenManager.CurrencyconverterapiToken.ToString();
                    if (Type.GetType("Mono.Runtime") != null)
                        cfg.WebSocketClientFactory = WebSocketSharpClient.CreateNew;
                    Bot = new Bot(cfg);
                    Bot.Client.Ready += Bot_Ready;
                    Bot.Client.GuildAvailable += AddGuild;
                    Bot.Client.GuildCreated += AddGuild;
                    Bot.Client.GuildUnavailable += RemoveGuild;
                    Bot.Client.GuildDeleted += RemoveGuild;
                    Bot.Client.ChannelCreated += AddChannel;
                    Bot.Client.ChannelDeleted += RemoveChannel;
                    Bot.Client.MessageCreated += AddMessage;
                    Bot.Client.MessageDeleted += RemoveMessage;
                    Bot.Client.ClientErrored += Bot_ClientErrored;
                    TokenSource = new CancellationTokenSource();
                    BotThread = Task.Run(BotThreadCallback);
                    if (showForm)
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
                form.ChannelTree.InvokeAction(new Action(form.ChannelTree.Nodes.Clear));
                form.SelectedGuild = default;
                form.SelectedChannel = default;
            }

            Bot = null;
            TokenSource = null;
            BotThread = null;
        }

        private static Task Bot_Ready(ReadyEventArgs e)
        {
            if (form != null && !form.IsDisposed)
                form.SetProperty(xf => xf.Text, "DiscHax Bot Menu (connected)");
            Bot.Client.DebugLogger.LogMessage(LogLevel.Info, "DiscHax",
                $"Your invite Link: https://discordapp.com/oauth2/authorize?client_id={e.Client.CurrentApplication.Id}&scope=bot&permissions=8",
                DateTime.Now);
            return Task.CompletedTask;
        }

        private static Task AddGuild(GuildCreateEventArgs e)
        {
            BotGuild tmp = GuildSingleton.Add(e.Guild);
            foreach (KeyValuePair<ulong, DiscordChannel> channel in e.Guild.Channels)
                e.Guild.getInstance().Channels.Add(channel.Key, new BotChannel(channel.Value));
            if (form != null && !form.IsDisposed)
                form.ChannelTree.InvokeAction(new Action<BotGuild>(form.AddGuild), tmp);
            return Task.CompletedTask;
        }

        private static Task RemoveGuild(GuildDeleteEventArgs e)
        {
            GuildSingleton.Remove(e.Guild.Id);
            if (form != null && !form.IsDisposed)
                form.ChannelTree.InvokeAction(new Action<ulong>(form.RemoveGuild), e.Guild.Id);
            return Task.CompletedTask;
        }

        private static Task AddChannel(ChannelCreateEventArgs e)
        {
            e.Guild.getInstance().Channels.Add(e.Channel.Id, new BotChannel(e.Channel));
            return Task.CompletedTask;
        }

        private static Task RemoveChannel(ChannelDeleteEventArgs e)
        {
            e.Guild.getInstance().Channels.Remove(e.Channel.Id);
            return Task.CompletedTask;
        }

        private static Task AddMessage(MessageCreateEventArgs e)
        {
            e.Message.Channel.getInstance().Messages.Add(e.Message.Id, new BotMessage(e.Message));
            if (form != null && !form.IsDisposed)
                form.ChannelTree.InvokeAction(new Action<BotMessage, BotChannel>(form.AddMessage),
                    new BotMessage(e.Message), new BotChannel(e.Channel));
            if (!e.Author.IsBot)
                ConfigManager.incrementMoney(e.Guild.getInstance(), e.Author,
                    rnd.Next(0, Math.Max(e.Message.Content.Length / 25, 20)));
            return Task.CompletedTask;
        }

        private static Task RemoveMessage(MessageDeleteEventArgs e)
        {
            e.Channel.getInstance().Messages.Remove(e.Message.Id);
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