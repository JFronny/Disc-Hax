using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.VisualBasic;

namespace MDExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            string MD = @"# Disc-Hax
A powerful multi-purpose discord bot!
[![CodeFactor](https://www.codefactor.io/repository/github/jfronny/disc-hax/badge)](https://www.codefactor.io/repository/github/jfronny/disc-hax)
[![Discord](https://img.shields.io/discord/466965965658128384?label=Discord)](https://discord.gg/UjhHBqt)
[![GitHub](https://img.shields.io/badge/-GitHub-informational)](https://github.com/JFronny/Disc-Hax)

Features:";
            if (args.Length == 0)
            {
                Console.WriteLine("Output MD");
                Console.Write("> ");
                args = new[] { Console.ReadLine() };
            }
            
            Assembly bot = typeof(Bot.Program).Assembly;
            IEnumerable<Type> groups = bot.GetTypes().Where(HasAtt<GroupAttribute>).OrderBy(s => GetAtt<GroupAttribute>(s).Name);
            MD += $@"
- ~{Math.Round(groups.SelectMany(GetCommands).Count() * 0.1f) * 10} powerful commands (state: {DateTime.Now.ToString(CultureInfo.InvariantCulture.DateTimeFormat.LongDatePattern, CultureInfo.InvariantCulture.DateTimeFormat)})
";
            MD += string.Join(@"
", groups.Select(s =>
            {
                string ret = $@"  - {GetAtt<GroupAttribute>(s).Name}: {GetAtt<DescriptionAttribute>(s).Description}
";
                ret += string.Join(@"
", GetCommands(s).OrderBy(s => GetAtt<CommandAttribute>(s).Name).Select(s =>
                    $"    - {GetAtt<CommandAttribute>(s).Name}: {GetAtt<DescriptionAttribute>(s).Description}"));
                return ret;
            }));
            MD += @"
- Powerful configuration
  - Custom XML-based layered database
  - Toggle every single command
  - Custom prefixes
  - Locally encrypted API Keys
- Easy to develop
  - Experiments for many features
  - Decently structured solution
  - Lots of testing
  - Few external dependencies (you don't need PHP, SQL, Node.JS)
  - No payed API keys (eg Google Translate)
  - Modern library
  - Every group, command and parameter is documented in [help]
- Fast
  - My instance runs just fine on a RasPi with minimal specs";
            File.WriteAllText(args[0], MD);
        }

        private static IEnumerable<MethodInfo> GetCommands(Type s) => s.GetMethods()
            .Where(HasAtt<CommandAttribute>)
            .GroupBy(s => GetAtt<CommandAttribute>(s).Name).Select(s => s.First());

        private static T GetAtt<T>(MemberInfo info) where T : Attribute =>
            (T)info.GetCustomAttributes(true).First(s => s is T);

        private static bool HasAtt<T>(MemberInfo info) where T : Attribute =>
            info.GetCustomAttributes(true).Any(s => s is T);
    }
}