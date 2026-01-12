namespace STROOP.Tabs
{
    partial class PaintingTab
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
            this.splitContainerPainting = new STROOP.BetterSplitContainer();
            this.listBoxPainting = new System.Windows.Forms.ListBox();
            this._variablePanelPainting = new STROOP.Controls.VariablePanel.VariablePanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerPainting)).BeginInit();
            this.splitContainerPainting.Panel1.SuspendLayout();
            this.splitContainerPainting.Panel2.SuspendLayout();
            this.splitContainerPainting.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerPainting
            // 
            this.splitContainerPainting.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerPainting.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerPainting.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerPainting.Location = new System.Drawing.Point(0, 0);
            this.splitContainerPainting.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainerPainting.Name = "splitContainerPainting";
            // 
            // splitContainerPainting.Panel1
            // 
            this.splitContainerPainting.Panel1.AutoScroll = true;
            this.splitContainerPainting.Panel1.Controls.Add(this.listBoxPainting);
            this.splitContainerPainting.Panel1MinSize = 0;
            // 
            // splitContainerPainting.Panel2
            // 
            this.splitContainerPainting.Panel2.Controls.Add(this._variablePanelPainting);
            this.splitContainerPainting.Panel2.Padding = new System.Windows.Forms.Padding(2);
            this.splitContainerPainting.Panel2MinSize = 0;
            this.splitContainerPainting.Size = new System.Drawing.Size(915, 463);
            this.splitContainerPainting.SplitterDistance = 153;
            this.splitContainerPainting.SplitterWidth = 1;
            this.splitContainerPainting.TabIndex = 35;
            // 
            // listBoxPainting
            // 
            this.listBoxPainting.BackColor = System.Drawing.SystemColors.Control;
            this.listBoxPainting.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxPainting.FormattingEnabled = true;
            this.listBoxPainting.Location = new System.Drawing.Point(0, 0);
            this.listBoxPainting.Name = "listBoxPainting";
            this.listBoxPainting.Size = new System.Drawing.Size(151, 461);
            this.listBoxPainting.TabIndex = 18;
            // 
            // watchVariablePanelPainting
            // 
            this._variablePanelPainting.DataPath = "Config/PaintingData.xml";
            this._variablePanelPainting.Dock = System.Windows.Forms.DockStyle.Fill;
            this._variablePanelPainting.Location = new System.Drawing.Point(2, 2);
            this._variablePanelPainting.Margin = new System.Windows.Forms.Padding(0);
            this._variablePanelPainting.Name = "_variablePanelPainting";
            this._variablePanelPainting.Size = new System.Drawing.Size(755, 457);
            this._variablePanelPainting.TabIndex = 7;
            // 
            // PaintingTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainerPainting);
            this.Name = "PaintingTab";
            this.Size = new System.Drawing.Size(915, 463);
            this.splitContainerPainting.Panel1.ResumeLayout(false);
            this.splitContainerPainting.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerPainting)).EndInit();
            this.splitContainerPainting.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private BetterSplitContainer splitContainerPainting;
        private System.Windows.Forms.ListBox listBoxPainting;
        private STROOP.Controls.VariablePanel.VariablePanel _variablePanelPainting;
    }
}
