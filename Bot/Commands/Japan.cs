using System;
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

namespace Bot.Commands
{
    [Group("japan")]
    [Aliases("j")]
    [Description("Commands for translation and dictionaries")]
    public class Japan : BaseCommandModule
    {
        [Command("anime")]
        [Aliases("a")]
        [Description("Give information about an anime")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Anime(CommandContext ctx, [Description("The animes name")] [RemainingText]
            string name)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
                await Display(ctx, name, true);
        }

        [Command("manga")]
        [Aliases("m")]
        [Description("Give information about a manga")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Manga(CommandContext ctx, [Description("The mangas name")] [RemainingText]
            string name)
        {
            if (ctx.Channel.Get(ConfigManager.Enabled)
                .AND(ctx.Channel.GetMethodEnabled()))
                await Display(ctx, name, false);
        }

        private async Task Display(CommandContext ctx, string name, bool isAnime)
        {
            using WebClient web = new WebClient();
            JObject o = JObject.Parse(web.DownloadString(
                $"https://kitsu.io/api/edge/{(isAnime ? "anime" : "manga")}?page[limit]=5&filter[text]={name}"));
            if (o["meta"].Value<int>("count") == 0)
                throw new Exception("Not found");
            JObject el = (JObject) o["data"][0];
            JObject att = (JObject) el["attributes"];
            if (
#if !NO_NSFW
                ctx.Channel.GetEvaluatedNsfw() ||
#endif
                !att.Value<bool>("nsfw"))
            {
                string[] tmp1 = att["abbreviatedTitles"].ToObject<string[]>();
                string fullName = att.Value<string>("canonicalTitle") +
                                  (tmp1 == null || tmp1.Length == 0 ? "" : " (" + string.Join(", ", tmp1) + ")");
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Title = fullName.Length > 256 ? att.Value<string>("canonicalTitle") : fullName,
                    Url = $"https://kitsu.io/{(isAnime ? "anime" : "manga")}/{att.Value<string>("slug")}",
                    Description = att.Value<string>("synopsis")
                };
                if (isAnime && HasValue<int>(att, "episodeCount"))
                    embed.AddField("Number of episodes",
                        $"{att.Value<int>("episodeCount")}{(HasValue<int>(att, "episodeLength") ? $"a {att.Value<int>("episodeLength")} min" : "")}",
                        true);
                if (HasValue<string>(att, "averageRating"))
                    embed.AddField("Score", att.Value<string>("averageRating"), true);
                if (HasValue<string>(att, "ageRating"))
                    embed.AddField("Age Rating",
                        $"{att.Value<string>("ageRating")}{(HasValue<string>(att, "ageRatingGuide") ? " (" + att.Value<string>("ageRatingGuide") + ")" : "")}",
                        true);
                embed.AddField("Release date",
                    HasValue<string>(att, "startDate")
                        ? HasValue<string>(att, "endDate")
                            ?
                            $"{att.Value<string>("startDate")} - {att.Value<string>("endDate")}"
                            : att.Value<string>("startDate")
                        : "To be announced", true);
                string img = att["posterImage"].Value<string>("original");
                string val = Program.Rnd.Next(10000, 99999).ToString();
                await ctx.RespondWithFileAsync($"{val}_img.jpg",
                    web.OpenRead(img), embed: embed.Build());
            }
            else
                await ctx.RespondAsync("Nsfw content is not allowed here");
        }

        private static bool HasValue<T>(JObject self, string name)
        {
            try
            {
                return self.Value<T>(name) != null;
            }
            catch
            {
                return false;
            }
        }
    }
}