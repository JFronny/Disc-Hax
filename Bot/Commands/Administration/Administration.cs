using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CC_Functions.Misc;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Eto.Drawing;
using Shared;
using Shared.Config;
using ImageFormat = Eto.Drawing.ImageFormat;

namespace Bot.Commands.Administration
{
    [Group("admin")]
    [Aliases("a")]
    [Description("Commands for administration and debugging")]
    public partial class Administration : BaseCommandModule
    {
        [Command("announce")]
        [Aliases("a")]
        [Description("Post a message containing the specified text to a channel")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task Announce(CommandContext ctx, [RemainingText, Description("The message to announce")] string text) => Announce(ctx, ctx.Channel, text);

        [Command("announce")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task Announce(CommandContext ctx, [Description("Role to mention")] DiscordRole mention, [RemainingText, Description("The message to announce")] string text) => Announce(ctx, mention, ctx.Channel, text);

        [Command("announce")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task Announce(CommandContext ctx, [Description("Channel to post in")] DiscordChannel target, [RemainingText, Description("The message to announce")] string text) => Announce(ctx, ctx.Guild.EveryoneRole, target, text);

        [Command("announce")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Announce(CommandContext ctx, [Description("Role to mention")] DiscordRole mention, [Description("Channel to post in")] DiscordChannel target, [RemainingText, Description("The message to announce")] string text)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .And(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                DiscordMember author = await ctx.Guild.GetMemberAsync(ctx.Message.Author.Id);
                Permissions authorPerms = author.PermissionsIn(target);
                if (mention == ctx.Guild.EveryoneRole && (authorPerms & Permissions.MentionEveryone) != Permissions.MentionEveryone ||
                    (authorPerms & Permissions.AccessChannels) != Permissions.AccessChannels ||
                    (authorPerms & Permissions.EmbedLinks) != Permissions.EmbedLinks ||
                    (authorPerms & Permissions.SendMessages) != Permissions.SendMessages)
                {
                    await ctx.RespondAsync("You don't have sufficient privileges");
                    return;
                }
                await target.SendMessageAsync(mention.Mention, embed: new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        IconUrl = author.AvatarUrl,
                        Name = author.Username
                    },
                    Description = text
                }.Build());
                await ctx.RespondAsync("Done!");
            }
        }
        
        [Command("ping")]
        [Aliases("pong", "p")]
        [Description("Responds with \"Pong\" if the bot is active")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Ping(CommandContext ctx) =>
            await Ping(ctx.Channel, (c1, c2, c3) => ctx.RespondAsync(c1, c2, c3));

        public static async Task Ping(DiscordChannel channel,
            Func<string, bool, DiscordEmbed, Task<DiscordMessage>> postMessage)
        {
            if (channel.Get(ConfigManager.Enabled).True())
                await postMessage($"Pong! ({Program.client.Ping}ms)", false, null);
        }

        [Command("cooldown")]
        [RequireUserPermissions(Permissions.DeafenMembers)]
        [Description("Sets a custom cooldown for this channel")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Cooldown(CommandContext ctx, int cooldown)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .And(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                if (cooldown <= 21600 && cooldown >= 0)
                {
                    await ctx.Channel.ModifyAsync(x => x.PerUserRateLimit = cooldown);
                    await ctx.RespondAsync($"Set cooldown to {cooldown} seconds.");
                    return;
                }
                await ctx.RespondAsync($"Invalid cooldown: {cooldown}");
            }
        }

        [Command("avatar")]
        [Aliases("icon", "av")]
        [Description("Gets the avatar of the specified user")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Avatar(CommandContext ctx, [Description("User to get icon from")] DiscordUser user)
        {
            await ctx.TriggerTypingAsync();
            WebClient wc = new WebClient();
            Bitmap bmp = new Bitmap(wc.OpenRead(user.AvatarUrl));
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Jpeg);
            ms.Position = 0;
            await ctx.RespondWithFileAsync("avatar.jpg", ms, $"Avatar of {user.Username}");
            wc.Dispose();
        }

        [Command("sudo")]
        [Description("Debug command - don't use!")]
        [Hidden]
        [RequireOwner]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task SudoAsync(CommandContext ctx, [Description("Member to sudo")] DiscordMember m,
            [Description("Command to sudo")] [RemainingText]
            string command)
        {
            await ctx.TriggerTypingAsync();
            if (ctx.Client.CurrentApplication.Owners.All(x => x.Id != ctx.User.Id))
            {
                await ctx.RespondAsync("You do not have permission to use this command!");
                return;
            }
            await ctx.CommandsNext.ExecuteCommandAsync(ctx.CommandsNext.CreateFakeContext(m, ctx.Channel, command,
                Common.Prefix, ctx.CommandsNext.FindCommand(command, out string _)));
        }

        [Command("stop")]
        [Description("Debug command - don't use!")]
        [Hidden]
        [RequireOwner]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Stop(CommandContext ctx) => Program.Exit = true;

        [Command("clean")]
        [Description("Debug command - don't use!")]
        [Hidden]
        [RequireOwner]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task Clean(CommandContext ctx)
        {
            ctx.Client.DebugLogger.LogMessage(LogLevel.Info, "DiscHax", "Cleaning...", DateTime.Now);
            Cleanup.Clean(CommandArr.GetCommandNames(),
                ctx.Client.Guilds.Select(s =>
                    new Tuple<string, IEnumerable<string>>(s.Key.ToString(),
                        s.Value.Members.Select(u => u.Key.ToString()))),
                ctx.Client.Guilds.SelectMany(s => s.Value.Channels).Select(s => s.Key.ToString()));
            return ctx.RespondAsync("Complete");
        }

        [Command("purge")]
        [Description("Purge commands by user or regex")]
        [RequirePermissions(Permissions.Administrator)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Purge(CommandContext ctx, [Description("User to delete posts from")]
            DiscordMember member, [Description("Amount of messages to search")]
            int span = 50, [Description("Reason for deletion")] string? reason = null)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .And(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                IEnumerable<DiscordMessage> messages = await ctx.Channel.GetMessagesAsync(span);
                messages = messages.Where(s => ctx.Guild.Members.First(a => a.Key == s.Author.Id).Value == member);
                await ctx.Channel.DeleteMessagesAsync(messages, reason);
                await ctx.RespondAsync($"Deleted {messages.Count()} messages");
            }
        }

        [Command("purge")]
        [RequirePermissions(Permissions.Administrator)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Purge(CommandContext ctx, [Description("Regex for messages")] string regex,
            [Description("Amount of messages to search")]
            int span, [Description("Reason for deletion")] string? reason = null)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .And(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                IEnumerable<DiscordMessage> messages = await ctx.Channel.GetMessagesAsync(span);
                Regex rex = new Regex(regex);
                messages = messages.Where(s => rex.IsMatch(s.Content));
                await ctx.Channel.DeleteMessagesAsync(messages, reason);
                await ctx.RespondAsync($"Deleted {messages.Count()} messages");
            }
        }
    }
}