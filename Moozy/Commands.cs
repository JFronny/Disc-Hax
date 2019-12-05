using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using System;
using System.Threading.Tasks;

namespace Moozy
{
    public class Commands
    {
        [Command("join")]
        public async Task Join(CommandContext ctx)
        {
            DiscordMessage tmp = await ctx.RespondAsync("JN: GVNC");
            var vnext = ctx.Client.GetVoiceNextClient();
            await tmp.DeleteAsync();
            tmp = await ctx.RespondAsync("JN: GGLD");
            var vnc = vnext.GetConnection(ctx.Guild);
            await tmp.DeleteAsync();
            tmp = await ctx.RespondAsync("JN: CVNC");
            if (vnc != null)
                throw new InvalidOperationException("Already connected in this guild.");
            await tmp.DeleteAsync();
            tmp = await ctx.RespondAsync("JN: CCHN");
            var chn = ctx.Member?.VoiceState?.Channel;
            if (chn == null)
                throw new InvalidOperationException("You need to be in a voice channel.");
            await tmp.DeleteAsync();
            tmp = await ctx.RespondAsync("JN: CTXC");
            vnc = await vnext.ConnectAsync(chn);
            await tmp.DeleteAsync();
            await ctx.RespondAsync("👌");
        }

        [Command("leave")]
        public async Task Leave(CommandContext ctx)
        {
            DiscordMessage tmp = await ctx.RespondAsync("DC");
            var vnext = ctx.Client.GetVoiceNextClient();
            await tmp.DeleteAsync();
            tmp = await ctx.RespondAsync("DC: GVNC");
            var vnc = vnext.GetConnection(ctx.Guild);
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
