using System;

namespace Bot
{
    static partial class Config
    {
        [Serializable]
        public class ULongBoolPair
        {
            private ulong key;
            private bool value;

            public ulong Key
            {
                get => key;
                set {
                    key = value;
                    Save();
                }
            }
            public bool Value
            {
                get => value;
                set {
                    this.value = value;
                    Save();
                }
            }

            public ULongBoolPair(ulong key, bool value)
            {
                Key = key;
                Value = value;
                Save();
            }

            public override bool Equals(object obj) => Key.Equals(obj);
            public override int GetHashCode() => Key.GetHashCode();
            public override string ToString() => Key.ToString() + ":" + Value.ToString();
        }
    }
}
