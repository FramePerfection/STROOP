namespace STROOP.Tabs
{
    partial class CellsTab
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
            this.splitContainerCells = new STROOP.BetterSplitContainer();
            this.splitContainerCellsControls = new STROOP.BetterSplitContainer();
            this.buttonCellsBuildTree = new System.Windows.Forms.Button();
            this.treeViewCells = new System.Windows.Forms.TreeView();
            this._variablePanelCells = new STROOP.Controls.VariablePanel.VariablePanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerCells)).BeginInit();
            this.splitContainerCells.Panel1.SuspendLayout();
            this.splitContainerCells.Panel2.SuspendLayout();
            this.splitContainerCells.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerCellsControls)).BeginInit();
            this.splitContainerCellsControls.Panel1.SuspendLayout();
            this.splitContainerCellsControls.Panel2.SuspendLayout();
            this.splitContainerCellsControls.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerCells
            // 
            this.splitContainerCells.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerCells.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerCells.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerCells.Location = new System.Drawing.Point(0, 0);
            this.splitContainerCells.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainerCells.Name = "splitContainerCells";
            // 
            // splitContainerCells.Panel1
            // 
            this.splitContainerCells.Panel1.AutoScroll = true;
            this.splitContainerCells.Panel1.Controls.Add(this.splitContainerCellsControls);
            this.splitContainerCells.Panel1MinSize = 0;
            // 
            // splitContainerCells.Panel2
            // 
            this.splitContainerCells.Panel2.Controls.Add(this._variablePanelCells);
            this.splitContainerCells.Panel2.Padding = new System.Windows.Forms.Padding(2);
            this.splitContainerCells.Panel2MinSize = 0;
            this.splitContainerCells.Size = new System.Drawing.Size(915, 463);
            this.splitContainerCells.SplitterDistance = 303;
            this.splitContainerCells.SplitterWidth = 1;
            this.splitContainerCells.TabIndex = 36;
            // 
            // splitContainerCellsControls
            // 
            this.splitContainerCellsControls.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerCellsControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerCellsControls.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerCellsControls.Location = new System.Drawing.Point(0, 0);
            this.splitContainerCellsControls.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainerCellsControls.Name = "splitContainerCellsControls";
            this.splitContainerCellsControls.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerCellsControls.Panel1
            // 
            this.splitContainerCellsControls.Panel1.AutoScroll = true;
            this.splitContainerCellsControls.Panel1.Controls.Add(this.buttonCellsBuildTree);
            this.splitContainerCellsControls.Panel1MinSize = 0;
            // 
            // splitContainerCellsControls.Panel2
            // 
            this.splitContainerCellsControls.Panel2.Controls.Add(this.treeViewCells);
            this.splitContainerCellsControls.Panel2MinSize = 0;
            this.splitContainerCellsControls.Size = new System.Drawing.Size(303, 463);
            this.splitContainerCellsControls.SplitterDistance = 42;
            this.splitContainerCellsControls.SplitterWidth = 1;
            this.splitContainerCellsControls.TabIndex = 39;
            // 
            // buttonCellsBuildTree
            // 
            this.buttonCellsBuildTree.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonCellsBuildTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonCellsBuildTree.Location = new System.Drawing.Point(0, 0);
            this.buttonCellsBuildTree.Margin = new System.Windows.Forms.Padding(2);
            this.buttonCellsBuildTree.Name = "buttonCellsBuildTree";
            this.buttonCellsBuildTree.Size = new System.Drawing.Size(301, 40);
            this.buttonCellsBuildTree.TabIndex = 20;
            this.buttonCellsBuildTree.Text = "Build Tree";
            this.buttonCellsBuildTree.UseVisualStyleBackColor = true;
            // 
            // treeViewCells
            // 
            this.treeViewCells.BackColor = System.Drawing.SystemColors.Control;
            this.treeViewCells.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewCells.Location = new System.Drawing.Point(0, 0);
            this.treeViewCells.Name = "treeViewCells";
            this.treeViewCells.Size = new System.Drawing.Size(301, 418);
            this.treeViewCells.TabIndex = 1;
            // 
            // watchVariablePanelCells
            // 
            this._variablePanelCells.DataPath = "Config/CellsData.xml";
            this._variablePanelCells.Dock = System.Windows.Forms.DockStyle.Fill;
            this._variablePanelCells.Location = new System.Drawing.Point(2, 2);
            this._variablePanelCells.Margin = new System.Windows.Forms.Padding(0);
            this._variablePanelCells.Name = "_variablePanelCells";
            this._variablePanelCells.Size = new System.Drawing.Size(605, 457);
            this._variablePanelCells.TabIndex = 3;
            // 
            // CellsTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainerCells);
            this.Name = "CellsTab";
            this.Size = new System.Drawing.Size(915, 463);
            this.splitContainerCells.Panel1.ResumeLayout(false);
            this.splitContainerCells.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerCells)).EndInit();
            this.splitContainerCells.ResumeLayout(false);
            this.splitContainerCellsControls.Panel1.ResumeLayout(false);
            this.splitContainerCellsControls.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerCellsControls)).EndInit();
            this.splitContainerCellsControls.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private BetterSplitContainer splitContainerCells;
        private BetterSplitContainer splitContainerCellsControls;
        private System.Windows.Forms.Button buttonCellsBuildTree;
        private System.Windows.Forms.TreeView treeViewCells;
        private STROOP.Controls.VariablePanel.VariablePanel _variablePanelCells;
    }
}
