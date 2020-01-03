using System.ComponentModel;

namespace Shared.Config
{
    partial class TokenForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            System.ComponentModel.ComponentResourceManager resources =
                new System.ComponentModel.ComponentResourceManager(typeof(TokenForm));
            this.discordBox = new System.Windows.Forms.TextBox();
            this.currConvBox = new System.Windows.Forms.TextBox();
            this.discordLabel = new System.Windows.Forms.Label();
            this.currConvLabel = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // discordBox
            // 
            this.discordBox.Anchor =
                ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top |
                                                        System.Windows.Forms.AnchorStyles.Left) |
                                                       System.Windows.Forms.AnchorStyles.Right)));
            this.discordBox.Location = new System.Drawing.Point(79, 12);
            this.discordBox.Name = "discordBox";
            this.discordBox.PasswordChar = '●';
            this.discordBox.Size = new System.Drawing.Size(280, 23);
            this.discordBox.TabIndex = 0;
            // 
            // currConvBox
            // 
            this.currConvBox.Anchor =
                ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top |
                                                        System.Windows.Forms.AnchorStyles.Left) |
                                                       System.Windows.Forms.AnchorStyles.Right)));
            this.currConvBox.Location = new System.Drawing.Point(79, 42);
            this.currConvBox.Name = "currConvBox";
            this.currConvBox.PasswordChar = '●';
            this.currConvBox.Size = new System.Drawing.Size(280, 23);
            this.currConvBox.TabIndex = 1;
            // 
            // discordLabel
            // 
            this.discordLabel.AutoSize = true;
            this.discordLabel.Location = new System.Drawing.Point(12, 15);
            this.discordLabel.Name = "discordLabel";
            this.discordLabel.Size = new System.Drawing.Size(50, 15);
            this.discordLabel.TabIndex = 2;
            this.discordLabel.Text = "Discord:";
            // 
            // currConvLabel
            // 
            this.currConvLabel.AutoSize = true;
            this.currConvLabel.Location = new System.Drawing.Point(12, 44);
            this.currConvLabel.Name = "currConvLabel";
            this.currConvLabel.Size = new System.Drawing.Size(61, 15);
            this.currConvLabel.TabIndex = 3;
            this.currConvLabel.Text = "CurrConv:";
            // 
            // okButton
            // 
            this.okButton.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom |
                                                       System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(283, 70);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 4;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // TokenForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(371, 100);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.currConvLabel);
            this.Controls.Add(this.discordLabel);
            this.Controls.Add(this.currConvBox);
            this.Controls.Add(this.discordBox);
            this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(500, 139);
            this.MinimumSize = new System.Drawing.Size(208, 139);
            this.Name = "TokenForm";
            this.Text = "Tokens";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label discordLabel;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label currConvLabel;
        private System.Windows.Forms.TextBox currConvBox;
        private System.Windows.Forms.TextBox discordBox;
    }
}