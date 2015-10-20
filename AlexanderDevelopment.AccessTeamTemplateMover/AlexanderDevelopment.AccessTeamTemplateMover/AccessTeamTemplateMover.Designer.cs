namespace AlexanderDevelopment.AccessTeamTemplateMover
{
    partial class AccessTeamTemplateMover
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AccessTeamTemplateMover));
            this.targetOrgLabel = new System.Windows.Forms.Label();
            this.sourceOrgLabel = new System.Windows.Forms.Label();
            this.selectSourceButton = new System.Windows.Forms.Button();
            this.selectTargetButton = new System.Windows.Forms.Button();
            this.executeButton = new System.Windows.Forms.Button();
            this.toolStripMenu = new System.Windows.Forms.ToolStrip();
            this.closeButton = new System.Windows.Forms.ToolStripButton();
            this.enableTargetAccessTeamsCheckBox = new System.Windows.Forms.CheckBox();
            this.toolStripMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // targetOrgLabel
            // 
            this.targetOrgLabel.AutoSize = true;
            this.targetOrgLabel.ForeColor = System.Drawing.Color.Red;
            this.targetOrgLabel.Location = new System.Drawing.Point(130, 65);
            this.targetOrgLabel.Name = "targetOrgLabel";
            this.targetOrgLabel.Size = new System.Drawing.Size(59, 13);
            this.targetOrgLabel.TabIndex = 0;
            this.targetOrgLabel.Text = "unselected";
            // 
            // sourceOrgLabel
            // 
            this.sourceOrgLabel.AutoSize = true;
            this.sourceOrgLabel.ForeColor = System.Drawing.Color.Red;
            this.sourceOrgLabel.Location = new System.Drawing.Point(130, 33);
            this.sourceOrgLabel.Name = "sourceOrgLabel";
            this.sourceOrgLabel.Size = new System.Drawing.Size(59, 13);
            this.sourceOrgLabel.TabIndex = 1;
            this.sourceOrgLabel.Text = "unselected";
            // 
            // selectSourceButton
            // 
            this.selectSourceButton.Location = new System.Drawing.Point(3, 28);
            this.selectSourceButton.Name = "selectSourceButton";
            this.selectSourceButton.Size = new System.Drawing.Size(121, 23);
            this.selectSourceButton.TabIndex = 10;
            this.selectSourceButton.Text = "Select source";
            this.selectSourceButton.UseVisualStyleBackColor = true;
            this.selectSourceButton.Click += new System.EventHandler(this.selectSourceButton_Click);
            // 
            // selectTargetButton
            // 
            this.selectTargetButton.Location = new System.Drawing.Point(3, 60);
            this.selectTargetButton.Name = "selectTargetButton";
            this.selectTargetButton.Size = new System.Drawing.Size(121, 23);
            this.selectTargetButton.TabIndex = 20;
            this.selectTargetButton.Text = "Select target";
            this.selectTargetButton.UseVisualStyleBackColor = true;
            this.selectTargetButton.Click += new System.EventHandler(this.selectTargetButton_Click);
            // 
            // executeButton
            // 
            this.executeButton.BackColor = System.Drawing.SystemColors.Control;
            this.executeButton.Enabled = false;
            this.executeButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.executeButton.Location = new System.Drawing.Point(3, 113);
            this.executeButton.Name = "executeButton";
            this.executeButton.Size = new System.Drawing.Size(186, 47);
            this.executeButton.TabIndex = 40;
            this.executeButton.Text = "Copy templates";
            this.executeButton.UseVisualStyleBackColor = false;
            this.executeButton.Click += new System.EventHandler(this.executeButton_Click);
            // 
            // toolStripMenu
            // 
            this.toolStripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.closeButton});
            this.toolStripMenu.Location = new System.Drawing.Point(0, 0);
            this.toolStripMenu.Name = "toolStripMenu";
            this.toolStripMenu.Size = new System.Drawing.Size(434, 25);
            this.toolStripMenu.TabIndex = 100;
            this.toolStripMenu.Text = "toolStrip1";
            // 
            // closeButton
            // 
            this.closeButton.Image = ((System.Drawing.Image)(resources.GetObject("closeButton.Image")));
            this.closeButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(56, 22);
            this.closeButton.Text = "Close";
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // enableTargetAccessTeamsCheckBox
            // 
            this.enableTargetAccessTeamsCheckBox.AutoSize = true;
            this.enableTargetAccessTeamsCheckBox.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.enableTargetAccessTeamsCheckBox.Checked = true;
            this.enableTargetAccessTeamsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.enableTargetAccessTeamsCheckBox.Location = new System.Drawing.Point(4, 90);
            this.enableTargetAccessTeamsCheckBox.Name = "enableTargetAccessTeamsCheckBox";
            this.enableTargetAccessTeamsCheckBox.Size = new System.Drawing.Size(163, 17);
            this.enableTargetAccessTeamsCheckBox.TabIndex = 25;
            this.enableTargetAccessTeamsCheckBox.Text = "Enable target access teams?";
            this.enableTargetAccessTeamsCheckBox.UseVisualStyleBackColor = true;
            this.enableTargetAccessTeamsCheckBox.CheckedChanged += new System.EventHandler(this.enableTargetAccessTeamsCheckBox_CheckedChanged);
            // 
            // AccessTeamTemplateMover
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.enableTargetAccessTeamsCheckBox);
            this.Controls.Add(this.toolStripMenu);
            this.Controls.Add(this.executeButton);
            this.Controls.Add(this.selectTargetButton);
            this.Controls.Add(this.selectSourceButton);
            this.Controls.Add(this.sourceOrgLabel);
            this.Controls.Add(this.targetOrgLabel);
            this.Name = "AccessTeamTemplateMover";
            this.Size = new System.Drawing.Size(434, 236);
            this.Load += new System.EventHandler(this.AccessTeamTemplateMover_Load);
            this.toolStripMenu.ResumeLayout(false);
            this.toolStripMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label targetOrgLabel;
        private System.Windows.Forms.Label sourceOrgLabel;
        private System.Windows.Forms.Button selectSourceButton;
        private System.Windows.Forms.Button selectTargetButton;
        private System.Windows.Forms.Button executeButton;
        private System.Windows.Forms.ToolStrip toolStripMenu;
        private System.Windows.Forms.ToolStripButton closeButton;
        private System.Windows.Forms.CheckBox enableTargetAccessTeamsCheckBox;
    }
}