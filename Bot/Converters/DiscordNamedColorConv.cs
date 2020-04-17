using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace Bot.Converters
{
    public class DiscordNamedColorConverter : IArgumentConverter<DiscordColor>
    {
        private static Regex ColorRegexHex { get; }
        private static Regex ColorRegexRgb { get; }
        public static Dictionary<string, DiscordColor> ColorNames { get; }

        static DiscordNamedColorConverter()
        {
            ColorRegexHex = new Regex(@"^#?([a-fA-F0-9]{6})$", RegexOptions.ECMAScript | RegexOptions.Compiled);
            ColorRegexRgb = new Regex(@"^(\d{1,3})\s*?,\s*?(\d{1,3}),\s*?(\d{1,3})$", RegexOptions.ECMAScript | RegexOptions.Compiled);
            ColorNames = typeof(DiscordColor).GetProperties().Where(s => s.PropertyType == typeof(DiscordColor))
                .ToDictionary(s => s.Name.ToLower(), s => (DiscordColor)s.GetValue(null));
        }

        Task<Optional<DiscordColor>> IArgumentConverter<DiscordColor>.ConvertAsync(string value, CommandContext ctx)
        {
            Match m = ColorRegexHex.Match(value);
            if (m.Success && int.TryParse(m.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int clr))
                return Task.FromResult(Optional.FromValue<DiscordColor>(clr));

            m = ColorRegexRgb.Match(value);
            if (m.Success)
            {
                bool p1 = byte.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out byte r);
                bool p2 = byte.TryParse(m.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out byte g);
                bool p3 = byte.TryParse(m.Groups[3].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out byte b);

                if (!(p1 && p2 && p3))
                    return Task.FromResult(Optional.FromNoValue<DiscordColor>());
                
                return Task.FromResult(Optional.FromValue(new DiscordColor(r, g, b)));
            }
            if (ColorNames.ContainsKey(value.ToLower()))
                return Task.FromResult(Optional.FromValue(ColorNames[value.ToLower()]));
            return Task.FromResult(Optional.FromNoValue<DiscordColor>());
        }
    }
}