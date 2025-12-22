using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.ComponentModel;
using STROOP.Structs;
using STROOP.Utilities;

namespace STROOP.M64
{
    public class M64File
    {
        private readonly Tabs.M64Tab tab;

        public string CurrentFilePath { get; private set; }
        public string CurrentFileName { get; private set; }
        public byte[] RawBytes { get; private set; }
        public int OriginalFrameCount { get; private set; }

        public bool IsModified = false;
        public readonly HashSet<M64InputFrame> ModifiedFrames = new HashSet<M64InputFrame>();

        public M64Header Header { get; }
        public BindingList<M64InputFrame> Inputs { get; }
        public M64Stats Stats { get; }

        public M64File(Tabs.M64Tab gui)
        {
            this.tab = gui;
            Header = new M64Header(this, gui);
            Inputs = new BindingList<M64InputFrame>();
            Stats = new M64Stats(this);
        }

        public bool OpenFile(string filePath, string fileName)
        {
            if (!File.Exists(filePath))
                return false;

            byte[] movieBytes;
            try
            {
                movieBytes = DialogUtilities.ReadFileBytes(filePath);
            }
            catch (IOException)
            {
                return false;
            }

            bool loadedSuccessfully = LoadBytes(movieBytes);
            if (loadedSuccessfully)
            {
                CurrentFilePath = filePath;
                CurrentFileName = fileName;
            }

            return true;
        }

        private bool LoadBytes(byte[] fileBytes)
        {
            // Check Header
            if (!fileBytes.Take(4).SequenceEqual(M64Config.SignatureBytes))
                return false;

            if (fileBytes.Length < M64Config.HeaderSize)
                return false;

            M64InputFrame.ClassIdIndex = 0;
            RawBytes = fileBytes;
            Inputs.Clear();
            byte[] headerBytes = fileBytes.Take(M64Config.HeaderSize).ToArray();
            Header.LoadBytes(headerBytes);
            byte[] frameBytes = fileBytes.Skip(M64Config.HeaderSize).ToArray();

            IsModified = false;
            ModifiedFrames.Clear();
            OriginalFrameCount = Header.NumInputs;
            for (int i = 0; i < frameBytes.Length && i < 4 * OriginalFrameCount; i += 4)
            {
                Inputs.Add(new M64InputFrame(
                    i / 4, BitConverter.ToUInt32(frameBytes, i), true, this, tab.dataGridViewM64Inputs));
            }

            tab.dataGridViewM64Inputs.Refresh();
            tab.propertyGridM64Header.Refresh();
            tab.propertyGridM64Stats.Refresh();

            return true;
        }

        private byte[] ToBytes()
        {
            byte[] headerBytes = Header.ToBytes();
            byte[] inputBytes = Inputs.SelectMany(input => input.ToBytes()).ToArray();
            return headerBytes.Concat(inputBytes).ToArray();
        }

        public bool Save()
        {
            if (RawBytes == null) return false;
            if (CurrentFilePath == null || CurrentFileName == null) return false;
            return Save(CurrentFilePath, CurrentFileName);
        }

        public bool Save(string filePath, string fileName)
        {
            if (RawBytes == null) return false;
            try
            {
                if (tab.checkBoxMaxOutViCount.Checked)
                    Header.NumVis = int.MaxValue;
                DialogUtilities.WriteFileBytes(filePath, ToBytes());
                int currentPosition = tab.dataGridViewM64Inputs.FirstDisplayedScrollingRowIndex;
                tab.Open(filePath, fileName);
                tab.Goto(currentPosition);
            }
            catch (IOException)
            {
                return false;
            }

            return true;
        }

        public void Close()
        {
            Header.Clear();
            Inputs.Clear();
            CurrentFilePath = null;
            CurrentFileName = null;
            RawBytes = null;
            OriginalFrameCount = 0;
            IsModified = false;
        }

        public void ResetChanges()
        {
            if (RawBytes == null) return;
            int currentPosition = tab.dataGridViewM64Inputs.FirstDisplayedScrollingRowIndex;
            tab.Open(CurrentFilePath, CurrentFileName);
            tab.Goto(currentPosition);
        }

        public void DeleteRows(int startIndex, int endIndex)
        {
            startIndex = Math.Max(startIndex, 0);
            endIndex = Math.Min(endIndex, Inputs.Count - 1);
            int numDeletes = endIndex - startIndex + 1;
            if (numDeletes <= 0) return;

            int currentPosition = tab.dataGridViewM64Inputs.FirstDisplayedScrollingRowIndex;
            tab.dataGridViewM64Inputs.DataSource = null;
            for (int i = 0; i < numDeletes; i++)
            {
                ModifiedFrames.Remove(Inputs[startIndex]);
                Inputs.RemoveAt(startIndex);
            }

            RefreshInputFrames(startIndex);
            tab.dataGridViewM64Inputs.DataSource = Inputs;
            tab.UpdateTableSettings(ModifiedFrames);
            ControlUtilities.TableGoTo(tab.dataGridViewM64Inputs, currentPosition);

            IsModified = true;
            Header.NumInputs = Inputs.Count;
            tab.dataGridViewM64Inputs.Refresh();
            tab.UpdateSelectionTextboxes();
        }

        public void Paste(M64CopiedData copiedData, int index, bool insert, int multiplicity)
        {
            if (RawBytes == null) return;
            index = MoreMath.Clamp(index, 0, Inputs.Count);
            int pasteCount = copiedData.TotalFrames * multiplicity;
            bool bigPaste = pasteCount > M64Config.PasteWarningLimit;
            if (bigPaste)
            {
                if (!DialogUtilities.AskQuestionAboutM64Pasting(pasteCount)) return;
                SetPasteProgressCount(0, pasteCount);
                SetPasteProgressVisibility(true);
            }

            if (insert)
            {
                int currentPosition = tab.dataGridViewM64Inputs.FirstDisplayedScrollingRowIndex;
                tab.dataGridViewM64Inputs.DataSource = null;
                for (int i = 0; i < pasteCount; i++)
                {
                    int insertionIndex = index + i;
                    M64InputFrame newInput = new M64InputFrame(
                        insertionIndex, copiedData.GetRawValue(i), false, this, tab.dataGridViewM64Inputs);
                    Inputs.Insert(insertionIndex, newInput);
                    ModifiedFrames.Add(newInput);

                    if (bigPaste)
                    {
                        SetPasteProgressCount(i + 1, pasteCount);
                    }
                }

                RefreshInputFrames(index);
                tab.dataGridViewM64Inputs.DataSource = Inputs;
                tab.UpdateTableSettings(ModifiedFrames);
                ControlUtilities.TableGoTo(tab.dataGridViewM64Inputs, currentPosition);
            }
            else
            {
                List<M64InputFrame> inputsToOverwrite = Inputs.Skip(index).Take(pasteCount).ToList();
                copiedData.Apply(inputsToOverwrite);
            }

            if (bigPaste)
            {
                SetPasteProgressVisibility(false);
            }

            IsModified = true;
            Header.NumInputs = Inputs.Count;
            RefreshInputFrames(index);
            tab.dataGridViewM64Inputs.Refresh();
            tab.UpdateSelectionTextboxes();
        }

        public void AddPauseBufferFrames(int startIndex, int endIndex)
        {
            if (RawBytes == null) return;
            if (startIndex > endIndex) return;
            startIndex = MoreMath.Clamp(startIndex, 0, Inputs.Count - 1);
            endIndex = MoreMath.Clamp(endIndex, 0, Inputs.Count - 1);

            for (int index = startIndex; index <= endIndex; index++)
            {
                M64CopiedData.OnePauseFrameOverwrite.Apply(Inputs[index]);
            }

            int currentPosition = tab.dataGridViewM64Inputs.FirstDisplayedScrollingRowIndex;
            tab.dataGridViewM64Inputs.DataSource = null;

            for (int index = startIndex; index <= endIndex; index++)
            {
                int currentFrame = startIndex + (index - startIndex) * 4;

                M64InputFrame newInput1 = new M64InputFrame(
                    currentFrame + 1, M64CopiedData.OneEmptyFrame.GetRawValue(0), false, this, tab.dataGridViewM64Inputs);
                Inputs.Insert(currentFrame + 1, newInput1);
                ModifiedFrames.Add(newInput1);

                M64InputFrame newInput2 = new M64InputFrame(
                    currentFrame + 2, M64CopiedData.OnePauseFrame.GetRawValue(0), false, this, tab.dataGridViewM64Inputs);
                Inputs.Insert(currentFrame + 2, newInput2);
                ModifiedFrames.Add(newInput2);

                M64InputFrame newInput3 = new M64InputFrame(
                    currentFrame + 3, M64CopiedData.OneEmptyFrame.GetRawValue(0), false, this, tab.dataGridViewM64Inputs);
                Inputs.Insert(currentFrame + 3, newInput3);
                ModifiedFrames.Add(newInput3);
            }

            RefreshInputFrames(startIndex);
            tab.dataGridViewM64Inputs.DataSource = Inputs;
            tab.UpdateTableSettings(ModifiedFrames);
            ControlUtilities.TableGoTo(tab.dataGridViewM64Inputs, currentPosition);

            IsModified = true;
            Header.NumInputs = Inputs.Count;
            tab.dataGridViewM64Inputs.Refresh();
            tab.UpdateSelectionTextboxes();
        }

        private void SetPasteProgressVisibility(bool visibility)
        {
            tab.labelM64ProgressBar.Visible = visibility;
            tab.labelM64ProgressBar.Update();
            tab.progressBarM64.Visible = visibility;
            tab.progressBarM64.Update();
        }

        private void SetPasteProgressCount(int value, int maximum)
        {
            string maximumString = maximum.ToString();
            string valueString = String.Format("{0:D" + maximumString.Length + "}", value);
            double percent = Math.Round(100d * value / maximum, 1);
            string percentString = percent.ToString("N1");
            tab.labelM64ProgressBar.Text = String.Format(
                "{0}% ({1} / {2})", percentString, valueString, maximumString);
            tab.labelM64ProgressBar.Update();
            tab.progressBarM64.Maximum = maximum;
            tab.progressBarM64.Value = value;
            tab.progressBarM64.Update();
        }

        private void RefreshInputFrames(int startIndex = 0)
        {
            for (int i = startIndex; i < Inputs.Count; i++)
            {
                Inputs[i].FrameIndex = i;
            }
        }
    }
}
