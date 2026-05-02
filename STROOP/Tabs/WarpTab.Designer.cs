namespace STROOP.Tabs
{
    partial class WarpTab
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
            this.splitContainerWarp = new STROOP.BetterSplitContainer();
            this.splitContainerWarpLeft = new STROOP.BetterSplitContainer();
            this.buttonWarpInstructions = new System.Windows.Forms.Button();
            this.buttonWarpHookUpTeleporters = new System.Windows.Forms.Button();
            this._variablePanelWarp = new STROOP.Controls.VariablePanel.VariablePanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerWarp)).BeginInit();
            this.splitContainerWarp.Panel1.SuspendLayout();
            this.splitContainerWarp.Panel2.SuspendLayout();
            this.splitContainerWarp.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerWarpLeft)).BeginInit();
            this.splitContainerWarpLeft.Panel1.SuspendLayout();
            this.splitContainerWarpLeft.Panel2.SuspendLayout();
            this.splitContainerWarpLeft.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerWarp
            // 
            this.splitContainerWarp.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerWarp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerWarp.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerWarp.Location = new System.Drawing.Point(0, 0);
            this.splitContainerWarp.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainerWarp.Name = "splitContainerWarp";
            // 
            // splitContainerWarp.Panel1
            // 
            this.splitContainerWarp.Panel1.AutoScroll = true;
            this.splitContainerWarp.Panel1.Controls.Add(this.splitContainerWarpLeft);
            this.splitContainerWarp.Panel1MinSize = 0;
            // 
            // splitContainerWarp.Panel2
            // 
            this.splitContainerWarp.Panel2.Controls.Add(this._variablePanelWarp);
            this.splitContainerWarp.Panel2.Padding = new System.Windows.Forms.Padding(2);
            this.splitContainerWarp.Panel2MinSize = 0;
            this.splitContainerWarp.Size = new System.Drawing.Size(915, 463);
            this.splitContainerWarp.SplitterDistance = 106;
            this.splitContainerWarp.SplitterWidth = 1;
            this.splitContainerWarp.TabIndex = 41;
            // 
            // splitContainerWarpLeft
            // 
            this.splitContainerWarpLeft.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerWarpLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerWarpLeft.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerWarpLeft.Location = new System.Drawing.Point(0, 0);
            this.splitContainerWarpLeft.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainerWarpLeft.Name = "splitContainerWarpLeft";
            this.splitContainerWarpLeft.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerWarpLeft.Panel1
            // 
            this.splitContainerWarpLeft.Panel1.AutoScroll = true;
            this.splitContainerWarpLeft.Panel1.Controls.Add(this.buttonWarpInstructions);
            this.splitContainerWarpLeft.Panel1MinSize = 0;
            // 
            // splitContainerWarpLeft.Panel2
            // 
            this.splitContainerWarpLeft.Panel2.Controls.Add(this.buttonWarpHookUpTeleporters);
            this.splitContainerWarpLeft.Panel2MinSize = 0;
            this.splitContainerWarpLeft.Size = new System.Drawing.Size(106, 463);
            this.splitContainerWarpLeft.SplitterDistance = 220;
            this.splitContainerWarpLeft.SplitterWidth = 1;
            this.splitContainerWarpLeft.TabIndex = 38;
            // 
            // buttonWarpInstructions
            // 
            this.buttonWarpInstructions.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonWarpInstructions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonWarpInstructions.Location = new System.Drawing.Point(0, 0);
            this.buttonWarpInstructions.Margin = new System.Windows.Forms.Padding(2);
            this.buttonWarpInstructions.Name = "buttonWarpInstructions";
            this.buttonWarpInstructions.Size = new System.Drawing.Size(104, 218);
            this.buttonWarpInstructions.TabIndex = 20;
            this.buttonWarpInstructions.Text = "Instructions";
            this.buttonWarpInstructions.UseVisualStyleBackColor = true;
            // 
            // buttonWarpHookUpTeleporters
            // 
            this.buttonWarpHookUpTeleporters.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonWarpHookUpTeleporters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonWarpHookUpTeleporters.Location = new System.Drawing.Point(0, 0);
            this.buttonWarpHookUpTeleporters.Margin = new System.Windows.Forms.Padding(2);
            this.buttonWarpHookUpTeleporters.Name = "buttonWarpHookUpTeleporters";
            this.buttonWarpHookUpTeleporters.Size = new System.Drawing.Size(104, 240);
            this.buttonWarpHookUpTeleporters.TabIndex = 20;
            this.buttonWarpHookUpTeleporters.Text = "Hook Up Teleporters";
            this.buttonWarpHookUpTeleporters.UseVisualStyleBackColor = true;
            // 
            // watchVariablePanelWarp
            // 
            this._variablePanelWarp.DataPath = "Config/WarpData.xml";
            this._variablePanelWarp.Location = new System.Drawing.Point(1, 2);
            this._variablePanelWarp.Name = "_variablePanelWarp";
            this._variablePanelWarp.Size = new System.Drawing.Size(800, 454);
            this._variablePanelWarp.TabIndex = 0;
            // 
            // WarpTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainerWarp);
            this.Name = "WarpTab";
            this.Size = new System.Drawing.Size(915, 463);
            this.splitContainerWarp.Panel1.ResumeLayout(false);
            this.splitContainerWarp.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerWarp)).EndInit();
            this.splitContainerWarp.ResumeLayout(false);
            this.splitContainerWarpLeft.Panel1.ResumeLayout(false);
            this.splitContainerWarpLeft.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerWarpLeft)).EndInit();
            this.splitContainerWarpLeft.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private BetterSplitContainer splitContainerWarp;
        private BetterSplitContainer splitContainerWarpLeft;
        private System.Windows.Forms.Button buttonWarpInstructions;
        private System.Windows.Forms.Button buttonWarpHookUpTeleporters;
        private STROOP.Controls.VariablePanel.VariablePanel _variablePanelWarp;
    }
}
