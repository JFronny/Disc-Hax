using System;

namespace Bot
{
    static partial class Config
    {
        [Serializable]
        public class Data
        {
            private IDMatrix checkedMatrix;
            private bool chan;
            private bool play;
            private bool waifu;
            private bool booru;
            private string key;

            public bool Chan
            {
                get {
                    return chan;
                }

                set {
                    chan = value;
                    Save();
                }
            }
            public bool Play
            {
                get {
                    return play;
                }

                set {
                    play = value;
                    Save();
                }
            }
            public bool Waifu
            {
                get {
                    return waifu;
                }

                set {
                    waifu = value;
                    Save();
                }
            }
            public bool Booru
            {
                get {
                    return booru;
                }

                set {
                    booru = value;
                    Save();
                }
            }

            public string Key
            {
                get {
                    return key;
                }

                set {
                    key = value;
                    Save();
                }
            }

            public IDMatrix CheckedMatrix
            {
                get {
                    if (checkedMatrix == null)
                        CheckedMatrix = new IDMatrix();
                    return checkedMatrix;
                }

                set {
                    checkedMatrix = value;
                    Save();
                }
            }

            public override string ToString() =>
                "Chan: " + Chan.ToString() +
                "\r\nPlay: " + Play.ToString() +
                "\r\rWaifu: " + Waifu.ToString() +
                "\r\nBooru: " + Booru.ToString() +
                "\r\nKey: " + Key +
                "\r\nCheckedMatrix: " + CheckedMatrix.ToString();
        }
    }
}
