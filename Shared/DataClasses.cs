#region

using System.Collections.Generic;
using DSharpPlus.Entities;

#endregion

namespace Shared
{
    public interface IBotStruct
    {
        ulong Id { get; }
    }

    public class BotGuild : IBotStruct
    {
        public Dictionary<ulong, BotChannel> Channels = new Dictionary<ulong, BotChannel>();

        public BotGuild(DiscordGuild gld) => Guild = gld;
        public DiscordGuild Guild { get; }
        public ulong Id => Guild.Id;

        public override string ToString() => Guild.Name;
    }

    public class BotChannel : IBotStruct
    {
        private readonly Dictionary<ulong, BotMessage> _messages = new Dictionary<ulong, BotMessage>();

        public BotChannel(DiscordChannel chn) => Channel = chn;
        public DiscordChannel Channel { get; }

        public Dictionary<ulong, BotMessage> Messages
        {
            get
            {
                while (_messages.Count > 100) _messages.RemoveAt(99);
                return _messages;
            }
        }

        public ulong Id => Channel.Id;

        public override string ToString() => $"#{Channel.Name}";
    }

    public class BotMessage : IBotStruct
    {
        public BotMessage(DiscordMessage msg) => Message = msg;
        public DiscordMessage Message { get; }
        public ulong Id => Message.Id;

        public override string ToString() =>
            $"<{(Message.Author.IsCurrent ? "SELF>" : Message.Author.IsBot ? $"BOT>[{Message.Author.Username}]" : $"USER>[{Message.Author.Username}]")}{Message.Content}";
    }
}