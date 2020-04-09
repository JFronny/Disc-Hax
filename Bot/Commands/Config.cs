using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CC_Functions.Misc;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Shared.Config;

namespace Bot.Commands
{
    public partial class Administration
    {
        [Command("config")]
        [Aliases("cfg", "c")]
        [RequireUserPermissions(Permissions.Administrator)]
        [Description("Prints or changes the config for this channel/guild (empty for guild)")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ConfigCmd(CommandContext ctx) => ConfigCmd(ctx, ctx.Guild);

        [Command("config")]
        [RequireUserPermissions(Permissions.Administrator)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ConfigCmd(CommandContext ctx, [Description("The channel to configure")]
            DiscordChannel channel) => ConfigCmd(ctx, (SnowflakeObject) channel);

        private async Task ConfigCmd(CommandContext ctx, SnowflakeObject target)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled).TRUE())
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync(target.GetStr(CommandArr.GetC()));
            }
        }

        [Command("config")]
        [RequireUserPermissions(Permissions.Administrator)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ConfigCmd(CommandContext ctx, [Description("Config element to print")]
            string element) => ConfigCmd(ctx, ctx.Guild, element);

        [Command("config")]
        [RequireUserPermissions(Permissions.Administrator)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ConfigCmd(CommandContext ctx, [Description("The channel to configure")]
            DiscordChannel channel, [Description("Config element to print")]
            string element) => ConfigCmd(ctx, (SnowflakeObject) channel, element);

        private async Task ConfigCmd(CommandContext ctx, SnowflakeObject target, string element)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled).TRUE())
            {
                await ctx.TriggerTypingAsync();
                if (element.ToLower() == "prefix")
                    await ctx.RespondAsync(
                        $"{element}: {target.Get(element, target is DiscordGuild ? Common.Prefix : ((DiscordChannel) target).Guild.Get("Prefix", Common.Prefix))}");
                else
                {
                    if (!CommandArr.GetC().Contains(element))
                        throw new ArgumentException($"Element ({element}) not in CommandArr");
                    await ctx.RespondAsync($"{element}: {target.Get(element)}");
                }
            }
        }

        [Command("config")]
        [RequireUserPermissions(Permissions.Administrator)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ConfigCmd(CommandContext ctx, [Description("Config element to change")]
            string element, [Description("New value")] string value) => ConfigCmd(ctx, ctx.Guild, element, value);

        [Command("config")]
        [RequireUserPermissions(Permissions.Administrator)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ConfigCmd(CommandContext ctx, [Description("The channel to configure")]
            DiscordChannel channel, [Description("Config element to change")]
            string element, [Description("New value")] string value) =>
            ConfigCmd(ctx, (SnowflakeObject) channel, element, value);

        private async Task ConfigCmd(CommandContext ctx, SnowflakeObject target, string element, string value)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled).TRUE())
            {
                await ctx.TriggerTypingAsync();
                if (element.ToLower() == "prefix")
                    target.Set("Prefix", value);
                else
                {
                    if (!CommandArr.GetC().Contains(element))
                        throw new ArgumentException($"Element ({element}) not in CommandArr");
                    target.Set(element, ctx.Client.GetCommandsNext().ConvertArgument<bool>(value, ctx).ToString());
                }
                await ctx.RespondAsync($"Set {element} to {value}");
            }
        }
    }
}