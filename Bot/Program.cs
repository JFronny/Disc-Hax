using System;
using System.Threading.Tasks;
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

namespace Bot
{
    internal static class Program
    {
        public static readonly Random Rnd = new Random();
        public static DiscordClient? client;
        public static GitHubClient? Github;
        public static Perspective? Perspective;
        public static DateTime Start = DateTime.Now;
        public static bool Exit;
        private static CommandsNextExtension? Commands { get; set; }

        [STAThread]
        private static void Main()
        {
            using Application app = new Application();
            Console.WriteLine("Initializing");
#if NO_TIMED_BAN || NO_NSFW
            Console.WriteLine("Build config:");
#if NO_TIMED_BAN
            Console.WriteLine("- NO_TIMED_BAN");
#endif
#if NO_NSFW
            Console.WriteLine("- NO_NSFW");
#endif
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
            client = new DiscordClient(cfg);
            Commands = client.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new string[0],
                EnableDms = false,
                PrefixResolver = async msg =>
                {
                    if (msg.Author.IsBot)
                        return -1;
                    string prefix = msg.Channel.Get(ConfigManager.Prefix, Common.Prefix);
                    string content = msg.Content.TrimStart(' ');
                    if (content.StartsWith(prefix))
                    {
                        if (content.StartsWith($"{prefix} "))
                            return prefix.Length + 1;
                        return prefix.Length;
                    }
                    if (!content.StartsWith(client.CurrentUser.Mention)) return -1;
                    if (content.StartsWith($"{client.CurrentUser.Mention} "))
                        return client.CurrentUser.Mention.Length + 1;
                    return client.CurrentUser.Mention.Length;
                }
            });
            Commands.CommandExecuted += Commands_CommandExecuted;
            Commands.CommandErrored += Commands_CommandErrored;
            client.UseInteractivity(new InteractivityConfiguration
            {
                PaginationBehaviour = PaginationBehaviour.Ignore,
                Timeout = TimeSpan.FromMinutes(2)
            });
            Commands.RegisterAll();
            client.DebugLogger.LogMessageReceived += DebugLogger_LogMessageReceived;
            client.Ready += ClientReady;
            client.MessageCreated += AddMessage;
            client.ClientErrored += ClientClientErrored;
            client.ConnectAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            while (!Exit)
                Exit = ProcessLoop.RunIteration(client);
            client?.DisconnectAsync().GetAwaiter().GetResult();
            client?.Dispose();
            client = null;
        }

        private static Task ClientReady(ReadyEventArgs e)
        {
            client.DebugLogger.LogMessage(LogLevel.Info, "DiscHax", "Ready!", DateTime.Now);
            client.DebugLogger.LogMessage(LogLevel.Info, "DiscHax",
                $"Your invite Link: https://discordapp.com/oauth2/authorize?client_id={e.Client.CurrentApplication.Id}&scope=bot&permissions=8",
                DateTime.Now);
            client.DebugLogger.LogMessage(LogLevel.Info, "DiscHax", "Enter \"x\" to stop", DateTime.Now);
            client.UpdateStatusAsync(new DiscordActivity("help", ActivityType.ListeningTo));
            return Task.CompletedTask;
        }

        private static Task AddMessage(MessageCreateEventArgs e)
        {
            if (!e.Author.IsBot)
                e.Guild.IncrementMoney(e.Guild.Members[e.Author.Id],
                    Rnd.Next(0, Math.Max(e.Message.Content.Length / 25, 20)));
            client.UpdateStatusAsync(new DiscordActivity("help", ActivityType.ListeningTo));
            return Task.CompletedTask;
        }

        private static Task ClientClientErrored(ClientErrorEventArgs e)
        {
            client.DebugLogger.LogMessage(LogLevel.Error, "DiscHax", $"Exception in {e.EventName}: {e.Exception}",
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