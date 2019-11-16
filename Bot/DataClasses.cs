using DSharpPlus.Entities;

namespace Bot
{
    public struct BotGuild
    {
        public DiscordGuild Guild { get; }
        public ulong Id => this.Guild.Id;

        public BotGuild(DiscordGuild gld)
        {
            this.Guild = gld;
        }

        public override string ToString()
        {
            return this.Guild.Name;
        }
    }

    public struct BotChannel
    {
        public DiscordChannel Channel { get; }
        public ulong Id => this.Channel.Id;

        public BotChannel(DiscordChannel chn)
        {
            this.Channel = chn;
        }

        public override string ToString()
        {
            return $"#{this.Channel.Name}";
        }
    }

    public struct BotMessage
    {
        public DiscordMessage Message { get; }
        public ulong Id => this.Message.Id;

        public BotMessage(DiscordMessage msg)
        {
            this.Message = msg;
        }

        public override string ToString()
        {
            return this.Message.Content;
        }
    }
}
