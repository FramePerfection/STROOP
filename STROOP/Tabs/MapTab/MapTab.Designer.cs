namespace STROOP.Tabs.MapTab
{
    partial class MapTab
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapTab));
            this.splitContainerMap = new STROOP.BetterSplitContainer();
            this.splitContainerMapLeft = new STROOP.BetterSplitContainer();
            this.tabControlMap = new System.Windows.Forms.TabControl();
            this.tabPageMapOptions = new System.Windows.Forms.TabPage();
            this.comboBoxViewMode = new System.Windows.Forms.ComboBox();
            this.comboBoxMapOptionsBackground = new System.Windows.Forms.ComboBox();
            this.comboBoxMapOptionsLevel = new System.Windows.Forms.ComboBox();
            this.textBoxMapOptionsGlobalIconSize = new STROOP.BetterTextbox();
            this.labelMapOptionsGlobalIconSize = new System.Windows.Forms.Label();
            this.labelMapOptionsBackground = new System.Windows.Forms.Label();
            this.labelViewMode = new System.Windows.Forms.Label();
            this.labelMapOptionsLevel = new System.Windows.Forms.Label();
            this.buttonMapOptionsSaveTrackerSettings = new System.Windows.Forms.Button();
            this.buttonMapOptionsClearAllTrackers = new System.Windows.Forms.Button();
            this.buttonMapOptionsLoadTrackerSettings = new System.Windows.Forms.Button();
            this.buttonMapOptionsAddNewTracker = new System.Windows.Forms.Button();
            this.checkBoxMapOptionsScaleIconSizes = new System.Windows.Forms.CheckBox();
            this.checkBoxMapOptionsReverseDragging = new System.Windows.Forms.CheckBox();
            this.checkBoxMapOptionsEnablePuView = new System.Windows.Forms.CheckBox();
            this.checkBoxMapOptionsTrackUnitGridlines = new System.Windows.Forms.CheckBox();
            this.checkBoxMapOptionsTrackGhost = new System.Windows.Forms.CheckBox();
            this.checkBoxMapOptionsTrackCeilingTri = new System.Windows.Forms.CheckBox();
            this.checkBoxMapOptionsTrackWallTri = new System.Windows.Forms.CheckBox();
            this.checkBoxMapOptionsTrackFloorTri = new System.Windows.Forms.CheckBox();
            this.checkBoxMapOptionsTrackCamera = new System.Windows.Forms.CheckBox();
            this.checkBoxMapOptionsTrackHolp = new System.Windows.Forms.CheckBox();
            this.checkBoxMapOptionsTrackMario = new System.Windows.Forms.CheckBox();
            this.trackBarMapOptionsGlobalIconSize = new STROOP.TrackBarEx();
            this.tabPageMapControllers = new System.Windows.Forms.TabPage();
            this.groupBoxMapControllersAngle = new System.Windows.Forms.GroupBox();
            this.textBoxMapControllersAngleChange = new STROOP.BetterTextbox();
            this.textBoxMapControllersAngleCustom = new STROOP.BetterTextbox();
            this.buttonMapControllersAngleCCW = new System.Windows.Forms.Button();
            this.radioButtonMapControllersAngleCentripetal = new System.Windows.Forms.RadioButton();
            this.radioButtonMapControllersAngleCamera = new System.Windows.Forms.RadioButton();
            this.radioButtonMapControllersAngleMario = new System.Windows.Forms.RadioButton();
            this.radioButtonMapControllersAngle49152 = new System.Windows.Forms.RadioButton();
            this.buttonMapControllersAngleCW = new System.Windows.Forms.Button();
            this.radioButtonMapControllersAngle16384 = new System.Windows.Forms.RadioButton();
            this.radioButtonMapControllersAngle0 = new System.Windows.Forms.RadioButton();
            this.radioButtonMapControllersAngleCustom = new System.Windows.Forms.RadioButton();
            this.radioButtonMapControllersAngle32768 = new System.Windows.Forms.RadioButton();
            this.groupBoxMapControllersCenter = new System.Windows.Forms.GroupBox();
            this.checkBoxMapControllersCenterChangeByPixels = new System.Windows.Forms.CheckBox();
            this.textBoxMapControllersCenterCustom = new STROOP.BetterTextbox();
            this.textBoxMapControllersCenterChange = new STROOP.BetterTextbox();
            this.radioButtonMapControllersCenterMario = new System.Windows.Forms.RadioButton();
            this.radioButtonMapControllersCenterOrigin = new System.Windows.Forms.RadioButton();
            this.buttonMapControllersCenterDownRight = new System.Windows.Forms.Button();
            this.radioButtonMapControllersCenterBestFit = new System.Windows.Forms.RadioButton();
            this.buttonMapControllersCenterRight = new System.Windows.Forms.Button();
            this.radioButtonMapControllersCenterCustom = new System.Windows.Forms.RadioButton();
            this.buttonMapControllersCenterUpLeft = new System.Windows.Forms.Button();
            this.buttonMapControllersCenterLeft = new System.Windows.Forms.Button();
            this.buttonMapControllersCenterDownLeft = new System.Windows.Forms.Button();
            this.buttonMapControllersCenterDown = new System.Windows.Forms.Button();
            this.buttonMapControllersCenterUpRight = new System.Windows.Forms.Button();
            this.buttonMapControllersCenterUp = new System.Windows.Forms.Button();
            this.groupBoxMapControllersScale = new System.Windows.Forms.GroupBox();
            this.textBoxMapControllersScaleCustom = new STROOP.BetterTextbox();
            this.textBoxMapControllersScaleChange2 = new STROOP.BetterTextbox();
            this.textBoxMapControllersScaleChange = new STROOP.BetterTextbox();
            this.radioButtonMapControllersScaleMaxCourseSize = new System.Windows.Forms.RadioButton();
            this.radioButtonMapControllersScaleCourseDefault = new System.Windows.Forms.RadioButton();
            this.buttonMapControllersScaleDivide = new System.Windows.Forms.Button();
            this.buttonMapControllersScaleMinus = new System.Windows.Forms.Button();
            this.radioButtonMapControllersScaleCustom = new System.Windows.Forms.RadioButton();
            this.buttonMapControllersScaleTimes = new System.Windows.Forms.Button();
            this.buttonMapControllersScalePlus = new System.Windows.Forms.Button();
            this.tabPageMapData = new System.Windows.Forms.TabPage();
            this.labelMapDataQpuCoordinates = new System.Windows.Forms.Label();
            this.labelMapDataYNorm = new System.Windows.Forms.Label();
            this.labelMapDataYNormValue = new System.Windows.Forms.Label();
            this.labelMapDataId = new System.Windows.Forms.Label();
            this.labelMapDataIdValues = new System.Windows.Forms.Label();
            this.labelMapDataQpuCoordinateValues = new System.Windows.Forms.Label();
            this.labelMapDataPuCoordinateValues = new System.Windows.Forms.Label();
            this.labelMapDataMapSubName = new System.Windows.Forms.Label();
            this.labelMapDataMapName = new System.Windows.Forms.Label();
            this.labelMapDataPuCoordinates = new System.Windows.Forms.Label();
            this.flowLayoutPanelMapTrackers = new STROOP.Tabs.MapTab.MapTrackerFlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMap)).BeginInit();
            this.splitContainerMap.Panel1.SuspendLayout();
            this.splitContainerMap.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMapLeft)).BeginInit();
            this.splitContainerMapLeft.Panel1.SuspendLayout();
            this.splitContainerMapLeft.Panel2.SuspendLayout();
            this.splitContainerMapLeft.SuspendLayout();
            this.tabControlMap.SuspendLayout();
            this.tabPageMapOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarMapOptionsGlobalIconSize)).BeginInit();
            this.tabPageMapControllers.SuspendLayout();
            this.groupBoxMapControllersAngle.SuspendLayout();
            this.groupBoxMapControllersCenter.SuspendLayout();
            this.groupBoxMapControllersScale.SuspendLayout();
            this.tabPageMapData.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerMap
            // 
            this.splitContainerMap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerMap.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerMap.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerMap.Location = new System.Drawing.Point(0, 0);
            this.splitContainerMap.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainerMap.Name = "splitContainerMap";
            // 
            // splitContainerMap.Panel1
            // 
            this.splitContainerMap.Panel1.Controls.Add(this.splitContainerMapLeft);
            this.splitContainerMap.Panel1MinSize = 0;
            // 
            // splitContainerMap.Panel2
            // 
            this.splitContainerMap.Panel2.BackColor = System.Drawing.Color.Black;
            this.splitContainerMap.Panel2MinSize = 0;
            this.splitContainerMap.Size = new System.Drawing.Size(915, 463);
            this.splitContainerMap.SplitterDistance = 357;
            this.splitContainerMap.SplitterWidth = 1;
            this.splitContainerMap.TabIndex = 19;
            // 
            // splitContainerMapLeft
            // 
            this.splitContainerMapLeft.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerMapLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMapLeft.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerMapLeft.Location = new System.Drawing.Point(0, 0);
            this.splitContainerMapLeft.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainerMapLeft.Name = "splitContainerMapLeft";
            this.splitContainerMapLeft.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerMapLeft.Panel1
            // 
            this.splitContainerMapLeft.Panel1.Controls.Add(this.tabControlMap);
            this.splitContainerMapLeft.Panel1MinSize = 0;
            // 
            // splitContainerMapLeft.Panel2
            // 
            this.splitContainerMapLeft.Panel2.Controls.Add(this.flowLayoutPanelMapTrackers);
            this.splitContainerMapLeft.Panel2MinSize = 0;
            this.splitContainerMapLeft.Size = new System.Drawing.Size(357, 463);
            this.splitContainerMapLeft.SplitterDistance = 225;
            this.splitContainerMapLeft.SplitterWidth = 1;
            this.splitContainerMapLeft.TabIndex = 41;
            // 
            // tabControlMap
            // 
            this.tabControlMap.Controls.Add(this.tabPageMapOptions);
            this.tabControlMap.Controls.Add(this.tabPageMapControllers);
            this.tabControlMap.Controls.Add(this.tabPageMapData);
            this.tabControlMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlMap.Location = new System.Drawing.Point(0, 0);
            this.tabControlMap.Margin = new System.Windows.Forms.Padding(0);
            this.tabControlMap.Name = "tabControlMap";
            this.tabControlMap.SelectedIndex = 0;
            this.tabControlMap.Size = new System.Drawing.Size(355, 223);
            this.tabControlMap.TabIndex = 39;
            // 
            // tabPageMapOptions
            // 
            this.tabPageMapOptions.AutoScroll = true;
            this.tabPageMapOptions.BackColor = System.Drawing.SystemColors.Control;
            this.tabPageMapOptions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabPageMapOptions.Controls.Add(this.comboBoxViewMode);
            this.tabPageMapOptions.Controls.Add(this.comboBoxMapOptionsBackground);
            this.tabPageMapOptions.Controls.Add(this.comboBoxMapOptionsLevel);
            this.tabPageMapOptions.Controls.Add(this.textBoxMapOptionsGlobalIconSize);
            this.tabPageMapOptions.Controls.Add(this.labelMapOptionsGlobalIconSize);
            this.tabPageMapOptions.Controls.Add(this.labelMapOptionsBackground);
            this.tabPageMapOptions.Controls.Add(this.labelViewMode);
            this.tabPageMapOptions.Controls.Add(this.labelMapOptionsLevel);
            this.tabPageMapOptions.Controls.Add(this.buttonMapOptionsSaveTrackerSettings);
            this.tabPageMapOptions.Controls.Add(this.buttonMapOptionsClearAllTrackers);
            this.tabPageMapOptions.Controls.Add(this.buttonMapOptionsLoadTrackerSettings);
            this.tabPageMapOptions.Controls.Add(this.buttonMapOptionsAddNewTracker);
            this.tabPageMapOptions.Controls.Add(this.checkBoxMapOptionsScaleIconSizes);
            this.tabPageMapOptions.Controls.Add(this.checkBoxMapOptionsReverseDragging);
            this.tabPageMapOptions.Controls.Add(this.checkBoxMapOptionsEnablePuView);
            this.tabPageMapOptions.Controls.Add(this.checkBoxMapOptionsTrackUnitGridlines);
            this.tabPageMapOptions.Controls.Add(this.checkBoxMapOptionsTrackGhost);
            this.tabPageMapOptions.Controls.Add(this.checkBoxMapOptionsTrackCeilingTri);
            this.tabPageMapOptions.Controls.Add(this.checkBoxMapOptionsTrackWallTri);
            this.tabPageMapOptions.Controls.Add(this.checkBoxMapOptionsTrackFloorTri);
            this.tabPageMapOptions.Controls.Add(this.checkBoxMapOptionsTrackCamera);
            this.tabPageMapOptions.Controls.Add(this.checkBoxMapOptionsTrackHolp);
            this.tabPageMapOptions.Controls.Add(this.checkBoxMapOptionsTrackMario);
            this.tabPageMapOptions.Controls.Add(this.trackBarMapOptionsGlobalIconSize);
            this.tabPageMapOptions.Location = new System.Drawing.Point(4, 22);
            this.tabPageMapOptions.Margin = new System.Windows.Forms.Padding(0);
            this.tabPageMapOptions.Name = "tabPageMapOptions";
            this.tabPageMapOptions.Size = new System.Drawing.Size(347, 197);
            this.tabPageMapOptions.TabIndex = 3;
            this.tabPageMapOptions.Text = "Options";
            // 
            // comboBoxViewMode
            // 
            this.comboBoxViewMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxViewMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxViewMode.FormattingEnabled = true;
            this.comboBoxViewMode.Items.AddRange(new object[] {
            "TopDown",
            "Ortho",
            "3D"});
            this.comboBoxViewMode.Location = new System.Drawing.Point(269, 171);
            this.comboBoxViewMode.Name = "comboBoxViewMode";
            this.comboBoxViewMode.Size = new System.Drawing.Size(66, 21);
            this.comboBoxViewMode.TabIndex = 45;
            this.comboBoxViewMode.SelectedIndexChanged += new System.EventHandler(this.comboBoxViewMode_SelectedIndexChanged);
            // 
            // comboBoxMapOptionsBackground
            // 
            this.comboBoxMapOptionsBackground.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMapOptionsBackground.Location = new System.Drawing.Point(69, 171);
            this.comboBoxMapOptionsBackground.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxMapOptionsBackground.Name = "comboBoxMapOptionsBackground";
            this.comboBoxMapOptionsBackground.Size = new System.Drawing.Size(183, 21);
            this.comboBoxMapOptionsBackground.TabIndex = 13;
            // 
            // comboBoxMapOptionsLevel
            // 
            this.comboBoxMapOptionsLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMapOptionsLevel.Location = new System.Drawing.Point(69, 146);
            this.comboBoxMapOptionsLevel.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxMapOptionsLevel.Name = "comboBoxMapOptionsLevel";
            this.comboBoxMapOptionsLevel.Size = new System.Drawing.Size(183, 21);
            this.comboBoxMapOptionsLevel.TabIndex = 13;
            // 
            // textBoxMapOptionsGlobalIconSize
            // 
            this.textBoxMapOptionsGlobalIconSize.Location = new System.Drawing.Point(272, 134);
            this.textBoxMapOptionsGlobalIconSize.Name = "textBoxMapOptionsGlobalIconSize";
            this.textBoxMapOptionsGlobalIconSize.Size = new System.Drawing.Size(65, 20);
            this.textBoxMapOptionsGlobalIconSize.TabIndex = 43;
            this.textBoxMapOptionsGlobalIconSize.Text = "25";
            this.textBoxMapOptionsGlobalIconSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // labelMapOptionsGlobalIconSize
            // 
            this.labelMapOptionsGlobalIconSize.AutoSize = true;
            this.labelMapOptionsGlobalIconSize.Location = new System.Drawing.Point(129, 124);
            this.labelMapOptionsGlobalIconSize.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelMapOptionsGlobalIconSize.Name = "labelMapOptionsGlobalIconSize";
            this.labelMapOptionsGlobalIconSize.Size = new System.Drawing.Size(87, 13);
            this.labelMapOptionsGlobalIconSize.TabIndex = 44;
            this.labelMapOptionsGlobalIconSize.Text = "Global Icon Size:";
            // 
            // labelMapOptionsBackground
            // 
            this.labelMapOptionsBackground.AutoSize = true;
            this.labelMapOptionsBackground.Location = new System.Drawing.Point(3, 174);
            this.labelMapOptionsBackground.Name = "labelMapOptionsBackground";
            this.labelMapOptionsBackground.Size = new System.Drawing.Size(68, 13);
            this.labelMapOptionsBackground.TabIndex = 12;
            this.labelMapOptionsBackground.Text = "Background:";
            // 
            // labelViewMode
            // 
            this.labelViewMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelViewMode.AutoSize = true;
            this.labelViewMode.Location = new System.Drawing.Point(269, 158);
            this.labelViewMode.Name = "labelViewMode";
            this.labelViewMode.Size = new System.Drawing.Size(63, 13);
            this.labelViewMode.TabIndex = 12;
            this.labelViewMode.Text = "View Mode:";
            // 
            // labelMapOptionsLevel
            // 
            this.labelMapOptionsLevel.AutoSize = true;
            this.labelMapOptionsLevel.Location = new System.Drawing.Point(39, 149);
            this.labelMapOptionsLevel.Name = "labelMapOptionsLevel";
            this.labelMapOptionsLevel.Size = new System.Drawing.Size(31, 13);
            this.labelMapOptionsLevel.TabIndex = 12;
            this.labelMapOptionsLevel.Text = "Map:";
            // 
            // buttonMapOptionsSaveTrackerSettings
            // 
            this.buttonMapOptionsSaveTrackerSettings.Location = new System.Drawing.Point(243, 35);
            this.buttonMapOptionsSaveTrackerSettings.Margin = new System.Windows.Forms.Padding(0);
            this.buttonMapOptionsSaveTrackerSettings.Name = "buttonMapOptionsSaveTrackerSettings";
            this.buttonMapOptionsSaveTrackerSettings.Size = new System.Drawing.Size(85, 25);
            this.buttonMapOptionsSaveTrackerSettings.TabIndex = 40;
            this.buttonMapOptionsSaveTrackerSettings.Text = "Save Settings";
            this.buttonMapOptionsSaveTrackerSettings.UseVisualStyleBackColor = true;
            this.buttonMapOptionsSaveTrackerSettings.Click += new System.EventHandler(this.buttonMapOptionsSaveTrackerSettings_Click);
            // 
            // buttonMapOptionsClearAllTrackers
            // 
            this.buttonMapOptionsClearAllTrackers.Location = new System.Drawing.Point(155, 35);
            this.buttonMapOptionsClearAllTrackers.Margin = new System.Windows.Forms.Padding(0);
            this.buttonMapOptionsClearAllTrackers.Name = "buttonMapOptionsClearAllTrackers";
            this.buttonMapOptionsClearAllTrackers.Size = new System.Drawing.Size(85, 25);
            this.buttonMapOptionsClearAllTrackers.TabIndex = 40;
            this.buttonMapOptionsClearAllTrackers.Text = "Clear All Trackers";
            this.buttonMapOptionsClearAllTrackers.UseVisualStyleBackColor = true;
            // 
            // buttonMapOptionsLoadTrackerSettings
            // 
            this.buttonMapOptionsLoadTrackerSettings.Location = new System.Drawing.Point(243, 7);
            this.buttonMapOptionsLoadTrackerSettings.Margin = new System.Windows.Forms.Padding(0);
            this.buttonMapOptionsLoadTrackerSettings.Name = "buttonMapOptionsLoadTrackerSettings";
            this.buttonMapOptionsLoadTrackerSettings.Size = new System.Drawing.Size(85, 25);
            this.buttonMapOptionsLoadTrackerSettings.TabIndex = 41;
            this.buttonMapOptionsLoadTrackerSettings.Text = "Load Settings";
            this.buttonMapOptionsLoadTrackerSettings.UseVisualStyleBackColor = true;
            this.buttonMapOptionsLoadTrackerSettings.Click += new System.EventHandler(this.buttonMapOptionsLoadTrackerSettings_Click);
            // 
            // buttonMapOptionsAddNewTracker
            // 
            this.buttonMapOptionsAddNewTracker.Location = new System.Drawing.Point(155, 7);
            this.buttonMapOptionsAddNewTracker.Margin = new System.Windows.Forms.Padding(0);
            this.buttonMapOptionsAddNewTracker.Name = "buttonMapOptionsAddNewTracker";
            this.buttonMapOptionsAddNewTracker.Size = new System.Drawing.Size(85, 25);
            this.buttonMapOptionsAddNewTracker.TabIndex = 41;
            this.buttonMapOptionsAddNewTracker.Text = "Add Tracker";
            this.buttonMapOptionsAddNewTracker.UseVisualStyleBackColor = true;
            // 
            // checkBoxMapOptionsScaleIconSizes
            // 
            this.checkBoxMapOptionsScaleIconSizes.AutoSize = true;
            this.checkBoxMapOptionsScaleIconSizes.Location = new System.Drawing.Point(155, 99);
            this.checkBoxMapOptionsScaleIconSizes.Name = "checkBoxMapOptionsScaleIconSizes";
            this.checkBoxMapOptionsScaleIconSizes.Size = new System.Drawing.Size(105, 17);
            this.checkBoxMapOptionsScaleIconSizes.TabIndex = 19;
            this.checkBoxMapOptionsScaleIconSizes.Text = "Scale Icon Sizes";
            this.checkBoxMapOptionsScaleIconSizes.UseVisualStyleBackColor = true;
            // 
            // checkBoxMapOptionsReverseDragging
            // 
            this.checkBoxMapOptionsReverseDragging.AutoSize = true;
            this.checkBoxMapOptionsReverseDragging.Location = new System.Drawing.Point(155, 81);
            this.checkBoxMapOptionsReverseDragging.Name = "checkBoxMapOptionsReverseDragging";
            this.checkBoxMapOptionsReverseDragging.Size = new System.Drawing.Size(112, 17);
            this.checkBoxMapOptionsReverseDragging.TabIndex = 19;
            this.checkBoxMapOptionsReverseDragging.Text = "Reverse Dragging";
            this.checkBoxMapOptionsReverseDragging.UseVisualStyleBackColor = true;
            // 
            // checkBoxMapOptionsEnablePuView
            // 
            this.checkBoxMapOptionsEnablePuView.AutoSize = true;
            this.checkBoxMapOptionsEnablePuView.Location = new System.Drawing.Point(155, 63);
            this.checkBoxMapOptionsEnablePuView.Name = "checkBoxMapOptionsEnablePuView";
            this.checkBoxMapOptionsEnablePuView.Size = new System.Drawing.Size(103, 17);
            this.checkBoxMapOptionsEnablePuView.TabIndex = 19;
            this.checkBoxMapOptionsEnablePuView.Text = "Enable PU View";
            this.checkBoxMapOptionsEnablePuView.UseVisualStyleBackColor = true;
            // 
            // checkBoxMapOptionsTrackUnitGridlines
            // 
            this.checkBoxMapOptionsTrackUnitGridlines.AutoSize = true;
            this.checkBoxMapOptionsTrackUnitGridlines.Location = new System.Drawing.Point(14, 126);
            this.checkBoxMapOptionsTrackUnitGridlines.Name = "checkBoxMapOptionsTrackUnitGridlines";
            this.checkBoxMapOptionsTrackUnitGridlines.Size = new System.Drawing.Size(119, 17);
            this.checkBoxMapOptionsTrackUnitGridlines.TabIndex = 19;
            this.checkBoxMapOptionsTrackUnitGridlines.Text = "Track Unit Gridlines";
            this.checkBoxMapOptionsTrackUnitGridlines.UseVisualStyleBackColor = true;
            // 
            // checkBoxMapOptionsTrackGhost
            // 
            this.checkBoxMapOptionsTrackGhost.AutoSize = true;
            this.checkBoxMapOptionsTrackGhost.Location = new System.Drawing.Point(14, 41);
            this.checkBoxMapOptionsTrackGhost.Name = "checkBoxMapOptionsTrackGhost";
            this.checkBoxMapOptionsTrackGhost.Size = new System.Drawing.Size(85, 17);
            this.checkBoxMapOptionsTrackGhost.TabIndex = 19;
            this.checkBoxMapOptionsTrackGhost.Text = "Track Ghost";
            this.checkBoxMapOptionsTrackGhost.UseVisualStyleBackColor = true;
            // 
            // checkBoxMapOptionsTrackCeilingTri
            // 
            this.checkBoxMapOptionsTrackCeilingTri.AutoSize = true;
            this.checkBoxMapOptionsTrackCeilingTri.Location = new System.Drawing.Point(14, 109);
            this.checkBoxMapOptionsTrackCeilingTri.Name = "checkBoxMapOptionsTrackCeilingTri";
            this.checkBoxMapOptionsTrackCeilingTri.Size = new System.Drawing.Size(103, 17);
            this.checkBoxMapOptionsTrackCeilingTri.TabIndex = 19;
            this.checkBoxMapOptionsTrackCeilingTri.Text = "Track Ceiling Tri";
            this.checkBoxMapOptionsTrackCeilingTri.UseVisualStyleBackColor = true;
            // 
            // checkBoxMapOptionsTrackWallTri
            // 
            this.checkBoxMapOptionsTrackWallTri.AutoSize = true;
            this.checkBoxMapOptionsTrackWallTri.Location = new System.Drawing.Point(14, 92);
            this.checkBoxMapOptionsTrackWallTri.Name = "checkBoxMapOptionsTrackWallTri";
            this.checkBoxMapOptionsTrackWallTri.Size = new System.Drawing.Size(93, 17);
            this.checkBoxMapOptionsTrackWallTri.TabIndex = 19;
            this.checkBoxMapOptionsTrackWallTri.Text = "Track Wall Tri";
            this.checkBoxMapOptionsTrackWallTri.UseVisualStyleBackColor = true;
            // 
            // checkBoxMapOptionsTrackFloorTri
            // 
            this.checkBoxMapOptionsTrackFloorTri.AutoSize = true;
            this.checkBoxMapOptionsTrackFloorTri.Location = new System.Drawing.Point(14, 75);
            this.checkBoxMapOptionsTrackFloorTri.Name = "checkBoxMapOptionsTrackFloorTri";
            this.checkBoxMapOptionsTrackFloorTri.Size = new System.Drawing.Size(95, 17);
            this.checkBoxMapOptionsTrackFloorTri.TabIndex = 19;
            this.checkBoxMapOptionsTrackFloorTri.Text = "Track Floor Tri";
            this.checkBoxMapOptionsTrackFloorTri.UseVisualStyleBackColor = true;
            // 
            // checkBoxMapOptionsTrackCamera
            // 
            this.checkBoxMapOptionsTrackCamera.AutoSize = true;
            this.checkBoxMapOptionsTrackCamera.Location = new System.Drawing.Point(14, 58);
            this.checkBoxMapOptionsTrackCamera.Name = "checkBoxMapOptionsTrackCamera";
            this.checkBoxMapOptionsTrackCamera.Size = new System.Drawing.Size(93, 17);
            this.checkBoxMapOptionsTrackCamera.TabIndex = 19;
            this.checkBoxMapOptionsTrackCamera.Text = "Track Camera";
            this.checkBoxMapOptionsTrackCamera.UseVisualStyleBackColor = true;
            // 
            // checkBoxMapOptionsTrackHolp
            // 
            this.checkBoxMapOptionsTrackHolp.AutoSize = true;
            this.checkBoxMapOptionsTrackHolp.Location = new System.Drawing.Point(14, 24);
            this.checkBoxMapOptionsTrackHolp.Name = "checkBoxMapOptionsTrackHolp";
            this.checkBoxMapOptionsTrackHolp.Size = new System.Drawing.Size(86, 17);
            this.checkBoxMapOptionsTrackHolp.TabIndex = 19;
            this.checkBoxMapOptionsTrackHolp.Text = "Track HOLP";
            this.checkBoxMapOptionsTrackHolp.UseVisualStyleBackColor = true;
            // 
            // checkBoxMapOptionsTrackMario
            // 
            this.checkBoxMapOptionsTrackMario.AutoSize = true;
            this.checkBoxMapOptionsTrackMario.Location = new System.Drawing.Point(14, 7);
            this.checkBoxMapOptionsTrackMario.Name = "checkBoxMapOptionsTrackMario";
            this.checkBoxMapOptionsTrackMario.Size = new System.Drawing.Size(83, 17);
            this.checkBoxMapOptionsTrackMario.TabIndex = 19;
            this.checkBoxMapOptionsTrackMario.Text = "Track Mario";
            this.checkBoxMapOptionsTrackMario.UseVisualStyleBackColor = true;
            // 
            // trackBarMapOptionsGlobalIconSize
            // 
            this.trackBarMapOptionsGlobalIconSize.Location = new System.Drawing.Point(218, 114);
            this.trackBarMapOptionsGlobalIconSize.Margin = new System.Windows.Forms.Padding(0);
            this.trackBarMapOptionsGlobalIconSize.Maximum = 100;
            this.trackBarMapOptionsGlobalIconSize.Name = "trackBarMapOptionsGlobalIconSize";
            this.trackBarMapOptionsGlobalIconSize.Size = new System.Drawing.Size(119, 45);
            this.trackBarMapOptionsGlobalIconSize.TabIndex = 42;
            this.trackBarMapOptionsGlobalIconSize.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarMapOptionsGlobalIconSize.Value = 25;
            // 
            // tabPageMapControllers
            // 
            this.tabPageMapControllers.AutoScroll = true;
            this.tabPageMapControllers.BackColor = System.Drawing.SystemColors.Control;
            this.tabPageMapControllers.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabPageMapControllers.Controls.Add(this.groupBoxMapControllersAngle);
            this.tabPageMapControllers.Controls.Add(this.groupBoxMapControllersCenter);
            this.tabPageMapControllers.Controls.Add(this.groupBoxMapControllersScale);
            this.tabPageMapControllers.Location = new System.Drawing.Point(4, 22);
            this.tabPageMapControllers.Margin = new System.Windows.Forms.Padding(0);
            this.tabPageMapControllers.Name = "tabPageMapControllers";
            this.tabPageMapControllers.Size = new System.Drawing.Size(345, 197);
            this.tabPageMapControllers.TabIndex = 1;
            this.tabPageMapControllers.Text = "Controllers";
            // 
            // groupBoxMapControllersAngle
            // 
            this.groupBoxMapControllersAngle.Controls.Add(this.textBoxMapControllersAngleChange);
            this.groupBoxMapControllersAngle.Controls.Add(this.textBoxMapControllersAngleCustom);
            this.groupBoxMapControllersAngle.Controls.Add(this.buttonMapControllersAngleCCW);
            this.groupBoxMapControllersAngle.Controls.Add(this.radioButtonMapControllersAngleCentripetal);
            this.groupBoxMapControllersAngle.Controls.Add(this.radioButtonMapControllersAngleCamera);
            this.groupBoxMapControllersAngle.Controls.Add(this.radioButtonMapControllersAngleMario);
            this.groupBoxMapControllersAngle.Controls.Add(this.radioButtonMapControllersAngle49152);
            this.groupBoxMapControllersAngle.Controls.Add(this.buttonMapControllersAngleCW);
            this.groupBoxMapControllersAngle.Controls.Add(this.radioButtonMapControllersAngle16384);
            this.groupBoxMapControllersAngle.Controls.Add(this.radioButtonMapControllersAngle0);
            this.groupBoxMapControllersAngle.Controls.Add(this.radioButtonMapControllersAngleCustom);
            this.groupBoxMapControllersAngle.Controls.Add(this.radioButtonMapControllersAngle32768);
            this.groupBoxMapControllersAngle.Location = new System.Drawing.Point(3, 161);
            this.groupBoxMapControllersAngle.Name = "groupBoxMapControllersAngle";
            this.groupBoxMapControllersAngle.Size = new System.Drawing.Size(328, 103);
            this.groupBoxMapControllersAngle.TabIndex = 38;
            this.groupBoxMapControllersAngle.TabStop = false;
            this.groupBoxMapControllersAngle.Text = "Angle";
            // 
            // textBoxMapControllersAngleChange
            // 
            this.textBoxMapControllersAngleChange.Location = new System.Drawing.Point(226, 43);
            this.textBoxMapControllersAngleChange.Name = "textBoxMapControllersAngleChange";
            this.textBoxMapControllersAngleChange.Size = new System.Drawing.Size(66, 20);
            this.textBoxMapControllersAngleChange.TabIndex = 34;
            this.textBoxMapControllersAngleChange.Text = "8192";
            this.textBoxMapControllersAngleChange.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBoxMapControllersAngleCustom
            // 
            this.textBoxMapControllersAngleCustom.Location = new System.Drawing.Point(67, 77);
            this.textBoxMapControllersAngleCustom.Name = "textBoxMapControllersAngleCustom";
            this.textBoxMapControllersAngleCustom.Size = new System.Drawing.Size(117, 20);
            this.textBoxMapControllersAngleCustom.TabIndex = 34;
            this.textBoxMapControllersAngleCustom.Text = "0";
            this.textBoxMapControllersAngleCustom.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // buttonMapControllersAngleCCW
            // 
            this.buttonMapControllersAngleCCW.BackgroundImage = global::STROOP.Properties.Resources.image_counterclockwise;
            this.buttonMapControllersAngleCCW.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.buttonMapControllersAngleCCW.Location = new System.Drawing.Point(200, 40);
            this.buttonMapControllersAngleCCW.Name = "buttonMapControllersAngleCCW";
            this.buttonMapControllersAngleCCW.Size = new System.Drawing.Size(25, 25);
            this.buttonMapControllersAngleCCW.TabIndex = 39;
            this.buttonMapControllersAngleCCW.UseVisualStyleBackColor = true;
            // 
            // radioButtonMapControllersAngleCentripetal
            // 
            this.radioButtonMapControllersAngleCentripetal.AutoSize = true;
            this.radioButtonMapControllersAngleCentripetal.Location = new System.Drawing.Point(68, 46);
            this.radioButtonMapControllersAngleCentripetal.Name = "radioButtonMapControllersAngleCentripetal";
            this.radioButtonMapControllersAngleCentripetal.Size = new System.Drawing.Size(75, 17);
            this.radioButtonMapControllersAngleCentripetal.TabIndex = 12;
            this.radioButtonMapControllersAngleCentripetal.Text = "Centripetal";
            this.radioButtonMapControllersAngleCentripetal.UseVisualStyleBackColor = true;
            // 
            // radioButtonMapControllersAngleCamera
            // 
            this.radioButtonMapControllersAngleCamera.AutoSize = true;
            this.radioButtonMapControllersAngleCamera.Location = new System.Drawing.Point(68, 30);
            this.radioButtonMapControllersAngleCamera.Name = "radioButtonMapControllersAngleCamera";
            this.radioButtonMapControllersAngleCamera.Size = new System.Drawing.Size(61, 17);
            this.radioButtonMapControllersAngleCamera.TabIndex = 12;
            this.radioButtonMapControllersAngleCamera.Text = "Camera";
            this.radioButtonMapControllersAngleCamera.UseVisualStyleBackColor = true;
            // 
            // radioButtonMapControllersAngleMario
            // 
            this.radioButtonMapControllersAngleMario.AutoSize = true;
            this.radioButtonMapControllersAngleMario.Location = new System.Drawing.Point(68, 14);
            this.radioButtonMapControllersAngleMario.Name = "radioButtonMapControllersAngleMario";
            this.radioButtonMapControllersAngleMario.Size = new System.Drawing.Size(51, 17);
            this.radioButtonMapControllersAngleMario.TabIndex = 12;
            this.radioButtonMapControllersAngleMario.Text = "Mario";
            this.radioButtonMapControllersAngleMario.UseVisualStyleBackColor = true;
            // 
            // radioButtonMapControllersAngle49152
            // 
            this.radioButtonMapControllersAngle49152.AutoSize = true;
            this.radioButtonMapControllersAngle49152.Location = new System.Drawing.Point(9, 62);
            this.radioButtonMapControllersAngle49152.Name = "radioButtonMapControllersAngle49152";
            this.radioButtonMapControllersAngle49152.Size = new System.Drawing.Size(55, 17);
            this.radioButtonMapControllersAngle49152.TabIndex = 12;
            this.radioButtonMapControllersAngle49152.Text = "49152";
            this.radioButtonMapControllersAngle49152.UseVisualStyleBackColor = true;
            // 
            // buttonMapControllersAngleCW
            // 
            this.buttonMapControllersAngleCW.BackgroundImage = global::STROOP.Properties.Resources.image_clockwise;
            this.buttonMapControllersAngleCW.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.buttonMapControllersAngleCW.Location = new System.Drawing.Point(293, 40);
            this.buttonMapControllersAngleCW.Name = "buttonMapControllersAngleCW";
            this.buttonMapControllersAngleCW.Size = new System.Drawing.Size(25, 25);
            this.buttonMapControllersAngleCW.TabIndex = 39;
            this.buttonMapControllersAngleCW.UseVisualStyleBackColor = true;
            // 
            // radioButtonMapControllersAngle16384
            // 
            this.radioButtonMapControllersAngle16384.AutoSize = true;
            this.radioButtonMapControllersAngle16384.Location = new System.Drawing.Point(9, 30);
            this.radioButtonMapControllersAngle16384.Name = "radioButtonMapControllersAngle16384";
            this.radioButtonMapControllersAngle16384.Size = new System.Drawing.Size(55, 17);
            this.radioButtonMapControllersAngle16384.TabIndex = 12;
            this.radioButtonMapControllersAngle16384.Text = "16384";
            this.radioButtonMapControllersAngle16384.UseVisualStyleBackColor = true;
            // 
            // radioButtonMapControllersAngle0
            // 
            this.radioButtonMapControllersAngle0.AutoSize = true;
            this.radioButtonMapControllersAngle0.Location = new System.Drawing.Point(9, 14);
            this.radioButtonMapControllersAngle0.Name = "radioButtonMapControllersAngle0";
            this.radioButtonMapControllersAngle0.Size = new System.Drawing.Size(31, 17);
            this.radioButtonMapControllersAngle0.TabIndex = 11;
            this.radioButtonMapControllersAngle0.Text = "0";
            this.radioButtonMapControllersAngle0.UseVisualStyleBackColor = true;
            // 
            // radioButtonMapControllersAngleCustom
            // 
            this.radioButtonMapControllersAngleCustom.AutoSize = true;
            this.radioButtonMapControllersAngleCustom.Location = new System.Drawing.Point(9, 78);
            this.radioButtonMapControllersAngleCustom.Name = "radioButtonMapControllersAngleCustom";
            this.radioButtonMapControllersAngleCustom.Size = new System.Drawing.Size(63, 17);
            this.radioButtonMapControllersAngleCustom.TabIndex = 13;
            this.radioButtonMapControllersAngleCustom.Text = "Custom:";
            this.radioButtonMapControllersAngleCustom.UseVisualStyleBackColor = true;
            // 
            // radioButtonMapControllersAngle32768
            // 
            this.radioButtonMapControllersAngle32768.AutoSize = true;
            this.radioButtonMapControllersAngle32768.Checked = true;
            this.radioButtonMapControllersAngle32768.Location = new System.Drawing.Point(9, 46);
            this.radioButtonMapControllersAngle32768.Name = "radioButtonMapControllersAngle32768";
            this.radioButtonMapControllersAngle32768.Size = new System.Drawing.Size(55, 17);
            this.radioButtonMapControllersAngle32768.TabIndex = 13;
            this.radioButtonMapControllersAngle32768.TabStop = true;
            this.radioButtonMapControllersAngle32768.Text = "32768";
            this.radioButtonMapControllersAngle32768.UseVisualStyleBackColor = true;
            // 
            // groupBoxMapControllersCenter
            // 
            this.groupBoxMapControllersCenter.Controls.Add(this.checkBoxMapControllersCenterChangeByPixels);
            this.groupBoxMapControllersCenter.Controls.Add(this.textBoxMapControllersCenterCustom);
            this.groupBoxMapControllersCenter.Controls.Add(this.textBoxMapControllersCenterChange);
            this.groupBoxMapControllersCenter.Controls.Add(this.radioButtonMapControllersCenterMario);
            this.groupBoxMapControllersCenter.Controls.Add(this.radioButtonMapControllersCenterOrigin);
            this.groupBoxMapControllersCenter.Controls.Add(this.buttonMapControllersCenterDownRight);
            this.groupBoxMapControllersCenter.Controls.Add(this.radioButtonMapControllersCenterBestFit);
            this.groupBoxMapControllersCenter.Controls.Add(this.buttonMapControllersCenterRight);
            this.groupBoxMapControllersCenter.Controls.Add(this.radioButtonMapControllersCenterCustom);
            this.groupBoxMapControllersCenter.Controls.Add(this.buttonMapControllersCenterUpLeft);
            this.groupBoxMapControllersCenter.Controls.Add(this.buttonMapControllersCenterLeft);
            this.groupBoxMapControllersCenter.Controls.Add(this.buttonMapControllersCenterDownLeft);
            this.groupBoxMapControllersCenter.Controls.Add(this.buttonMapControllersCenterDown);
            this.groupBoxMapControllersCenter.Controls.Add(this.buttonMapControllersCenterUpRight);
            this.groupBoxMapControllersCenter.Controls.Add(this.buttonMapControllersCenterUp);
            this.groupBoxMapControllersCenter.Location = new System.Drawing.Point(3, 73);
            this.groupBoxMapControllersCenter.Name = "groupBoxMapControllersCenter";
            this.groupBoxMapControllersCenter.Size = new System.Drawing.Size(328, 87);
            this.groupBoxMapControllersCenter.TabIndex = 38;
            this.groupBoxMapControllersCenter.TabStop = false;
            this.groupBoxMapControllersCenter.Text = "Center";
            // 
            // checkBoxMapControllersCenterChangeByPixels
            // 
            this.checkBoxMapControllersCenterChangeByPixels.AutoSize = true;
            this.checkBoxMapControllersCenterChangeByPixels.Checked = true;
            this.checkBoxMapControllersCenterChangeByPixels.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxMapControllersCenterChangeByPixels.Location = new System.Drawing.Point(91, 15);
            this.checkBoxMapControllersCenterChangeByPixels.Name = "checkBoxMapControllersCenterChangeByPixels";
            this.checkBoxMapControllersCenterChangeByPixels.Size = new System.Drawing.Size(107, 17);
            this.checkBoxMapControllersCenterChangeByPixels.TabIndex = 40;
            this.checkBoxMapControllersCenterChangeByPixels.Text = "Change by Pixels";
            this.checkBoxMapControllersCenterChangeByPixels.UseVisualStyleBackColor = true;
            // 
            // textBoxMapControllersCenterCustom
            // 
            this.textBoxMapControllersCenterCustom.Location = new System.Drawing.Point(67, 61);
            this.textBoxMapControllersCenterCustom.Name = "textBoxMapControllersCenterCustom";
            this.textBoxMapControllersCenterCustom.Size = new System.Drawing.Size(117, 20);
            this.textBoxMapControllersCenterCustom.TabIndex = 34;
            this.textBoxMapControllersCenterCustom.Text = "0,0";
            this.textBoxMapControllersCenterCustom.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBoxMapControllersCenterChange
            // 
            this.textBoxMapControllersCenterChange.Location = new System.Drawing.Point(226, 36);
            this.textBoxMapControllersCenterChange.Name = "textBoxMapControllersCenterChange";
            this.textBoxMapControllersCenterChange.Size = new System.Drawing.Size(66, 20);
            this.textBoxMapControllersCenterChange.TabIndex = 34;
            this.textBoxMapControllersCenterChange.Text = "100";
            this.textBoxMapControllersCenterChange.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // radioButtonMapControllersCenterMario
            // 
            this.radioButtonMapControllersCenterMario.AutoSize = true;
            this.radioButtonMapControllersCenterMario.Location = new System.Drawing.Point(9, 46);
            this.radioButtonMapControllersCenterMario.Name = "radioButtonMapControllersCenterMario";
            this.radioButtonMapControllersCenterMario.Size = new System.Drawing.Size(51, 17);
            this.radioButtonMapControllersCenterMario.TabIndex = 12;
            this.radioButtonMapControllersCenterMario.Text = "Mario";
            this.radioButtonMapControllersCenterMario.UseVisualStyleBackColor = true;
            // 
            // radioButtonMapControllersCenterOrigin
            // 
            this.radioButtonMapControllersCenterOrigin.AutoSize = true;
            this.radioButtonMapControllersCenterOrigin.Location = new System.Drawing.Point(9, 30);
            this.radioButtonMapControllersCenterOrigin.Name = "radioButtonMapControllersCenterOrigin";
            this.radioButtonMapControllersCenterOrigin.Size = new System.Drawing.Size(52, 17);
            this.radioButtonMapControllersCenterOrigin.TabIndex = 12;
            this.radioButtonMapControllersCenterOrigin.Text = "Origin";
            this.radioButtonMapControllersCenterOrigin.UseVisualStyleBackColor = true;
            // 
            // buttonMapControllersCenterDownRight
            // 
            this.buttonMapControllersCenterDownRight.BackgroundImage = global::STROOP.Properties.Resources.image_downright;
            this.buttonMapControllersCenterDownRight.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.buttonMapControllersCenterDownRight.Location = new System.Drawing.Point(293, 58);
            this.buttonMapControllersCenterDownRight.Name = "buttonMapControllersCenterDownRight";
            this.buttonMapControllersCenterDownRight.Size = new System.Drawing.Size(25, 25);
            this.buttonMapControllersCenterDownRight.TabIndex = 39;
            this.buttonMapControllersCenterDownRight.UseVisualStyleBackColor = true;
            // 
            // radioButtonMapControllersCenterBestFit
            // 
            this.radioButtonMapControllersCenterBestFit.AutoSize = true;
            this.radioButtonMapControllersCenterBestFit.Checked = true;
            this.radioButtonMapControllersCenterBestFit.Location = new System.Drawing.Point(9, 14);
            this.radioButtonMapControllersCenterBestFit.Name = "radioButtonMapControllersCenterBestFit";
            this.radioButtonMapControllersCenterBestFit.Size = new System.Drawing.Size(60, 17);
            this.radioButtonMapControllersCenterBestFit.TabIndex = 11;
            this.radioButtonMapControllersCenterBestFit.TabStop = true;
            this.radioButtonMapControllersCenterBestFit.Text = "Best Fit";
            this.radioButtonMapControllersCenterBestFit.UseVisualStyleBackColor = true;
            // 
            // buttonMapControllersCenterRight
            // 
            this.buttonMapControllersCenterRight.BackgroundImage = global::STROOP.Properties.Resources.image_right;
            this.buttonMapControllersCenterRight.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.buttonMapControllersCenterRight.Location = new System.Drawing.Point(293, 33);
            this.buttonMapControllersCenterRight.Name = "buttonMapControllersCenterRight";
            this.buttonMapControllersCenterRight.Size = new System.Drawing.Size(25, 25);
            this.buttonMapControllersCenterRight.TabIndex = 39;
            this.buttonMapControllersCenterRight.UseVisualStyleBackColor = true;
            // 
            // radioButtonMapControllersCenterCustom
            // 
            this.radioButtonMapControllersCenterCustom.AutoSize = true;
            this.radioButtonMapControllersCenterCustom.Location = new System.Drawing.Point(9, 62);
            this.radioButtonMapControllersCenterCustom.Name = "radioButtonMapControllersCenterCustom";
            this.radioButtonMapControllersCenterCustom.Size = new System.Drawing.Size(63, 17);
            this.radioButtonMapControllersCenterCustom.TabIndex = 13;
            this.radioButtonMapControllersCenterCustom.Text = "Custom:";
            this.radioButtonMapControllersCenterCustom.UseVisualStyleBackColor = true;
            // 
            // buttonMapControllersCenterUpLeft
            // 
            this.buttonMapControllersCenterUpLeft.BackgroundImage = global::STROOP.Properties.Resources.image_upleft;
            this.buttonMapControllersCenterUpLeft.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.buttonMapControllersCenterUpLeft.Location = new System.Drawing.Point(200, 8);
            this.buttonMapControllersCenterUpLeft.Name = "buttonMapControllersCenterUpLeft";
            this.buttonMapControllersCenterUpLeft.Size = new System.Drawing.Size(25, 25);
            this.buttonMapControllersCenterUpLeft.TabIndex = 39;
            this.buttonMapControllersCenterUpLeft.UseVisualStyleBackColor = true;
            // 
            // buttonMapControllersCenterLeft
            // 
            this.buttonMapControllersCenterLeft.BackgroundImage = global::STROOP.Properties.Resources.image_left;
            this.buttonMapControllersCenterLeft.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.buttonMapControllersCenterLeft.Location = new System.Drawing.Point(200, 33);
            this.buttonMapControllersCenterLeft.Name = "buttonMapControllersCenterLeft";
            this.buttonMapControllersCenterLeft.Size = new System.Drawing.Size(25, 25);
            this.buttonMapControllersCenterLeft.TabIndex = 39;
            this.buttonMapControllersCenterLeft.UseVisualStyleBackColor = true;
            // 
            // buttonMapControllersCenterDownLeft
            // 
            this.buttonMapControllersCenterDownLeft.BackgroundImage = global::STROOP.Properties.Resources.image_downleft;
            this.buttonMapControllersCenterDownLeft.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.buttonMapControllersCenterDownLeft.Location = new System.Drawing.Point(200, 58);
            this.buttonMapControllersCenterDownLeft.Name = "buttonMapControllersCenterDownLeft";
            this.buttonMapControllersCenterDownLeft.Size = new System.Drawing.Size(25, 25);
            this.buttonMapControllersCenterDownLeft.TabIndex = 39;
            this.buttonMapControllersCenterDownLeft.UseVisualStyleBackColor = true;
            // 
            // buttonMapControllersCenterDown
            // 
            this.buttonMapControllersCenterDown.BackgroundImage = global::STROOP.Properties.Resources.image_down;
            this.buttonMapControllersCenterDown.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.buttonMapControllersCenterDown.Location = new System.Drawing.Point(246, 58);
            this.buttonMapControllersCenterDown.Name = "buttonMapControllersCenterDown";
            this.buttonMapControllersCenterDown.Size = new System.Drawing.Size(25, 25);
            this.buttonMapControllersCenterDown.TabIndex = 39;
            this.buttonMapControllersCenterDown.UseVisualStyleBackColor = true;
            // 
            // buttonMapControllersCenterUpRight
            // 
            this.buttonMapControllersCenterUpRight.BackgroundImage = global::STROOP.Properties.Resources.image_upright;
            this.buttonMapControllersCenterUpRight.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.buttonMapControllersCenterUpRight.Location = new System.Drawing.Point(293, 8);
            this.buttonMapControllersCenterUpRight.Name = "buttonMapControllersCenterUpRight";
            this.buttonMapControllersCenterUpRight.Size = new System.Drawing.Size(25, 25);
            this.buttonMapControllersCenterUpRight.TabIndex = 39;
            this.buttonMapControllersCenterUpRight.UseVisualStyleBackColor = true;
            // 
            // buttonMapControllersCenterUp
            // 
            this.buttonMapControllersCenterUp.BackgroundImage = global::STROOP.Properties.Resources.image_up;
            this.buttonMapControllersCenterUp.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.buttonMapControllersCenterUp.Location = new System.Drawing.Point(246, 8);
            this.buttonMapControllersCenterUp.Name = "buttonMapControllersCenterUp";
            this.buttonMapControllersCenterUp.Size = new System.Drawing.Size(25, 25);
            this.buttonMapControllersCenterUp.TabIndex = 39;
            this.buttonMapControllersCenterUp.UseVisualStyleBackColor = true;
            // 
            // groupBoxMapControllersScale
            // 
            this.groupBoxMapControllersScale.Controls.Add(this.textBoxMapControllersScaleCustom);
            this.groupBoxMapControllersScale.Controls.Add(this.textBoxMapControllersScaleChange2);
            this.groupBoxMapControllersScale.Controls.Add(this.textBoxMapControllersScaleChange);
            this.groupBoxMapControllersScale.Controls.Add(this.radioButtonMapControllersScaleMaxCourseSize);
            this.groupBoxMapControllersScale.Controls.Add(this.radioButtonMapControllersScaleCourseDefault);
            this.groupBoxMapControllersScale.Controls.Add(this.buttonMapControllersScaleDivide);
            this.groupBoxMapControllersScale.Controls.Add(this.buttonMapControllersScaleMinus);
            this.groupBoxMapControllersScale.Controls.Add(this.radioButtonMapControllersScaleCustom);
            this.groupBoxMapControllersScale.Controls.Add(this.buttonMapControllersScaleTimes);
            this.groupBoxMapControllersScale.Controls.Add(this.buttonMapControllersScalePlus);
            this.groupBoxMapControllersScale.Location = new System.Drawing.Point(3, 2);
            this.groupBoxMapControllersScale.Name = "groupBoxMapControllersScale";
            this.groupBoxMapControllersScale.Size = new System.Drawing.Size(328, 70);
            this.groupBoxMapControllersScale.TabIndex = 38;
            this.groupBoxMapControllersScale.TabStop = false;
            this.groupBoxMapControllersScale.Text = "Scale";
            // 
            // textBoxMapControllersScaleCustom
            // 
            this.textBoxMapControllersScaleCustom.Location = new System.Drawing.Point(67, 45);
            this.textBoxMapControllersScaleCustom.Name = "textBoxMapControllersScaleCustom";
            this.textBoxMapControllersScaleCustom.Size = new System.Drawing.Size(117, 20);
            this.textBoxMapControllersScaleCustom.TabIndex = 34;
            this.textBoxMapControllersScaleCustom.Text = "0";
            this.textBoxMapControllersScaleCustom.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBoxMapControllersScaleChange2
            // 
            this.textBoxMapControllersScaleChange2.Location = new System.Drawing.Point(226, 42);
            this.textBoxMapControllersScaleChange2.Name = "textBoxMapControllersScaleChange2";
            this.textBoxMapControllersScaleChange2.Size = new System.Drawing.Size(66, 20);
            this.textBoxMapControllersScaleChange2.TabIndex = 34;
            this.textBoxMapControllersScaleChange2.Text = "1.5";
            this.textBoxMapControllersScaleChange2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBoxMapControllersScaleChange
            // 
            this.textBoxMapControllersScaleChange.Location = new System.Drawing.Point(226, 16);
            this.textBoxMapControllersScaleChange.Name = "textBoxMapControllersScaleChange";
            this.textBoxMapControllersScaleChange.Size = new System.Drawing.Size(66, 20);
            this.textBoxMapControllersScaleChange.TabIndex = 34;
            this.textBoxMapControllersScaleChange.Text = "0.01";
            this.textBoxMapControllersScaleChange.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // radioButtonMapControllersScaleMaxCourseSize
            // 
            this.radioButtonMapControllersScaleMaxCourseSize.AutoSize = true;
            this.radioButtonMapControllersScaleMaxCourseSize.Location = new System.Drawing.Point(9, 30);
            this.radioButtonMapControllersScaleMaxCourseSize.Name = "radioButtonMapControllersScaleMaxCourseSize";
            this.radioButtonMapControllersScaleMaxCourseSize.Size = new System.Drawing.Size(104, 17);
            this.radioButtonMapControllersScaleMaxCourseSize.TabIndex = 12;
            this.radioButtonMapControllersScaleMaxCourseSize.Text = "Max Course Size";
            this.radioButtonMapControllersScaleMaxCourseSize.UseVisualStyleBackColor = true;
            // 
            // radioButtonMapControllersScaleCourseDefault
            // 
            this.radioButtonMapControllersScaleCourseDefault.AutoSize = true;
            this.radioButtonMapControllersScaleCourseDefault.Checked = true;
            this.radioButtonMapControllersScaleCourseDefault.Location = new System.Drawing.Point(9, 14);
            this.radioButtonMapControllersScaleCourseDefault.Name = "radioButtonMapControllersScaleCourseDefault";
            this.radioButtonMapControllersScaleCourseDefault.Size = new System.Drawing.Size(95, 17);
            this.radioButtonMapControllersScaleCourseDefault.TabIndex = 11;
            this.radioButtonMapControllersScaleCourseDefault.TabStop = true;
            this.radioButtonMapControllersScaleCourseDefault.Text = "Course Default";
            this.radioButtonMapControllersScaleCourseDefault.UseVisualStyleBackColor = true;
            // 
            // buttonMapControllersScaleDivide
            // 
            this.buttonMapControllersScaleDivide.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonMapControllersScaleDivide.BackgroundImage")));
            this.buttonMapControllersScaleDivide.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.buttonMapControllersScaleDivide.Location = new System.Drawing.Point(200, 39);
            this.buttonMapControllersScaleDivide.Name = "buttonMapControllersScaleDivide";
            this.buttonMapControllersScaleDivide.Size = new System.Drawing.Size(25, 25);
            this.buttonMapControllersScaleDivide.TabIndex = 39;
            this.buttonMapControllersScaleDivide.UseVisualStyleBackColor = true;
            // 
            // buttonMapControllersScaleMinus
            // 
            this.buttonMapControllersScaleMinus.BackgroundImage = global::STROOP.Properties.Resources.image_minus;
            this.buttonMapControllersScaleMinus.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.buttonMapControllersScaleMinus.Location = new System.Drawing.Point(200, 13);
            this.buttonMapControllersScaleMinus.Name = "buttonMapControllersScaleMinus";
            this.buttonMapControllersScaleMinus.Size = new System.Drawing.Size(25, 25);
            this.buttonMapControllersScaleMinus.TabIndex = 39;
            this.buttonMapControllersScaleMinus.UseVisualStyleBackColor = true;
            // 
            // radioButtonMapControllersScaleCustom
            // 
            this.radioButtonMapControllersScaleCustom.AutoSize = true;
            this.radioButtonMapControllersScaleCustom.Location = new System.Drawing.Point(9, 46);
            this.radioButtonMapControllersScaleCustom.Name = "radioButtonMapControllersScaleCustom";
            this.radioButtonMapControllersScaleCustom.Size = new System.Drawing.Size(63, 17);
            this.radioButtonMapControllersScaleCustom.TabIndex = 13;
            this.radioButtonMapControllersScaleCustom.Text = "Custom:";
            this.radioButtonMapControllersScaleCustom.UseVisualStyleBackColor = true;
            // 
            // buttonMapControllersScaleTimes
            // 
            this.buttonMapControllersScaleTimes.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonMapControllersScaleTimes.BackgroundImage")));
            this.buttonMapControllersScaleTimes.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.buttonMapControllersScaleTimes.Location = new System.Drawing.Point(293, 39);
            this.buttonMapControllersScaleTimes.Name = "buttonMapControllersScaleTimes";
            this.buttonMapControllersScaleTimes.Size = new System.Drawing.Size(25, 25);
            this.buttonMapControllersScaleTimes.TabIndex = 39;
            this.buttonMapControllersScaleTimes.UseVisualStyleBackColor = true;
            // 
            // buttonMapControllersScalePlus
            // 
            this.buttonMapControllersScalePlus.BackgroundImage = global::STROOP.Properties.Resources.image_plus;
            this.buttonMapControllersScalePlus.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.buttonMapControllersScalePlus.Location = new System.Drawing.Point(293, 13);
            this.buttonMapControllersScalePlus.Name = "buttonMapControllersScalePlus";
            this.buttonMapControllersScalePlus.Size = new System.Drawing.Size(25, 25);
            this.buttonMapControllersScalePlus.TabIndex = 39;
            this.buttonMapControllersScalePlus.UseVisualStyleBackColor = true;
            // 
            // tabPageMapData
            // 
            this.tabPageMapData.AutoScroll = true;
            this.tabPageMapData.BackColor = System.Drawing.SystemColors.Control;
            this.tabPageMapData.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabPageMapData.Controls.Add(this.labelMapDataQpuCoordinates);
            this.tabPageMapData.Controls.Add(this.labelMapDataYNorm);
            this.tabPageMapData.Controls.Add(this.labelMapDataYNormValue);
            this.tabPageMapData.Controls.Add(this.labelMapDataId);
            this.tabPageMapData.Controls.Add(this.labelMapDataIdValues);
            this.tabPageMapData.Controls.Add(this.labelMapDataQpuCoordinateValues);
            this.tabPageMapData.Controls.Add(this.labelMapDataPuCoordinateValues);
            this.tabPageMapData.Controls.Add(this.labelMapDataMapSubName);
            this.tabPageMapData.Controls.Add(this.labelMapDataMapName);
            this.tabPageMapData.Controls.Add(this.labelMapDataPuCoordinates);
            this.tabPageMapData.Location = new System.Drawing.Point(4, 22);
            this.tabPageMapData.Margin = new System.Windows.Forms.Padding(0);
            this.tabPageMapData.Name = "tabPageMapData";
            this.tabPageMapData.Size = new System.Drawing.Size(345, 197);
            this.tabPageMapData.TabIndex = 2;
            this.tabPageMapData.Text = "Data";
            // 
            // labelMapDataQpuCoordinates
            // 
            this.labelMapDataQpuCoordinates.AutoSize = true;
            this.labelMapDataQpuCoordinates.Location = new System.Drawing.Point(7, 67);
            this.labelMapDataQpuCoordinates.Name = "labelMapDataQpuCoordinates";
            this.labelMapDataQpuCoordinates.Size = new System.Drawing.Size(69, 13);
            this.labelMapDataQpuCoordinates.TabIndex = 56;
            this.labelMapDataQpuCoordinates.Text = "QPU [X:Y:Z]:";
            // 
            // labelMapDataYNorm
            // 
            this.labelMapDataYNorm.AutoSize = true;
            this.labelMapDataYNorm.Location = new System.Drawing.Point(5, 107);
            this.labelMapDataYNorm.Name = "labelMapDataYNorm";
            this.labelMapDataYNorm.Size = new System.Drawing.Size(71, 13);
            this.labelMapDataYNorm.TabIndex = 37;
            this.labelMapDataYNorm.Text = "Floor Y Norm:";
            // 
            // labelMapDataYNormValue
            // 
            this.labelMapDataYNormValue.AutoSize = true;
            this.labelMapDataYNormValue.Location = new System.Drawing.Point(81, 107);
            this.labelMapDataYNormValue.Name = "labelMapDataYNormValue";
            this.labelMapDataYNormValue.Size = new System.Drawing.Size(40, 13);
            this.labelMapDataYNormValue.TabIndex = 37;
            this.labelMapDataYNormValue.Text = "1.0000";
            // 
            // labelMapDataId
            // 
            this.labelMapDataId.AutoSize = true;
            this.labelMapDataId.Location = new System.Drawing.Point(25, 87);
            this.labelMapDataId.Name = "labelMapDataId";
            this.labelMapDataId.Size = new System.Drawing.Size(51, 13);
            this.labelMapDataId.TabIndex = 38;
            this.labelMapDataId.Text = "Location:";
            // 
            // labelMapDataIdValues
            // 
            this.labelMapDataIdValues.AutoSize = true;
            this.labelMapDataIdValues.Location = new System.Drawing.Point(81, 87);
            this.labelMapDataIdValues.Name = "labelMapDataIdValues";
            this.labelMapDataIdValues.Size = new System.Drawing.Size(46, 13);
            this.labelMapDataIdValues.TabIndex = 38;
            this.labelMapDataIdValues.Text = "[9:1:1:2]";
            // 
            // labelMapDataQpuCoordinateValues
            // 
            this.labelMapDataQpuCoordinateValues.AutoSize = true;
            this.labelMapDataQpuCoordinateValues.Location = new System.Drawing.Point(81, 67);
            this.labelMapDataQpuCoordinateValues.Name = "labelMapDataQpuCoordinateValues";
            this.labelMapDataQpuCoordinateValues.Size = new System.Drawing.Size(37, 13);
            this.labelMapDataQpuCoordinateValues.TabIndex = 39;
            this.labelMapDataQpuCoordinateValues.Text = "[0:0:0]";
            // 
            // labelMapDataPuCoordinateValues
            // 
            this.labelMapDataPuCoordinateValues.AutoSize = true;
            this.labelMapDataPuCoordinateValues.Location = new System.Drawing.Point(81, 47);
            this.labelMapDataPuCoordinateValues.Name = "labelMapDataPuCoordinateValues";
            this.labelMapDataPuCoordinateValues.Size = new System.Drawing.Size(37, 13);
            this.labelMapDataPuCoordinateValues.TabIndex = 40;
            this.labelMapDataPuCoordinateValues.Text = "[0:0:0]";
            // 
            // labelMapDataMapSubName
            // 
            this.labelMapDataMapSubName.AutoSize = true;
            this.labelMapDataMapSubName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMapDataMapSubName.Location = new System.Drawing.Point(7, 23);
            this.labelMapDataMapSubName.Name = "labelMapDataMapSubName";
            this.labelMapDataMapSubName.Size = new System.Drawing.Size(81, 13);
            this.labelMapDataMapSubName.TabIndex = 52;
            this.labelMapDataMapSubName.Text = "Map Sub Name";
            // 
            // labelMapDataMapName
            // 
            this.labelMapDataMapName.AutoSize = true;
            this.labelMapDataMapName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMapDataMapName.Location = new System.Drawing.Point(7, 7);
            this.labelMapDataMapName.Name = "labelMapDataMapName";
            this.labelMapDataMapName.Size = new System.Drawing.Size(67, 13);
            this.labelMapDataMapName.TabIndex = 54;
            this.labelMapDataMapName.Text = "Map Name";
            // 
            // labelMapDataPuCoordinates
            // 
            this.labelMapDataPuCoordinates.AutoSize = true;
            this.labelMapDataPuCoordinates.Location = new System.Drawing.Point(15, 47);
            this.labelMapDataPuCoordinates.Name = "labelMapDataPuCoordinates";
            this.labelMapDataPuCoordinates.Size = new System.Drawing.Size(61, 13);
            this.labelMapDataPuCoordinates.TabIndex = 41;
            this.labelMapDataPuCoordinates.Text = "PU [X:Y:Z]:";
            // 
            // flowLayoutPanelMapTrackers
            // 
            this.flowLayoutPanelMapTrackers.AutoScroll = true;
            this.flowLayoutPanelMapTrackers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelMapTrackers.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelMapTrackers.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanelMapTrackers.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanelMapTrackers.Name = "flowLayoutPanelMapTrackers";
            this.flowLayoutPanelMapTrackers.Size = new System.Drawing.Size(355, 235);
            this.flowLayoutPanelMapTrackers.TabIndex = 0;
            this.flowLayoutPanelMapTrackers.WrapContents = false;
            // 
            // MapTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainerMap);
            this.Name = "MapTab";
            this.splitContainerMap.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMap)).EndInit();
            this.splitContainerMap.ResumeLayout(false);
            this.splitContainerMapLeft.Panel1.ResumeLayout(false);
            this.splitContainerMapLeft.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMapLeft)).EndInit();
            this.splitContainerMapLeft.ResumeLayout(false);
            this.tabControlMap.ResumeLayout(false);
            this.tabPageMapOptions.ResumeLayout(false);
            this.tabPageMapOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarMapOptionsGlobalIconSize)).EndInit();
            this.tabPageMapControllers.ResumeLayout(false);
            this.groupBoxMapControllersAngle.ResumeLayout(false);
            this.groupBoxMapControllersAngle.PerformLayout();
            this.groupBoxMapControllersCenter.ResumeLayout(false);
            this.groupBoxMapControllersCenter.PerformLayout();
            this.groupBoxMapControllersScale.ResumeLayout(false);
            this.groupBoxMapControllersScale.PerformLayout();
            this.tabPageMapData.ResumeLayout(false);
            this.tabPageMapData.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal BetterSplitContainer splitContainerMap;
        internal BetterSplitContainer splitContainerMapLeft;
        internal System.Windows.Forms.TabControl tabControlMap;
        internal System.Windows.Forms.TabPage tabPageMapOptions;
        internal System.Windows.Forms.ComboBox comboBoxMapOptionsBackground;
        internal BetterTextbox textBoxMapOptionsGlobalIconSize;
        internal System.Windows.Forms.Label labelMapOptionsGlobalIconSize;
        internal System.Windows.Forms.Label labelMapOptionsBackground;
        internal System.Windows.Forms.Button buttonMapOptionsClearAllTrackers;
        internal System.Windows.Forms.Button buttonMapOptionsAddNewTracker;
        internal System.Windows.Forms.CheckBox checkBoxMapOptionsScaleIconSizes;
        internal System.Windows.Forms.CheckBox checkBoxMapOptionsReverseDragging;
        internal System.Windows.Forms.CheckBox checkBoxMapOptionsEnablePuView;
        internal System.Windows.Forms.CheckBox checkBoxMapOptionsTrackUnitGridlines;
        internal System.Windows.Forms.CheckBox checkBoxMapOptionsTrackGhost;
        internal System.Windows.Forms.CheckBox checkBoxMapOptionsTrackCeilingTri;
        internal System.Windows.Forms.CheckBox checkBoxMapOptionsTrackWallTri;
        internal System.Windows.Forms.CheckBox checkBoxMapOptionsTrackFloorTri;
        internal System.Windows.Forms.CheckBox checkBoxMapOptionsTrackCamera;
        internal System.Windows.Forms.CheckBox checkBoxMapOptionsTrackHolp;
        internal System.Windows.Forms.CheckBox checkBoxMapOptionsTrackMario;
        internal TrackBarEx trackBarMapOptionsGlobalIconSize;
        internal System.Windows.Forms.TabPage tabPageMapControllers;
        internal System.Windows.Forms.GroupBox groupBoxMapControllersAngle;
        internal BetterTextbox textBoxMapControllersAngleChange;
        internal BetterTextbox textBoxMapControllersAngleCustom;
        internal System.Windows.Forms.Button buttonMapControllersAngleCCW;
        internal System.Windows.Forms.RadioButton radioButtonMapControllersAngleCentripetal;
        internal System.Windows.Forms.RadioButton radioButtonMapControllersAngleCamera;
        internal System.Windows.Forms.RadioButton radioButtonMapControllersAngleMario;
        internal System.Windows.Forms.RadioButton radioButtonMapControllersAngle49152;
        internal System.Windows.Forms.Button buttonMapControllersAngleCW;
        internal System.Windows.Forms.RadioButton radioButtonMapControllersAngle16384;
        internal System.Windows.Forms.RadioButton radioButtonMapControllersAngle0;
        internal System.Windows.Forms.RadioButton radioButtonMapControllersAngleCustom;
        internal System.Windows.Forms.RadioButton radioButtonMapControllersAngle32768;
        internal System.Windows.Forms.GroupBox groupBoxMapControllersCenter;
        internal System.Windows.Forms.CheckBox checkBoxMapControllersCenterChangeByPixels;
        internal BetterTextbox textBoxMapControllersCenterCustom;
        internal BetterTextbox textBoxMapControllersCenterChange;
        internal System.Windows.Forms.RadioButton radioButtonMapControllersCenterMario;
        internal System.Windows.Forms.RadioButton radioButtonMapControllersCenterOrigin;
        internal System.Windows.Forms.Button buttonMapControllersCenterDownRight;
        internal System.Windows.Forms.RadioButton radioButtonMapControllersCenterBestFit;
        internal System.Windows.Forms.Button buttonMapControllersCenterRight;
        internal System.Windows.Forms.RadioButton radioButtonMapControllersCenterCustom;
        internal System.Windows.Forms.Button buttonMapControllersCenterUpLeft;
        internal System.Windows.Forms.Button buttonMapControllersCenterLeft;
        internal System.Windows.Forms.Button buttonMapControllersCenterDownLeft;
        internal System.Windows.Forms.Button buttonMapControllersCenterDown;
        internal System.Windows.Forms.Button buttonMapControllersCenterUpRight;
        internal System.Windows.Forms.Button buttonMapControllersCenterUp;
        internal System.Windows.Forms.GroupBox groupBoxMapControllersScale;
        internal BetterTextbox textBoxMapControllersScaleCustom;
        internal BetterTextbox textBoxMapControllersScaleChange2;
        internal BetterTextbox textBoxMapControllersScaleChange;
        internal System.Windows.Forms.RadioButton radioButtonMapControllersScaleMaxCourseSize;
        internal System.Windows.Forms.RadioButton radioButtonMapControllersScaleCourseDefault;
        internal System.Windows.Forms.Button buttonMapControllersScaleDivide;
        internal System.Windows.Forms.Button buttonMapControllersScaleMinus;
        internal System.Windows.Forms.RadioButton radioButtonMapControllersScaleCustom;
        internal System.Windows.Forms.Button buttonMapControllersScaleTimes;
        internal System.Windows.Forms.Button buttonMapControllersScalePlus;
        internal System.Windows.Forms.TabPage tabPageMapData;
        internal System.Windows.Forms.Label labelMapDataQpuCoordinates;
        internal System.Windows.Forms.Label labelMapDataYNorm;
        internal System.Windows.Forms.Label labelMapDataYNormValue;
        internal System.Windows.Forms.Label labelMapDataId;
        internal System.Windows.Forms.Label labelMapDataIdValues;
        internal System.Windows.Forms.Label labelMapDataQpuCoordinateValues;
        internal System.Windows.Forms.Label labelMapDataPuCoordinateValues;
        internal System.Windows.Forms.Label labelMapDataMapSubName;
        internal System.Windows.Forms.Label labelMapDataMapName;
        internal System.Windows.Forms.Label labelMapDataPuCoordinates;
        internal Tabs.MapTab.MapTrackerFlowLayoutPanel flowLayoutPanelMapTrackers;
        private System.Windows.Forms.ComboBox comboBoxViewMode;
        internal System.Windows.Forms.ComboBox comboBoxMapOptionsLevel;
        internal System.Windows.Forms.Label labelViewMode;
        internal System.Windows.Forms.Label labelMapOptionsLevel;
        internal System.Windows.Forms.Button buttonMapOptionsSaveTrackerSettings;
        internal System.Windows.Forms.Button buttonMapOptionsLoadTrackerSettings;
    }
}
