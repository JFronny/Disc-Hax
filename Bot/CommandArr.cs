using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Shared;
using Shared.Config;

namespace Bot
{
    public static class CommandArr
    {
        private static IEnumerable<Tuple<Type, string>>? _groups;
        private static IEnumerable<string>? _commandNames;

        private static IEnumerable<Tuple<Type, string>> GetGroups() => _groups ??= Assembly.GetExecutingAssembly()
            .GetTypes().Where(s => typeof(BaseCommandModule).IsAssignableFrom(s)).Select(s => new Tuple<Type, string>(s,
                $"group_{s.GetCustomAttributes<GroupAttribute>().First().Name}"));

        public static IEnumerable<string> GetGroupNames() => GetGroups().Select(s => s.Item2);

        public static IEnumerable<string> GetCommandNames(string group) => GetCommandNames(
            _groups.First(s => string.Equals(s.Item2, group, StringComparison.CurrentCultureIgnoreCase)).Item1);

        private static IEnumerable<string> GetCommandNames(Type group) =>
            group.GetMethods().Where(s => s.GetCustomAttributes(typeof(CommandAttribute), false).Length > 0)
                .Select(CommandComparer.GetName)
                .Except(new[] {"method_ping", "method_config"})
                .OrderBy(s => s)
                .Union(new[] {ConfigManager.Nsfw, ConfigManager.Enabled});

        public static string[] GetCommandNames() =>
            (_commandNames ??= GetGroups().Select(s => s.Item1).SelectMany(GetCommandNames)).ToArray();
    }
}