namespace STROOP.Tabs
{
    partial class MusicTab
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
            this.splitContainerMusic = new STROOP.BetterSplitContainer();
            this.listBoxMusic = new System.Windows.Forms.ListBox();
            this._variablePanelMusic = new STROOP.Controls.VariablePanel.VariablePanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMusic)).BeginInit();
            this.splitContainerMusic.Panel1.SuspendLayout();
            this.splitContainerMusic.Panel2.SuspendLayout();
            this.splitContainerMusic.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerMusic
            // 
            this.splitContainerMusic.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerMusic.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerMusic.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerMusic.Location = new System.Drawing.Point(0, 0);
            this.splitContainerMusic.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainerMusic.Name = "splitContainerMusic";
            // 
            // splitContainerMusic.Panel1
            // 
            this.splitContainerMusic.Panel1.AutoScroll = true;
            this.splitContainerMusic.Panel1.Controls.Add(this.listBoxMusic);
            this.splitContainerMusic.Panel1MinSize = 0;
            // 
            // splitContainerMusic.Panel2
            // 
            this.splitContainerMusic.Panel2.Controls.Add(this._variablePanelMusic);
            this.splitContainerMusic.Panel2.Padding = new System.Windows.Forms.Padding(2);
            this.splitContainerMusic.Panel2MinSize = 0;
            this.splitContainerMusic.Size = new System.Drawing.Size(915, 463);
            this.splitContainerMusic.SplitterDistance = 153;
            this.splitContainerMusic.SplitterWidth = 1;
            this.splitContainerMusic.TabIndex = 36;
            // 
            // listBoxMusic
            // 
            this.listBoxMusic.BackColor = System.Drawing.SystemColors.Control;
            this.listBoxMusic.FormattingEnabled = true;
            this.listBoxMusic.Location = new System.Drawing.Point(0, 0);
            this.listBoxMusic.Name = "listBoxMusic";
            this.listBoxMusic.Size = new System.Drawing.Size(151, 459);
            this.listBoxMusic.TabIndex = 18;
            // 
            // watchVariablePanelMusic
            // 
            this._variablePanelMusic.DataPath = "Config/MusicData2.xml";
            this._variablePanelMusic.Location = new System.Drawing.Point(2, 2);
            this._variablePanelMusic.Margin = new System.Windows.Forms.Padding(0);
            this._variablePanelMusic.Name = "_variablePanelMusic";
            this._variablePanelMusic.Size = new System.Drawing.Size(755, 457);
            this._variablePanelMusic.TabIndex = 7;
            // 
            // MusicTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainerMusic);
            this.Name = "MusicTab";
            this.Size = new System.Drawing.Size(915, 463);
            this.splitContainerMusic.Panel1.ResumeLayout(false);
            this.splitContainerMusic.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMusic)).EndInit();
            this.splitContainerMusic.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private BetterSplitContainer splitContainerMusic;
        private System.Windows.Forms.ListBox listBoxMusic;
        private STROOP.Controls.VariablePanel.VariablePanel _variablePanelMusic;
    }
}
