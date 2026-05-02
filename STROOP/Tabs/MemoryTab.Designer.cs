namespace STROOP.Tabs
{
    partial class MemoryTab
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MemoryTab));
            this.splitContainerMemory = new STROOP.BetterSplitContainer();
            this.splitContainerMemoryControls = new STROOP.BetterSplitContainer();
            this.comboBoxMemoryTypes = new System.Windows.Forms.ComboBox();
            this.checkBoxMemoryObj = new System.Windows.Forms.CheckBox();
            this.labelMemoryMemorySize = new System.Windows.Forms.Label();
            this.labelMemoryBaseAddress = new System.Windows.Forms.Label();
            this.checkBoxMemoryHex = new System.Windows.Forms.CheckBox();
            this.textBoxMemoryMemorySize = new STROOP.BetterTextbox();
            this.textBoxMemoryBaseAddress = new STROOP.BetterTextbox();
            this.checkBoxMemoryRelativeAddresses = new System.Windows.Forms.CheckBox();
            this.checkBoxMemoryUseObjAddress = new System.Windows.Forms.CheckBox();
            this.buttonMemoryMoveUpContinuously = new System.Windows.Forms.Button();
            this.buttonMemoryMoveUpOnce = new System.Windows.Forms.Button();
            this.buttonMemoryMoveDownContinuously = new System.Windows.Forms.Button();
            this.checkBoxMemoryHighlightObjVars = new System.Windows.Forms.CheckBox();
            this.buttonMemoryPasteObject = new System.Windows.Forms.Button();
            this.buttonMemoryCopyObject = new System.Windows.Forms.Button();
            this.buttonMemoryMoveDownOnce = new System.Windows.Forms.Button();
            this.checkBoxMemoryUpdateContinuously = new System.Windows.Forms.CheckBox();
            this.checkBoxMemoryLittleEndian = new System.Windows.Forms.CheckBox();
            this.splitContainerMemoryControlsDisplays = new STROOP.BetterSplitContainer();
            this.richTextBoxMemoryAddresses = new STROOP.Controls.RichTextBoxEx();
            this.richTextBoxMemoryValues = new STROOP.Controls.RichTextBoxEx();
            this._variablePanelMemory = new STROOP.Controls.VariablePanel.VariablePanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMemory)).BeginInit();
            this.splitContainerMemory.Panel1.SuspendLayout();
            this.splitContainerMemory.Panel2.SuspendLayout();
            this.splitContainerMemory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMemoryControls)).BeginInit();
            this.splitContainerMemoryControls.Panel1.SuspendLayout();
            this.splitContainerMemoryControls.Panel2.SuspendLayout();
            this.splitContainerMemoryControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMemoryControlsDisplays)).BeginInit();
            this.splitContainerMemoryControlsDisplays.Panel1.SuspendLayout();
            this.splitContainerMemoryControlsDisplays.Panel2.SuspendLayout();
            this.splitContainerMemoryControlsDisplays.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerMemory
            // 
            this.splitContainerMemory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerMemory.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerMemory.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainerMemory.Location = new System.Drawing.Point(0, 0);
            this.splitContainerMemory.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainerMemory.Name = "splitContainerMemory";
            // 
            // splitContainerMemory.Panel1
            // 
            this.splitContainerMemory.Panel1.AutoScroll = true;
            this.splitContainerMemory.Panel1.Controls.Add(this.splitContainerMemoryControls);
            this.splitContainerMemory.Panel1MinSize = 0;
            // 
            // splitContainerMemory.Panel2
            // 
            this.splitContainerMemory.Panel2.Controls.Add(this._variablePanelMemory);
            this.splitContainerMemory.Panel2.Padding = new System.Windows.Forms.Padding(2);
            this.splitContainerMemory.Panel2MinSize = 0;
            this.splitContainerMemory.Size = new System.Drawing.Size(915, 463);
            this.splitContainerMemory.SplitterDistance = 740;
            this.splitContainerMemory.SplitterWidth = 1;
            this.splitContainerMemory.TabIndex = 38;
            // 
            // splitContainerMemoryControls
            // 
            this.splitContainerMemoryControls.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerMemoryControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMemoryControls.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerMemoryControls.Location = new System.Drawing.Point(0, 0);
            this.splitContainerMemoryControls.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainerMemoryControls.Name = "splitContainerMemoryControls";
            this.splitContainerMemoryControls.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerMemoryControls.Panel1
            // 
            this.splitContainerMemoryControls.Panel1.AutoScroll = true;
            this.splitContainerMemoryControls.Panel1.Controls.Add(this.comboBoxMemoryTypes);
            this.splitContainerMemoryControls.Panel1.Controls.Add(this.checkBoxMemoryObj);
            this.splitContainerMemoryControls.Panel1.Controls.Add(this.labelMemoryMemorySize);
            this.splitContainerMemoryControls.Panel1.Controls.Add(this.labelMemoryBaseAddress);
            this.splitContainerMemoryControls.Panel1.Controls.Add(this.checkBoxMemoryHex);
            this.splitContainerMemoryControls.Panel1.Controls.Add(this.textBoxMemoryMemorySize);
            this.splitContainerMemoryControls.Panel1.Controls.Add(this.textBoxMemoryBaseAddress);
            this.splitContainerMemoryControls.Panel1.Controls.Add(this.checkBoxMemoryRelativeAddresses);
            this.splitContainerMemoryControls.Panel1.Controls.Add(this.checkBoxMemoryUseObjAddress);
            this.splitContainerMemoryControls.Panel1.Controls.Add(this.buttonMemoryMoveUpContinuously);
            this.splitContainerMemoryControls.Panel1.Controls.Add(this.buttonMemoryMoveUpOnce);
            this.splitContainerMemoryControls.Panel1.Controls.Add(this.buttonMemoryMoveDownContinuously);
            this.splitContainerMemoryControls.Panel1.Controls.Add(this.checkBoxMemoryHighlightObjVars);
            this.splitContainerMemoryControls.Panel1.Controls.Add(this.buttonMemoryPasteObject);
            this.splitContainerMemoryControls.Panel1.Controls.Add(this.buttonMemoryCopyObject);
            this.splitContainerMemoryControls.Panel1.Controls.Add(this.buttonMemoryMoveDownOnce);
            this.splitContainerMemoryControls.Panel1.Controls.Add(this.checkBoxMemoryUpdateContinuously);
            this.splitContainerMemoryControls.Panel1.Controls.Add(this.checkBoxMemoryLittleEndian);
            this.splitContainerMemoryControls.Panel1MinSize = 0;
            // 
            // splitContainerMemoryControls.Panel2
            // 
            this.splitContainerMemoryControls.Panel2.Controls.Add(this.splitContainerMemoryControlsDisplays);
            this.splitContainerMemoryControls.Panel2MinSize = 0;
            this.splitContainerMemoryControls.Size = new System.Drawing.Size(740, 463);
            this.splitContainerMemoryControls.SplitterDistance = 55;
            this.splitContainerMemoryControls.SplitterWidth = 1;
            this.splitContainerMemoryControls.TabIndex = 38;
            // 
            // comboBoxMemoryTypes
            // 
            this.comboBoxMemoryTypes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMemoryTypes.Location = new System.Drawing.Point(527, 3);
            this.comboBoxMemoryTypes.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxMemoryTypes.Name = "comboBoxMemoryTypes";
            this.comboBoxMemoryTypes.Size = new System.Drawing.Size(55, 21);
            this.comboBoxMemoryTypes.TabIndex = 36;
            // 
            // checkBoxMemoryObj
            // 
            this.checkBoxMemoryObj.AutoSize = true;
            this.checkBoxMemoryObj.Location = new System.Drawing.Point(573, 32);
            this.checkBoxMemoryObj.Name = "checkBoxMemoryObj";
            this.checkBoxMemoryObj.Size = new System.Drawing.Size(42, 17);
            this.checkBoxMemoryObj.TabIndex = 35;
            this.checkBoxMemoryObj.Text = "Obj";
            this.checkBoxMemoryObj.UseVisualStyleBackColor = true;
            // 
            // labelMemoryMemorySize
            // 
            this.labelMemoryMemorySize.AutoSize = true;
            this.labelMemoryMemorySize.Location = new System.Drawing.Point(4, 33);
            this.labelMemoryMemorySize.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelMemoryMemorySize.Name = "labelMemoryMemorySize";
            this.labelMemoryMemorySize.Size = new System.Drawing.Size(70, 13);
            this.labelMemoryMemorySize.TabIndex = 9;
            this.labelMemoryMemorySize.Text = "Memory Size:";
            // 
            // labelMemoryBaseAddress
            // 
            this.labelMemoryBaseAddress.AutoSize = true;
            this.labelMemoryBaseAddress.Location = new System.Drawing.Point(4, 7);
            this.labelMemoryBaseAddress.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelMemoryBaseAddress.Name = "labelMemoryBaseAddress";
            this.labelMemoryBaseAddress.Size = new System.Drawing.Size(75, 13);
            this.labelMemoryBaseAddress.TabIndex = 9;
            this.labelMemoryBaseAddress.Text = "Base Address:";
            // 
            // checkBoxMemoryHex
            // 
            this.checkBoxMemoryHex.AutoSize = true;
            this.checkBoxMemoryHex.Checked = true;
            this.checkBoxMemoryHex.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxMemoryHex.Location = new System.Drawing.Point(527, 32);
            this.checkBoxMemoryHex.Name = "checkBoxMemoryHex";
            this.checkBoxMemoryHex.Size = new System.Drawing.Size(45, 17);
            this.checkBoxMemoryHex.TabIndex = 35;
            this.checkBoxMemoryHex.Text = "Hex";
            this.checkBoxMemoryHex.UseVisualStyleBackColor = true;
            // 
            // textBoxMemoryMemorySize
            // 
            this.textBoxMemoryMemorySize.Location = new System.Drawing.Point(80, 30);
            this.textBoxMemoryMemorySize.Name = "textBoxMemoryMemorySize";
            this.textBoxMemoryMemorySize.Size = new System.Drawing.Size(84, 20);
            this.textBoxMemoryMemorySize.TabIndex = 34;
            this.textBoxMemoryMemorySize.Text = "0x260";
            this.textBoxMemoryMemorySize.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBoxMemoryBaseAddress
            // 
            this.textBoxMemoryBaseAddress.Location = new System.Drawing.Point(80, 4);
            this.textBoxMemoryBaseAddress.Name = "textBoxMemoryBaseAddress";
            this.textBoxMemoryBaseAddress.Size = new System.Drawing.Size(84, 20);
            this.textBoxMemoryBaseAddress.TabIndex = 34;
            this.textBoxMemoryBaseAddress.Text = "0x00000000";
            this.textBoxMemoryBaseAddress.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // checkBoxMemoryRelativeAddresses
            // 
            this.checkBoxMemoryRelativeAddresses.AutoSize = true;
            this.checkBoxMemoryRelativeAddresses.Checked = true;
            this.checkBoxMemoryRelativeAddresses.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxMemoryRelativeAddresses.Location = new System.Drawing.Point(170, 32);
            this.checkBoxMemoryRelativeAddresses.Name = "checkBoxMemoryRelativeAddresses";
            this.checkBoxMemoryRelativeAddresses.Size = new System.Drawing.Size(94, 17);
            this.checkBoxMemoryRelativeAddresses.TabIndex = 35;
            this.checkBoxMemoryRelativeAddresses.Text = "Rel Addresses";
            this.checkBoxMemoryRelativeAddresses.UseVisualStyleBackColor = true;
            // 
            // checkBoxMemoryUseObjAddress
            // 
            this.checkBoxMemoryUseObjAddress.AutoSize = true;
            this.checkBoxMemoryUseObjAddress.Checked = true;
            this.checkBoxMemoryUseObjAddress.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxMemoryUseObjAddress.Location = new System.Drawing.Point(401, 6);
            this.checkBoxMemoryUseObjAddress.Name = "checkBoxMemoryUseObjAddress";
            this.checkBoxMemoryUseObjAddress.Size = new System.Drawing.Size(105, 17);
            this.checkBoxMemoryUseObjAddress.TabIndex = 35;
            this.checkBoxMemoryUseObjAddress.Text = "Use Obj Address";
            this.checkBoxMemoryUseObjAddress.UseVisualStyleBackColor = true;
            // 
            // buttonMemoryMoveUpContinuously
            // 
            this.buttonMemoryMoveUpContinuously.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonMemoryMoveUpContinuously.BackgroundImage")));
            this.buttonMemoryMoveUpContinuously.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonMemoryMoveUpContinuously.Location = new System.Drawing.Point(641, 3);
            this.buttonMemoryMoveUpContinuously.Margin = new System.Windows.Forms.Padding(2);
            this.buttonMemoryMoveUpContinuously.Name = "buttonMemoryMoveUpContinuously";
            this.buttonMemoryMoveUpContinuously.Size = new System.Drawing.Size(21, 21);
            this.buttonMemoryMoveUpContinuously.TabIndex = 20;
            this.buttonMemoryMoveUpContinuously.UseVisualStyleBackColor = true;
            // 
            // buttonMemoryMoveUpOnce
            // 
            this.buttonMemoryMoveUpOnce.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonMemoryMoveUpOnce.BackgroundImage")));
            this.buttonMemoryMoveUpOnce.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonMemoryMoveUpOnce.Location = new System.Drawing.Point(617, 3);
            this.buttonMemoryMoveUpOnce.Margin = new System.Windows.Forms.Padding(2);
            this.buttonMemoryMoveUpOnce.Name = "buttonMemoryMoveUpOnce";
            this.buttonMemoryMoveUpOnce.Size = new System.Drawing.Size(21, 21);
            this.buttonMemoryMoveUpOnce.TabIndex = 20;
            this.buttonMemoryMoveUpOnce.UseVisualStyleBackColor = true;
            // 
            // buttonMemoryMoveDownContinuously
            // 
            this.buttonMemoryMoveDownContinuously.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonMemoryMoveDownContinuously.BackgroundImage")));
            this.buttonMemoryMoveDownContinuously.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonMemoryMoveDownContinuously.Location = new System.Drawing.Point(641, 29);
            this.buttonMemoryMoveDownContinuously.Margin = new System.Windows.Forms.Padding(2);
            this.buttonMemoryMoveDownContinuously.Name = "buttonMemoryMoveDownContinuously";
            this.buttonMemoryMoveDownContinuously.Size = new System.Drawing.Size(21, 21);
            this.buttonMemoryMoveDownContinuously.TabIndex = 20;
            this.buttonMemoryMoveDownContinuously.UseVisualStyleBackColor = true;
            // 
            // checkBoxMemoryHighlightObjVars
            // 
            this.checkBoxMemoryHighlightObjVars.AutoSize = true;
            this.checkBoxMemoryHighlightObjVars.Checked = true;
            this.checkBoxMemoryHighlightObjVars.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxMemoryHighlightObjVars.Location = new System.Drawing.Point(401, 32);
            this.checkBoxMemoryHighlightObjVars.Name = "checkBoxMemoryHighlightObjVars";
            this.checkBoxMemoryHighlightObjVars.Size = new System.Drawing.Size(110, 17);
            this.checkBoxMemoryHighlightObjVars.TabIndex = 35;
            this.checkBoxMemoryHighlightObjVars.Text = "Highlight Obj Vars";
            this.checkBoxMemoryHighlightObjVars.UseVisualStyleBackColor = true;
            // 
            // buttonMemoryPasteObject
            // 
            this.buttonMemoryPasteObject.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonMemoryPasteObject.Location = new System.Drawing.Point(325, 29);
            this.buttonMemoryPasteObject.Margin = new System.Windows.Forms.Padding(2);
            this.buttonMemoryPasteObject.Name = "buttonMemoryPasteObject";
            this.buttonMemoryPasteObject.Size = new System.Drawing.Size(61, 21);
            this.buttonMemoryPasteObject.TabIndex = 20;
            this.buttonMemoryPasteObject.Text = "Paste Obj";
            this.buttonMemoryPasteObject.UseVisualStyleBackColor = true;
            // 
            // buttonMemoryCopyObject
            // 
            this.buttonMemoryCopyObject.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonMemoryCopyObject.Location = new System.Drawing.Point(266, 29);
            this.buttonMemoryCopyObject.Margin = new System.Windows.Forms.Padding(2);
            this.buttonMemoryCopyObject.Name = "buttonMemoryCopyObject";
            this.buttonMemoryCopyObject.Size = new System.Drawing.Size(58, 21);
            this.buttonMemoryCopyObject.TabIndex = 20;
            this.buttonMemoryCopyObject.Text = "Copy Obj";
            this.buttonMemoryCopyObject.UseVisualStyleBackColor = true;
            // 
            // buttonMemoryMoveDownOnce
            // 
            this.buttonMemoryMoveDownOnce.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonMemoryMoveDownOnce.BackgroundImage")));
            this.buttonMemoryMoveDownOnce.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonMemoryMoveDownOnce.Location = new System.Drawing.Point(617, 29);
            this.buttonMemoryMoveDownOnce.Margin = new System.Windows.Forms.Padding(2);
            this.buttonMemoryMoveDownOnce.Name = "buttonMemoryMoveDownOnce";
            this.buttonMemoryMoveDownOnce.Size = new System.Drawing.Size(21, 21);
            this.buttonMemoryMoveDownOnce.TabIndex = 20;
            this.buttonMemoryMoveDownOnce.UseVisualStyleBackColor = true;
            // 
            // checkBoxMemoryUpdateContinuously
            // 
            this.checkBoxMemoryUpdateContinuously.AutoSize = true;
            this.checkBoxMemoryUpdateContinuously.Checked = true;
            this.checkBoxMemoryUpdateContinuously.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxMemoryUpdateContinuously.Location = new System.Drawing.Point(267, 6);
            this.checkBoxMemoryUpdateContinuously.Name = "checkBoxMemoryUpdateContinuously";
            this.checkBoxMemoryUpdateContinuously.Size = new System.Drawing.Size(124, 17);
            this.checkBoxMemoryUpdateContinuously.TabIndex = 35;
            this.checkBoxMemoryUpdateContinuously.Text = "Update Continuously";
            this.checkBoxMemoryUpdateContinuously.UseVisualStyleBackColor = true;
            // 
            // checkBoxMemoryLittleEndian
            // 
            this.checkBoxMemoryLittleEndian.AutoSize = true;
            this.checkBoxMemoryLittleEndian.Location = new System.Drawing.Point(170, 6);
            this.checkBoxMemoryLittleEndian.Name = "checkBoxMemoryLittleEndian";
            this.checkBoxMemoryLittleEndian.Size = new System.Drawing.Size(84, 17);
            this.checkBoxMemoryLittleEndian.TabIndex = 35;
            this.checkBoxMemoryLittleEndian.Text = "Little Endian";
            this.checkBoxMemoryLittleEndian.UseVisualStyleBackColor = true;
            // 
            // splitContainerMemoryControlsDisplays
            // 
            this.splitContainerMemoryControlsDisplays.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerMemoryControlsDisplays.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMemoryControlsDisplays.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerMemoryControlsDisplays.Location = new System.Drawing.Point(0, 0);
            this.splitContainerMemoryControlsDisplays.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainerMemoryControlsDisplays.Name = "splitContainerMemoryControlsDisplays";
            // 
            // splitContainerMemoryControlsDisplays.Panel1
            // 
            this.splitContainerMemoryControlsDisplays.Panel1.AutoScroll = true;
            this.splitContainerMemoryControlsDisplays.Panel1.Controls.Add(this.richTextBoxMemoryAddresses);
            this.splitContainerMemoryControlsDisplays.Panel1MinSize = 0;
            // 
            // splitContainerMemoryControlsDisplays.Panel2
            // 
            this.splitContainerMemoryControlsDisplays.Panel2.Controls.Add(this.richTextBoxMemoryValues);
            this.splitContainerMemoryControlsDisplays.Panel2MinSize = 0;
            this.splitContainerMemoryControlsDisplays.Size = new System.Drawing.Size(740, 407);
            this.splitContainerMemoryControlsDisplays.SplitterDistance = 98;
            this.splitContainerMemoryControlsDisplays.SplitterWidth = 1;
            this.splitContainerMemoryControlsDisplays.TabIndex = 39;
            // 
            // richTextBoxMemoryAddresses
            // 
            this.richTextBoxMemoryAddresses.BackColor = System.Drawing.SystemColors.Control;
            this.richTextBoxMemoryAddresses.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxMemoryAddresses.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxMemoryAddresses.Font = new System.Drawing.Font("Courier New", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxMemoryAddresses.Location = new System.Drawing.Point(0, 0);
            this.richTextBoxMemoryAddresses.Margin = new System.Windows.Forms.Padding(2);
            this.richTextBoxMemoryAddresses.Name = "richTextBoxMemoryAddresses";
            this.richTextBoxMemoryAddresses.ReadOnly = true;
            this.richTextBoxMemoryAddresses.Size = new System.Drawing.Size(96, 405);
            this.richTextBoxMemoryAddresses.TabIndex = 8;
            this.richTextBoxMemoryAddresses.Text = "";
            // 
            // richTextBoxMemoryValues
            // 
            this.richTextBoxMemoryValues.BackColor = System.Drawing.SystemColors.Control;
            this.richTextBoxMemoryValues.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxMemoryValues.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxMemoryValues.Font = new System.Drawing.Font("Courier New", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxMemoryValues.Location = new System.Drawing.Point(0, 0);
            this.richTextBoxMemoryValues.Margin = new System.Windows.Forms.Padding(2);
            this.richTextBoxMemoryValues.Name = "richTextBoxMemoryValues";
            this.richTextBoxMemoryValues.ReadOnly = true;
            this.richTextBoxMemoryValues.Size = new System.Drawing.Size(639, 405);
            this.richTextBoxMemoryValues.TabIndex = 8;
            this.richTextBoxMemoryValues.Text = "";
            // 
            // watchVariablePanelMemory
            // 
            this._variablePanelMemory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._variablePanelMemory.DataPath = "Config/ObjectData.xml";
            this._variablePanelMemory.Location = new System.Drawing.Point(2, 2);
            this._variablePanelMemory.Margin = new System.Windows.Forms.Padding(0);
            this._variablePanelMemory.Name = "_variablePanelMemory";
            this._variablePanelMemory.Size = new System.Drawing.Size(168, 457);
            this._variablePanelMemory.TabIndex = 7;
            // 
            // MemoryTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainerMemory);
            this.Name = "MemoryTab";
            this.splitContainerMemory.Panel1.ResumeLayout(false);
            this.splitContainerMemory.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMemory)).EndInit();
            this.splitContainerMemory.ResumeLayout(false);
            this.splitContainerMemoryControls.Panel1.ResumeLayout(false);
            this.splitContainerMemoryControls.Panel1.PerformLayout();
            this.splitContainerMemoryControls.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMemoryControls)).EndInit();
            this.splitContainerMemoryControls.ResumeLayout(false);
            this.splitContainerMemoryControlsDisplays.Panel1.ResumeLayout(false);
            this.splitContainerMemoryControlsDisplays.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMemoryControlsDisplays)).EndInit();
            this.splitContainerMemoryControlsDisplays.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private BetterSplitContainer splitContainerMemory;
        private BetterSplitContainer splitContainerMemoryControls;
        private System.Windows.Forms.ComboBox comboBoxMemoryTypes;
        private System.Windows.Forms.CheckBox checkBoxMemoryObj;
        private System.Windows.Forms.Label labelMemoryMemorySize;
        private System.Windows.Forms.Label labelMemoryBaseAddress;
        private System.Windows.Forms.CheckBox checkBoxMemoryHex;
        private BetterTextbox textBoxMemoryMemorySize;
        private BetterTextbox textBoxMemoryBaseAddress;
        private System.Windows.Forms.CheckBox checkBoxMemoryRelativeAddresses;
        private System.Windows.Forms.CheckBox checkBoxMemoryUseObjAddress;
        private System.Windows.Forms.Button buttonMemoryMoveUpContinuously;
        private System.Windows.Forms.Button buttonMemoryMoveUpOnce;
        private System.Windows.Forms.Button buttonMemoryMoveDownContinuously;
        private System.Windows.Forms.CheckBox checkBoxMemoryHighlightObjVars;
        private System.Windows.Forms.Button buttonMemoryPasteObject;
        private System.Windows.Forms.Button buttonMemoryCopyObject;
        private System.Windows.Forms.Button buttonMemoryMoveDownOnce;
        private System.Windows.Forms.CheckBox checkBoxMemoryUpdateContinuously;
        private System.Windows.Forms.CheckBox checkBoxMemoryLittleEndian;
        private BetterSplitContainer splitContainerMemoryControlsDisplays;
        private Controls.RichTextBoxEx richTextBoxMemoryAddresses;
        private Controls.RichTextBoxEx richTextBoxMemoryValues;
        private STROOP.Controls.VariablePanel.VariablePanel _variablePanelMemory;
    }
}
