﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Bot.Commands;
using CC_Functions.Misc;
using DSharpPlus;
using Shared;
using Shared.Config;

namespace Bot
{
    public partial class MainForm : Form
    {
        private readonly List<CheckBox> settingsBoxes;
        private bool busy;
        public bool ChannelDefined;

        private bool recCheck = true;

        public MainForm()
        {
            InitializeComponent();
            ToolTip tt = new ToolTip();
            ConfigManager.configUpdate += (sender, ID, element) =>
            {
                if (ChannelDefined && ulong.TryParse(ID, out ulong q) && SelectedChannel.Id == q)
                    Invoke((MethodInvoker) delegate
                    {
                        channelTree_AfterSelect(null, new TreeViewEventArgs(
                            channelTree.Nodes[0].Nodes.OfType<TreeNode>()
                                .SelectMany(s => s.Nodes.OfType<TreeNode>())
                                .First(s => ((BotChannel) s.Tag).Id == q)));
                    });
                if (element == ConfigManager.ENABLED)
                    ClassExtensions.InvokeAction(this, (MethodInvoker) delegate { updateChecking(); });
            };
            settingsBoxes = new List<CheckBox>();
            string[] array = CommandArr.getC().Except(new[] {ConfigManager.ENABLED, ConfigManager.NSFW})
                .ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                CheckBox box = new CheckBox();
                box.Tag = array[i];
                box.Text = array[i].Replace("method_", "");
                tt.SetToolTip(box, $"Command: {box.Text}");
                box.CheckStateChanged += (sender, e) =>
                {
                    GenericExtensions.runIf(ChannelDefined,
                        () => ConfigManager.set(SelectedChannel.Channel.getInstance(), (string) box.Tag,
                            box.Checked,
                            true));
                };
                clientCheckGrid.Controls.Add(box);
                settingsBoxes.Add(box);
            }
            channelTree.Nodes[0].Checked = Common.guildsBox;
            channelTree.Enabled = true;
            chatBox.Enabled = true;
            chatSend.Enabled = true;
            BotGuild[] arr = GuildSingleton.getAll();
            for (int i = 0; i < arr.Length; i++) AddGuild(arr[i]);
        }

        public Task BotThread { get; set; }
        public Bot Bot { get; set; }
        public CancellationTokenSource TokenSource { get; set; }
        public BotGuild SelectedGuild { get; set; }
        public BotChannel SelectedChannel { get; set; }

        public void AddGuild(BotGuild gld)
        {
            TreeNode node = new TreeNode
            {
                Text = gld.Guild.Name,
                Tag = gld,
                Checked = ConfigManager.get(gld, ConfigManager.ENABLED).TRUE()
            };
            IEnumerable<BotChannel> chns = gld.Guild.Channels.Where(xc => xc.Value.Type == ChannelType.Text)
                .OrderBy(xc => xc.Value.Position).Select(xc => xc.Value.getInstance());
            chns.ToList().ForEach(s =>
            {
                node.Nodes.Add(new TreeNode
                {
                    Text = s.Channel.Name, Tag = s, Checked = ConfigManager.get(s, ConfigManager.ENABLED).TRUE()
                });
            });
            channelTree.TopNode.Nodes.Add(node);
            node.Expand();
            node.ExpandAll();
            channelTree.Nodes[0].Expand();
            channelTree.Sort();
        }

        public void RemoveGuild(ulong id)
        {
            channelTree.TopNode.Nodes.OfType<TreeNode>().Where(s => ((BotGuild) s.Tag).Id == id).ToList()
                .ForEach(s => channelTree.Nodes.Remove(s));
        }

        public void AddMessage(BotMessage msg, BotChannel channel)
        {
            try
            {
                string logMsg = msg.Message.getInstance().ToString();
                if (ChannelDefined && channel.Id == SelectedChannel.Id)
                {
                    chatBox.Items.Add(logMsg);
                    chatBox.SelectedItem = logMsg;
                }
            }
            catch (Exception e)
            {
                SendMessage($"Failed: {e.Message}", channel);
            }
        }

        public void channelTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null && e.Node.Tag != null && e.Node.Parent != null && e.Node.Parent.Tag != null)
            {
                try
                {
                    SelectedGuild = (BotGuild) e.Node.Parent.Tag;
                    SelectedChannel = (BotChannel) e.Node.Tag;
                    ChannelDefined = true;
                    chatBox.Items.Clear();
                    SelectedChannel.Messages.Values.ToList().ForEach(s => chatBox.Items.Add(s));
                    nsfwBox.Checked = SelectedChannel.Channel.IsNSFW ||
                                      ConfigManager.get(SelectedChannel, ConfigManager.NSFW).TRUE();
                    settingsBoxes.ForEach(s =>
                        s.Checked = ConfigManager.get(SelectedChannel, (string) s.Tag).TRUE());
                    nsfwBox.Enabled = !SelectedChannel.Channel.IsNSFW;
                    clientSettingsPanel.Text = $"{SelectedGuild.Guild.Name} - {SelectedChannel.Channel.Name}";
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
                nodes.ToList().ForEach(s =>
                    s.Checked = ConfigManager.get((IBotStruct) s.Tag, ConfigManager.ENABLED).TRUE());
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
                SendMessage(chatSend.Text, SelectedChannel,
                    t => ClassExtensions.SetProperty(chatSend, x => x.Text, ""));
            }
        }

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
                            ConfigManager.set(c, ConfigManager.ENABLED, e.Node.Checked, true);
                        else if (e.Node.Tag.tryCast(out BotGuild g))
                            ConfigManager.set(g, ConfigManager.ENABLED, e.Node.Checked, true);
                    }
                    else
                    {
                        Common.guildsBox = e.Node.Checked;
                    }
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

        private void nsfwBox_CheckedChanged(object sender, EventArgs e) => GenericExtensions.runIf(ChannelDefined,
            () => ConfigManager.set(SelectedChannel, ConfigManager.NSFW, nsfwBox.Checked, true));

        private void debugButton_Click(object sender, EventArgs e)
        {
            string tmp = ConfigManager.getStr(SelectedChannel);
            Console.WriteLine(tmp);
            MessageBox.Show(this, tmp, "", MessageBoxButtons.OK);
        }

        private void SendMessage(string message, BotChannel channel, Action<Task> continuationAction = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;
            _ = continuationAction == null
                ? Task.Run(() => channel.Channel.SendMessageAsync(message))
                : Task.Run(() => channel.Channel.SendMessageAsync(message)).ContinueWith(continuationAction);
        }

        private void pingButton_Click(object sender, EventArgs e) => GenericExtensions.runIf(ChannelDefined,
            () => _ = Administration.Ping(SelectedChannel.Channel,
                (c1, c2, c3) => SelectedChannel.Channel.SendMessageAsync(c1, c2, c3)));

        private void resetButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to reset everything?", "", MessageBoxButtons.YesNo) ==
                DialogResult.Yes)
            {
                Directory.Delete(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Cfgs"), true);
                Close();
            }
        }

        private void channelTree_AfterCollapse(object sender, TreeViewEventArgs e) => channelTree.Nodes[0].Expand();

        private void cleanButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete unused configs?", "", MessageBoxButtons.YesNo) ==
                DialogResult.Yes)
            {
                string[] cfgs =
                    Directory.GetFiles(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), @"Cfgs"));
                string[] guilds = channelTree.Nodes[0].Nodes
                    .OfType<TreeNode>().Select(s => ((IBotStruct) s.Tag).Id.ToString()).ToArray();
                string[] channels = channelTree.Nodes[0].Nodes.OfType<TreeNode>()
                    .SelectMany(s => s.Nodes.OfType<TreeNode>()).Select(s => ((IBotStruct) s.Tag).Id.ToString())
                    .ToArray();
                string[] allowedNames = {"common.xml", "keys.secure"};
                for (int i = 0; i < cfgs.Length; i++)
                {
                    //Clean files
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
                    //Clean contents
                    if (File.Exists(cfgs[i]) && cfgs[i] != allowedNames[1])
                    {
                        XElement root = el.Element("guild") ?? el.Element("channel") ?? el.Element("common");
                        List<string> allowedVars = CommandArr.getC().ToList();
                        if (cfgs[i] != allowedNames[0])
                            allowedVars.AddRange(new[] {"prefix", "guildsBox"});
                        else
                            allowedVars.AddRange(new[] {"upperType", "upper"});
                        List<XElement> filteredELs =
                            root.Elements().Where(s => allowedVars.Contains(s.Name.LocalName)).ToList();
                        root.RemoveAll();
                        filteredELs.ForEach(s => root.Add(s));
                        el.Save(cfgs[i]);
                    }
                }
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
            Dispose();
        }
    }
}