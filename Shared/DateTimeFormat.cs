using System;
using System.Collections.Generic;
using System.Linq;

namespace Shared
{
    public static class DateTimeFormat
    {
        public static string GetReadable(this TimeSpan ts)
        {
            // formats and its cutoffs based on totalseconds
            SortedList<long, string> cutoff = new SortedList<long, string>
            {
                {59, "{3:S}"},
                {60, "{2:M}"},
                {60 * 60 - 1, "{2:M}, {3:S}"},
                {60 * 60, "{1:H}"},
                {24 * 60 * 60 - 1, "{1:H}, {2:M}"},
                {24 * 60 * 60, "{0:D}"},
                {long.MaxValue, "{0:D}, {1:H}"}
            };

            // find nearest best match
            int find = cutoff.Keys.ToList()
                .BinarySearch((long) ts.TotalSeconds);
            // negative values indicate a nearest match
            int near = find < 0 ? Math.Abs(find) - 1 : find;
            // use custom formatter to get the string
            return string.Format(
                new HmsFormatter(),
                cutoff[cutoff.Keys[near]],
                ts.Days,
                ts.Hours,
                ts.Minutes,
                ts.Seconds);
        }

        private class HmsFormatter : ICustomFormatter, IFormatProvider
        {
            private static readonly Dictionary<string, string> TimeFormats = new Dictionary<string, string>
            {
                {"S", "{0:P:Seconds:Second}"},
                {"M", "{0:P:Minutes:Minute}"},
                {"H", "{0:P:Hours:Hour}"},
                {"D", "{0:P:Days:Day}"}
            };

            public string Format(string? format, object? arg, IFormatProvider? formatProvider) =>
                string.Format(new PluralFormatter(), TimeFormats[format], arg);

            public object GetFormat(Type? formatType) => formatType == typeof(ICustomFormatter) ? this : null;
        }

        private class PluralFormatter : ICustomFormatter, IFormatProvider
        {
            public string Format(string? format, object? arg, IFormatProvider? formatProvider)
            {
                if (arg == null) return string.Format(format, arg);
                string[] parts = format.Split(':');
                if (parts[0] != "P") return string.Format(format, arg);
                int partIndex = arg.ToString() == "1" ? 2 : 1;
                return $"{arg} {(parts.Length > partIndex ? parts[partIndex] : "")}";
            }

            public object GetFormat(Type? formatType) => formatType == typeof(ICustomFormatter) ? this : null;
        }
    }
}