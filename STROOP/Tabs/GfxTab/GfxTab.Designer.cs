namespace STROOP.Tabs.GfxTab
{
    partial class GfxTab
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
            this.splitContainerGfxLeft = new STROOP.BetterSplitContainer();
            this.treeViewGfx = new System.Windows.Forms.TreeView();
            this.splitContainerGfxRight = new STROOP.BetterSplitContainer();
            this.splitContainerGfxMiddle = new STROOP.BetterSplitContainer();
            this.buttonGfxHitboxHack = new System.Windows.Forms.Button();
            this.buttonGfxDumpDisplayList = new System.Windows.Forms.Button();
            this.buttonGfxRefreshObject = new System.Windows.Forms.Button();
            this.buttonGfxRefresh = new System.Windows.Forms.Button();
            this._variablePanelGfx = new STROOP.Controls.VariablePanel.VariablePanel();
            this.richTextBoxGfx = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerGfxLeft)).BeginInit();
            this.splitContainerGfxLeft.Panel1.SuspendLayout();
            this.splitContainerGfxLeft.Panel2.SuspendLayout();
            this.splitContainerGfxLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerGfxRight)).BeginInit();
            this.splitContainerGfxRight.Panel1.SuspendLayout();
            this.splitContainerGfxRight.Panel2.SuspendLayout();
            this.splitContainerGfxRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerGfxMiddle)).BeginInit();
            this.splitContainerGfxMiddle.Panel1.SuspendLayout();
            this.splitContainerGfxMiddle.Panel2.SuspendLayout();
            this.splitContainerGfxMiddle.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerGfxLeft
            // 
            this.splitContainerGfxLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerGfxLeft.Location = new System.Drawing.Point(0, 0);
            this.splitContainerGfxLeft.Name = "splitContainerGfxLeft";
            // 
            // splitContainerGfxLeft.Panel1
            // 
            this.splitContainerGfxLeft.Panel1.Controls.Add(this.treeViewGfx);
            // 
            // splitContainerGfxLeft.Panel2
            // 
            this.splitContainerGfxLeft.Panel2.Controls.Add(this.splitContainerGfxRight);
            this.splitContainerGfxLeft.Size = new System.Drawing.Size(915, 463);
            this.splitContainerGfxLeft.SplitterDistance = 301;
            this.splitContainerGfxLeft.TabIndex = 1;
            // 
            // treeViewGfx
            // 
            this.treeViewGfx.BackColor = System.Drawing.SystemColors.Control;
            this.treeViewGfx.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewGfx.Location = new System.Drawing.Point(0, 0);
            this.treeViewGfx.Name = "treeViewGfx";
            this.treeViewGfx.Size = new System.Drawing.Size(301, 463);
            this.treeViewGfx.TabIndex = 0;
            // 
            // splitContainerGfxRight
            // 
            this.splitContainerGfxRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerGfxRight.Location = new System.Drawing.Point(0, 0);
            this.splitContainerGfxRight.Name = "splitContainerGfxRight";
            // 
            // splitContainerGfxRight.Panel1
            // 
            this.splitContainerGfxRight.Panel1.Controls.Add(this.splitContainerGfxMiddle);
            // 
            // splitContainerGfxRight.Panel2
            // 
            this.splitContainerGfxRight.Panel2.Controls.Add(this.richTextBoxGfx);
            this.splitContainerGfxRight.Size = new System.Drawing.Size(610, 463);
            this.splitContainerGfxRight.SplitterDistance = 325;
            this.splitContainerGfxRight.TabIndex = 0;
            // 
            // splitContainerGfxMiddle
            // 
            this.splitContainerGfxMiddle.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainerGfxMiddle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerGfxMiddle.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerGfxMiddle.Location = new System.Drawing.Point(0, 0);
            this.splitContainerGfxMiddle.Name = "splitContainerGfxMiddle";
            this.splitContainerGfxMiddle.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerGfxMiddle.Panel1
            // 
            this.splitContainerGfxMiddle.Panel1.Controls.Add(this.buttonGfxHitboxHack);
            this.splitContainerGfxMiddle.Panel1.Controls.Add(this.buttonGfxDumpDisplayList);
            this.splitContainerGfxMiddle.Panel1.Controls.Add(this.buttonGfxRefreshObject);
            this.splitContainerGfxMiddle.Panel1.Controls.Add(this.buttonGfxRefresh);
            // 
            // splitContainerGfxMiddle.Panel2
            // 
            this.splitContainerGfxMiddle.Panel2.Controls.Add(this._variablePanelGfx);
            this.splitContainerGfxMiddle.Size = new System.Drawing.Size(325, 463);
            this.splitContainerGfxMiddle.SplitterDistance = 60;
            this.splitContainerGfxMiddle.TabIndex = 1;
            // 
            // buttonGfxHitboxHack
            // 
            this.buttonGfxHitboxHack.Location = new System.Drawing.Point(113, 31);
            this.buttonGfxHitboxHack.Name = "buttonGfxHitboxHack";
            this.buttonGfxHitboxHack.Size = new System.Drawing.Size(128, 23);
            this.buttonGfxHitboxHack.TabIndex = 3;
            this.buttonGfxHitboxHack.Text = "Inject hitbox view code";
            this.buttonGfxHitboxHack.UseVisualStyleBackColor = true;
            // 
            // buttonGfxDumpDisplayList
            // 
            this.buttonGfxDumpDisplayList.Location = new System.Drawing.Point(3, 31);
            this.buttonGfxDumpDisplayList.Name = "buttonGfxDumpDisplayList";
            this.buttonGfxDumpDisplayList.Size = new System.Drawing.Size(104, 23);
            this.buttonGfxDumpDisplayList.TabIndex = 2;
            this.buttonGfxDumpDisplayList.Text = "Export display list";
            this.buttonGfxDumpDisplayList.UseVisualStyleBackColor = true;
            // 
            // buttonGfxRefreshObject
            // 
            this.buttonGfxRefreshObject.Location = new System.Drawing.Point(94, 3);
            this.buttonGfxRefreshObject.Name = "buttonGfxRefreshObject";
            this.buttonGfxRefreshObject.Size = new System.Drawing.Size(147, 23);
            this.buttonGfxRefreshObject.TabIndex = 1;
            this.buttonGfxRefreshObject.Text = "Build from selected objects";
            this.buttonGfxRefreshObject.UseVisualStyleBackColor = true;
            // 
            // buttonGfxRefresh
            // 
            this.buttonGfxRefresh.Location = new System.Drawing.Point(3, 3);
            this.buttonGfxRefresh.Name = "buttonGfxRefresh";
            this.buttonGfxRefresh.Size = new System.Drawing.Size(85, 23);
            this.buttonGfxRefresh.TabIndex = 0;
            this.buttonGfxRefresh.Text = "Build from root";
            this.buttonGfxRefresh.UseVisualStyleBackColor = true;
            // 
            // watchVariablePanelGfx
            // 
            this._variablePanelGfx.DataPath = null;
            this._variablePanelGfx.Dock = System.Windows.Forms.DockStyle.Fill;
            this._variablePanelGfx.Location = new System.Drawing.Point(0, 0);
            this._variablePanelGfx.Name = "_variablePanelGfx";
            this._variablePanelGfx.Size = new System.Drawing.Size(325, 399);
            this._variablePanelGfx.TabIndex = 0;
            // 
            // richTextBoxGfx
            // 
            this.richTextBoxGfx.BackColor = System.Drawing.SystemColors.Control;
            this.richTextBoxGfx.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxGfx.Location = new System.Drawing.Point(0, 0);
            this.richTextBoxGfx.Name = "richTextBoxGfx";
            this.richTextBoxGfx.Size = new System.Drawing.Size(281, 463);
            this.richTextBoxGfx.TabIndex = 0;
            this.richTextBoxGfx.Text = "";
            // 
            // GfxTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainerGfxLeft);
            this.Name = "GfxTab";
            this.Size = new System.Drawing.Size(915, 463);
            this.splitContainerGfxLeft.Panel1.ResumeLayout(false);
            this.splitContainerGfxLeft.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerGfxLeft)).EndInit();
            this.splitContainerGfxLeft.ResumeLayout(false);
            this.splitContainerGfxRight.Panel1.ResumeLayout(false);
            this.splitContainerGfxRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerGfxRight)).EndInit();
            this.splitContainerGfxRight.ResumeLayout(false);
            this.splitContainerGfxMiddle.Panel1.ResumeLayout(false);
            this.splitContainerGfxMiddle.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerGfxMiddle)).EndInit();
            this.splitContainerGfxMiddle.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private BetterSplitContainer splitContainerGfxLeft;
        private System.Windows.Forms.TreeView treeViewGfx;
        private BetterSplitContainer splitContainerGfxRight;
        private BetterSplitContainer splitContainerGfxMiddle;
        private System.Windows.Forms.Button buttonGfxHitboxHack;
        private System.Windows.Forms.Button buttonGfxDumpDisplayList;
        private System.Windows.Forms.Button buttonGfxRefreshObject;
        private System.Windows.Forms.Button buttonGfxRefresh;
        private STROOP.Controls.VariablePanel.VariablePanel _variablePanelGfx;
        private System.Windows.Forms.RichTextBox richTextBoxGfx;
    }
}
