using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bot
{
    public partial class Form : System.Windows.Forms.Form
    {
        Dictionary<BotChannel, List<string>> messageSave = new Dictionary<BotChannel, List<string>>();
        private Task BotThread { get; set; }
        private Bot Bot { get; set; }
        private CancellationTokenSource TokenSource { get; set; }
        private BotGuild SelectedGuild { get; set; }
        private BotChannel SelectedChannel { get; set; }
        bool ChannelDefined = false;
        public Form()
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
                Tag = gld
            };
            IEnumerable<BotChannel> chns = gld.Guild.Channels.Where(xc => xc.Type == ChannelType.Text).OrderBy(xc => xc.Position).Select(xc => new BotChannel(xc));
            chns.ToList().ForEach(s =>
            {
                node.Nodes.Add(new TreeNode { Text = s.Channel.Name, Tag = s, Checked = false });
            });
            channelTree.TopNode.Nodes.Add(node);
            channelTree.TopNode.ExpandAll();
        }

        private void RemoveGuild(ulong id)
        {
            channelTree.TopNode.Nodes.OfType<TreeNode>().Where(s => ((BotGuild)s.Tag).Id == id).ToList().ForEach(s => channelTree.Nodes.Remove(s));
        }

        private void AddMessage(BotMessage msg, BotChannel channel)
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
            switch (msg.Message.Content.Split(' ')[0])
            {
                case "!play":
                    SendMessage("No.", channel);
                    break;
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

        void SendMessage(string message, BotChannel channel, Action<Task> continuationAction = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;
            _ = continuationAction == null
                ? Task.Run(() => BotSendMessageCallback(message, channel))
                : Task.Run(() => BotSendMessageCallback(message, channel)).ContinueWith(continuationAction);
        }

        bool busy = false;
        private void channelTree_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (busy) return;
            busy = true;
            try
            {
                checkNodes(e.Node, e.Node.Checked);
            }
            finally
            {
                busy = false;
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
    }
}
