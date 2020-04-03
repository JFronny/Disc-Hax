using System;
using System.Collections.Generic;
using System.Threading;
using Bot.Commands;
using Bot.Converters;
using CC_Functions.Misc;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using Octokit;
using Shared;
using Shared.Config;
using Application = Eto.Forms.Application;
using DateTime = System.DateTime;
using Math = Bot.Commands.Math;
using Mutex = System.Threading.Mutex;
using Task = System.Threading.Tasks.Task;
using Thread = System.Threading.Thread;

namespace Bot
{
    internal static class Program
    {
        public static readonly Random Rnd = new Random();
        public static DiscordClient Bot;
        private static CancellationTokenSource _tokenSource;
        public static GitHubClient Github;
        public static Perspective Perspective;
        private static CommandsNextExtension Commands { get; set; }

        [STAThread]
        private static void Main()
        {
            using Application app = new Application();
            try
            {
                Console.WriteLine("Initializing");
#if NO_TIMED_BAN
                Console.WriteLine("Build config:");
#endif
#if NO_TIMED_BAN
                Console.WriteLine("- NO_TIMED_BAN");
#endif
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
                Bot.MessageCreated += AddMessage;
                Bot.ClientErrored += Bot_ClientErrored;
                _tokenSource = new CancellationTokenSource();
                Task.Run(BotThreadCallback);
                while (true)
                {
                    try
                    {
                        Thread.Sleep(5000);
#if !NO_TIMED_BAN
                        if (Bot == null)
                            return;
                        foreach (KeyValuePair<ulong, DiscordGuild> guild in Bot.Guilds)
                            guild.Value.EvalBans();
#endif
                    }
                    catch (Exception e)
                    {
#if NO_TIMED_BAN
                        Bot.DebugLogger.LogMessage(LogLevel.Error, "DiscHax",
                            $"A crash occured in the main Thread: {e}", DateTime.Now, e);
#else
                        Bot.DebugLogger.LogMessage(LogLevel.Error, "DiscHax",
                            $"A crash occured in the ban-evaluation Thread: {e}", DateTime.Now, e);
#endif
                    }
                }
            }
            finally
            {
                _tokenSource?.Cancel();
                Bot?.DisconnectAsync();
                Bot?.Dispose();
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
            Bot = null;
            _tokenSource = null;
        }

        private static Task Bot_Ready(ReadyEventArgs e)
        {
            Bot.DebugLogger.LogMessage(LogLevel.Info, "DiscHax", "Ready!", DateTime.Now);
            Bot.DebugLogger.LogMessage(LogLevel.Info, "DiscHax",
                $"Your invite Link: https://discordapp.com/oauth2/authorize?client_id={e.Client.CurrentApplication.Id}&scope=bot&permissions=8",
                DateTime.Now);
            return Task.CompletedTask;
        }

        private static Task AddMessage(MessageCreateEventArgs e)
        {
            if (!e.Author.IsBot)
                e.Guild.IncrementMoney(e.Guild.Members[e.Author.Id],
                    Rnd.Next(0, System.Math.Max(e.Message.Content.Length / 25, 20)));
            return Task.CompletedTask;
        }

        private static Task Bot_ClientErrored(ClientErrorEventArgs e)
        {
            Bot.DebugLogger.LogMessage(LogLevel.Error, "DiscHax", $"Exception in {e.EventName}: {e.Exception}",
                DateTime.Now);
            return Task.CompletedTask;
        }

        private static void DebugLogger_LogMessageReceived(object sender, DebugLogMessageEventArgs e)
        {
            Console.WriteLine(
                $"[{e.Timestamp:yyyy-MM-dd HH:mm:ss}] [{e.Application}] [{e.Level}] {e.Message}");
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