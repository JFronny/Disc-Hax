using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace Bot
{
    public class Bot
    {
        public DiscordClient Client { get; }
        public CommandsNextModule Commands { get; set; }
        public Bot(DiscordConfiguration cfg)
        {
            Client = new DiscordClient(cfg);
            Commands = Client.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefix = "!"
            });
            Commands.CommandExecuted += Commands_CommandExecuted; ;
            Commands.CommandErrored += Commands_CommandErrored; ;
            Commands.RegisterCommands<Commands>();
            Client.DebugLogger.LogMessageReceived += DebugLogger_LogMessageReceived;
        }

        private async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, "ExampleBot", $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);
            if (e.Exception is ChecksFailedException ex)
            {
                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Access denied",
                    Description = $"{emoji} You do not have the permissions required to execute this command.",
                    Color = new DiscordColor(0xFF0000)
                };
                await e.Context.RespondAsync("", embed: embed);
            }
        }

        private Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, "ExampleBot", $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'", DateTime.Now);
            return Task.CompletedTask;
        }

        public Task StartAsync() => Client.ConnectAsync();
        public Task StopAsync()
        {
            return Client.DisconnectAsync();
        }

        private void DebugLogger_LogMessageReceived(object sender, DebugLogMessageEventArgs e) => Debug.WriteLine($"[{e.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")}] [{e.Application}] [{e.Level}] {e.Message}");
    }
}
