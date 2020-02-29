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
        private static Type[]? M;
        private static string[]? C;

        public static Type[] getM()
        {
            if (M == null)
                M = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(s => typeof(BaseCommandModule).IsAssignableFrom(s)).ToArray();
            return M;
        }

        public static string[] getC()
        {
            if (C == null)
                C = getM().SelectMany(s => s.GetMethods())
                    .Where(s => s.GetCustomAttributes(typeof(CommandAttribute), false).Length > 0)
                    .Select(s => CommandComparer.GetName(s))
                    .Except(new[] {"method_ping"})
                    .OrderBy(s => s)
                    .Union(new[] {ConfigManager.Nsfw, ConfigManager.Enabled}).ToArray();
            return C;
        }
    }
}