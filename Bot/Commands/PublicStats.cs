using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CC_Functions.Misc;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Shared;
using Shared.Config;

namespace Bot.Commands
{
    [Group("stat")]
    [Aliases("s")]
    [Description("Information that is not unique to this server")]
    public class PublicStats : BaseCommandModule
    {
        [Command("about")]
        [Description("Prints some info about the bot")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task About(CommandContext ctx)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync($@"Webpage: https://jfronny.github.io/home/bot
Repo: https://github.com/JFronny/Disc-Hax
Uptime: {(DateTime.Now - Program.Start).GetReadable()}");
            }
        }

        [Command("guildcount")]
        [Description("Gets the amount of connected Guilds")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task GuildCount(CommandContext ctx)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync($"Currently connected to {Program.client.Guilds.Count} Guilds");
            }
        }

        [Command("changelog")]
        [Description("Gets the amount of connected Guilds")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Changelog(CommandContext ctx)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsyncFix((await Program.Github.Repository.Commit.GetAll("JFronny", "Disc-Hax"))[0]
                    .Commit
                    .Message);
            }
        }

        [Command("invite")]
        [Description("Gets the invite-link")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Invite(CommandContext ctx)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync(
                    $"Invite Link: https://discordapp.com/oauth2/authorize?client_id={ctx.Client.CurrentApplication.Id}&scope=bot&permissions=8");
            }
        }

        [Command("github")]
        [Aliases("contribute", "issue")]
        [Description("Pastes the github link")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Github(CommandContext ctx)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync("Repo Link: https://github.com/JFronny/Disc-Hax");
            }
        }

        [Command("website")]
        [Aliases("info")]
        [Description("Pastes the github link")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Website(CommandContext ctx)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync("Webpage: https://jfronny.github.io/home/bot");
            }
        }

        [Command("uptime")]
        [Aliases("u")]
        [Description("Prints the bots uptime")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Uptime(CommandContext ctx)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync($"Up for {(DateTime.Now - Program.Start).GetReadable()}");
            }
        }
    }
}