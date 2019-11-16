namespace Bot
{
    partial class Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Guilds");
            this.channelTree = new System.Windows.Forms.TreeView();
            this.chatBox = new System.Windows.Forms.ListBox();
            this.chatPanel = new System.Windows.Forms.Panel();
            this.chatSend = new System.Windows.Forms.TextBox();
            this.settingsPanel = new System.Windows.Forms.Panel();
            this.debugButton = new System.Windows.Forms.Button();
            this.booruBox = new System.Windows.Forms.CheckBox();
            this.waifuBox = new System.Windows.Forms.CheckBox();
            this.playBox = new System.Windows.Forms.CheckBox();
            this.chanBox = new System.Windows.Forms.CheckBox();
            this.chanButton = new System.Windows.Forms.Button();
            this.playButton = new System.Windows.Forms.Button();
            this.waifuButton = new System.Windows.Forms.Button();
            this.booruButton = new System.Windows.Forms.Button();
            this.chatPanel.SuspendLayout();
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
            treeNode5.Name = "Guilds";
            treeNode5.Text = "Guilds";
            this.channelTree.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode5});
            this.channelTree.Size = new System.Drawing.Size(166, 310);
            this.channelTree.TabIndex = 0;
            this.channelTree.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.channelTree_AfterCheck);
            this.channelTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.channelTree_AfterSelect);
            // 
            // chatBox
            // 
            this.chatBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chatBox.Enabled = false;
            this.chatBox.FormattingEnabled = true;
            this.chatBox.Location = new System.Drawing.Point(0, 0);
            this.chatBox.Name = "chatBox";
            this.chatBox.Size = new System.Drawing.Size(512, 310);
            this.chatBox.TabIndex = 1;
            // 
            // chatPanel
            // 
            this.chatPanel.Controls.Add(this.chatSend);
            this.chatPanel.Controls.Add(this.chatBox);
            this.chatPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chatPanel.Location = new System.Drawing.Point(166, 0);
            this.chatPanel.Name = "chatPanel";
            this.chatPanel.Size = new System.Drawing.Size(512, 310);
            this.chatPanel.TabIndex = 2;
            // 
            // chatSend
            // 
            this.chatSend.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.chatSend.Enabled = false;
            this.chatSend.Location = new System.Drawing.Point(0, 290);
            this.chatSend.Name = "chatSend";
            this.chatSend.Size = new System.Drawing.Size(512, 20);
            this.chatSend.TabIndex = 2;
            this.chatSend.KeyDown += new System.Windows.Forms.KeyEventHandler(this.chatSend_KeyDown);
            // 
            // settingsPanel
            // 
            this.settingsPanel.Controls.Add(this.booruButton);
            this.settingsPanel.Controls.Add(this.waifuButton);
            this.settingsPanel.Controls.Add(this.playButton);
            this.settingsPanel.Controls.Add(this.chanButton);
            this.settingsPanel.Controls.Add(this.debugButton);
            this.settingsPanel.Controls.Add(this.booruBox);
            this.settingsPanel.Controls.Add(this.waifuBox);
            this.settingsPanel.Controls.Add(this.playBox);
            this.settingsPanel.Controls.Add(this.chanBox);
            this.settingsPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.settingsPanel.Location = new System.Drawing.Point(678, 0);
            this.settingsPanel.Name = "settingsPanel";
            this.settingsPanel.Size = new System.Drawing.Size(200, 310);
            this.settingsPanel.TabIndex = 3;
            // 
            // debugButton
            // 
            this.debugButton.Location = new System.Drawing.Point(132, 284);
            this.debugButton.Name = "debugButton";
            this.debugButton.Size = new System.Drawing.Size(65, 23);
            this.debugButton.TabIndex = 4;
            this.debugButton.Text = "SaveEx";
            this.debugButton.UseVisualStyleBackColor = true;
            this.debugButton.Click += new System.EventHandler(this.debugButton_Click);
            // 
            // booruBox
            // 
            this.booruBox.AutoSize = true;
            this.booruBox.Location = new System.Drawing.Point(6, 81);
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
            this.waifuBox.Location = new System.Drawing.Point(6, 58);
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
            this.playBox.Location = new System.Drawing.Point(6, 35);
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
            this.chanBox.Location = new System.Drawing.Point(6, 12);
            this.chanBox.Name = "chanBox";
            this.chanBox.Size = new System.Drawing.Size(108, 17);
            this.chanBox.TabIndex = 0;
            this.chanBox.Text = "!4chan command";
            this.chanBox.UseVisualStyleBackColor = true;
            this.chanBox.CheckedChanged += new System.EventHandler(this.chanBox_CheckedChanged);
            // 
            // chanButton
            // 
            this.chanButton.Location = new System.Drawing.Point(136, 8);
            this.chanButton.Name = "chanButton";
            this.chanButton.Size = new System.Drawing.Size(52, 23);
            this.chanButton.TabIndex = 5;
            this.chanButton.Text = "Run";
            this.chanButton.UseVisualStyleBackColor = true;
            this.chanButton.Click += new System.EventHandler(this.chanButton_Click);
            // 
            // playButton
            // 
            this.playButton.Location = new System.Drawing.Point(136, 31);
            this.playButton.Name = "playButton";
            this.playButton.Size = new System.Drawing.Size(52, 23);
            this.playButton.TabIndex = 6;
            this.playButton.Text = "Run";
            this.playButton.UseVisualStyleBackColor = true;
            this.playButton.Click += new System.EventHandler(this.playButton_Click);
            // 
            // waifuButton
            // 
            this.waifuButton.Location = new System.Drawing.Point(136, 54);
            this.waifuButton.Name = "waifuButton";
            this.waifuButton.Size = new System.Drawing.Size(52, 23);
            this.waifuButton.TabIndex = 7;
            this.waifuButton.Text = "Run";
            this.waifuButton.UseVisualStyleBackColor = true;
            this.waifuButton.Click += new System.EventHandler(this.waifuButton_Click);
            // 
            // booruButton
            // 
            this.booruButton.Location = new System.Drawing.Point(136, 77);
            this.booruButton.Name = "booruButton";
            this.booruButton.Size = new System.Drawing.Size(52, 23);
            this.booruButton.TabIndex = 8;
            this.booruButton.Text = "Run";
            this.booruButton.UseVisualStyleBackColor = true;
            this.booruButton.Click += new System.EventHandler(this.booruButton_Click);
            // 
            // Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(878, 310);
            this.Controls.Add(this.chatPanel);
            this.Controls.Add(this.channelTree);
            this.Controls.Add(this.settingsPanel);
            this.MaximizeBox = false;
            this.Name = "Form";
            this.Text = "DiscHax Bot Menu";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_FormClosed);
            this.chatPanel.ResumeLayout(false);
            this.chatPanel.PerformLayout();
            this.settingsPanel.ResumeLayout(false);
            this.settingsPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.TreeView channelTree;
        public System.Windows.Forms.ListBox chatBox;
        public System.Windows.Forms.Panel chatPanel;
        public System.Windows.Forms.TextBox chatSend;
        public System.Windows.Forms.Panel settingsPanel;
        public System.Windows.Forms.CheckBox chanBox;
        public System.Windows.Forms.CheckBox playBox;
        public System.Windows.Forms.CheckBox waifuBox;
        public System.Windows.Forms.CheckBox booruBox;
        private System.Windows.Forms.Button debugButton;
        private System.Windows.Forms.Button chanButton;
        private System.Windows.Forms.Button booruButton;
        private System.Windows.Forms.Button waifuButton;
        private System.Windows.Forms.Button playButton;
    }
}

