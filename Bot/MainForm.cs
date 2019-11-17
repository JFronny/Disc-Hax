using Bot.Properties;
using Chan.Net;
using Chan.Net.JsonModel;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bot
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        public static MainForm Instance;
        public Random rnd = new Random();
        Dictionary<BotChannel, List<string>> messageSave = new Dictionary<BotChannel, List<string>>();
        private Task BotThread { get; set; }
        private Bot Bot { get; set; }
        private CancellationTokenSource TokenSource { get; set; }
        private BotGuild SelectedGuild { get; set; }
        private BotChannel SelectedChannel { get; set; }
        bool ChannelDefined = false;
        public MainForm()
        {
            InitializeComponent();
            Bot = new Bot(Program.cfg);
            Bot.Client.Ready += Bot_Ready;
            Bot.Client.GuildAvailable += Bot_GuildAvailable;
            Bot.Client.GuildCreated += Bot_GuildCreated;
            Bot.Client.GuildUnavailable += Bot_GuildUnavailable;
            Bot.Client.GuildDeleted += Bot_GuildDeleted;
            Bot.Client.MessageCreated += Bot_MessageCreated;
            Bot.Client.ClientErrored += Bot_ClientErrored;
            TokenSource = new CancellationTokenSource();
            BotThread = Task.Run(BotThreadCallback);
            channelTree.Enabled = true;
            chatBox.Enabled = true;
            chatSend.Enabled = true;
            Instance = this;
            booruBox.Checked = Config.data.Booru;
            chanBox.Checked = Config.data.Chan;
            playBox.Checked = Config.data.Play;
            waifuBox.Checked = Config.data.Waifu;
            nsfwBox.Checked = Config.data.Nsfw;
            configBox.Checked = Config.data.Config;
            beemovieBox.Checked = Config.data.Bees;
        }

        private async Task BotThreadCallback()
        {
            await Bot.StartAsync().ConfigureAwait(false);
            try
            {
                await Task.Delay(-1, TokenSource.Token).ConfigureAwait(false);
            }
            catch { }
            await Bot.StopAsync().ConfigureAwait(false);
            this.SetProperty(x => x.Text, "DiscHax Bot Menu");
            channelTree.InvokeAction(new Action(channelTree.Nodes.Clear));
            SelectedGuild = default;
            SelectedChannel = default;
            Bot = null;
            TokenSource = null;
            BotThread = null;
        }

        private Task BotSendMessageCallback(string text, BotChannel chn) => chn.Channel.SendMessageAsync(text);
        private Task BotSendMessageCallback(Uri image, string imageTitle, BotChannel chn) => chn.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder { Title = imageTitle, ImageUrl = image.AbsoluteUri });

        private Task Bot_Ready(ReadyEventArgs e)
        {
            this.SetProperty(xf => xf.Text, "DiscHax Bot Menu (connected)");
            return Task.CompletedTask;
        }

        private Task Bot_GuildAvailable(GuildCreateEventArgs e)
        {
            channelTree.InvokeAction(new Action<BotGuild>(AddGuild), new BotGuild(e.Guild));
            return Task.CompletedTask;
        }

        private Task Bot_GuildCreated(GuildCreateEventArgs e)
        {
            channelTree.InvokeAction(new Action<BotGuild>(AddGuild), new BotGuild(e.Guild));
            return Task.CompletedTask;
        }

        private Task Bot_GuildUnavailable(GuildDeleteEventArgs e)
        {
            channelTree.InvokeAction(new Action<ulong>(RemoveGuild), e.Guild.Id);
            return Task.CompletedTask;
        }

        private Task Bot_GuildDeleted(GuildDeleteEventArgs e)
        {
            channelTree.InvokeAction(new Action<ulong>(RemoveGuild), e.Guild.Id);
            return Task.CompletedTask;
        }

        private Task Bot_MessageCreated(MessageCreateEventArgs e)
        {
            channelTree.InvokeAction(new Action<BotMessage, BotChannel>(AddMessage), new BotMessage(e.Message), new BotChannel(e.Channel));
            return Task.CompletedTask;
        }

        private Task Bot_ClientErrored(ClientErrorEventArgs e)
        {
            this.InvokeAction(new Action(() => MessageBox.Show(this, $"Exception in {e.EventName}: {e.Exception.ToString()}", "Unhandled exception in the bot", MessageBoxButtons.OK, MessageBoxIcon.Warning)));
            return Task.CompletedTask;
        }

        private void AddGuild(BotGuild gld)
        {
            TreeNode node = new TreeNode
            {
                Text = gld.Guild.Name,
                Tag = gld,
                Checked = Config.data.CheckedMatrix[gld.Id]
            };
            IEnumerable<BotChannel> chns = gld.Guild.Channels.Where(xc => xc.Type == ChannelType.Text).OrderBy(xc => xc.Position).Select(xc => new BotChannel(xc));
            chns.ToList().ForEach(s =>
            {
                node.Nodes.Add(new TreeNode { Text = s.Channel.Name, Tag = s, Checked = Config.data.CheckedMatrix[s.Id] });
            });
            channelTree.TopNode.Nodes.Add(node);
            channelTree.Sort();
            channelTree.TopNode.ExpandAll();
        }

        private void RemoveGuild(ulong id)
        {
            channelTree.TopNode.Nodes.OfType<TreeNode>().Where(s => ((BotGuild)s.Tag).Id == id).ToList().ForEach(s => channelTree.Nodes.Remove(s));
        }

        private void AddMessage(BotMessage msg, BotChannel channel)
        {
            try
            {
                string logMsg = "";
                if (msg.Message.Author.IsCurrent)
                    logMsg = "<SELF>" + msg.Message.Content;
                else if (msg.Message.Author.IsBot)
                    logMsg = "<BOT>[" + msg.Message.Author.Username + "]" + msg.Message.Content;
                else
                    logMsg = "<USER>[" + msg.Message.Author.Username + "]" + msg.Message.Content;
                if (!messageSave.ContainsKey(channel))
                    messageSave.Add(channel, new List<string>());
                messageSave[channel].Add(logMsg);
                if ((!ChannelDefined) || channel.Id == SelectedChannel.Id)
                {
                    chatBox.Items.Add(logMsg);
                    chatBox.SelectedItem = logMsg;
                }
            }
            catch (Exception e)
            {
                SendMessage("Failed: " + e.Message, channel);
            }
        }

        private void Form_FormClosed(object sender, FormClosedEventArgs e) => TokenSource.Cancel();

        private void channelTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null && e.Node.Tag != null && e.Node.Parent != null && e.Node.Parent.Tag != null)
            {
                try
                {
                    SelectedGuild = (BotGuild)e.Node.Parent.Tag;
                    SelectedChannel = (BotChannel)e.Node.Tag;
                    ChannelDefined = true;
                    chatBox.Items.Clear();
                    if (!messageSave.ContainsKey(SelectedChannel))
                        messageSave.Add(SelectedChannel, new List<string>());
                    messageSave[SelectedChannel].ForEach(s => chatBox.Items.Add(s));
                }
                catch (InvalidCastException e1)
                {
                    Console.WriteLine(e1.ToString());
                }
            }
        }

        private void chatSend_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                SendMessage(chatSend.Text, SelectedChannel, t => chatSend.SetProperty(x => x.Text, ""));
            }
        }

        bool busy = false;
        private void channelTree_AfterCheck(object sender, TreeViewEventArgs e)
        {
            try
            {
                if (!busy)
                {
                    busy = true;
                    checkNodes(e.Node, e.Node.Checked);
                }
            }
            finally
            {
                busy = false;
                if (e.Node.Tag != null)
                {
                    Config.data.CheckedMatrix[((IBotStruct)e.Node.Tag).Id] = e.Node.Checked;
                }
            }
        }

        private void checkNodes(TreeNode node, bool check)
        {
            foreach (TreeNode child in node.Nodes)
            {
                child.Checked = check;
                checkNodes(child, check);
            }
        }

        private void chanBox_CheckedChanged(object sender, EventArgs e) => Config.data.Chan = chanBox.Checked;

        private void playBox_CheckedChanged(object sender, EventArgs e) => Config.data.Play = playBox.Checked;

        private void waifuBox_CheckedChanged(object sender, EventArgs e) => Config.data.Waifu = waifuBox.Checked;

        private void booruBox_CheckedChanged(object sender, EventArgs e) => Config.data.Booru = booruBox.Checked;

        private void nsfwBox_CheckedChanged(object sender, EventArgs e) => Config.data.Nsfw = nsfwBox.Checked;

        private void configBox_CheckedChanged(object sender, EventArgs e) => Config.data.Config = configBox.Checked;

        private void beemovieBox_CheckedChanged(object sender, EventArgs e) => Config.data.Bees = beemovieBox.Checked;

        private void debugButton_Click(object sender, EventArgs e) => new System.Threading.Thread(() => MessageBox.Show(Config.data.ToString(false))).Start();

        void SendMessage(string message, BotChannel channel, Action<Task> continuationAction = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;
            _ = continuationAction == null
                ? Task.Run(() => BotSendMessageCallback(message, channel))
                : Task.Run(() => BotSendMessageCallback(message, channel)).ContinueWith(continuationAction);
        }

        void SendMessage(Uri image, string imageTitle, BotChannel channel, Action<Task> continuationAction = null)
        {
            _ = continuationAction == null
                ? Task.Run(() => BotSendMessageCallback(image, imageTitle, channel))
                : Task.Run(() => BotSendMessageCallback(image, imageTitle, channel)).ContinueWith(continuationAction);
        }

        private void chanButton_Click(object sender, EventArgs e)
        {
            if (ChannelDefined)
                _ = Commands.Chan(new string[] { Interaction.InputBox("Please select a channel") }, SelectedChannel.Channel, (c1, c2, c3) => SelectedChannel.Channel.SendMessageAsync(c1, c2, c3));
        }

        private void playButton_Click(object sender, EventArgs e)
        {
            if (ChannelDefined)
                _ = Commands.Play((c1, c2, c3) => SelectedChannel.Channel.SendMessageAsync(c1, c2, c3));
        }

        private void waifuButton_Click(object sender, EventArgs e)
        {
            if (ChannelDefined)
                _ = Commands.Waifu(new string[] { "f" }, SelectedChannel.Channel, (c1, c2, c3) => SelectedChannel.Channel.SendMessageAsync(c1, c2, c3));
        }

        private void booruButton_Click(object sender, EventArgs e)
        {
            if (ChannelDefined)
                _ = Commands.Booru(new string[] { Interaction.InputBox("Please select categories") }, SelectedChannel.Channel, (c1, c2, c3) => SelectedChannel.Channel.SendMessageAsync(c1, c2, c3));
        }

        private void configButton_Click(object sender, EventArgs e)
        {
            if (ChannelDefined)
                _ = Commands.ConfigCmd((c1, c2, c3) => SelectedChannel.Channel.SendMessageAsync(c1, c2, c3));
        }

        private void pingButton_Click(object sender, EventArgs e)
        {
            if (ChannelDefined)
                _ = Commands.Ping((c1, c2, c3) => SelectedChannel.Channel.SendMessageAsync(c1, c2, c3));
        }

        private void beemovieButton_Click(object sender, EventArgs e)
        {
            if (ChannelDefined)
                _ = Commands.Bees((c1, c2, c3) => SelectedChannel.Channel.SendMessageAsync(c1, c2, c3));
        }
    }
}
