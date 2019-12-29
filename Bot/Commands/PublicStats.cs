#region

using System.Threading.Tasks;
using CC_Functions.Misc;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Shared;
using Shared.Config;

#endregion

namespace Bot.Commands
{
    public class PublicStats : BaseCommandModule
    {
        [Command("about")]
        [Description("Prints out the Readme.md file from the repository")]
        public async Task About(CommandContext ctx)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled).TRUE())
                await ctx.RespondPaginated(HTMLProcessor.ToPlainText(
                    await (await Program.cli.Repository.Content.GetReadme("JFronny", "Disc-Hax")).GetHtmlContent()));
        }

        [Command("guildcount")]
        [Description("Gets the amount of connected Guilds")]
        public async Task GuildCount(CommandContext ctx)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled).TRUE())
                await ctx.RespondAsync($"Currently connected to {GuildSingleton.Count} Guilds");
        }

        [Command("changelog")]
        [Description("Gets the amount of connected Guilds")]
        public async Task Changelog(CommandContext ctx)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled).TRUE())
                await ctx.RespondAsyncFix((await Program.cli.Repository.Commit.GetAll("JFronny", "Disc-Hax"))[0].Commit
                    .Message);
        }

        [Command("invite")]
        [Description("Gets the invite-link")]
        public async Task Invite(CommandContext ctx)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled).TRUE())
                await ctx.RespondAsync(
                    $"Invite Link: https://discordapp.com/oauth2/authorize?client_id={ctx.Client.CurrentApplication.Id}&scope=bot&permissions=8");
        }
    }
}