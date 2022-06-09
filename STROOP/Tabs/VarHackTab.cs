﻿using System;
using System.Collections.Generic;
using STROOP.Structs;
using System.Windows.Forms;
using STROOP.Utilities;
using STROOP.Controls;
using STROOP.Structs.Configurations;
using STROOP.Managers;

namespace STROOP.Tabs
{
    public partial class VarHackTab : STROOPTab, IVariableAdder
    {
        public VarHackTab()
        {
            InitializeComponent();
        }

        public override string GetDisplayName() => "Var Hack";

        public override void InitializeTab()
        {
            base.InitializeTab();

            buttonVarHackAddNewVariable.Click +=
                (sender, e) => varHackPanel.AddNewControl();

            ControlUtilities.AddContextMenuStripFunctions(
                buttonVarHackAddNewVariable,
                new List<string>()
                {
                    "RNG Index",
                    "Floor YNorm",
                    "Defacto Speed",
                    "Sliding Speed",
                    "Mario Action",
                    "Mario Animation",
                    "DYaw Intended - Facing",
                    "DYaw Intended - Facing (HAU)",
                },
                new List<Action>()
                {
                    () => AddVariable("RngIndex"),
                    () => AddVariable("FloorYNorm"),
                    () => AddVariable("DefactoSpeed"),
                    () => AddVariable("SlidingSpeed"),
                    () => AddVariable("MarioAction"),
                    () => AddVariable("MarioAnimation"),
                    () => AddVariable("DYawIntendFacing"),
                    () => AddVariable("DYawIntendFacingHau"),
                });

            buttonVarHackOpenVars.Click +=
                (sender, e) => varHackPanel.OpenVariables();

            buttonVarHackSaveVars.Click +=
                (sender, e) => varHackPanel.SaveVariables();

            buttonVarHackClearVars.Click +=
                (sender, e) => varHackPanel.ClearVariables();

            buttonVarHackShowVariableBytesInLittleEndian.Click +=
                (sender, e) => varHackPanel.ShowVariableBytesInLittleEndian();

            buttonVarHackShowVariableBytesInBigEndian.Click +=
                (sender, e) => varHackPanel.ShowVariableBytesInBigEndian();


            buttonVarHackApplyVariablesToMemory.Click +=
                (sender, e) => varHackPanel.ApplyVariablesToMemory();

            buttonVarHackClearVariablesInMemory.Click +=
                (sender, e) => varHackPanel.ClearVariablesInMemory();

            buttonEnableDisableRomHack.Click += (sender, e) => VarHackConfig.ShowVarRomHack.LoadPayload();

            ControlUtilities.AddContextMenuStripFunctions(
                buttonEnableDisableRomHack,
                new List<string>() { "1f Delay Hack (Standard)", "0f Delay Hack (Experimental)" },
                new List<Action>()
                {
                    () => VarHackConfig.ShowVarRomHack.LoadPayload(),
                    () => VarHackConfig.ShowVarRomHack2.LoadPayload(),
                });

            // Middle buttons

            textBoxXPosValue.AddEnterAction(() => SetPositionsAndApplyVariablesToMemory());
            textBoxXPosValue.Text = VarHackConfig.DefaultXPos.ToString();
            InitializePositionControls(
                textBoxXPosValue,
                textBoxXPosChange,
                buttonXPosSubtract,
                buttonXPosAdd
            );

            textBoxYPosValue.AddEnterAction(() => SetPositionsAndApplyVariablesToMemory());
            textBoxYPosValue.Text = VarHackConfig.DefaultYPos.ToString();
            InitializePositionControls(
                textBoxYPosValue,
                textBoxYPosChange,
                buttonYPosSubtract,
                buttonYPosAdd
            );

            textBoxYDeltaValue.AddEnterAction(() => SetPositionsAndApplyVariablesToMemory());
            textBoxYDeltaValue.Text = VarHackConfig.DefaultYDelta.ToString();
            InitializePositionControls(
                textBoxYDeltaValue,
                textBoxYDeltaChange,
                buttonYDeltaSubtract,
                buttonYDeltaAdd
            );

            buttonSetPositionsAndApplyVariablesToMemory.Click +=
                (sender, e) => SetPositionsAndApplyVariablesToMemory();
        }

        private void InitializePositionControls(
            TextBox valueTextbox,
            TextBox changeTextbox,
            Button subtractButton,
            Button addButton)
        {
            subtractButton.Click += (sender, e) =>
            {
                int? change = ParsingUtilities.ParseIntNullable(changeTextbox.Text);
                if (!change.HasValue) return;
                int? oldValue = ParsingUtilities.ParseIntNullable(valueTextbox.Text);
                if (!oldValue.HasValue) return;
                int newValue = oldValue.Value - change.Value;
                valueTextbox.Text = newValue.ToString();
                SetPositionsAndApplyVariablesToMemory();
            };

            addButton.Click += (sender, e) =>
            {
                int? change = ParsingUtilities.ParseIntNullable(changeTextbox.Text);
                if (!change.HasValue) return;
                int? oldValue = ParsingUtilities.ParseIntNullable(valueTextbox.Text);
                if (!oldValue.HasValue) return;
                int newValue = oldValue.Value + change.Value;
                valueTextbox.Text = newValue.ToString();
                SetPositionsAndApplyVariablesToMemory();
            };
        }

        private void SetPositionsAndApplyVariablesToMemory()
        {
            int? xPos = ParsingUtilities.ParseIntNullable(textBoxXPosValue.Text);
            int? yPos = ParsingUtilities.ParseIntNullable(textBoxYPosValue.Text);
            int? yDelta = ParsingUtilities.ParseIntNullable(textBoxYDeltaValue.Text);
            if (!xPos.HasValue || !yPos.HasValue || !yDelta.HasValue) return;
            varHackPanel.SetPositions(xPos.Value, yPos.Value, yDelta.Value);
            varHackPanel.ApplyVariablesToMemory();
        }

        public void AddVariable(string varName, uint address, Type memoryType, bool useHex, uint? pointerOffset)
        {
            varHackPanel.AddNewControl(varName, address, memoryType, useHex, pointerOffset);
        }

        public void AddVariable(string specialType)
        {
            varHackPanel.AddNewControl(specialType);
        }

        void IVariableAdder.AddVariable(WatchVariableControl control)
        {
            WatchVariableWrapper watchVarWrapper = control.WatchVarWrapper;
            WatchVariable watchVar = watchVarWrapper.WatchVar;

            if (watchVar.BaseAddressType == BaseAddressType.Triangle)
            {
                var trianglePointerAddress = AccessScope<StroopMainForm>.content.GetTab<TrianglesTab>().TrianglePointerAddress;
                if (trianglePointerAddress.HasValue)
                {
                    AddVariable(
                        control.VarName + " " + VarHackConfig.EscapeChar,
                        trianglePointerAddress.Value,
                        watchVar.MemoryType,
                        watchVarWrapper.DisplayAsHex(),
                        watchVar.Offset);
                }
            }
            else
            {
                List<uint> addressList = watchVar.GetAddressList(control.FixedAddressListGetter());
                for (int i = 0; i < addressList.Count; i++)
                {
                    string indexSuffix = addressList.Count > 1 ? (i + 1).ToString() : "";
                    AddVariable(
                        control.VarName + indexSuffix + " " + VarHackConfig.EscapeChar,
                        addressList[i],
                        watchVar.MemoryType,
                        watchVarWrapper.DisplayAsHex(),
                        null);
                }
            }
        }

        void IVariableAdder.AddVariables(List<WatchVariableControl> controls)
        {
            controls.ForEach(control => ((IVariableAdder)this).AddVariable(control));
        }

        public override void Update(bool updateView)
        {
            varHackPanel.UpdateControls();
        }
    }
}
