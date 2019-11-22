﻿using Bot.Config;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Commands
{
    public class Misc
    {
        [Command("poll"), Description("Run a poll with reactions. WARNING: Only normal emoticons (:laughing:, :grinning:) are allowed! Special emojis (:one:, :regional_indicator_d:) might cause problems")]
        public async Task Poll(CommandContext ctx, [Description("What to ask")] string text, [Description("How long should the poll last.")] TimeSpan duration, [Description("What options should people have.")] params DiscordEmoji[] options)
        {
            if (ConfigManager.get(ctx.Channel.Id, ConfigElement.Enabled).AND(ConfigManager.get(ctx.Channel.Id, ConfigElement.Poll)))
            {
                IEnumerable<string> poll_options = options.Select(xe => xe.ToString());
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Poll time!",
                    Description = text
                };
                var msg = await ctx.RespondAsync(null, false, embed);
                for (var i = 0; i < options.Length; i++)
                    await msg.CreateReactionAsync(options[i]);
                var poll_result = await Bot.instance.Client.GetInteractivityModule().CollectReactionsAsync(msg, duration);
                var results = poll_result.Reactions.Where(xkvp => options.Contains(xkvp.Key))
                    .Select(xkvp => $"{xkvp.Key}: {xkvp.Value}");
                await ctx.RespondAsync(string.Join("\n", results), false, null);
            }
        }

        [Command("quicktype"), Description("Waits for a response containing a generated code")]
        public async Task QuickType(CommandContext ctx, [Description("Bytes to generate. One byte equals two characters")] int bytes, [Description("Time before exiting")] TimeSpan time)
        {
            if (ConfigManager.get(ctx.Channel.Id, ConfigElement.Enabled).AND(ConfigManager.get(ctx.Channel.Id, ConfigElement.Quicktype)))
            {
                InteractivityModule interactivity = ctx.Client.GetInteractivityModule();
                byte[] codebytes = new byte[bytes];
                MainForm.Instance.rnd.NextBytes(codebytes);
                string code = BitConverter.ToString(codebytes).ToLower().Replace("-", "");
                await ctx.RespondAsync($"The first one to type the following code gets a reward: " + code.emotify());
                var msg = await interactivity.WaitForMessageAsync(xm => xm.Content.Contains(code), time);
                if (msg != null)
                {
                    await ctx.RespondAsync($"And the winner is: {msg.Message.Author.Mention}");
                }
                else
                {
                    await ctx.RespondAsync("Nobody? Really?");
                }
            }
        }

        [Command("emotify"), Description("Converts your text to emoticons")]
        public async Task Emotify(CommandContext ctx, [Description("What should be converted")] params string[] args)
        {
            if (ConfigManager.get(ctx.Channel.Id, ConfigElement.Enabled).AND(ConfigManager.get(ctx.Channel.Id, ConfigElement.Emojify)))
                await ctx.RespondAsync(string.Join(" ", args.Select(s => s.emotify())));
        }

        //Possibly create a news browser using this?
        //[Command("sendpaginated"), Description("Sends a paginated message.")]
        //public async Task SendPaginated(CommandContext ctx)
        //{
        //    var interactivity = ctx.Client.GetInteractivityModule();
        //    var lipsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris vitae velit eget nunc iaculis laoreet vitae eu risus. Nullam sit amet cursus purus. Duis enim elit, malesuada consequat aliquam sit amet, interdum vel orci. Donec vehicula ut lacus consequat cursus. Aliquam pellentesque eleifend lectus vitae sollicitudin. Vestibulum sit amet risus rhoncus, hendrerit felis eget, tincidunt odio. Nulla sed urna ante. Mauris consectetur accumsan purus, ac dignissim ligula condimentum eu. Phasellus ullamcorper, arcu sed scelerisque tristique, ante elit tincidunt sapien, eu laoreet ipsum mauris eu justo. Curabitur mattis cursus urna, eu ornare lacus pulvinar in. Vivamus cursus gravida nunc. Sed dolor nisi, congue non hendrerit at, rutrum sed mi. Duis est metus, consectetur sed libero quis, dignissim gravida lacus. Mauris suscipit diam dolor, semper placerat justo sodales vel. Curabitur sed fringilla odio.\n\nMorbi pretium placerat nulla sit amet condimentum. Duis placerat, felis ornare vehicula auctor, augue odio consectetur eros, sit amet tristique dolor risus nec leo. Aenean vulputate ipsum sagittis augue malesuada, id viverra odio gravida. Curabitur aliquet elementum feugiat. Phasellus eu faucibus nibh, eget finibus nibh. Proin ac fermentum enim, non consequat orci. Nam quis elit vulputate, mollis eros ut, maximus lacus. Vivamus et lobortis odio. Suspendisse potenti. Fusce nec magna in eros tempor tincidunt non vel mi. Pellentesque auctor eros tellus, vel ultrices mi ultricies eu. Nam pharetra sed tortor id elementum. Donec sit amet mi eleifend, iaculis purus sit amet, interdum turpis.\n\nAliquam at consectetur lectus. Ut et ultrices augue. Etiam feugiat, tortor nec dictum pharetra, nulla mauris convallis magna, quis auctor libero ipsum vitae mi. Mauris posuere feugiat feugiat. Phasellus molestie purus sit amet ipsum sodales, eget pretium lorem pharetra. Quisque in porttitor quam, nec hendrerit ligula. Fusce tempus, diam ut malesuada semper, leo tortor vulputate erat, non porttitor nisi elit eget turpis. Nam vitae arcu felis. Aliquam molestie neque orci, vel consectetur velit mattis vel. Fusce eget tempus leo. Morbi sit amet bibendum mauris. Aliquam erat volutpat. Phasellus nunc lectus, vulputate vitae turpis vel, tristique vulputate nulla. Aenean sit amet augue at mauris laoreet convallis. Nam quis finibus dui, at lobortis lectus.\n\nSuspendisse potenti. Pellentesque massa enim, dapibus at tortor eu, posuere ultricies augue. Nunc condimentum enim id ex sagittis, ut dignissim neque tempor. Nulla cursus interdum turpis. Aenean auctor tempor justo, sed rhoncus lorem sollicitudin quis. Fusce non quam a ante suscipit laoreet eget at ligula. Aenean condimentum consectetur nunc, sit amet facilisis eros lacinia sit amet. Integer quis urna finibus, tristique justo ut, pretium lectus. Proin consectetur enim sed risus rutrum, eu vehicula augue pretium. Vivamus ultricies justo enim, id imperdiet lectus molestie at. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas.\n\nNullam tincidunt dictum nibh, dignissim laoreet libero eleifend ut. Vestibulum eget maximus nulla. Suspendisse a auctor elit, ac facilisis tellus. Sed iaculis turpis ac purus tempor, ut pretium ante ultrices. Aenean commodo tempus vestibulum. Morbi vulputate pharetra molestie. Ut rhoncus quam felis, id mollis quam dapibus id. Curabitur faucibus id justo in ornare. Praesent facilisis dolor lorem, non vulputate velit finibus ut. Praesent vestibulum nunc ac nibh iaculis porttitor.\n\nFusce mattis leo sed ligula laoreet accumsan. Pellentesque tortor magna, ornare vitae tellus eget, mollis placerat est. Suspendisse potenti. Ut sit amet lacus sed nibh pulvinar mattis in bibendum dui. Mauris vitae turpis tempor, malesuada velit in, sodales lacus. Sed vehicula eros in magna condimentum vestibulum. Aenean semper finibus lectus, vel hendrerit lorem euismod a. Sed tempor ante quis magna sollicitudin, eu bibendum risus congue. Donec lectus sem, accumsan ut mollis et, accumsan sed lacus. Nam non dui non tellus pretium mattis. Mauris ultrices et felis ut imperdiet. Nam erat risus, consequat eu eros ac, convallis viverra sapien. Etiam maximus nunc et felis ultrices aliquam.\n\nUt tincidunt at magna at interdum. Sed fringilla in sem non lobortis. In dictum magna justo, nec lacinia eros porta at. Maecenas laoreet mattis vulputate. Sed efficitur tempor euismod. Integer volutpat a odio eu sagittis. Aliquam congue tristique nisi, quis aliquet nunc tristique vitae. Vivamus ac iaculis nunc, et faucibus diam. Donec vitae auctor ipsum, quis posuere est. Proin finibus, dolor ac euismod consequat, urna sem ultrices lectus, in iaculis sem nulla et odio. Integer et vulputate metus. Phasellus finibus et lorem eget lacinia. Maecenas velit est, luctus quis fermentum nec, fringilla eu lorem.\n\nPellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Mauris faucibus neque eu consectetur egestas. Mauris aliquet nibh pellentesque mollis facilisis. Duis egestas lectus sed justo sagittis ultrices. Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Curabitur hendrerit quis arcu id dictum. Praesent in massa eget lectus pulvinar consectetur. Aliquam eget ipsum et velit congue porta vitae ut eros. Quisque convallis lacus et venenatis sagittis. Phasellus sit amet eros ac nibh facilisis laoreet vel eget nisi. In ante libero, volutpat in risus vel, tristique blandit leo. Morbi posuere bibendum libero, non efficitur mi sagittis vel. Cras viverra pulvinar pellentesque. Mauris auctor et lacus ut pellentesque. Nunc pretium luctus nisi eu convallis.\n\nSed nec ultricies arcu. Aliquam eu tincidunt diam, nec luctus ligula. Ut laoreet dignissim est, eu fermentum massa fermentum eget. Nullam non viverra justo, sed congue felis. Phasellus id convallis mauris. Aliquam elementum euismod ex, vitae dignissim nunc consectetur vitae. Donec ut odio quis ex placerat elementum sit amet eget lectus. Suspendisse potenti. Nam non massa id mi suscipit euismod. Nullam varius tincidunt diam congue congue. Proin pharetra vestibulum eros, vel imperdiet sem rutrum at. Cras eget gravida ligula, quis facilisis ex.\n\nEtiam consectetur elit mauris, euismod porta urna auctor a. Nulla facilisi. Praesent massa ipsum, iaculis non odio at, varius lobortis nisi. Aliquam viverra erat a dapibus porta. Pellentesque imperdiet maximus mattis. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Donec luctus elit sit amet feugiat convallis. Phasellus varius, sem ut volutpat vestibulum, magna arcu porttitor libero, in dapibus metus dolor nec dolor. Fusce at eleifend magna. Mauris cursus pellentesque sagittis. Nullam nec laoreet ante, in sodales arcu.";
        //    var lipsum_pages = interactivity.GeneratePagesInEmbeds(lipsum);
        //    await interactivity.SendPaginatedMessage(ctx.Channel, ctx.User, lipsum_pages, TimeSpan.FromMinutes(5), TimeoutBehaviour.Delete);
        //}

        //[Command("play"), Description("Sends \"No.\" to the chat. For users used to RhythmBot.")]
        //public async Task Play(CommandContext ctx)
        //{
        //    if (ConfigManager.get(ctx.Channel.Id, ConfigElement.Enabled).AND(ConfigManager.get(ctx.Channel.Id, ConfigElement.Play)))
        //        await ctx.RespondAsync("No.");
        //}
    }
}
