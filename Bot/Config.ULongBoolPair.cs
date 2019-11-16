using System;

namespace Bot
{
    static partial class Config
    {
        [Serializable]
        public class ULongBoolPair
        {
            public ulong Key;
            public bool Value;

            public ULongBoolPair(ulong key, bool value)
            {
                Key = key;
                Value = value;
            }

            public override bool Equals(object obj) => Key.Equals(obj);
            public override int GetHashCode() => Key.GetHashCode();
            public override string ToString() => Key.ToString() + ":" + Value.ToString();
        }
    }
}
