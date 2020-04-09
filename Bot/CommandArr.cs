using System;
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
        private static Type[]? _m;
        private static string[]? _c;

        public static Type[] GetM()
        {
            if (_m == null)
                _m = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(s => typeof(BaseCommandModule).IsAssignableFrom(s)).ToArray();
            return _m;
        }

        public static string[] GetC()
        {
            if (_c == null)
                _c = GetM().SelectMany(s => s.GetMethods())
                    .Where(s => s.GetCustomAttributes(typeof(CommandAttribute), false).Length > 0)
                    .Select(s => CommandComparer.GetName(s))
                    .Except(new[] {"method_ping"})
                    .OrderBy(s => s)
                    .Union(new[] {ConfigManager.Nsfw, ConfigManager.Enabled}).ToArray();
            return _c;
        }
    }
}