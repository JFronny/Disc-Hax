using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Bot
{
    static partial class Config
    {
        [Serializable]
        public class IDMatrix : IEnumerable<ULongBoolPair>
        {
            List<ULongBoolPair> values = new List<ULongBoolPair>();
            void Remove(ulong ID) => values = values.Where(s => s.Key != ID).ToList();
            void Add(ulong ID, bool field)
            {
                values.Add(new ULongBoolPair(ID, field));
                Console.WriteLine(ID);
            }

            public IEnumerator<ULongBoolPair> GetEnumerator() => values.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public override string ToString() => "{" + string.Join("", values.Select(s => "\r\n- " + s.ToString()).ToArray()) + "\r\n}";

            public bool this[ulong ID]
            {
                get => this[ID, false];
                set => this[ID, value] = value;
            }
            public bool this[ulong ID, bool defalutval]
            {
                get {
                    if (values.Where(s => s.Key == ID).Count() == 0)
                        Add(ID, defalutval);
                    return values.First(s => s.Key == ID).Value;
                }
                set {
                    Remove(ID);
                    Add(ID, value);
                }
            }
        }
    }
}
