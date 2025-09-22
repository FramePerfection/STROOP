using STROOP.Core;
using STROOP.M64;
using STROOP.Structs;
using STROOP.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace STROOP.Tabs
{
    public partial class M64Tab : STROOPTab
    {
        private readonly M64File _m64File;

        private ushort? _copiedCountryCode = null;
        private uint? _copiedCrc32 = null;

        public M64Tab()
        {
            InitializeComponent();
            _m64File = new M64File(this);
        }

        public override string GetDisplayName() => "M64";

        public override void InitializeTab()
        {
            base.InitializeTab();

            buttonM64Save.Click += (sender, e) => Save();
            buttonM64SaveAs.Click += (sender, e) => SaveAs();
            buttonM64ResetChanges.Click += (sender, e) => _m64File.ResetChanges();
            buttonM64Open.Click += (sender, e) => Open();
            buttonM64Close.Click += (sender, e) => Close();

            buttonM64Goto.Click += (sender, e) => Goto();
            textBoxM64Goto.AddEnterAction(() => Goto());

            buttonM64SetUsRom.Click += (sender, e) => SetHeaderRomVersion(RomVersion.US);
            buttonM64SetJpRom.Click += (sender, e) => SetHeaderRomVersion(RomVersion.JP);
            buttonM64CopyRom.Click += (sender, e) => CopyHeaderRomVersion();
            buttonM64PasteRom.Click += (sender, e) => PasteHeaderRomVersion();

            dataGridViewM64Inputs.DataError += (sender, e) => dataGridViewM64Inputs.CancelEdit();
            dataGridViewM64Inputs.SelectionChanged += (sender, e) => UpdateSelectionTextboxes();
            dataGridViewM64Inputs.CellContentClick += (sender, e) =>
            {
                if (e.ColumnIndex >= 4)
                {
                    dataGridViewM64Inputs.ClearSelection();
                    dataGridViewM64Inputs.Parent.Focus();
                }
            };
            ControlUtilities.SetTableDoubleBuffered(dataGridViewM64Inputs, true);

            dataGridViewM64Inputs.DataSource = _m64File.Inputs;
            UpdateTableSettings();
            propertyGridM64Header.SelectedObject = _m64File.Header;
            propertyGridM64Header.Refresh();
            propertyGridM64Stats.SelectedObject = _m64File.Stats;
            propertyGridM64Stats.Refresh();
            propertyGridM64Stats.ContextMenuStrip = _m64File.Stats.CreateContextMenuStrip();
            tabControlM64Details.SelectedIndexChanged += TabControlDetails_SelectedIndexChanged;

            dataGridViewM64Inputs.DragOver += (object sender, DragEventArgs e) =>
                e.Effect = DragDropEffects.Copy;
            dataGridViewM64Inputs.DragDrop += (object sender, DragEventArgs e) =>
            {
                string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                if (filePaths.Length == 0) return;
                string filePath = filePaths[0];
                string fileName = Path.GetFileName(filePath);
                Open(filePath, fileName);
            };

            buttonM64TurnOffRowRange.Click += (sender, e) => SetValuesOfSelection(CellSelectionType.RowRange, false);
            buttonM64TurnOffInputRange.Click += (sender, e) => SetValuesOfSelection(CellSelectionType.PartialRowRange, false);
            buttonM64TurnOffCells.Click += (sender, e) => SetValuesOfSelection(CellSelectionType.Cells, false);
            buttonM64TurnOnInputRange.Click += (sender, e) => SetValuesOfSelection(CellSelectionType.PartialRowRange, true);
            buttonM64TurnOnCells.Click += (sender, e) => SetValuesOfSelection(CellSelectionType.Cells, true);

            buttonM64DeleteRowRange.Click += (sender, e) => DeleteRows();

            buttonM64CopyInputRange.Click += (sender, e) => CopyData(false);
            buttonM64CopyRowRange.Click += (sender, e) => CopyData(true);
            buttonM64PasteInsert.Click += (sender, e) => PasteData(true);
            buttonM64PasteOverwrite.Click += (sender, e) => PasteData(false);

            listBoxM64Copied.Items.Add(M64CopiedData.OneEmptyFrame);
            listBoxM64Copied.SelectedItem = M64CopiedData.OneEmptyFrame;
            listBoxM64Copied.KeyDown += (sender, e) => ListBoxCopied_KeyDown();

            comboBoxM64FrameInputRelation.DataSource = Enum.GetValues(typeof(FrameInputRelationType));
            comboBoxM64FrameInputRelation.SelectedItem = M64Config.FrameInputRelation;

            buttonM64QuickDuplicationDuplicate.Click += (sender, e) => PerformQuickDuplication();
            buttonM64AddPauseBufferFrames.Click += (sender, e) => AddPauseBufferFrames();

            progressBarM64.Visible = false;
            labelM64ProgressBar.Visible = false;
        }

        private void DeleteRows()
        {
            (int? startFrame, int? endFrame) = GetFrameBounds();
            if (!startFrame.HasValue || !endFrame.HasValue) return;
            _m64File.DeleteRows(startFrame.Value, endFrame.Value);
        }

        private void PasteData(bool insert)
        {
            M64CopiedData copiedData = listBoxM64Copied.SelectedItem as M64CopiedData;
            if (copiedData == null) return;
            (int? startFrame, int? endFrame) = GetFrameBounds();
            if (!startFrame.HasValue) return;
            int? multiplicity = ParsingUtilities.ParseIntNullable(textBoxM64PasteMultiplicity.Text);
            if (!multiplicity.HasValue) return;
            _m64File.Paste(copiedData, startFrame.Value, insert, multiplicity.Value);
        }

        private void CopyData(bool useRow)
        {
            (int? startFrame, int? endFrame) = GetFrameBounds();
            string inputsString = textBoxM64SelectionInputs.Text;
            if (!startFrame.HasValue || !endFrame.HasValue) return;
            M64CopiedData copiedData = M64CopiedData.CreateCopiedData(
                dataGridViewM64Inputs, _m64File.CurrentFileName,
                startFrame.Value, endFrame.Value, useRow, inputsString);
            if (copiedData == null) return;
            listBoxM64Copied.Items.Add(copiedData);
            listBoxM64Copied.SelectedItem = copiedData;
        }

        private void CopyHeaderRomVersion()
        {
            if (_m64File.RawBytes == null) return;
            _copiedCountryCode = _m64File.Header.CountryCode;
            _copiedCrc32 = _m64File.Header.Crc32;
        }

        private void PasteHeaderRomVersion()
        {
            if (_m64File.RawBytes == null) return;
            if (!_copiedCountryCode.HasValue || !_copiedCrc32.HasValue) return;
            _m64File.Header.CountryCode = _copiedCountryCode.Value;
            _m64File.Header.Crc32 = _copiedCrc32.Value;
            propertyGridM64Header.Refresh();
        }

        private void SetHeaderRomVersion(RomVersion romVersion)
        {
            if (_m64File.RawBytes == null) return;
            switch (romVersion)
            {
                case RomVersion.US:
                    _m64File.Header.CountryCode = M64Config.CountryCodeUS;
                    _m64File.Header.Crc32 = M64Config.CrcUS;
                    break;
                case RomVersion.JP:
                    _m64File.Header.CountryCode = M64Config.CountryCodeJP;
                    _m64File.Header.Crc32 = M64Config.CrcJP;
                    break;
                case RomVersion.SH:
                default:
                    throw new ArgumentOutOfRangeException();
            }

            propertyGridM64Header.Refresh();
        }

        private void TabControlDetails_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlM64Details.SelectedTab == tabPageM64Inputs)
            {
                dataGridViewM64Inputs.Refresh();
            }
            else if (tabControlM64Details.SelectedTab == tabPageM64Header)
            {
                ControlUtilities.SetPropertyGridLabelColumnWidth(propertyGridM64Header, 160);
                propertyGridM64Header.Refresh();
            }
            else if (tabControlM64Details.SelectedTab == tabPageM64Stats)
            {
                ControlUtilities.SetPropertyGridLabelColumnWidth(propertyGridM64Stats, 160);
                propertyGridM64Stats.Refresh();
            }
        }

        public void UpdateSelectionTextboxes()
        {
            List<M64InputCell> cells = M64Utilities.GetSelectedInputCells(
                dataGridViewM64Inputs, CellSelectionType.Cells);
            (int? minFrame, int? maxFrame, string inputsString) = M64Utilities.GetCellStats(cells, true);
            if (minFrame.HasValue) textBoxM64SelectionStartFrame.Text = minFrame.Value.ToString();
            if (maxFrame.HasValue) textBoxM64SelectionEndFrame.Text = maxFrame.Value.ToString();
            textBoxM64SelectionInputs.Text = inputsString;
        }

        private void SetValuesOfSelection(CellSelectionType cellSelectionType, bool value)
        {
            (int? startFrame, int? endFrame) = GetFrameBounds();
            List<M64InputCell> cells = M64Utilities.GetSelectedInputCells(
                dataGridViewM64Inputs,
                cellSelectionType,
                startFrame,
                endFrame,
                textBoxM64SelectionInputs.Text);
            int? intOnValue = ParsingUtilities.ParseIntNullable(textBoxM64OnValue.Text);
            cells.ForEach(cell => cell.SetValue(value, intOnValue));
            dataGridViewM64Inputs.Refresh();
        }

        public void Goto(int? gotoValueNullable = null)
        {
            gotoValueNullable = gotoValueNullable ?? ParsingUtilities.ParseIntNullable(textBoxM64Goto.Text);
            if (gotoValueNullable.HasValue)
            {
                int gotoValue = M64Utilities.ConvertDisplayedValueToFrame(gotoValueNullable.Value);
                ControlUtilities.TableGoTo(dataGridViewM64Inputs, gotoValue);
            }
        }

        private void SaveAs()
        {
            SaveFileDialog saveFileDialog = DialogUtilities.CreateSaveFileDialog(FileType.MupenMovie);
            DialogResult dialogResult = saveFileDialog.ShowDialog();
            if (dialogResult != DialogResult.OK)
                return;

            string filePath = saveFileDialog.FileName;
            string fileName = new FileInfo(filePath).Name;
            bool success = _m64File.Save(filePath, fileName);
            if (!success)
            {
                MessageBox.Show(
                    "Could not save file.\n" +
                    "Perhaps Mupen is currently editing it.\n" +
                    "Try closing Mupen and trying again.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void Save()
        {
            bool success = _m64File.Save();
            if (!success)
            {
                MessageBox.Show(
                    "Could not save file.\n" +
                    "Perhaps Mupen is currently editing it.\n" +
                    "Try closing Mupen and trying again.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void Open()
        {
            OpenFileDialog openFileDialog = DialogUtilities.CreateOpenFileDialog(FileType.MupenMovie);
            DialogResult dialogResult = openFileDialog.ShowDialog();
            if (dialogResult != DialogResult.OK)
                return;

            string filePath = openFileDialog.FileName;
            string fileName = openFileDialog.SafeFileName;
            Open(filePath, fileName);
        }

        public void Open(string filePath, string fileName)
        {
            dataGridViewM64Inputs.DataSource = null;
            propertyGridM64Header.SelectedObject = null;
            bool success = _m64File.OpenFile(filePath, fileName);
            if (!success)
            {
                MessageBox.Show(
                    "Could not open file " + filePath + ".\n" +
                    "Perhaps Mupen is currently editing it.\n" +
                    "Try closing Mupen and trying again.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            dataGridViewM64Inputs.DataSource = _m64File.Inputs;
            UpdateTableSettings();
            propertyGridM64Header.SelectedObject = _m64File.Header;
            dataGridViewM64Inputs.Refresh();
            propertyGridM64Header.Refresh();
            propertyGridM64Stats.Refresh();
        }

        private void Close()
        {
            _m64File.Close();
            dataGridViewM64Inputs.Refresh();
            propertyGridM64Header.Refresh();
            propertyGridM64Stats.Refresh();
        }

        private void ListBoxCopied_KeyDown()
        {
            if (GlobalKeyboard.IsDeletishKeyDown())
            {
                M64CopiedData copiedData = listBoxM64Copied.SelectedItem as M64CopiedData;
                if (copiedData == null || copiedData == M64CopiedData.OneEmptyFrame) return;
                int index = listBoxM64Copied.SelectedIndex;
                listBoxM64Copied.Items.Remove(copiedData);
                if (index == listBoxM64Copied.Items.Count) index--;
                listBoxM64Copied.SelectedIndex = index;
            }
        }

        private void PerformQuickDuplication()
        {
            int? iter1StartObserved = ParsingUtilities.ParseIntNullable(
                textBoxM64QuickDuplication1stIterationStart.Text);
            int? iter2StartObserved = ParsingUtilities.ParseIntNullable(
                textBoxM64QuickDuplication2ndIterationStart.Text);
            int? totalIters = ParsingUtilities.ParseIntNullable(
                textBoxM64QuickDuplicationTotalIterations.Text);
            if (!iter1StartObserved.HasValue ||
                !iter2StartObserved.HasValue ||
                !totalIters.HasValue) return;

            int iter1Start = iter1StartObserved.Value - 1;
            int iter2Start = iter2StartObserved.Value - 1;
            int multiplicity = totalIters.Value - 1;
            int iter1End = iter2Start - 1;

            M64CopiedData copiedData = M64CopiedData.CreateCopiedData(
                dataGridViewM64Inputs, _m64File.CurrentFileName,
                iter1Start, iter1End, true /* useRow */);
            _m64File.Paste(copiedData, iter2Start, true /* insert */, multiplicity);
        }

        private void AddPauseBufferFrames()
        {
            (int? startFrameNullable, int? endFrameNullable) = GetFrameBounds();
            if (!startFrameNullable.HasValue || !endFrameNullable.HasValue) return;
            int startFrame = startFrameNullable.Value;
            int endFrame = endFrameNullable.Value;
            _m64File.AddPauseBufferFrames(startFrame, endFrame);
        }

        private (int? startFrame, int? endFrame) GetFrameBounds()
        {
            int? startFrame = ParsingUtilities.ParseIntNullable(textBoxM64SelectionStartFrame.Text);
            int? endFrame = ParsingUtilities.ParseIntNullable(textBoxM64SelectionEndFrame.Text);
            if (startFrame.HasValue) startFrame = M64Utilities.ConvertDisplayedValueToFrame(startFrame.Value);
            if (endFrame.HasValue) endFrame = M64Utilities.ConvertDisplayedValueToFrame(endFrame.Value);
            return (startFrame, endFrame);
        }

        public void UpdateTableSettings(IEnumerable<M64InputFrame> modifiedFrames = null)
        {
            DataGridView table = dataGridViewM64Inputs;
            if (table.Columns.Count != M64Utilities.ColumnParameters.Count)
                throw new ArgumentOutOfRangeException();

            if (modifiedFrames != null)
            {
                foreach (M64InputFrame input in modifiedFrames)
                {
                    input.UpdateRowColor();
                    input.UpdateCellColors();
                }
            }

            for (int i = 0; i < table.Columns.Count; i++)
            {
                (string headerText, int fillWeight, Color backColor) = M64Utilities.ColumnParameters[i];
                table.Columns[i].HeaderText = headerText;
                table.Columns[i].FillWeight = fillWeight;
                table.Columns[i].DefaultCellStyle.BackColor = backColor;
                table.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        public override void Update(bool updateView)
        {
            if (!updateView) return;

            string fileName = _m64File.CurrentFileName ?? "(No File Opened)";
            string isModifiedSuffix = _m64File.IsModified ? " [MODIFIED]" : "";
            labelM64FileName.Text = fileName + isModifiedSuffix;

            int currentFrameCount = _m64File.Inputs.Count;
            int originalFrameCount = _m64File.OriginalFrameCount;
            int frameCountDiff = currentFrameCount - originalFrameCount;
            labelM64NumInputsValue.Text = String.Format(
                "{0} / {1} [{2}]",
                currentFrameCount,
                originalFrameCount,
                StringUtilities.FormatIntegerWithSign(frameCountDiff));

            FrameInputRelationType selectedFrameInputRelation =
                (FrameInputRelationType)comboBoxM64FrameInputRelation.SelectedItem;
            if (selectedFrameInputRelation != M64Config.FrameInputRelation)
            {
                M64Config.FrameInputRelation = selectedFrameInputRelation;
                dataGridViewM64Inputs.Refresh();
                UpdateSelectionTextboxes();
            }
        }
    }
}
