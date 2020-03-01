using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Bot.Commands;
using CC_Functions.Misc;
using DSharpPlus;
using DSharpPlus.Entities;
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
            ConfigManager.ConfigUpdate += (sender, ID, element) =>
            {
                if (ChannelDefined && ulong.TryParse(ID, out ulong q) && SelectedChannel.Id == q)
                    Invoke((MethodInvoker) delegate
                    {
                        channelTree_AfterSelect(null, new TreeViewEventArgs(
                            ChannelTree.Nodes[0].Nodes.OfType<TreeNode>()
                                .SelectMany(s => s.Nodes.OfType<TreeNode>())
                                .First(s => ((DiscordChannel) s.Tag).Id == q)));
                    });
                if (element == ConfigManager.Enabled)
                    this.InvokeAction((MethodInvoker) delegate { updateChecking(); });
            };
            settingsBoxes = new List<CheckBox>();
            string[] array = CommandArr.getC().Except(new[] {ConfigManager.Enabled, ConfigManager.Nsfw})
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
                        () => SelectedChannel.Set((string) box.Tag,
                            box.Checked,
                            true));
                };
                clientCheckGrid.Controls.Add(box);
                settingsBoxes.Add(box);
            }
            ChannelTree.Nodes[0].Checked = Common.guildsBox;
            ChannelTree.Enabled = true;
            chatBox.Enabled = true;
            chatSend.Enabled = true;
            DiscordGuild[] arr = Program.Bot.Guilds.Values.ToArray();
            for (int i = 0; i < arr.Length; i++)
                AddGuild(arr[i]);
        }

        public TreeView ChannelTree { get; private set; }

        public Task BotThread { get; set; }
        public CancellationTokenSource TokenSource { get; set; }
        public DiscordGuild SelectedGuild { get; set; }
        public DiscordChannel SelectedChannel { get; set; }

        public void AddGuild(DiscordGuild gld)
        {
            TreeNode node = new TreeNode
            {
                Text = gld.Name,
                Tag = gld,
                Checked = gld.Get(ConfigManager.Enabled).TRUE()
            };
            IEnumerable<DiscordChannel> chns = gld.Channels.Where(xc => xc.Value.Type == ChannelType.Text)
                .OrderBy(xc => xc.Value.Position).Select(xc => xc.Value);
            chns.ToList().ForEach(s =>
            {
                node.Nodes.Add(new TreeNode
                {
                    Text = s.Name, Tag = s, Checked = s.Get(ConfigManager.Enabled).TRUE()
                });
            });
            ChannelTree.Nodes[0].Nodes.Add(node);
            node.Expand();
            node.ExpandAll();
            ChannelTree.Nodes[0].Expand();
            ChannelTree.Sort();
        }

        public void RemoveGuild(ulong id)
        {
            ChannelTree.TopNode.Nodes.OfType<TreeNode>().Where(s => ((DiscordGuild) s.Tag).Id == id).ToList()
                .ForEach(s => ChannelTree.Nodes.Remove(s));
        }

        public void AddMessage(DiscordMessage msg, DiscordChannel channel)
        {
            try
            {
                string logMsg = msg.GetString();
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

        public async void channelTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null && e.Node.Tag != null && e.Node.Parent != null && e.Node.Parent.Tag != null)
            {
                try
                {
                    SelectedGuild = (DiscordGuild) e.Node.Parent.Tag;
                    SelectedChannel = (DiscordChannel) e.Node.Tag;
                    ChannelDefined = true;
                    chatBox.Items.Clear();
                    (await SelectedChannel.GetMessagesAsync()).ToList().ForEach(s => chatBox.Items.Add(s.GetString()));
                    nsfwBox.Checked = SelectedChannel.IsNSFW || SelectedChannel.getEvaluatedNSFW();
                    settingsBoxes.ForEach(s =>
                        s.Checked = SelectedChannel.Get((string) s.Tag).TRUE());
                    nsfwBox.Enabled = !SelectedChannel.IsNSFW;
                    clientSettingsPanel.Text = $"{SelectedGuild.Name} - {SelectedChannel.Name}";
                    clientSettingsPanel.Enabled = true;
                }
                catch (InvalidCastException e1)
                {
                    Program.Bot.DebugLogger.LogMessage(LogLevel.Error, "DiscHax",
                        "channelTree_AfterSelect: cast failed", DateTime.Now, e1);
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
                IEnumerable<TreeNode> nodes = ChannelTree.Nodes.OfType<TreeNode>();
                nodes.First().Checked = Common.guildsBox;
                nodes = nodes.First().Nodes.OfType<TreeNode>();
                nodes = nodes.Concat(nodes.SelectMany(s => s.Nodes.OfType<TreeNode>()));
                nodes.ToList().ForEach(s =>
                {
                    if (s.Tag.GetType() == typeof(DiscordChannel))
                        s.Checked = ((DiscordChannel) s.Tag).Get(ConfigManager.Enabled).TRUE();
                    else if (s.Tag.GetType() == typeof(DiscordGuild))
                        s.Checked = ((DiscordGuild) s.Tag).Get(ConfigManager.Enabled).TRUE();
                    else
                        Program.Bot.DebugLogger.LogMessage(LogLevel.Error, "DiscHax",
                            $"Unexpected Type ({s.Tag.GetType()}) in MainForm.updateChecking()", DateTime.Now);
                });
            }
            catch (Exception e)
            {
                Program.Bot.DebugLogger.LogMessage(LogLevel.Error, "DiscHax", "updateChecking failed",
                    DateTime.Now, e);
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
                    t => chatSend.SetProperty(x => x.Text, ""));
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
                        if (e.Node.Tag.tryCast(out DiscordChannel c))
                            c.Set(ConfigManager.Enabled, e.Node.Checked, true);
                        else if (e.Node.Tag.tryCast(out DiscordGuild g))
                            g.Set(ConfigManager.Enabled, e.Node.Checked, true);
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
            () => SelectedChannel.Set(ConfigManager.Nsfw, nsfwBox.Checked, true));

        private void debugButton_Click(object sender, EventArgs e)
        {
            if (SelectedChannel != null)
            {
                if (SelectBox.Show(new[] {"Open File", "Show"}, "What should we do?") == "Show")
                {
                    MessageBox.Show(this, SelectedChannel.GetStr(), "", MessageBoxButtons.OK);
                }
                else
                {
                    ConfigManager.GetXml(SelectedChannel.Id.ToString(), ConfigManager.Channel, out string xmlPath);
                    Process.Start(xmlPath);
                }
            }
        }

        private void SendMessage(string message, DiscordChannel channel, Action<Task>? continuationAction = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;
            _ = continuationAction == null
                ? Task.Run(() => channel.SendMessageAsync(message))
                : Task.Run(() => channel.SendMessageAsync(message)).ContinueWith(continuationAction);
        }

        private void pingButton_Click(object sender, EventArgs e) => GenericExtensions.runIf(ChannelDefined,
            () => _ = Administration.Ping(SelectedChannel,
                (c1, c2, c3) => SelectedChannel.SendMessageAsync(c1, c2, c3)));

        private void resetButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to reset everything?", "", MessageBoxButtons.YesNo) ==
                DialogResult.Yes)
            {
                Directory.Delete(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Cfgs"), true);
                Close();
            }
        }

        private void channelTree_AfterCollapse(object sender, TreeViewEventArgs e) => ChannelTree.Nodes[0].Expand();

        private async void cleanButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete unused configs?", "", MessageBoxButtons.YesNo) ==
                DialogResult.Yes)
            {
                string[] cfgs =
                    Directory.GetFiles(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), @"Cfgs"));
                string[] guilds = Program.Bot.Guilds.Select(s => s.Key.ToString()).ToArray();
                string[] channels = Program.Bot.Guilds.SelectMany(s => s.Value.Channels).Select(s => s.Key.ToString())
                    .ToArray();
                string[] allowedNames = {"common.xml"};
                cfgs = cfgs.Where(s => Path.GetFileName(s) != "keys.secure").ToArray();
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
                    if (File.Exists(cfgs[i]))
                    {
                        XElement root = el.Element(ConfigManager.Guild) ??
                                        el.Element(ConfigManager.Channel) ?? el.Element("common");
                        List<string> allowedVars = CommandArr.getC().ToList();
                        if (cfgs[i] != allowedNames[0])
                            allowedVars.AddRange(new[] {"prefix", "guildsBox"});
                        else
                            allowedVars.AddRange(new[] {"upperType", "upper", ConfigManager.Users, ConfigManager.Bans});
                        List<XElement> filteredELs =
                            root.Elements().Where(s => allowedVars.Contains(s.Name.LocalName)).ToList();
                        root.RemoveAll();
                        filteredELs.ForEach(s => root.Add(s));
                        if (root.Name.LocalName == ConfigManager.Guild)
                            if (root.Element(ConfigManager.Users) != null)
                                foreach (XElement element in root.Element(ConfigManager.Users).Elements())
                                    if (guilds.Count(s => s == element.Name.LocalName.Replace("user", "")) == 0)
                                        element.Remove();
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

        private void tokenButton_Click(object sender, EventArgs e)
        {
            new TokenForm(TokenManager.DiscordToken, TokenManager.CurrencyconverterapiToken,
                TokenManager.PerspectiveToken).Show();
        }
    }
}