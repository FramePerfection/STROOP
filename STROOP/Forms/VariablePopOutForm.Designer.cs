namespace STROOP.Forms
{
    partial class VariablePopOutForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VariablePopOutForm));
            this._variablePanel = new STROOP.Controls.VariablePanel.VariablePanel();
            this.SuspendLayout();
            // 
            // _watchVariablePanel
            // 
            this._variablePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._variablePanel.Location = new System.Drawing.Point(2, 2);
            this._variablePanel.Margin = new System.Windows.Forms.Padding(0);
            this._variablePanel.Name = "_variablePanel";
            this._variablePanel.Size = new System.Drawing.Size(280, 170);
            this._variablePanel.TabIndex = 3;
            // 
            // VariablePopOutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 174);
            this.Controls.Add(this._variablePanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(10000, 10000);
            this.MinimizeBox = false;
            this.Name = "VariablePopOutForm";
            this.Padding = new System.Windows.Forms.Padding(2);
            this.ShowIcon = false;
            this.Text = "Variables";
            this.ResumeLayout(false);

        }

        #endregion

        private STROOP.Controls.VariablePanel.VariablePanel _variablePanel;
    }
}