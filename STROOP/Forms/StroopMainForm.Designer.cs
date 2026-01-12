using STROOP.Controls;
using System.Windows.Forms;

namespace STROOP
{
    partial class StroopMainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StroopMainForm));
            this.labelProcessSelect = new System.Windows.Forms.Label();
            this.labelVersionNumber = new System.Windows.Forms.Label();
            this.buttonDisconnect = new System.Windows.Forms.Button();
            this.panelConnect = new System.Windows.Forms.Panel();
            this.buttonRefreshAndConnect = new System.Windows.Forms.Button();
            this.buttonBypass = new System.Windows.Forms.Button();
            this.buttonProcessOptions = new System.Windows.Forms.Button();
            this.buttonOpenSavestate = new System.Windows.Forms.Button();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.labelNotConnected = new System.Windows.Forms.Label();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.listBoxProcessesList = new System.Windows.Forms.ListBox();
            this.contextMenuStripProcessesList = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.itemShowSimilarProcesses = new System.Windows.Forms.ToolStripMenuItem();
            this.labelFpsCounter = new System.Windows.Forms.Label();
            this.comboBoxRomVersion = new System.Windows.Forms.ComboBox();
            this.comboBoxReadWriteMode = new System.Windows.Forms.ComboBox();
            this.labelDebugText = new System.Windows.Forms.Label();
            this.openFileDialogSt = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialogSt = new System.Windows.Forms.SaveFileDialog();
            this.pictureBoxCog = new System.Windows.Forms.PictureBox();
            this.buttonShowTopPane = new System.Windows.Forms.Button();
            this.buttonShowTopBottomPane = new System.Windows.Forms.Button();
            this.buttonShowBottomPane = new System.Windows.Forms.Button();
            this.buttonShowRightPane = new System.Windows.Forms.Button();
            this.buttonShowLeftRightPane = new System.Windows.Forms.Button();
            this.buttonTabAdd = new System.Windows.Forms.Button();
            this.buttonMoveTabLeft = new System.Windows.Forms.Button();
            this.buttonMoveTabRight = new System.Windows.Forms.Button();
            this.buttonShowLeftPane = new System.Windows.Forms.Button();
            this.splitContainerMain = new STROOP.BetterSplitContainer();
            this.tabControlMain = new STROOP.TabControlEx();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBoxObjects = new System.Windows.Forms.GroupBox();
            this.comboBoxSelectionMethod = new System.Windows.Forms.ComboBox();
            this.labelSelectionMethod = new System.Windows.Forms.Label();
            this.comboBoxLabelMethod = new System.Windows.Forms.ComboBox();
            this.labelLabelMethod = new System.Windows.Forms.Label();
            this.labelSortMethod = new System.Windows.Forms.Label();
            this.comboBoxSortMethod = new System.Windows.Forms.ComboBox();
            this.labelSlotSize = new System.Windows.Forms.Label();
            this.VariablePanelObjects = new STROOP.Controls.ObjectSlotFlowLayoutPanel();
            this.trackBarObjSlotSize = new System.Windows.Forms.TrackBar();
            this.panelConnect.SuspendLayout();
            this.contextMenuStripProcessesList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCog)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
            this.tabControlMain.SuspendLayout();
            this.groupBoxObjects.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarObjSlotSize)).BeginInit();
            this.SuspendLayout();
            //
            // labelProcessSelect
            //
            this.labelProcessSelect.AutoSize = true;
            this.labelProcessSelect.Location = new System.Drawing.Point(145, 15);
            this.labelProcessSelect.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelProcessSelect.Name = "labelProcessSelect";
            this.labelProcessSelect.Size = new System.Drawing.Size(78, 13);
            this.labelProcessSelect.TabIndex = 1;
            this.labelProcessSelect.Text = "Connected To:";
            //
            // labelVersionNumber
            //
            this.labelVersionNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelVersionNumber.AutoSize = true;
            this.labelVersionNumber.Location = new System.Drawing.Point(868, 15);
            this.labelVersionNumber.Name = "labelVersionNumber";
            this.labelVersionNumber.Size = new System.Drawing.Size(41, 13);
            this.labelVersionNumber.TabIndex = 5;
            this.labelVersionNumber.Text = "version";
            this.labelVersionNumber.TextAlign = System.Drawing.ContentAlignment.TopRight;
            //
            // buttonDisconnect
            //
            this.buttonDisconnect.Location = new System.Drawing.Point(11, 11);
            this.buttonDisconnect.Margin = new System.Windows.Forms.Padding(2);
            this.buttonDisconnect.Name = "buttonDisconnect";
            this.buttonDisconnect.Size = new System.Drawing.Size(72, 21);
            this.buttonDisconnect.TabIndex = 17;
            this.buttonDisconnect.Text = "Disconnect";
            this.buttonDisconnect.UseVisualStyleBackColor = true;
            this.buttonDisconnect.Click += new System.EventHandler(this.buttonDisconnect_Click);
            //
            // panelConnect
            //
            this.panelConnect.Controls.Add(this.buttonRefreshAndConnect);
            this.panelConnect.Controls.Add(this.buttonBypass);
            this.panelConnect.Controls.Add(this.buttonProcessOptions);
            this.panelConnect.Controls.Add(this.buttonOpenSavestate);
            this.panelConnect.Controls.Add(this.buttonRefresh);
            this.panelConnect.Controls.Add(this.labelNotConnected);
            this.panelConnect.Controls.Add(this.buttonConnect);
            this.panelConnect.Controls.Add(this.listBoxProcessesList);
            this.panelConnect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelConnect.Location = new System.Drawing.Point(0, 0);
            this.panelConnect.Name = "panelConnect";
            this.panelConnect.Size = new System.Drawing.Size(947, 741);
            this.panelConnect.TabIndex = 17;
            //
            // buttonRefreshAndConnect
            //
            this.buttonRefreshAndConnect.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonRefreshAndConnect.Location = new System.Drawing.Point(476, 417);
            this.buttonRefreshAndConnect.Name = "buttonRefreshAndConnect";
            this.buttonRefreshAndConnect.Size = new System.Drawing.Size(112, 37);
            this.buttonRefreshAndConnect.TabIndex = 3;
            this.buttonRefreshAndConnect.Text = "Refresh && Connect";
            this.buttonRefreshAndConnect.UseVisualStyleBackColor = true;
            this.buttonRefreshAndConnect.Click += new System.EventHandler(this.buttonRefreshAndConnect_Click);
            //
            // buttonBypass
            //
            this.buttonBypass.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonBypass.Location = new System.Drawing.Point(359, 417);
            this.buttonBypass.Name = "buttonBypass";
            this.buttonBypass.Size = new System.Drawing.Size(113, 37);
            this.buttonBypass.TabIndex = 3;
            this.buttonBypass.Text = "Bypass";
            this.buttonBypass.UseVisualStyleBackColor = true;
            this.buttonBypass.Click += new System.EventHandler(this.buttonBypass_Click);
            //
            // buttonProcessOptions
            //
            this.buttonProcessOptions.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonProcessOptions.Location = new System.Drawing.Point(511, 460);
            this.buttonProcessOptions.Name = "buttonProcessOptions";
            this.buttonProcessOptions.Size = new System.Drawing.Size(77, 37);
            this.buttonProcessOptions.TabIndex = 3;
            this.buttonProcessOptions.Text = "Options";
            this.buttonProcessOptions.UseVisualStyleBackColor = true;
            this.buttonProcessOptions.Click += new System.EventHandler(this.buttonProcessOptions_Click);
            //
            // buttonOpenSavestate
            //
            this.buttonOpenSavestate.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonOpenSavestate.Location = new System.Drawing.Point(359, 460);
            this.buttonOpenSavestate.Name = "buttonOpenSavestate";
            this.buttonOpenSavestate.Size = new System.Drawing.Size(113, 37);
            this.buttonOpenSavestate.TabIndex = 3;
            this.buttonOpenSavestate.Text = "Open Savestate";
            this.buttonOpenSavestate.UseVisualStyleBackColor = true;
            this.buttonOpenSavestate.Click += new System.EventHandler(this.buttonOpenSavestate_Click);
            //
            // buttonRefresh
            //
            this.buttonRefresh.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonRefresh.Location = new System.Drawing.Point(359, 376);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(113, 37);
            this.buttonRefresh.TabIndex = 3;
            this.buttonRefresh.Text = "Refresh";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
            //
            // labelNotConnected
            //
            this.labelNotConnected.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labelNotConnected.AutoSize = true;
            this.labelNotConnected.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelNotConnected.Location = new System.Drawing.Point(394, 244);
            this.labelNotConnected.Name = "labelNotConnected";
            this.labelNotConnected.Size = new System.Drawing.Size(157, 26);
            this.labelNotConnected.TabIndex = 2;
            this.labelNotConnected.Text = "Not Connected";
            //
            // buttonConnect
            //
            this.buttonConnect.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonConnect.Location = new System.Drawing.Point(476, 376);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(112, 37);
            this.buttonConnect.TabIndex = 1;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            //
            // listBoxProcessesList
            //
            this.listBoxProcessesList.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.listBoxProcessesList.FormattingEnabled = true;
            this.listBoxProcessesList.Location = new System.Drawing.Point(359, 275);
            this.listBoxProcessesList.Name = "listBoxProcessesList";
            this.listBoxProcessesList.Size = new System.Drawing.Size(229, 95);
            this.listBoxProcessesList.TabIndex = 0;
            this.listBoxProcessesList.DoubleClick += new System.EventHandler(this.listBoxProcessesList_DoubleClick);
            //
            // contextMenuStripProcessesList
            //
            this.contextMenuStripProcessesList.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemShowSimilarProcesses});
            this.contextMenuStripProcessesList.Name = "contextMenuStripProcessesList";
            this.contextMenuStripProcessesList.Size = new System.Drawing.Size(197, 26);
            this.contextMenuStripProcessesList.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripProcessesList_Opening);
            //
            // itemShowSimilarProcesses
            //
            this.itemShowSimilarProcesses.CheckOnClick = true;
            this.itemShowSimilarProcesses.Name = "itemShowSimilarProcesses";
            this.itemShowSimilarProcesses.Size = new System.Drawing.Size(196, 22);
            this.itemShowSimilarProcesses.Text = "Show Similar Processes";
            this.itemShowSimilarProcesses.CheckedChanged += new System.EventHandler(this.itemShowSimilarProcesses_CheckedChanged);
            //
            // labelFpsCounter
            //
            this.labelFpsCounter.AutoSize = true;
            this.labelFpsCounter.Location = new System.Drawing.Point(88, 15);
            this.labelFpsCounter.Name = "labelFpsCounter";
            this.labelFpsCounter.Size = new System.Drawing.Size(39, 13);
            this.labelFpsCounter.TabIndex = 18;
            this.labelFpsCounter.Text = "FPS: 0";
            //
            // comboBoxRomVersion
            //
            this.comboBoxRomVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxRomVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxRomVersion.Location = new System.Drawing.Point(479, 11);
            this.comboBoxRomVersion.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxRomVersion.Name = "comboBoxRomVersion";
            this.comboBoxRomVersion.Size = new System.Drawing.Size(79, 21);
            this.comboBoxRomVersion.TabIndex = 22;
            //
            // comboBoxReadWriteMode
            //
            this.comboBoxReadWriteMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxReadWriteMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxReadWriteMode.Location = new System.Drawing.Point(562, 11);
            this.comboBoxReadWriteMode.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxReadWriteMode.Name = "comboBoxReadWriteMode";
            this.comboBoxReadWriteMode.Size = new System.Drawing.Size(75, 21);
            this.comboBoxReadWriteMode.TabIndex = 22;
            //
            // labelDebugText
            //
            this.labelDebugText.AutoSize = true;
            this.labelDebugText.BackColor = System.Drawing.Color.White;
            this.labelDebugText.Location = new System.Drawing.Point(271, 15);
            this.labelDebugText.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelDebugText.Name = "labelDebugText";
            this.labelDebugText.Size = new System.Drawing.Size(63, 13);
            this.labelDebugText.TabIndex = 1;
            this.labelDebugText.Text = "Debug Text";
            this.labelDebugText.Visible = false;
            //
            // openFileDialogSt
            //
            this.openFileDialogSt.Filter = "ST files |*.st;*.savestate|All files|*";
            //
            // saveFileDialogSt
            //
            this.saveFileDialogSt.Filter = "ST files |*.st;*.savestate";
            //
            // pictureBoxCog
            //
            this.pictureBoxCog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxCog.BackgroundImage = global::STROOP.Properties.Resources.cog;
            this.pictureBoxCog.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBoxCog.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBoxCog.Location = new System.Drawing.Point(842, 11);
            this.pictureBoxCog.Name = "pictureBoxCog";
            this.pictureBoxCog.Size = new System.Drawing.Size(20, 20);
            this.pictureBoxCog.TabIndex = 23;
            this.pictureBoxCog.TabStop = false;
            //
            // buttonShowTopPane
            //
            this.buttonShowTopPane.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonShowTopPane.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonShowTopPane.BackgroundImage")));
            this.buttonShowTopPane.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonShowTopPane.Location = new System.Drawing.Point(818, 11);
            this.buttonShowTopPane.Margin = new System.Windows.Forms.Padding(2);
            this.buttonShowTopPane.Name = "buttonShowTopPane";
            this.buttonShowTopPane.Size = new System.Drawing.Size(21, 21);
            this.buttonShowTopPane.TabIndex = 19;
            this.buttonShowTopPane.UseVisualStyleBackColor = true;
            this.buttonShowTopPane.Click += new System.EventHandler(this.buttonShowTopPanel_Click);
            //
            // buttonShowTopBottomPane
            //
            this.buttonShowTopBottomPane.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonShowTopBottomPane.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonShowTopBottomPane.BackgroundImage")));
            this.buttonShowTopBottomPane.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonShowTopBottomPane.Location = new System.Drawing.Point(793, 11);
            this.buttonShowTopBottomPane.Margin = new System.Windows.Forms.Padding(2);
            this.buttonShowTopBottomPane.Name = "buttonShowTopBottomPane";
            this.buttonShowTopBottomPane.Size = new System.Drawing.Size(21, 21);
            this.buttonShowTopBottomPane.TabIndex = 20;
            this.buttonShowTopBottomPane.UseVisualStyleBackColor = true;
            this.buttonShowTopBottomPane.Click += new System.EventHandler(this.buttonShowTopBottomPanel_Click);
            //
            // buttonShowBottomPane
            //
            this.buttonShowBottomPane.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonShowBottomPane.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonShowBottomPane.BackgroundImage")));
            this.buttonShowBottomPane.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonShowBottomPane.Location = new System.Drawing.Point(768, 11);
            this.buttonShowBottomPane.Margin = new System.Windows.Forms.Padding(2);
            this.buttonShowBottomPane.Name = "buttonShowBottomPane";
            this.buttonShowBottomPane.Size = new System.Drawing.Size(21, 21);
            this.buttonShowBottomPane.TabIndex = 20;
            this.buttonShowBottomPane.UseVisualStyleBackColor = true;
            this.buttonShowBottomPane.Click += new System.EventHandler(this.buttonShowBottomPanel_Click);
            //
            // buttonShowRightPane
            //
            this.buttonShowRightPane.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonShowRightPane.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonShowRightPane.BackgroundImage")));
            this.buttonShowRightPane.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonShowRightPane.Location = new System.Drawing.Point(743, 11);
            this.buttonShowRightPane.Margin = new System.Windows.Forms.Padding(2);
            this.buttonShowRightPane.Name = "buttonShowRightPane";
            this.buttonShowRightPane.Size = new System.Drawing.Size(21, 21);
            this.buttonShowRightPane.TabIndex = 19;
            this.buttonShowRightPane.UseVisualStyleBackColor = true;
            this.buttonShowRightPane.Click += new System.EventHandler(this.buttonShowRightPanel_Click);
            //
            // buttonShowLeftRightPane
            //
            this.buttonShowLeftRightPane.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonShowLeftRightPane.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonShowLeftRightPane.BackgroundImage")));
            this.buttonShowLeftRightPane.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonShowLeftRightPane.Location = new System.Drawing.Point(718, 11);
            this.buttonShowLeftRightPane.Margin = new System.Windows.Forms.Padding(2);
            this.buttonShowLeftRightPane.Name = "buttonShowLeftRightPane";
            this.buttonShowLeftRightPane.Size = new System.Drawing.Size(21, 21);
            this.buttonShowLeftRightPane.TabIndex = 20;
            this.buttonShowLeftRightPane.UseVisualStyleBackColor = true;
            this.buttonShowLeftRightPane.Click += new System.EventHandler(this.buttonShowLeftRightPanel_Click);
            //
            // buttonTabAdd
            //
            this.buttonTabAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTabAdd.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonTabAdd.BackgroundImage")));
            this.buttonTabAdd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonTabAdd.Location = new System.Drawing.Point(454, 11);
            this.buttonTabAdd.Margin = new System.Windows.Forms.Padding(2);
            this.buttonTabAdd.Name = "buttonTabAdd";
            this.buttonTabAdd.Size = new System.Drawing.Size(21, 21);
            this.buttonTabAdd.TabIndex = 20;
            this.buttonTabAdd.UseVisualStyleBackColor = true;
            this.buttonTabAdd.Click += new System.EventHandler(this.buttonTabAdd_Click);
            //
            // buttonMoveTabLeft
            //
            this.buttonMoveTabLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonMoveTabLeft.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonMoveTabLeft.BackgroundImage")));
            this.buttonMoveTabLeft.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonMoveTabLeft.Location = new System.Drawing.Point(404, 11);
            this.buttonMoveTabLeft.Margin = new System.Windows.Forms.Padding(2);
            this.buttonMoveTabLeft.Name = "buttonMoveTabLeft";
            this.buttonMoveTabLeft.Size = new System.Drawing.Size(21, 21);
            this.buttonMoveTabLeft.TabIndex = 20;
            this.buttonMoveTabLeft.UseVisualStyleBackColor = true;
            this.buttonMoveTabLeft.Click += new System.EventHandler(this.buttonMoveTabLeft_Click);
            //
            // buttonMoveTabRight
            //
            this.buttonMoveTabRight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonMoveTabRight.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonMoveTabRight.BackgroundImage")));
            this.buttonMoveTabRight.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonMoveTabRight.Location = new System.Drawing.Point(429, 11);
            this.buttonMoveTabRight.Margin = new System.Windows.Forms.Padding(2);
            this.buttonMoveTabRight.Name = "buttonMoveTabRight";
            this.buttonMoveTabRight.Size = new System.Drawing.Size(21, 21);
            this.buttonMoveTabRight.TabIndex = 20;
            this.buttonMoveTabRight.UseVisualStyleBackColor = true;
            this.buttonMoveTabRight.Click += new System.EventHandler(this.buttonMoveTabRight_Click);
            //
            // buttonShowLeftPane
            //
            this.buttonShowLeftPane.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonShowLeftPane.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonShowLeftPane.BackgroundImage")));
            this.buttonShowLeftPane.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonShowLeftPane.Location = new System.Drawing.Point(693, 11);
            this.buttonShowLeftPane.Margin = new System.Windows.Forms.Padding(2);
            this.buttonShowLeftPane.Name = "buttonShowLeftPane";
            this.buttonShowLeftPane.Size = new System.Drawing.Size(21, 21);
            this.buttonShowLeftPane.TabIndex = 20;
            this.buttonShowLeftPane.UseVisualStyleBackColor = true;
            this.buttonShowLeftPane.Click += new System.EventHandler(this.buttonShowLeftPanel_Click);
            //
            // splitContainerMain
            //
            this.splitContainerMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerMain.Location = new System.Drawing.Point(12, 36);
            this.splitContainerMain.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainerMain.Name = "splitContainerMain";
            this.splitContainerMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            //
            // splitContainerMain.Panel1
            //
            this.splitContainerMain.Panel1.Controls.Add(this.tabControlMain);
            this.splitContainerMain.Panel1MinSize = 0;
            //
            // splitContainerMain.Panel2
            //
            this.splitContainerMain.Panel2.Controls.Add(this.groupBoxObjects);
            this.splitContainerMain.Panel2MinSize = 0;
            this.splitContainerMain.Size = new System.Drawing.Size(927, 698);
            this.splitContainerMain.SplitterDistance = 491;
            this.splitContainerMain.SplitterWidth = 3;
            this.splitContainerMain.TabIndex = 4;
            //
            // tabControlMain
            //
            this.tabControlMain.AllowDrop = true;
            this.tabControlMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlMain.Controls.Add(this.tabPage1);
            this.tabControlMain.Cursor = System.Windows.Forms.Cursors.Default;
            this.tabControlMain.HotTrack = true;
            this.tabControlMain.Location = new System.Drawing.Point(2, 2);
            this.tabControlMain.Margin = new System.Windows.Forms.Padding(2);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(923, 489);
            this.tabControlMain.TabIndex = 3;
            //
            // tabPage1
            //
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(915, 463);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Dummy";
            this.tabPage1.UseVisualStyleBackColor = true;
            //
            // groupBoxObjects
            //
            this.groupBoxObjects.Controls.Add(this.comboBoxSelectionMethod);
            this.groupBoxObjects.Controls.Add(this.labelSelectionMethod);
            this.groupBoxObjects.Controls.Add(this.comboBoxLabelMethod);
            this.groupBoxObjects.Controls.Add(this.labelLabelMethod);
            this.groupBoxObjects.Controls.Add(this.labelSortMethod);
            this.groupBoxObjects.Controls.Add(this.comboBoxSortMethod);
            this.groupBoxObjects.Controls.Add(this.labelSlotSize);
            this.groupBoxObjects.Controls.Add(this.VariablePanelObjects);
            this.groupBoxObjects.Controls.Add(this.trackBarObjSlotSize);
            this.groupBoxObjects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxObjects.Location = new System.Drawing.Point(0, 0);
            this.groupBoxObjects.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxObjects.Name = "groupBoxObjects";
            this.groupBoxObjects.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxObjects.Size = new System.Drawing.Size(927, 204);
            this.groupBoxObjects.TabIndex = 2;
            this.groupBoxObjects.TabStop = false;
            this.groupBoxObjects.Text = "Objects";
            //
            // comboBoxSelectionMethod
            //
            this.comboBoxSelectionMethod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxSelectionMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSelectionMethod.Location = new System.Drawing.Point(456, 15);
            this.comboBoxSelectionMethod.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxSelectionMethod.Name = "comboBoxSelectionMethod";
            this.comboBoxSelectionMethod.Size = new System.Drawing.Size(82, 21);
            this.comboBoxSelectionMethod.TabIndex = 13;
            //
            // labelSelectionMethod
            //
            this.labelSelectionMethod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelSelectionMethod.AutoSize = true;
            this.labelSelectionMethod.Location = new System.Drawing.Point(362, 18);
            this.labelSelectionMethod.Name = "labelSelectionMethod";
            this.labelSelectionMethod.Size = new System.Drawing.Size(93, 13);
            this.labelSelectionMethod.TabIndex = 12;
            this.labelSelectionMethod.Text = "Selection Method:";
            //
            // comboBoxLabelMethod
            //
            this.comboBoxLabelMethod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxLabelMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLabelMethod.Location = new System.Drawing.Point(623, 15);
            this.comboBoxLabelMethod.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxLabelMethod.Name = "comboBoxLabelMethod";
            this.comboBoxLabelMethod.Size = new System.Drawing.Size(102, 21);
            this.comboBoxLabelMethod.TabIndex = 13;
            //
            // labelLabelMethod
            //
            this.labelLabelMethod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLabelMethod.AutoSize = true;
            this.labelLabelMethod.Location = new System.Drawing.Point(547, 18);
            this.labelLabelMethod.Name = "labelLabelMethod";
            this.labelLabelMethod.Size = new System.Drawing.Size(75, 13);
            this.labelLabelMethod.TabIndex = 12;
            this.labelLabelMethod.Text = "Label Method:";
            //
            // labelSortMethod
            //
            this.labelSortMethod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelSortMethod.AutoSize = true;
            this.labelSortMethod.Location = new System.Drawing.Point(738, 18);
            this.labelSortMethod.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelSortMethod.Name = "labelSortMethod";
            this.labelSortMethod.Size = new System.Drawing.Size(68, 13);
            this.labelSortMethod.TabIndex = 5;
            this.labelSortMethod.Text = "Sort Method:";
            //
            // comboBoxSortMethod
            //
            this.comboBoxSortMethod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxSortMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSortMethod.Location = new System.Drawing.Point(807, 15);
            this.comboBoxSortMethod.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxSortMethod.Name = "comboBoxSortMethod";
            this.comboBoxSortMethod.Size = new System.Drawing.Size(113, 21);
            this.comboBoxSortMethod.TabIndex = 4;
            //
            // labelSlotSize
            //
            this.labelSlotSize.AutoSize = true;
            this.labelSlotSize.Location = new System.Drawing.Point(110, 19);
            this.labelSlotSize.Name = "labelSlotSize";
            this.labelSlotSize.Size = new System.Drawing.Size(51, 13);
            this.labelSlotSize.TabIndex = 11;
            this.labelSlotSize.Text = "Slot Size:";
            //
            // VariablePanelObjects
            //
            this.VariablePanelObjects.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.VariablePanelObjects.AutoScroll = true;
            this.VariablePanelObjects.Location = new System.Drawing.Point(4, 45);
            this.VariablePanelObjects.Margin = new System.Windows.Forms.Padding(2);
            this.VariablePanelObjects.Name = "VariablePanelObjects";
            this.VariablePanelObjects.Size = new System.Drawing.Size(919, 155);
            this.VariablePanelObjects.TabIndex = 0;
            //
            // trackBarObjSlotSize
            //
            this.trackBarObjSlotSize.Location = new System.Drawing.Point(167, 15);
            this.trackBarObjSlotSize.Maximum = 100;
            this.trackBarObjSlotSize.Minimum = 15;
            this.trackBarObjSlotSize.Name = "trackBarObjSlotSize";
            this.trackBarObjSlotSize.Size = new System.Drawing.Size(104, 45);
            this.trackBarObjSlotSize.TabIndex = 3;
            this.trackBarObjSlotSize.TickFrequency = 10;
            this.trackBarObjSlotSize.Value = 36;
            this.trackBarObjSlotSize.ValueChanged += new System.EventHandler(this.trackBarObjSlotSize_ValueChanged);
            //
            // StroopMainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(947, 741);
            this.Controls.Add(this.labelDebugText);
            this.Controls.Add(this.panelConnect);
            this.Controls.Add(this.pictureBoxCog);
            this.Controls.Add(this.comboBoxReadWriteMode);
            this.Controls.Add(this.comboBoxRomVersion);
            this.Controls.Add(this.buttonShowTopPane);
            this.Controls.Add(this.buttonShowTopBottomPane);
            this.Controls.Add(this.buttonShowBottomPane);
            this.Controls.Add(this.buttonShowRightPane);
            this.Controls.Add(this.buttonShowLeftRightPane);
            this.Controls.Add(this.buttonTabAdd);
            this.Controls.Add(this.buttonMoveTabLeft);
            this.Controls.Add(this.buttonMoveTabRight);
            this.Controls.Add(this.buttonShowLeftPane);
            this.Controls.Add(this.buttonDisconnect);
            this.Controls.Add(this.labelVersionNumber);
            this.Controls.Add(this.splitContainerMain);
            this.Controls.Add(this.labelProcessSelect);
            this.Controls.Add(this.labelFpsCounter);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "StroopMainForm";
            this.Text = "STROOP";
            this.Load += new System.EventHandler(this.StroopMainForm_Load);
            this.panelConnect.ResumeLayout(false);
            this.panelConnect.PerformLayout();
            this.contextMenuStripProcessesList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCog)).EndInit();
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.ResumeLayout(false);
            this.tabControlMain.ResumeLayout(false);
            this.groupBoxObjects.ResumeLayout(false);
            this.groupBoxObjects.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarObjSlotSize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label labelProcessSelect;
        private System.Windows.Forms.GroupBox groupBoxObjects;
        internal System.Windows.Forms.ComboBox comboBoxSortMethod;
        private System.Windows.Forms.Label labelSortMethod;
        internal ObjectSlotFlowLayoutPanel VariablePanelObjects;
        private BetterSplitContainer splitContainerMain;
        private System.Windows.Forms.Label labelVersionNumber;
        private System.Windows.Forms.TrackBar trackBarObjSlotSize;
        private System.Windows.Forms.Label labelSlotSize;
        internal ComboBox comboBoxLabelMethod;
        private Label labelLabelMethod;
        private Button buttonDisconnect;
        private Panel panelConnect;
        private Button buttonRefresh;
        private Label labelNotConnected;
        private Button buttonConnect;
        private ListBox listBoxProcessesList;
        private ContextMenuStrip contextMenuStripProcessesList;
        private ToolStripMenuItem itemShowSimilarProcesses;
        private Label labelFpsCounter;
        private Button buttonShowTopPane;
        private Button buttonShowTopBottomPane;
        private Button buttonRefreshAndConnect;
        private Button buttonShowBottomPane;
        private Button buttonShowRightPane;
        private Button buttonShowLeftRightPane;
        private Button buttonShowLeftPane;
        private ComboBox comboBoxRomVersion;
        private ComboBox comboBoxReadWriteMode;
        private Button buttonBypass;
        private Button buttonMoveTabRight;
        private Button buttonMoveTabLeft;
        private PictureBox pictureBoxCog;
        private Label labelDebugText;
        private Button buttonTabAdd;
        private Button buttonOpenSavestate;
        private OpenFileDialog openFileDialogSt;
        private SaveFileDialog saveFileDialogSt;
        internal ComboBox comboBoxSelectionMethod;
        private Label labelSelectionMethod;
        internal TabControlEx tabControlMain;
        private TabPage tabPage1;
        private Button buttonProcessOptions;
    }
}

