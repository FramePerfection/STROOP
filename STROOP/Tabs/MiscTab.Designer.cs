namespace STROOP.Tabs
{
    partial class MiscTab
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
            this.splitContainerMisc = new STROOP.BetterSplitContainer();
            this.groupBoxRNGIndexTester = new System.Windows.Forms.GroupBox();
            this.textBoxRNGIndexTester = new STROOP.BetterTextbox();
            this.buttonRNGIndexTester = new System.Windows.Forms.Button();
            this.buttonMiscGoToCourse = new System.Windows.Forms.Button();
            this.checkBoxTurnOffMusic = new System.Windows.Forms.CheckBox();
            this.panelMiscBorder = new System.Windows.Forms.Panel();
            this.pictureBoxMisc = new STROOP.Controls.IntPictureBox();
            this._variablePanelMisc = new STROOP.Controls.VariablePanel.VariablePanel();
            this.txtRNGIncrement = new STROOP.BetterTextbox();
            this.labelRNGIncrement = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMisc)).BeginInit();
            this.splitContainerMisc.Panel1.SuspendLayout();
            this.splitContainerMisc.Panel2.SuspendLayout();
            this.splitContainerMisc.SuspendLayout();
            this.groupBoxRNGIndexTester.SuspendLayout();
            this.panelMiscBorder.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMisc)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainerMisc
            // 
            this.splitContainerMisc.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerMisc.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerMisc.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerMisc.Location = new System.Drawing.Point(0, 0);
            this.splitContainerMisc.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainerMisc.Name = "splitContainerMisc";
            // 
            // splitContainerMisc.Panel1
            // 
            this.splitContainerMisc.Panel1.AutoScroll = true;
            this.splitContainerMisc.Panel1.Controls.Add(this.groupBoxRNGIndexTester);
            this.splitContainerMisc.Panel1.Controls.Add(this.buttonMiscGoToCourse);
            this.splitContainerMisc.Panel1.Controls.Add(this.checkBoxTurnOffMusic);
            this.splitContainerMisc.Panel1.Controls.Add(this.panelMiscBorder);
            this.splitContainerMisc.Panel1MinSize = 0;
            // 
            // splitContainerMisc.Panel2
            // 
            this.splitContainerMisc.Panel2.Controls.Add(this._variablePanelMisc);
            this.splitContainerMisc.Panel2.Padding = new System.Windows.Forms.Padding(2);
            this.splitContainerMisc.Panel2MinSize = 0;
            this.splitContainerMisc.Size = new System.Drawing.Size(915, 463);
            this.splitContainerMisc.SplitterDistance = 130;
            this.splitContainerMisc.SplitterWidth = 1;
            this.splitContainerMisc.TabIndex = 19;
            // 
            // groupBoxRNGIndexTester
            // 
            this.groupBoxRNGIndexTester.Controls.Add(this.labelRNGIncrement);
            this.groupBoxRNGIndexTester.Controls.Add(this.txtRNGIncrement);
            this.groupBoxRNGIndexTester.Controls.Add(this.textBoxRNGIndexTester);
            this.groupBoxRNGIndexTester.Controls.Add(this.buttonRNGIndexTester);
            this.groupBoxRNGIndexTester.Location = new System.Drawing.Point(3, 94);
            this.groupBoxRNGIndexTester.Name = "groupBoxRNGIndexTester";
            this.groupBoxRNGIndexTester.Size = new System.Drawing.Size(107, 104);
            this.groupBoxRNGIndexTester.TabIndex = 33;
            this.groupBoxRNGIndexTester.TabStop = false;
            this.groupBoxRNGIndexTester.Text = "RNG Index Tester";
            // 
            // textBoxRNGIndexTester
            // 
            this.textBoxRNGIndexTester.Location = new System.Drawing.Point(7, 19);
            this.textBoxRNGIndexTester.Name = "textBoxRNGIndexTester";
            this.textBoxRNGIndexTester.Size = new System.Drawing.Size(93, 20);
            this.textBoxRNGIndexTester.TabIndex = 33;
            this.textBoxRNGIndexTester.Text = "0";
            this.textBoxRNGIndexTester.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // buttonRNGIndexTester
            // 
            this.buttonRNGIndexTester.Location = new System.Drawing.Point(7, 42);
            this.buttonRNGIndexTester.Margin = new System.Windows.Forms.Padding(0);
            this.buttonRNGIndexTester.Name = "buttonRNGIndexTester";
            this.buttonRNGIndexTester.Size = new System.Drawing.Size(93, 25);
            this.buttonRNGIndexTester.TabIndex = 35;
            this.buttonRNGIndexTester.Text = "Set && Increment";
            this.buttonRNGIndexTester.UseVisualStyleBackColor = true;
            // 
            // buttonMiscGoToCourse
            // 
            this.buttonMiscGoToCourse.Location = new System.Drawing.Point(3, 201);
            this.buttonMiscGoToCourse.Margin = new System.Windows.Forms.Padding(0);
            this.buttonMiscGoToCourse.Name = "buttonMiscGoToCourse";
            this.buttonMiscGoToCourse.Size = new System.Drawing.Size(107, 25);
            this.buttonMiscGoToCourse.TabIndex = 35;
            this.buttonMiscGoToCourse.Text = "Go to Course";
            this.buttonMiscGoToCourse.UseVisualStyleBackColor = true;
            // 
            // checkBoxTurnOffMusic
            // 
            this.checkBoxTurnOffMusic.AutoSize = true;
            this.checkBoxTurnOffMusic.Location = new System.Drawing.Point(10, 64);
            this.checkBoxTurnOffMusic.Name = "checkBoxTurnOffMusic";
            this.checkBoxTurnOffMusic.Size = new System.Drawing.Size(96, 17);
            this.checkBoxTurnOffMusic.TabIndex = 32;
            this.checkBoxTurnOffMusic.Text = "Turn Off Music";
            this.checkBoxTurnOffMusic.UseVisualStyleBackColor = true;
            // 
            // panelMiscBorder
            // 
            this.panelMiscBorder.Controls.Add(this.pictureBoxMisc);
            this.panelMiscBorder.Location = new System.Drawing.Point(3, 4);
            this.panelMiscBorder.Margin = new System.Windows.Forms.Padding(2);
            this.panelMiscBorder.Name = "panelMiscBorder";
            this.panelMiscBorder.Size = new System.Drawing.Size(55, 55);
            this.panelMiscBorder.TabIndex = 4;
            // 
            // pictureBoxMisc
            // 
            this.pictureBoxMisc.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxMisc.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            this.pictureBoxMisc.Location = new System.Drawing.Point(3, 3);
            this.pictureBoxMisc.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBoxMisc.MaximumSize = new System.Drawing.Size(133, 130);
            this.pictureBoxMisc.Name = "pictureBoxMisc";
            this.pictureBoxMisc.Size = new System.Drawing.Size(49, 49);
            this.pictureBoxMisc.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxMisc.TabIndex = 0;
            this.pictureBoxMisc.TabStop = false;
            // 
            // watchVariablePanelMisc
            // 
            this._variablePanelMisc.DataPath = "Config/MiscData.xml";
            this._variablePanelMisc.Dock = System.Windows.Forms.DockStyle.Fill;
            this._variablePanelMisc.Location = new System.Drawing.Point(2, 2);
            this._variablePanelMisc.Margin = new System.Windows.Forms.Padding(0);
            this._variablePanelMisc.Name = "_variablePanelMisc";
            this._variablePanelMisc.Size = new System.Drawing.Size(778, 457);
            this._variablePanelMisc.TabIndex = 5;
            // 
            // txtRNGIncrement
            // 
            this.txtRNGIncrement.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRNGIncrement.Location = new System.Drawing.Point(70, 75);
            this.txtRNGIncrement.Name = "txtRNGIncrement";
            this.txtRNGIncrement.Size = new System.Drawing.Size(30, 20);
            this.txtRNGIncrement.TabIndex = 33;
            this.txtRNGIncrement.Text = "4";
            this.txtRNGIncrement.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // labelRNGIncrement
            // 
            this.labelRNGIncrement.AutoSize = true;
            this.labelRNGIncrement.Location = new System.Drawing.Point(7, 78);
            this.labelRNGIncrement.Name = "labelRNGIncrement";
            this.labelRNGIncrement.Size = new System.Drawing.Size(57, 13);
            this.labelRNGIncrement.TabIndex = 36;
            this.labelRNGIncrement.Text = "Increment:";
            // 
            // MiscTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainerMisc);
            this.Name = "MiscTab";
            this.splitContainerMisc.Panel1.ResumeLayout(false);
            this.splitContainerMisc.Panel1.PerformLayout();
            this.splitContainerMisc.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMisc)).EndInit();
            this.splitContainerMisc.ResumeLayout(false);
            this.groupBoxRNGIndexTester.ResumeLayout(false);
            this.groupBoxRNGIndexTester.PerformLayout();
            this.panelMiscBorder.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMisc)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private BetterSplitContainer splitContainerMisc;
        private System.Windows.Forms.GroupBox groupBoxRNGIndexTester;
        private BetterTextbox textBoxRNGIndexTester;
        private System.Windows.Forms.Button buttonRNGIndexTester;
        private System.Windows.Forms.Button buttonMiscGoToCourse;
        private System.Windows.Forms.CheckBox checkBoxTurnOffMusic;
        private System.Windows.Forms.Panel panelMiscBorder;
        private Controls.IntPictureBox pictureBoxMisc;
        private STROOP.Controls.VariablePanel.VariablePanel _variablePanelMisc;
        private System.Windows.Forms.Label labelRNGIncrement;
        private BetterTextbox txtRNGIncrement;
    }
}
