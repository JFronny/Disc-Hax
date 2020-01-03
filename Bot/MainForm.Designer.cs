namespace Bot
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        public System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Guilds");
            System.ComponentModel.ComponentResourceManager resources =
                new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.ChannelTree = new System.Windows.Forms.TreeView();
            this.chatBox = new System.Windows.Forms.ListBox();
            this.chatPanel = new System.Windows.Forms.Panel();
            this.chatSend = new System.Windows.Forms.TextBox();
            this.resetButton = new System.Windows.Forms.Button();
            this.pingButton = new System.Windows.Forms.Button();
            this.debugButton = new System.Windows.Forms.Button();
            this.nsfwBox = new System.Windows.Forms.CheckBox();
            this.clientSettingsPanel = new System.Windows.Forms.GroupBox();
            this.clientCheckGrid = new System.Windows.Forms.FlowLayoutPanel();
            this.cleanButton = new System.Windows.Forms.Button();
            this.settingsPanel = new System.Windows.Forms.Panel();
            this.serverSettingsPanel = new System.Windows.Forms.Panel();
            this.tokenButton = new System.Windows.Forms.Button();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.chatPanel.SuspendLayout();
            this.clientSettingsPanel.SuspendLayout();
            this.settingsPanel.SuspendLayout();
            this.serverSettingsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // channelTree
            // 
            this.ChannelTree.CheckBoxes = true;
            this.ChannelTree.Dock = System.Windows.Forms.DockStyle.Left;
            this.ChannelTree.Enabled = false;
            this.ChannelTree.Location = new System.Drawing.Point(0, 0);
            this.ChannelTree.Name = "ChannelTree";
            treeNode1.Name = "Guilds";
            treeNode1.Text = "Guilds";
            this.ChannelTree.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {treeNode1});
            this.ChannelTree.Size = new System.Drawing.Size(193, 437);
            this.ChannelTree.TabIndex = 0;
            this.ChannelTree.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.channelTree_AfterCheck);
            this.ChannelTree.AfterCollapse +=
                new System.Windows.Forms.TreeViewEventHandler(this.channelTree_AfterCollapse);
            this.ChannelTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.channelTree_AfterSelect);
            // 
            // chatBox
            // 
            this.chatBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chatBox.Enabled = false;
            this.chatBox.FormattingEnabled = true;
            this.chatBox.ItemHeight = 15;
            this.chatBox.Location = new System.Drawing.Point(0, 0);
            this.chatBox.Name = "chatBox";
            this.chatBox.Size = new System.Drawing.Size(557, 414);
            this.chatBox.TabIndex = 1;
            // 
            // chatPanel
            // 
            this.chatPanel.Controls.Add(this.chatBox);
            this.chatPanel.Controls.Add(this.chatSend);
            this.chatPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chatPanel.Location = new System.Drawing.Point(193, 0);
            this.chatPanel.Name = "chatPanel";
            this.chatPanel.Size = new System.Drawing.Size(557, 437);
            this.chatPanel.TabIndex = 2;
            // 
            // chatSend
            // 
            this.chatSend.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.chatSend.Enabled = false;
            this.chatSend.Location = new System.Drawing.Point(0, 414);
            this.chatSend.Name = "chatSend";
            this.chatSend.Size = new System.Drawing.Size(557, 23);
            this.chatSend.TabIndex = 2;
            this.chatSend.KeyDown += new System.Windows.Forms.KeyEventHandler(this.chatSend_KeyDown);
            // 
            // resetButton
            // 
            this.resetButton.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom |
                                                       System.Windows.Forms.AnchorStyles.Right)));
            this.resetButton.Location = new System.Drawing.Point(176, 15);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(51, 27);
            this.resetButton.TabIndex = 15;
            this.resetButton.Text = "Reset";
            this.resetButton.UseVisualStyleBackColor = true;
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // pingButton
            // 
            this.pingButton.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom |
                                                       System.Windows.Forms.AnchorStyles.Right)));
            this.pingButton.Location = new System.Drawing.Point(166, 348);
            this.pingButton.Name = "pingButton";
            this.pingButton.Size = new System.Drawing.Size(61, 27);
            this.pingButton.TabIndex = 12;
            this.pingButton.Text = "Ping";
            this.pingButton.UseVisualStyleBackColor = true;
            this.pingButton.Click += new System.EventHandler(this.pingButton_Click);
            // 
            // debugButton
            // 
            this.debugButton.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom |
                                                       System.Windows.Forms.AnchorStyles.Right)));
            this.debugButton.Location = new System.Drawing.Point(115, 15);
            this.debugButton.Name = "debugButton";
            this.debugButton.Size = new System.Drawing.Size(58, 27);
            this.debugButton.TabIndex = 4;
            this.debugButton.Text = "SaveX";
            this.debugButton.UseVisualStyleBackColor = true;
            this.debugButton.Click += new System.EventHandler(this.debugButton_Click);
            // 
            // nsfwBox
            // 
            this.nsfwBox.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom |
                                                       System.Windows.Forms.AnchorStyles.Left)));
            this.nsfwBox.AutoSize = true;
            this.nsfwBox.Location = new System.Drawing.Point(6, 356);
            this.nsfwBox.Name = "nsfwBox";
            this.nsfwBox.Size = new System.Drawing.Size(91, 19);
            this.nsfwBox.TabIndex = 9;
            this.nsfwBox.Text = "Allow NSFW";
            this.nsfwBox.UseVisualStyleBackColor = true;
            this.nsfwBox.CheckedChanged += new System.EventHandler(this.nsfwBox_CheckedChanged);
            // 
            // clientSettingsPanel
            // 
            this.clientSettingsPanel.Controls.Add(this.clientCheckGrid);
            this.clientSettingsPanel.Controls.Add(this.pingButton);
            this.clientSettingsPanel.Controls.Add(this.nsfwBox);
            this.clientSettingsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.clientSettingsPanel.Enabled = false;
            this.clientSettingsPanel.Location = new System.Drawing.Point(0, 0);
            this.clientSettingsPanel.Name = "clientSettingsPanel";
            this.clientSettingsPanel.Size = new System.Drawing.Size(233, 382);
            this.clientSettingsPanel.TabIndex = 16;
            this.clientSettingsPanel.TabStop = false;
            this.clientSettingsPanel.Text = "Channel";
            // 
            // clientCheckGrid
            // 
            this.clientCheckGrid.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top |
                                                         System.Windows.Forms.AnchorStyles.Bottom) |
                                                        System.Windows.Forms.AnchorStyles.Left) |
                                                       System.Windows.Forms.AnchorStyles.Right)));
            this.clientCheckGrid.AutoScroll = true;
            this.clientCheckGrid.Location = new System.Drawing.Point(0, 22);
            this.clientCheckGrid.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.clientCheckGrid.Name = "clientCheckGrid";
            this.clientCheckGrid.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.clientCheckGrid.Size = new System.Drawing.Size(233, 323);
            this.clientCheckGrid.TabIndex = 13;
            // 
            // cleanButton
            // 
            this.cleanButton.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom |
                                                       System.Windows.Forms.AnchorStyles.Right)));
            this.cleanButton.Location = new System.Drawing.Point(63, 15);
            this.cleanButton.Name = "cleanButton";
            this.cleanButton.Size = new System.Drawing.Size(50, 27);
            this.cleanButton.TabIndex = 19;
            this.cleanButton.Text = "Clean";
            this.cleanButton.UseVisualStyleBackColor = true;
            this.cleanButton.Click += new System.EventHandler(this.cleanButton_Click);
            // 
            // settingsPanel
            // 
            this.settingsPanel.Controls.Add(this.clientSettingsPanel);
            this.settingsPanel.Controls.Add(this.serverSettingsPanel);
            this.settingsPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.settingsPanel.Location = new System.Drawing.Point(753, 0);
            this.settingsPanel.Name = "settingsPanel";
            this.settingsPanel.Size = new System.Drawing.Size(233, 437);
            this.settingsPanel.TabIndex = 20;
            // 
            // serverSettingsPanel
            // 
            this.serverSettingsPanel.Controls.Add(this.tokenButton);
            this.serverSettingsPanel.Controls.Add(this.cleanButton);
            this.serverSettingsPanel.Controls.Add(this.resetButton);
            this.serverSettingsPanel.Controls.Add(this.debugButton);
            this.serverSettingsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.serverSettingsPanel.Location = new System.Drawing.Point(0, 382);
            this.serverSettingsPanel.MinimumSize = new System.Drawing.Size(233, 55);
            this.serverSettingsPanel.Name = "serverSettingsPanel";
            this.serverSettingsPanel.Size = new System.Drawing.Size(233, 55);
            this.serverSettingsPanel.TabIndex = 20;
            // 
            // tokenButton
            // 
            this.tokenButton.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom |
                                                       System.Windows.Forms.AnchorStyles.Right)));
            this.tokenButton.Location = new System.Drawing.Point(6, 15);
            this.tokenButton.Name = "tokenButton";
            this.tokenButton.Size = new System.Drawing.Size(55, 27);
            this.tokenButton.TabIndex = 20;
            this.tokenButton.Text = "Tokens";
            this.tokenButton.UseVisualStyleBackColor = true;
            this.tokenButton.Click += new System.EventHandler(this.tokenButton_Click);
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(193, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 437);
            this.splitter1.TabIndex = 21;
            this.splitter1.TabStop = false;
            // 
            // splitter2
            // 
            this.splitter2.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitter2.Location = new System.Drawing.Point(750, 0);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(3, 437);
            this.splitter2.TabIndex = 22;
            this.splitter2.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(986, 437);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.chatPanel);
            this.Controls.Add(this.splitter2);
            this.Controls.Add(this.ChannelTree);
            this.Controls.Add(this.settingsPanel);
            this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(645, 364);
            this.Name = "MainForm";
            this.Text = "DiscHax Bot Menu";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.chatPanel.ResumeLayout(false);
            this.chatPanel.PerformLayout();
            this.clientSettingsPanel.ResumeLayout(false);
            this.clientSettingsPanel.PerformLayout();
            this.settingsPanel.ResumeLayout(false);
            this.serverSettingsPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button cleanButton;
        private System.Windows.Forms.Panel settingsPanel;
        private System.Windows.Forms.FlowLayoutPanel clientCheckGrid;
        private System.Windows.Forms.Panel serverSettingsPanel;
        private System.Windows.Forms.GroupBox clientSettingsPanel;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.Button pingButton;
        private System.Windows.Forms.CheckBox nsfwBox;
        private System.Windows.Forms.Button debugButton;
        private System.Windows.Forms.TextBox chatSend;
        private System.Windows.Forms.Panel chatPanel;
        private System.Windows.Forms.ListBox chatBox;
        private System.Windows.Forms.Button tokenButton;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Splitter splitter2;
    }
}

