using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Catharsis.Commons;
using CC_Functions.Misc;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Shared.Config;

namespace Bot.Commands.Administration
{
    public partial class Administration
    {
        [Command("role-color")]
        [Aliases("set-color", "color")]
        [RequirePermissions(Permissions.ManageRoles)]
        [Description("Set a roles color")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task SetColor(CommandContext ctx, [Description("The role to modify")] DiscordRole role, [Description("The new color")] DiscordColor color, [Description("Reason for the action"), RemainingText] string reason)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                DiscordMember member = await ctx.Guild.GetMemberAsync(ctx.Message.Author.Id);
                if (member.IsOwner || member.Roles.Any() && member.Roles.OrderBy(s => s.Position).Last().Position > role.Position)
                {
                    await role.ModifyAsync(color: color, reason: reason);
                    await ctx.RespondAsync("Done!");
                }
                else
                    await ctx.RespondAsync("You can't edit this role.");
            }
        }
        
        [Command("role-mentionable")]
        [Aliases("set-mention", "mentionable")]
        [RequirePermissions(Permissions.ManageRoles)]
        [Description("Change a roles mentionability")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task SetMentionable(CommandContext ctx, [Description("The role to modify")] DiscordRole role, [Description("Whether it can be mentioned")] bool mentionable, [Description("Reason for the action"), RemainingText] string reason)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                DiscordMember member = await ctx.Guild.GetMemberAsync(ctx.Message.Author.Id);
                if (member.IsOwner || member.Roles.Any() && member.Roles.OrderBy(s => s.Position).Last().Position > role.Position)
                {
                    await role.ModifyAsync(mentionable: mentionable, reason: reason);
                    await ctx.RespondAsync("Done!");
                }
                else
                    await ctx.RespondAsync("You can't edit this role.");
            }
        }
        
        [Command("role-hoist")]
        [Aliases("set-hoist", "hoist")]
        [RequirePermissions(Permissions.ManageRoles)]
        [Description("Set grouping behaviour")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task SetHoist(CommandContext ctx, [Description("The role to modify")] DiscordRole role, [Description("Whether to group members by this role")] bool hoist, [Description("Reason for the action"), RemainingText] string reason)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                DiscordMember member = await ctx.Guild.GetMemberAsync(ctx.Message.Author.Id);
                if (member.IsOwner || member.Roles.Any() && member.Roles.OrderBy(s => s.Position).Last().Position > role.Position)
                {
                    await role.ModifyAsync(hoist: hoist, reason: reason);
                    await ctx.RespondAsync("Done!");
                }
                else
                    await ctx.RespondAsync("You can't edit this role.");
            }
        }
        
        [Command("role-name")]
        [Aliases("set-name")]
        [RequirePermissions(Permissions.ManageRoles)]
        [Description("Set a roles name")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task SetName(CommandContext ctx, [Description("The role to modify")] DiscordRole role, [Description("The new name")] string name, [Description("Reason for the action"), RemainingText] string reason)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                DiscordMember member = await ctx.Guild.GetMemberAsync(ctx.Message.Author.Id);
                if (member.IsOwner || member.Roles.Any() && member.Roles.OrderBy(s => s.Position).Last().Position > role.Position)
                {
                    await role.ModifyAsync(name, reason: reason);
                    await ctx.RespondAsync("Done!");
                }
                else
                    await ctx.RespondAsync("You can't edit this role.");
            }
        }
        
        [Command("remove-bots")]
        [Aliases("rmbot")]
        [RequirePermissions(Permissions.Administrator)]
        [Description("Remove the role from all members that are bots")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task RemoveBots(CommandContext ctx, [Description("The role to modify")] DiscordRole role, [Description("Reason for the action"), RemainingText] string reason)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                DiscordMember member = await ctx.Guild.GetMemberAsync(ctx.Message.Author.Id);
                foreach (DiscordMember bot in ctx.Guild.Members.Select(s => s.Value).Where(s => s.IsBot && s.Roles.Contains(role))) await bot.RevokeRoleAsync(role, reason);
                await ctx.RespondAsync("Done!");
            }
        }
        
        [Command("remove-humans")]
        [Aliases("rmbot")]
        [RequirePermissions(Permissions.Administrator)]
        [Description("Remove the role from all members that are not bots")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task RemoveHumans(CommandContext ctx, [Description("The role to modify")] DiscordRole role, [Description("Reason for the action"), RemainingText] string reason)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                DiscordMember member = await ctx.Guild.GetMemberAsync(ctx.Message.Author.Id);
                foreach (DiscordMember human in ctx.Guild.Members.Select(s => s.Value).Where(s => !s.IsBot && s.Roles.Contains(role))) await human.RevokeRoleAsync(role, reason);
                await ctx.RespondAsync("Done!");
            }
        }
    }
}