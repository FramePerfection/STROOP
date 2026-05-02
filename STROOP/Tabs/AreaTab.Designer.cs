namespace STROOP.Tabs
{
    partial class AreaTab
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
            this.splitContainerArea = new STROOP.BetterSplitContainer();
            this.labelSelectedArea = new System.Windows.Forms.Label();
            this.radioButtonArea7 = new System.Windows.Forms.RadioButton();
            this.radioButtonArea4 = new System.Windows.Forms.RadioButton();
            this.radioButtonArea3 = new System.Windows.Forms.RadioButton();
            this.radioButtonArea5 = new System.Windows.Forms.RadioButton();
            this.radioButtonArea0 = new System.Windows.Forms.RadioButton();
            this.radioButtonArea6 = new System.Windows.Forms.RadioButton();
            this.radioButtonArea1 = new System.Windows.Forms.RadioButton();
            this.radioButtonArea2 = new System.Windows.Forms.RadioButton();
            this.checkBoxSelectCurrentArea = new System.Windows.Forms.CheckBox();
            this._variablePanelArea = new STROOP.Controls.VariablePanel.VariablePanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerArea)).BeginInit();
            this.splitContainerArea.Panel1.SuspendLayout();
            this.splitContainerArea.Panel2.SuspendLayout();
            this.splitContainerArea.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerArea
            // 
            this.splitContainerArea.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerArea.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerArea.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerArea.Location = new System.Drawing.Point(0, 0);
            this.splitContainerArea.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainerArea.Name = "splitContainerArea";
            // 
            // splitContainerArea.Panel1
            // 
            this.splitContainerArea.Panel1.AutoScroll = true;
            this.splitContainerArea.Panel1.Controls.Add(this.labelSelectedArea);
            this.splitContainerArea.Panel1.Controls.Add(this.radioButtonArea7);
            this.splitContainerArea.Panel1.Controls.Add(this.radioButtonArea4);
            this.splitContainerArea.Panel1.Controls.Add(this.radioButtonArea3);
            this.splitContainerArea.Panel1.Controls.Add(this.radioButtonArea5);
            this.splitContainerArea.Panel1.Controls.Add(this.radioButtonArea0);
            this.splitContainerArea.Panel1.Controls.Add(this.radioButtonArea6);
            this.splitContainerArea.Panel1.Controls.Add(this.radioButtonArea1);
            this.splitContainerArea.Panel1.Controls.Add(this.radioButtonArea2);
            this.splitContainerArea.Panel1.Controls.Add(this.checkBoxSelectCurrentArea);
            this.splitContainerArea.Panel1MinSize = 0;
            // 
            // splitContainerArea.Panel2
            // 
            this.splitContainerArea.Panel2.Controls.Add(this._variablePanelArea);
            this.splitContainerArea.Panel2.Padding = new System.Windows.Forms.Padding(2);
            this.splitContainerArea.Panel2MinSize = 0;
            this.splitContainerArea.Size = new System.Drawing.Size(915, 463);
            this.splitContainerArea.SplitterDistance = 153;
            this.splitContainerArea.SplitterWidth = 1;
            this.splitContainerArea.TabIndex = 34;
            // 
            // labelSelectedArea
            // 
            this.labelSelectedArea.AutoSize = true;
            this.labelSelectedArea.Location = new System.Drawing.Point(9, 10);
            this.labelSelectedArea.Name = "labelSelectedArea";
            this.labelSelectedArea.Size = new System.Drawing.Size(77, 13);
            this.labelSelectedArea.TabIndex = 13;
            this.labelSelectedArea.Text = "Selected Area:";
            // 
            // radioButtonArea7
            // 
            this.radioButtonArea7.AutoSize = true;
            this.radioButtonArea7.Location = new System.Drawing.Point(12, 192);
            this.radioButtonArea7.Name = "radioButtonArea7";
            this.radioButtonArea7.Size = new System.Drawing.Size(56, 17);
            this.radioButtonArea7.TabIndex = 3;
            this.radioButtonArea7.Text = "Area 7";
            this.radioButtonArea7.UseVisualStyleBackColor = true;
            // 
            // radioButtonArea4
            // 
            this.radioButtonArea4.AutoSize = true;
            this.radioButtonArea4.Location = new System.Drawing.Point(12, 123);
            this.radioButtonArea4.Name = "radioButtonArea4";
            this.radioButtonArea4.Size = new System.Drawing.Size(56, 17);
            this.radioButtonArea4.TabIndex = 0;
            this.radioButtonArea4.Text = "Area 4";
            this.radioButtonArea4.UseVisualStyleBackColor = true;
            // 
            // radioButtonArea3
            // 
            this.radioButtonArea3.AutoSize = true;
            this.radioButtonArea3.Location = new System.Drawing.Point(12, 100);
            this.radioButtonArea3.Name = "radioButtonArea3";
            this.radioButtonArea3.Size = new System.Drawing.Size(56, 17);
            this.radioButtonArea3.TabIndex = 3;
            this.radioButtonArea3.Text = "Area 3";
            this.radioButtonArea3.UseVisualStyleBackColor = true;
            // 
            // radioButtonArea5
            // 
            this.radioButtonArea5.AutoSize = true;
            this.radioButtonArea5.Location = new System.Drawing.Point(12, 146);
            this.radioButtonArea5.Name = "radioButtonArea5";
            this.radioButtonArea5.Size = new System.Drawing.Size(56, 17);
            this.radioButtonArea5.TabIndex = 1;
            this.radioButtonArea5.Text = "Area 5";
            this.radioButtonArea5.UseVisualStyleBackColor = true;
            // 
            // radioButtonArea0
            // 
            this.radioButtonArea0.AutoSize = true;
            this.radioButtonArea0.Checked = true;
            this.radioButtonArea0.Location = new System.Drawing.Point(12, 31);
            this.radioButtonArea0.Name = "radioButtonArea0";
            this.radioButtonArea0.Size = new System.Drawing.Size(56, 17);
            this.radioButtonArea0.TabIndex = 0;
            this.radioButtonArea0.TabStop = true;
            this.radioButtonArea0.Text = "Area 0";
            this.radioButtonArea0.UseVisualStyleBackColor = true;
            // 
            // radioButtonArea6
            // 
            this.radioButtonArea6.AutoSize = true;
            this.radioButtonArea6.Location = new System.Drawing.Point(12, 169);
            this.radioButtonArea6.Name = "radioButtonArea6";
            this.radioButtonArea6.Size = new System.Drawing.Size(56, 17);
            this.radioButtonArea6.TabIndex = 2;
            this.radioButtonArea6.Text = "Area 6";
            this.radioButtonArea6.UseVisualStyleBackColor = true;
            // 
            // radioButtonArea1
            // 
            this.radioButtonArea1.AutoSize = true;
            this.radioButtonArea1.Location = new System.Drawing.Point(12, 54);
            this.radioButtonArea1.Name = "radioButtonArea1";
            this.radioButtonArea1.Size = new System.Drawing.Size(56, 17);
            this.radioButtonArea1.TabIndex = 1;
            this.radioButtonArea1.Text = "Area 1";
            this.radioButtonArea1.UseVisualStyleBackColor = true;
            // 
            // radioButtonArea2
            // 
            this.radioButtonArea2.AutoSize = true;
            this.radioButtonArea2.Location = new System.Drawing.Point(12, 77);
            this.radioButtonArea2.Name = "radioButtonArea2";
            this.radioButtonArea2.Size = new System.Drawing.Size(56, 17);
            this.radioButtonArea2.TabIndex = 2;
            this.radioButtonArea2.Text = "Area 2";
            this.radioButtonArea2.UseVisualStyleBackColor = true;
            // 
            // checkBoxSelectCurrentArea
            // 
            this.checkBoxSelectCurrentArea.AutoSize = true;
            this.checkBoxSelectCurrentArea.Checked = true;
            this.checkBoxSelectCurrentArea.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSelectCurrentArea.Location = new System.Drawing.Point(12, 219);
            this.checkBoxSelectCurrentArea.Name = "checkBoxSelectCurrentArea";
            this.checkBoxSelectCurrentArea.Size = new System.Drawing.Size(118, 17);
            this.checkBoxSelectCurrentArea.TabIndex = 12;
            this.checkBoxSelectCurrentArea.Text = "Select Current Area";
            this.checkBoxSelectCurrentArea.UseVisualStyleBackColor = true;
            // 
            // watchVariablePanelArea
            // 
            this._variablePanelArea.DataPath = "Config/AreaData.xml";
            this._variablePanelArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this._variablePanelArea.Location = new System.Drawing.Point(2, 2);
            this._variablePanelArea.Margin = new System.Windows.Forms.Padding(0);
            this._variablePanelArea.Name = "_variablePanelArea";
            this._variablePanelArea.Size = new System.Drawing.Size(755, 457);
            this._variablePanelArea.TabIndex = 7;
            // 
            // AreaTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainerArea);
            this.Name = "AreaTab";
            this.Size = new System.Drawing.Size(915, 463);
            this.splitContainerArea.Panel1.ResumeLayout(false);
            this.splitContainerArea.Panel1.PerformLayout();
            this.splitContainerArea.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerArea)).EndInit();
            this.splitContainerArea.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private BetterSplitContainer splitContainerArea;
        private System.Windows.Forms.Label labelSelectedArea;
        private System.Windows.Forms.RadioButton radioButtonArea7;
        private System.Windows.Forms.RadioButton radioButtonArea4;
        private System.Windows.Forms.RadioButton radioButtonArea3;
        private System.Windows.Forms.RadioButton radioButtonArea5;
        private System.Windows.Forms.RadioButton radioButtonArea0;
        private System.Windows.Forms.RadioButton radioButtonArea6;
        private System.Windows.Forms.RadioButton radioButtonArea1;
        private System.Windows.Forms.RadioButton radioButtonArea2;
        private System.Windows.Forms.CheckBox checkBoxSelectCurrentArea;
        private STROOP.Controls.VariablePanel.VariablePanel _variablePanelArea;
    }
}
