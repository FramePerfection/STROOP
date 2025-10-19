namespace STROOP.Tabs
{
    partial class DebugTab
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
            this.splitContainerDebug = new STROOP.BetterSplitContainer();
            this.panelDebugBorder = new System.Windows.Forms.Panel();
            this.pictureBoxDebug = new STROOP.Controls.IntPictureBox();
            this.groupBoxMiscDebug = new System.Windows.Forms.GroupBox();
            this.checkBoxFreeMovement = new System.Windows.Forms.CheckBox();
            this.checkBoxSpawnMode = new System.Windows.Forms.CheckBox();
            this.checkBoxStageSelect = new System.Windows.Forms.CheckBox();
            this.checkBoxClassicMode = new System.Windows.Forms.CheckBox();
            this.groupBoxResourceMeter = new System.Windows.Forms.GroupBox();
            this.radioButtonResourceMeterOff = new System.Windows.Forms.RadioButton();
            this.radioButtonResourceMeter1 = new System.Windows.Forms.RadioButton();
            this.radioButtonResourceMeter2 = new System.Windows.Forms.RadioButton();
            this.groupBoxAdvancedMode = new System.Windows.Forms.GroupBox();
            this.radioButtonAdvancedModeOff = new System.Windows.Forms.RadioButton();
            this.radioButtonAdvancedModeEnemyInfo = new System.Windows.Forms.RadioButton();
            this.radioButtonAdvancedModeObjectCounter = new System.Windows.Forms.RadioButton();
            this.radioButtonAdvancedModeEffectInfo = new System.Windows.Forms.RadioButton();
            this.radioButtonAdvancedModeCheckInfo = new System.Windows.Forms.RadioButton();
            this.radioButtonAdvancedModeStageInfo = new System.Windows.Forms.RadioButton();
            this.radioButtonAdvancedModeMapInfo = new System.Windows.Forms.RadioButton();
            this._variablePanelDebug = new STROOP.Controls.VariablePanel.VariablePanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerDebug)).BeginInit();
            this.splitContainerDebug.Panel1.SuspendLayout();
            this.splitContainerDebug.Panel2.SuspendLayout();
            this.splitContainerDebug.SuspendLayout();
            this.panelDebugBorder.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDebug)).BeginInit();
            this.groupBoxMiscDebug.SuspendLayout();
            this.groupBoxResourceMeter.SuspendLayout();
            this.groupBoxAdvancedMode.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerDebug
            // 
            this.splitContainerDebug.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerDebug.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerDebug.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerDebug.Location = new System.Drawing.Point(0, 0);
            this.splitContainerDebug.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainerDebug.Name = "splitContainerDebug";
            // 
            // splitContainerDebug.Panel1
            // 
            this.splitContainerDebug.Panel1.AutoScroll = true;
            this.splitContainerDebug.Panel1.Controls.Add(this.panelDebugBorder);
            this.splitContainerDebug.Panel1.Controls.Add(this.groupBoxMiscDebug);
            this.splitContainerDebug.Panel1.Controls.Add(this.groupBoxResourceMeter);
            this.splitContainerDebug.Panel1.Controls.Add(this.groupBoxAdvancedMode);
            this.splitContainerDebug.Panel1MinSize = 0;
            // 
            // splitContainerDebug.Panel2
            // 
            this.splitContainerDebug.Panel2.Controls.Add(this._variablePanelDebug);
            this.splitContainerDebug.Panel2.Padding = new System.Windows.Forms.Padding(2);
            this.splitContainerDebug.Panel2MinSize = 0;
            this.splitContainerDebug.Size = new System.Drawing.Size(915, 463);
            this.splitContainerDebug.SplitterDistance = 388;
            this.splitContainerDebug.SplitterWidth = 1;
            this.splitContainerDebug.TabIndex = 40;
            // 
            // panelDebugBorder
            // 
            this.panelDebugBorder.Controls.Add(this.pictureBoxDebug);
            this.panelDebugBorder.Location = new System.Drawing.Point(3, 4);
            this.panelDebugBorder.Margin = new System.Windows.Forms.Padding(2);
            this.panelDebugBorder.Name = "panelDebugBorder";
            this.panelDebugBorder.Size = new System.Drawing.Size(55, 55);
            this.panelDebugBorder.TabIndex = 3;
            // 
            // pictureBoxDebug
            // 
            this.pictureBoxDebug.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxDebug.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            this.pictureBoxDebug.Location = new System.Drawing.Point(3, 3);
            this.pictureBoxDebug.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBoxDebug.MaximumSize = new System.Drawing.Size(133, 130);
            this.pictureBoxDebug.Name = "pictureBoxDebug";
            this.pictureBoxDebug.Size = new System.Drawing.Size(49, 49);
            this.pictureBoxDebug.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxDebug.TabIndex = 0;
            this.pictureBoxDebug.TabStop = false;
            // 
            // groupBoxMiscDebug
            // 
            this.groupBoxMiscDebug.Controls.Add(this.checkBoxFreeMovement);
            this.groupBoxMiscDebug.Controls.Add(this.checkBoxSpawnMode);
            this.groupBoxMiscDebug.Controls.Add(this.checkBoxStageSelect);
            this.groupBoxMiscDebug.Controls.Add(this.checkBoxClassicMode);
            this.groupBoxMiscDebug.Location = new System.Drawing.Point(247, 64);
            this.groupBoxMiscDebug.Name = "groupBoxMiscDebug";
            this.groupBoxMiscDebug.Size = new System.Drawing.Size(117, 125);
            this.groupBoxMiscDebug.TabIndex = 38;
            this.groupBoxMiscDebug.TabStop = false;
            this.groupBoxMiscDebug.Text = "Misc Debug";
            // 
            // checkBoxFreeMovement
            // 
            this.checkBoxFreeMovement.AutoSize = true;
            this.checkBoxFreeMovement.Location = new System.Drawing.Point(6, 96);
            this.checkBoxFreeMovement.Name = "checkBoxFreeMovement";
            this.checkBoxFreeMovement.Size = new System.Drawing.Size(100, 17);
            this.checkBoxFreeMovement.TabIndex = 24;
            this.checkBoxFreeMovement.Text = "Free Movement";
            this.checkBoxFreeMovement.UseVisualStyleBackColor = true;
            // 
            // checkBoxSpawnMode
            // 
            this.checkBoxSpawnMode.AutoSize = true;
            this.checkBoxSpawnMode.Location = new System.Drawing.Point(6, 46);
            this.checkBoxSpawnMode.Name = "checkBoxSpawnMode";
            this.checkBoxSpawnMode.Size = new System.Drawing.Size(89, 17);
            this.checkBoxSpawnMode.TabIndex = 12;
            this.checkBoxSpawnMode.Text = "Spawn Mode";
            this.checkBoxSpawnMode.UseVisualStyleBackColor = true;
            // 
            // checkBoxStageSelect
            // 
            this.checkBoxStageSelect.AutoSize = true;
            this.checkBoxStageSelect.Location = new System.Drawing.Point(6, 71);
            this.checkBoxStageSelect.Name = "checkBoxStageSelect";
            this.checkBoxStageSelect.Size = new System.Drawing.Size(87, 17);
            this.checkBoxStageSelect.TabIndex = 24;
            this.checkBoxStageSelect.Text = "Stage Select";
            this.checkBoxStageSelect.UseVisualStyleBackColor = true;
            // 
            // checkBoxClassicMode
            // 
            this.checkBoxClassicMode.AutoSize = true;
            this.checkBoxClassicMode.Location = new System.Drawing.Point(6, 21);
            this.checkBoxClassicMode.Name = "checkBoxClassicMode";
            this.checkBoxClassicMode.Size = new System.Drawing.Size(89, 17);
            this.checkBoxClassicMode.TabIndex = 23;
            this.checkBoxClassicMode.Text = "Classic Mode";
            this.checkBoxClassicMode.UseVisualStyleBackColor = true;
            // 
            // groupBoxResourceMeter
            // 
            this.groupBoxResourceMeter.Controls.Add(this.radioButtonResourceMeterOff);
            this.groupBoxResourceMeter.Controls.Add(this.radioButtonResourceMeter1);
            this.groupBoxResourceMeter.Controls.Add(this.radioButtonResourceMeter2);
            this.groupBoxResourceMeter.Location = new System.Drawing.Point(137, 64);
            this.groupBoxResourceMeter.Name = "groupBoxResourceMeter";
            this.groupBoxResourceMeter.Size = new System.Drawing.Size(104, 94);
            this.groupBoxResourceMeter.TabIndex = 38;
            this.groupBoxResourceMeter.TabStop = false;
            this.groupBoxResourceMeter.Text = "Resource Meter";
            // 
            // radioButtonResourceMeterOff
            // 
            this.radioButtonResourceMeterOff.AutoSize = true;
            this.radioButtonResourceMeterOff.Checked = true;
            this.radioButtonResourceMeterOff.Location = new System.Drawing.Point(11, 21);
            this.radioButtonResourceMeterOff.Name = "radioButtonResourceMeterOff";
            this.radioButtonResourceMeterOff.Size = new System.Drawing.Size(39, 17);
            this.radioButtonResourceMeterOff.TabIndex = 7;
            this.radioButtonResourceMeterOff.TabStop = true;
            this.radioButtonResourceMeterOff.Text = "Off";
            this.radioButtonResourceMeterOff.UseVisualStyleBackColor = true;
            // 
            // radioButtonResourceMeter1
            // 
            this.radioButtonResourceMeter1.AutoSize = true;
            this.radioButtonResourceMeter1.Location = new System.Drawing.Point(11, 44);
            this.radioButtonResourceMeter1.Name = "radioButtonResourceMeter1";
            this.radioButtonResourceMeter1.Size = new System.Drawing.Size(61, 17);
            this.radioButtonResourceMeter1.TabIndex = 8;
            this.radioButtonResourceMeter1.Text = "Meter 1";
            this.radioButtonResourceMeter1.UseVisualStyleBackColor = true;
            // 
            // radioButtonResourceMeter2
            // 
            this.radioButtonResourceMeter2.AutoSize = true;
            this.radioButtonResourceMeter2.Location = new System.Drawing.Point(11, 67);
            this.radioButtonResourceMeter2.Name = "radioButtonResourceMeter2";
            this.radioButtonResourceMeter2.Size = new System.Drawing.Size(61, 17);
            this.radioButtonResourceMeter2.TabIndex = 9;
            this.radioButtonResourceMeter2.Text = "Meter 2";
            this.radioButtonResourceMeter2.UseVisualStyleBackColor = true;
            // 
            // groupBoxAdvancedMode
            // 
            this.groupBoxAdvancedMode.Controls.Add(this.radioButtonAdvancedModeOff);
            this.groupBoxAdvancedMode.Controls.Add(this.radioButtonAdvancedModeEnemyInfo);
            this.groupBoxAdvancedMode.Controls.Add(this.radioButtonAdvancedModeObjectCounter);
            this.groupBoxAdvancedMode.Controls.Add(this.radioButtonAdvancedModeEffectInfo);
            this.groupBoxAdvancedMode.Controls.Add(this.radioButtonAdvancedModeCheckInfo);
            this.groupBoxAdvancedMode.Controls.Add(this.radioButtonAdvancedModeStageInfo);
            this.groupBoxAdvancedMode.Controls.Add(this.radioButtonAdvancedModeMapInfo);
            this.groupBoxAdvancedMode.Location = new System.Drawing.Point(6, 64);
            this.groupBoxAdvancedMode.Name = "groupBoxAdvancedMode";
            this.groupBoxAdvancedMode.Size = new System.Drawing.Size(125, 184);
            this.groupBoxAdvancedMode.TabIndex = 38;
            this.groupBoxAdvancedMode.TabStop = false;
            this.groupBoxAdvancedMode.Text = "Advanced Mode";
            // 
            // radioButtonAdvancedModeOff
            // 
            this.radioButtonAdvancedModeOff.AutoSize = true;
            this.radioButtonAdvancedModeOff.Checked = true;
            this.radioButtonAdvancedModeOff.Location = new System.Drawing.Point(11, 19);
            this.radioButtonAdvancedModeOff.Name = "radioButtonAdvancedModeOff";
            this.radioButtonAdvancedModeOff.Size = new System.Drawing.Size(39, 17);
            this.radioButtonAdvancedModeOff.TabIndex = 1;
            this.radioButtonAdvancedModeOff.TabStop = true;
            this.radioButtonAdvancedModeOff.Text = "Off";
            this.radioButtonAdvancedModeOff.UseVisualStyleBackColor = true;
            // 
            // radioButtonAdvancedModeEnemyInfo
            // 
            this.radioButtonAdvancedModeEnemyInfo.AutoSize = true;
            this.radioButtonAdvancedModeEnemyInfo.Location = new System.Drawing.Point(11, 157);
            this.radioButtonAdvancedModeEnemyInfo.Name = "radioButtonAdvancedModeEnemyInfo";
            this.radioButtonAdvancedModeEnemyInfo.Size = new System.Drawing.Size(78, 17);
            this.radioButtonAdvancedModeEnemyInfo.TabIndex = 10;
            this.radioButtonAdvancedModeEnemyInfo.Text = "Enemy Info";
            this.radioButtonAdvancedModeEnemyInfo.UseVisualStyleBackColor = true;
            // 
            // radioButtonAdvancedModeObjectCounter
            // 
            this.radioButtonAdvancedModeObjectCounter.AutoSize = true;
            this.radioButtonAdvancedModeObjectCounter.Location = new System.Drawing.Point(11, 42);
            this.radioButtonAdvancedModeObjectCounter.Name = "radioButtonAdvancedModeObjectCounter";
            this.radioButtonAdvancedModeObjectCounter.Size = new System.Drawing.Size(96, 17);
            this.radioButtonAdvancedModeObjectCounter.TabIndex = 5;
            this.radioButtonAdvancedModeObjectCounter.Text = "Object Counter";
            this.radioButtonAdvancedModeObjectCounter.UseVisualStyleBackColor = true;
            // 
            // radioButtonAdvancedModeEffectInfo
            // 
            this.radioButtonAdvancedModeEffectInfo.AutoSize = true;
            this.radioButtonAdvancedModeEffectInfo.Location = new System.Drawing.Point(11, 134);
            this.radioButtonAdvancedModeEffectInfo.Name = "radioButtonAdvancedModeEffectInfo";
            this.radioButtonAdvancedModeEffectInfo.Size = new System.Drawing.Size(74, 17);
            this.radioButtonAdvancedModeEffectInfo.TabIndex = 9;
            this.radioButtonAdvancedModeEffectInfo.Text = "Effect Info";
            this.radioButtonAdvancedModeEffectInfo.UseVisualStyleBackColor = true;
            // 
            // radioButtonAdvancedModeCheckInfo
            // 
            this.radioButtonAdvancedModeCheckInfo.AutoSize = true;
            this.radioButtonAdvancedModeCheckInfo.Location = new System.Drawing.Point(11, 65);
            this.radioButtonAdvancedModeCheckInfo.Name = "radioButtonAdvancedModeCheckInfo";
            this.radioButtonAdvancedModeCheckInfo.Size = new System.Drawing.Size(77, 17);
            this.radioButtonAdvancedModeCheckInfo.TabIndex = 6;
            this.radioButtonAdvancedModeCheckInfo.Text = "Check Info";
            this.radioButtonAdvancedModeCheckInfo.UseVisualStyleBackColor = true;
            // 
            // radioButtonAdvancedModeStageInfo
            // 
            this.radioButtonAdvancedModeStageInfo.AutoSize = true;
            this.radioButtonAdvancedModeStageInfo.Location = new System.Drawing.Point(11, 111);
            this.radioButtonAdvancedModeStageInfo.Name = "radioButtonAdvancedModeStageInfo";
            this.radioButtonAdvancedModeStageInfo.Size = new System.Drawing.Size(74, 17);
            this.radioButtonAdvancedModeStageInfo.TabIndex = 8;
            this.radioButtonAdvancedModeStageInfo.Text = "Stage Info";
            this.radioButtonAdvancedModeStageInfo.UseVisualStyleBackColor = true;
            // 
            // radioButtonAdvancedModeMapInfo
            // 
            this.radioButtonAdvancedModeMapInfo.AutoSize = true;
            this.radioButtonAdvancedModeMapInfo.Location = new System.Drawing.Point(11, 88);
            this.radioButtonAdvancedModeMapInfo.Name = "radioButtonAdvancedModeMapInfo";
            this.radioButtonAdvancedModeMapInfo.Size = new System.Drawing.Size(67, 17);
            this.radioButtonAdvancedModeMapInfo.TabIndex = 7;
            this.radioButtonAdvancedModeMapInfo.Text = "Map Info";
            this.radioButtonAdvancedModeMapInfo.UseVisualStyleBackColor = true;
            // 
            // watchVariablePanelDebug
            // 
            this._variablePanelDebug.DataPath = "Config/DebugData.xml";
            this._variablePanelDebug.Dock = System.Windows.Forms.DockStyle.Fill;
            this._variablePanelDebug.Location = new System.Drawing.Point(2, 2);
            this._variablePanelDebug.Margin = new System.Windows.Forms.Padding(0);
            this._variablePanelDebug.Name = "_variablePanelDebug";
            this._variablePanelDebug.Size = new System.Drawing.Size(520, 457);
            this._variablePanelDebug.TabIndex = 26;
            // 
            // DebugTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainerDebug);
            this.Name = "DebugTab";
            this.Size = new System.Drawing.Size(915, 463);
            this.splitContainerDebug.Panel1.ResumeLayout(false);
            this.splitContainerDebug.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerDebug)).EndInit();
            this.splitContainerDebug.ResumeLayout(false);
            this.panelDebugBorder.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDebug)).EndInit();
            this.groupBoxMiscDebug.ResumeLayout(false);
            this.groupBoxMiscDebug.PerformLayout();
            this.groupBoxResourceMeter.ResumeLayout(false);
            this.groupBoxResourceMeter.PerformLayout();
            this.groupBoxAdvancedMode.ResumeLayout(false);
            this.groupBoxAdvancedMode.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private BetterSplitContainer splitContainerDebug;
        private System.Windows.Forms.Panel panelDebugBorder;
        private Controls.IntPictureBox pictureBoxDebug;
        private System.Windows.Forms.GroupBox groupBoxMiscDebug;
        private System.Windows.Forms.CheckBox checkBoxFreeMovement;
        private System.Windows.Forms.CheckBox checkBoxSpawnMode;
        private System.Windows.Forms.CheckBox checkBoxStageSelect;
        private System.Windows.Forms.CheckBox checkBoxClassicMode;
        private System.Windows.Forms.GroupBox groupBoxResourceMeter;
        private System.Windows.Forms.RadioButton radioButtonResourceMeterOff;
        private System.Windows.Forms.RadioButton radioButtonResourceMeter1;
        private System.Windows.Forms.RadioButton radioButtonResourceMeter2;
        private System.Windows.Forms.GroupBox groupBoxAdvancedMode;
        private System.Windows.Forms.RadioButton radioButtonAdvancedModeOff;
        private System.Windows.Forms.RadioButton radioButtonAdvancedModeEnemyInfo;
        private System.Windows.Forms.RadioButton radioButtonAdvancedModeObjectCounter;
        private System.Windows.Forms.RadioButton radioButtonAdvancedModeEffectInfo;
        private System.Windows.Forms.RadioButton radioButtonAdvancedModeCheckInfo;
        private System.Windows.Forms.RadioButton radioButtonAdvancedModeStageInfo;
        private System.Windows.Forms.RadioButton radioButtonAdvancedModeMapInfo;
        private STROOP.Controls.VariablePanel.VariablePanel _variablePanelDebug;
    }
}
