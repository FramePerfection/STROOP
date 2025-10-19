namespace STROOP.Tabs
{
    partial class OptionsTab
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
            this.splitContainerOptions = new STROOP.BetterSplitContainer();
            this.checkedListBoxObjectSlotOverlaysToShow = new System.Windows.Forms.CheckedListBox();
            this.checkedListBoxSavedSettings = new System.Windows.Forms.CheckedListBox();
            this.checkBoxUseRomHack = new System.Windows.Forms.CheckBox();
            this.buttonOptionsResetSavedSettings = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this._variablePanelOptions = new STROOP.Controls.VariablePanel.VariablePanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerOptions)).BeginInit();
            this.splitContainerOptions.Panel1.SuspendLayout();
            this.splitContainerOptions.Panel2.SuspendLayout();
            this.splitContainerOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerOptions
            // 
            this.splitContainerOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerOptions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerOptions.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerOptions.Location = new System.Drawing.Point(0, 0);
            this.splitContainerOptions.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainerOptions.Name = "splitContainerOptions";
            // 
            // splitContainerOptions.Panel1
            // 
            this.splitContainerOptions.Panel1.AutoScroll = true;
            this.splitContainerOptions.Panel1.Controls.Add(this.checkedListBoxObjectSlotOverlaysToShow);
            this.splitContainerOptions.Panel1.Controls.Add(this.checkedListBoxSavedSettings);
            this.splitContainerOptions.Panel1.Controls.Add(this.checkBoxUseRomHack);
            this.splitContainerOptions.Panel1.Controls.Add(this.buttonOptionsResetSavedSettings);
            this.splitContainerOptions.Panel1.Controls.Add(this.label3);
            this.splitContainerOptions.Panel1MinSize = 0;
            // 
            // splitContainerOptions.Panel2
            // 
            this.splitContainerOptions.Panel2.Controls.Add(this._variablePanelOptions);
            this.splitContainerOptions.Panel2.Padding = new System.Windows.Forms.Padding(2);
            this.splitContainerOptions.Panel2MinSize = 0;
            this.splitContainerOptions.Size = new System.Drawing.Size(915, 463);
            this.splitContainerOptions.SplitterDistance = 468;
            this.splitContainerOptions.SplitterWidth = 1;
            this.splitContainerOptions.TabIndex = 43;
            // 
            // checkedListBoxObjectSlotOverlaysToShow
            // 
            this.checkedListBoxObjectSlotOverlaysToShow.BackColor = System.Drawing.SystemColors.Control;
            this.checkedListBoxObjectSlotOverlaysToShow.CheckOnClick = true;
            this.checkedListBoxObjectSlotOverlaysToShow.FormattingEnabled = true;
            this.checkedListBoxObjectSlotOverlaysToShow.Location = new System.Drawing.Point(266, 7);
            this.checkedListBoxObjectSlotOverlaysToShow.Name = "checkedListBoxObjectSlotOverlaysToShow";
            this.checkedListBoxObjectSlotOverlaysToShow.Size = new System.Drawing.Size(176, 274);
            this.checkedListBoxObjectSlotOverlaysToShow.TabIndex = 41;
            // 
            // checkedListBoxSavedSettings
            // 
            this.checkedListBoxSavedSettings.BackColor = System.Drawing.SystemColors.Control;
            this.checkedListBoxSavedSettings.CheckOnClick = true;
            this.checkedListBoxSavedSettings.FormattingEnabled = true;
            this.checkedListBoxSavedSettings.Location = new System.Drawing.Point(6, 7);
            this.checkedListBoxSavedSettings.Name = "checkedListBoxSavedSettings";
            this.checkedListBoxSavedSettings.Size = new System.Drawing.Size(257, 274);
            this.checkedListBoxSavedSettings.TabIndex = 40;
            // 
            // checkBoxUseRomHack
            // 
            this.checkBoxUseRomHack.AutoSize = true;
            this.checkBoxUseRomHack.Location = new System.Drawing.Point(10, 320);
            this.checkBoxUseRomHack.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxUseRomHack.Name = "checkBoxUseRomHack";
            this.checkBoxUseRomHack.Size = new System.Drawing.Size(166, 17);
            this.checkBoxUseRomHack.TabIndex = 2;
            this.checkBoxUseRomHack.Text = "Enable STROOP ROM hack*";
            this.checkBoxUseRomHack.UseVisualStyleBackColor = true;
            // 
            // buttonOptionsResetSavedSettings
            // 
            this.buttonOptionsResetSavedSettings.Location = new System.Drawing.Point(6, 286);
            this.buttonOptionsResetSavedSettings.Margin = new System.Windows.Forms.Padding(0);
            this.buttonOptionsResetSavedSettings.Name = "buttonOptionsResetSavedSettings";
            this.buttonOptionsResetSavedSettings.Size = new System.Drawing.Size(257, 28);
            this.buttonOptionsResetSavedSettings.TabIndex = 38;
            this.buttonOptionsResetSavedSettings.Text = "Reset Saved Settings";
            this.buttonOptionsResetSavedSettings.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(9, 339);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(129, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "*Requires Pure Interpreter";
            // 
            // watchVariablePanelOptions
            // 
            this._variablePanelOptions.DataPath = "Config/OptionsData.xml";
            this._variablePanelOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this._variablePanelOptions.Location = new System.Drawing.Point(2, 2);
            this._variablePanelOptions.Margin = new System.Windows.Forms.Padding(0);
            this._variablePanelOptions.Name = "_variablePanelOptions";
            this._variablePanelOptions.Size = new System.Drawing.Size(440, 457);
            this._variablePanelOptions.TabIndex = 5;
            // 
            // OptionsTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainerOptions);
            this.Name = "OptionsTab";
            this.Size = new System.Drawing.Size(915, 463);
            this.splitContainerOptions.Panel1.ResumeLayout(false);
            this.splitContainerOptions.Panel1.PerformLayout();
            this.splitContainerOptions.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerOptions)).EndInit();
            this.splitContainerOptions.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private BetterSplitContainer splitContainerOptions;
        private System.Windows.Forms.CheckedListBox checkedListBoxObjectSlotOverlaysToShow;
        private System.Windows.Forms.CheckedListBox checkedListBoxSavedSettings;
        internal System.Windows.Forms.CheckBox checkBoxUseRomHack;
        private System.Windows.Forms.Button buttonOptionsResetSavedSettings;
        private System.Windows.Forms.Label label3;
        private STROOP.Controls.VariablePanel.VariablePanel _variablePanelOptions;
    }
}
