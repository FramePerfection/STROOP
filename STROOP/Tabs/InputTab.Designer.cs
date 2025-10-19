namespace STROOP.Tabs
{
    partial class InputTab
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
            this.splitContainerInput = new STROOP.BetterSplitContainer();
            this.inputDisplayPanel = new STROOP.InputDisplayPanel();
            this._variablePanelInput = new STROOP.Controls.VariablePanel.VariablePanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerInput)).BeginInit();
            this.splitContainerInput.Panel1.SuspendLayout();
            this.splitContainerInput.Panel2.SuspendLayout();
            this.splitContainerInput.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerInput
            // 
            this.splitContainerInput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerInput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerInput.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerInput.Location = new System.Drawing.Point(0, 0);
            this.splitContainerInput.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainerInput.Name = "splitContainerInput";
            // 
            // splitContainerInput.Panel1
            // 
            this.splitContainerInput.Panel1.Controls.Add(this.inputDisplayPanel);
            this.splitContainerInput.Panel1.Padding = new System.Windows.Forms.Padding(2);
            this.splitContainerInput.Panel1MinSize = 0;
            // 
            // splitContainerInput.Panel2
            // 
            this.splitContainerInput.Panel2.Controls.Add(this._variablePanelInput);
            this.splitContainerInput.Panel2.Padding = new System.Windows.Forms.Padding(2);
            this.splitContainerInput.Panel2MinSize = 0;
            this.splitContainerInput.Size = new System.Drawing.Size(915, 463);
            this.splitContainerInput.SplitterDistance = 428;
            this.splitContainerInput.SplitterWidth = 1;
            this.splitContainerInput.TabIndex = 18;
            // 
            // inputDisplayPanel
            // 
            this.inputDisplayPanel.AutoSize = true;
            this.inputDisplayPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputDisplayPanel.Location = new System.Drawing.Point(2, 2);
            this.inputDisplayPanel.Margin = new System.Windows.Forms.Padding(0);
            this.inputDisplayPanel.Name = "inputDisplayPanel";
            this.inputDisplayPanel.Size = new System.Drawing.Size(422, 457);
            this.inputDisplayPanel.TabIndex = 4;
            // 
            // watchVariablePanelInput
            // 
            this._variablePanelInput.DataPath = "Config/InputData.xml";
            this._variablePanelInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this._variablePanelInput.Location = new System.Drawing.Point(2, 2);
            this._variablePanelInput.Margin = new System.Windows.Forms.Padding(0);
            this._variablePanelInput.Name = "_variablePanelInput";
            this._variablePanelInput.Size = new System.Drawing.Size(480, 457);
            this._variablePanelInput.TabIndex = 2;
            // 
            // InputTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainerInput);
            this.Name = "InputTab";
            this.Size = new System.Drawing.Size(915, 463);
            this.splitContainerInput.Panel1.ResumeLayout(false);
            this.splitContainerInput.Panel1.PerformLayout();
            this.splitContainerInput.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerInput)).EndInit();
            this.splitContainerInput.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private BetterSplitContainer splitContainerInput;
        private InputDisplayPanel inputDisplayPanel;
        private STROOP.Controls.VariablePanel.VariablePanel _variablePanelInput;
    }
}
