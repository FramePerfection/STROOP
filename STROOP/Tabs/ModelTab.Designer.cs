using OpenTK.GLControl;

namespace STROOP.Tabs
{
    partial class ModelTab
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
            this.splitContainerModel = new STROOP.BetterSplitContainer();
            this.checkBoxModelLevel = new System.Windows.Forms.CheckBox();
            this.groupBoxTransformation = new System.Windows.Forms.GroupBox();
            this.labelTransScaleValue = new System.Windows.Forms.Label();
            this.labelTransAngleValue = new System.Windows.Forms.Label();
            this.labelTransPosValue = new System.Windows.Forms.Label();
            this.checkBoxTransUseObj = new System.Windows.Forms.CheckBox();
            this.groupBoxTransScale = new System.Windows.Forms.GroupBox();
            this.checkBoxTransScaleMultiply = new System.Windows.Forms.CheckBox();
            this.checkBoxTransScaleAggregate = new System.Windows.Forms.CheckBox();
            this.textbpxTransScaleDValue = new STROOP.BetterTextbox();
            this.textbpxTransScaleHValue = new STROOP.BetterTextbox();
            this.textbpxTransScaleWValue = new STROOP.BetterTextbox();
            this.buttonTransScaleDn = new System.Windows.Forms.Button();
            this.buttonTransScaleHn = new System.Windows.Forms.Button();
            this.buttonTransScaleWn = new System.Windows.Forms.Button();
            this.buttonTransScaleDp = new System.Windows.Forms.Button();
            this.buttonTransScaleHp = new System.Windows.Forms.Button();
            this.buttonTransScaleWp = new System.Windows.Forms.Button();
            this.betterTextbox4 = new STROOP.BetterTextbox();
            this.button8 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.groupBoxTransAngle = new System.Windows.Forms.GroupBox();
            this.textboxTransAngleRValue = new STROOP.BetterTextbox();
            this.textboxTransAnglePValue = new STROOP.BetterTextbox();
            this.textboxTransAngleYValue = new STROOP.BetterTextbox();
            this.buttonTransAngleRn = new System.Windows.Forms.Button();
            this.buttonTransAnglePn = new System.Windows.Forms.Button();
            this.buttonTransAngleYn = new System.Windows.Forms.Button();
            this.buttonTransAngleRp = new System.Windows.Forms.Button();
            this.buttonTransAnglePp = new System.Windows.Forms.Button();
            this.buttonTransAngleYp = new System.Windows.Forms.Button();
            this.groupBoxTransPos = new System.Windows.Forms.GroupBox();
            this.checkBoxTransPosRel = new System.Windows.Forms.CheckBox();
            this.textboxTransPosYValue = new STROOP.BetterTextbox();
            this.buttonTransPosYp = new System.Windows.Forms.Button();
            this.buttonTransPosYn = new System.Windows.Forms.Button();
            this.buttonTransPosXpZp = new System.Windows.Forms.Button();
            this.textboxTransPosXZValue = new STROOP.BetterTextbox();
            this.buttonTransPosXp = new System.Windows.Forms.Button();
            this.buttonTransPosXpZn = new System.Windows.Forms.Button();
            this.buttonTransPosZn = new System.Windows.Forms.Button();
            this.buttonTransPosZp = new System.Windows.Forms.Button();
            this.buttonTransPosXnZp = new System.Windows.Forms.Button();
            this.buttonTransPosXn = new System.Windows.Forms.Button();
            this.buttonTransPosXnZn = new System.Windows.Forms.Button();
            this.buttonTransReset = new System.Windows.Forms.Button();
            this.splitContainerModelTables = new STROOP.BetterSplitContainer();
            this.labelModelVertices = new System.Windows.Forms.Label();
            this.dataGridViewVertices = new System.Windows.Forms.DataGridView();
            this.Index = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.X = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Y = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Z = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.labelModelTriangles = new System.Windows.Forms.Label();
            this.dataGridViewTriangles = new System.Windows.Forms.DataGridView();
            this.Group = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxModelAddress = new System.Windows.Forms.TextBox();
            this.glControlModelView = new GLControl();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerModel)).BeginInit();
            this.splitContainerModel.Panel1.SuspendLayout();
            this.splitContainerModel.Panel2.SuspendLayout();
            this.splitContainerModel.SuspendLayout();
            this.groupBoxTransformation.SuspendLayout();
            this.groupBoxTransScale.SuspendLayout();
            this.groupBoxTransAngle.SuspendLayout();
            this.groupBoxTransPos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerModelTables)).BeginInit();
            this.splitContainerModelTables.Panel1.SuspendLayout();
            this.splitContainerModelTables.Panel2.SuspendLayout();
            this.splitContainerModelTables.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewVertices)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTriangles)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainerModel
            // 
            this.splitContainerModel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerModel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerModel.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerModel.Location = new System.Drawing.Point(0, 0);
            this.splitContainerModel.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainerModel.Name = "splitContainerModel";
            // 
            // splitContainerModel.Panel1
            // 
            this.splitContainerModel.Panel1.AutoScroll = true;
            this.splitContainerModel.Panel1.Controls.Add(this.checkBoxModelLevel);
            this.splitContainerModel.Panel1.Controls.Add(this.groupBoxTransformation);
            this.splitContainerModel.Panel1.Controls.Add(this.splitContainerModelTables);
            this.splitContainerModel.Panel1.Controls.Add(this.label1);
            this.splitContainerModel.Panel1.Controls.Add(this.textBoxModelAddress);
            this.splitContainerModel.Panel1MinSize = 0;
            // 
            // splitContainerModel.Panel2
            // 
            this.splitContainerModel.Panel2.Controls.Add(this.glControlModelView);
            this.splitContainerModel.Panel2MinSize = 0;
            this.splitContainerModel.Size = new System.Drawing.Size(915, 463);
            this.splitContainerModel.SplitterDistance = 416;
            this.splitContainerModel.SplitterWidth = 1;
            this.splitContainerModel.TabIndex = 33;
            // 
            // checkBoxModelLevel
            // 
            this.checkBoxModelLevel.AutoCheck = false;
            this.checkBoxModelLevel.AutoSize = true;
            this.checkBoxModelLevel.Location = new System.Drawing.Point(193, 5);
            this.checkBoxModelLevel.Name = "checkBoxModelLevel";
            this.checkBoxModelLevel.Size = new System.Drawing.Size(78, 17);
            this.checkBoxModelLevel.TabIndex = 12;
            this.checkBoxModelLevel.Text = "View Level";
            this.checkBoxModelLevel.UseVisualStyleBackColor = true;
            // 
            // groupBoxTransformation
            // 
            this.groupBoxTransformation.Controls.Add(this.labelTransScaleValue);
            this.groupBoxTransformation.Controls.Add(this.labelTransAngleValue);
            this.groupBoxTransformation.Controls.Add(this.labelTransPosValue);
            this.groupBoxTransformation.Controls.Add(this.checkBoxTransUseObj);
            this.groupBoxTransformation.Controls.Add(this.groupBoxTransScale);
            this.groupBoxTransformation.Controls.Add(this.groupBoxTransAngle);
            this.groupBoxTransformation.Controls.Add(this.groupBoxTransPos);
            this.groupBoxTransformation.Controls.Add(this.buttonTransReset);
            this.groupBoxTransformation.Location = new System.Drawing.Point(3, 447);
            this.groupBoxTransformation.Name = "groupBoxTransformation";
            this.groupBoxTransformation.Size = new System.Drawing.Size(390, 282);
            this.groupBoxTransformation.TabIndex = 11;
            this.groupBoxTransformation.TabStop = false;
            this.groupBoxTransformation.Text = "Transformation";
            this.groupBoxTransformation.Visible = false;
            // 
            // labelTransScaleValue
            // 
            this.labelTransScaleValue.AutoSize = true;
            this.labelTransScaleValue.Location = new System.Drawing.Point(6, 71);
            this.labelTransScaleValue.Name = "labelTransScaleValue";
            this.labelTransScaleValue.Size = new System.Drawing.Size(73, 13);
            this.labelTransScaleValue.TabIndex = 36;
            this.labelTransScaleValue.Text = "Scale: (x, y, z)";
            // 
            // labelTransAngleValue
            // 
            this.labelTransAngleValue.AutoSize = true;
            this.labelTransAngleValue.Location = new System.Drawing.Point(6, 58);
            this.labelTransAngleValue.Name = "labelTransAngleValue";
            this.labelTransAngleValue.Size = new System.Drawing.Size(72, 13);
            this.labelTransAngleValue.TabIndex = 35;
            this.labelTransAngleValue.Text = "Angle: (y, p, r)";
            // 
            // labelTransPosValue
            // 
            this.labelTransPosValue.AutoSize = true;
            this.labelTransPosValue.Location = new System.Drawing.Point(6, 45);
            this.labelTransPosValue.Name = "labelTransPosValue";
            this.labelTransPosValue.Size = new System.Drawing.Size(83, 13);
            this.labelTransPosValue.TabIndex = 34;
            this.labelTransPosValue.Text = "Position: (x, y, z)";
            // 
            // checkBoxTransUseObj
            // 
            this.checkBoxTransUseObj.AutoSize = true;
            this.checkBoxTransUseObj.Checked = true;
            this.checkBoxTransUseObj.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxTransUseObj.Location = new System.Drawing.Point(87, 23);
            this.checkBoxTransUseObj.Name = "checkBoxTransUseObj";
            this.checkBoxTransUseObj.Size = new System.Drawing.Size(120, 17);
            this.checkBoxTransUseObj.TabIndex = 33;
            this.checkBoxTransUseObj.Text = "Use current object\'s";
            this.checkBoxTransUseObj.UseVisualStyleBackColor = true;
            // 
            // groupBoxTransScale
            // 
            this.groupBoxTransScale.Controls.Add(this.checkBoxTransScaleMultiply);
            this.groupBoxTransScale.Controls.Add(this.checkBoxTransScaleAggregate);
            this.groupBoxTransScale.Controls.Add(this.textbpxTransScaleDValue);
            this.groupBoxTransScale.Controls.Add(this.textbpxTransScaleHValue);
            this.groupBoxTransScale.Controls.Add(this.textbpxTransScaleWValue);
            this.groupBoxTransScale.Controls.Add(this.buttonTransScaleDn);
            this.groupBoxTransScale.Controls.Add(this.buttonTransScaleHn);
            this.groupBoxTransScale.Controls.Add(this.buttonTransScaleWn);
            this.groupBoxTransScale.Controls.Add(this.buttonTransScaleDp);
            this.groupBoxTransScale.Controls.Add(this.buttonTransScaleHp);
            this.groupBoxTransScale.Controls.Add(this.buttonTransScaleWp);
            this.groupBoxTransScale.Controls.Add(this.betterTextbox4);
            this.groupBoxTransScale.Controls.Add(this.button8);
            this.groupBoxTransScale.Controls.Add(this.button9);
            this.groupBoxTransScale.Location = new System.Drawing.Point(200, 182);
            this.groupBoxTransScale.Name = "groupBoxTransScale";
            this.groupBoxTransScale.Size = new System.Drawing.Size(185, 95);
            this.groupBoxTransScale.TabIndex = 32;
            this.groupBoxTransScale.TabStop = false;
            this.groupBoxTransScale.Text = "Scale";
            // 
            // checkBoxTransScaleMultiply
            // 
            this.checkBoxTransScaleMultiply.AutoSize = true;
            this.checkBoxTransScaleMultiply.Location = new System.Drawing.Point(124, 0);
            this.checkBoxTransScaleMultiply.Name = "checkBoxTransScaleMultiply";
            this.checkBoxTransScaleMultiply.Size = new System.Drawing.Size(61, 17);
            this.checkBoxTransScaleMultiply.TabIndex = 38;
            this.checkBoxTransScaleMultiply.Text = "Multiply";
            this.checkBoxTransScaleMultiply.UseVisualStyleBackColor = true;
            // 
            // checkBoxTransScaleAggregate
            // 
            this.checkBoxTransScaleAggregate.AutoSize = true;
            this.checkBoxTransScaleAggregate.Location = new System.Drawing.Point(50, 0);
            this.checkBoxTransScaleAggregate.Name = "checkBoxTransScaleAggregate";
            this.checkBoxTransScaleAggregate.Size = new System.Drawing.Size(75, 17);
            this.checkBoxTransScaleAggregate.TabIndex = 39;
            this.checkBoxTransScaleAggregate.Text = "Aggregate";
            this.checkBoxTransScaleAggregate.UseVisualStyleBackColor = true;
            // 
            // textbpxTransScaleDValue
            // 
            this.textbpxTransScaleDValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textbpxTransScaleDValue.Location = new System.Drawing.Point(67, 69);
            this.textbpxTransScaleDValue.Name = "textbpxTransScaleDValue";
            this.textbpxTransScaleDValue.Size = new System.Drawing.Size(51, 20);
            this.textbpxTransScaleDValue.TabIndex = 33;
            this.textbpxTransScaleDValue.Text = "1";
            this.textbpxTransScaleDValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textbpxTransScaleHValue
            // 
            this.textbpxTransScaleHValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textbpxTransScaleHValue.Location = new System.Drawing.Point(67, 44);
            this.textbpxTransScaleHValue.Name = "textbpxTransScaleHValue";
            this.textbpxTransScaleHValue.Size = new System.Drawing.Size(51, 20);
            this.textbpxTransScaleHValue.TabIndex = 33;
            this.textbpxTransScaleHValue.Text = "1";
            this.textbpxTransScaleHValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textbpxTransScaleWValue
            // 
            this.textbpxTransScaleWValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textbpxTransScaleWValue.Location = new System.Drawing.Point(67, 19);
            this.textbpxTransScaleWValue.Name = "textbpxTransScaleWValue";
            this.textbpxTransScaleWValue.Size = new System.Drawing.Size(51, 20);
            this.textbpxTransScaleWValue.TabIndex = 33;
            this.textbpxTransScaleWValue.Text = "1";
            this.textbpxTransScaleWValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // buttonTransScaleDn
            // 
            this.buttonTransScaleDn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTransScaleDn.Location = new System.Drawing.Point(3, 66);
            this.buttonTransScaleDn.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTransScaleDn.Name = "buttonTransScaleDn";
            this.buttonTransScaleDn.Size = new System.Drawing.Size(61, 25);
            this.buttonTransScaleDn.TabIndex = 35;
            this.buttonTransScaleDn.Text = "Depth-";
            this.buttonTransScaleDn.UseVisualStyleBackColor = true;
            // 
            // buttonTransScaleHn
            // 
            this.buttonTransScaleHn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTransScaleHn.Location = new System.Drawing.Point(3, 41);
            this.buttonTransScaleHn.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTransScaleHn.Name = "buttonTransScaleHn";
            this.buttonTransScaleHn.Size = new System.Drawing.Size(61, 25);
            this.buttonTransScaleHn.TabIndex = 35;
            this.buttonTransScaleHn.Text = "Height-";
            this.buttonTransScaleHn.UseVisualStyleBackColor = true;
            // 
            // buttonTransScaleWn
            // 
            this.buttonTransScaleWn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTransScaleWn.Location = new System.Drawing.Point(3, 16);
            this.buttonTransScaleWn.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTransScaleWn.Name = "buttonTransScaleWn";
            this.buttonTransScaleWn.Size = new System.Drawing.Size(61, 25);
            this.buttonTransScaleWn.TabIndex = 35;
            this.buttonTransScaleWn.Text = "Width-";
            this.buttonTransScaleWn.UseVisualStyleBackColor = true;
            // 
            // buttonTransScaleDp
            // 
            this.buttonTransScaleDp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTransScaleDp.Location = new System.Drawing.Point(121, 66);
            this.buttonTransScaleDp.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTransScaleDp.Name = "buttonTransScaleDp";
            this.buttonTransScaleDp.Size = new System.Drawing.Size(61, 25);
            this.buttonTransScaleDp.TabIndex = 35;
            this.buttonTransScaleDp.Text = "Depth+";
            this.buttonTransScaleDp.UseVisualStyleBackColor = true;
            // 
            // buttonTransScaleHp
            // 
            this.buttonTransScaleHp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTransScaleHp.Location = new System.Drawing.Point(121, 41);
            this.buttonTransScaleHp.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTransScaleHp.Name = "buttonTransScaleHp";
            this.buttonTransScaleHp.Size = new System.Drawing.Size(61, 25);
            this.buttonTransScaleHp.TabIndex = 35;
            this.buttonTransScaleHp.Text = "Height+";
            this.buttonTransScaleHp.UseVisualStyleBackColor = true;
            // 
            // buttonTransScaleWp
            // 
            this.buttonTransScaleWp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTransScaleWp.Location = new System.Drawing.Point(121, 16);
            this.buttonTransScaleWp.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTransScaleWp.Name = "buttonTransScaleWp";
            this.buttonTransScaleWp.Size = new System.Drawing.Size(61, 25);
            this.buttonTransScaleWp.TabIndex = 35;
            this.buttonTransScaleWp.Text = "Width+";
            this.buttonTransScaleWp.UseVisualStyleBackColor = true;
            // 
            // betterTextbox4
            // 
            this.betterTextbox4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.betterTextbox4.Location = new System.Drawing.Point(67, 44);
            this.betterTextbox4.Name = "betterTextbox4";
            this.betterTextbox4.Size = new System.Drawing.Size(51, 20);
            this.betterTextbox4.TabIndex = 40;
            this.betterTextbox4.Text = "1";
            this.betterTextbox4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.betterTextbox4.Visible = false;
            // 
            // button8
            // 
            this.button8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button8.Location = new System.Drawing.Point(3, 16);
            this.button8.Margin = new System.Windows.Forms.Padding(0);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(61, 75);
            this.button8.TabIndex = 41;
            this.button8.Text = "Scale-";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Visible = false;
            // 
            // button9
            // 
            this.button9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button9.Location = new System.Drawing.Point(121, 16);
            this.button9.Margin = new System.Windows.Forms.Padding(0);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(61, 75);
            this.button9.TabIndex = 42;
            this.button9.Text = "Scale+";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Visible = false;
            // 
            // groupBoxTransAngle
            // 
            this.groupBoxTransAngle.Controls.Add(this.textboxTransAngleRValue);
            this.groupBoxTransAngle.Controls.Add(this.textboxTransAnglePValue);
            this.groupBoxTransAngle.Controls.Add(this.textboxTransAngleYValue);
            this.groupBoxTransAngle.Controls.Add(this.buttonTransAngleRn);
            this.groupBoxTransAngle.Controls.Add(this.buttonTransAnglePn);
            this.groupBoxTransAngle.Controls.Add(this.buttonTransAngleYn);
            this.groupBoxTransAngle.Controls.Add(this.buttonTransAngleRp);
            this.groupBoxTransAngle.Controls.Add(this.buttonTransAnglePp);
            this.groupBoxTransAngle.Controls.Add(this.buttonTransAngleYp);
            this.groupBoxTransAngle.Location = new System.Drawing.Point(200, 87);
            this.groupBoxTransAngle.Name = "groupBoxTransAngle";
            this.groupBoxTransAngle.Size = new System.Drawing.Size(185, 95);
            this.groupBoxTransAngle.TabIndex = 31;
            this.groupBoxTransAngle.TabStop = false;
            this.groupBoxTransAngle.Text = "Angle";
            // 
            // textboxTransAngleRValue
            // 
            this.textboxTransAngleRValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textboxTransAngleRValue.Location = new System.Drawing.Point(67, 69);
            this.textboxTransAngleRValue.Name = "textboxTransAngleRValue";
            this.textboxTransAngleRValue.Size = new System.Drawing.Size(51, 20);
            this.textboxTransAngleRValue.TabIndex = 33;
            this.textboxTransAngleRValue.Text = "1024";
            this.textboxTransAngleRValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textboxTransAnglePValue
            // 
            this.textboxTransAnglePValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textboxTransAnglePValue.Location = new System.Drawing.Point(67, 44);
            this.textboxTransAnglePValue.Name = "textboxTransAnglePValue";
            this.textboxTransAnglePValue.Size = new System.Drawing.Size(51, 20);
            this.textboxTransAnglePValue.TabIndex = 33;
            this.textboxTransAnglePValue.Text = "1024";
            this.textboxTransAnglePValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textboxTransAngleYValue
            // 
            this.textboxTransAngleYValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textboxTransAngleYValue.Location = new System.Drawing.Point(67, 19);
            this.textboxTransAngleYValue.Name = "textboxTransAngleYValue";
            this.textboxTransAngleYValue.Size = new System.Drawing.Size(51, 20);
            this.textboxTransAngleYValue.TabIndex = 33;
            this.textboxTransAngleYValue.Text = "1024";
            this.textboxTransAngleYValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // buttonTransAngleRn
            // 
            this.buttonTransAngleRn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTransAngleRn.Location = new System.Drawing.Point(3, 66);
            this.buttonTransAngleRn.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTransAngleRn.Name = "buttonTransAngleRn";
            this.buttonTransAngleRn.Size = new System.Drawing.Size(61, 25);
            this.buttonTransAngleRn.TabIndex = 35;
            this.buttonTransAngleRn.Text = "Roll-";
            this.buttonTransAngleRn.UseVisualStyleBackColor = true;
            // 
            // buttonTransAnglePn
            // 
            this.buttonTransAnglePn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTransAnglePn.Location = new System.Drawing.Point(3, 41);
            this.buttonTransAnglePn.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTransAnglePn.Name = "buttonTransAnglePn";
            this.buttonTransAnglePn.Size = new System.Drawing.Size(61, 25);
            this.buttonTransAnglePn.TabIndex = 35;
            this.buttonTransAnglePn.Text = "Pitch-";
            this.buttonTransAnglePn.UseVisualStyleBackColor = true;
            // 
            // buttonTransAngleYn
            // 
            this.buttonTransAngleYn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTransAngleYn.Location = new System.Drawing.Point(3, 16);
            this.buttonTransAngleYn.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTransAngleYn.Name = "buttonTransAngleYn";
            this.buttonTransAngleYn.Size = new System.Drawing.Size(61, 25);
            this.buttonTransAngleYn.TabIndex = 35;
            this.buttonTransAngleYn.Text = "Yaw-";
            this.buttonTransAngleYn.UseVisualStyleBackColor = true;
            // 
            // buttonTransAngleRp
            // 
            this.buttonTransAngleRp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTransAngleRp.Location = new System.Drawing.Point(121, 66);
            this.buttonTransAngleRp.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTransAngleRp.Name = "buttonTransAngleRp";
            this.buttonTransAngleRp.Size = new System.Drawing.Size(61, 25);
            this.buttonTransAngleRp.TabIndex = 35;
            this.buttonTransAngleRp.Text = "Roll+";
            this.buttonTransAngleRp.UseVisualStyleBackColor = true;
            // 
            // buttonTransAnglePp
            // 
            this.buttonTransAnglePp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTransAnglePp.Location = new System.Drawing.Point(121, 41);
            this.buttonTransAnglePp.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTransAnglePp.Name = "buttonTransAnglePp";
            this.buttonTransAnglePp.Size = new System.Drawing.Size(61, 25);
            this.buttonTransAnglePp.TabIndex = 35;
            this.buttonTransAnglePp.Text = "Pitch+";
            this.buttonTransAnglePp.UseVisualStyleBackColor = true;
            // 
            // buttonTransAngleYp
            // 
            this.buttonTransAngleYp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTransAngleYp.Location = new System.Drawing.Point(121, 16);
            this.buttonTransAngleYp.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTransAngleYp.Name = "buttonTransAngleYp";
            this.buttonTransAngleYp.Size = new System.Drawing.Size(61, 25);
            this.buttonTransAngleYp.TabIndex = 35;
            this.buttonTransAngleYp.Text = "Yaw+";
            this.buttonTransAngleYp.UseVisualStyleBackColor = true;
            // 
            // groupBoxTransPos
            // 
            this.groupBoxTransPos.Controls.Add(this.checkBoxTransPosRel);
            this.groupBoxTransPos.Controls.Add(this.textboxTransPosYValue);
            this.groupBoxTransPos.Controls.Add(this.buttonTransPosYp);
            this.groupBoxTransPos.Controls.Add(this.buttonTransPosYn);
            this.groupBoxTransPos.Controls.Add(this.buttonTransPosXpZp);
            this.groupBoxTransPos.Controls.Add(this.textboxTransPosXZValue);
            this.groupBoxTransPos.Controls.Add(this.buttonTransPosXp);
            this.groupBoxTransPos.Controls.Add(this.buttonTransPosXpZn);
            this.groupBoxTransPos.Controls.Add(this.buttonTransPosZn);
            this.groupBoxTransPos.Controls.Add(this.buttonTransPosZp);
            this.groupBoxTransPos.Controls.Add(this.buttonTransPosXnZp);
            this.groupBoxTransPos.Controls.Add(this.buttonTransPosXn);
            this.groupBoxTransPos.Controls.Add(this.buttonTransPosXnZn);
            this.groupBoxTransPos.Location = new System.Drawing.Point(9, 87);
            this.groupBoxTransPos.Name = "groupBoxTransPos";
            this.groupBoxTransPos.Size = new System.Drawing.Size(185, 146);
            this.groupBoxTransPos.TabIndex = 30;
            this.groupBoxTransPos.TabStop = false;
            this.groupBoxTransPos.Text = "Position";
            // 
            // checkBoxTransPosRel
            // 
            this.checkBoxTransPosRel.AutoSize = true;
            this.checkBoxTransPosRel.Location = new System.Drawing.Point(120, 0);
            this.checkBoxTransPosRel.Name = "checkBoxTransPosRel";
            this.checkBoxTransPosRel.Size = new System.Drawing.Size(65, 17);
            this.checkBoxTransPosRel.TabIndex = 37;
            this.checkBoxTransPosRel.Text = "Relative";
            this.checkBoxTransPosRel.UseVisualStyleBackColor = true;
            // 
            // textboxTransPosYValue
            // 
            this.textboxTransPosYValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textboxTransPosYValue.Location = new System.Drawing.Point(140, 70);
            this.textboxTransPosYValue.Name = "textboxTransPosYValue";
            this.textboxTransPosYValue.Size = new System.Drawing.Size(42, 20);
            this.textboxTransPosYValue.TabIndex = 33;
            this.textboxTransPosYValue.Text = "100";
            this.textboxTransPosYValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // buttonTransPosYp
            // 
            this.buttonTransPosYp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTransPosYp.Location = new System.Drawing.Point(140, 16);
            this.buttonTransPosYp.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTransPosYp.Name = "buttonTransPosYp";
            this.buttonTransPosYp.Size = new System.Drawing.Size(42, 42);
            this.buttonTransPosYp.TabIndex = 35;
            this.buttonTransPosYp.Text = "Y+";
            this.buttonTransPosYp.UseVisualStyleBackColor = true;
            // 
            // buttonTransPosYn
            // 
            this.buttonTransPosYn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTransPosYn.Location = new System.Drawing.Point(140, 100);
            this.buttonTransPosYn.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTransPosYn.Name = "buttonTransPosYn";
            this.buttonTransPosYn.Size = new System.Drawing.Size(42, 42);
            this.buttonTransPosYn.TabIndex = 34;
            this.buttonTransPosYn.Text = "Y-";
            this.buttonTransPosYn.UseVisualStyleBackColor = true;
            // 
            // buttonTransPosXpZp
            // 
            this.buttonTransPosXpZp.Location = new System.Drawing.Point(87, 100);
            this.buttonTransPosXpZp.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTransPosXpZp.Name = "buttonTransPosXpZp";
            this.buttonTransPosXpZp.Size = new System.Drawing.Size(42, 42);
            this.buttonTransPosXpZp.TabIndex = 32;
            this.buttonTransPosXpZp.Text = "X+Z+";
            this.buttonTransPosXpZp.UseVisualStyleBackColor = true;
            // 
            // textboxTransPosXZValue
            // 
            this.textboxTransPosXZValue.Location = new System.Drawing.Point(45, 70);
            this.textboxTransPosXZValue.Name = "textboxTransPosXZValue";
            this.textboxTransPosXZValue.Size = new System.Drawing.Size(42, 20);
            this.textboxTransPosXZValue.TabIndex = 27;
            this.textboxTransPosXZValue.Text = "100";
            this.textboxTransPosXZValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // buttonTransPosXp
            // 
            this.buttonTransPosXp.Location = new System.Drawing.Point(87, 58);
            this.buttonTransPosXp.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTransPosXp.Name = "buttonTransPosXp";
            this.buttonTransPosXp.Size = new System.Drawing.Size(42, 42);
            this.buttonTransPosXp.TabIndex = 31;
            this.buttonTransPosXp.Text = "X+";
            this.buttonTransPosXp.UseVisualStyleBackColor = true;
            // 
            // buttonTransPosXpZn
            // 
            this.buttonTransPosXpZn.Location = new System.Drawing.Point(87, 16);
            this.buttonTransPosXpZn.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTransPosXpZn.Name = "buttonTransPosXpZn";
            this.buttonTransPosXpZn.Size = new System.Drawing.Size(42, 42);
            this.buttonTransPosXpZn.TabIndex = 30;
            this.buttonTransPosXpZn.Text = "X+Z-";
            this.buttonTransPosXpZn.UseVisualStyleBackColor = true;
            // 
            // buttonTransPosZn
            // 
            this.buttonTransPosZn.Location = new System.Drawing.Point(45, 16);
            this.buttonTransPosZn.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTransPosZn.Name = "buttonTransPosZn";
            this.buttonTransPosZn.Size = new System.Drawing.Size(42, 42);
            this.buttonTransPosZn.TabIndex = 29;
            this.buttonTransPosZn.Text = "Z-";
            this.buttonTransPosZn.UseVisualStyleBackColor = true;
            // 
            // buttonTransPosZp
            // 
            this.buttonTransPosZp.Location = new System.Drawing.Point(45, 100);
            this.buttonTransPosZp.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTransPosZp.Name = "buttonTransPosZp";
            this.buttonTransPosZp.Size = new System.Drawing.Size(42, 42);
            this.buttonTransPosZp.TabIndex = 28;
            this.buttonTransPosZp.Text = "Z+";
            this.buttonTransPosZp.UseVisualStyleBackColor = true;
            // 
            // buttonTransPosXnZp
            // 
            this.buttonTransPosXnZp.Location = new System.Drawing.Point(3, 100);
            this.buttonTransPosXnZp.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTransPosXnZp.Name = "buttonTransPosXnZp";
            this.buttonTransPosXnZp.Size = new System.Drawing.Size(42, 42);
            this.buttonTransPosXnZp.TabIndex = 27;
            this.buttonTransPosXnZp.Text = "X-Z+";
            this.buttonTransPosXnZp.UseVisualStyleBackColor = true;
            // 
            // buttonTransPosXn
            // 
            this.buttonTransPosXn.Location = new System.Drawing.Point(3, 58);
            this.buttonTransPosXn.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTransPosXn.Name = "buttonTransPosXn";
            this.buttonTransPosXn.Size = new System.Drawing.Size(42, 42);
            this.buttonTransPosXn.TabIndex = 26;
            this.buttonTransPosXn.Text = "X-";
            this.buttonTransPosXn.UseVisualStyleBackColor = true;
            // 
            // buttonTransPosXnZn
            // 
            this.buttonTransPosXnZn.Location = new System.Drawing.Point(3, 16);
            this.buttonTransPosXnZn.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTransPosXnZn.Name = "buttonTransPosXnZn";
            this.buttonTransPosXnZn.Size = new System.Drawing.Size(42, 42);
            this.buttonTransPosXnZn.TabIndex = 25;
            this.buttonTransPosXnZn.Text = "X-Z-";
            this.buttonTransPosXnZn.UseVisualStyleBackColor = true;
            // 
            // buttonTransReset
            // 
            this.buttonTransReset.Location = new System.Drawing.Point(6, 19);
            this.buttonTransReset.Name = "buttonTransReset";
            this.buttonTransReset.Size = new System.Drawing.Size(75, 23);
            this.buttonTransReset.TabIndex = 0;
            this.buttonTransReset.Text = "Reset";
            this.buttonTransReset.UseVisualStyleBackColor = true;
            // 
            // splitContainerModelTables
            // 
            this.splitContainerModelTables.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerModelTables.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerModelTables.Location = new System.Drawing.Point(3, 29);
            this.splitContainerModelTables.Name = "splitContainerModelTables";
            this.splitContainerModelTables.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerModelTables.Panel1
            // 
            this.splitContainerModelTables.Panel1.Controls.Add(this.labelModelVertices);
            this.splitContainerModelTables.Panel1.Controls.Add(this.dataGridViewVertices);
            // 
            // splitContainerModelTables.Panel2
            // 
            this.splitContainerModelTables.Panel2.Controls.Add(this.labelModelTriangles);
            this.splitContainerModelTables.Panel2.Controls.Add(this.dataGridViewTriangles);
            this.splitContainerModelTables.Size = new System.Drawing.Size(0, 412);
            this.splitContainerModelTables.SplitterDistance = 200;
            this.splitContainerModelTables.TabIndex = 2;
            // 
            // labelModelVertices
            // 
            this.labelModelVertices.AutoSize = true;
            this.labelModelVertices.Location = new System.Drawing.Point(3, 0);
            this.labelModelVertices.Name = "labelModelVertices";
            this.labelModelVertices.Size = new System.Drawing.Size(48, 13);
            this.labelModelVertices.TabIndex = 11;
            this.labelModelVertices.Text = "Vertices:";
            // 
            // dataGridViewVertices
            // 
            this.dataGridViewVertices.AllowUserToAddRows = false;
            this.dataGridViewVertices.AllowUserToDeleteRows = false;
            this.dataGridViewVertices.AllowUserToResizeRows = false;
            this.dataGridViewVertices.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewVertices.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewVertices.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewVertices.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Index,
            this.X,
            this.Y,
            this.Z});
            this.dataGridViewVertices.Location = new System.Drawing.Point(3, 16);
            this.dataGridViewVertices.Name = "dataGridViewVertices";
            this.dataGridViewVertices.ReadOnly = true;
            this.dataGridViewVertices.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewVertices.Size = new System.Drawing.Size(0, 181);
            this.dataGridViewVertices.TabIndex = 1;
            // 
            // Index
            // 
            this.Index.HeaderText = "Index";
            this.Index.Name = "Index";
            this.Index.ReadOnly = true;
            // 
            // X
            // 
            this.X.HeaderText = "X";
            this.X.Name = "X";
            this.X.ReadOnly = true;
            // 
            // Y
            // 
            this.Y.HeaderText = "Y";
            this.Y.Name = "Y";
            this.Y.ReadOnly = true;
            // 
            // Z
            // 
            this.Z.HeaderText = "Z";
            this.Z.Name = "Z";
            this.Z.ReadOnly = true;
            // 
            // labelModelTriangles
            // 
            this.labelModelTriangles.AutoSize = true;
            this.labelModelTriangles.Location = new System.Drawing.Point(3, 0);
            this.labelModelTriangles.Name = "labelModelTriangles";
            this.labelModelTriangles.Size = new System.Drawing.Size(53, 13);
            this.labelModelTriangles.TabIndex = 12;
            this.labelModelTriangles.Text = "Triangles:";
            // 
            // dataGridViewTriangles
            // 
            this.dataGridViewTriangles.AllowUserToAddRows = false;
            this.dataGridViewTriangles.AllowUserToDeleteRows = false;
            this.dataGridViewTriangles.AllowUserToResizeRows = false;
            this.dataGridViewTriangles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewTriangles.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewTriangles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewTriangles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Group,
            this.Type,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4});
            this.dataGridViewTriangles.Location = new System.Drawing.Point(3, 16);
            this.dataGridViewTriangles.Name = "dataGridViewTriangles";
            this.dataGridViewTriangles.ReadOnly = true;
            this.dataGridViewTriangles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewTriangles.Size = new System.Drawing.Size(0, 189);
            this.dataGridViewTriangles.TabIndex = 2;
            // 
            // Group
            // 
            this.Group.HeaderText = "Group";
            this.Group.Name = "Group";
            this.Group.ReadOnly = true;
            // 
            // Type
            // 
            this.Type.HeaderText = "Type";
            this.Type.Name = "Type";
            this.Type.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.HeaderText = "T1";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.HeaderText = "T2";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.HeaderText = "T3";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Model Address:";
            // 
            // textBoxModelAddress
            // 
            this.textBoxModelAddress.Location = new System.Drawing.Point(87, 3);
            this.textBoxModelAddress.Name = "textBoxModelAddress";
            this.textBoxModelAddress.ReadOnly = true;
            this.textBoxModelAddress.Size = new System.Drawing.Size(100, 20);
            this.textBoxModelAddress.TabIndex = 9;
            // 
            // glControlModelView
            // 
            this.glControlModelView.BackColor = System.Drawing.Color.Black;
            this.glControlModelView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControlModelView.Location = new System.Drawing.Point(0, 0);
            this.glControlModelView.Margin = new System.Windows.Forms.Padding(0);
            this.glControlModelView.Name = "glControlModelView";
            this.glControlModelView.Padding = new System.Windows.Forms.Padding(2);
            this.glControlModelView.Size = new System.Drawing.Size(496, 461);
            this.glControlModelView.TabIndex = 0;
            // 
            // ModelTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainerModel);
            this.Name = "ModelTab";
            this.Size = new System.Drawing.Size(915, 463);
            this.splitContainerModel.Panel1.ResumeLayout(false);
            this.splitContainerModel.Panel1.PerformLayout();
            this.splitContainerModel.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerModel)).EndInit();
            this.splitContainerModel.ResumeLayout(false);
            this.groupBoxTransformation.ResumeLayout(false);
            this.groupBoxTransformation.PerformLayout();
            this.groupBoxTransScale.ResumeLayout(false);
            this.groupBoxTransScale.PerformLayout();
            this.groupBoxTransAngle.ResumeLayout(false);
            this.groupBoxTransAngle.PerformLayout();
            this.groupBoxTransPos.ResumeLayout(false);
            this.groupBoxTransPos.PerformLayout();
            this.splitContainerModelTables.Panel1.ResumeLayout(false);
            this.splitContainerModelTables.Panel1.PerformLayout();
            this.splitContainerModelTables.Panel2.ResumeLayout(false);
            this.splitContainerModelTables.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerModelTables)).EndInit();
            this.splitContainerModelTables.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewVertices)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTriangles)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private BetterSplitContainer splitContainerModel;
        private System.Windows.Forms.CheckBox checkBoxModelLevel;
        private System.Windows.Forms.GroupBox groupBoxTransformation;
        private System.Windows.Forms.Label labelTransScaleValue;
        private System.Windows.Forms.Label labelTransAngleValue;
        private System.Windows.Forms.Label labelTransPosValue;
        private System.Windows.Forms.CheckBox checkBoxTransUseObj;
        private System.Windows.Forms.GroupBox groupBoxTransScale;
        private System.Windows.Forms.CheckBox checkBoxTransScaleMultiply;
        private System.Windows.Forms.CheckBox checkBoxTransScaleAggregate;
        private BetterTextbox textbpxTransScaleDValue;
        private BetterTextbox textbpxTransScaleHValue;
        private BetterTextbox textbpxTransScaleWValue;
        private System.Windows.Forms.Button buttonTransScaleDn;
        private System.Windows.Forms.Button buttonTransScaleHn;
        private System.Windows.Forms.Button buttonTransScaleWn;
        private System.Windows.Forms.Button buttonTransScaleDp;
        private System.Windows.Forms.Button buttonTransScaleHp;
        private System.Windows.Forms.Button buttonTransScaleWp;
        private BetterTextbox betterTextbox4;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.GroupBox groupBoxTransAngle;
        private BetterTextbox textboxTransAngleRValue;
        private BetterTextbox textboxTransAnglePValue;
        private BetterTextbox textboxTransAngleYValue;
        private System.Windows.Forms.Button buttonTransAngleRn;
        private System.Windows.Forms.Button buttonTransAnglePn;
        private System.Windows.Forms.Button buttonTransAngleYn;
        private System.Windows.Forms.Button buttonTransAngleRp;
        private System.Windows.Forms.Button buttonTransAnglePp;
        private System.Windows.Forms.Button buttonTransAngleYp;
        private System.Windows.Forms.GroupBox groupBoxTransPos;
        private System.Windows.Forms.CheckBox checkBoxTransPosRel;
        private BetterTextbox textboxTransPosYValue;
        private System.Windows.Forms.Button buttonTransPosYp;
        private System.Windows.Forms.Button buttonTransPosYn;
        private System.Windows.Forms.Button buttonTransPosXpZp;
        private BetterTextbox textboxTransPosXZValue;
        private System.Windows.Forms.Button buttonTransPosXp;
        private System.Windows.Forms.Button buttonTransPosXpZn;
        private System.Windows.Forms.Button buttonTransPosZn;
        private System.Windows.Forms.Button buttonTransPosZp;
        private System.Windows.Forms.Button buttonTransPosXnZp;
        private System.Windows.Forms.Button buttonTransPosXn;
        private System.Windows.Forms.Button buttonTransPosXnZn;
        private System.Windows.Forms.Button buttonTransReset;
        private BetterSplitContainer splitContainerModelTables;
        private System.Windows.Forms.Label labelModelVertices;
        private System.Windows.Forms.DataGridView dataGridViewVertices;
        private System.Windows.Forms.DataGridViewTextBoxColumn Index;
        private System.Windows.Forms.DataGridViewTextBoxColumn X;
        private System.Windows.Forms.DataGridViewTextBoxColumn Y;
        private System.Windows.Forms.DataGridViewTextBoxColumn Z;
        private System.Windows.Forms.Label labelModelTriangles;
        private System.Windows.Forms.DataGridView dataGridViewTriangles;
        private System.Windows.Forms.DataGridViewTextBoxColumn Group;
        private System.Windows.Forms.DataGridViewTextBoxColumn Type;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxModelAddress;
        private GLControl glControlModelView;
    }
}
