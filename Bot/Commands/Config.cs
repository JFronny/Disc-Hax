using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CC_Functions.Misc;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Shared.Config;

namespace Bot.Commands
{
    public partial class Administration
    {
        [Command("config")]
        [Aliases("cfg", "c")]
        [RequireUserPermissions(Permissions.Administrator)]
        [Description(
            "Prints or changes the config for this channel/guild (empty for guild). You can also set all commands in a group by using \"group_[NAME]\"")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ConfigCmd(CommandContext ctx) => ConfigCmd(ctx, ctx.Guild);

        [Command("reset-config")]
        [RequireUserPermissions(Permissions.Administrator)]
        [Description("Reverts all configs")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task ConfigReset(CommandContext ctx)
        {
            InteractivityExtension interactivity = ctx.Client.GetInteractivity();
            DiscordMessage msg =
                await ctx.RespondAsync(
                    "Please type out \"yes\" to confirm. All configuration for your server will be reset!");
            InteractivityResult<DiscordMessage> tmp =
                await interactivity.WaitForMessageAsync(s => s.Author == ctx.Message.Author, new TimeSpan(0, 0, 30));
            if (!tmp.TimedOut || tmp.Result.Content != "yes")
            {
                string path;
                foreach ((ulong key, DiscordChannel _) in ctx.Guild.Channels)
                {
                    ConfigManager.GetXml(key.ToString(), ConfigManager.Channel, out path);
                    File.Delete(path);
                }
                ConfigManager.GetXml(ctx.Guild.Id.ToString(), ConfigManager.Guild, out path);
                File.Delete(path);
                await msg.ModifyAsync("Deleted.");
            }
            else
                await msg.ModifyAsync("Cancelled.");
        }

        [Command("reset-config")]
        [RequireUserPermissions(Permissions.Administrator)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task ConfigReset(CommandContext ctx, [Description("The channel to reset")] DiscordChannel channel)
        {
            InteractivityExtension interactivity = ctx.Client.GetInteractivity();
            DiscordMessage msg =
                await ctx.RespondAsync(
                    "Please type out \"yes\" to confirm. All configuration for this channel will be reset!");
            InteractivityResult<DiscordMessage> tmp =
                await interactivity.WaitForMessageAsync(s => s.Author == ctx.Message.Author, new TimeSpan(0, 0, 30));
            if (!tmp.TimedOut || tmp.Result.Content != "yes")
            {
                ConfigManager.GetXml(channel.Id.ToString(), ConfigManager.Channel, out string path);
                File.Delete(path);
                await msg.ModifyAsync("Deleted.");
            }
            else
                await msg.ModifyAsync("Cancelled.");
        }

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
                await ctx.RespondAsync(target.GetStr(CommandArr.GetCommandNames()));
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
                if (string.Equals(element, ConfigManager.Prefix, StringComparison.CurrentCultureIgnoreCase))
                    await ctx.RespondAsync(
                        $"{element}: {target.Get(element, target is DiscordGuild ? Common.Prefix : ((DiscordChannel) target).Guild.Get(ConfigManager.Prefix, Common.Prefix))}");
                else
                {
                    if (!CommandArr.GetCommandNames().Contains(element))
                        throw new ArgumentException($"Element ({element}) not in CommandArr");
                    await ctx.RespondAsync($"{element}: {target.Get(element)}");
                }
            }
        }

        [Command("config")]
        [RequireUserPermissions(Permissions.Administrator)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ConfigCmd(CommandContext ctx, [Description("Config element to change")]
            string element, [Description("New value")] string value)
        {
            foreach ((ulong _, DiscordChannel channel) in ctx.Guild.Channels) channel.Reset(value);
            return ConfigCmd(ctx, ctx.Guild, element, value);
        }

        [Command("config")]
        [RequireUserPermissions(Permissions.Administrator)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ConfigCmd(CommandContext ctx, [Description("The channel to configure")]
            DiscordChannel channel, [Description("Config element to change")]
            string element, [Description("New value")] string value) =>
            ConfigCmd(ctx, (SnowflakeObject) channel, element, value);

        private async Task ConfigCmd(CommandContext ctx, SnowflakeObject target, string element, string value,
            bool noComment = false)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled).TRUE() ||
                ctx.Guild.Channels.All(s => s.Value.Get(ConfigManager.Enabled).FALSE()))
            {
                if (!noComment)
                    await ctx.TriggerTypingAsync();
                if (string.Equals(element, ConfigManager.Prefix, StringComparison.CurrentCultureIgnoreCase))
                    target.Set(ConfigManager.Prefix, value);
                else if (CommandArr.GetGroupNames().Contains(element))
                    foreach (string t in CommandArr.GetCommandNames(element))
                        await ConfigCmd(ctx, target, t, value, true);
                else if (!CommandArr.GetCommandNames().Contains(element))
                    throw new ArgumentException($"Element ({element}) not in CommandArr");
                else
                {
                    if (target is DiscordGuild guild)
                    {
                        if (string.Equals(element, ConfigManager.Enabled, StringComparison.CurrentCultureIgnoreCase))
                        {
                            await ctx.RespondAsync("Please do not EVER set \"enabled\" to false globally!");
                            return;
                        }
                        foreach ((ulong _, DiscordChannel channel) in guild.Channels)
                            await ConfigCmd(ctx, channel, element, value, true);
                    }
                    target.Set(element,
                        (await ctx.Client.GetCommandsNext().ConvertArgument<bool>(value, ctx)).ToString());
                }
                if (!noComment)
                    await ctx.RespondAsync($"Set {element} to {value}");
            }
        }
    }
}