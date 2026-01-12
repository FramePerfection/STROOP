namespace STROOP.Tabs
{
    partial class SearchTab
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.splitContainerSearch = new STROOP.BetterSplitContainer();
            this.splitContainerSearchOptions = new STROOP.BetterSplitContainer();
            this.labelSearchProgress = new System.Windows.Forms.Label();
            this.progressBarSearch = new System.Windows.Forms.ProgressBar();
            this.buttonSearchAddAllAsVars = new System.Windows.Forms.Button();
            this.buttonSearchAddSelectedAsVars = new System.Windows.Forms.Button();
            this.labelSearchNumResults = new System.Windows.Forms.Label();
            this.comboBoxSearchValueRelationship = new System.Windows.Forms.ComboBox();
            this.comboBoxSearchMemoryType = new System.Windows.Forms.ComboBox();
            this.textBoxSearchValue = new STROOP.BetterTextbox();
            this.buttonSearchUndoScan = new System.Windows.Forms.Button();
            this.buttonSearchClearResults = new System.Windows.Forms.Button();
            this.buttonSearchNextScan = new System.Windows.Forms.Button();
            this.buttonSearchFirstScan = new System.Windows.Forms.Button();
            this.dataGridViewSearch = new System.Windows.Forms.DataGridView();
            this.Address = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._variablePanelSearch = new STROOP.Controls.VariablePanel.VariablePanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerSearch)).BeginInit();
            this.splitContainerSearch.Panel1.SuspendLayout();
            this.splitContainerSearch.Panel2.SuspendLayout();
            this.splitContainerSearch.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerSearchOptions)).BeginInit();
            this.splitContainerSearchOptions.Panel1.SuspendLayout();
            this.splitContainerSearchOptions.Panel2.SuspendLayout();
            this.splitContainerSearchOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSearch)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainerSearch
            // 
            this.splitContainerSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerSearch.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerSearch.Location = new System.Drawing.Point(0, 0);
            this.splitContainerSearch.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainerSearch.Name = "splitContainerSearch";
            // 
            // splitContainerSearch.Panel1
            // 
            this.splitContainerSearch.Panel1.AutoScroll = true;
            this.splitContainerSearch.Panel1.Controls.Add(this.splitContainerSearchOptions);
            this.splitContainerSearch.Panel1MinSize = 0;
            // 
            // splitContainerSearch.Panel2
            // 
            this.splitContainerSearch.Panel2.Controls.Add(this._variablePanelSearch);
            this.splitContainerSearch.Panel2.Padding = new System.Windows.Forms.Padding(2);
            this.splitContainerSearch.Panel2MinSize = 0;
            this.splitContainerSearch.Size = new System.Drawing.Size(915, 463);
            this.splitContainerSearch.SplitterDistance = 457;
            this.splitContainerSearch.SplitterWidth = 1;
            this.splitContainerSearch.TabIndex = 39;
            // 
            // splitContainerSearchOptions
            // 
            this.splitContainerSearchOptions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerSearchOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerSearchOptions.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerSearchOptions.Location = new System.Drawing.Point(0, 0);
            this.splitContainerSearchOptions.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainerSearchOptions.Name = "splitContainerSearchOptions";
            this.splitContainerSearchOptions.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerSearchOptions.Panel1
            // 
            this.splitContainerSearchOptions.Panel1.AutoScroll = true;
            this.splitContainerSearchOptions.Panel1.Controls.Add(this.labelSearchProgress);
            this.splitContainerSearchOptions.Panel1.Controls.Add(this.progressBarSearch);
            this.splitContainerSearchOptions.Panel1.Controls.Add(this.buttonSearchAddAllAsVars);
            this.splitContainerSearchOptions.Panel1.Controls.Add(this.buttonSearchAddSelectedAsVars);
            this.splitContainerSearchOptions.Panel1.Controls.Add(this.labelSearchNumResults);
            this.splitContainerSearchOptions.Panel1.Controls.Add(this.comboBoxSearchValueRelationship);
            this.splitContainerSearchOptions.Panel1.Controls.Add(this.comboBoxSearchMemoryType);
            this.splitContainerSearchOptions.Panel1.Controls.Add(this.textBoxSearchValue);
            this.splitContainerSearchOptions.Panel1.Controls.Add(this.buttonSearchUndoScan);
            this.splitContainerSearchOptions.Panel1.Controls.Add(this.buttonSearchClearResults);
            this.splitContainerSearchOptions.Panel1.Controls.Add(this.buttonSearchNextScan);
            this.splitContainerSearchOptions.Panel1.Controls.Add(this.buttonSearchFirstScan);
            this.splitContainerSearchOptions.Panel1MinSize = 0;
            // 
            // splitContainerSearchOptions.Panel2
            // 
            this.splitContainerSearchOptions.Panel2.Controls.Add(this.dataGridViewSearch);
            this.splitContainerSearchOptions.Panel2MinSize = 0;
            this.splitContainerSearchOptions.Size = new System.Drawing.Size(457, 463);
            this.splitContainerSearchOptions.SplitterDistance = 108;
            this.splitContainerSearchOptions.SplitterWidth = 1;
            this.splitContainerSearchOptions.TabIndex = 38;
            // 
            // labelSearchProgress
            // 
            this.labelSearchProgress.AutoSize = true;
            this.labelSearchProgress.Location = new System.Drawing.Point(174, 85);
            this.labelSearchProgress.MinimumSize = new System.Drawing.Size(80, 2);
            this.labelSearchProgress.Name = "labelSearchProgress";
            this.labelSearchProgress.Size = new System.Drawing.Size(80, 13);
            this.labelSearchProgress.TabIndex = 37;
            this.labelSearchProgress.Text = "0 Results";
            // 
            // progressBarSearch
            // 
            this.progressBarSearch.Location = new System.Drawing.Point(2, 77);
            this.progressBarSearch.Margin = new System.Windows.Forms.Padding(2);
            this.progressBarSearch.Name = "progressBarSearch";
            this.progressBarSearch.Size = new System.Drawing.Size(451, 27);
            this.progressBarSearch.TabIndex = 42;
            // 
            // buttonSearchAddAllAsVars
            // 
            this.buttonSearchAddAllAsVars.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonSearchAddAllAsVars.Location = new System.Drawing.Point(336, 1);
            this.buttonSearchAddAllAsVars.Margin = new System.Windows.Forms.Padding(2);
            this.buttonSearchAddAllAsVars.Name = "buttonSearchAddAllAsVars";
            this.buttonSearchAddAllAsVars.Size = new System.Drawing.Size(118, 34);
            this.buttonSearchAddAllAsVars.TabIndex = 38;
            this.buttonSearchAddAllAsVars.Text = "Add All as Vars";
            this.buttonSearchAddAllAsVars.UseVisualStyleBackColor = true;
            // 
            // buttonSearchAddSelectedAsVars
            // 
            this.buttonSearchAddSelectedAsVars.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonSearchAddSelectedAsVars.Location = new System.Drawing.Point(336, 39);
            this.buttonSearchAddSelectedAsVars.Margin = new System.Windows.Forms.Padding(2);
            this.buttonSearchAddSelectedAsVars.Name = "buttonSearchAddSelectedAsVars";
            this.buttonSearchAddSelectedAsVars.Size = new System.Drawing.Size(118, 34);
            this.buttonSearchAddSelectedAsVars.TabIndex = 38;
            this.buttonSearchAddSelectedAsVars.Text = "Add Selected as Vars";
            this.buttonSearchAddSelectedAsVars.UseVisualStyleBackColor = true;
            // 
            // labelSearchNumResults
            // 
            this.labelSearchNumResults.AutoSize = true;
            this.labelSearchNumResults.Location = new System.Drawing.Point(156, 56);
            this.labelSearchNumResults.MinimumSize = new System.Drawing.Size(80, 2);
            this.labelSearchNumResults.Name = "labelSearchNumResults";
            this.labelSearchNumResults.Size = new System.Drawing.Size(80, 13);
            this.labelSearchNumResults.TabIndex = 37;
            this.labelSearchNumResults.Text = "0 Results";
            // 
            // comboBoxSearchValueRelationship
            // 
            this.comboBoxSearchValueRelationship.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSearchValueRelationship.Location = new System.Drawing.Point(2, 27);
            this.comboBoxSearchValueRelationship.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxSearchValueRelationship.Name = "comboBoxSearchValueRelationship";
            this.comboBoxSearchValueRelationship.Size = new System.Drawing.Size(148, 21);
            this.comboBoxSearchValueRelationship.TabIndex = 36;
            // 
            // comboBoxSearchMemoryType
            // 
            this.comboBoxSearchMemoryType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSearchMemoryType.Location = new System.Drawing.Point(2, 2);
            this.comboBoxSearchMemoryType.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxSearchMemoryType.Name = "comboBoxSearchMemoryType";
            this.comboBoxSearchMemoryType.Size = new System.Drawing.Size(148, 21);
            this.comboBoxSearchMemoryType.TabIndex = 36;
            // 
            // textBoxSearchValue
            // 
            this.textBoxSearchValue.Location = new System.Drawing.Point(2, 53);
            this.textBoxSearchValue.Name = "textBoxSearchValue";
            this.textBoxSearchValue.Size = new System.Drawing.Size(148, 20);
            this.textBoxSearchValue.TabIndex = 34;
            this.textBoxSearchValue.Text = "100";
            this.textBoxSearchValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // buttonSearchUndoScan
            // 
            this.buttonSearchUndoScan.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonSearchUndoScan.Location = new System.Drawing.Point(155, 27);
            this.buttonSearchUndoScan.Margin = new System.Windows.Forms.Padding(2);
            this.buttonSearchUndoScan.Name = "buttonSearchUndoScan";
            this.buttonSearchUndoScan.Size = new System.Drawing.Size(86, 21);
            this.buttonSearchUndoScan.TabIndex = 20;
            this.buttonSearchUndoScan.Text = "Undo Scan";
            this.buttonSearchUndoScan.UseVisualStyleBackColor = true;
            // 
            // buttonSearchClearResults
            // 
            this.buttonSearchClearResults.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonSearchClearResults.Location = new System.Drawing.Point(245, 27);
            this.buttonSearchClearResults.Margin = new System.Windows.Forms.Padding(2);
            this.buttonSearchClearResults.Name = "buttonSearchClearResults";
            this.buttonSearchClearResults.Size = new System.Drawing.Size(86, 21);
            this.buttonSearchClearResults.TabIndex = 20;
            this.buttonSearchClearResults.Text = "Clear Results";
            this.buttonSearchClearResults.UseVisualStyleBackColor = true;
            // 
            // buttonSearchNextScan
            // 
            this.buttonSearchNextScan.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonSearchNextScan.Location = new System.Drawing.Point(245, 2);
            this.buttonSearchNextScan.Margin = new System.Windows.Forms.Padding(2);
            this.buttonSearchNextScan.Name = "buttonSearchNextScan";
            this.buttonSearchNextScan.Size = new System.Drawing.Size(86, 21);
            this.buttonSearchNextScan.TabIndex = 20;
            this.buttonSearchNextScan.Text = "Next Scan";
            this.buttonSearchNextScan.UseVisualStyleBackColor = true;
            // 
            // buttonSearchFirstScan
            // 
            this.buttonSearchFirstScan.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonSearchFirstScan.Location = new System.Drawing.Point(155, 2);
            this.buttonSearchFirstScan.Margin = new System.Windows.Forms.Padding(2);
            this.buttonSearchFirstScan.Name = "buttonSearchFirstScan";
            this.buttonSearchFirstScan.Size = new System.Drawing.Size(86, 21);
            this.buttonSearchFirstScan.TabIndex = 20;
            this.buttonSearchFirstScan.Text = "First Scan";
            this.buttonSearchFirstScan.UseVisualStyleBackColor = true;
            // 
            // dataGridViewSearch
            // 
            this.dataGridViewSearch.AllowUserToAddRows = false;
            this.dataGridViewSearch.AllowUserToDeleteRows = false;
            this.dataGridViewSearch.AllowUserToOrderColumns = true;
            this.dataGridViewSearch.AllowUserToResizeRows = false;
            this.dataGridViewSearch.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewSearch.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewSearch.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewSearch.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Address,
            this.Value});
            this.dataGridViewSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewSearch.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewSearch.Margin = new System.Windows.Forms.Padding(0);
            this.dataGridViewSearch.Name = "dataGridViewSearch";
            this.dataGridViewSearch.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewSearch.Size = new System.Drawing.Size(455, 352);
            this.dataGridViewSearch.TabIndex = 4;
            // 
            // Address
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Address.DefaultCellStyle = dataGridViewCellStyle2;
            this.Address.HeaderText = "Address";
            this.Address.MinimumWidth = 2;
            this.Address.Name = "Address";
            this.Address.ReadOnly = true;
            // 
            // Value
            // 
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Value.DefaultCellStyle = dataGridViewCellStyle3;
            this.Value.HeaderText = "Value";
            this.Value.MinimumWidth = 2;
            this.Value.Name = "Value";
            this.Value.ReadOnly = true;
            // 
            // watchVariablePanelSearch
            // 
            this._variablePanelSearch.DataPath = null;
            this._variablePanelSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this._variablePanelSearch.Location = new System.Drawing.Point(2, 2);
            this._variablePanelSearch.Margin = new System.Windows.Forms.Padding(0);
            this._variablePanelSearch.Name = "_variablePanelSearch";
            this._variablePanelSearch.Size = new System.Drawing.Size(451, 457);
            this._variablePanelSearch.TabIndex = 7;
            // 
            // SearchTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainerSearch);
            this.Name = "SearchTab";
            this.Size = new System.Drawing.Size(915, 463);
            this.splitContainerSearch.Panel1.ResumeLayout(false);
            this.splitContainerSearch.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerSearch)).EndInit();
            this.splitContainerSearch.ResumeLayout(false);
            this.splitContainerSearchOptions.Panel1.ResumeLayout(false);
            this.splitContainerSearchOptions.Panel1.PerformLayout();
            this.splitContainerSearchOptions.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerSearchOptions)).EndInit();
            this.splitContainerSearchOptions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSearch)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private BetterSplitContainer splitContainerSearch;
        private BetterSplitContainer splitContainerSearchOptions;
        private System.Windows.Forms.Label labelSearchProgress;
        private System.Windows.Forms.ProgressBar progressBarSearch;
        private System.Windows.Forms.Button buttonSearchAddAllAsVars;
        private System.Windows.Forms.Button buttonSearchAddSelectedAsVars;
        private System.Windows.Forms.Label labelSearchNumResults;
        private System.Windows.Forms.ComboBox comboBoxSearchValueRelationship;
        private System.Windows.Forms.ComboBox comboBoxSearchMemoryType;
        private BetterTextbox textBoxSearchValue;
        private System.Windows.Forms.Button buttonSearchUndoScan;
        private System.Windows.Forms.Button buttonSearchClearResults;
        private System.Windows.Forms.Button buttonSearchNextScan;
        private System.Windows.Forms.Button buttonSearchFirstScan;
        private System.Windows.Forms.DataGridView dataGridViewSearch;
        private System.Windows.Forms.DataGridViewTextBoxColumn Address;
        private System.Windows.Forms.DataGridViewTextBoxColumn Value;
        private STROOP.Controls.VariablePanel.VariablePanel _variablePanelSearch;
    }
}
