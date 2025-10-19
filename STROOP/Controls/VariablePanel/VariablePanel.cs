using STROOP.Controls.VariablePanel.Cells;
using STROOP.Core;
using STROOP.Core.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class VariablePanel : UserControl, IVariablePanel<VariablePanelUiContext>
    {
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

        [InitializeSpecial]
        static void InitializeSpecial()
        {
            var target = WatchVariableSpecialUtilities.dictionary;
            target.Add("WatchVarPanelNameWidth", () => SavedSettingsConfig.WatchVarPanelNameWidth.value, (uint value) =>
            {
                SavedSettingsConfig.WatchVarPanelNameWidth.value = Math.Max(1, value);
                return true;
            });
            target.Add("WatchVarPanelValueWidth", () => SavedSettingsConfig.WatchVarPanelValueWidth.value, (uint value) =>
            {
                SavedSettingsConfig.WatchVarPanelValueWidth.value = Math.Max(1, value);
                return true;
            });
            target.Add("WatchVarPanelXMargin", () => SavedSettingsConfig.WatchVarPanelHorizontalMargin.value, (uint value) =>
            {
                SavedSettingsConfig.WatchVarPanelHorizontalMargin.value = (uint)Math.Max(1, value);
                return true;
            });
            target.Add("WatchVarPanelYMargin", () => SavedSettingsConfig.WatchVarPanelVerticalMargin.value, (uint value) =>
            {
                SavedSettingsConfig.WatchVarPanelVerticalMargin.value = (uint)Math.Max(1, value);
                return true;
            });
            target.Add("WatchVarPanelBoldNames", () => SavedSettingsConfig.WatchVarPanelBoldNames.value, (bool value) =>
            {
                SavedSettingsConfig.WatchVarPanelBoldNames.value = value;
                return true;
            });
            target.Add("WatchVarPanelFont", () => SavedSettingsConfig.WatchVarPanelFontOverride.value?.Name ?? "(default)", (string value) => false);
            VariableStringCell.specialTypeContextMenuHandlers.Add("WatchVarPanelFont", () =>
            {
                var dlg = new FontDialog();
                if (SavedSettingsConfig.WatchVarPanelFontOverride.value != null)
                    dlg.Font = SavedSettingsConfig.WatchVarPanelFontOverride;
                try
                {
                    if (dlg.ShowDialog() == DialogResult.OK)
                        SavedSettingsConfig.WatchVarPanelFontOverride.value = dlg.Font;
                }
                catch (ArgumentException ex)
                {
                    // Apparently ACCEPTING a FontDialog can throw if the selected Font is not a TrueType-Font.
                    MessageBox.Show($"This font is not supported.\nHere's a scary error report:\n\n{ex.Message}");
                }
            });
        }

        public override bool Focused => renderer.Focused;
        public List<ToolStripItem> customContextMenuItems = new List<ToolStripItem>();

        public bool initialized = false;

        List<Action> deferredActions = new List<Action>();

        private string _varFilePath;
        string _dataPath;

        [Category("Data"), Browsable(true)]
        public string DataPath
        {
            get { return _dataPath; }
            set { Initialize(_dataPath = value); }
        }

        [Category("Layout"), Browsable(true)] public int? elementNameWidth { get; set; } = null;
        [Category("Layout"), Browsable(true)] public int? elementValueWidth { get; set; } = null;

        public readonly Func<List<IWinFormsVariableCell>> GetSelectedVars;

        public delegate IEnumerable<IVariable> SpecialFuncWatchVariables(PositionAngle.HybridPositionAngle input);

        public Func<IEnumerable<(string name, SpecialFuncWatchVariables generateVariables)>> getSpecialFuncWatchVariables = null;
        public bool IsSelected => Focused;

        private List<IWinFormsVariableCell> _allWatchVarControls;
        private SortedList<IWinFormsVariableCell> _shownWatchVarControls;
        private List<IWinFormsVariableCell> _hiddenSearchResults = [];
        private HashSet<IWinFormsVariableCell> _selectedWatchVarControls;
        private List<IWinFormsVariableCell> _reorderingWatchVarControls;

        private List<string> _allGroups;
        private List<string> _initialVisibleGroups;
        private List<string> _visibleGroups;
        private List<ToolStripMenuItem> _filteringDropDownItems;

        ToolStripMenuItem filterVariablesItem = new ToolStripMenuItem("Filter Variables...");

        WatchVariablePanelRenderer renderer;

        public IWinFormsVariableCell HoveringWinFormsVariableCellControl =>
            renderer.GetVariableAt(renderer.PointToClient(Cursor.Position)).cell;

        public VariablePanel()
        {
            GetSelectedVars = () => new List<IWinFormsVariableCell>(_selectedWatchVarControls);

            _allWatchVarControls = new List<IWinFormsVariableCell>();
            _allGroups = new List<string>();
            _initialVisibleGroups = new List<string>();
            _visibleGroups = new List<string>();

            _selectedWatchVarControls = new HashSet<IWinFormsVariableCell>();
            _reorderingWatchVarControls = new List<IWinFormsVariableCell>();

            renderer = new WatchVariablePanelRenderer(this);
            renderer.KeyDown += (_, args) =>
            {
                if (GlobalKeyboard.IsCtrlDown() && args.KeyCode == Keys.F)
                    (FindForm() as StroopMainForm)?.ShowSearchDialog();
            };
            getSpecialFuncWatchVariables = () => new[] { PositionAngle.HybridPositionAngle.GenerateBaseVariables };
            UpdateSortOption(WinFormsVariableControl.SortByPriority);
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
            renderer.Draw();
        }

        bool hasGroupsSet = false;

        public void SetGroups(
            List<string> allVariableGroupsNullable,
            List<string> visibleVariableGroupsNullable)
        {
            if (Program.IsVisualStudioHostProcess()) return;

            hasGroupsSet = true;
            DeferActionToUpdate(nameof(SetGroups), () =>
            {
                _allGroups = allVariableGroupsNullable != null ? new List<string>(allVariableGroupsNullable) : new List<string>();

                _visibleGroups = visibleVariableGroupsNullable != null ? new List<string>(visibleVariableGroupsNullable) : new List<string>();

                _initialVisibleGroups.AddRange(_visibleGroups);
                UpdateControlsBasedOnFilters();
            });
        }

        public void Initialize(string varFilePath = null)
        {
            AutoScroll = true;

            Controls.Add(renderer);
            _varFilePath = varFilePath;
            if (varFilePath != null && !System.IO.File.Exists(varFilePath))
                return;
            DeferActionToUpdate(nameof(Initialize), () =>
            {
                SuspendLayout();

                List<IVariable> precursors = _varFilePath == null
                    ? new List<IVariable>()
                    : XmlConfigParser.OpenWatchVariableControlPrecursors(_varFilePath);

                foreach (var watchVarControl in precursors.ConvertAll(precursor => new WinFormsVariableControl(this, precursor)))
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
                            selected.DoubleClick(new VariablePanelUiContext(
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
                            selected.SingleClick(new VariablePanelUiContext(
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

        void ShowVarContextMenu()
        {
            ContextMenuStrip ctx = new ContextMenuStrip();

            var uniqueSettings = GetSelectedVars().SelectMany(x => x.control.AvailableSettings()).ToHashSet();
            var sortedOptions = uniqueSettings.ToList();
            sortedOptions.Sort((a, b) => string.Compare(a.Name, b.Name));
            foreach (var setting in sortedOptions)
                setting.CreateContextMenuEntry(ctx.Items, GetSelectedVars);

            ctx.Items.Add(new ToolStripSeparator());
            foreach (var item in WatchVariableSelectionUtilities.CreateSelectionToolStripItems(GetSelectedVars(), this))
                ctx.Items.Add(item);

            ctx.Show(Cursor.Position);
        }

        public void UpdateSortOption(WinFormsVariableControl.SortVariables newSortOption)
        {
            _shownWatchVarControls = new SortedList<IWinFormsVariableCell>((a, b) => newSortOption(a, b));
            foreach (var shownVar in _allWatchVarControls.Where(ShouldShow))
                _shownWatchVarControls.Add(shownVar);
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

        void AddToVarHackTab(List<IWinFormsVariableCell> cells)
        {
            foreach (var cell in cells)
                cell.control.FlashColor(WinFormsVariableControl.ADD_TO_VAR_HACK_TAB_COLOR);
            MessageBox.Show("This feature is currently not implemented :(");
        }

        public void DeferredInitialize()
        {
            foreach (var action in deferredActions)
                action.Invoke();
            deferredActions.Clear();
        }

        private void DeferActionToUpdate(string name, Action action)
        {
            deferredActions.Add(action);
        }

        private static int numDummies = 0;

        private static CustomVariable CreateDummyVariable<T>() where T : struct, IConvertible
        {
            throw new NotImplementedException();
            // T capturedValue = default(T);
            //
            // return new CustomVariableView<T>(WatchVariableUtilities.GetWrapperType(typeof(T)))
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

        private void ShowContextMenu()
        {
            ToolStripMenuItem resetVariablesItem = new ToolStripMenuItem("Reset Variables");
            resetVariablesItem.Click += (sender, e) => ResetVariables();

            ToolStripMenuItem clearAllButHighlightedItem = new ToolStripMenuItem("Clear All But Highlighted");
            clearAllButHighlightedItem.Click += (sender, e) => ClearAllButHighlightedVariables();

            ToolStripMenuItem addCustomVariablesItem = new ToolStripMenuItem("Add Custom Variables");
            addCustomVariablesItem.Click += (sender, e) =>
            {
                VariableCreationForm form = new VariableCreationForm();
                form.Initialize(this);
                form.Show();
            };

            ToolStripMenuItem addDummyVariableItem = new ToolStripMenuItem("Add Dummy Variable...");
            foreach (string typeString in TypeUtilities.InGameTypeList)
            {
                ToolStripMenuItem typeItem = new ToolStripMenuItem(typeString);
                addDummyVariableItem.DropDownItems.Add(typeItem);
                typeItem.Click += (sender, e) =>
                {
                    int numEntries = 1;
                    if (GlobalKeyboard.IsCtrlDown())
                    {
                        string numEntriesString = DialogUtilities.GetStringFromDialog(labelText: "Enter Num Vars:");
                        if (numEntriesString == null) return;
                        int parsed = ParsingUtilities.ParseInt(numEntriesString);
                        parsed = Math.Max(parsed, 0);
                        numEntries = parsed;
                    }

                    for (int i = 0; i < numEntries; i++)
                    {
                        Type type = TypeUtilities.StringToType[typeString];
                        var view = (CustomVariable)typeof(VariablePanel)
                            .GetMethod(nameof(CreateDummyVariable), BindingFlags.NonPublic | BindingFlags.Static)
                            .MakeGenericMethod(type)
                            .Invoke(null, Array.Empty<object>());
                        AddVariable(view);
                    }
                };
            }

            ToolStripMenuItem addRelativeVariablesItem = null, removePointVariableItem = null;
            var getSpecialFuncVars = getSpecialFuncWatchVariables?.Invoke() ?? null;
            var specificsCount = getSpecialFuncVars?.Count() ?? 0;
            if (PositionAngle.HybridPositionAngle.pointPAs.Count > 0)
            {
                if (getSpecialFuncVars != null && specificsCount > 0)
                {
                    void BindHandler(ToolStripMenuItem menuItem, PositionAngle.HybridPositionAngle targetPA, SpecialFuncWatchVariables generator) =>
                        menuItem.Click += (_, __) => AddVariables(generator(targetPA));

                    addRelativeVariablesItem = new ToolStripMenuItem("Add relative variables for...");
                    if (specificsCount == 1)
                        addRelativeVariablesItem.Text = $"Add {getSpecialFuncVars.First().name} for...";
                    foreach (var pa in PositionAngle.HybridPositionAngle.pointPAs)
                    {
                        var paItem = new ToolStripMenuItem(pa.name);
                        if (specificsCount == 1)
                            BindHandler(paItem, pa, getSpecialFuncVars.First().generateVariables);
                        else
                            foreach (var specialFunc in getSpecialFuncVars)
                            {
                                var specificsItem = new ToolStripMenuItem(specialFunc.name);
                                BindHandler(specificsItem, pa, specialFunc.generateVariables);
                                paItem.DropDownItems.Add(specificsItem);
                            }

                        addRelativeVariablesItem.DropDownItems.Add(paItem);
                    }
                }

                removePointVariableItem = new ToolStripMenuItem("Remove custom point ...");
                foreach (var customPA in PositionAngle.HybridPositionAngle.pointPAs)
                {
                    var capture = customPA;
                    var subElement = new ToolStripMenuItem(customPA.name);
                    subElement.Click += (_, __) =>
                    {
                        capture.first = () => PositionAngle.NaN;
                        capture.second = () => PositionAngle.NaN;
                        capture.OnDelete();
                        PositionAngle.HybridPositionAngle.pointPAs.Remove(capture);
                    };
                    removePointVariableItem.DropDownItems.Add(subElement);
                }
            }

            var addPointVariableItem = new ToolStripMenuItem("Add custom point...");
            addPointVariableItem.Click += (_, __) =>
            {
                var ptCount = 1;
                while (PositionAngle.HybridPositionAngle.pointPAs.Any(pa => pa.name.ToLower() == $"point{ptCount}"))
                    ptCount++;
                var newName = DialogUtilities.GetStringFromDialog($"Point{ptCount}", "Enter name of new custom point", "Add custom point");
                if (newName?.Trim() != null)
                    PositionAngle.HybridPositionAngle.pointPAs.Add(
                        new PositionAngle.HybridPositionAngle(() => PositionAngle.Mario, () => PositionAngle.Mario, newName));
            };

            ToolStripMenuItem openSaveClearItem = new ToolStripMenuItem("Open / Save / Clear ...");
            ControlUtilities.AddDropDownItems(
                openSaveClearItem,
                new List<string>() { "Restore", "Open", "Open as Pop Out", "Save in Place", "Save As", "Clear" },
                new List<Action>()
                {
                    () => OpenVariables(DialogUtilities.OpenXmlElements(FileType.StroopVariables, _dataPath)),
                    () => OpenVariables(),
                    () => OpenVariablesAsPopOut(),
                    () => SaveVariablesInPlace(),
                    () => SaveVariables(),
                    () => ClearVariables(),
                });

            ToolStripMenuItem doToAllVariablesItem = new ToolStripMenuItem("Do to all variables...");
            WatchVariableSelectionUtilities.CreateSelectionToolStripItems(GetCurrentlyVisibleCells(), this)
                .ForEach(item => doToAllVariablesItem.DropDownItems.Add(item));

            filterVariablesItem.DropDown.MouseEnter += (sender, e) => { filterVariablesItem.DropDown.AutoClose = false; };
            filterVariablesItem.DropDown.MouseLeave += (sender, e) =>
            {
                filterVariablesItem.DropDown.AutoClose = true;
                filterVariablesItem.DropDown.Close();
            };

            ToolStripItem searchVariablesItem = new ToolStripMenuItem("Search variables...");
            searchVariablesItem.Click += (_, __) => (FindForm() as StroopMainForm)?.ShowSearchDialog();

            var strip = new ContextMenuStrip();
            strip.Items.Add(resetVariablesItem);
            strip.Items.Add(clearAllButHighlightedItem);
            strip.Items.Add(new ToolStripSeparator());
            if (addRelativeVariablesItem != null)
                strip.Items.Add(addRelativeVariablesItem);
            strip.Items.Add(addPointVariableItem);
            strip.Items.Add(removePointVariableItem);
            strip.Items.Add(addCustomVariablesItem);
            strip.Items.Add(addDummyVariableItem);
            strip.Items.Add(new ToolStripSeparator());
            strip.Items.Add(openSaveClearItem);
            strip.Items.Add(doToAllVariablesItem);
            strip.Items.Add(filterVariablesItem);
            strip.Items.Add(searchVariablesItem);
            if (customContextMenuItems.Count > 0)
            {
                strip.Items.Add(new ToolStripSeparator());
                foreach (var item in customContextMenuItems)
                    strip.Items.Add(item);
            }

            strip.Show(System.Windows.Forms.Cursor.Position);
        }

        private ToolStripMenuItem CreateFilterItem(string varGroup)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(varGroup.ToString());
            item.Click += (sender, e) => ToggleVarGroupVisibility(varGroup);
            return item;
        }

        public void BeginMoveSelected()
        {
            _reorderingWatchVarControls.Clear();
            _reorderingWatchVarControls.AddRange(_selectedWatchVarControls.Where(v => !_hiddenSearchResults.Contains(v)));
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

        public IWinFormsVariableCell AddVariable(IVariable view) =>
            AddVariables([ view ]).First();

        public IEnumerable<IWinFormsVariableCell> AddVariables(IEnumerable<IVariable> views)
            => AddVariablesInternal(views.Select(view =>  new WinFormsVariableControl(this, view).varCell));

        public IEnumerable<IWinFormsVariableCell> AddVariables(IEnumerable<IWinFormsVariableCell> cells)
            => AddVariablesInternal(cells.Select(cell => cell.control.CreateCopy(this).varCell));

        IEnumerable<IWinFormsVariableCell> AddVariablesInternal(IEnumerable<IWinFormsVariableCell> cells)
        {
            if (!initialized)
                DeferredInitialize();

            var lst = new List<IWinFormsVariableCell>();
            foreach (var cell in cells)
            {
                lst.Add(cell);
                _allWatchVarControls.Add(cell);
                if (ShouldShow(cell))
                    _shownWatchVarControls.Add(cell);
            }

            return lst;
        }

        public void RemoveVariable(IWinFormsVariableCell varCellControl) =>
            RemoveVariables([varCellControl]);

        public void RemoveVariables(IEnumerable<IWinFormsVariableCell> watchVarControls)
        {
            foreach (IWinFormsVariableCell watchVarControl in watchVarControls)
            {
                _reorderingWatchVarControls.Remove(watchVarControl);
                _allWatchVarControls.Remove(watchVarControl);
                _shownWatchVarControls.Remove(watchVarControl);
            }
        }

        public void RemoveVariableGroup(string varGroup)
        {
            List<IWinFormsVariableCell> watchVarControls =
                _allWatchVarControls.FindAll(
                    watchVarControl => watchVarControl.control.BelongsToGroup(varGroup));
            RemoveVariables(watchVarControls);
        }

        public void ShowOnlyVariableGroup(string visibleVarGroup) => ShowOnlyVariableGroups(new List<string>() { visibleVarGroup });

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

            List<IVariable> views = _varFilePath == null
                ? new List<IVariable>()
                : XmlConfigParser.OpenWatchVariableControlPrecursors(_varFilePath);
            AddVariables(views);
        }

        public void UnselectAllVariables()
        {
            foreach (var cell in _selectedWatchVarControls)
                cell.control.IsSelected = false;
            _selectedWatchVarControls.Clear();
        }

        private List<XElement> GetCurrentVarXmlElements(bool useCurrentState = true) =>
            GetCurrentlyVisibleCells().ConvertAll(cell => cell.control.ToXml(useCurrentState));

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
            form.Initialize(elements.ConvertAndRemoveNull(x => VariableCellFactory<VariablePanelUiContext>.ParseXml(x, WatchVariableSpecialUtilities.dictionary)));
            form.ShowForm();
        }

        public void OpenVariables(List<XElement> elements)
        {
            AddVariables(elements.ConvertAll(x => VariableCellFactory<VariablePanelUiContext>.ParseXml(x, WatchVariableSpecialUtilities.dictionary)));
        }

        public void SaveVariablesInPlace()
        {
            if (_varFilePath == null) return;
            if (!DialogUtilities.AskQuestionAboutSavingVariableFileInPlace()) return;
            SaveVariables(_varFilePath);
        }

        public void SaveVariables(string fileName = null)
        {
            DialogUtilities.SaveXmlElements(FileType.StroopVariables, "VarData", GetCurrentVarXmlElements(), fileName);
        }

        public List<IWinFormsVariableCell> GetCurrentlyVisibleCells()
            => [.._shownWatchVarControls, .._hiddenSearchResults];

        public IEnumerable<MemoryDescriptor> GetCurrentVariablePrecursors()
            => GetCurrentlyVisibleCells().ConvertAndRemoveNull(control => control.memory?.descriptor);

        public List<string> GetCurrentVariableValues() =>
            GetCurrentlyVisibleCells().ConvertAll(cell => cell.GetValueText());

        public List<string> GetCurrentVariableNames() => GetCurrentlyVisibleCells().ConvertAll(cell => cell.control.VarName);

        public bool SetVariableValueByName<T>(string name, T value) where T : IConvertible
        {
            var cellControl = GetCurrentlyVisibleCells().FirstOrDefault(c => c.control.VarName == name);
            if (cellControl == null)
                return false;
            return cellControl.control.SetValue(value);
        }

        public IWinFormsVariableCell[] GetWinFormsVariableControlsByName(params string[] names)
        {
            var result = new IWinFormsVariableCell[names.Length];
            foreach (var var in _allWatchVarControls)
            {
                var index = Array.IndexOf(names, var.control.VarName);
                if (index != -1)
                    result[index] = var;
            }

            return result;
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

        private bool ShouldShow(IWinFormsVariableCell cell)
        {
            if (!hasGroupsSet || cell.control.alwaysVisible)
                return true;
            return cell.control.BelongsToAnyGroupOrHasNoGroup(_visibleGroups);
        }

        public override string ToString()
        {
            List<string> varNames = _allWatchVarControls.ConvertAll(cell => cell.control.VarName);
            return String.Join(",", varNames);
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


        public void ColorVarsUsingFunction(Func<WinFormsVariableControl, Color> getColor)
        {
            foreach (WinFormsVariableControl control in _allWatchVarControls)
                control.BaseColor = getColor(control);
        }

        public int GetAutoHeight(int numColumns = 1) =>
            (_shownWatchVarControls.Count + numColumns - 1) / numColumns * renderer.elementHeight + renderer.borderMargin * 2;
    }
}
