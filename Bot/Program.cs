using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bot.Commands;
using Bot.Converters;
using Bot.Properties;
using CC_Functions.Misc;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.VoiceNext;
using Octokit;
using Shared;
using Shared.Config;
using Application = System.Windows.Forms.Application;
using Math = Bot.Commands.Math;

namespace Bot
{
    internal static class Program
    {
        public static readonly Random Rnd = new Random();
        public static DiscordClient Bot;
        private static CancellationTokenSource _tokenSource;
        private static MainForm _form;
        private static NotifyIcon _notifyIcon;
        private static ApplicationContext _ctx;
        public static GitHubClient Github;
        public static Perspective Perspective;
        private static CommandsNextExtension Commands { get; set; }

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
            string mutexId = "Global\\{DiscHaxBot}";
            MutexAccessRule allowEveryoneRule = new MutexAccessRule(
                new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl,
                AccessControlType.Allow);
            MutexSecurity securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);
            using Mutex mutex = new Mutex(false, mutexId, out bool _, securitySettings);
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
                _notifyIcon = new NotifyIcon
                {
                    Text = "DiscHax", Icon = Resources.TextTemplate, Visible = true, ContextMenu = new ContextMenu()
                };
                if (formKey)
                {
                    MenuItem formItem = new MenuItem("Show");
                    formItem.Click += (sender, e) =>
                    {
                        if (_form != null && !_form.IsDisposed)
                            _form.Dispose();
                        _form = new MainForm();
                        _form.Show();
                    };
                    _notifyIcon.ContextMenu.MenuItems.Add(formItem);
                }
                MenuItem exitItem = new MenuItem("Exit");
                exitItem.Click += (sender, e) =>
                {
                    Bot.DisconnectAsync();
                    Bot.Dispose();
                    _ctx.ExitThread();
                };
                _notifyIcon.ContextMenu.MenuItems.Add(exitItem);
                Github = new GitHubClient(new ProductHeaderValue("DiscHax"));
                Perspective = new Perspective(TokenManager.PerspectiveToken);
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
                Bot = new DiscordClient(cfg);
                Commands = Bot.UseCommandsNext(new CommandsNextConfiguration
                {
                    StringPrefixes = new[] {Common.prefix},
                    EnableDms = false
                });
                Commands.CommandExecuted += Commands_CommandExecuted;
                Commands.CommandErrored += Commands_CommandErrored;
                Bot.UseInteractivity(new InteractivityConfiguration
                {
                    PaginationBehaviour = PaginationBehaviour.Ignore,
                    Timeout = TimeSpan.FromMinutes(2)
                });
                Bot.UseVoiceNext(new VoiceNextConfiguration());
                Commands.RegisterCommands<ImageBoards>();
                Commands.RegisterCommands<Administration>();
                Commands.RegisterCommands<LocalStats>();
                Commands.RegisterCommands<Minigames>();
                Commands.RegisterCommands<Misc>();
                Commands.RegisterCommands<Math>();
                Commands.RegisterCommands<Money>();
                Commands.RegisterCommands<Quotes>();
                Commands.RegisterCommands<PublicStats>();
                Commands.RegisterConverter(new BoardConv());
                Commands.RegisterConverter(new BooruConv());
                Commands.RegisterConverter(new CurrencyConv());
                Commands.RegisterConverter(new RPSOptionConv());
                Commands.SetHelpFormatter<HelpFormatter>();
                Bot.DebugLogger.LogMessageReceived += DebugLogger_LogMessageReceived;
                Bot.Ready += Bot_Ready;
                Bot.GuildAvailable += AddGuild;
                Bot.GuildCreated += AddGuild;
                Bot.GuildUnavailable += RemoveGuild;
                Bot.GuildDeleted += RemoveGuild;
                Bot.ChannelCreated += AddChannel;
                Bot.ChannelDeleted += RemoveChannel;
                Bot.MessageCreated += AddMessage;
                Bot.MessageDeleted += RemoveMessage;
                Bot.ClientErrored += Bot_ClientErrored;
                _tokenSource = new CancellationTokenSource();
                Task.Run(BotThreadCallback);
                new Thread(() =>
                {
                    while (true)
                        try
                        {
                            Thread.Sleep(5000);
                            if (Bot == null)
                                return;
                            foreach (KeyValuePair<ulong, DiscordGuild> guild in Bot.Guilds)
                                guild.Value.EvalBans();
                        }
                        catch (Exception e)
                        {
                            Bot.DebugLogger.LogMessage(LogLevel.Error, "DiscHax",
                                $"A crash occured in the ban-evaluation Thread: {e}", DateTime.Now, e);
                        }
                }).Start();
                if (showForm)
                {
                    _form = new MainForm();
                    _form.Show();
                }
                _ctx = new ApplicationContext();
                Application.Run(_ctx);
                _tokenSource.Cancel();
                _notifyIcon.Visible = false;
            }
            finally
            {
                if (hasHandle)
                    mutex.ReleaseMutex();
            }
        }

        private static async Task BotThreadCallback()
        {
            await Bot.ConnectAsync().ConfigureAwait(false);
            try
            {
                await Task.Delay(-1, _tokenSource.Token).ConfigureAwait(false);
            }
            catch
            {
                // ignored
            }

            await Bot.DisconnectAsync().ConfigureAwait(false);
            if (_form != null)
            {
                _form.SetProperty(x => x.Text, "DiscHax Bot Menu");
                _form.ChannelTree.InvokeAction(new Action(_form.ChannelTree.Nodes.Clear));
                _form.SelectedGuild = default;
                _form.SelectedChannel = default;
            }

            Bot = null;
            _tokenSource = null;
        }

        private static Task Bot_Ready(ReadyEventArgs e)
        {
            if (_form != null && !_form.IsDisposed)
                _form.SetProperty(xf => xf.Text, "DiscHax Bot Menu (connected)");
            Bot.DebugLogger.LogMessage(LogLevel.Info, "DiscHax",
                $"Your invite Link: https://discordapp.com/oauth2/authorize?client_id={e.Client.CurrentApplication.Id}&scope=bot&permissions=8",
                DateTime.Now);
            return Task.CompletedTask;
        }

        private static Task AddGuild(GuildCreateEventArgs e)
        {
            if (_form != null && !_form.IsDisposed)
                _form.ChannelTree.InvokeAction(new Action<DiscordGuild>(_form.AddGuild), e.Guild);
            return Task.CompletedTask;
        }

        private static Task RemoveGuild(GuildDeleteEventArgs e)
        {
            if (_form != null && !_form.IsDisposed)
                _form.ChannelTree.InvokeAction(new Action<ulong>(_form.RemoveGuild), e.Guild.Id);
            return Task.CompletedTask;
        }

        private static Task AddChannel(ChannelCreateEventArgs e) => Task.CompletedTask;

        private static Task RemoveChannel(ChannelDeleteEventArgs e) => Task.CompletedTask;

        private static Task AddMessage(MessageCreateEventArgs e)
        {
            if (_form != null && !_form.IsDisposed)
                _form.ChannelTree.InvokeAction(new Action<DiscordMessage, DiscordChannel>(_form.AddMessage),
                    e.Message, e.Channel);
            if (!e.Author.IsBot)
                e.Guild.IncrementMoney(e.Guild.Members[e.Author.Id],
                    Rnd.Next(0, System.Math.Max(e.Message.Content.Length / 25, 20)));
            return Task.CompletedTask;
        }

        private static Task RemoveMessage(MessageDeleteEventArgs e) => Task.CompletedTask;

        private static Task Bot_ClientErrored(ClientErrorEventArgs e)
        {
            Bot.DebugLogger.LogMessage(LogLevel.Error, "DiscHax", $"Exception in {e.EventName}: {e.Exception}",
                DateTime.Now);
            return Task.CompletedTask;
        }

        private static void DebugLogger_LogMessageReceived(object sender, DebugLogMessageEventArgs e)
        {
            Console.WriteLine(
                $"[{e.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")}] [{e.Application}] [{e.Level}] {e.Message}");
        }

        private static async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            if (e.Context.Channel.Get(ConfigManager.Enabled).TRUE())
            {
                if (e.Exception is UnwantedExecutionException)
                    return;
                e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, "DiscHax",
                    $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception}",
                    DateTime.Now);
                if (e.Exception is ChecksFailedException)
                    await e.Context.RespondAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = "Access denied",
                        Description =
                            $"{DiscordEmoji.FromName(e.Context.Client, ":no_entry:")} You do not have the permissions required to execute this command.",
                        Color = new DiscordColor(0xFF0000)
                    }.Build());
                else if (!(e.Exception is CommandNotFoundException))
                    await e.Context.RespondAsyncFix($"The command failed: {e.Exception.Message}");
            }
        }

        private static Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, "DiscHax",
                $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'", DateTime.Now);
            return Task.CompletedTask;
        }
    }
}