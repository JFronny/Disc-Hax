using System;
using System.Collections.Generic;
using System.Linq;
using CC_Functions.Misc;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using Shared;
using Shared.Config;

namespace Bot
{
    public class HelpFormatter : BaseHelpFormatter
    {
        private readonly DiscordEmbedBuilder _builder;
        private readonly CommandContext _ctx;
        private Command? _command;

        public HelpFormatter(CommandContext ctx) : base(ctx)
        {
            _builder = new DiscordEmbedBuilder();
            _ctx = ctx;
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            _command = command;
            _builder.Title = $"{command.QualifiedName} ({(command is CommandGroup ? "Group" : "Command")})";
            if (_ctx.Channel.getMethodEnabled_ext(method: CommandComparer.GetName(command.Name)).FALSE())
                _builder.Title += " (disabled)";
            if (command.Aliases.Any())
                _builder.AddField("Aliases", string.Join(", ", command.Aliases.Select(s => $"`{s}`")));
            if (command.Overloads.Any())
                _builder.AddField("Overloads", string.Join("\n\n",
                    command.Overloads.OrderBy(s => s.Priority).Select(s =>
                    {
                        return
                            $"{command.QualifiedName}{(s.Arguments.Any() ? "\n" : "")}{string.Join("\n", s.Arguments.Select(a => { string tmp = $"-   `{a.Name} "; string type = CommandsNext.GetUserFriendlyTypeName(a.Type); if (a.IsCatchAll) tmp += $"[{type}...]"; else if (a.IsOptional) tmp += $"({type})"; else tmp += $"<{type}>"; return $"{tmp}`: {a.Description}"; }))}";
                    })
                ));
            _builder.Description = command.Description;
            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            string text = string.Join("\n", subcommands
                .Where(s => !s.IsHidden)
                .Select(s => s is CommandGroup group
                    ? $"{s.Name}: {string.Join(" ", group.Children.Where(a => !a.IsHidden).Distinct(new CommandComparer()).Select(a => $"`{a.Name}`"))}"
                    : $"`{s.Name}`")
            );
            Console.WriteLine(text.Length);
            _builder.AddField("Commands", text);
            return this;
        }

        public override CommandHelpMessage Build()
        {
            if (!_ctx.Channel.Get(ConfigManager.Enabled).TRUE()) throw new UnwantedExecutionException();
            if (_command == null)
                _builder.AddField("Notes",
                        "You can use \"help [command]\" to get help about a specific command or group")
                    .AddField($"Current Prefix (`{_ctx.Client.CurrentUser.Mention} a c Prefix` to configure)",
                        _ctx.Channel.Get("Prefix", Common.Prefix))
                    .WithTitle("Help");
            return new CommandHelpMessage(embed: _builder.Build());
        }
    }
}