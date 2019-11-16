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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Guilds");
            this.channelTree = new System.Windows.Forms.TreeView();
            this.chatBox = new System.Windows.Forms.ListBox();
            this.chatPanel = new System.Windows.Forms.Panel();
            this.chatSend = new System.Windows.Forms.TextBox();
            this.settingsPanel = new System.Windows.Forms.Panel();
            this.chatPanel.SuspendLayout();
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
            this.settingsPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.settingsPanel.Location = new System.Drawing.Point(678, 0);
            this.settingsPanel.Name = "settingsPanel";
            this.settingsPanel.Size = new System.Drawing.Size(200, 310);
            this.settingsPanel.TabIndex = 3;
            // 
            // Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
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
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView channelTree;
        private System.Windows.Forms.ListBox chatBox;
        private System.Windows.Forms.Panel chatPanel;
        private System.Windows.Forms.TextBox chatSend;
        private System.Windows.Forms.Panel settingsPanel;
    }
}

