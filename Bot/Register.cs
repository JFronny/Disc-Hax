using Bot.Commands;
using Bot.Converters;
using DSharpPlus.CommandsNext;

namespace Bot
{
    public static class Register
    {
        public static void RegisterAll(this CommandsNextExtension client)
        {
            client.RegisterCommands<Administration>();
            client.RegisterCommands<ImageBoards>();
            client.RegisterCommands<Japan>();
            client.RegisterCommands<Language>();
            client.RegisterCommands<LocalStats>();
            client.RegisterCommands<Math>();
            client.RegisterCommands<Minigames>();
            client.RegisterCommands<Misc>();
            client.RegisterCommands<Money>();
            client.RegisterCommands<PublicStats>();
            client.RegisterCommands<Quotes>();
            client.RegisterCommands<ReactionRoles>();
            client.RegisterConverter(new BoardConv());
            client.RegisterConverter(new BooruConv());
            client.RegisterConverter(new CurrencyConv());
            client.RegisterConverter(new DoujinEnumConv());
            client.RegisterConverter(new RpsOptionConv());
            client.SetHelpFormatter<HelpFormatter>();
        }
    }
}