﻿namespace STROOP.Tabs
{
    partial class QuarterFramesTab
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
            this.watchVariablePanelQuarterFrame = new STROOP.Controls.WatchVariableFlowLayoutPanel();
            this.SuspendLayout();
            // 
            // watchVariablePanelQuarterFrame
            // 
            this.watchVariablePanelQuarterFrame.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.watchVariablePanelQuarterFrame.DataPath = "Config/QuarterFrameData.xml";
            this.watchVariablePanelQuarterFrame.Location = new System.Drawing.Point(0, 0);
            this.watchVariablePanelQuarterFrame.Margin = new System.Windows.Forms.Padding(0);
            this.watchVariablePanelQuarterFrame.Name = "watchVariablePanelQuarterFrame";
            this.watchVariablePanelQuarterFrame.Size = new System.Drawing.Size(915, 463);
            this.watchVariablePanelQuarterFrame.TabIndex = 3;
            // 
            // QuarterFramesTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.watchVariablePanelQuarterFrame);
            this.Name = "QuarterFramesTab";
            this.Size = new System.Drawing.Size(915, 463);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.WatchVariableFlowLayoutPanel watchVariablePanelQuarterFrame;
    }
}
