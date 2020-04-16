using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
using Language = Bot.Commands.Language;
using Math = Bot.Commands.Math;

namespace Bot
{
    internal static class Program
    {
        public static readonly Random Rnd = new Random();
        public static DiscordClient? Bot;
        public static GitHubClient? Github;
        public static Perspective? Perspective;
        public static DateTime Start = DateTime.Now;
        public static bool Exit = false;
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
            Bot = new DiscordClient(cfg);
            Commands = Bot.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new string[0],
                EnableDms = false,
                PrefixResolver = async msg =>
                {
                    string prefix = msg.Channel.Get(ConfigManager.Prefix, Common.Prefix);
                    string content = msg.Content;
                    if (content.StartsWith(prefix))
                    {
                        if (content.StartsWith($"{prefix} "))
                            return prefix.Length + 1;
                        return prefix.Length;
                    }
                    if (!content.StartsWith(Bot.CurrentUser.Mention)) return -1;
                    if (content.StartsWith($"{Bot.CurrentUser.Mention} "))
                        return Bot.CurrentUser.Mention.Length + 1;
                    return Bot.CurrentUser.Mention.Length;
                }
            });
            Commands.CommandExecuted += Commands_CommandExecuted;
            Commands.CommandErrored += Commands_CommandErrored;
            Bot.UseInteractivity(new InteractivityConfiguration
            {
                PaginationBehaviour = PaginationBehaviour.Ignore,
                Timeout = TimeSpan.FromMinutes(2)
            });
            Commands.RegisterCommands<Administration>();
            Commands.RegisterCommands<ImageBoards>();
            Commands.RegisterCommands<Japan>();
            Commands.RegisterCommands<Language>();
            Commands.RegisterCommands<LocalStats>();
            Commands.RegisterCommands<Math>();
            Commands.RegisterCommands<Minigames>();
            Commands.RegisterCommands<Misc>();
            Commands.RegisterCommands<Money>();
            Commands.RegisterCommands<PublicStats>();
            Commands.RegisterCommands<Quotes>();
            Commands.RegisterCommands<ReactionRoles>();
            Commands.RegisterConverter(new BoardConv());
            Commands.RegisterConverter(new BooruConv());
            Commands.RegisterConverter(new CurrencyConv());
            Commands.RegisterConverter(new DoujinEnumConv());
            Commands.RegisterConverter(new RpsOptionConv());
            Commands.SetHelpFormatter<HelpFormatter>();
            Bot.DebugLogger.LogMessageReceived += DebugLogger_LogMessageReceived;
            Bot.Ready += Bot_Ready;
            Bot.MessageCreated += AddMessage;
            Bot.ClientErrored += Bot_ClientErrored;
            Bot.ConnectAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            while (!Exit)
                try
                {
                    if (Console.KeyAvailable && Console.Read() == 'x')
                    {
                        Exit = true;
                        continue;
                    }
                    Thread.Sleep(5000);
#if !NO_TIMED_BAN
                    if (Bot == null) return;
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
                finally
                {
                    try
                    {
                        foreach (KeyValuePair<ulong, DiscordGuild> guild in Bot.Guilds)
                        {
                            Dictionary<string,DiscordRole> roles = guild.Value.GetReactionRoles();
                            (ulong? channel, ulong? message) = guild.Value.GetReactionRoleMessage() ?? new Tuple<ulong?, ulong?>(0, 0);
                            if (channel == 0 || message == 0) continue;
                            DiscordMessage msg =  
                                Bot.GetChannelAsync(channel.Value).Result.GetMessageAsync(message.Value).Result;
                            foreach (DiscordEmoji disallowed in msg.Reactions.Where(s => !s.IsMe).Select(s => s.Emoji))
                                msg.DeleteReactionsEmojiAsync(disallowed);
                            foreach (KeyValuePair<string,DiscordRole> role in roles)
                            {
                                Func<DiscordReaction, bool> selector = s =>
                                    s.IsMe && s.Emoji.GetDiscordName() == role.Key;
                                DiscordEmoji emoji = DiscordEmoji.FromName(Bot, role.Key);
                                if (!msg.Reactions.Any(selector)) msg.CreateReactionAsync(emoji);
                                IReadOnlyList<DiscordUser> reactions = msg.GetReactionsAsync(emoji).Result;
                                foreach (KeyValuePair<ulong,DiscordMember> member in guild.Value.Members)
                                {
                                    bool inGroup = member.Value.Roles.Contains(role.Value);
                                    bool shouldBe = reactions.Contains(member.Value);
                                    if (inGroup == shouldBe) continue;
                                    if (shouldBe)
                                        member.Value.GrantRoleAsync(role.Value);
                                    else
                                        member.Value.RevokeRoleAsync(role.Value);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Bot.DebugLogger.LogMessage(LogLevel.Error, "DiscHax",
                            $"A crash occured in the reaction-roles Thread: {e}", DateTime.Now, e);
                    }
                }
            Bot?.DisconnectAsync().GetAwaiter().GetResult();
            Bot?.Dispose();
            Bot = null;
        }

        private static Task Bot_Ready(ReadyEventArgs e)
        {
            Bot.DebugLogger.LogMessage(LogLevel.Info, "DiscHax", "Ready!", DateTime.Now);
            Bot.DebugLogger.LogMessage(LogLevel.Info, "DiscHax",
                $"Your invite Link: https://discordapp.com/oauth2/authorize?client_id={e.Client.CurrentApplication.Id}&scope=bot&permissions=8",
                DateTime.Now);
            Bot.DebugLogger.LogMessage(LogLevel.Info, "DiscHax", "Enter \"x\" to stop", DateTime.Now);
            Bot.UpdateStatusAsync(new DiscordActivity("help", ActivityType.ListeningTo));
            return Task.CompletedTask;
        }

        private static Task AddMessage(MessageCreateEventArgs e)
        {
            if (!e.Author.IsBot)
                e.Guild.IncrementMoney(e.Guild.Members[e.Author.Id],
                    Rnd.Next(0, System.Math.Max(e.Message.Content.Length / 25, 20)));
            Bot.UpdateStatusAsync(new DiscordActivity("help", ActivityType.ListeningTo));
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