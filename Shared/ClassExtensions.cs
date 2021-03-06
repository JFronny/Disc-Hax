﻿using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CC_Functions.Misc;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using Shared.Config;

namespace Shared
{
    public static class ClassExtensions
    {
#if !NO_NSFW
        public static bool GetEvaluatedNsfw(this DiscordChannel channel) =>
            channel.IsNSFW || channel.Get(ConfigManager.Nsfw, false).True();
#endif

        public static string Emotify(this string self)
        {
            return string.Join("", self.ToLower().ToCharArray().Select(s =>
            {
                if (Regex.IsMatch(s.ToString(), "[a-z]"))
                    return $":regional_indicator_{s}:";
                return s switch
                {
                    '1' => ":one:",
                    '2' => ":two:",
                    '3' => ":three:",
                    '4' => ":four:",
                    '5' => ":five:",
                    '6' => ":six:",
                    '7' => ":seven:",
                    '8' => ":eight:",
                    '9' => ":nine:",
                    '0' => ":zero:",
                    '!' => ":grey_exclamation:",
                    '?' => ":grey_question:",
                    '>' => ":arrow_forward:",
                    '<' => ":arrow_backward:",
                    '#' => ":hash:",
                    '*' => ":asterisk:",
                    '^' => ":arrow_up_small:",
                    '°' => ":record_button:",
                    '+' => ":heavy_plus_sign:",
                    '-' => ":heavy_minus_sign:",
                    _ => s.ToString()
                };
            }).ToArray());
        }

        public static string Leetify(this string self)
        {
            return string.Join("", self.ToLower().ToCharArray().Select(s =>
            {
                return char.ToLower(s) switch
                {
                    'a' => "4",
                    'b' => "8",
                    'c' => "(",
                    'd' => "[)",
                    'e' => "3",
                    'f' => "|=",
                    'g' => "9",
                    'h' => "||",
                    'i' => "!",
                    'j' => ".]",
                    'k' => "|<",
                    'l' => "1",
                    'm' => "|Y|",
                    'n' => "/\\/",
                    'o' => "0",
                    'p' => "|>",
                    'q' => "0,",
                    'r' => "|2",
                    's' => "5",
                    't' => "7",
                    'u' => "[_]",
                    'v' => "\\/",
                    'w' => "\\v/",
                    'x' => "}{",
                    'y' => "9",
                    'z' => "2",
                    _ => s.ToString()
                };
            }).ToArray());
        }

        public static Task<DiscordMessage> RespondAsyncFix(this CommandContext ctx, string content = null,
            bool isTts = false, DiscordEmbed embed = null) =>
            ctx.RespondAsync(content.Replace("*", "\\*").Replace("_", "\\_"), isTts, embed);

        public static bool IsLocal(this Uri self)
        {
            IPAddress[] local = Dns.GetHostAddresses(Dns.GetHostName());
            return Dns.GetHostAddresses(self.Host).Count(host => IPAddress.IsLoopback(host) || local.Contains(host)) >
                   0;
        }

        public static string GetString(this DiscordMessage msg) =>
            $"<{(msg.Author.IsCurrent ? "SELF>" : msg.Author.IsBot ? $"BOT>[{msg.Author.Username}]" : $"USER>[{msg.Author.Username}]")}{msg.Content}";

        public static Task RespondPaginatedIfTooLong(this CommandContext ctx, string content)
        {
            if (content.Length <= 2000)
                return ctx.RespondAsync(content);
            InteractivityExtension interactivity = ctx.Client.GetInteractivity();
            Page[] pages = interactivity.GeneratePagesInEmbed(content);
            return interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages,
                deletion: PaginationDeletion.DeleteMessage);
        }
    }
}