namespace STROOP.Tabs
{
    partial class SnowTab
    {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.splitContainerSnow = new STROOP.BetterSplitContainer();
            this.textBoxSnowIndex = new STROOP.BetterTextbox();
            this.buttonSnowRetrieve = new System.Windows.Forms.Button();
            this.groupBoxSnowPosition = new System.Windows.Forms.GroupBox();
            this.checkBoxSnowPositionRelative = new System.Windows.Forms.CheckBox();
            this.textBoxSnowPositionY = new STROOP.BetterTextbox();
            this.buttonSnowPositionYp = new System.Windows.Forms.Button();
            this.buttonSnowPositionYn = new System.Windows.Forms.Button();
            this.buttonSnowPositionXpZp = new System.Windows.Forms.Button();
            this.textBoxSnowPositionXZ = new STROOP.BetterTextbox();
            this.buttonSnowPositionXp = new System.Windows.Forms.Button();
            this.buttonSnowPositionXpZn = new System.Windows.Forms.Button();
            this.buttonSnowPositionZn = new System.Windows.Forms.Button();
            this.buttonSnowPositionZp = new System.Windows.Forms.Button();
            this.buttonSnowPositionXnZp = new System.Windows.Forms.Button();
            this.buttonSnowPositionXn = new System.Windows.Forms.Button();
            this.buttonSnowPositionXnZn = new System.Windows.Forms.Button();
            this._variablePanelSnow = new STROOP.Controls.VariablePanel.VariablePanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerSnow)).BeginInit();
            this.splitContainerSnow.Panel1.SuspendLayout();
            this.splitContainerSnow.Panel2.SuspendLayout();
            this.splitContainerSnow.SuspendLayout();
            this.groupBoxSnowPosition.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerSnow
            // 
            this.splitContainerSnow.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerSnow.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerSnow.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerSnow.Location = new System.Drawing.Point(0, 0);
            this.splitContainerSnow.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainerSnow.Name = "splitContainerSnow";
            // 
            // splitContainerSnow.Panel1
            // 
            this.splitContainerSnow.Panel1.AutoScroll = true;
            this.splitContainerSnow.Panel1.Controls.Add(this.textBoxSnowIndex);
            this.splitContainerSnow.Panel1.Controls.Add(this.buttonSnowRetrieve);
            this.splitContainerSnow.Panel1.Controls.Add(this.groupBoxSnowPosition);
            this.splitContainerSnow.Panel1MinSize = 0;
            // 
            // splitContainerSnow.Panel2
            // 
            this.splitContainerSnow.Panel2.Controls.Add(this._variablePanelSnow);
            this.splitContainerSnow.Panel2.Padding = new System.Windows.Forms.Padding(2);
            this.splitContainerSnow.Panel2MinSize = 0;
            this.splitContainerSnow.Size = new System.Drawing.Size(915, 463);
            this.splitContainerSnow.SplitterDistance = 210;
            this.splitContainerSnow.SplitterWidth = 1;
            this.splitContainerSnow.TabIndex = 35;
            // 
            // textBoxSnowIndex
            // 
            this.textBoxSnowIndex.Location = new System.Drawing.Point(6, 3);
            this.textBoxSnowIndex.Name = "textBoxSnowIndex";
            this.textBoxSnowIndex.Size = new System.Drawing.Size(86, 20);
            this.textBoxSnowIndex.TabIndex = 32;
            this.textBoxSnowIndex.Text = "0";
            this.textBoxSnowIndex.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // buttonSnowRetrieve
            // 
            this.buttonSnowRetrieve.Location = new System.Drawing.Point(97, 3);
            this.buttonSnowRetrieve.Margin = new System.Windows.Forms.Padding(2);
            this.buttonSnowRetrieve.Name = "buttonSnowRetrieve";
            this.buttonSnowRetrieve.Size = new System.Drawing.Size(91, 21);
            this.buttonSnowRetrieve.TabIndex = 31;
            this.buttonSnowRetrieve.Text = "Retrieve";
            this.buttonSnowRetrieve.UseVisualStyleBackColor = true;
            // 
            // groupBoxSnowPosition
            // 
            this.groupBoxSnowPosition.Controls.Add(this.checkBoxSnowPositionRelative);
            this.groupBoxSnowPosition.Controls.Add(this.textBoxSnowPositionY);
            this.groupBoxSnowPosition.Controls.Add(this.buttonSnowPositionYp);
            this.groupBoxSnowPosition.Controls.Add(this.buttonSnowPositionYn);
            this.groupBoxSnowPosition.Controls.Add(this.buttonSnowPositionXpZp);
            this.groupBoxSnowPosition.Controls.Add(this.textBoxSnowPositionXZ);
            this.groupBoxSnowPosition.Controls.Add(this.buttonSnowPositionXp);
            this.groupBoxSnowPosition.Controls.Add(this.buttonSnowPositionXpZn);
            this.groupBoxSnowPosition.Controls.Add(this.buttonSnowPositionZn);
            this.groupBoxSnowPosition.Controls.Add(this.buttonSnowPositionZp);
            this.groupBoxSnowPosition.Controls.Add(this.buttonSnowPositionXnZp);
            this.groupBoxSnowPosition.Controls.Add(this.buttonSnowPositionXn);
            this.groupBoxSnowPosition.Controls.Add(this.buttonSnowPositionXnZn);
            this.groupBoxSnowPosition.Location = new System.Drawing.Point(3, 28);
            this.groupBoxSnowPosition.Name = "groupBoxSnowPosition";
            this.groupBoxSnowPosition.Size = new System.Drawing.Size(185, 146);
            this.groupBoxSnowPosition.TabIndex = 30;
            this.groupBoxSnowPosition.TabStop = false;
            this.groupBoxSnowPosition.Text = "Position";
            // 
            // checkBoxSnowPositionRelative
            // 
            this.checkBoxSnowPositionRelative.AutoSize = true;
            this.checkBoxSnowPositionRelative.Location = new System.Drawing.Point(120, 0);
            this.checkBoxSnowPositionRelative.Name = "checkBoxSnowPositionRelative";
            this.checkBoxSnowPositionRelative.Size = new System.Drawing.Size(65, 17);
            this.checkBoxSnowPositionRelative.TabIndex = 36;
            this.checkBoxSnowPositionRelative.Text = "Relative";
            this.checkBoxSnowPositionRelative.UseVisualStyleBackColor = true;
            // 
            // textBoxSnowPositionY
            // 
            this.textBoxSnowPositionY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSnowPositionY.Location = new System.Drawing.Point(140, 70);
            this.textBoxSnowPositionY.Name = "textBoxSnowPositionY";
            this.textBoxSnowPositionY.Size = new System.Drawing.Size(42, 20);
            this.textBoxSnowPositionY.TabIndex = 33;
            this.textBoxSnowPositionY.Text = "100";
            this.textBoxSnowPositionY.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // buttonSnowPositionYp
            // 
            this.buttonSnowPositionYp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSnowPositionYp.Location = new System.Drawing.Point(140, 16);
            this.buttonSnowPositionYp.Margin = new System.Windows.Forms.Padding(0);
            this.buttonSnowPositionYp.Name = "buttonSnowPositionYp";
            this.buttonSnowPositionYp.Size = new System.Drawing.Size(42, 42);
            this.buttonSnowPositionYp.TabIndex = 35;
            this.buttonSnowPositionYp.Text = "Y+";
            this.buttonSnowPositionYp.UseVisualStyleBackColor = true;
            // 
            // buttonSnowPositionYn
            // 
            this.buttonSnowPositionYn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSnowPositionYn.Location = new System.Drawing.Point(140, 100);
            this.buttonSnowPositionYn.Margin = new System.Windows.Forms.Padding(0);
            this.buttonSnowPositionYn.Name = "buttonSnowPositionYn";
            this.buttonSnowPositionYn.Size = new System.Drawing.Size(42, 42);
            this.buttonSnowPositionYn.TabIndex = 34;
            this.buttonSnowPositionYn.Text = "Y-";
            this.buttonSnowPositionYn.UseVisualStyleBackColor = true;
            // 
            // buttonSnowPositionXpZp
            // 
            this.buttonSnowPositionXpZp.Location = new System.Drawing.Point(87, 100);
            this.buttonSnowPositionXpZp.Margin = new System.Windows.Forms.Padding(0);
            this.buttonSnowPositionXpZp.Name = "buttonSnowPositionXpZp";
            this.buttonSnowPositionXpZp.Size = new System.Drawing.Size(42, 42);
            this.buttonSnowPositionXpZp.TabIndex = 32;
            this.buttonSnowPositionXpZp.Text = "X+Z+";
            this.buttonSnowPositionXpZp.UseVisualStyleBackColor = true;
            // 
            // textBoxSnowPositionXZ
            // 
            this.textBoxSnowPositionXZ.AcceptsReturn = true;
            this.textBoxSnowPositionXZ.Location = new System.Drawing.Point(45, 70);
            this.textBoxSnowPositionXZ.Name = "textBoxSnowPositionXZ";
            this.textBoxSnowPositionXZ.Size = new System.Drawing.Size(42, 20);
            this.textBoxSnowPositionXZ.TabIndex = 27;
            this.textBoxSnowPositionXZ.Text = "100";
            this.textBoxSnowPositionXZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // buttonSnowPositionXp
            // 
            this.buttonSnowPositionXp.Location = new System.Drawing.Point(87, 58);
            this.buttonSnowPositionXp.Margin = new System.Windows.Forms.Padding(0);
            this.buttonSnowPositionXp.Name = "buttonSnowPositionXp";
            this.buttonSnowPositionXp.Size = new System.Drawing.Size(42, 42);
            this.buttonSnowPositionXp.TabIndex = 31;
            this.buttonSnowPositionXp.Text = "X+";
            this.buttonSnowPositionXp.UseVisualStyleBackColor = true;
            // 
            // buttonSnowPositionXpZn
            // 
            this.buttonSnowPositionXpZn.Location = new System.Drawing.Point(87, 16);
            this.buttonSnowPositionXpZn.Margin = new System.Windows.Forms.Padding(0);
            this.buttonSnowPositionXpZn.Name = "buttonSnowPositionXpZn";
            this.buttonSnowPositionXpZn.Size = new System.Drawing.Size(42, 42);
            this.buttonSnowPositionXpZn.TabIndex = 30;
            this.buttonSnowPositionXpZn.Text = "X+Z-";
            this.buttonSnowPositionXpZn.UseVisualStyleBackColor = true;
            // 
            // buttonSnowPositionZn
            // 
            this.buttonSnowPositionZn.Location = new System.Drawing.Point(45, 16);
            this.buttonSnowPositionZn.Margin = new System.Windows.Forms.Padding(0);
            this.buttonSnowPositionZn.Name = "buttonSnowPositionZn";
            this.buttonSnowPositionZn.Size = new System.Drawing.Size(42, 42);
            this.buttonSnowPositionZn.TabIndex = 29;
            this.buttonSnowPositionZn.Text = "Z-";
            this.buttonSnowPositionZn.UseVisualStyleBackColor = true;
            // 
            // buttonSnowPositionZp
            // 
            this.buttonSnowPositionZp.Location = new System.Drawing.Point(45, 100);
            this.buttonSnowPositionZp.Margin = new System.Windows.Forms.Padding(0);
            this.buttonSnowPositionZp.Name = "buttonSnowPositionZp";
            this.buttonSnowPositionZp.Size = new System.Drawing.Size(42, 42);
            this.buttonSnowPositionZp.TabIndex = 28;
            this.buttonSnowPositionZp.Text = "Z+";
            this.buttonSnowPositionZp.UseVisualStyleBackColor = true;
            // 
            // buttonSnowPositionXnZp
            // 
            this.buttonSnowPositionXnZp.Location = new System.Drawing.Point(3, 100);
            this.buttonSnowPositionXnZp.Margin = new System.Windows.Forms.Padding(0);
            this.buttonSnowPositionXnZp.Name = "buttonSnowPositionXnZp";
            this.buttonSnowPositionXnZp.Size = new System.Drawing.Size(42, 42);
            this.buttonSnowPositionXnZp.TabIndex = 27;
            this.buttonSnowPositionXnZp.Text = "X-Z+";
            this.buttonSnowPositionXnZp.UseVisualStyleBackColor = true;
            // 
            // buttonSnowPositionXn
            // 
            this.buttonSnowPositionXn.Location = new System.Drawing.Point(3, 58);
            this.buttonSnowPositionXn.Margin = new System.Windows.Forms.Padding(0);
            this.buttonSnowPositionXn.Name = "buttonSnowPositionXn";
            this.buttonSnowPositionXn.Size = new System.Drawing.Size(42, 42);
            this.buttonSnowPositionXn.TabIndex = 26;
            this.buttonSnowPositionXn.Text = "X-";
            this.buttonSnowPositionXn.UseVisualStyleBackColor = true;
            // 
            // buttonSnowPositionXnZn
            // 
            this.buttonSnowPositionXnZn.Location = new System.Drawing.Point(3, 16);
            this.buttonSnowPositionXnZn.Margin = new System.Windows.Forms.Padding(0);
            this.buttonSnowPositionXnZn.Name = "buttonSnowPositionXnZn";
            this.buttonSnowPositionXnZn.Size = new System.Drawing.Size(42, 42);
            this.buttonSnowPositionXnZn.TabIndex = 25;
            this.buttonSnowPositionXnZn.Text = "X-Z-";
            this.buttonSnowPositionXnZn.UseVisualStyleBackColor = true;
            // 
            // watchVariablePanelSnow
            // 
            this._variablePanelSnow.DataPath = "Config/SnowData.xml";
            this._variablePanelSnow.Dock = System.Windows.Forms.DockStyle.Fill;
            this._variablePanelSnow.Location = new System.Drawing.Point(2, 2);
            this._variablePanelSnow.Margin = new System.Windows.Forms.Padding(0);
            this._variablePanelSnow.Name = "_variablePanelSnow";
            this._variablePanelSnow.Size = new System.Drawing.Size(698, 457);
            this._variablePanelSnow.TabIndex = 3;
            // 
            // SnowTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainerSnow);
            this.Name = "SnowTab";
            this.Size = new System.Drawing.Size(915, 463);
            this.splitContainerSnow.Panel1.ResumeLayout(false);
            this.splitContainerSnow.Panel1.PerformLayout();
            this.splitContainerSnow.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerSnow)).EndInit();
            this.splitContainerSnow.ResumeLayout(false);
            this.groupBoxSnowPosition.ResumeLayout(false);
            this.groupBoxSnowPosition.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private BetterSplitContainer splitContainerSnow;
        private BetterTextbox textBoxSnowIndex;
        private System.Windows.Forms.Button buttonSnowRetrieve;
        private System.Windows.Forms.GroupBox groupBoxSnowPosition;
        private System.Windows.Forms.CheckBox checkBoxSnowPositionRelative;
        private BetterTextbox textBoxSnowPositionY;
        private System.Windows.Forms.Button buttonSnowPositionYp;
        private System.Windows.Forms.Button buttonSnowPositionYn;
        private System.Windows.Forms.Button buttonSnowPositionXpZp;
        private BetterTextbox textBoxSnowPositionXZ;
        private System.Windows.Forms.Button buttonSnowPositionXp;
        private System.Windows.Forms.Button buttonSnowPositionXpZn;
        private System.Windows.Forms.Button buttonSnowPositionZn;
        private System.Windows.Forms.Button buttonSnowPositionZp;
        private System.Windows.Forms.Button buttonSnowPositionXnZp;
        private System.Windows.Forms.Button buttonSnowPositionXn;
        private System.Windows.Forms.Button buttonSnowPositionXnZn;
        private STROOP.Controls.VariablePanel.VariablePanel _variablePanelSnow;
    }
}
