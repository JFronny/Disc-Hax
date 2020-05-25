using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CC_Functions.Misc;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Shared;
using Shared.Config;

namespace Bot.Commands
{
    [Group("reactionroles")]
    [Aliases("rr")]
    [Description("Random pieces of text from various sources")]
    public class ReactionRoles : BaseCommandModule
    {
        [Command("bind")]
        [RequirePermissions(Permissions.Administrator)]
        [Description("Bind ReactionRoles to a new guild-wide message")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Bind(CommandContext ctx, [Description("Channel to post in, defaults to current")]
            DiscordChannel? channel = null)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .And(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                channel ??= ctx.Channel;
                await Bind(ctx, await channel.SendMessageAsync("Please react to this message to get your roles"));
            }
        }

        [Command("bind")]
        [RequirePermissions(Permissions.Administrator)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Bind(CommandContext ctx, [Description("Message to bind")] DiscordMessage msg)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .And(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                ctx.Guild.SetReactionRoleMessage(msg);
                await ctx.RespondAsync("Successfully wrote key");
            }
        }

        [Command("unbind")]
        [RequirePermissions(Permissions.Administrator)]
        [Description("Unbind ReactionRoles")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Unbind(CommandContext ctx)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .And(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                (ulong? channel, ulong? message) = ctx.Guild.GetReactionRoleMessage() ??
                                                   throw new Exception("Most likely already unbound");
                if (!channel.HasValue || !message.HasValue)
                {
                    await ctx.RespondAsync("Already unbound");
                    return;
                }
                DiscordMessage msg =
                    await (await ctx.Client.GetChannelAsync(channel.Value)).GetMessageAsync(message.Value);
                if (msg.Author.IsCurrent)
                    await msg.DeleteAsync();
                ctx.Guild.SetReactionRoleMessage(null);
                await ctx.RespondAsync("Done.");
            }
        }

        [Command("list")]
        [Aliases("ls")]
        [Description("List all registered roles")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task List(CommandContext ctx)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .And(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                Dictionary<string, ulong> roles = ctx.Guild.GetReactionRoles();
                if (roles.Count == 0)
                    await ctx.RespondAsync("No items are bound");
                else
                {
                    string text = "";
                    foreach (KeyValuePair<string, ulong> pair in roles)
                        //DiscordEmoji emoticon = await ctx.Guild.GetEmojiAsync(pair.Key);
                        text += $"{pair.Key} - {ctx.Guild.GetRole(pair.Value).Name}\n";
                    await ctx.RespondPaginatedIfTooLong(text.Remove(text.Length - 1));
                }
            }
        }

        [Command("add")]
        [RequirePermissions(Permissions.Administrator)]
        [Aliases("create")]
        [Description("Binds a new role and emoji to RR")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Add(CommandContext ctx, [Description("The emoji to bind")] DiscordEmoji emoji,
            [Description("The role to bind")] DiscordRole role)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .And(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                Dictionary<string, ulong> roles = ctx.Guild.GetReactionRoles();
                if (roles.Count > 19)
                {
                    await ctx.RespondAsync("Too many roles!");
                    return;
                }
                if (roles.ContainsKey(emoji.Name) || roles.ContainsValue(role.Id))
                {
                    await ctx.RespondAsync("Element already registered");
                    return;
                }
                Console.WriteLine(role.Name);
                //I know hard-coding @everyone is bad code but it works for now
                if (emoji.IsManaged || role.IsManaged || role.Name == "@everyone")
                {
                    await ctx.RespondAsync("Invalid value");
                    return;
                }
                roles.Add(emoji.GetDiscordName(), role.Id);
                ctx.Guild.SetReactionRoles(roles);
                await ctx.RespondAsync("Done!");
            }
        }

        [Command("remove")]
        [RequirePermissions(Permissions.Administrator)]
        [Aliases("rm", "del")]
        [Description("Unbinds a role from RR")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Remove(CommandContext ctx, [Description("The emoji to unbind")] DiscordEmoji emoji)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .And(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                Dictionary<string, ulong> roles = ctx.Guild.GetReactionRoles();
                roles.Remove(emoji.GetDiscordName());
                ctx.Guild.SetReactionRoles(roles);
                await ctx.RespondAsync("Done!");
            }
        }

        [Command("remove")]
        [RequirePermissions(Permissions.Administrator)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Remove(CommandContext ctx, [Description("The role to unbind")] DiscordRole role)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .And(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                Dictionary<string, ulong> roles = ctx.Guild.GetReactionRoles();
                roles.Remove(roles.First(s => s.Value == role.Id).Key);
                ctx.Guild.SetReactionRoles(roles);
                await ctx.RespondAsync("Done!");
            }
        }

        [Command("clear")]
        [RequirePermissions(Permissions.Administrator)]
        [Description("Unbinds all roles from RR")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Clear(CommandContext ctx)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .And(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                ctx.Guild.SetReactionRoles(new Dictionary<string, ulong>());
                await ctx.RespondAsync("Done!");
            }
        }

        [Command("jumplink")]
        [RequirePermissions(Permissions.Administrator)]
        [Aliases("jump", "link")]
        [Description("Unbinds all roles from RR")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task JumpLink(CommandContext ctx)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .And(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                (ulong? channel, ulong? message) = ctx.Guild.GetReactionRoleMessage() ??
                                                   throw new Exception("No message is configured in your guild");
                await ctx.RespondAsync(
                    (await (await ctx.Client.GetChannelAsync(channel.Value)).GetMessageAsync(message.Value)).JumpLink
                    .AbsoluteUri);
            }
        }
    }
}