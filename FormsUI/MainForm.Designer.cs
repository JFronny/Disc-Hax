namespace FormsUI
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
            this.pingButton = new System.Windows.Forms.Button();
            this.debugButton = new System.Windows.Forms.Button();
            this.nsfwBox = new System.Windows.Forms.CheckBox();
            this.clientSettingsPanel = new System.Windows.Forms.GroupBox();
            this.cleanButton = new System.Windows.Forms.Button();
            this.settingsPanel = new System.Windows.Forms.Panel();
            this.clientCheckGrid = new System.Windows.Forms.FlowLayoutPanel();
            this.serverSettingsPanel = new System.Windows.Forms.Panel();
            this.chatPanel.SuspendLayout();
            this.clientSettingsPanel.SuspendLayout();
            this.settingsPanel.SuspendLayout();
            this.serverSettingsPanel.SuspendLayout();
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
            this.resetButton.Location = new System.Drawing.Point(142, 13);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(52, 23);
            this.resetButton.TabIndex = 15;
            this.resetButton.Text = "Reset";
            this.resetButton.UseVisualStyleBackColor = true;
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // pingButton
            // 
            this.pingButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pingButton.Location = new System.Drawing.Point(142, 302);
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
            this.debugButton.Location = new System.Drawing.Point(64, 13);
            this.debugButton.Name = "debugButton";
            this.debugButton.Size = new System.Drawing.Size(72, 23);
            this.debugButton.TabIndex = 4;
            this.debugButton.Text = "SaveEx";
            this.debugButton.UseVisualStyleBackColor = true;
            this.debugButton.Click += new System.EventHandler(this.debugButton_Click);
            // 
            // nsfwBox
            // 
            this.nsfwBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.nsfwBox.AutoSize = true;
            this.nsfwBox.Location = new System.Drawing.Point(6, 306);
            this.nsfwBox.Name = "nsfwBox";
            this.nsfwBox.Size = new System.Drawing.Size(86, 17);
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
            this.clientSettingsPanel.Size = new System.Drawing.Size(200, 331);
            this.clientSettingsPanel.TabIndex = 16;
            this.clientSettingsPanel.TabStop = false;
            this.clientSettingsPanel.Text = "Channel";
            // 
            // cleanButton
            // 
            this.cleanButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cleanButton.Location = new System.Drawing.Point(6, 13);
            this.cleanButton.Name = "cleanButton";
            this.cleanButton.Size = new System.Drawing.Size(52, 23);
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
            this.settingsPanel.Location = new System.Drawing.Point(645, 0);
            this.settingsPanel.Name = "settingsPanel";
            this.settingsPanel.Size = new System.Drawing.Size(200, 379);
            this.settingsPanel.TabIndex = 20;
            // 
            // clientCheckGrid
            // 
            this.clientCheckGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.clientCheckGrid.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.clientCheckGrid.Location = new System.Drawing.Point(0, 19);
            this.clientCheckGrid.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.clientCheckGrid.Name = "clientCheckGrid";
            this.clientCheckGrid.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.clientCheckGrid.Size = new System.Drawing.Size(200, 280);
            this.clientCheckGrid.TabIndex = 13;
            // 
            // serverSettingsPanel
            // 
            this.serverSettingsPanel.Controls.Add(this.cleanButton);
            this.serverSettingsPanel.Controls.Add(this.resetButton);
            this.serverSettingsPanel.Controls.Add(this.debugButton);
            this.serverSettingsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.serverSettingsPanel.Location = new System.Drawing.Point(0, 331);
            this.serverSettingsPanel.Name = "serverSettingsPanel";
            this.serverSettingsPanel.Size = new System.Drawing.Size(200, 48);
            this.serverSettingsPanel.TabIndex = 20;
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
            this.serverSettingsPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.TreeView channelTree;
        public System.Windows.Forms.ListBox chatBox;
        public System.Windows.Forms.Panel chatPanel;
        public System.Windows.Forms.TextBox chatSend;
        public System.Windows.Forms.Button debugButton;
        public System.Windows.Forms.CheckBox nsfwBox;
        public System.Windows.Forms.Button pingButton;
        public System.Windows.Forms.Button resetButton;
        public System.Windows.Forms.GroupBox clientSettingsPanel;
        private System.Windows.Forms.Button cleanButton;
        private System.Windows.Forms.Panel settingsPanel;
        private System.Windows.Forms.FlowLayoutPanel clientCheckGrid;
        private System.Windows.Forms.Panel serverSettingsPanel;
    }
}