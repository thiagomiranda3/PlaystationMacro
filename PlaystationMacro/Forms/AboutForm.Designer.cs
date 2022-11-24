namespace PlaystationMacro.Forms
{
    partial class AboutForm
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
            this.createdByLabel = new System.Windows.Forms.Label();
            this.bioLinkLabel = new System.Windows.Forms.LinkLabel();
            this.iconPictureBox = new System.Windows.Forms.PictureBox();
            this.titleLabel = new System.Windows.Forms.Label();
            this.githubLinkLabel = new System.Windows.Forms.LinkLabel();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.iconPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // createdByLabel
            // 
            this.createdByLabel.AutoSize = true;
            this.createdByLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(222)));
            this.createdByLabel.Location = new System.Drawing.Point(109, 64);
            this.createdByLabel.Name = "createdByLabel";
            this.createdByLabel.Size = new System.Drawing.Size(166, 20);
            this.createdByLabel.TabIndex = 0;
            this.createdByLabel.Text = "Forked From: Komefai";
            // 
            // bioLinkLabel
            // 
            this.bioLinkLabel.AutoSize = true;
            this.bioLinkLabel.Location = new System.Drawing.Point(113, 87);
            this.bioLinkLabel.Name = "bioLinkLabel";
            this.bioLinkLabel.Size = new System.Drawing.Size(98, 13);
            this.bioLinkLabel.TabIndex = 1;
            this.bioLinkLabel.TabStop = true;
            this.bioLinkLabel.Text = "http://komefai.com";
            this.bioLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.bioLinkLabel_LinkClicked);
            // 
            // iconPictureBox
            // 
            this.iconPictureBox.BackgroundImage = global::PlaystationMacro.Properties.Resources.icon_img;
            this.iconPictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.iconPictureBox.Location = new System.Drawing.Point(12, 48);
            this.iconPictureBox.Name = "iconPictureBox";
            this.iconPictureBox.Size = new System.Drawing.Size(84, 87);
            this.iconPictureBox.TabIndex = 2;
            this.iconPictureBox.TabStop = false;
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(222)));
            this.titleLabel.Location = new System.Drawing.Point(109, 12);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(156, 24);
            this.titleLabel.TabIndex = 3;
            this.titleLabel.Text = "Playstation Macro";
            this.titleLabel.Click += new System.EventHandler(this.titleLabel_Click);
            // 
            // githubLinkLabel
            // 
            this.githubLinkLabel.AutoSize = true;
            this.githubLinkLabel.Location = new System.Drawing.Point(113, 36);
            this.githubLinkLabel.Name = "githubLinkLabel";
            this.githubLinkLabel.Size = new System.Drawing.Size(258, 13);
            this.githubLinkLabel.TabIndex = 4;
            this.githubLinkLabel.TabStop = true;
            this.githubLinkLabel.Text = "https://github.com/thiagomiranda3/PlaystationMacro";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(113, 138);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(137, 13);
            this.linkLabel1.TabIndex = 6;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "https://tommiranda.com.br/";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(222)));
            this.label1.Location = new System.Drawing.Point(109, 115);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(226, 20);
            this.label1.TabIndex = 5;
            this.label1.Text = "Maintained By: Thiago Miranda";
            // 
            // AboutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(371, 175);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.githubLinkLabel);
            this.Controls.Add(this.titleLabel);
            this.Controls.Add(this.iconPictureBox);
            this.Controls.Add(this.bioLinkLabel);
            this.Controls.Add(this.createdByLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About";
            ((System.ComponentModel.ISupportInitialize)(this.iconPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label createdByLabel;
        private System.Windows.Forms.LinkLabel bioLinkLabel;
        private System.Windows.Forms.PictureBox iconPictureBox;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.LinkLabel githubLinkLabel;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label1;
    }
}