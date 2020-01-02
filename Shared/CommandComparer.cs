using System.Collections.Generic;
using System.Reflection;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Shared
{
    public class CommandComparer : IEqualityComparer<Command>, IEqualityComparer<MethodInfo>
    {
        public bool Equals(Command x, Command y) => ReferenceEquals(x, y) ||
                                                    (!ReferenceEquals(x, null) && !ReferenceEquals(y, null) &&
                                                     x.QualifiedName == y.QualifiedName);

        public int GetHashCode(Command product) => product.QualifiedName.GetHashCode();

        public bool Equals(MethodInfo x, MethodInfo y) => GetName(x).Equals(GetName(y));

        public int GetHashCode(MethodInfo obj) => GetName(obj).GetHashCode();

        public static string GetName(MethodBase obj) =>
            $"method_{((CommandAttribute) obj.GetCustomAttributes(typeof(CommandAttribute), false)[0]).Name}".ToLower()
                .Replace(" ", "");
    }
}