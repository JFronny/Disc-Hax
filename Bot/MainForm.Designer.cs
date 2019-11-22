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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.channelTree = new System.Windows.Forms.TreeView();
            this.chatBox = new System.Windows.Forms.ListBox();
            this.chatPanel = new System.Windows.Forms.Panel();
            this.chatSend = new System.Windows.Forms.TextBox();
            this.resetButton = new System.Windows.Forms.Button();
            this.beemovieButton = new System.Windows.Forms.Button();
            this.beemovieBox = new System.Windows.Forms.CheckBox();
            this.pingButton = new System.Windows.Forms.Button();
            this.debugButton = new System.Windows.Forms.Button();
            this.configButton = new System.Windows.Forms.Button();
            this.configBox = new System.Windows.Forms.CheckBox();
            this.nsfwBox = new System.Windows.Forms.CheckBox();
            this.booruButton = new System.Windows.Forms.Button();
            this.waifuButton = new System.Windows.Forms.Button();
            this.playButton = new System.Windows.Forms.Button();
            this.chanButton = new System.Windows.Forms.Button();
            this.booruBox = new System.Windows.Forms.CheckBox();
            this.waifuBox = new System.Windows.Forms.CheckBox();
            this.playBox = new System.Windows.Forms.CheckBox();
            this.chanBox = new System.Windows.Forms.CheckBox();
            this.clientSettingsPanel = new System.Windows.Forms.GroupBox();
            this.cleanButton = new System.Windows.Forms.Button();
            this.pollButton = new System.Windows.Forms.Button();
            this.pollBox = new System.Windows.Forms.CheckBox();
            this.settingsPanel = new System.Windows.Forms.Panel();
            this.chatPanel.SuspendLayout();
            this.clientSettingsPanel.SuspendLayout();
            this.settingsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // channelTree
            // 
            this.channelTree.CheckBoxes = true;
            this.channelTree.Dock = System.Windows.Forms.DockStyle.Left;
            this.channelTree.Enabled = false;
            this.channelTree.Location = new System.Drawing.Point(0, 0);
            this.channelTree.Name = "channelTree";
            treeNode1.Name = "Guilds";
            treeNode1.Text = "Guilds";
            this.channelTree.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.channelTree.Size = new System.Drawing.Size(166, 379);
            this.channelTree.TabIndex = 0;
            this.channelTree.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.channelTree_AfterCheck);
            this.channelTree.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.channelTree_AfterCollapse);
            this.channelTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.channelTree_AfterSelect);
            // 
            // chatBox
            // 
            this.chatBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chatBox.Enabled = false;
            this.chatBox.FormattingEnabled = true;
            this.chatBox.Location = new System.Drawing.Point(0, 0);
            this.chatBox.Name = "chatBox";
            this.chatBox.Size = new System.Drawing.Size(479, 379);
            this.chatBox.TabIndex = 1;
            // 
            // chatPanel
            // 
            this.chatPanel.Controls.Add(this.chatSend);
            this.chatPanel.Controls.Add(this.chatBox);
            this.chatPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chatPanel.Location = new System.Drawing.Point(166, 0);
            this.chatPanel.Name = "chatPanel";
            this.chatPanel.Size = new System.Drawing.Size(479, 379);
            this.chatPanel.TabIndex = 2;
            // 
            // chatSend
            // 
            this.chatSend.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.chatSend.Enabled = false;
            this.chatSend.Location = new System.Drawing.Point(0, 359);
            this.chatSend.Name = "chatSend";
            this.chatSend.Size = new System.Drawing.Size(479, 20);
            this.chatSend.TabIndex = 2;
            this.chatSend.KeyDown += new System.Windows.Forms.KeyEventHandler(this.chatSend_KeyDown);
            // 
            // resetButton
            // 
            this.resetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.resetButton.Location = new System.Drawing.Point(6, 344);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(52, 23);
            this.resetButton.TabIndex = 15;
            this.resetButton.Text = "Reset";
            this.resetButton.UseVisualStyleBackColor = true;
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // beemovieButton
            // 
            this.beemovieButton.Location = new System.Drawing.Point(142, 107);
            this.beemovieButton.Name = "beemovieButton";
            this.beemovieButton.Size = new System.Drawing.Size(52, 23);
            this.beemovieButton.TabIndex = 14;
            this.beemovieButton.Text = "Run";
            this.beemovieButton.UseVisualStyleBackColor = true;
            this.beemovieButton.Click += new System.EventHandler(this.beemovieButton_Click);
            // 
            // beemovieBox
            // 
            this.beemovieBox.AutoSize = true;
            this.beemovieBox.Location = new System.Drawing.Point(12, 111);
            this.beemovieBox.Name = "beemovieBox";
            this.beemovieBox.Size = new System.Drawing.Size(124, 17);
            this.beemovieBox.TabIndex = 13;
            this.beemovieBox.Text = "!beemovie command";
            this.beemovieBox.UseVisualStyleBackColor = true;
            this.beemovieBox.CheckedChanged += new System.EventHandler(this.beemovieBox_CheckedChanged);
            // 
            // pingButton
            // 
            this.pingButton.Location = new System.Drawing.Point(142, 176);
            this.pingButton.Name = "pingButton";
            this.pingButton.Size = new System.Drawing.Size(52, 23);
            this.pingButton.TabIndex = 12;
            this.pingButton.Text = "Ping";
            this.pingButton.UseVisualStyleBackColor = true;
            this.pingButton.Click += new System.EventHandler(this.pingButton_Click);
            // 
            // debugButton
            // 
            this.debugButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.debugButton.Location = new System.Drawing.Point(136, 344);
            this.debugButton.Name = "debugButton";
            this.debugButton.Size = new System.Drawing.Size(52, 23);
            this.debugButton.TabIndex = 4;
            this.debugButton.Text = "SaveEx";
            this.debugButton.UseVisualStyleBackColor = true;
            this.debugButton.Click += new System.EventHandler(this.debugButton_Click);
            // 
            // configButton
            // 
            this.configButton.Location = new System.Drawing.Point(142, 130);
            this.configButton.Name = "configButton";
            this.configButton.Size = new System.Drawing.Size(52, 23);
            this.configButton.TabIndex = 11;
            this.configButton.Text = "Run";
            this.configButton.UseVisualStyleBackColor = true;
            this.configButton.Click += new System.EventHandler(this.configButton_Click);
            // 
            // configBox
            // 
            this.configBox.AutoSize = true;
            this.configBox.Location = new System.Drawing.Point(12, 134);
            this.configBox.Name = "configBox";
            this.configBox.Size = new System.Drawing.Size(107, 17);
            this.configBox.TabIndex = 10;
            this.configBox.Text = "!config command";
            this.configBox.UseVisualStyleBackColor = true;
            this.configBox.CheckedChanged += new System.EventHandler(this.configBox_CheckedChanged);
            // 
            // nsfwBox
            // 
            this.nsfwBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.nsfwBox.AutoSize = true;
            this.nsfwBox.Location = new System.Drawing.Point(6, 313);
            this.nsfwBox.Name = "nsfwBox";
            this.nsfwBox.Size = new System.Drawing.Size(86, 17);
            this.nsfwBox.TabIndex = 9;
            this.nsfwBox.Text = "Allow NSFW";
            this.nsfwBox.UseVisualStyleBackColor = true;
            this.nsfwBox.CheckedChanged += new System.EventHandler(this.nsfwBox_CheckedChanged);
            // 
            // booruButton
            // 
            this.booruButton.Location = new System.Drawing.Point(142, 84);
            this.booruButton.Name = "booruButton";
            this.booruButton.Size = new System.Drawing.Size(52, 23);
            this.booruButton.TabIndex = 8;
            this.booruButton.Text = "Run";
            this.booruButton.UseVisualStyleBackColor = true;
            this.booruButton.Click += new System.EventHandler(this.booruButton_Click);
            // 
            // waifuButton
            // 
            this.waifuButton.Location = new System.Drawing.Point(142, 61);
            this.waifuButton.Name = "waifuButton";
            this.waifuButton.Size = new System.Drawing.Size(52, 23);
            this.waifuButton.TabIndex = 7;
            this.waifuButton.Text = "Run";
            this.waifuButton.UseVisualStyleBackColor = true;
            this.waifuButton.Click += new System.EventHandler(this.waifuButton_Click);
            // 
            // playButton
            // 
            this.playButton.Location = new System.Drawing.Point(142, 38);
            this.playButton.Name = "playButton";
            this.playButton.Size = new System.Drawing.Size(52, 23);
            this.playButton.TabIndex = 6;
            this.playButton.Text = "Run";
            this.playButton.UseVisualStyleBackColor = true;
            this.playButton.Click += new System.EventHandler(this.playButton_Click);
            // 
            // chanButton
            // 
            this.chanButton.Location = new System.Drawing.Point(142, 15);
            this.chanButton.Name = "chanButton";
            this.chanButton.Size = new System.Drawing.Size(52, 23);
            this.chanButton.TabIndex = 5;
            this.chanButton.Text = "Run";
            this.chanButton.UseVisualStyleBackColor = true;
            this.chanButton.Click += new System.EventHandler(this.chanButton_Click);
            // 
            // booruBox
            // 
            this.booruBox.AutoSize = true;
            this.booruBox.Location = new System.Drawing.Point(12, 88);
            this.booruBox.Name = "booruBox";
            this.booruBox.Size = new System.Drawing.Size(105, 17);
            this.booruBox.TabIndex = 3;
            this.booruBox.Text = "!booru command";
            this.booruBox.UseVisualStyleBackColor = true;
            this.booruBox.CheckedChanged += new System.EventHandler(this.booruBox_CheckedChanged);
            // 
            // waifuBox
            // 
            this.waifuBox.AutoSize = true;
            this.waifuBox.Location = new System.Drawing.Point(12, 65);
            this.waifuBox.Name = "waifuBox";
            this.waifuBox.Size = new System.Drawing.Size(103, 17);
            this.waifuBox.TabIndex = 2;
            this.waifuBox.Text = "!waifu command";
            this.waifuBox.UseVisualStyleBackColor = true;
            this.waifuBox.CheckedChanged += new System.EventHandler(this.waifuBox_CheckedChanged);
            // 
            // playBox
            // 
            this.playBox.AutoSize = true;
            this.playBox.Location = new System.Drawing.Point(12, 42);
            this.playBox.Name = "playBox";
            this.playBox.Size = new System.Drawing.Size(97, 17);
            this.playBox.TabIndex = 1;
            this.playBox.Text = "!play command";
            this.playBox.UseVisualStyleBackColor = true;
            this.playBox.CheckedChanged += new System.EventHandler(this.playBox_CheckedChanged);
            // 
            // chanBox
            // 
            this.chanBox.AutoSize = true;
            this.chanBox.Location = new System.Drawing.Point(12, 19);
            this.chanBox.Name = "chanBox";
            this.chanBox.Size = new System.Drawing.Size(108, 17);
            this.chanBox.TabIndex = 0;
            this.chanBox.Text = "!4chan command";
            this.chanBox.UseVisualStyleBackColor = true;
            this.chanBox.CheckedChanged += new System.EventHandler(this.chanBox_CheckedChanged);
            // 
            // clientSettingsPanel
            // 
            this.clientSettingsPanel.Controls.Add(this.pollButton);
            this.clientSettingsPanel.Controls.Add(this.pollBox);
            this.clientSettingsPanel.Controls.Add(this.chanBox);
            this.clientSettingsPanel.Controls.Add(this.beemovieButton);
            this.clientSettingsPanel.Controls.Add(this.playBox);
            this.clientSettingsPanel.Controls.Add(this.beemovieBox);
            this.clientSettingsPanel.Controls.Add(this.waifuBox);
            this.clientSettingsPanel.Controls.Add(this.pingButton);
            this.clientSettingsPanel.Controls.Add(this.booruBox);
            this.clientSettingsPanel.Controls.Add(this.chanButton);
            this.clientSettingsPanel.Controls.Add(this.configButton);
            this.clientSettingsPanel.Controls.Add(this.playButton);
            this.clientSettingsPanel.Controls.Add(this.configBox);
            this.clientSettingsPanel.Controls.Add(this.waifuButton);
            this.clientSettingsPanel.Controls.Add(this.nsfwBox);
            this.clientSettingsPanel.Controls.Add(this.booruButton);
            this.clientSettingsPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.clientSettingsPanel.Enabled = false;
            this.clientSettingsPanel.Location = new System.Drawing.Point(0, 0);
            this.clientSettingsPanel.Name = "clientSettingsPanel";
            this.clientSettingsPanel.Size = new System.Drawing.Size(200, 338);
            this.clientSettingsPanel.TabIndex = 16;
            this.clientSettingsPanel.TabStop = false;
            this.clientSettingsPanel.Text = "Channel";
            // 
            // cleanButton
            // 
            this.cleanButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cleanButton.Location = new System.Drawing.Point(71, 344);
            this.cleanButton.Name = "cleanButton";
            this.cleanButton.Size = new System.Drawing.Size(52, 23);
            this.cleanButton.TabIndex = 19;
            this.cleanButton.Text = "Clean";
            this.cleanButton.UseVisualStyleBackColor = true;
            this.cleanButton.Click += new System.EventHandler(this.cleanButton_Click);
            // 
            // pollButton
            // 
            this.pollButton.Location = new System.Drawing.Point(142, 153);
            this.pollButton.Name = "pollButton";
            this.pollButton.Size = new System.Drawing.Size(52, 23);
            this.pollButton.TabIndex = 17;
            this.pollButton.Text = "Run";
            this.pollButton.UseVisualStyleBackColor = true;
            this.pollButton.Click += new System.EventHandler(this.pollButton_Click);
            // 
            // pollBox
            // 
            this.pollBox.AutoSize = true;
            this.pollBox.Location = new System.Drawing.Point(12, 157);
            this.pollBox.Name = "pollBox";
            this.pollBox.Size = new System.Drawing.Size(94, 17);
            this.pollBox.TabIndex = 16;
            this.pollBox.Text = "!poll command";
            this.pollBox.UseVisualStyleBackColor = true;
            this.pollBox.CheckedChanged += new System.EventHandler(this.pollBox_CheckedChanged);
            // 
            // settingsPanel
            // 
            this.settingsPanel.Controls.Add(this.cleanButton);
            this.settingsPanel.Controls.Add(this.clientSettingsPanel);
            this.settingsPanel.Controls.Add(this.resetButton);
            this.settingsPanel.Controls.Add(this.debugButton);
            this.settingsPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.settingsPanel.Location = new System.Drawing.Point(645, 0);
            this.settingsPanel.Name = "settingsPanel";
            this.settingsPanel.Size = new System.Drawing.Size(200, 379);
            this.settingsPanel.TabIndex = 20;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(845, 379);
            this.Controls.Add(this.chatPanel);
            this.Controls.Add(this.channelTree);
            this.Controls.Add(this.settingsPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(555, 321);
            this.Name = "MainForm";
            this.Text = "DiscHax Bot Menu";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_FormClosed);
            this.chatPanel.ResumeLayout(false);
            this.chatPanel.PerformLayout();
            this.clientSettingsPanel.ResumeLayout(false);
            this.clientSettingsPanel.PerformLayout();
            this.settingsPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.TreeView channelTree;
        public System.Windows.Forms.ListBox chatBox;
        public System.Windows.Forms.Panel chatPanel;
        public System.Windows.Forms.TextBox chatSend;
        public System.Windows.Forms.CheckBox chanBox;
        public System.Windows.Forms.CheckBox playBox;
        public System.Windows.Forms.CheckBox waifuBox;
        public System.Windows.Forms.CheckBox booruBox;
        public System.Windows.Forms.Button debugButton;
        public System.Windows.Forms.Button chanButton;
        public System.Windows.Forms.Button booruButton;
        public System.Windows.Forms.Button waifuButton;
        public System.Windows.Forms.Button playButton;
        public System.Windows.Forms.CheckBox nsfwBox;
        public System.Windows.Forms.Button configButton;
        public System.Windows.Forms.CheckBox configBox;
        public System.Windows.Forms.Button pingButton;
        public System.Windows.Forms.CheckBox beemovieBox;
        public System.Windows.Forms.Button beemovieButton;
        public System.Windows.Forms.Button resetButton;
        public System.Windows.Forms.GroupBox clientSettingsPanel;
        public System.Windows.Forms.Button pollButton;
        public System.Windows.Forms.CheckBox pollBox;
        private System.Windows.Forms.Button cleanButton;
        private System.Windows.Forms.Panel settingsPanel;
    }
}

