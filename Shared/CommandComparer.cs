using System.Collections.Generic;
using DSharpPlus.CommandsNext;

namespace Shared
{
    public class CommandComparer : IEqualityComparer<Command>
    {
        public bool Equals(Command x, Command y) => ReferenceEquals(x, y) ||
                                                    (!ReferenceEquals(x, null) && !ReferenceEquals(y, null) &&
                                                     x.QualifiedName == y.QualifiedName);

        public int GetHashCode(Command product) => product.QualifiedName.GetHashCode();
    }
}