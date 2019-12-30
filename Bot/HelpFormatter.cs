using System.Collections.Generic;
using System.Linq;
using CC_Functions.Misc;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using Shared;
using Shared.Config;

namespace Bot
{
    public class DefHelpFormatter : BaseHelpFormatter
    {
        public readonly DiscordEmbedBuilder EmbedBuilder =
            new DiscordEmbedBuilder().WithTitle("Help").WithColor(0x007FFF);

        /// <summary>
        ///     Creates a new default help formatter.
        /// </summary>
        /// <param name="ctx">Context in which this formatter is being invoked.</param>
        public DefHelpFormatter(CommandContext ctx) : base(ctx)
        {
        }

        private Command Command { get; set; }

        /// <summary>
        ///     Sets the command this help message will be for.
        /// </summary>
        /// <param name="command">Command for which the help message is being produced.</param>
        /// <returns>This help formatter.</returns>
        public override BaseHelpFormatter WithCommand(Command command)
        {
            Command = command;
            EmbedBuilder.WithDescription(
                $"{Formatter.InlineCode(command.Name)}: {command.Description ?? "No description provided."}");
            if (command is CommandGroup cgroup && cgroup.IsExecutableWithoutSubcommands)
                EmbedBuilder.WithDescription(
                    $"{EmbedBuilder.Description}\n\nThis group can be executed as a standalone command.");
            if (command.Aliases?.Any() == true)
                EmbedBuilder.AddField("Aliases", string.Join(", ", command.Aliases.Select(Formatter.InlineCode)));
            if (command.Overloads?.Any() == true)
            {
                string str = string.Join("\n", command.Overloads.OrderByDescending(x => x.Priority).Select(ovl =>
                {
                    string tmp = '`' + command.QualifiedName + string.Join("",
                                     ovl.Arguments.Select(arg =>
                                         (arg.IsOptional || arg.IsCatchAll ? " [" : " <")
                                         + arg.Name
                                         + (arg.IsCatchAll ? "..." : "")
                                         + (arg.IsOptional || arg.IsCatchAll ? ']' : '>')));
                    tmp += "\n`";
                    tmp += string.Join("\n`",
                        ovl.Arguments.Select(s => s.Name + " (" + CommandsNext.GetUserFriendlyTypeName(s.Type) +
                                                  ")`: " + s.Description ?? "No description provided."));
                    return tmp;
                }));
                EmbedBuilder.AddField("Arguments", str);
            }
            return this;
        }

        /// <summary>
        ///     Sets the subcommands for this command, if applicable. This method will be called with filtered data.
        /// </summary>
        /// <param name="subcommands">Subcommands for this command group.</param>
        /// <returns>This help formatter.</returns>
        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            EmbedBuilder.AddField(Command != null ? "Subcommands" : "Commands",
                string.Join(", ", subcommands.Select(x => Formatter.InlineCode(x.Name))));
            return this;
        }

        /// <summary>
        ///     Construct the help message.
        /// </summary>
        /// <returns>Data for the help message.</returns>
        public override CommandHelpMessage Build()
        {
            if (Command == null)
                EmbedBuilder.WithDescription(
                    "Listing all top-level commands and groups. Specify a command to see more information.");
            return new CommandHelpMessage(embed: EmbedBuilder.Build());
        }
    }

    public class HelpFormatter : BaseHelpFormatter
    {
        private readonly DiscordEmbedBuilder builder;
        private readonly CommandContext ctx;
        private Command command;

        public HelpFormatter(CommandContext ctx) : base(ctx)
        {
            builder = new DiscordEmbedBuilder();
            this.ctx = ctx;
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            this.command = command;
            builder.Title = $"{command.QualifiedName} ({(command is CommandGroup ? "Group" : "Command")})";
            if (command.Aliases.Any())
                builder.AddField("Aliases", string.Join(", ", "`" + command.Aliases + "`"));
            if (command.Overloads.Any())
                builder.AddField("Overloads", string.Join("\n\n",
                    command.Overloads.OrderBy(s => s.Priority).Select(s =>
                    {
                        return $"{command.QualifiedName}{(s.Arguments.Any() ? "\n" : "")}" + string.Join("\n",
                                   s.Arguments.Select(a =>
                                   {
                                       string tmp = $"-   `{a.Name} ";
                                       if (a.IsCatchAll)
                                           tmp += $"[{a.Type}...]";
                                       else if (a.IsOptional)
                                           tmp += $"({a.Type})";
                                       else
                                           tmp += $"<{a.Type}>";
                                       return tmp + $"`: {a.Description}";
                                   }));
                    })
                ));
            builder.Description = command.Description;
            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            builder.AddField("SubCommands",
                string.Join("\n", subcommands
                    .Where(s => !s.IsHidden)
                    .Select(s => $"`{s.Name}`" + (string.IsNullOrEmpty(s.Description) ? "" : ": " + s.Description)
                                               + (s is CommandGroup group
                                                   ? "\n    SubCommands:" + string.Join(", ", group.Children
                                                         .Where(a => !a.IsHidden)
                                                         .Distinct(new CommandComparer())
                                                         .Select(a => $"`{a.Name}`"))
                                                   : ""))
                ));
            return this;
        }

        public override CommandHelpMessage Build()
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled).TRUE())
            {
                if (command == null)
                    builder.AddField("Notes",
                        "You can use help [command] to get help about a specific command or group").WithTitle("Help");
                return new CommandHelpMessage(embed: builder.Build());
            }
            throw new UnwantedExecutionException();
        }
    }
}