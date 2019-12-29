using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CC_Functions.Misc;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Octokit;
using Shared;
using Shared.Config;

namespace Bot.Commands
{
    public class Quotes : BaseCommandModule
    {
        private static string[] _beequotes;
        private readonly string[] fortunequotes;
        private readonly string[] fortunequotes_off;

        public Quotes()
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
            fortunequotes = getFortuneQuotes("fortune-mod/datfiles");
            fortunequotes_off = getFortuneQuotes("fortune-mod/datfiles/off/unrotated");
            Console.WriteLine(" Finished.");
        }

        private string[] getFortuneQuotes(string path)
        {
            Console.Write(".");
            IEnumerable<RepositoryContent> files =
                Program.cli.Repository.Content.GetAllContents("shlomif", "fortune-mod", path).GetAwaiter().GetResult();
            Console.Write(".");
            IEnumerable<string> disallowednames = new[] {"CMakeLists.txt", null};
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
        public async Task Fortune(CommandContext ctx)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled)
                .AND(ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Fortune)))
            {
                string[] quotes = ctx.Channel.getEvaluatedNSFW() ? fortunequotes_off : fortunequotes;
                await ctx.RespondAsyncFix(quotes[Program.rnd.Next(quotes.Length)], true);
            }
        }

        [Command("beemovie")]
        [Description("Sends a quote from the bee movie script as TTS")]
        public async Task Bees(CommandContext ctx)
        {
            if (ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Enabled)
                .AND(ConfigManager.get(ctx.Channel.getInstance(), ConfigElement.Bees)))
            {
                int q = Program.rnd.Next(_beequotes.Length - 2);
                await ctx.RespondAsyncFix(
                    (_beequotes[q] + "\n\n" + _beequotes[q + 1] + "\n\n" + _beequotes[q + 2]).Replace("\n", "\r\n"),
                    true);
            }
        }
    }
}