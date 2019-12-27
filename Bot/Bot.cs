﻿using System;
using System.Threading.Tasks;
using Bot.Commands;
using Bot.Converters;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.VoiceNext;
using Shared.Config;
using Math = Bot.Commands.Math;

namespace Bot
{
    public class Bot
    {
        public static Bot instance;

        public Bot(DiscordConfiguration cfg)
        {
            instance = this;
            Client = new DiscordClient(cfg);
            Commands = Client.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new[] {Common.prefix},
                EnableDms = false
            });
            Commands.CommandExecuted += Commands_CommandExecuted;
            Commands.CommandErrored += Commands_CommandErrored;
            _ = Client.UseInteractivity(new InteractivityConfiguration
            {
                PaginationBehaviour = PaginationBehaviour.Ignore,
                Timeout = TimeSpan.FromMinutes(2)
            });
            _ = Client.UseVoiceNext(new VoiceNextConfiguration());
            Commands.RegisterCommands<ImageBoards>();
            Commands.RegisterCommands<Administration>();
            //Commands.RegisterCommands<Music>();
            Commands.RegisterCommands<Misc>();
            Commands.RegisterCommands<Math>();
            Commands.RegisterCommands<Quotes>();
            Commands.RegisterCommands<PublicStats>();
            Commands.RegisterConverter(new CurrencyConv());
            Client.DebugLogger.LogMessageReceived += DebugLogger_LogMessageReceived;
        }

        public DiscordClient Client { get; }
        public CommandsNextExtension Commands { get; set; }

        private async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, "DiscHax",
                $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}",
                DateTime.Now);
            if (e.Exception is ChecksFailedException ex)
            {
                DiscordEmoji emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Title = "Access denied",
                    Description = $"{emoji} You do not have the permissions required to execute this command.",
                    Color = new DiscordColor(0xFF0000)
                };
                await e.Context.RespondAsync("", embed: embed);
            }
            else if (!(e.Exception is CommandNotFoundException))
            {
                await e.Context.RespondAsync($"The command failed: {e.Exception.Message}");
            }
        }

        private Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, "DiscHax",
                $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'", DateTime.Now);
            return Task.CompletedTask;
        }

        public Task StartAsync() => Client.ConnectAsync();

        public Task StopAsync() => Client.DisconnectAsync();

        private void DebugLogger_LogMessageReceived(object sender, DebugLogMessageEventArgs e)
        {
            Console.WriteLine(
                $"[{e.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")}] [{e.Application}] [{e.Level}] {e.Message}");
        }
    }
}