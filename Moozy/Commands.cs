using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;

namespace Moozy
{
    public class Commands : BaseCommandModule
    {
        [Command("join")]
        public async Task Join(CommandContext ctx)
        {
            DiscordMessage tmp = await ctx.RespondAsync("JN: GVNC");
            VoiceNextExtension vnext = ctx.Client.GetVoiceNext();
            await tmp.DeleteAsync();
            tmp = await ctx.RespondAsync("JN: GGLD");
            VoiceNextConnection vnc = vnext.GetConnection(ctx.Guild);
            await tmp.DeleteAsync();
            tmp = await ctx.RespondAsync("JN: CVNC");
            if (vnc != null)
                throw new InvalidOperationException("Already connected in this guild.");
            await tmp.DeleteAsync();
            tmp = await ctx.RespondAsync("JN: CCHN");
            DiscordChannel chn = ctx.Member?.VoiceState?.Channel;
            if (chn == null)
                throw new InvalidOperationException("You need to be in a voice channel.");
            await tmp.DeleteAsync();
            tmp = await ctx.RespondAsync("JN: CTXC");
            vnc = await vnext.ConnectAsync(chn);
            await tmp.DeleteAsync();
            await ctx.RespondAsync("👌Connected");
        }

        [Command("leave")]
        public async Task Leave(CommandContext ctx)
        {
            DiscordMessage tmp = await ctx.RespondAsync("DC");
            VoiceNextExtension vnext = ctx.Client.GetVoiceNext();
            await tmp.DeleteAsync();
            tmp = await ctx.RespondAsync("DC: GVNC");
            VoiceNextConnection vnc = vnext.GetConnection(ctx.Guild);
            await tmp.DeleteAsync();
            tmp = await ctx.RespondAsync("DC: CVNC");
            if (vnc == null)
                throw new InvalidOperationException("Not connected in this guild.");
            await tmp.DeleteAsync();
            tmp = await ctx.RespondAsync("DC: DCXC");
            vnc.Disconnect();
            await tmp.DeleteAsync();
            await ctx.RespondAsync("👌");
        }
    }
}