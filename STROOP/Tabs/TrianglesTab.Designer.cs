namespace STROOP.Tabs
{
    partial class TrianglesTab
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
            this.splitContainerTriangles = new STROOP.BetterSplitContainer();
            this.groupBoxTrianglePos = new System.Windows.Forms.GroupBox();
            this.checkBoxTrianglePosRelative = new System.Windows.Forms.CheckBox();
            this.textBoxTrianglePosY = new STROOP.BetterTextbox();
            this.buttonTrianglePosYp = new System.Windows.Forms.Button();
            this.buttonTrianglePosYn = new System.Windows.Forms.Button();
            this.buttonTrianglePosXpZp = new System.Windows.Forms.Button();
            this.textBoxTrianglePosXZ = new STROOP.BetterTextbox();
            this.buttonTrianglePosXp = new System.Windows.Forms.Button();
            this.buttonTrianglePosXpZn = new System.Windows.Forms.Button();
            this.buttonTrianglePosZn = new System.Windows.Forms.Button();
            this.buttonTrianglePosZp = new System.Windows.Forms.Button();
            this.buttonTrianglePosXnZp = new System.Windows.Forms.Button();
            this.buttonTrianglePosXn = new System.Windows.Forms.Button();
            this.buttonTrianglePosXnZn = new System.Windows.Forms.Button();
            this.groupBoxTriangleTypeConversion = new System.Windows.Forms.GroupBox();
            this.textBoxTriangleTypeConversionToType = new STROOP.BetterTextbox();
            this.labelTriangleTypeConversionToType = new System.Windows.Forms.Label();
            this.textBoxTriangleTypeConversionFromType = new STROOP.BetterTextbox();
            this.labelTriangleTypeConversionFromType = new System.Windows.Forms.Label();
            this.labelTriangleTypeConversionConvert = new System.Windows.Forms.Label();
            this.comboBoxTriangleTypeConversionConvert = new System.Windows.Forms.ComboBox();
            this.buttonTriangleTypeConversionConvert = new System.Windows.Forms.Button();
            this.groupBoxTriangleNormal = new System.Windows.Forms.GroupBox();
            this.textBoxTriangleNormal = new STROOP.BetterTextbox();
            this.buttonTriangleNormalN = new System.Windows.Forms.Button();
            this.buttonTriangleNormalP = new System.Windows.Forms.Button();
            this.radioButtonTriCustom = new System.Windows.Forms.RadioButton();
            this.buttonTriangleShowAddresses = new System.Windows.Forms.Button();
            this.buttonTriangleClearData = new System.Windows.Forms.Button();
            this.buttonTriangleDisableAllCamCollision = new System.Windows.Forms.Button();
            this.buttonTriangleShowVertices = new System.Windows.Forms.Button();
            this.buttonAnnihilateTriangle = new System.Windows.Forms.Button();
            this.radioButtonTriFloor = new System.Windows.Forms.RadioButton();
            this.buttonTriangleShowAllTris = new System.Windows.Forms.Button();
            this.buttonTriangleShowObjTris = new System.Windows.Forms.Button();
            this.buttonTriangleNeutralizeAllTriangles = new System.Windows.Forms.Button();
            this.buttonTriangleShowLevelTris = new System.Windows.Forms.Button();
            this.buttonTriangleShowData = new System.Windows.Forms.Button();
            this.buttonNeutralizeTriangle = new System.Windows.Forms.Button();
            this.radioButtonTriWall = new System.Windows.Forms.RadioButton();
            this.buttonTriangleShowCoords = new System.Windows.Forms.Button();
            this.buttonGotoVClosest = new System.Windows.Forms.Button();
            this.radioButtonTriCeiling = new System.Windows.Forms.RadioButton();
            this.checkBoxRepeatFirstVertex = new System.Windows.Forms.CheckBox();
            this.checkBoxNeutralizeTriangle = new System.Windows.Forms.CheckBox();
            this.checkBoxRecordTriangleData = new System.Windows.Forms.CheckBox();
            this.checkBoxVertexMisalignment = new System.Windows.Forms.CheckBox();
            this.textBoxCustomTriangle = new STROOP.BetterTextbox();
            this.buttonTriangleShowEquation = new System.Windows.Forms.Button();
            this.buttonRetrieveTriangle = new System.Windows.Forms.Button();
            this.labelRecordTriangleCount = new System.Windows.Forms.Label();
            this.labelTriangleSelection = new System.Windows.Forms.Label();
            this.buttonGotoV3 = new System.Windows.Forms.Button();
            this.buttonGotoV1 = new System.Windows.Forms.Button();
            this.buttonGotoV2 = new System.Windows.Forms.Button();
            this._variablePanelTriangles = new STROOP.Controls.VariablePanel.VariablePanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerTriangles)).BeginInit();
            this.splitContainerTriangles.Panel1.SuspendLayout();
            this.splitContainerTriangles.Panel2.SuspendLayout();
            this.splitContainerTriangles.SuspendLayout();
            this.groupBoxTrianglePos.SuspendLayout();
            this.groupBoxTriangleTypeConversion.SuspendLayout();
            this.groupBoxTriangleNormal.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerTriangles
            // 
            this.splitContainerTriangles.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerTriangles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerTriangles.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerTriangles.Location = new System.Drawing.Point(0, 0);
            this.splitContainerTriangles.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainerTriangles.Name = "splitContainerTriangles";
            // 
            // splitContainerTriangles.Panel1
            // 
            this.splitContainerTriangles.Panel1.AutoScroll = true;
            this.splitContainerTriangles.Panel1.Controls.Add(this.groupBoxTrianglePos);
            this.splitContainerTriangles.Panel1.Controls.Add(this.groupBoxTriangleTypeConversion);
            this.splitContainerTriangles.Panel1.Controls.Add(this.groupBoxTriangleNormal);
            this.splitContainerTriangles.Panel1.Controls.Add(this.radioButtonTriCustom);
            this.splitContainerTriangles.Panel1.Controls.Add(this.buttonTriangleShowAddresses);
            this.splitContainerTriangles.Panel1.Controls.Add(this.buttonTriangleClearData);
            this.splitContainerTriangles.Panel1.Controls.Add(this.buttonTriangleDisableAllCamCollision);
            this.splitContainerTriangles.Panel1.Controls.Add(this.buttonTriangleShowVertices);
            this.splitContainerTriangles.Panel1.Controls.Add(this.buttonAnnihilateTriangle);
            this.splitContainerTriangles.Panel1.Controls.Add(this.radioButtonTriFloor);
            this.splitContainerTriangles.Panel1.Controls.Add(this.buttonTriangleShowAllTris);
            this.splitContainerTriangles.Panel1.Controls.Add(this.buttonTriangleShowObjTris);
            this.splitContainerTriangles.Panel1.Controls.Add(this.buttonTriangleNeutralizeAllTriangles);
            this.splitContainerTriangles.Panel1.Controls.Add(this.buttonTriangleShowLevelTris);
            this.splitContainerTriangles.Panel1.Controls.Add(this.buttonTriangleShowData);
            this.splitContainerTriangles.Panel1.Controls.Add(this.buttonNeutralizeTriangle);
            this.splitContainerTriangles.Panel1.Controls.Add(this.radioButtonTriWall);
            this.splitContainerTriangles.Panel1.Controls.Add(this.buttonTriangleShowCoords);
            this.splitContainerTriangles.Panel1.Controls.Add(this.buttonGotoVClosest);
            this.splitContainerTriangles.Panel1.Controls.Add(this.radioButtonTriCeiling);
            this.splitContainerTriangles.Panel1.Controls.Add(this.checkBoxRepeatFirstVertex);
            this.splitContainerTriangles.Panel1.Controls.Add(this.checkBoxNeutralizeTriangle);
            this.splitContainerTriangles.Panel1.Controls.Add(this.checkBoxRecordTriangleData);
            this.splitContainerTriangles.Panel1.Controls.Add(this.checkBoxVertexMisalignment);
            this.splitContainerTriangles.Panel1.Controls.Add(this.textBoxCustomTriangle);
            this.splitContainerTriangles.Panel1.Controls.Add(this.buttonTriangleShowEquation);
            this.splitContainerTriangles.Panel1.Controls.Add(this.buttonRetrieveTriangle);
            this.splitContainerTriangles.Panel1.Controls.Add(this.labelRecordTriangleCount);
            this.splitContainerTriangles.Panel1.Controls.Add(this.labelTriangleSelection);
            this.splitContainerTriangles.Panel1.Controls.Add(this.buttonGotoV3);
            this.splitContainerTriangles.Panel1.Controls.Add(this.buttonGotoV1);
            this.splitContainerTriangles.Panel1.Controls.Add(this.buttonGotoV2);
            this.splitContainerTriangles.Panel1MinSize = 0;
            // 
            // splitContainerTriangles.Panel2
            // 
            this.splitContainerTriangles.Panel2.Controls.Add(this._variablePanelTriangles);
            this.splitContainerTriangles.Panel2.Padding = new System.Windows.Forms.Padding(2);
            this.splitContainerTriangles.Panel2MinSize = 0;
            this.splitContainerTriangles.Size = new System.Drawing.Size(915, 463);
            this.splitContainerTriangles.SplitterDistance = 208;
            this.splitContainerTriangles.SplitterWidth = 1;
            this.splitContainerTriangles.TabIndex = 33;
            // 
            // groupBoxTrianglePos
            // 
            this.groupBoxTrianglePos.Controls.Add(this.checkBoxTrianglePosRelative);
            this.groupBoxTrianglePos.Controls.Add(this.textBoxTrianglePosY);
            this.groupBoxTrianglePos.Controls.Add(this.buttonTrianglePosYp);
            this.groupBoxTrianglePos.Controls.Add(this.buttonTrianglePosYn);
            this.groupBoxTrianglePos.Controls.Add(this.buttonTrianglePosXpZp);
            this.groupBoxTrianglePos.Controls.Add(this.textBoxTrianglePosXZ);
            this.groupBoxTrianglePos.Controls.Add(this.buttonTrianglePosXp);
            this.groupBoxTrianglePos.Controls.Add(this.buttonTrianglePosXpZn);
            this.groupBoxTrianglePos.Controls.Add(this.buttonTrianglePosZn);
            this.groupBoxTrianglePos.Controls.Add(this.buttonTrianglePosZp);
            this.groupBoxTrianglePos.Controls.Add(this.buttonTrianglePosXnZp);
            this.groupBoxTrianglePos.Controls.Add(this.buttonTrianglePosXn);
            this.groupBoxTrianglePos.Controls.Add(this.buttonTrianglePosXnZn);
            this.groupBoxTrianglePos.Location = new System.Drawing.Point(2, 222);
            this.groupBoxTrianglePos.Name = "groupBoxTrianglePos";
            this.groupBoxTrianglePos.Size = new System.Drawing.Size(185, 146);
            this.groupBoxTrianglePos.TabIndex = 29;
            this.groupBoxTrianglePos.TabStop = false;
            this.groupBoxTrianglePos.Text = "Position";
            // 
            // checkBoxTrianglePosRelative
            // 
            this.checkBoxTrianglePosRelative.AutoSize = true;
            this.checkBoxTrianglePosRelative.Location = new System.Drawing.Point(118, 0);
            this.checkBoxTrianglePosRelative.Name = "checkBoxTrianglePosRelative";
            this.checkBoxTrianglePosRelative.Size = new System.Drawing.Size(65, 17);
            this.checkBoxTrianglePosRelative.TabIndex = 38;
            this.checkBoxTrianglePosRelative.Text = "Relative";
            this.checkBoxTrianglePosRelative.UseVisualStyleBackColor = true;
            // 
            // textBoxTrianglePosY
            // 
            this.textBoxTrianglePosY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxTrianglePosY.Location = new System.Drawing.Point(140, 70);
            this.textBoxTrianglePosY.Name = "textBoxTrianglePosY";
            this.textBoxTrianglePosY.Size = new System.Drawing.Size(42, 20);
            this.textBoxTrianglePosY.TabIndex = 33;
            this.textBoxTrianglePosY.Text = "50";
            this.textBoxTrianglePosY.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // buttonTrianglePosYp
            // 
            this.buttonTrianglePosYp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTrianglePosYp.Location = new System.Drawing.Point(140, 16);
            this.buttonTrianglePosYp.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTrianglePosYp.Name = "buttonTrianglePosYp";
            this.buttonTrianglePosYp.Size = new System.Drawing.Size(42, 42);
            this.buttonTrianglePosYp.TabIndex = 35;
            this.buttonTrianglePosYp.Text = "Y+";
            this.buttonTrianglePosYp.UseVisualStyleBackColor = true;
            // 
            // buttonTrianglePosYn
            // 
            this.buttonTrianglePosYn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTrianglePosYn.Location = new System.Drawing.Point(140, 100);
            this.buttonTrianglePosYn.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTrianglePosYn.Name = "buttonTrianglePosYn";
            this.buttonTrianglePosYn.Size = new System.Drawing.Size(42, 42);
            this.buttonTrianglePosYn.TabIndex = 34;
            this.buttonTrianglePosYn.Text = "Y-";
            this.buttonTrianglePosYn.UseVisualStyleBackColor = true;
            // 
            // buttonTrianglePosXpZp
            // 
            this.buttonTrianglePosXpZp.Location = new System.Drawing.Point(87, 100);
            this.buttonTrianglePosXpZp.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTrianglePosXpZp.Name = "buttonTrianglePosXpZp";
            this.buttonTrianglePosXpZp.Size = new System.Drawing.Size(42, 42);
            this.buttonTrianglePosXpZp.TabIndex = 32;
            this.buttonTrianglePosXpZp.Text = "X+Z+";
            this.buttonTrianglePosXpZp.UseVisualStyleBackColor = true;
            // 
            // textBoxTrianglePosXZ
            // 
            this.textBoxTrianglePosXZ.Location = new System.Drawing.Point(45, 70);
            this.textBoxTrianglePosXZ.Name = "textBoxTrianglePosXZ";
            this.textBoxTrianglePosXZ.Size = new System.Drawing.Size(42, 20);
            this.textBoxTrianglePosXZ.TabIndex = 27;
            this.textBoxTrianglePosXZ.Text = "50";
            this.textBoxTrianglePosXZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // buttonTrianglePosXp
            // 
            this.buttonTrianglePosXp.Location = new System.Drawing.Point(87, 58);
            this.buttonTrianglePosXp.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTrianglePosXp.Name = "buttonTrianglePosXp";
            this.buttonTrianglePosXp.Size = new System.Drawing.Size(42, 42);
            this.buttonTrianglePosXp.TabIndex = 31;
            this.buttonTrianglePosXp.Text = "X+";
            this.buttonTrianglePosXp.UseVisualStyleBackColor = true;
            // 
            // buttonTrianglePosXpZn
            // 
            this.buttonTrianglePosXpZn.Location = new System.Drawing.Point(87, 16);
            this.buttonTrianglePosXpZn.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTrianglePosXpZn.Name = "buttonTrianglePosXpZn";
            this.buttonTrianglePosXpZn.Size = new System.Drawing.Size(42, 42);
            this.buttonTrianglePosXpZn.TabIndex = 30;
            this.buttonTrianglePosXpZn.Text = "X+Z-";
            this.buttonTrianglePosXpZn.UseVisualStyleBackColor = true;
            // 
            // buttonTrianglePosZn
            // 
            this.buttonTrianglePosZn.Location = new System.Drawing.Point(45, 16);
            this.buttonTrianglePosZn.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTrianglePosZn.Name = "buttonTrianglePosZn";
            this.buttonTrianglePosZn.Size = new System.Drawing.Size(42, 42);
            this.buttonTrianglePosZn.TabIndex = 29;
            this.buttonTrianglePosZn.Text = "Z-";
            this.buttonTrianglePosZn.UseVisualStyleBackColor = true;
            // 
            // buttonTrianglePosZp
            // 
            this.buttonTrianglePosZp.Location = new System.Drawing.Point(45, 100);
            this.buttonTrianglePosZp.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTrianglePosZp.Name = "buttonTrianglePosZp";
            this.buttonTrianglePosZp.Size = new System.Drawing.Size(42, 42);
            this.buttonTrianglePosZp.TabIndex = 28;
            this.buttonTrianglePosZp.Text = "Z+";
            this.buttonTrianglePosZp.UseVisualStyleBackColor = true;
            // 
            // buttonTrianglePosXnZp
            // 
            this.buttonTrianglePosXnZp.Location = new System.Drawing.Point(3, 100);
            this.buttonTrianglePosXnZp.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTrianglePosXnZp.Name = "buttonTrianglePosXnZp";
            this.buttonTrianglePosXnZp.Size = new System.Drawing.Size(42, 42);
            this.buttonTrianglePosXnZp.TabIndex = 27;
            this.buttonTrianglePosXnZp.Text = "X-Z+";
            this.buttonTrianglePosXnZp.UseVisualStyleBackColor = true;
            // 
            // buttonTrianglePosXn
            // 
            this.buttonTrianglePosXn.Location = new System.Drawing.Point(3, 58);
            this.buttonTrianglePosXn.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTrianglePosXn.Name = "buttonTrianglePosXn";
            this.buttonTrianglePosXn.Size = new System.Drawing.Size(42, 42);
            this.buttonTrianglePosXn.TabIndex = 26;
            this.buttonTrianglePosXn.Text = "X-";
            this.buttonTrianglePosXn.UseVisualStyleBackColor = true;
            // 
            // buttonTrianglePosXnZn
            // 
            this.buttonTrianglePosXnZn.Location = new System.Drawing.Point(3, 16);
            this.buttonTrianglePosXnZn.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTrianglePosXnZn.Name = "buttonTrianglePosXnZn";
            this.buttonTrianglePosXnZn.Size = new System.Drawing.Size(42, 42);
            this.buttonTrianglePosXnZn.TabIndex = 25;
            this.buttonTrianglePosXnZn.Text = "X-Z-";
            this.buttonTrianglePosXnZn.UseVisualStyleBackColor = true;
            // 
            // groupBoxTriangleTypeConversion
            // 
            this.groupBoxTriangleTypeConversion.Controls.Add(this.textBoxTriangleTypeConversionToType);
            this.groupBoxTriangleTypeConversion.Controls.Add(this.labelTriangleTypeConversionToType);
            this.groupBoxTriangleTypeConversion.Controls.Add(this.textBoxTriangleTypeConversionFromType);
            this.groupBoxTriangleTypeConversion.Controls.Add(this.labelTriangleTypeConversionFromType);
            this.groupBoxTriangleTypeConversion.Controls.Add(this.labelTriangleTypeConversionConvert);
            this.groupBoxTriangleTypeConversion.Controls.Add(this.comboBoxTriangleTypeConversionConvert);
            this.groupBoxTriangleTypeConversion.Controls.Add(this.buttonTriangleTypeConversionConvert);
            this.groupBoxTriangleTypeConversion.Location = new System.Drawing.Point(2, 575);
            this.groupBoxTriangleTypeConversion.Name = "groupBoxTriangleTypeConversion";
            this.groupBoxTriangleTypeConversion.Size = new System.Drawing.Size(185, 127);
            this.groupBoxTriangleTypeConversion.TabIndex = 31;
            this.groupBoxTriangleTypeConversion.TabStop = false;
            this.groupBoxTriangleTypeConversion.Text = "Type Conversion";
            // 
            // textBoxTriangleTypeConversionToType
            // 
            this.textBoxTriangleTypeConversionToType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxTriangleTypeConversionToType.Location = new System.Drawing.Point(70, 70);
            this.textBoxTriangleTypeConversionToType.Name = "textBoxTriangleTypeConversionToType";
            this.textBoxTriangleTypeConversionToType.Size = new System.Drawing.Size(109, 20);
            this.textBoxTriangleTypeConversionToType.TabIndex = 33;
            this.textBoxTriangleTypeConversionToType.Text = "0x15";
            this.textBoxTriangleTypeConversionToType.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // labelTriangleTypeConversionToType
            // 
            this.labelTriangleTypeConversionToType.AutoSize = true;
            this.labelTriangleTypeConversionToType.Location = new System.Drawing.Point(7, 73);
            this.labelTriangleTypeConversionToType.Name = "labelTriangleTypeConversionToType";
            this.labelTriangleTypeConversionToType.Size = new System.Drawing.Size(50, 13);
            this.labelTriangleTypeConversionToType.TabIndex = 14;
            this.labelTriangleTypeConversionToType.Text = "To Type:";
            // 
            // textBoxTriangleTypeConversionFromType
            // 
            this.textBoxTriangleTypeConversionFromType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxTriangleTypeConversionFromType.Location = new System.Drawing.Point(70, 44);
            this.textBoxTriangleTypeConversionFromType.Name = "textBoxTriangleTypeConversionFromType";
            this.textBoxTriangleTypeConversionFromType.Size = new System.Drawing.Size(109, 20);
            this.textBoxTriangleTypeConversionFromType.TabIndex = 33;
            this.textBoxTriangleTypeConversionFromType.Text = "0x0A";
            this.textBoxTriangleTypeConversionFromType.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // labelTriangleTypeConversionFromType
            // 
            this.labelTriangleTypeConversionFromType.AutoSize = true;
            this.labelTriangleTypeConversionFromType.Location = new System.Drawing.Point(7, 47);
            this.labelTriangleTypeConversionFromType.Name = "labelTriangleTypeConversionFromType";
            this.labelTriangleTypeConversionFromType.Size = new System.Drawing.Size(60, 13);
            this.labelTriangleTypeConversionFromType.TabIndex = 14;
            this.labelTriangleTypeConversionFromType.Text = "From Type:";
            // 
            // labelTriangleTypeConversionConvert
            // 
            this.labelTriangleTypeConversionConvert.AutoSize = true;
            this.labelTriangleTypeConversionConvert.Location = new System.Drawing.Point(7, 21);
            this.labelTriangleTypeConversionConvert.Name = "labelTriangleTypeConversionConvert";
            this.labelTriangleTypeConversionConvert.Size = new System.Drawing.Size(47, 13);
            this.labelTriangleTypeConversionConvert.TabIndex = 14;
            this.labelTriangleTypeConversionConvert.Text = "Convert:";
            // 
            // comboBoxTriangleTypeConversionConvert
            // 
            this.comboBoxTriangleTypeConversionConvert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxTriangleTypeConversionConvert.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTriangleTypeConversionConvert.Location = new System.Drawing.Point(70, 18);
            this.comboBoxTriangleTypeConversionConvert.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxTriangleTypeConversionConvert.Name = "comboBoxTriangleTypeConversionConvert";
            this.comboBoxTriangleTypeConversionConvert.Size = new System.Drawing.Size(109, 21);
            this.comboBoxTriangleTypeConversionConvert.TabIndex = 13;
            // 
            // buttonTriangleTypeConversionConvert
            // 
            this.buttonTriangleTypeConversionConvert.Location = new System.Drawing.Point(6, 96);
            this.buttonTriangleTypeConversionConvert.Name = "buttonTriangleTypeConversionConvert";
            this.buttonTriangleTypeConversionConvert.Size = new System.Drawing.Size(174, 23);
            this.buttonTriangleTypeConversionConvert.TabIndex = 14;
            this.buttonTriangleTypeConversionConvert.Text = "Convert";
            this.buttonTriangleTypeConversionConvert.UseVisualStyleBackColor = true;
            // 
            // groupBoxTriangleNormal
            // 
            this.groupBoxTriangleNormal.Controls.Add(this.textBoxTriangleNormal);
            this.groupBoxTriangleNormal.Controls.Add(this.buttonTriangleNormalN);
            this.groupBoxTriangleNormal.Controls.Add(this.buttonTriangleNormalP);
            this.groupBoxTriangleNormal.Location = new System.Drawing.Point(2, 374);
            this.groupBoxTriangleNormal.Name = "groupBoxTriangleNormal";
            this.groupBoxTriangleNormal.Size = new System.Drawing.Size(185, 45);
            this.groupBoxTriangleNormal.TabIndex = 31;
            this.groupBoxTriangleNormal.TabStop = false;
            this.groupBoxTriangleNormal.Text = "Normal";
            // 
            // textBoxTriangleNormal
            // 
            this.textBoxTriangleNormal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxTriangleNormal.Location = new System.Drawing.Point(67, 19);
            this.textBoxTriangleNormal.Name = "textBoxTriangleNormal";
            this.textBoxTriangleNormal.Size = new System.Drawing.Size(51, 20);
            this.textBoxTriangleNormal.TabIndex = 33;
            this.textBoxTriangleNormal.Text = "50";
            this.textBoxTriangleNormal.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // buttonTriangleNormalN
            // 
            this.buttonTriangleNormalN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTriangleNormalN.Location = new System.Drawing.Point(3, 16);
            this.buttonTriangleNormalN.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTriangleNormalN.Name = "buttonTriangleNormalN";
            this.buttonTriangleNormalN.Size = new System.Drawing.Size(61, 25);
            this.buttonTriangleNormalN.TabIndex = 35;
            this.buttonTriangleNormalN.Text = "Normal-";
            this.buttonTriangleNormalN.UseVisualStyleBackColor = true;
            // 
            // buttonTriangleNormalP
            // 
            this.buttonTriangleNormalP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTriangleNormalP.Location = new System.Drawing.Point(121, 16);
            this.buttonTriangleNormalP.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTriangleNormalP.Name = "buttonTriangleNormalP";
            this.buttonTriangleNormalP.Size = new System.Drawing.Size(61, 25);
            this.buttonTriangleNormalP.TabIndex = 35;
            this.buttonTriangleNormalP.Text = "Normal+";
            this.buttonTriangleNormalP.UseVisualStyleBackColor = true;
            // 
            // radioButtonTriCustom
            // 
            this.radioButtonTriCustom.AutoSize = true;
            this.radioButtonTriCustom.Location = new System.Drawing.Point(12, 88);
            this.radioButtonTriCustom.Name = "radioButtonTriCustom";
            this.radioButtonTriCustom.Size = new System.Drawing.Size(63, 17);
            this.radioButtonTriCustom.TabIndex = 3;
            this.radioButtonTriCustom.Text = "Custom:";
            this.radioButtonTriCustom.UseVisualStyleBackColor = true;
            // 
            // buttonTriangleShowAddresses
            // 
            this.buttonTriangleShowAddresses.Location = new System.Drawing.Point(5, 526);
            this.buttonTriangleShowAddresses.Name = "buttonTriangleShowAddresses";
            this.buttonTriangleShowAddresses.Size = new System.Drawing.Size(87, 23);
            this.buttonTriangleShowAddresses.TabIndex = 15;
            this.buttonTriangleShowAddresses.Text = "Show Addrs";
            this.buttonTriangleShowAddresses.UseVisualStyleBackColor = true;
            // 
            // buttonTriangleClearData
            // 
            this.buttonTriangleClearData.Location = new System.Drawing.Point(98, 526);
            this.buttonTriangleClearData.Name = "buttonTriangleClearData";
            this.buttonTriangleClearData.Size = new System.Drawing.Size(87, 23);
            this.buttonTriangleClearData.TabIndex = 15;
            this.buttonTriangleClearData.Text = "Clear Data";
            this.buttonTriangleClearData.UseVisualStyleBackColor = true;
            // 
            // buttonTriangleDisableAllCamCollision
            // 
            this.buttonTriangleDisableAllCamCollision.Location = new System.Drawing.Point(98, 709);
            this.buttonTriangleDisableAllCamCollision.Name = "buttonTriangleDisableAllCamCollision";
            this.buttonTriangleDisableAllCamCollision.Size = new System.Drawing.Size(87, 43);
            this.buttonTriangleDisableAllCamCollision.TabIndex = 15;
            this.buttonTriangleDisableAllCamCollision.Text = "Disable All Cam Collision";
            this.buttonTriangleDisableAllCamCollision.UseVisualStyleBackColor = true;
            // 
            // buttonTriangleShowVertices
            // 
            this.buttonTriangleShowVertices.Location = new System.Drawing.Point(98, 500);
            this.buttonTriangleShowVertices.Name = "buttonTriangleShowVertices";
            this.buttonTriangleShowVertices.Size = new System.Drawing.Size(87, 23);
            this.buttonTriangleShowVertices.TabIndex = 15;
            this.buttonTriangleShowVertices.Text = "Show Vertices";
            this.buttonTriangleShowVertices.UseVisualStyleBackColor = true;
            // 
            // buttonAnnihilateTriangle
            // 
            this.buttonAnnihilateTriangle.Location = new System.Drawing.Point(98, 170);
            this.buttonAnnihilateTriangle.Name = "buttonAnnihilateTriangle";
            this.buttonAnnihilateTriangle.Size = new System.Drawing.Size(87, 23);
            this.buttonAnnihilateTriangle.TabIndex = 15;
            this.buttonAnnihilateTriangle.Text = "Annihilate";
            this.buttonAnnihilateTriangle.UseVisualStyleBackColor = true;
            // 
            // radioButtonTriFloor
            // 
            this.radioButtonTriFloor.AutoSize = true;
            this.radioButtonTriFloor.Checked = true;
            this.radioButtonTriFloor.Location = new System.Drawing.Point(12, 19);
            this.radioButtonTriFloor.Name = "radioButtonTriFloor";
            this.radioButtonTriFloor.Size = new System.Drawing.Size(48, 17);
            this.radioButtonTriFloor.TabIndex = 0;
            this.radioButtonTriFloor.TabStop = true;
            this.radioButtonTriFloor.Text = "Floor";
            this.radioButtonTriFloor.UseVisualStyleBackColor = true;
            // 
            // buttonTriangleShowAllTris
            // 
            this.buttonTriangleShowAllTris.Location = new System.Drawing.Point(128, 757);
            this.buttonTriangleShowAllTris.Name = "buttonTriangleShowAllTris";
            this.buttonTriangleShowAllTris.Size = new System.Drawing.Size(57, 43);
            this.buttonTriangleShowAllTris.TabIndex = 14;
            this.buttonTriangleShowAllTris.Text = "Show\r\nAll Tris";
            this.buttonTriangleShowAllTris.UseVisualStyleBackColor = true;
            // 
            // buttonTriangleShowObjTris
            // 
            this.buttonTriangleShowObjTris.Location = new System.Drawing.Point(68, 757);
            this.buttonTriangleShowObjTris.Name = "buttonTriangleShowObjTris";
            this.buttonTriangleShowObjTris.Size = new System.Drawing.Size(58, 43);
            this.buttonTriangleShowObjTris.TabIndex = 14;
            this.buttonTriangleShowObjTris.Text = "Show\r\nObj Tris";
            this.buttonTriangleShowObjTris.UseVisualStyleBackColor = true;
            // 
            // buttonTriangleNeutralizeAllTriangles
            // 
            this.buttonTriangleNeutralizeAllTriangles.Location = new System.Drawing.Point(5, 709);
            this.buttonTriangleNeutralizeAllTriangles.Name = "buttonTriangleNeutralizeAllTriangles";
            this.buttonTriangleNeutralizeAllTriangles.Size = new System.Drawing.Size(87, 43);
            this.buttonTriangleNeutralizeAllTriangles.TabIndex = 14;
            this.buttonTriangleNeutralizeAllTriangles.Text = "Neutralize All Triangles";
            this.buttonTriangleNeutralizeAllTriangles.UseVisualStyleBackColor = true;
            // 
            // buttonTriangleShowLevelTris
            // 
            this.buttonTriangleShowLevelTris.Location = new System.Drawing.Point(5, 757);
            this.buttonTriangleShowLevelTris.Name = "buttonTriangleShowLevelTris";
            this.buttonTriangleShowLevelTris.Size = new System.Drawing.Size(61, 43);
            this.buttonTriangleShowLevelTris.TabIndex = 14;
            this.buttonTriangleShowLevelTris.Text = "Show\r\nLevel Tris";
            this.buttonTriangleShowLevelTris.UseVisualStyleBackColor = true;
            // 
            // buttonTriangleShowData
            // 
            this.buttonTriangleShowData.Location = new System.Drawing.Point(5, 500);
            this.buttonTriangleShowData.Name = "buttonTriangleShowData";
            this.buttonTriangleShowData.Size = new System.Drawing.Size(87, 23);
            this.buttonTriangleShowData.TabIndex = 14;
            this.buttonTriangleShowData.Text = "Show Data";
            this.buttonTriangleShowData.UseVisualStyleBackColor = true;
            // 
            // buttonNeutralizeTriangle
            // 
            this.buttonNeutralizeTriangle.Location = new System.Drawing.Point(5, 170);
            this.buttonNeutralizeTriangle.Name = "buttonNeutralizeTriangle";
            this.buttonNeutralizeTriangle.Size = new System.Drawing.Size(87, 23);
            this.buttonNeutralizeTriangle.TabIndex = 14;
            this.buttonNeutralizeTriangle.Text = "Neutralize";
            this.buttonNeutralizeTriangle.UseVisualStyleBackColor = true;
            // 
            // radioButtonTriWall
            // 
            this.radioButtonTriWall.AutoSize = true;
            this.radioButtonTriWall.Location = new System.Drawing.Point(12, 42);
            this.radioButtonTriWall.Name = "radioButtonTriWall";
            this.radioButtonTriWall.Size = new System.Drawing.Size(46, 17);
            this.radioButtonTriWall.TabIndex = 1;
            this.radioButtonTriWall.Text = "Wall";
            this.radioButtonTriWall.UseVisualStyleBackColor = true;
            // 
            // buttonTriangleShowCoords
            // 
            this.buttonTriangleShowCoords.Location = new System.Drawing.Point(5, 448);
            this.buttonTriangleShowCoords.Name = "buttonTriangleShowCoords";
            this.buttonTriangleShowCoords.Size = new System.Drawing.Size(87, 23);
            this.buttonTriangleShowCoords.TabIndex = 13;
            this.buttonTriangleShowCoords.Text = "Show Coords";
            this.buttonTriangleShowCoords.UseVisualStyleBackColor = true;
            // 
            // buttonGotoVClosest
            // 
            this.buttonGotoVClosest.Location = new System.Drawing.Point(5, 141);
            this.buttonGotoVClosest.Name = "buttonGotoVClosest";
            this.buttonGotoVClosest.Size = new System.Drawing.Size(87, 23);
            this.buttonGotoVClosest.TabIndex = 13;
            this.buttonGotoVClosest.Text = "Goto Closest";
            this.buttonGotoVClosest.UseVisualStyleBackColor = true;
            // 
            // radioButtonTriCeiling
            // 
            this.radioButtonTriCeiling.AutoSize = true;
            this.radioButtonTriCeiling.Location = new System.Drawing.Point(12, 65);
            this.radioButtonTriCeiling.Name = "radioButtonTriCeiling";
            this.radioButtonTriCeiling.Size = new System.Drawing.Size(56, 17);
            this.radioButtonTriCeiling.TabIndex = 2;
            this.radioButtonTriCeiling.Text = "Ceiling";
            this.radioButtonTriCeiling.UseVisualStyleBackColor = true;
            // 
            // checkBoxRepeatFirstVertex
            // 
            this.checkBoxRepeatFirstVertex.AutoSize = true;
            this.checkBoxRepeatFirstVertex.Checked = true;
            this.checkBoxRepeatFirstVertex.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxRepeatFirstVertex.Location = new System.Drawing.Point(6, 552);
            this.checkBoxRepeatFirstVertex.Name = "checkBoxRepeatFirstVertex";
            this.checkBoxRepeatFirstVertex.Size = new System.Drawing.Size(116, 17);
            this.checkBoxRepeatFirstVertex.TabIndex = 12;
            this.checkBoxRepeatFirstVertex.Text = "Repeat First Vertex";
            this.checkBoxRepeatFirstVertex.UseVisualStyleBackColor = true;
            // 
            // checkBoxNeutralizeTriangle
            // 
            this.checkBoxNeutralizeTriangle.AutoSize = true;
            this.checkBoxNeutralizeTriangle.Location = new System.Drawing.Point(6, 425);
            this.checkBoxNeutralizeTriangle.Name = "checkBoxNeutralizeTriangle";
            this.checkBoxNeutralizeTriangle.Size = new System.Drawing.Size(114, 17);
            this.checkBoxNeutralizeTriangle.TabIndex = 12;
            this.checkBoxNeutralizeTriangle.Text = "Neutralize Triangle";
            this.checkBoxNeutralizeTriangle.UseVisualStyleBackColor = true;
            // 
            // checkBoxRecordTriangleData
            // 
            this.checkBoxRecordTriangleData.AutoSize = true;
            this.checkBoxRecordTriangleData.Location = new System.Drawing.Point(6, 482);
            this.checkBoxRecordTriangleData.Name = "checkBoxRecordTriangleData";
            this.checkBoxRecordTriangleData.Size = new System.Drawing.Size(128, 17);
            this.checkBoxRecordTriangleData.TabIndex = 12;
            this.checkBoxRecordTriangleData.Text = "Record Triangle Data";
            this.checkBoxRecordTriangleData.UseVisualStyleBackColor = true;
            // 
            // checkBoxVertexMisalignment
            // 
            this.checkBoxVertexMisalignment.AutoSize = true;
            this.checkBoxVertexMisalignment.Location = new System.Drawing.Point(6, 199);
            this.checkBoxVertexMisalignment.Name = "checkBoxVertexMisalignment";
            this.checkBoxVertexMisalignment.Size = new System.Drawing.Size(151, 17);
            this.checkBoxVertexMisalignment.TabIndex = 12;
            this.checkBoxVertexMisalignment.Text = "Vertex Misalignment Offset";
            this.checkBoxVertexMisalignment.UseVisualStyleBackColor = true;
            // 
            // textBoxCustomTriangle
            // 
            this.textBoxCustomTriangle.Location = new System.Drawing.Point(77, 87);
            this.textBoxCustomTriangle.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxCustomTriangle.Name = "textBoxCustomTriangle";
            this.textBoxCustomTriangle.Size = new System.Drawing.Size(87, 20);
            this.textBoxCustomTriangle.TabIndex = 5;
            // 
            // buttonTriangleShowEquation
            // 
            this.buttonTriangleShowEquation.Location = new System.Drawing.Point(98, 448);
            this.buttonTriangleShowEquation.Name = "buttonTriangleShowEquation";
            this.buttonTriangleShowEquation.Size = new System.Drawing.Size(87, 23);
            this.buttonTriangleShowEquation.TabIndex = 11;
            this.buttonTriangleShowEquation.Text = "Show Equation";
            this.buttonTriangleShowEquation.UseVisualStyleBackColor = true;
            // 
            // buttonRetrieveTriangle
            // 
            this.buttonRetrieveTriangle.Location = new System.Drawing.Point(98, 141);
            this.buttonRetrieveTriangle.Name = "buttonRetrieveTriangle";
            this.buttonRetrieveTriangle.Size = new System.Drawing.Size(87, 23);
            this.buttonRetrieveTriangle.TabIndex = 11;
            this.buttonRetrieveTriangle.Text = "Retrieve";
            this.buttonRetrieveTriangle.UseVisualStyleBackColor = true;
            // 
            // labelRecordTriangleCount
            // 
            this.labelRecordTriangleCount.AutoSize = true;
            this.labelRecordTriangleCount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelRecordTriangleCount.Location = new System.Drawing.Point(144, 482);
            this.labelRecordTriangleCount.MinimumSize = new System.Drawing.Size(40, 2);
            this.labelRecordTriangleCount.Name = "labelRecordTriangleCount";
            this.labelRecordTriangleCount.Size = new System.Drawing.Size(40, 15);
            this.labelRecordTriangleCount.TabIndex = 6;
            this.labelRecordTriangleCount.Text = "34";
            this.labelRecordTriangleCount.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // labelTriangleSelection
            // 
            this.labelTriangleSelection.AutoSize = true;
            this.labelTriangleSelection.Location = new System.Drawing.Point(1, 3);
            this.labelTriangleSelection.Name = "labelTriangleSelection";
            this.labelTriangleSelection.Size = new System.Drawing.Size(48, 13);
            this.labelTriangleSelection.TabIndex = 6;
            this.labelTriangleSelection.Text = "Triangle:";
            // 
            // buttonGotoV3
            // 
            this.buttonGotoV3.Location = new System.Drawing.Point(129, 112);
            this.buttonGotoV3.Name = "buttonGotoV3";
            this.buttonGotoV3.Size = new System.Drawing.Size(56, 23);
            this.buttonGotoV3.TabIndex = 10;
            this.buttonGotoV3.Text = "Goto V3";
            this.buttonGotoV3.UseVisualStyleBackColor = true;
            // 
            // buttonGotoV1
            // 
            this.buttonGotoV1.Location = new System.Drawing.Point(5, 112);
            this.buttonGotoV1.Name = "buttonGotoV1";
            this.buttonGotoV1.Size = new System.Drawing.Size(57, 23);
            this.buttonGotoV1.TabIndex = 8;
            this.buttonGotoV1.Text = "Goto V1";
            this.buttonGotoV1.UseVisualStyleBackColor = true;
            // 
            // buttonGotoV2
            // 
            this.buttonGotoV2.Location = new System.Drawing.Point(68, 112);
            this.buttonGotoV2.Name = "buttonGotoV2";
            this.buttonGotoV2.Size = new System.Drawing.Size(55, 23);
            this.buttonGotoV2.TabIndex = 9;
            this.buttonGotoV2.Text = "Goto V2";
            this.buttonGotoV2.UseVisualStyleBackColor = true;
            // 
            // watchVariablePanelTriangles
            // 
            this._variablePanelTriangles.DataPath = "Config/TrianglesData.xml";
            this._variablePanelTriangles.Dock = System.Windows.Forms.DockStyle.Fill;
            this._variablePanelTriangles.Location = new System.Drawing.Point(2, 2);
            this._variablePanelTriangles.Margin = new System.Windows.Forms.Padding(0);
            this._variablePanelTriangles.Name = "_variablePanelTriangles";
            this._variablePanelTriangles.Size = new System.Drawing.Size(700, 457);
            this._variablePanelTriangles.TabIndex = 7;
            // 
            // TrianglesTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainerTriangles);
            this.Name = "TrianglesTab";
            this.Size = new System.Drawing.Size(915, 463);
            this.splitContainerTriangles.Panel1.ResumeLayout(false);
            this.splitContainerTriangles.Panel1.PerformLayout();
            this.splitContainerTriangles.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerTriangles)).EndInit();
            this.splitContainerTriangles.ResumeLayout(false);
            this.groupBoxTrianglePos.ResumeLayout(false);
            this.groupBoxTrianglePos.PerformLayout();
            this.groupBoxTriangleTypeConversion.ResumeLayout(false);
            this.groupBoxTriangleTypeConversion.PerformLayout();
            this.groupBoxTriangleNormal.ResumeLayout(false);
            this.groupBoxTriangleNormal.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private BetterSplitContainer splitContainerTriangles;
        private System.Windows.Forms.GroupBox groupBoxTrianglePos;
        private System.Windows.Forms.CheckBox checkBoxTrianglePosRelative;
        private BetterTextbox textBoxTrianglePosY;
        private System.Windows.Forms.Button buttonTrianglePosYp;
        private System.Windows.Forms.Button buttonTrianglePosYn;
        private System.Windows.Forms.Button buttonTrianglePosXpZp;
        private BetterTextbox textBoxTrianglePosXZ;
        private System.Windows.Forms.Button buttonTrianglePosXp;
        private System.Windows.Forms.Button buttonTrianglePosXpZn;
        private System.Windows.Forms.Button buttonTrianglePosZn;
        private System.Windows.Forms.Button buttonTrianglePosZp;
        private System.Windows.Forms.Button buttonTrianglePosXnZp;
        private System.Windows.Forms.Button buttonTrianglePosXn;
        private System.Windows.Forms.Button buttonTrianglePosXnZn;
        private System.Windows.Forms.GroupBox groupBoxTriangleTypeConversion;
        private BetterTextbox textBoxTriangleTypeConversionToType;
        private System.Windows.Forms.Label labelTriangleTypeConversionToType;
        private BetterTextbox textBoxTriangleTypeConversionFromType;
        private System.Windows.Forms.Label labelTriangleTypeConversionFromType;
        private System.Windows.Forms.Label labelTriangleTypeConversionConvert;
        private System.Windows.Forms.ComboBox comboBoxTriangleTypeConversionConvert;
        private System.Windows.Forms.Button buttonTriangleTypeConversionConvert;
        private System.Windows.Forms.GroupBox groupBoxTriangleNormal;
        private BetterTextbox textBoxTriangleNormal;
        private System.Windows.Forms.Button buttonTriangleNormalN;
        private System.Windows.Forms.Button buttonTriangleNormalP;
        private System.Windows.Forms.RadioButton radioButtonTriCustom;
        private System.Windows.Forms.Button buttonTriangleShowAddresses;
        private System.Windows.Forms.Button buttonTriangleClearData;
        private System.Windows.Forms.Button buttonTriangleDisableAllCamCollision;
        private System.Windows.Forms.Button buttonTriangleShowVertices;
        private System.Windows.Forms.Button buttonAnnihilateTriangle;
        private System.Windows.Forms.RadioButton radioButtonTriFloor;
        private System.Windows.Forms.Button buttonTriangleShowAllTris;
        private System.Windows.Forms.Button buttonTriangleShowObjTris;
        private System.Windows.Forms.Button buttonTriangleNeutralizeAllTriangles;
        private System.Windows.Forms.Button buttonTriangleShowLevelTris;
        private System.Windows.Forms.Button buttonTriangleShowData;
        private System.Windows.Forms.Button buttonNeutralizeTriangle;
        private System.Windows.Forms.RadioButton radioButtonTriWall;
        private System.Windows.Forms.Button buttonTriangleShowCoords;
        private System.Windows.Forms.Button buttonGotoVClosest;
        private System.Windows.Forms.RadioButton radioButtonTriCeiling;
        private System.Windows.Forms.CheckBox checkBoxRepeatFirstVertex;
        private System.Windows.Forms.CheckBox checkBoxNeutralizeTriangle;
        private System.Windows.Forms.CheckBox checkBoxRecordTriangleData;
        private System.Windows.Forms.CheckBox checkBoxVertexMisalignment;
        private BetterTextbox textBoxCustomTriangle;
        private System.Windows.Forms.Button buttonTriangleShowEquation;
        private System.Windows.Forms.Button buttonRetrieveTriangle;
        private System.Windows.Forms.Label labelRecordTriangleCount;
        private System.Windows.Forms.Label labelTriangleSelection;
        private System.Windows.Forms.Button buttonGotoV3;
        private System.Windows.Forms.Button buttonGotoV1;
        private System.Windows.Forms.Button buttonGotoV2;
        private STROOP.Controls.VariablePanel.VariablePanel _variablePanelTriangles;
    }
}
