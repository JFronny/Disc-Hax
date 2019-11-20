using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Config
{
    public struct ChannelConfig
    {
        private bool chan;
        private bool play;
        private bool waifu;
        private bool booru;
        private bool nsfw;
        private bool config;
        private bool bees;
        private bool enabled;
        public Action Save;

        public ChannelConfig(Action save) : this() => Save = save ?? throw new ArgumentNullException(nameof(save));

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

        public bool Enabled
        {
            get {
                return enabled;
            }
            set {
                enabled = value;
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
                "\r\nConfig: " + Config.ToString() +
                "\r\nEnabled: " + Enabled.ToString();
            return msg;
        }
    }
}
