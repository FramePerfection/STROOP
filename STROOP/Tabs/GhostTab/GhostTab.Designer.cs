namespace STROOP.Tabs.GhostTab
{
    partial class GhostTab
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            groupBoxGhosts = new System.Windows.Forms.GroupBox();
            listBoxGhosts = new System.Windows.Forms.ListBox();
            buttonMarioColor = new System.Windows.Forms.Button();
            buttonWatchGhostFile = new System.Windows.Forms.Button();
            buttonLoadGhost = new System.Windows.Forms.Button();
            groupBoxGhostInfo = new System.Windows.Forms.GroupBox();
            checkTransparentGhosts = new System.Windows.Forms.CheckBox();
            buttonGhostColor = new System.Windows.Forms.Button();
            textBoxGhostName = new System.Windows.Forms.TextBox();
            labelNumFrames = new System.Windows.Forms.Label();
            labelName = new System.Windows.Forms.Label();
            labelGhostFile = new System.Windows.Forms.Label();
            labelGhostPlaybackStart = new System.Windows.Forms.Label();
            lblPlaybackOffset = new System.Windows.Forms.Label();
            numericUpDownPlaybackOffset = new System.Windows.Forms.NumericUpDown();
            labelBaseGlobalTimer = new System.Windows.Forms.Label();
            numericUpDownStartOfPlayback = new System.Windows.Forms.NumericUpDown();
            buttonSaveGhost = new System.Windows.Forms.Button();
            buttonTutorialRecord = new System.Windows.Forms.Button();
            groupGhostHack = new System.Windows.Forms.GroupBox();
            labelHackActiveState = new System.Windows.Forms.Label();
            buttonDisableGhostHack = new System.Windows.Forms.Button();
            buttonEnableGhostHack = new System.Windows.Forms.Button();
            _variablePanelGhost = new STROOP.Controls.VariablePanel.VariablePanel();
            groupBoxVariables = new System.Windows.Forms.GroupBox();
            groupBoxHelp = new System.Windows.Forms.GroupBox();
            buttonTutorialFileWatch = new System.Windows.Forms.Button();
            buttonHelpGfxPool = new System.Windows.Forms.Button();
            buttonTutorialPlayback = new System.Windows.Forms.Button();
            buttonTutorialNotes = new System.Windows.Forms.Button();
            groupBoxGfxPool = new System.Windows.Forms.GroupBox();
            textBoxPoolSize = new System.Windows.Forms.TextBox();
            textBoxPoolAddr2 = new System.Windows.Forms.TextBox();
            textBoxPoolAddr1 = new System.Windows.Forms.TextBox();
            labelPoolSize = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            labelPool1Address = new System.Windows.Forms.Label();
            buttonMoveGfxPool = new System.Windows.Forms.Button();
            lblRAMOffsetBase = new System.Windows.Forms.Label();
            txtRAMOffsetBase = new System.Windows.Forms.TextBox();
            groupBoxGhosts.SuspendLayout();
            groupBoxGhostInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownPlaybackOffset).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownStartOfPlayback).BeginInit();
            groupGhostHack.SuspendLayout();
            groupBoxVariables.SuspendLayout();
            groupBoxHelp.SuspendLayout();
            groupBoxGfxPool.SuspendLayout();
            SuspendLayout();
            // 
            // groupBoxGhosts
            // 
            groupBoxGhosts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
            groupBoxGhosts.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            groupBoxGhosts.Controls.Add(listBoxGhosts);
            groupBoxGhosts.Controls.Add(buttonMarioColor);
            groupBoxGhosts.Controls.Add(buttonWatchGhostFile);
            groupBoxGhosts.Controls.Add(buttonLoadGhost);
            groupBoxGhosts.Controls.Add(groupBoxGhostInfo);
            groupBoxGhosts.Controls.Add(buttonSaveGhost);
            groupBoxGhosts.Location = new System.Drawing.Point(198, 3);
            groupBoxGhosts.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBoxGhosts.Name = "groupBoxGhosts";
            groupBoxGhosts.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBoxGhosts.Size = new System.Drawing.Size(601, 527);
            groupBoxGhosts.TabIndex = 2;
            groupBoxGhosts.TabStop = false;
            groupBoxGhosts.Text = "Ghosts";
            // 
            // listBoxGhosts
            // 
            listBoxGhosts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
            listBoxGhosts.FormattingEnabled = true;
            listBoxGhosts.ItemHeight = 15;
            listBoxGhosts.Location = new System.Drawing.Point(10, 14);
            listBoxGhosts.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            listBoxGhosts.Name = "listBoxGhosts";
            listBoxGhosts.Size = new System.Drawing.Size(408, 364);
            listBoxGhosts.TabIndex = 5;
            listBoxGhosts.SelectedIndexChanged += listBoxGhosts_SelectedIndexChanged;
            // 
            // buttonMarioColor
            // 
            buttonMarioColor.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            buttonMarioColor.BackColor = System.Drawing.Color.Red;
            buttonMarioColor.Location = new System.Drawing.Point(426, 136);
            buttonMarioColor.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonMarioColor.Name = "buttonMarioColor";
            buttonMarioColor.Size = new System.Drawing.Size(134, 27);
            buttonMarioColor.TabIndex = 5;
            buttonMarioColor.Text = "Main Mario Color";
            buttonMarioColor.UseVisualStyleBackColor = false;
            buttonMarioColor.Click += buttonMarioColor_Click;
            // 
            // buttonWatchGhostFile
            // 
            buttonWatchGhostFile.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            buttonWatchGhostFile.Location = new System.Drawing.Point(426, 14);
            buttonWatchGhostFile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonWatchGhostFile.Name = "buttonWatchGhostFile";
            buttonWatchGhostFile.Size = new System.Drawing.Size(167, 27);
            buttonWatchGhostFile.TabIndex = 0;
            buttonWatchGhostFile.Text = "Edit File Watch List";
            buttonWatchGhostFile.UseVisualStyleBackColor = true;
            buttonWatchGhostFile.Click += buttonWatchGhostFile_Click;
            // 
            // buttonLoadGhost
            // 
            buttonLoadGhost.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            buttonLoadGhost.Location = new System.Drawing.Point(426, 69);
            buttonLoadGhost.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonLoadGhost.Name = "buttonLoadGhost";
            buttonLoadGhost.Size = new System.Drawing.Size(167, 27);
            buttonLoadGhost.TabIndex = 0;
            buttonLoadGhost.Text = "Load Ghost";
            buttonLoadGhost.UseVisualStyleBackColor = true;
            buttonLoadGhost.Click += buttonLoadGhost_Click;
            // 
            // groupBoxGhostInfo
            // 
            groupBoxGhostInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
            groupBoxGhostInfo.Controls.Add(checkTransparentGhosts);
            groupBoxGhostInfo.Controls.Add(buttonGhostColor);
            groupBoxGhostInfo.Controls.Add(textBoxGhostName);
            groupBoxGhostInfo.Controls.Add(labelNumFrames);
            groupBoxGhostInfo.Controls.Add(labelName);
            groupBoxGhostInfo.Controls.Add(labelGhostFile);
            groupBoxGhostInfo.Controls.Add(labelGhostPlaybackStart);
            groupBoxGhostInfo.Controls.Add(lblPlaybackOffset);
            groupBoxGhostInfo.Controls.Add(numericUpDownPlaybackOffset);
            groupBoxGhostInfo.Controls.Add(labelBaseGlobalTimer);
            groupBoxGhostInfo.Controls.Add(numericUpDownStartOfPlayback);
            groupBoxGhostInfo.Location = new System.Drawing.Point(10, 396);
            groupBoxGhostInfo.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBoxGhostInfo.Name = "groupBoxGhostInfo";
            groupBoxGhostInfo.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBoxGhostInfo.Size = new System.Drawing.Size(582, 125);
            groupBoxGhostInfo.TabIndex = 1;
            groupBoxGhostInfo.TabStop = false;
            groupBoxGhostInfo.Text = "Ghost Info";
            // 
            // checkTransparentGhosts
            // 
            checkTransparentGhosts.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            checkTransparentGhosts.AutoSize = true;
            checkTransparentGhosts.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            checkTransparentGhosts.Checked = true;
            checkTransparentGhosts.CheckState = System.Windows.Forms.CheckState.Checked;
            checkTransparentGhosts.Location = new System.Drawing.Point(449, 17);
            checkTransparentGhosts.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkTransparentGhosts.Name = "checkTransparentGhosts";
            checkTransparentGhosts.Size = new System.Drawing.Size(126, 19);
            checkTransparentGhosts.TabIndex = 6;
            checkTransparentGhosts.Text = "Transparent Ghosts";
            checkTransparentGhosts.UseVisualStyleBackColor = true;
            checkTransparentGhosts.CheckedChanged += checkTransparentGhosts_CheckedChanged;
            // 
            // buttonGhostColor
            // 
            buttonGhostColor.Enabled = false;
            buttonGhostColor.Location = new System.Drawing.Point(182, 13);
            buttonGhostColor.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonGhostColor.Name = "buttonGhostColor";
            buttonGhostColor.Size = new System.Drawing.Size(57, 27);
            buttonGhostColor.TabIndex = 5;
            buttonGhostColor.Text = "Color";
            buttonGhostColor.UseVisualStyleBackColor = true;
            buttonGhostColor.Click += buttonGhostColor_Click;
            // 
            // textBoxGhostName
            // 
            textBoxGhostName.Location = new System.Drawing.Point(58, 15);
            textBoxGhostName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBoxGhostName.Name = "textBoxGhostName";
            textBoxGhostName.Size = new System.Drawing.Size(116, 23);
            textBoxGhostName.TabIndex = 4;
            textBoxGhostName.TextChanged += textBoxGhostName_TextChanged;
            // 
            // labelNumFrames
            // 
            labelNumFrames.AutoSize = true;
            labelNumFrames.Location = new System.Drawing.Point(7, 61);
            labelNumFrames.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelNumFrames.Name = "labelNumFrames";
            labelNumFrames.Size = new System.Drawing.Size(107, 15);
            labelNumFrames.TabIndex = 3;
            labelNumFrames.Text = "Number of frames:";
            // 
            // labelName
            // 
            labelName.AutoSize = true;
            labelName.Location = new System.Drawing.Point(7, 18);
            labelName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelName.Name = "labelName";
            labelName.Size = new System.Drawing.Size(42, 15);
            labelName.TabIndex = 3;
            labelName.Text = "Name:";
            // 
            // labelGhostFile
            // 
            labelGhostFile.AutoSize = true;
            labelGhostFile.Location = new System.Drawing.Point(7, 46);
            labelGhostFile.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelGhostFile.Name = "labelGhostFile";
            labelGhostFile.Size = new System.Drawing.Size(28, 15);
            labelGhostFile.TabIndex = 3;
            labelGhostFile.Text = "File:";
            // 
            // labelGhostPlaybackStart
            // 
            labelGhostPlaybackStart.AutoSize = true;
            labelGhostPlaybackStart.Location = new System.Drawing.Point(7, 76);
            labelGhostPlaybackStart.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelGhostPlaybackStart.Name = "labelGhostPlaybackStart";
            labelGhostPlaybackStart.Size = new System.Drawing.Size(129, 15);
            labelGhostPlaybackStart.TabIndex = 3;
            labelGhostPlaybackStart.Text = "Original Playback Start:";
            // 
            // lblPlaybackOffset
            // 
            lblPlaybackOffset.AutoSize = true;
            lblPlaybackOffset.Location = new System.Drawing.Point(340, 102);
            lblPlaybackOffset.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblPlaybackOffset.Name = "lblPlaybackOffset";
            lblPlaybackOffset.Size = new System.Drawing.Size(90, 15);
            lblPlaybackOffset.TabIndex = 3;
            lblPlaybackOffset.Text = "Playback offset:";
            // 
            // numericUpDownPlaybackOffset
            // 
            numericUpDownPlaybackOffset.Location = new System.Drawing.Point(442, 99);
            numericUpDownPlaybackOffset.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            numericUpDownPlaybackOffset.Maximum = new decimal(new int[]
            {
                -1,
                0,
                0,
                0
            });
            numericUpDownPlaybackOffset.Minimum = new decimal(new int[]
            {
                -1,
                0,
                0,
                -2147483648
            });
            numericUpDownPlaybackOffset.Name = "numericUpDownPlaybackOffset";
            numericUpDownPlaybackOffset.Size = new System.Drawing.Size(107, 23);
            numericUpDownPlaybackOffset.TabIndex = 2;
            numericUpDownPlaybackOffset.ValueChanged += numericUpDownPlaybakcOffset_ValueChanged;
            // 
            // labelBaseGlobalTimer
            // 
            labelBaseGlobalTimer.AutoSize = true;
            labelBaseGlobalTimer.Location = new System.Drawing.Point(7, 102);
            labelBaseGlobalTimer.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelBaseGlobalTimer.Name = "labelBaseGlobalTimer";
            labelBaseGlobalTimer.Size = new System.Drawing.Size(167, 15);
            labelBaseGlobalTimer.TabIndex = 3;
            labelBaseGlobalTimer.Text = "Start Playback at Global Timer:";
            // 
            // numericUpDownStartOfPlayback
            // 
            numericUpDownStartOfPlayback.Location = new System.Drawing.Point(192, 99);
            numericUpDownStartOfPlayback.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            numericUpDownStartOfPlayback.Maximum = new decimal(new int[]
            {
                -1,
                0,
                0,
                0
            });
            numericUpDownStartOfPlayback.Name = "numericUpDownStartOfPlayback";
            numericUpDownStartOfPlayback.Size = new System.Drawing.Size(140, 23);
            numericUpDownStartOfPlayback.TabIndex = 2;
            numericUpDownStartOfPlayback.ValueChanged += numericUpDownStartOfPlayback_ValueChanged;
            // 
            // buttonSaveGhost
            // 
            buttonSaveGhost.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            buttonSaveGhost.Location = new System.Drawing.Point(426, 103);
            buttonSaveGhost.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonSaveGhost.Name = "buttonSaveGhost";
            buttonSaveGhost.Size = new System.Drawing.Size(167, 27);
            buttonSaveGhost.TabIndex = 0;
            buttonSaveGhost.Text = "Save Selected";
            buttonSaveGhost.UseVisualStyleBackColor = true;
            buttonSaveGhost.Click += buttonSaveGhost_Click;
            // 
            // buttonTutorialRecord
            // 
            buttonTutorialRecord.Location = new System.Drawing.Point(10, 60);
            buttonTutorialRecord.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonTutorialRecord.Name = "buttonTutorialRecord";
            buttonTutorialRecord.Size = new System.Drawing.Size(170, 27);
            buttonTutorialRecord.TabIndex = 7;
            buttonTutorialRecord.Text = "Recording Ghosts";
            buttonTutorialRecord.UseVisualStyleBackColor = true;
            buttonTutorialRecord.Click += buttonTutorialRecord_Click;
            // 
            // groupGhostHack
            // 
            groupGhostHack.Controls.Add(txtRAMOffsetBase);
            groupGhostHack.Controls.Add(lblRAMOffsetBase);
            groupGhostHack.Controls.Add(labelHackActiveState);
            groupGhostHack.Controls.Add(buttonDisableGhostHack);
            groupGhostHack.Controls.Add(buttonEnableGhostHack);
            groupGhostHack.Location = new System.Drawing.Point(4, 3);
            groupGhostHack.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupGhostHack.Name = "groupGhostHack";
            groupGhostHack.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupGhostHack.Size = new System.Drawing.Size(188, 189);
            groupGhostHack.TabIndex = 3;
            groupGhostHack.TabStop = false;
            groupGhostHack.Text = "Ghost Hack";
            // 
            // labelHackActiveState
            // 
            labelHackActiveState.AutoSize = true;
            labelHackActiveState.Location = new System.Drawing.Point(7, 95);
            labelHackActiveState.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelHackActiveState.Name = "labelHackActiveState";
            labelHackActiveState.Size = new System.Drawing.Size(143, 15);
            labelHackActiveState.TabIndex = 3;
            labelHackActiveState.Text = "Ghost hack is not enabled";
            // 
            // buttonDisableGhostHack
            // 
            buttonDisableGhostHack.Location = new System.Drawing.Point(7, 55);
            buttonDisableGhostHack.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonDisableGhostHack.Name = "buttonDisableGhostHack";
            buttonDisableGhostHack.Size = new System.Drawing.Size(167, 27);
            buttonDisableGhostHack.TabIndex = 0;
            buttonDisableGhostHack.Text = "Disable Ghost Hack";
            buttonDisableGhostHack.UseVisualStyleBackColor = true;
            buttonDisableGhostHack.Click += buttonDisableGhostHack_Click;
            // 
            // buttonEnableGhostHack
            // 
            buttonEnableGhostHack.Location = new System.Drawing.Point(7, 22);
            buttonEnableGhostHack.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonEnableGhostHack.Name = "buttonEnableGhostHack";
            buttonEnableGhostHack.Size = new System.Drawing.Size(167, 27);
            buttonEnableGhostHack.TabIndex = 0;
            buttonEnableGhostHack.Text = "Enable Ghost Hack";
            buttonEnableGhostHack.UseVisualStyleBackColor = true;
            buttonEnableGhostHack.Click += buttonEnableGhostHack_Click;
            // 
            // _variablePanelGhost
            // 
            _variablePanelGhost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
            _variablePanelGhost.AutoScroll = true;
            _variablePanelGhost.DataPath = "Config/GhostData.xml";
            _variablePanelGhost.elementNameWidth = null;
            _variablePanelGhost.elementValueWidth = null;
            _variablePanelGhost.Location = new System.Drawing.Point(7, 22);
            _variablePanelGhost.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            _variablePanelGhost.Name = "_variablePanelGhost";
            _variablePanelGhost.Size = new System.Drawing.Size(244, 498);
            _variablePanelGhost.TabIndex = 4;
            // 
            // groupBoxVariables
            // 
            groupBoxVariables.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Right));
            groupBoxVariables.Controls.Add(_variablePanelGhost);
            groupBoxVariables.Location = new System.Drawing.Point(806, 3);
            groupBoxVariables.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBoxVariables.Name = "groupBoxVariables";
            groupBoxVariables.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBoxVariables.Size = new System.Drawing.Size(258, 527);
            groupBoxVariables.TabIndex = 5;
            groupBoxVariables.TabStop = false;
            groupBoxVariables.Text = "Variables";
            // 
            // groupBoxHelp
            // 
            groupBoxHelp.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left));
            groupBoxHelp.Controls.Add(buttonTutorialFileWatch);
            groupBoxHelp.Controls.Add(buttonHelpGfxPool);
            groupBoxHelp.Controls.Add(buttonTutorialPlayback);
            groupBoxHelp.Controls.Add(buttonTutorialNotes);
            groupBoxHelp.Controls.Add(buttonTutorialRecord);
            groupBoxHelp.Location = new System.Drawing.Point(4, 332);
            groupBoxHelp.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBoxHelp.Name = "groupBoxHelp";
            groupBoxHelp.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBoxHelp.Size = new System.Drawing.Size(188, 198);
            groupBoxHelp.TabIndex = 8;
            groupBoxHelp.TabStop = false;
            groupBoxHelp.Text = "Help";
            // 
            // buttonTutorialFileWatch
            // 
            buttonTutorialFileWatch.Location = new System.Drawing.Point(10, 160);
            buttonTutorialFileWatch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonTutorialFileWatch.Name = "buttonTutorialFileWatch";
            buttonTutorialFileWatch.Size = new System.Drawing.Size(170, 27);
            buttonTutorialFileWatch.TabIndex = 7;
            buttonTutorialFileWatch.Text = "Using File Watchers";
            buttonTutorialFileWatch.UseVisualStyleBackColor = true;
            buttonTutorialFileWatch.Click += buttonTutorialFileWatch_Click;
            // 
            // buttonHelpGfxPool
            // 
            buttonHelpGfxPool.Location = new System.Drawing.Point(10, 127);
            buttonHelpGfxPool.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonHelpGfxPool.Name = "buttonHelpGfxPool";
            buttonHelpGfxPool.Size = new System.Drawing.Size(170, 27);
            buttonHelpGfxPool.TabIndex = 7;
            buttonHelpGfxPool.Text = "Moving the GFX Pool";
            buttonHelpGfxPool.UseVisualStyleBackColor = true;
            buttonHelpGfxPool.Click += buttonHelpGfxPool_Click;
            // 
            // buttonTutorialPlayback
            // 
            buttonTutorialPlayback.Location = new System.Drawing.Point(10, 93);
            buttonTutorialPlayback.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonTutorialPlayback.Name = "buttonTutorialPlayback";
            buttonTutorialPlayback.Size = new System.Drawing.Size(170, 27);
            buttonTutorialPlayback.TabIndex = 7;
            buttonTutorialPlayback.Text = "Playing Ghosts back";
            buttonTutorialPlayback.UseVisualStyleBackColor = true;
            buttonTutorialPlayback.Click += buttonTutorialPlayback_Click;
            // 
            // buttonTutorialNotes
            // 
            buttonTutorialNotes.Location = new System.Drawing.Point(10, 22);
            buttonTutorialNotes.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonTutorialNotes.Name = "buttonTutorialNotes";
            buttonTutorialNotes.Size = new System.Drawing.Size(170, 27);
            buttonTutorialNotes.TabIndex = 7;
            buttonTutorialNotes.Text = "General Notes";
            buttonTutorialNotes.UseVisualStyleBackColor = true;
            buttonTutorialNotes.Click += buttonTutorialNotes_Click;
            // 
            // groupBoxGfxPool
            // 
            groupBoxGfxPool.Controls.Add(textBoxPoolSize);
            groupBoxGfxPool.Controls.Add(textBoxPoolAddr2);
            groupBoxGfxPool.Controls.Add(textBoxPoolAddr1);
            groupBoxGfxPool.Controls.Add(labelPoolSize);
            groupBoxGfxPool.Controls.Add(label1);
            groupBoxGfxPool.Controls.Add(labelPool1Address);
            groupBoxGfxPool.Controls.Add(buttonMoveGfxPool);
            groupBoxGfxPool.Location = new System.Drawing.Point(4, 197);
            groupBoxGfxPool.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBoxGfxPool.Name = "groupBoxGfxPool";
            groupBoxGfxPool.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBoxGfxPool.Size = new System.Drawing.Size(181, 128);
            groupBoxGfxPool.TabIndex = 9;
            groupBoxGfxPool.TabStop = false;
            groupBoxGfxPool.Text = "Gfx Pool";
            // 
            // textBoxPoolSize
            // 
            textBoxPoolSize.Location = new System.Drawing.Point(93, 66);
            textBoxPoolSize.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBoxPoolSize.Name = "textBoxPoolSize";
            textBoxPoolSize.Size = new System.Drawing.Size(80, 23);
            textBoxPoolSize.TabIndex = 2;
            textBoxPoolSize.Text = "FFF00";
            // 
            // textBoxPoolAddr2
            // 
            textBoxPoolAddr2.Location = new System.Drawing.Point(93, 40);
            textBoxPoolAddr2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBoxPoolAddr2.Name = "textBoxPoolAddr2";
            textBoxPoolAddr2.Size = new System.Drawing.Size(80, 23);
            textBoxPoolAddr2.TabIndex = 2;
            textBoxPoolAddr2.Text = "80700000";
            // 
            // textBoxPoolAddr1
            // 
            textBoxPoolAddr1.Location = new System.Drawing.Point(93, 15);
            textBoxPoolAddr1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBoxPoolAddr1.Name = "textBoxPoolAddr1";
            textBoxPoolAddr1.Size = new System.Drawing.Size(80, 23);
            textBoxPoolAddr1.TabIndex = 2;
            textBoxPoolAddr1.Text = "80600000";
            // 
            // labelPoolSize
            // 
            labelPoolSize.AutoSize = true;
            labelPoolSize.Location = new System.Drawing.Point(27, 69);
            labelPoolSize.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelPoolSize.Name = "labelPoolSize";
            labelPoolSize.Size = new System.Drawing.Size(54, 15);
            labelPoolSize.TabIndex = 1;
            labelPoolSize.Text = "Pool Size";
            labelPoolSize.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(7, 44);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(75, 15);
            label1.TabIndex = 1;
            label1.Text = "Pool Addr. 2:";
            // 
            // labelPool1Address
            // 
            labelPool1Address.AutoSize = true;
            labelPool1Address.Location = new System.Drawing.Point(7, 18);
            labelPool1Address.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelPool1Address.Name = "labelPool1Address";
            labelPool1Address.Size = new System.Drawing.Size(75, 15);
            labelPool1Address.TabIndex = 1;
            labelPool1Address.Text = "Pool Addr. 1:";
            // 
            // buttonMoveGfxPool
            // 
            buttonMoveGfxPool.Location = new System.Drawing.Point(61, 95);
            buttonMoveGfxPool.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonMoveGfxPool.Name = "buttonMoveGfxPool";
            buttonMoveGfxPool.Size = new System.Drawing.Size(113, 27);
            buttonMoveGfxPool.TabIndex = 0;
            buttonMoveGfxPool.Text = "Move GFX Pool";
            buttonMoveGfxPool.UseVisualStyleBackColor = true;
            buttonMoveGfxPool.Click += buttonMoveGfxPool_Click;
            // 
            // lblRAMOffsetBase
            // 
            lblRAMOffsetBase.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left));
            lblRAMOffsetBase.Location = new System.Drawing.Point(11, 160);
            lblRAMOffsetBase.Name = "lblRAMOffsetBase";
            lblRAMOffsetBase.Size = new System.Drawing.Size(126, 23);
            lblRAMOffsetBase.TabIndex = 4;
            lblRAMOffsetBase.Text = "RAM offset base:";
            lblRAMOffsetBase.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtRAMOffsetBase
            // 
            txtRAMOffsetBase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
            txtRAMOffsetBase.Location = new System.Drawing.Point(143, 160);
            txtRAMOffsetBase.Name = "txtRAMOffsetBase";
            txtRAMOffsetBase.Size = new System.Drawing.Size(38, 23);
            txtRAMOffsetBase.TabIndex = 5;
            txtRAMOffsetBase.Text = "8040";
            // 
            // GhostTab
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            Controls.Add(groupBoxGfxPool);
            Controls.Add(groupBoxHelp);
            Controls.Add(groupBoxVariables);
            Controls.Add(groupBoxGhosts);
            Controls.Add(groupGhostHack);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBoxGhosts.ResumeLayout(false);
            groupBoxGhostInfo.ResumeLayout(false);
            groupBoxGhostInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownPlaybackOffset).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownStartOfPlayback).EndInit();
            groupGhostHack.ResumeLayout(false);
            groupGhostHack.PerformLayout();
            groupBoxVariables.ResumeLayout(false);
            groupBoxHelp.ResumeLayout(false);
            groupBoxGfxPool.ResumeLayout(false);
            groupBoxGfxPool.PerformLayout();
            ResumeLayout(false);
        }

        private System.Windows.Forms.Label lblRAMOffsetBase;
        private System.Windows.Forms.TextBox txtRAMOffsetBase;

        #endregion

        private System.Windows.Forms.GroupBox groupBoxGhosts;
        private System.Windows.Forms.ListBox listBoxGhosts;
        private System.Windows.Forms.Button buttonLoadGhost;
        private System.Windows.Forms.GroupBox groupBoxGhostInfo;
        private System.Windows.Forms.Label labelBaseGlobalTimer;
        private System.Windows.Forms.NumericUpDown numericUpDownStartOfPlayback;
        private System.Windows.Forms.Button buttonSaveGhost;
        private System.Windows.Forms.GroupBox groupGhostHack;
        private System.Windows.Forms.Button buttonDisableGhostHack;
        private System.Windows.Forms.Button buttonEnableGhostHack;
        public System.Windows.Forms.Label labelNumFrames;
        public System.Windows.Forms.Label labelGhostFile;
        public System.Windows.Forms.Label labelGhostPlaybackStart;
        public System.Windows.Forms.Label labelHackActiveState;
        private System.Windows.Forms.TextBox textBoxGhostName;
        public System.Windows.Forms.Label labelName;
        private STROOP.Controls.VariablePanel.VariablePanel _variablePanelGhost;
        private System.Windows.Forms.Button buttonWatchGhostFile;
        private System.Windows.Forms.Button buttonTutorialRecord;
        private System.Windows.Forms.GroupBox groupBoxVariables;
        private System.Windows.Forms.GroupBox groupBoxHelp;
        private System.Windows.Forms.Button buttonTutorialPlayback;
        private System.Windows.Forms.Button buttonTutorialNotes;
        private System.Windows.Forms.Button buttonTutorialFileWatch;
        private System.Windows.Forms.Button buttonGhostColor;
        private System.Windows.Forms.CheckBox checkTransparentGhosts;
        private System.Windows.Forms.Button buttonMarioColor;
        private System.Windows.Forms.GroupBox groupBoxGfxPool;
        private System.Windows.Forms.Button buttonMoveGfxPool;
        private System.Windows.Forms.TextBox textBoxPoolSize;
        private System.Windows.Forms.TextBox textBoxPoolAddr2;
        private System.Windows.Forms.TextBox textBoxPoolAddr1;
        private System.Windows.Forms.Label labelPoolSize;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelPool1Address;
        private System.Windows.Forms.Button buttonHelpGfxPool;
        private System.Windows.Forms.Label lblPlaybackOffset;
        private System.Windows.Forms.NumericUpDown numericUpDownPlaybackOffset;
    }
}
