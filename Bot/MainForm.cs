using Bot.Config;
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
using System.Xml.Linq;

namespace Bot
{
    public partial class MainForm : Form
    {
        public static MainForm Instance;
        public Random rnd = new Random();
        public Dictionary<BotChannel, List<string>> messageSave = new Dictionary<BotChannel, List<string>>();
        public Task BotThread { get; set; }
        public Bot Bot { get; set; }
        public CancellationTokenSource TokenSource { get; set; }
        public BotGuild SelectedGuild { get; set; }
        public BotChannel SelectedChannel { get; set; }
        public ulong ChID => ChannelDefined ? SelectedChannel.Id : 0;
        public bool ChannelDefined = false;
        public MainForm()
        {
            InitializeComponent();
            channelTree.Nodes[0].Checked = Common.guildsBox;
            Bot = new Bot(new DiscordConfiguration { Token = TokenContainer.Token, TokenType = TokenType.Bot, AutoReconnect = true, LogLevel = LogLevel.Debug, UseInternalLogHandler = false });
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
            Console.WriteLine("Your invite Link:\r\n    https://discordapp.com/oauth2/authorize?client_id=" + e.Client.CurrentApplication.Id.ToString() + "&scope=bot&permissions=8");
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
                Checked = ChCfgMgr.getGl(gld.Id)
            };
            IEnumerable<BotChannel> chns = gld.Guild.Channels.Where(xc => xc.Type == ChannelType.Text).OrderBy(xc => xc.Position).Select(xc => new BotChannel(xc));
            chns.ToList().ForEach(s =>
            {
                node.Nodes.Add(new TreeNode { Text = s.Channel.Name, Tag = s, Checked = ChCfgMgr.getCh(s.Id, ConfigElement.Enabled) });
            });
            channelTree.TopNode.Nodes.Add(node);
            node.Expand();
            node.ExpandAll();
            channelTree.Nodes[0].Expand();
            channelTree.Sort();
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
                if (messageSave[channel].Count > 100)
                    messageSave[channel].RemoveRange(99, messageSave[channel].Count - 100);
                messageSave[channel].Add(logMsg);
                if (ChannelDefined && channel.Id == SelectedChannel.Id)
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
        bool recCheck = true;
        public void channelTree_AfterSelect(object sender, TreeViewEventArgs e)
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
                    booruBox.Checked = ChCfgMgr.getCh(SelectedChannel.Id, ConfigElement.Booru);
                    chanBox.Checked = ChCfgMgr.getCh(SelectedChannel.Id, ConfigElement.Chan);
                    playBox.Checked = ChCfgMgr.getCh(SelectedChannel.Id, ConfigElement.Play);
                    waifuBox.Checked = ChCfgMgr.getCh(SelectedChannel.Id, ConfigElement.Waifu);
                    nsfwBox.Checked = SelectedChannel.Channel.IsNSFW || ChCfgMgr.getCh(SelectedChannel.Id, ConfigElement.Nsfw);
                    configBox.Checked = ChCfgMgr.getCh(SelectedChannel.Id, ConfigElement.Config);
                    beemovieBox.Checked = ChCfgMgr.getCh(SelectedChannel.Id, ConfigElement.Bees);
                    pollBox.Checked = ChCfgMgr.getCh(SelectedChannel.Id, ConfigElement.Poll);
                    nsfwBox.Enabled = !SelectedChannel.Channel.IsNSFW;
                    settingsPanel.Text = SelectedGuild.Guild.Name + " - " + SelectedChannel.Channel.Name;
                    settingsPanel.Enabled = true;
                }
                catch (InvalidCastException e1)
                {
                    Console.WriteLine(e1.ToString());
                }
            }
            else
            {
                ChannelDefined = false;
                settingsPanel.Enabled = false;
                settingsPanel.Text = "Channel";
            }
        }

        public void updateChecking()
        {
            try
            {
                recCheck = false;
                IEnumerable<TreeNode> nodes = channelTree.Nodes.OfType<TreeNode>();
                nodes.First().Checked = Common.guildsBox;
                nodes = nodes.First().Nodes.OfType<TreeNode>();
                nodes = nodes.Concat(nodes.SelectMany(s => s.Nodes.OfType<TreeNode>()));
                nodes.ToList().ForEach(s => s.Checked = s.Tag.tryCast(out BotChannel c) ? ChCfgMgr.getCh(c.Id, ConfigElement.Enabled) : s.Tag.tryCast(out BotGuild g) ? ChCfgMgr.getGl(g.Id) : s.Checked);
            }
            finally
            {
                recCheck = true;
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
            if (recCheck)
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
                    if (e.Node.Parent != null)
                    {
                        if (e.Node.Tag.tryCast(out BotChannel c))
                        {
                            ChCfgMgr.setCh(c.Id, ConfigElement.Enabled, e.Node.Checked);
                        }
                        else if (e.Node.Tag.tryCast(out BotGuild g))
                        {
                            ChCfgMgr.setGl(g.Id, e.Node.Checked);
                        }
                    }
                    else
                        Common.guildsBox = e.Node.Checked;
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

        private void chanBox_CheckedChanged(object sender, EventArgs e) => ChCfgMgr.setCh(ChID, ConfigElement.Chan, chanBox.Checked, true);

        private void playBox_CheckedChanged(object sender, EventArgs e) => ChCfgMgr.setCh(ChID, ConfigElement.Play, playBox.Checked, true);

        private void waifuBox_CheckedChanged(object sender, EventArgs e) => ChCfgMgr.setCh(ChID, ConfigElement.Waifu, waifuBox.Checked, true);

        private void booruBox_CheckedChanged(object sender, EventArgs e) => ChCfgMgr.setCh(ChID, ConfigElement.Booru, booruBox.Checked, true);

        private void nsfwBox_CheckedChanged(object sender, EventArgs e) => ChCfgMgr.setCh(ChID, ConfigElement.Nsfw, nsfwBox.Checked, true);

        private void configBox_CheckedChanged(object sender, EventArgs e) => ChCfgMgr.setCh(ChID, ConfigElement.Config, configBox.Checked, true);

        private void beemovieBox_CheckedChanged(object sender, EventArgs e) => ChCfgMgr.setCh(ChID, ConfigElement.Bees, beemovieBox.Checked, true);

        private void pollBox_CheckedChanged(object sender, EventArgs e) => ChCfgMgr.setCh(ChID, ConfigElement.Poll, pollBox.Checked, true);

        private void debugButton_Click(object sender, EventArgs e)
        {
            if (ChannelDefined)
                new System.Threading.Thread(() => MessageBox.Show(ChCfgMgr.getChStr(SelectedChannel.Id))).Start();
        }

        void SendMessage(string message, BotChannel channel, Action<Task> continuationAction = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;
            _ = continuationAction == null
                ? Task.Run(() => BotSendMessageCallback(message, channel))
                : Task.Run(() => BotSendMessageCallback(message, channel)).ContinueWith(continuationAction);
        }

        private void chanButton_Click(object sender, EventArgs e)
        {
            if (ChannelDefined)
                _ = Commands.Chan(new string[] { Interaction.InputBox("Please select a channel") }, SelectedChannel.Channel, (c1, c2, c3) => SelectedChannel.Channel.SendMessageAsync(c1, c2, c3));
        }

        private void playButton_Click(object sender, EventArgs e)
        {
            if (ChannelDefined)
                _ = Commands.Play(SelectedChannel.Channel, (c1, c2, c3) => SelectedChannel.Channel.SendMessageAsync(c1, c2, c3));
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
                _ = Commands.ConfigCmd(Interaction.InputBox("Please select parameters").Split(' '), SelectedChannel.Channel, (c1, c2, c3) => SelectedChannel.Channel.SendMessageAsync(c1, c2, c3));
        }

        private void pingButton_Click(object sender, EventArgs e)
        {
            if (ChannelDefined)
                _ = Commands.Ping(SelectedChannel.Channel, (c1, c2, c3) => SelectedChannel.Channel.SendMessageAsync(c1, c2, c3));
        }

        private void beemovieButton_Click(object sender, EventArgs e)
        {
            if (ChannelDefined)
                _ = Commands.Bees(SelectedChannel.Channel, (c1, c2, c3) => SelectedChannel.Channel.SendMessageAsync(c1, c2, c3));
        }

        private void pollButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (ChannelDefined)
                    _ = Commands.Poll(SelectedChannel.Channel, (c1, c2, c3) => SelectedChannel.Channel.SendMessageAsync(c1, c2, c3), new TimeSpan(10000 * long.Parse(Interaction.InputBox("Milliseconds"))),
                        Interaction.InputBox("Emoticons").Split(' ').Select(s => DiscordEmoji.FromName(Bot.Client, s)).ToArray());
            }
            catch (Exception e1)
            {
                MessageBox.Show("Failed: " + e1.Message);
            }
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to reset everything?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Directory.Delete(Path.GetDirectoryName(Application.ExecutablePath) + @"\Cfgs");
                Close();
            }
        }

        private void channelTree_AfterCollapse(object sender, TreeViewEventArgs e) => channelTree.Nodes[0].Expand();

        private void cleanButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete unused configs?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                string[] cfgs = Directory.GetFiles(Path.GetDirectoryName(Application.ExecutablePath) + @"\Cfgs");
                string[] guilds = channelTree.Nodes[0].Nodes
                    .OfType<TreeNode>().Select(s => ((IBotStruct)s.Tag).Id.ToString()).ToArray();
                string[] channels = channelTree.Nodes[0].Nodes.OfType<TreeNode>().SelectMany(s => s.Nodes.OfType<TreeNode>())
                    .OfType<TreeNode>().Select(s => ((IBotStruct)s.Tag).Id.ToString()).ToArray();
                string[] allowedNames = new string[] { "common.xml", "key.secure" };
                for (int i = 0; i < cfgs.Length; i++)
                {
                    XDocument el;
                    try
                    {
                        el = XDocument.Load(cfgs[i]);
                    }
                    catch
                    {
                        el = new XDocument();
                    }
                    string file = Path.GetFileName(cfgs[i]);
                    string id = Path.GetFileNameWithoutExtension(file);
                    if (el.Element("guild") != null)
                    {
                        if (!guilds.Contains(id))
                            File.Delete(cfgs[i]);
                    }
                    else if (el.Element("channel") != null)
                    {
                        if (!channels.Contains(id))
                            File.Delete(cfgs[i]);
                    }
                    else
                    {
                        if (!allowedNames.Contains(file))
                            File.Delete(cfgs[i]);
                    }
                }
            }
        }
    }
}
