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
        [Description("Prints out the README file from the repository")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task About(CommandContext ctx)
        {
            if (ctx.Channel.getInstance().get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getInstance().getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondPaginated(HTMLProcessor.ToPlainText(
                    await (await Program.cli.Repository.Content.GetReadme("JFronny", "Disc-Hax")).GetHtmlContent()));
            }
        }

        [Command("guildcount")]
        [Description("Gets the amount of connected Guilds")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task GuildCount(CommandContext ctx)
        {
            if (ctx.Channel.getInstance().get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getInstance().getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync($"Currently connected to {GuildSingleton.Count} Guilds");
            }
        }

        [Command("changelog")]
        [Description("Gets the amount of connected Guilds")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Changelog(CommandContext ctx)
        {
            if (ctx.Channel.getInstance().get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getInstance().getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsyncFix((await Program.cli.Repository.Commit.GetAll("JFronny", "Disc-Hax"))[0].Commit
                    .Message);
            }
        }

        [Command("invite")]
        [Description("Gets the invite-link")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Invite(CommandContext ctx)
        {
            if (ctx.Channel.getInstance().get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getInstance().getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync(
                    $"Invite Link: https://discordapp.com/oauth2/authorize?client_id={ctx.Client.CurrentApplication.Id}&scope=bot&permissions=8");
            }
        }

        [Command("github")]
        [Aliases("website", "contribute", "issue")]
        [Description("Pastes the github link")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Github(CommandContext ctx)
        {
            if (ctx.Channel.getInstance().get(ConfigManager.ENABLED)
                .AND(ctx.Channel.getInstance().getMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync("Repo Link: https://github.com/JFronny/Disc-Hax");
            }
        }
    }
}