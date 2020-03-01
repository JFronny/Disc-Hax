using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CC_Functions.Misc;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Octokit;
using Shared;
using Shared.Config;

namespace Bot.Commands
{
    [Group("quote")]
    [Aliases("q")]
    [Description("Random pieces of text from various sources")]
    public class Quotes : BaseCommandModule
    {
        private static readonly string[] _beequotes;
        private static readonly string[] _fortunequotes;
        private static readonly string[] _fortunequotesOff;

        static Quotes()
        {
            Console.Write("Downloading BeeMovie quotes...");
            using (WebClient client = new WebClient())
            {
                _beequotes = client
                    .DownloadString(
                        "http://www.script-o-rama.com/movie_scripts/a1/bee-movie-script-transcript-seinfeld.html")
                    .Split(new[] {"<pre>"}, StringSplitOptions.None)[1]
                    .Split(new[] {"</pre>"}, StringSplitOptions.None)[0]
                    .Split(new[] {"\n\n  \n"}, StringSplitOptions.None);
                _beequotes[0] = _beequotes[0].Replace("  \n  \n", "");
            }
            Console.WriteLine(" Finished.");
            Console.Write("Downloading fortunes");
            _fortunequotes = getFortuneQuotes("fortune-mod/datfiles");
            _fortunequotesOff = getFortuneQuotes("fortune-mod/datfiles/off/unrotated");
            Console.WriteLine(" Finished.");
        }

        private static string[] getFortuneQuotes(string path)
        {
            Console.Write(".");
            IEnumerable<RepositoryContent> files =
                Program.Github.Repository.Content.GetAllContents("shlomif", "fortune-mod", path).GetAwaiter()
                    .GetResult();
            Console.Write(".");
            IEnumerable<string?> disallowednames = new[] {"CMakeLists.txt", null};
            Console.Write(".");
            IEnumerable<RepositoryContent> filteredFiles =
                files.Where(s => s.Type == ContentType.File && !disallowednames.Contains(s.Name));
            Console.Write(".");
            IEnumerable<string> cookies =
                filteredFiles.Where(s => !disallowednames.Contains(s.Name)).Select(s => s.DownloadUrl);
            Console.Write(".");
            IEnumerable<string> contents;
            using (WebClient client = new WebClient())
            {
                contents = cookies.Select(s =>
                {
                    Console.Write(".");
                    return client.DownloadString(s);
                });
                Console.Write(".");
            }
            return contents.SelectMany(s => s.Split(new[] {"\n%\n"}, StringSplitOptions.None)).ToArray();
        }

        [Command("fortune")]
        [Description("Spits out a quote")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Fortune(CommandContext ctx)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                string[] quotes = ctx.Channel.getEvaluatedNSFW() ? _fortunequotesOff : _fortunequotes;
                await ctx.RespondAsyncFix(quotes[Program.Rnd.Next(quotes.Length)], true);
            }
        }

        [Command("beemovie")]
        [Description("Sends a quote from the bee movie script as TTS")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Bees(CommandContext ctx)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                int q = Program.Rnd.Next(_beequotes.Length - 2);
                await ctx.RespondAsyncFix(
                    (_beequotes[q] + "\n\n" + _beequotes[q + 1] + "\n\n" + _beequotes[q + 2]).Replace("\n", "\r\n"),
                    true);
            }
        }
    }
}