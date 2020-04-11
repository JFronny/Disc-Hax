using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CC_Functions.Misc;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;
using Shared;
using Shared.Config;
using Yandex.Translator;

namespace Bot.Commands
{
    [Group("lang")]
    [Aliases("l")]
    [Description("Commands for translation and dictionaries")]
    public class Language : BaseCommandModule
    {
        private IYandexTranslator translator;
        public Language()
        {
            translator = Yandex.Translator.Yandex.Translator(api => api.ApiKey("trnsl.1.1.20200118T180605Z.654f2ec649458c36.107c6ad38dc02937f25e660aa1f8f4097d6561a8").Format(ApiDataFormat.Json));
        }
        
        [Command("urban")]
        [Aliases("u")]
        [Description("Search urban dictionary for a term")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Urban(CommandContext ctx, [Description("The term to search"), RemainingText] string term)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                if (ctx.Channel.GetEvaluatedNsfw())
                {
                    using WebClient c = new WebClient();
                    JObject result = (JObject) JObject.Parse(c.DownloadString(
                        $"https://api.urbandictionary.com/v0/define?term={Uri.EscapeDataString(term)}"))["list"][0];
                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = result.Value<string>("word"),
                            Author = new DiscordEmbedBuilder.EmbedAuthor
                            {
                                Name = result.Value<string>("author"),
                                Url = $"https://www.urbandictionary.com/author.php?author={Uri.EscapeDataString(result.Value<string>("author"))}"
                            },
                            Url = result.Value<string>("permalink"),
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"{result.Value<string>("thumbs_up")}+ {result.Value<string>("thumbs_down")}-"
                            },
                            Timestamp = DateTime.Parse(result.Value<string>("written_on")).Date
                        }
                        .AddField("Definition", result.Value<string>("definition"))
                        .AddField("Example", result.Value<string>("example"))
                        .Build());
                }
                else
                    await ctx.RespondAsync("NSFW Channels only!");
            }
        }
        
        [Command("translate")]
        [Aliases("t")]
        [Description("Translate a short piece of text")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Translate(CommandContext ctx, [Description("The language to translate to")] string lang, [Description("The term to search"), RemainingText] string term)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                if (term.Length > 200)
                {
                    await ctx.RespondAsync("Max length is 200");
                    return;
                }
                await ctx.RespondAsync(translator.Translate(lang, term).Text);
            }
        }
        
        [Command("corrupt")]
        [Aliases("c")]
        [Description("Corrupt text by translating it over and over")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task Corrupt(CommandContext ctx, [Description("The term to search"), RemainingText] string term) => Corrupt(ctx, false, term);

        [Command("corrupt")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Corrupt(CommandContext ctx, [Description("Set to true to fully break the text")] bool breakFully, [Description("The term to search"), RemainingText] string term)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
            {
                await ctx.TriggerTypingAsync();
                if (term.Length > 200)
                {
                    await ctx.RespondAsync("Max length is 200");
                    return;
                }
                IEnumerable<ITranslationPair> pairs = translator.TranslationPairs().OrderBy(_ => Program.Rnd.NextDouble());
                string startLang = translator.Detect(term);
                if (breakFully)
                {
                    ITranslationPair[] translationPairs = pairs as ITranslationPair[] ?? pairs.ToArray();
                    for (int i = 0; i < 20; i++)
                    {
                        ITranslationPair pair = translationPairs[i];
                        term = translator
                            .Translate($"{pair.FromLanguage}-{pair.ToLanguage}", term).Text;
                    }
                }
                else
                {
                    string currentLang = new string(startLang);
                    for (int i = 0; i < 20; i++)
                    {
                        ITranslationPair pair = pairs.First(s => s.FromLanguage == currentLang);
                        currentLang = pair.ToLanguage;
                        term = translator
                            .Translate($"{pair.FromLanguage}-{pair.ToLanguage}", term).Text;
                    }
                }
                await ctx.RespondAsync(translator.Translate(startLang, term).Text);
            }
        }
    }
}