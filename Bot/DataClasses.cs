using DSharpPlus.Entities;

namespace Bot
{
    public interface IBotStruct
    {
        ulong Id { get; }
    }
    public struct BotGuild : IBotStruct
    {
        public DiscordGuild Guild { get; }
        public ulong Id => Guild.Id;

        public BotGuild(DiscordGuild gld) => Guild = gld;

        public override string ToString() => Guild.Name;
    }

    public struct BotChannel : IBotStruct
    {
        public DiscordChannel Channel { get; }
        public ulong Id => Channel.Id;

        public BotChannel(DiscordChannel chn) => Channel = chn;

        public override string ToString() => $"#{Channel.Name}";
    }

    public struct BotMessage : IBotStruct
    {
        public DiscordMessage Message { get; }
        public ulong Id => Message.Id;

        public BotMessage(DiscordMessage msg) => Message = msg;

        public override string ToString() => Message.Content;
    }
}
