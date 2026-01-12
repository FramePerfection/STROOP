using STROOP.Core;
using STROOP.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Linq;
using STROOP.Forms;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using STROOP.Variables;
using STROOP.Variables.Formatting;
using STROOP.Variables.Utilities;
using STROOP.Variables.VariablePanel;

namespace STROOP.Controls.VariablePanel
{
    public partial class VariablePanel : UserControl
    {
        public delegate IEnumerable<VariablePrecursor> SpecialFuncVariables(PositionAngle.HybridPositionAngle input);

        static void ViewInMemoryTab(DescribedMemoryState memoryDescriptor)
        {
            List<uint> addressList = memoryDescriptor.GetAddressList().ToList();
            if (addressList.Count == 0) return;
            uint address = addressList[0];
            var tab = AccessScope<StroopMainForm>.content.GetTab<Tabs.MemoryTab>();
            tab.UpdateOrInitialize(true);
            Config.TabControlMain.SelectedTab = tab.Tab;
            tab.SetCustomAddress(address);
            tab.UpdateHexDisplay();
        }

        private static int numDummies = 0;

        public readonly Func<List<IWinFormsVariableCell>> GetSelectedVars;

        public Func<IEnumerable<(string name, SpecialFuncVariables generateVariables)>> getSpecialFuncVariables = null;

        bool initialized = false;
        List<Action> deferredActions = new List<Action>();

        private List<IWinFormsVariableCell> _allWatchVarControls;
        private List<IWinFormsVariableCell> _hiddenSearchResults = [];
        private List<IWinFormsVariableCell> _reorderingWatchVarControls;
        private HashSet<IWinFormsVariableCell> _selectedWatchVarControls;
        private SortedList<IWinFormsVariableCell> _shownWatchVarControls;

        private List<string> _allGroups;
        private List<string> _initialVisibleGroups;
        private List<string> _visibleGroups;
        private List<ToolStripMenuItem> _filteringDropDownItems;

        private ToolStripMenuItem filterVariablesItem = new ToolStripMenuItem("Filter Variables...");
        private Renderer renderer;
        private bool hasGroupsSet = false;

        public VariablePanel()
        {
            GetSelectedVars = () => new List<IWinFormsVariableCell>(_selectedWatchVarControls);

            _allWatchVarControls = new List<IWinFormsVariableCell>();
            _allGroups = new List<string>();
            _initialVisibleGroups = new List<string>();
            _visibleGroups = new List<string>();

            _selectedWatchVarControls = new HashSet<IWinFormsVariableCell>();
            _reorderingWatchVarControls = new List<IWinFormsVariableCell>();

            renderer = new Renderer(this);
            renderer.KeyDown += (_, args) =>
            {
                if (GlobalKeyboard.IsCtrlDown() && args.KeyCode == Keys.F)
                    (FindForm() as StroopMainForm)?.ShowSearchDialog();
            };
            getSpecialFuncVariables = () => [ PositionAngle.HybridPositionAngle.GenerateBaseVariables ];
            UpdateSortOption(WinFormsVariableControl.SortByPriority);
        }

        public void Initialize(string varFilePath = null)
        {
            AutoScroll = true;

            Controls.Add(renderer);
            _varFilePath = varFilePath;
            if (varFilePath != null && !System.IO.File.Exists(varFilePath))
                return;
            deferredActions.Add(() =>
            {
                SuspendLayout();

                var controls = (_varFilePath != null ? XmlConfigParser.OpenVariableControlPrecursors(_varFilePath) : [])
                    .Select(precursor => new WinFormsVariableControl(this, precursor.var) { VarName = precursor.name });
                foreach (var watchVarControl in controls)
                    _allWatchVarControls.Add(watchVarControl.varCell);

                MouseDown += (_, __) =>
                {
                    if (__.Button == MouseButtons.Right)
                        ShowContextMenu();
                };

                int lastSelectedEntry = -1;
                int lastClicked = -1;
                bool clickedName = false;
                renderer.DoubleClick += (_, __) =>
                {
                    foreach (var selected in _selectedWatchVarControls)
                    {
                        if (clickedName)
                            ShowVarInfo(selected);
                        else if (lastClicked != -1)
                            selected.DoubleClick(new WinFormsVariablePanelUiContext(
                                renderer,
                                null,
                                renderer.GetVariableControlBounds(lastClicked))
                            );
                        break;
                    }
                };
                renderer.Click += (_, __) =>
                {
                    if (lastClicked != -1)
                        foreach (var selected in _selectedWatchVarControls)
                            selected.SingleClick(new WinFormsVariablePanelUiContext(
                                renderer,
                                null,
                                renderer.GetVariableControlBounds(lastClicked))
                            );
                };

                renderer.MouseDown += (_, args) =>
                {
                    renderer.Focus();
                    (int index, IWinFormsVariableCell cell, bool select) = renderer.GetVariableAt(args.Location);
                    lastClicked = index;

                    bool ctrlHeld = GlobalKeyboard.IsCtrlDown();
                    bool shiftHeld = GlobalKeyboard.IsShiftDown();
                    clickedName = select | shiftHeld;

                    if (_reorderingWatchVarControls.Count > 0)
                    {
                        if (args.Button == MouseButtons.Left)
                        {
                            var tmp = new List<IWinFormsVariableCell>();
                            foreach (var ctrl in _shownWatchVarControls)
                                if (!_reorderingWatchVarControls.Contains(ctrl))
                                    tmp.Add(ctrl);
                            tmp.AddRange(_reorderingWatchVarControls);
                            _shownWatchVarControls.Clear();
                            foreach (var x in tmp)
                                _shownWatchVarControls.Add(x);
                            lastSelectedEntry = -1;
                        }

                        _reorderingWatchVarControls.Clear();
                        return;
                    }

                    var numSelected = _selectedWatchVarControls.Count;
                    if (!ctrlHeld && (numSelected == 1 || args.Button != MouseButtons.Right))
                        UnselectAllVariables();

                    if (shiftHeld)
                    {
                        int k = 0;
                        var low = Math.Min(lastSelectedEntry, index);
                        var high = Math.Max(lastSelectedEntry, index);
                        foreach (var ctrl in _shownWatchVarControls)
                        {
                            if (k >= low)
                            {
                                _selectedWatchVarControls.Add(ctrl);
                                ctrl.control.IsSelected = true;
                            }

                            if (k >= high)
                                break;
                            k++;
                        }
                    }
                    else if (cell != null)
                    {
                        if (ctrlHeld && cell.control.IsSelected)
                        {
                            _selectedWatchVarControls.Remove(cell);
                            cell.control.IsSelected = false;
                        }
                        else
                        {
                            _selectedWatchVarControls.Add(cell);
                            cell.control.IsSelected = true;
                        }
                    }

                    if (args.Button == MouseButtons.Left)
                        OnVariableClick(_selectedWatchVarControls.ToList());

                    if (args.Button == MouseButtons.Right)
                    {
                        if (cell != null)
                            ShowVarContextMenu();
                        else
                            ShowContextMenu();
                    }

                    if (!shiftHeld || _selectedWatchVarControls.Count == 0)
                        if (cell != null && cell.control.IsSelected)
                            lastSelectedEntry = index;
                };

                _allGroups.AddRange(new List<string>(new[] { VariableGroup.Custom }));
                _initialVisibleGroups.AddRange(new List<string>(new[] { VariableGroup.Custom }));
                _visibleGroups.AddRange(new List<string>(new[] { VariableGroup.Custom }));
                if (!hasGroupsSet)
                    UpdateControlsBasedOnFilters();

                ResumeLayout();
            });
        }

        public void DeferredInitialize()
        {
            foreach (var action in deferredActions)
                action.Invoke();
            deferredActions.Clear();
        }

        public void UpdatePanel()
        {
            if (SavedSettingsConfig.WatchVarPanelFontOverride.value != null)
                Font = SavedSettingsConfig.WatchVarPanelFontOverride;
            else if (Font != SystemFonts.DefaultFont)
                Font = SystemFonts.DefaultFont;

            var searchForm = (FindForm() as StroopMainForm)?.searchVariableDialog ?? null;
            _hiddenSearchResults.Clear();
            var shownVars = new HashSet<IWinFormsVariableCell>(_shownWatchVarControls);
            var removeLater = new HashSet<IWinFormsVariableCell>();
            foreach (var v in _allWatchVarControls)
            {
                if (!shownVars.Contains(v))
                {
                    if (ShouldShow(v))
                        _shownWatchVarControls.Add(v);
                    else if (searchForm != null && searchForm.searchHidden && searchForm.IsMatch(v.control.VarName))
                        _hiddenSearchResults.Add(v);
                }
                else if (!ShouldShow(v))
                    removeLater.Add(v);
            }

            foreach (var toBeRemoved in removeLater)
                _shownWatchVarControls.Remove(toBeRemoved);
            GetCurrentlyVisibleCells().ForEach(cell => cell.Update());
            renderer.Draw();
        }

        public void SetGroups(
            List<string> allVariableGroupsNullable,
            List<string> visibleVariableGroupsNullable
        )
        {
            if (Program.IsVisualStudioHostProcess()) return;

            hasGroupsSet = true;
            deferredActions.Add(() =>
            {
                _allGroups = allVariableGroupsNullable != null ? new List<string>(allVariableGroupsNullable) : new List<string>();

                _visibleGroups = visibleVariableGroupsNullable != null ? new List<string>(visibleVariableGroupsNullable) : new List<string>();

                _initialVisibleGroups.AddRange(_visibleGroups);
                UpdateControlsBasedOnFilters();
            });
        }

        public void UpdateSortOption(WinFormsVariableControl.SortVariables newSortOption)
        {
            _shownWatchVarControls = new SortedList<IWinFormsVariableCell>((a, b) => newSortOption(a, b));
            foreach (var shownVar in _allWatchVarControls.Where(ShouldShow))
                _shownWatchVarControls.Add(shownVar);
        }

        public void BeginMoveSelected()
        {
            _reorderingWatchVarControls.Clear();
            _reorderingWatchVarControls.AddRange(_selectedWatchVarControls.Where(v => !_hiddenSearchResults.Contains(v)));
        }

        private void OnVariableClick(List<IWinFormsVariableCell> cells)
        {
            if (cells.Count == 0)
                return;

            bool isShiftKeyHeld = GlobalKeyboard.IsShiftDown();
            bool isFKeyHeld = GlobalKeyboard.IsDown(Keys.F);
            bool isHKeyHeld = GlobalKeyboard.IsDown(Keys.H);
            bool isCKeyHeld = GlobalKeyboard.IsDown(Keys.C);
            bool isBKeyHeld = GlobalKeyboard.IsDown(Keys.B);
            bool isQKeyHeld = GlobalKeyboard.IsDown(Keys.Q);
            bool isOKeyHeld = GlobalKeyboard.IsDown(Keys.O);
            bool isNKeyHeld = GlobalKeyboard.IsDown(Keys.N);
            bool isXKeyHeld = GlobalKeyboard.IsDown(Keys.X);
            bool isDeletishKeyHeld = GlobalKeyboard.IsDeletishKeyDown();
            bool isBacktickHeld = GlobalKeyboard.IsDown(Keys.Oemtilde);
            bool isZHeld = GlobalKeyboard.IsDown(Keys.Z);
            bool isNumberHeld = GlobalKeyboard.IsNumberDown();

            if (isShiftKeyHeld && isNumberHeld)
            {
                UnselectAllVariables();
                cells.ForEach(cell => cell.control.BaseColor = ColorUtilities.GetColorForVariable(GlobalKeyboard.GetCurrentlyInputtedNumber()));
            }
            //else if (isSKeyHeld)
            //{
            //    containingPanel.UnselectAllVariables();
            //    AddToTab(Config.CustomManager);
            //}
            //else if (isMKeyHeld)
            //{
            //    containingPanel.UnselectAllVariables();
            //    AddToTab(Config.MemoryManager);
            //}
            else if (isNKeyHeld)
            {
                var memory = cells.FirstOrDefault()?.memory;
                if (memory != null)
                {
                    UnselectAllVariables();
                    ViewInMemoryTab(memory);
                }
            }
            else if (isFKeyHeld)
            {
                UnselectAllVariables();
                cells.ForEach(watchVar => watchVar.control.ToggleFixedAddress(null));
            }
            else if (isHKeyHeld)
            {
                UnselectAllVariables();
                cells.ForEach(watchVar => watchVar.control.ToggleHighlighted());
            }
            else if (isNumberHeld)
            {
                UnselectAllVariables();
                Color? color = ColorUtilities.GetColorForHighlight(GlobalKeyboard.GetCurrentlyInputtedNumber());
                cells.ForEach(watchVar => watchVar.control.ToggleHighlighted(color));
            }
            else if (isCKeyHeld)
            {
                UnselectAllVariables();
                var cell = cells.Last();
                new VariableControllerForm(cell.control.VarName, cell.control.varCell).Show();
            }
            else if (isBKeyHeld)
            {
                var cell = cells.Last();
                if (cell.memory != null)
                {
                    UnselectAllVariables();
                    new VariableBitForm(cell.control.VarName, cell.memory.descriptor, true).Show();
                }
            }
            else if (isDeletishKeyHeld)
            {
                UnselectAllVariables();
                RemoveVariables(cells);
            }
            else if (isBacktickHeld)
            {
                UnselectAllVariables();
                AddToVarHackTab(cells);
            }
            else if (isZHeld)
            {
                UnselectAllVariables();
                cells.ForEach(cell => cell.control.SetValue(0));
            }
            else if (isXKeyHeld)
            {
                BeginMoveSelected();
            }
            else if (isQKeyHeld)
            {
                UnselectAllVariables();
                Color? newColor = ColorDialogUtilities.GetColorFromDialog(cells.First().control.BaseColor);
                if (newColor.HasValue)
                {
                    cells.ForEach(cell => cell.control.BaseColor = newColor.Value);
                    ColorUtilities.LastCustomColor = newColor.Value;
                }
            }
            else if (isOKeyHeld)
            {
                UnselectAllVariables();
                cells.ForEach(cell => cell.control.BaseColor = ColorUtilities.LastCustomColor);
            }
        }

        private void AddToVarHackTab(List<IWinFormsVariableCell> cells)
        {
            foreach (var cell in cells)
                cell.control.FlashColor(WinFormsVariableControl.ADD_TO_VAR_HACK_TAB_COLOR);
            MessageBox.Show("This feature is currently not implemented :(");
        }

        private static CustomVariable CreateDummyVariable<T>() where T : struct, IConvertible
        {
            throw new NotImplementedException();
            // T capturedValue = default(T);
            //
            // return new CustomVariableView<T>(VariableUtilities.GetWrapperType(typeof(T)))
            // {
            //     Name = $"Dummy {++numDummies} {StringUtilities.Capitalize(typeof(T).Name)}",
            //     _getterFunction = () => capturedValue.Yield(),
            //     _setterFunction = (T value) =>
            //     {
            //         capturedValue = value;
            //         return true.Yield();
            //     }
            // };
        }

        private ToolStripMenuItem CreateFilterItem(string varGroup)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(varGroup);
            item.Click += (sender, e) => ToggleVarGroupVisibility(varGroup);
            return item;
        }

        private void ToggleVarGroupVisibility(string varGroup, bool? newVisibilityNullable = null)
        {
            // Toggle visibility if no visibility is provided
            bool newVisibility = newVisibilityNullable ?? !_visibleGroups.Contains(varGroup);
            if (newVisibility) // change to visible
                _visibleGroups.Add(varGroup);
            else // change to hidden
                _visibleGroups.Remove(varGroup);
            UpdateControlsBasedOnFilters();
            UpdateFilterItemCheckedStatuses();
        }

        private void UpdateFilterItemCheckedStatuses()
        {
            if (_allGroups.Count != _filteringDropDownItems.Count) throw new ArgumentOutOfRangeException();

            for (int i = 0; i < _allGroups.Count; i++)
                _filteringDropDownItems[i].Checked = _visibleGroups.Contains(_allGroups[i]);
        }

        private void UpdateControlsBasedOnFilters()
        {
            _shownWatchVarControls.Clear();
            foreach (var shownVar in _allWatchVarControls.Where(ShouldShow))
                _shownWatchVarControls.Add(shownVar);

            filterVariablesItem.DropDownItems.Clear();
            _filteringDropDownItems = _allGroups.ConvertAll(varGroup => CreateFilterItem(varGroup));
            UpdateFilterItemCheckedStatuses();
            _filteringDropDownItems.ForEach(item => filterVariablesItem.DropDownItems.Add(item));
        }

        public void RemoveVariable(IWinFormsVariableCell varCellControl) =>
            RemoveVariables([varCellControl]);

        public void RemoveVariableGroup(string varGroup)
        {
            List<IWinFormsVariableCell> watchVarControls =
                _allWatchVarControls.FindAll(
                    watchVarControl => watchVarControl.control.BelongsToGroup(varGroup));
            RemoveVariables(watchVarControls);
        }

        public void ShowOnlyVariableGroups(List<string> visibleVarGroups)
        {
            foreach (string varGroup in _allGroups)
            {
                bool newVisibility = visibleVarGroups.Contains(varGroup);
                ToggleVarGroupVisibility(varGroup, newVisibility);
            }
        }

        public void ClearVariables()
        {
            List<IWinFormsVariableCell> watchVarControlListCopy =
                new List<IWinFormsVariableCell>(_allWatchVarControls);
            RemoveVariables(watchVarControlListCopy);
        }

        public void ClearAllButHighlightedVariables()
        {
            List<IWinFormsVariableCell> nonHighlighted = _allWatchVarControls.FindAll(cell => !cell.control.Highlighted);
            RemoveVariables(nonHighlighted);
            _allWatchVarControls.ForEach(cell => cell.control.Highlighted = false);
        }

        private void ResetVariables()
        {
            ClearVariables();
            _visibleGroups.Clear();
            _visibleGroups.AddRange(_initialVisibleGroups);
            UpdateFilterItemCheckedStatuses();

            this.AddVariables(_varFilePath != null ? XmlConfigParser.OpenVariableControlPrecursors(_varFilePath) : []);
        }

        public void UnselectAllVariables()
        {
            foreach (var cell in _selectedWatchVarControls)
                cell.control.IsSelected = false;
            _selectedWatchVarControls.Clear();
        }

        public void OpenVariables()
        {
            List<XElement> elements = DialogUtilities.OpenXmlElements(FileType.StroopVariables);
            OpenVariables(elements);
        }

        public void OpenVariablesAsPopOut()
        {
            List<XElement> elements = DialogUtilities.OpenXmlElements(FileType.StroopVariables);
            if (elements.Count == 0) return;
            VariablePopOutForm form = new VariablePopOutForm();
            form.Initialize(elements
                .Select(x => VariableCellFactory<WinFormsVariablePanelUiContext>.ParseXml(x, VariableSpecialDictionary.Instance))
                .Where(x => x.var != null)
            );
            form.ShowForm();
        }

        public void OpenVariables(List<XElement> elements)
            => this.AddVariables(elements.ConvertAll(x => VariableCellFactory<WinFormsVariablePanelUiContext>.ParseXml(x, VariableSpecialDictionary.Instance)));

        public void SaveVariablesInPlace()
        {
            if (_varFilePath == null) return;
            if (!DialogUtilities.AskQuestionAboutSavingVariableFileInPlace()) return;
            SaveVariables(_varFilePath);
        }

        public void SaveVariables(string fileName = null)
            => DialogUtilities.SaveXmlElements(
                FileType.StroopVariables,
                "VarData",
                GetCurrentlyVisibleCells().ConvertAll(cell => cell.control.ToXml()),
                fileName
            );

        public List<IWinFormsVariableCell> GetCurrentlyVisibleCells()
            => [.._shownWatchVarControls, .._hiddenSearchResults];

        public IEnumerable<MemoryDescriptor> GetCurrentVariableMemoryDescriptors()
            => GetCurrentlyVisibleCells().ConvertAndRemoveNull(control => control.memory?.descriptor);

        public List<string> GetCurrentVariableValues()
            => GetCurrentlyVisibleCells().ConvertAll(cell => cell.GetValueText());

        public List<string> GetCurrentVariableNames()
            => GetCurrentlyVisibleCells().ConvertAll(cell => cell.control.VarName);

        private bool ShouldShow(IWinFormsVariableCell cell)
        {
            if (!hasGroupsSet || cell.control.alwaysVisible)
                return true;
            return cell.control.BelongsToAnyGroupOrHasNoGroup(_visibleGroups);
        }

        private void ShowVarInfo(IWinFormsVariableCell cell)
        {
            var memoryDescriptor = cell.memory?.descriptor;
            VariableViewerForm varInfo =
                new VariableViewerForm(
                    name: cell.control.VarName,
                    clazz: cell.GetClass(),
                    type: memoryDescriptor?.GetTypeDescription() ?? "special",
                    baseTypeOffset: memoryDescriptor?.GetBaseTypeOffsetDescription() ?? "<none>",
                    n64BaseAddress: memoryDescriptor?.GetBaseAddressListString() ?? "<none>",
                    emulatorBaseAddress: memoryDescriptor?.GetProcessAddressListString() ?? "<none>",
                    n64Address: memoryDescriptor?.GetRamAddressListString(true) ?? "<none>",
                    emulatorAddress: memoryDescriptor?.GetProcessAddressListString() ?? "<none>");
            varInfo.Show();
        }

        public override string ToString()
        {
            List<string> varNames = _allWatchVarControls.ConvertAll(cell => cell.control.VarName);
            return String.Join(",", varNames);
        }
    }
}
