using Bot.Commands;
using DSharpPlus;
using Shared;
using Shared.Config;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Bot
{
    public partial class MainForm : Form
    {
        public Dictionary<BotChannel, List<string>> messageSave = new Dictionary<BotChannel, List<string>>();
        public Task BotThread { get; set; }
        public Bot Bot { get; set; }
        public CancellationTokenSource TokenSource { get; set; }
        public BotGuild SelectedGuild { get; set; }
        public BotChannel SelectedChannel { get; set; }
        public ulong ChID => ChannelDefined ? SelectedChannel.Id : 0;
        public bool ChannelDefined = false;
        private List<CheckBox> settingsBoxes;

        public MainForm()
        {
            InitializeComponent();
            ConfigManager.configUpdate += (sender, ID, element) =>
            {
                if (ChannelDefined && SelectedChannel.Id == ID)
                    Invoke((MethodInvoker)delegate ()
                    {
                        channelTree_AfterSelect(null, new TreeViewEventArgs(
                            channelTree.Nodes[0].Nodes.OfType<TreeNode>()
                            .SelectMany(s => ((TreeNode)s).Nodes.OfType<TreeNode>())
                            .First(s => ((BotChannel)s.Tag).Id == ID)));
                    });
                if (element == ConfigElement.Enabled)
                    this.InvokeAction((MethodInvoker)delegate () { updateChecking(); });
            };
            settingsBoxes = new List<CheckBox>();
            string[] array = Enum.GetNames(typeof(ConfigElement)).Where(s => new List<string> { ConfigElement.Enabled.ToString(), ConfigElement.Nsfw.ToString() }.IndexOf(s) == -1).ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                CheckBox box = new CheckBox();
                box.Tag = ClassExtensions.ParseToEnum<ConfigElement>(array[i]);
                box.Text = array[i];
                box.CheckStateChanged += (sender, e) =>
                {
                    ClassExtensions.ExIf(ChannelDefined, () => ConfigManager.set(SelectedChannel.Channel, (ConfigElement)box.Tag, box.Checked, true));
                };
                clientCheckGrid.Controls.Add(box);
                settingsBoxes.Add(box);
            }
            channelTree.Nodes[0].Checked = Common.guildsBox;
            channelTree.Enabled = true;
            chatBox.Enabled = true;
            chatSend.Enabled = true;
        }

        public void AddGuild(BotGuild gld)
        {
            TreeNode node = new TreeNode
            {
                Text = gld.Guild.Name,
                Tag = gld,
                Checked = ConfigManager.get(gld.Id, ConfigElement.Enabled).TRUE()
            };
            IEnumerable<BotChannel> chns = gld.Guild.Channels.Where(xc => xc.Value.Type == ChannelType.Text).OrderBy(xc => xc.Value.Position).Select(xc => new BotChannel(xc.Value));
            chns.ToList().ForEach(s =>
            {
                node.Nodes.Add(new TreeNode { Text = s.Channel.Name, Tag = s, Checked = ConfigManager.get(s.Id, ConfigElement.Enabled).TRUE() });
            });
            channelTree.TopNode.Nodes.Add(node);
            node.Expand();
            node.ExpandAll();
            channelTree.Nodes[0].Expand();
            channelTree.Sort();
        }

        public void RemoveGuild(ulong id)
        {
            channelTree.TopNode.Nodes.OfType<TreeNode>().Where(s => ((BotGuild)s.Tag).Id == id).ToList().ForEach(s => channelTree.Nodes.Remove(s));
        }

        public void AddMessage(BotMessage msg, BotChannel channel)
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

        private bool recCheck = true;

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
                    nsfwBox.Checked = SelectedChannel.Channel.IsNSFW || ConfigManager.get(SelectedChannel.Id, ConfigElement.Nsfw).TRUE();
                    settingsBoxes.ForEach(s => s.Checked = ConfigManager.get(SelectedChannel.Id, (ConfigElement)s.Tag).TRUE());
                    nsfwBox.Enabled = !SelectedChannel.Channel.IsNSFW;
                    clientSettingsPanel.Text = SelectedGuild.Guild.Name + " - " + SelectedChannel.Channel.Name;
                    clientSettingsPanel.Enabled = true;
                }
                catch (InvalidCastException e1)
                {
                    Console.WriteLine(e1.ToString());
                }
            }
            else
            {
                ChannelDefined = false;
                clientSettingsPanel.Enabled = false;
                clientSettingsPanel.Text = "Channel";
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
                nodes.ToList().ForEach(s => s.Checked = ConfigManager.get(((IBotStruct)s.Tag).Id, ConfigElement.Enabled).TRUE());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
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

        private bool busy = false;

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
                            ConfigManager.set(c.Id, ConfigElement.Enabled, e.Node.Checked, true);
                        }
                        else if (e.Node.Tag.tryCast(out BotGuild g))
                        {
                            ConfigManager.set(g.Id, ConfigElement.Enabled, e.Node.Checked, true, "guild");
                        }
                    }
                    else
                        Common.guildsBox = e.Node.Checked;
                }
        }

        private void checkNodes(TreeNode node, bool check)
        {
            if (recCheck)
                foreach (TreeNode child in node.Nodes)
                {
                    child.Checked = check;
                    checkNodes(child, check);
                }
        }

        private void nsfwBox_CheckedChanged(object sender, EventArgs e) => ClassExtensions.ExIf(ChannelDefined, () => ConfigManager.set(SelectedChannel.Channel, ConfigElement.Nsfw, nsfwBox.Checked, true));

        private void debugButton_Click(object sender, EventArgs e) => ClassExtensions.ExIf(ChannelDefined, () => new Thread(() => MessageBox.Show(ConfigManager.getStr(SelectedChannel.Id))).Start());

        private void SendMessage(string message, BotChannel channel, Action<Task> continuationAction = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;
            _ = continuationAction == null
                ? Task.Run(() => channel.Channel.SendMessageAsync(message))
                : Task.Run(() => channel.Channel.SendMessageAsync(message)).ContinueWith(continuationAction);
        }

        private void pingButton_Click(object sender, EventArgs e) => ClassExtensions.ExIf(ChannelDefined, () => _ = Administration.Ping(SelectedChannel.Channel, (c1, c2, c3) => SelectedChannel.Channel.SendMessageAsync(c1, c2, c3)));

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

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}