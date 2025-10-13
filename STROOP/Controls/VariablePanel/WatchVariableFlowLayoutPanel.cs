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
using STROOP.Variables.SM64MemoryLayout;
using STROOP.Variables.Utilities;
using STROOP.Variables.Views;

namespace STROOP.Controls.VariablePanel
{
    public partial class WatchVariablePanel : UserControl
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

        public override bool Focused => renderer.Focused;

        public delegate void CustomDraw(Graphics g, Rectangle rect);

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
            WatchVariableStringWrapper.specialTypeContextMenuHandlers.Add("WatchVarPanelFont", () =>
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

        public readonly Func<List<WatchVariableControl>> GetSelectedVars;
        public List<ToolStripItem> customContextMenuItems = new List<ToolStripItem>();

        public delegate IEnumerable<IVariableView> SpecialFuncWatchVariables(PositionAngle.HybridPositionAngle input);

        public Func<IEnumerable<(string name, SpecialFuncWatchVariables generateVariables)>> getSpecialFuncWatchVariables = null;

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

        public bool IsSelected => Focused;

        private List<WatchVariableControl> _allWatchVarControls;
        private SortedList<WatchVariableControl> _shownWatchVarControls;
        private List<WatchVariableControl> _hiddenSearchResults = new List<WatchVariableControl>();
        private List<string> _allGroups;
        private List<string> _initialVisibleGroups;
        private List<string> _visibleGroups;
        private List<ToolStripMenuItem> _filteringDropDownItems;

        private HashSet<WatchVariableControl> _selectedWatchVarControls;
        private List<WatchVariableControl> _reorderingWatchVarControls;

        ToolStripMenuItem filterVariablesItem = new ToolStripMenuItem("Filter Variables...");

        WatchVariablePanelRenderer renderer;

        public WatchVariableControl hoveringWatchVariableControl => renderer.GetVariableAt(renderer.PointToClient(System.Windows.Forms.Cursor.Position)).ctrl;

        public WatchVariablePanel()
        {
            GetSelectedVars = () => new List<WatchVariableControl>(_selectedWatchVarControls);

            _allWatchVarControls = new List<WatchVariableControl>();
            _allGroups = new List<string>();
            _initialVisibleGroups = new List<string>();
            _visibleGroups = new List<string>();

            _selectedWatchVarControls = new HashSet<WatchVariableControl>();
            _reorderingWatchVarControls = new List<WatchVariableControl>();

            renderer = new WatchVariablePanelRenderer(this);
            renderer.KeyDown += (_, args) =>
            {
                if (GlobalKeyboard.IsCtrlDown() && args.KeyCode == Keys.F)
                    (FindForm() as StroopMainForm)?.ShowSearchDialog();
            };
            getSpecialFuncWatchVariables = () => new[] { PositionAngle.HybridPositionAngle.GenerateBaseVariables };
            UpdateSortOption(WatchVariableControl.SortByPriority);
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

                List<IVariableView> precursors = _varFilePath == null
                    ? new List<IVariableView>()
                    : XmlConfigParser.OpenWatchVariableControlPrecursors(_varFilePath);

                foreach (var watchVarControl in precursors.ConvertAll(precursor => new WatchVariableControl(this, precursor)))
                    _allWatchVarControls.Add(watchVarControl);

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
                            selected.WatchVarWrapper.ShowVarInfo();
                        else if (lastClicked != -1)
                            selected.WatchVarWrapper.DoubleClick(renderer, renderer.GetVariableControlBounds(lastClicked));
                        break;
                    }
                };
                renderer.Click += (_, __) =>
                {
                    if (lastClicked != -1)
                        foreach (var selected in _selectedWatchVarControls)
                            selected.WatchVarWrapper.SingleClick(renderer, renderer.GetVariableControlBounds(lastClicked));
                };

                renderer.MouseDown += (_, __) =>
                {
                    renderer.Focus();
                    (int index, var var, var _select) = renderer.GetVariableAt(__.Location);
                    lastClicked = index;

                    bool ctrlHeld = GlobalKeyboard.IsCtrlDown();
                    bool shiftHeld = GlobalKeyboard.IsShiftDown();
                    clickedName = _select | shiftHeld;

                    if (_reorderingWatchVarControls.Count > 0)
                    {
                        if (__.Button == MouseButtons.Left)
                        {
                            var dings = new List<WatchVariableControl>();
                            foreach (var ctrl in _shownWatchVarControls)
                                if (!_reorderingWatchVarControls.Contains(ctrl))
                                    dings.Add(ctrl);
                            dings.AddRange(_reorderingWatchVarControls);
                            _shownWatchVarControls.Clear();
                            foreach (var ding in dings)
                                _shownWatchVarControls.Add(ding);
                            lastSelectedEntry = -1;
                        }

                        _reorderingWatchVarControls.Clear();
                        return;
                    }

                    var numSelected = _selectedWatchVarControls.Count;
                    if (!ctrlHeld && (numSelected == 1 || __.Button != MouseButtons.Right))
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
                                ctrl.IsSelected = true;
                            }

                            if (k >= high)
                                break;
                            k++;
                        }
                    }
                    else if (var != null)
                    {
                        if (ctrlHeld && var.IsSelected)
                        {
                            _selectedWatchVarControls.Remove(var);
                            var.IsSelected = false;
                        }
                        else
                        {
                            _selectedWatchVarControls.Add(var);
                            var.IsSelected = true;
                        }
                    }

                    if (__.Button == MouseButtons.Left)
                        OnVariableClick(_selectedWatchVarControls.ToList());

                    if (__.Button == MouseButtons.Right)
                    {
                        if (var != null)
                            var.ShowContextMenu();
                        else
                            ShowContextMenu();
                    }

                    if (!shiftHeld || _selectedWatchVarControls.Count == 0)
                        if (var != null && var.IsSelected)
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

        public void UpdateSortOption(WatchVariableControl.SortVariables newSortOption)
        {
            _shownWatchVarControls = new SortedList<WatchVariableControl>((a, b) => newSortOption(a, b));
            foreach (var shownVar in _allWatchVarControls.Where(_ => ShouldShow(_)))
                _shownWatchVarControls.Add(shownVar);
        }

        private void OnVariableClick(List<WatchVariableControl> watchVars)
        {
            if (watchVars.Count == 0)
                return;

            bool isCtrlKeyHeld = GlobalKeyboard.IsCtrlDown();
            bool isShiftKeyHeld = GlobalKeyboard.IsShiftDown();
            bool isAltKeyHeld = GlobalKeyboard.IsAltDown();
            bool isFKeyHeld = GlobalKeyboard.IsDown(Keys.F);
            bool isHKeyHeld = GlobalKeyboard.IsDown(Keys.H);
            bool isDKeyHeld = GlobalKeyboard.IsDown(Keys.D);
            bool isRKeyHeld = GlobalKeyboard.IsDown(Keys.R);
            bool isCKeyHeld = GlobalKeyboard.IsDown(Keys.C);
            bool isBKeyHeld = GlobalKeyboard.IsDown(Keys.B);
            bool isQKeyHeld = GlobalKeyboard.IsDown(Keys.Q);
            bool isOKeyHeld = GlobalKeyboard.IsDown(Keys.O);
            bool isMKeyHeld = GlobalKeyboard.IsDown(Keys.M);
            bool isNKeyHeld = GlobalKeyboard.IsDown(Keys.N);
            bool isPKeyHeld = GlobalKeyboard.IsDown(Keys.P);
            bool isXKeyHeld = GlobalKeyboard.IsDown(Keys.X);
            bool isSKeyHeld = GlobalKeyboard.IsDown(Keys.S);
            bool isDeletishKeyHeld = GlobalKeyboard.IsDeletishKeyDown();
            bool isBacktickHeld = GlobalKeyboard.IsDown(Keys.Oemtilde);
            bool isZHeld = GlobalKeyboard.IsDown(Keys.Z);
            bool isMinusHeld = GlobalKeyboard.IsDown(Keys.OemMinus);
            bool isPlusHeld = GlobalKeyboard.IsDown(Keys.Oemplus);
            bool isNumberHeld = GlobalKeyboard.IsNumberDown();

            if (isShiftKeyHeld && isNumberHeld)
            {
                UnselectAllVariables();
                watchVars.ForEach(watchVar => watchVar.BaseColor = ColorUtilities.GetColorForVariable());
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
                var memoryDescriptorView = watchVars.FirstOrDefault()?.view as NamedVariableCollection.IMemoryDescriptorView;
                if (memoryDescriptorView != null)
                {
                    UnselectAllVariables();
                    ViewInMemoryTab(memoryDescriptorView.describedMemoryState);
                }
            }
            else if (isFKeyHeld)
            {
                UnselectAllVariables();
                watchVars.ForEach(watchVar => watchVar.ToggleFixedAddress(null));
            }
            else if (isHKeyHeld)
            {
                UnselectAllVariables();
                watchVars.ForEach(watchVar => watchVar.ToggleHighlighted());
            }
            else if (isNumberHeld)
            {
                UnselectAllVariables();
                Color? color = ColorUtilities.GetColorForHighlight();
                watchVars.ForEach(watchVar => watchVar.ToggleHighlighted(color));
            }
            else if (isDKeyHeld)
            {
                UnselectAllVariables();
                watchVars.ForEach(watchVar => watchVar.WatchVarWrapper.ToggleDisplay());
            }
            else if (isCKeyHeld)
            {
                UnselectAllVariables();
                watchVars.Last().WatchVarWrapper.ShowControllerForm();
            }
            else if (isBKeyHeld)
            {
                UnselectAllVariables();
                watchVars.Last().WatchVarWrapper.ShowBitForm();
            }
            else if (isDeletishKeyHeld)
            {
                UnselectAllVariables();
                RemoveVariables(watchVars);
            }
            else if (isBacktickHeld)
            {
                UnselectAllVariables();
                AddToVarHackTab(watchVars);
            }
            else if (isZHeld)
            {
                UnselectAllVariables();
                watchVars.ForEach(watchVar => watchVar.SetValue(0));
            }
            else if (isXKeyHeld)
            {
                BeginMoveSelected();
            }
            else if (isQKeyHeld)
            {
                UnselectAllVariables();
                Color? newColor = ColorUtilities.GetColorFromDialog(watchVars.First().BaseColor);
                if (newColor.HasValue)
                {
                    watchVars.ForEach(watchVar => watchVar.BaseColor = newColor.Value);
                    ColorUtilities.LastCustomColor = newColor.Value;
                }
            }
            else if (isOKeyHeld)
            {
                UnselectAllVariables();
                watchVars.ForEach(watchVar => watchVar.BaseColor = ColorUtilities.LastCustomColor);
            }
        }

        void AddToVarHackTab(List<WatchVariableControl> watchVars)
        {
            foreach (var watchVar in watchVars)
                watchVar.FlashColor(WatchVariableControl.ADD_TO_VAR_HACK_TAB_COLOR);
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

        private static NamedVariableCollection.CustomView<T> CreateDummyVariable<T>() where T : struct, IConvertible
        {
            throw new NotImplementedException();
            // T capturedValue = default(T);
            //
            // return new NamedVariableCollection.CustomView<T>(WatchVariableUtilities.GetWrapperType(typeof(T)))
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
                        var view = (NamedVariableCollection.CustomView)typeof(WatchVariablePanel)
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
            WatchVariableSelectionUtilities.CreateSelectionToolStripItems(GetCurrentVariableControls(), this)
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
            foreach (var shownVar in _allWatchVarControls.Where(_ => ShouldShow(_)))
                _shownWatchVarControls.Add(shownVar);

            filterVariablesItem.DropDownItems.Clear();
            _filteringDropDownItems = _allGroups.ConvertAll(varGroup => CreateFilterItem(varGroup));
            UpdateFilterItemCheckedStatuses();
            _filteringDropDownItems.ForEach(item => filterVariablesItem.DropDownItems.Add(item));
        }

        public WatchVariableControl AddVariable(IVariableView view) =>
            AddVariables(new[] { view }).First();

        public IEnumerable<WatchVariableControl> AddVariables(IEnumerable<IVariableView> watchVars)
        {
            if (!initialized)
                DeferredInitialize();

            var lst = new List<WatchVariableControl>();
            foreach (var view in watchVars)
            {
                var newControl = new WatchVariableControl(this, view);
                lst.Add(newControl);
                _allWatchVarControls.Add(newControl);
                if (ShouldShow(newControl)) _shownWatchVarControls.Add(newControl);
            }

            return lst;
        }

        public void RemoveVariable(WatchVariableControl watchVarControl) =>
            RemoveVariables(new List<WatchVariableControl>() { watchVarControl });

        public void RemoveVariables(IEnumerable<WatchVariableControl> watchVarControls)
        {
            foreach (WatchVariableControl watchVarControl in watchVarControls)
            {
                _reorderingWatchVarControls.Remove(watchVarControl);
                _allWatchVarControls.Remove(watchVarControl);
                _shownWatchVarControls.Remove(watchVarControl);
            }
        }

        public void RemoveVariableGroup(string varGroup)
        {
            List<WatchVariableControl> watchVarControls =
                _allWatchVarControls.FindAll(
                    watchVarControl => watchVarControl.BelongsToGroup(varGroup));
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
            List<WatchVariableControl> watchVarControlListCopy =
                new List<WatchVariableControl>(_allWatchVarControls);
            RemoveVariables(watchVarControlListCopy);
        }

        public void ClearAllButHighlightedVariables()
        {
            List<WatchVariableControl> nonHighlighted =
                _allWatchVarControls.FindAll(control => !control.Highlighted);
            RemoveVariables(nonHighlighted);
            _allWatchVarControls.ForEach(control => control.Highlighted = false);
        }

        private void ResetVariables()
        {
            ClearVariables();
            _visibleGroups.Clear();
            _visibleGroups.AddRange(_initialVisibleGroups);
            UpdateFilterItemCheckedStatuses();

            List<IVariableView> views = _varFilePath == null
                ? new List<IVariableView>()
                : XmlConfigParser.OpenWatchVariableControlPrecursors(_varFilePath);
            AddVariables(views);
        }

        public void UnselectAllVariables()
        {
            foreach (var control in _selectedWatchVarControls)
                control.IsSelected = false;
            _selectedWatchVarControls.Clear();
        }

        private List<XElement> GetCurrentVarXmlElements(bool useCurrentState = true) =>
            GetCurrentVariableControls().ConvertAll(control => control.ToXml(useCurrentState));

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
            form.Initialize(elements.ConvertAndRemoveNull(WatchVariableWrapperFactory.ParseXml));
            form.ShowForm();
        }

        public void OpenVariables(List<XElement> elements)
        {
            AddVariables(elements.ConvertAll(WatchVariableWrapperFactory.ParseXml));
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

        public List<WatchVariableControl> GetCurrentVariableControls()
        {
            var lst = new List<WatchVariableControl>(_shownWatchVarControls);
            lst.AddRange(_hiddenSearchResults);
            return lst;
        }

        public IEnumerable<MemoryDescriptor> GetCurrentVariablePrecursors()
            => GetCurrentVariableControls().ConvertAndRemoveNull(control => (control.view as NamedVariableCollection.IMemoryDescriptorView)?.memoryDescriptor);

        public List<string> GetCurrentVariableValues() =>
            GetCurrentVariableControls().ConvertAll(control => control.WatchVarWrapper.GetValueText());

        public List<string> GetCurrentVariableNames() => GetCurrentVariableControls().ConvertAll(control => control.VarName);

        public bool SetVariableValueByName<T>(string name, T value) where T : IConvertible
        {
            WatchVariableControl control = GetCurrentVariableControls().FirstOrDefault(c => c.VarName == name);
            if (control == null) return false;
            return control.SetValue(value);
        }

        public IVariableView[] GetWatchVariablesByName(params string[] names) =>
            GetWatchVariableControlsByName(names).Select(x => x?.view ?? null).ToArray();

        public WatchVariableControl[] GetWatchVariableControlsByName(params string[] names)
        {
            var result = new WatchVariableControl[names.Length];
            foreach (var var in _allWatchVarControls)
            {
                var index = Array.IndexOf(names, var.view.Name);
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
            var shownVars = new HashSet<WatchVariableControl>(_shownWatchVarControls);
            var removeLater = new HashSet<WatchVariableControl>();
            foreach (var v in _allWatchVarControls)
            {
                if (!shownVars.Contains(v))
                {
                    if (ShouldShow(v))
                        _shownWatchVarControls.Add(v);
                    else if (searchForm != null && searchForm.searchHidden && searchForm.IsMatch(v.VarName))
                        _hiddenSearchResults.Add(v);
                }
                else if (!ShouldShow(v))
                    removeLater.Add(v);
            }

            foreach (var toBeRemoved in removeLater)
                _shownWatchVarControls.Remove(toBeRemoved);
            GetCurrentVariableControls().ForEach(watchVarControl => watchVarControl.UpdateControl());
            renderer.Draw();
        }

        private bool ShouldShow(WatchVariableControl watchVarControl)
        {
            if (!hasGroupsSet || watchVarControl.alwaysVisible)
                return true;
            return watchVarControl.BelongsToAnyGroupOrHasNoGroup(_visibleGroups);
        }

        public override string ToString()
        {
            List<string> varNames = _allWatchVarControls.ConvertAll(control => control.VarName);
            return String.Join(",", varNames);
        }

        public void ColorVarsUsingFunction(Func<WatchVariableControl, Color> getColor)
        {
            foreach (WatchVariableControl control in _allWatchVarControls)
                control.BaseColor = getColor(control);
        }

        public int GetAutoHeight(int numColumns = 1) =>
            (_shownWatchVarControls.Count + numColumns - 1) / numColumns * renderer.elementHeight + renderer.borderMargin * 2;
    }
}
