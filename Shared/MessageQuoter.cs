using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace Shared
{
    public static class MessageQuoter
    {
        public static Task<DiscordMessage> Quote(this DiscordMessage msg, CommandContext ctx)
        {
            if (ctx.Channel.GetEvaluatedNsfw() || !msg.Channel.GetEvaluatedNsfw())
                return ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        IconUrl = msg.Author.AvatarUrl,
                        Name = $"{msg.Author.Username}#{msg.Author.Discriminator}"
                    },
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"{msg.Timestamp:yyyy-dd-M--HH-mm-ss} in {msg.Channel.Name}"
                    },
                    Description = msg.Content
                }.Build());
            return ctx.RespondAsync(
                "You are trying to post a message from a NSFW channel in a non-NSFW channel\nDont do that.");
        }
    }
}