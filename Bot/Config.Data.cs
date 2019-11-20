﻿using System;
using System.Collections.Generic;

namespace Bot
{
    static partial class Config
    {
        [Serializable]
        public class Data
        {
            private Dictionary<ulong, bool> checkedMatrix;
            private bool chan;
            private bool play;
            private bool waifu;
            private bool booru;
            private string key;
            private bool nsfw;
            private bool config;
            private bool bees;

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

            public Dictionary<ulong, bool> CheckedMatrix
            {
                get {
                    if (checkedMatrix == null)
                        CheckedMatrix = new Dictionary<ulong, bool>();
                    return checkedMatrix;
                }

                set {
                    checkedMatrix = value;
                    Save();
                }
            }

            public bool Nsfw
            {
                get {
                    return nsfw;
                }
                set {
                    nsfw = value;
                    Save();
                }
            }

            public bool Config
            {
                get {
                    return config;
                }
                set {
                    config = value;
                    Save();
                }
            }

            public bool Bees
            {
                get {
                    return bees;
                }
                set {
                    bees = value;
                    Save();
                }
            }

            public override string ToString() => ToString(true);

            public string ToString(bool hideSensitiveInfo)
            {
                string msg = "Chan: " + Chan.ToString() +
                    "\r\nPlay: " + Play.ToString() +
                    "\r\nWaifu: " + Waifu.ToString() +
                    "\r\nBooru: " + Booru.ToString() +
                    "\r\nNSFW: " + Nsfw.ToString() +
                    "\r\nBees: " + Bees.ToString() +
                    "\r\nConfig: " + Config.ToString();
                if (!hideSensitiveInfo)
                    msg += "\r\nKey: " + new string('*', Key.Length - 4) + Key.Substring(Key.Length - 5, 4) +
                    "\r\nCheckedMatrix: " + CheckedMatrix.ToString();
                return msg;
            }
        }
    }
}
