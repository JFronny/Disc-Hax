using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using CC_Functions.Misc;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using Shared.Config;

namespace Shared
{
    public delegate void SetPropertyDelegate<TCtl, TProp>(TCtl control, Expression<Func<TCtl, TProp>> propexpr,
        TProp value) where TCtl : Control;

    public delegate void InvokeActionDelegate<TCtl>(TCtl control, Delegate dlg, params object[] args)
        where TCtl : Control;

    public static class ClassExtensions
    {
        public static void SetProperty<TCtl, TProp>(this TCtl control, Expression<Func<TCtl, TProp>> propexpr,
            TProp value) where TCtl : Control
        {
            if (control == null)
                throw new ArgumentNullException(nameof(control));

            if (propexpr == null)
                throw new ArgumentNullException(nameof(propexpr));
            if (control.InvokeRequired)
            {
                control.Invoke(new SetPropertyDelegate<TCtl, TProp>(SetProperty), control, propexpr, value);
                return;
            }

            MemberExpression propexprm = propexpr.Body as MemberExpression;
            if (propexprm == null)
                throw new ArgumentException("Invalid member expression.", nameof(propexpr));
            PropertyInfo prop = propexprm.Member as PropertyInfo;
            if (prop == null)
                throw new ArgumentException("Invalid property supplied.", nameof(propexpr));
            prop.SetValue(control, value);
        }

        public static void InvokeAction<TCtl>(this TCtl control, Delegate dlg, params object[] args)
            where TCtl : Control
        {
            if (control == null)
                throw new ArgumentNullException(nameof(control));
            if (dlg == null)
                throw new ArgumentNullException(nameof(dlg));
            if (control.InvokeRequired)
            {
                control.Invoke(new InvokeActionDelegate<TCtl>(InvokeAction), control, dlg, args);
                return;
            }

            dlg.DynamicInvoke(args);
        }

        public static bool getEvaluatedNSFW(this DiscordChannel Channel) =>
            Channel.IsNSFW || Channel.get(ConfigManager.NSFW, false).TRUE();

        public static string emotify(this string self)
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

        public static string leetify(this string self)
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

        public static Task RespondPaginated(this CommandContext ctx, string message)
        {
            InteractivityExtension interactivity = ctx.Client.GetInteractivity();
            Page[] pages = interactivity.GeneratePagesInEmbed(message);
            return interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages,
                deletion: PaginationDeletion.DeleteMessage);
        }

        public static Task<DiscordMessage> RespondAsyncFix(this CommandContext ctx, string content = null,
            bool isTTS = false, DiscordEmbed embed = null) =>
            ctx.RespondAsync(content.Replace("*", "\\*").Replace("_", "\\_"), isTTS, embed);

        public static bool IsLocal(this Uri self)
        {
            IPAddress[] local = Dns.GetHostAddresses(Dns.GetHostName());
            return Dns.GetHostAddresses(self.Host).Count(host => IPAddress.IsLoopback(host) || local.Contains(host)) >
                   0;
        }

        public static string GetString(this DiscordMessage msg) => $"<{(msg.Author.IsCurrent ? "SELF>" : msg.Author.IsBot ? $"BOT>[{msg.Author.Username}]" : $"USER>[{msg.Author.Username}]")}{msg.Content}";
    }
}
