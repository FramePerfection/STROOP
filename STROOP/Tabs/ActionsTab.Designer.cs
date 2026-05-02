namespace STROOP.Tabs
{
    partial class ActionsTab
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
            this.textBoxAnimationDescription = new STROOP.BetterTextbox();
            this.textBoxActionDescription = new STROOP.BetterTextbox();
            this._variablePanelActions = new STROOP.Controls.VariablePanel.VariablePanel();
            this.SuspendLayout();
            // 
            // textBoxAnimationDescription
            // 
            this.textBoxAnimationDescription.BackColor = System.Drawing.SystemColors.Control;
            this.textBoxAnimationDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxAnimationDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold);
            this.textBoxAnimationDescription.ForeColor = System.Drawing.Color.MediumBlue;
            this.textBoxAnimationDescription.Location = new System.Drawing.Point(3, 33);
            this.textBoxAnimationDescription.Name = "textBoxAnimationDescription";
            this.textBoxAnimationDescription.ReadOnly = true;
            this.textBoxAnimationDescription.Size = new System.Drawing.Size(692, 31);
            this.textBoxAnimationDescription.TabIndex = 35;
            this.textBoxAnimationDescription.Text = "Animation Description";
            // 
            // textBoxActionDescription
            // 
            this.textBoxActionDescription.BackColor = System.Drawing.SystemColors.Control;
            this.textBoxActionDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxActionDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold);
            this.textBoxActionDescription.Location = new System.Drawing.Point(3, 0);
            this.textBoxActionDescription.Name = "textBoxActionDescription";
            this.textBoxActionDescription.ReadOnly = true;
            this.textBoxActionDescription.Size = new System.Drawing.Size(692, 31);
            this.textBoxActionDescription.TabIndex = 36;
            this.textBoxActionDescription.Text = "Action Description";
            // 
            // watchVariablePanelActions
            // 
            this._variablePanelActions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._variablePanelActions.DataPath = "Config/ActionsData.xml";
            this._variablePanelActions.Location = new System.Drawing.Point(3, 70);
            this._variablePanelActions.Name = "_variablePanelActions";
            this._variablePanelActions.Size = new System.Drawing.Size(909, 390);
            this._variablePanelActions.TabIndex = 37;
            // 
            // ActionsTab
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this._variablePanelActions);
            this.Controls.Add(this.textBoxAnimationDescription);
            this.Controls.Add(this.textBoxActionDescription);
            this.Name = "ActionsTab";
            this.Size = new System.Drawing.Size(915, 463);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private BetterTextbox textBoxAnimationDescription;
        private BetterTextbox textBoxActionDescription;
        private STROOP.Controls.VariablePanel.VariablePanel _variablePanelActions;
    }
}
