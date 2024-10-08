﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using STROOP.Utilities;
using STROOP.Structs;
using STROOP.Managers;
using STROOP.Extensions;
using STROOP.Structs.Configurations;
using STROOP.Forms;
using STROOP.Models;
using STROOP.Core.Variables;

namespace STROOP
{
    public partial class StroopMainForm : Form
    {
        // STROOP VERSION NAME
        const string _version = "Refactor 0.6.3";

        public event Action Updating;

        DataTable _tableOtherData = new DataTable();
        Dictionary<int, DataRow> _otherDataRowAssoc = new Dictionary<int, DataRow>();
        Dictionary<Type, Tabs.STROOPTab> tabsByType = new Dictionary<Type, Tabs.STROOPTab>();
        public ObjectSlotsManager ObjectSlotsManager;

        bool _objSlotResizing = false;
        int _resizeObjSlotTime = 0;
        readonly bool isMainForm;
        List<Process> _availableProcesses = new List<Process>();
            
        public readonly SearchVariableDialog searchVariableDialog;
        
        public StroopMainForm(bool isMainForm)
        {
            this.searchVariableDialog = new SearchVariableDialog(this);
            this.isMainForm = isMainForm;
            InitializeComponent();
            InitTabs();
            ObjectSlotsManager = new ObjectSlotsManager(this, tabControlMain);
            GetTab<Tabs.OptionsTab>().AddCogContextMenu(pictureBoxCog);
        }

        public void ShowSearchDialog()
        {
            if (!searchVariableDialog.Visible)
            {
                searchVariableDialog.StartPosition = FormStartPosition.Manual;
                searchVariableDialog.Location = PointToScreen(new Point(150, 150));
            }
            searchVariableDialog.Show();
            searchVariableDialog.Activate();
        }

        private bool AttachToProcess(Process process)
        {
            if (process.HasExited)
            {
                return false;
            }
            
            // Find emulator
            var emulators = Config.Emulators.Where(e => e.ProcessName.ToLower() == process.ProcessName.ToLower()).ToList();

            if (emulators.Count > 1)
            {
                MessageBox.Show("Ambiguous emulator type", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return Config.Stream.SwitchProcess(process, emulators[0]);
        }

        private void InitTabs()
        {
            SavedSettingsConfig._allTabs.Clear();
            tabControlMain.TabPages.Clear();
            foreach (var t in typeof(Tabs.STROOPTab).Assembly.GetTypes())
            {
                if (t.IsSubclassOf(typeof(Tabs.STROOPTab)) && !t.IsAbstract && !t.IsGenericType)
                {
                    var ctor = t.GetConstructor(new Type[0]);
                    if (ctor != null)
                    {
                        var tabControl = (Tabs.STROOPTab)ctor.Invoke(new object[0]);
                        var newTab = new TabPage(tabControl.GetDisplayName());
                        newTab.Controls.Add(tabControl);
                        tabControl.Bounds = newTab.ClientRectangle;
                        tabControl.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
                        tabsByType[t] = tabControl;
                        tabControlMain.TabPages.Add(newTab);
                        SavedSettingsConfig._allTabs.Add(newTab);
                    }
                }
            }
        }

        private string GetDisplayNameForProcess(Process process)
        {
            if (string.IsNullOrWhiteSpace(process.MainWindowTitle))
            {
                return $"{process.ProcessName} ({process.Id})";
            }
            
            return process.MainWindowTitle;
        }
        
        private void StroopMainForm_Load(object sender, EventArgs e)
        {
            Config.Stream.OnDisconnect += _sm64Stream_OnDisconnect;
            Config.Stream.WarnReadonlyOff += _sm64Stream_WarnReadonlyOff;

            comboBoxRomVersion.DataSource = Enum.GetValues(typeof(RomVersionSelection));
            comboBoxReadWriteMode.DataSource = Enum.GetValues(typeof(ReadWriteMode));

            SetUpContextMenuStrips();

            Config.TabControlMain = tabControlMain;
            Config.DebugText = labelDebugText;

            SavedSettingsConfig.InvokeInitiallySavedTabOrder();
            Config.TabControlMain.SelectedIndex = 0;
            InitializeTabRemoval();
            SavedSettingsConfig.InvokeInitiallySavedRemovedTabs();

            labelVersionNumber.Text = _version;

            // Collect garbage, we are fully loaded now!
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Load process
            buttonRefresh_Click(this, new EventArgs());
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            BringToFront();
            Activate();
            using (new AccessScope<StroopMainForm>(this))
                Config.Stream.Run();
        }

        private void InitializeTabRemoval()
        {
            tabControlMain.Click += (se, ev) =>
            {
                if (KeyboardUtilities.IsCtrlHeld())
                {
                    SavedSettingsConfig.RemoveTab(tabControlMain.SelectedTab);
                }
            };

            buttonTabAdd.ContextMenuStrip = new ContextMenuStrip();
            Action openingFunction = () =>
            {
                buttonTabAdd.ContextMenuStrip.Items.Clear();
                SavedSettingsConfig.GetRemovedTabItems().ForEach(
                    item => buttonTabAdd.ContextMenuStrip.Items.Add(item));
            };
            buttonTabAdd.ContextMenuStrip.Opening += (se, ev) => openingFunction();
            openingFunction();
        }

        private void SetUpContextMenuStrips()
        {
            ControlUtilities.AddContextMenuStripFunctions(
                labelVersionNumber,
                new List<string>()
                {
                    "Open Mapping",
                    "Clear Mapping",
                    "Inject Hitbox View Code",
                    "Free Movement Action",
                    "Everything in File",
                    "Go to Closest Floor Vertex",
                    "Save as Savestate",
                    "Show MHS Vars",
                    "Download Latest STROOP Release",
                    "Documentation",
                    "Show All Helpful Hints",
                    "Show Image Form",
                    "Show Coin Ring Display Form",
                    "Format Subtitles",
                },
                new List<Action>()
                {
                    () => MappingConfig.OpenMapping(),
                    () => MappingConfig.ClearMapping(),
                    () => GetTab<Tabs.GfxTab.GfxTab>().InjectHitboxViewCode(),
                    () => Config.Stream.SetValue(MarioConfig.FreeMovementAction, MarioConfig.StructAddress + MarioConfig.ActionOffset),
                    () => GetTab<Tabs.FileTab>().DoEverything(),
                    () => GetTab<Tabs.TrianglesTab>().GoToClosestVertex(),
                    () => saveAsSavestate(),
                    () =>
                    {
                        string varFilePath = @"Config/MhsData.xml";
                        List<NamedVariableCollection.IView> precursors = XmlConfigParser.OpenWatchVariableControlPrecursors(varFilePath);
                        VariablePopOutForm form = new VariablePopOutForm();
                        form.Initialize(precursors);
                        form.ShowForm();
                    },
                    () => Process.Start("https://github.com/SM64-TAS-ABC/STROOP/releases/download/vDev/STROOP.zip"),
                    () => Process.Start("https://ukikipedia.net/wiki/STROOP"),
                    () => HelpfulHintUtilities.ShowAllHelpfulHints(),
                    () =>
                    {
                        ImageForm imageForm = new ImageForm();
                        imageForm.Show();
                    },
                    () =>
                    {
                        CoinRingDisplayForm form = new CoinRingDisplayForm();
                        form.Show();
                    },
                    () => SubtitleUtilities.FormatSubtitlesFromClipboard(),
                });

            ControlUtilities.AddCheckableContextMenuStripFunctions(
                labelVersionNumber,
                new List<string>()
                {
                    "Update Cam Hack Angle",
                    "Update Floor Tri",
                },
                new List<Func<bool>>()
                {
                    () =>
                    {
                        TestingConfig.UpdateCamHackAngle = !TestingConfig.UpdateCamHackAngle;
                        return TestingConfig.UpdateCamHackAngle;
                    },
                    () =>
                    {
                        TestingConfig.UpdateFloorTri = !TestingConfig.UpdateFloorTri;
                        return TestingConfig.UpdateFloorTri;
                    },
                });

            ControlUtilities.AddContextMenuStripFunctions(
                buttonMoveTabLeft,
                new List<string>() { "Restore Recommended Tab Order" },
                new List<Action>() { () => SavedSettingsConfig.InvokeRecommendedTabOrder() });

            ControlUtilities.AddContextMenuStripFunctions(
                buttonMoveTabRight,
                new List<string>() { "Restore Recommended Tab Order" },
                new List<Action>() { () => SavedSettingsConfig.InvokeRecommendedTabOrder() });

            ControlUtilities.AddContextMenuStripFunctions(
                trackBarObjSlotSize,
                new List<string>() { "Reset to Default Object Slot Size" },
                new List<Action>() {
                    () =>
                    {
                        trackBarObjSlotSize.Value = ObjectSlotsManager.DefaultSlotSize;
                        ChangeObjectSlotSize(ObjectSlotsManager.DefaultSlotSize);
                    }
                });
        }

        private void _sm64Stream_WarnReadonlyOff(object sender, EventArgs e)
        {
            this.TryInvoke(new Action(() =>
                {
                    var dr = MessageBox.Show("Warning! Editing variables and enabling hacks may cause the emulator to freeze. Turn off read-only mode?",
                        "Turn Off Read-only Mode?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                    switch (dr)
                    {
                        case DialogResult.Yes:
                            Config.Stream.Readonly = false;
                            Config.Stream.ShowWarning = false;
                            break;

                        case DialogResult.No:
                            Config.Stream.ShowWarning = false;
                            break;

                        case DialogResult.Cancel:
                            break;
                    }
                }));
        }

        private void _sm64Stream_OnDisconnect(object sender, EventArgs e)
        {
            this.BeginInvoke(new Action(() =>
            {
                buttonRefresh_Click(this, new EventArgs());
            }));
        }

        private List<Process> GetAvailableProcesses()
        {
            var AvailableProcesses = Process.GetProcesses();
            List<Process> resortList = new List<Process>();
            foreach (Process p in AvailableProcesses)
            {
                try
                {
                    if (!Config.Emulators.Any(e => e.ProcessName.ToLower() == p.ProcessName.ToLower()))
                        continue;

                    if (p.HasExited)
                        continue;
                }
                catch (Win32Exception) // Access is denied
                {
                    continue;
                }

                resortList.Add(p);
            }
            return resortList;
        }

        public void OnUpdate()
        {
            using (new AccessScope<StroopMainForm>(this))
            {
                labelFpsCounter.Text = "FPS: " + (int?)Config.Stream?.FpsInPractice ?? "<none>";
                if (Config.Stream != null)
                {
                    UpdateGlobalConfig();
                    DataModels.Update();
                }
                FormManager.Update();
                ObjectSlotsManager.Update();
                //Config.InjectionManager.Update();

                foreach (TabPage page in tabControlMain.TabPages)
                    Tabs.STROOPTab.UpdateTab(page, tabControlMain.SelectedTab == page);

                WatchVariableLockManager.Update();
                TriangleDataModel.ClearCache();
                Updating?.Invoke();
            }
        }

        private void UpdateGlobalConfig()
        {
            // Rom Version
            RomVersionConfig.UpdateRomVersion(comboBoxRomVersion);

            // Readonly / Read+Write
            Config.Stream.Readonly = (ReadWriteMode)comboBoxReadWriteMode.SelectedItem == ReadWriteMode.ReadOnly;
        }

        private void buttonShowTopPanel_Click(object sender, EventArgs e)
        {
            SplitContainer splitContainer =
                ControlUtilities.GetDescendantSplitContainer(
                    splitContainerMain, Orientation.Horizontal);
            if (splitContainer == null) return;
            splitContainer.Panel1Collapsed = false;
            splitContainer.Panel2Collapsed = true;
        }

        private void buttonShowBottomPanel_Click(object sender, EventArgs e)
        {
            SplitContainer splitContainer =
                ControlUtilities.GetDescendantSplitContainer(
                    splitContainerMain, Orientation.Horizontal);
            if (splitContainer == null) return;
            splitContainer.Panel1Collapsed = true;
            splitContainer.Panel2Collapsed = false;
        }

        private void buttonShowTopBottomPanel_Click(object sender, EventArgs e)
        {
            SplitContainer splitContainer =
                ControlUtilities.GetDescendantSplitContainer(
                    splitContainerMain, Orientation.Horizontal);
            if (splitContainer == null) return;
            splitContainer.Panel1Collapsed = false;
            splitContainer.Panel2Collapsed = false;
        }

        private void buttonShowLeftPanel_Click(object sender, EventArgs e)
        {
            SplitContainer splitContainer =
                ControlUtilities.GetDescendantSplitContainer(
                    splitContainerMain, Orientation.Vertical);
            if (splitContainer == null) return;
            splitContainer.Panel1Collapsed = false;
            splitContainer.Panel2Collapsed = true;
        }

        private void buttonShowRightPanel_Click(object sender, EventArgs e)
        {
            SplitContainer splitContainer =
                ControlUtilities.GetDescendantSplitContainer(
                    splitContainerMain, Orientation.Vertical);
            if (splitContainer == null) return;
            splitContainer.Panel1Collapsed = true;
            splitContainer.Panel2Collapsed = false;
        }

        private void buttonShowLeftRightPanel_Click(object sender, EventArgs e)
        {
            SplitContainer splitContainer =
                ControlUtilities.GetDescendantSplitContainer(
                    splitContainerMain, Orientation.Vertical);
            if (splitContainer == null) return;
            splitContainer.Panel1Collapsed = false;
            splitContainer.Panel2Collapsed = false;
        }

        private void buttonMoveTabLeft_Click(object sender, EventArgs e)
        {
            if (KeyboardUtilities.IsCtrlHeld() || KeyboardUtilities.IsNumberHeld())
            {
                ObjectOrderingUtilities.Move(false);
            }
            else
            {
                MoveTab(false);
            }
        }

        private void buttonMoveTabRight_Click(object sender, EventArgs e)
        {
            if (KeyboardUtilities.IsCtrlHeld() || KeyboardUtilities.IsNumberHeld())
            {
                ObjectOrderingUtilities.Move(true);
            }
            else
            {
                MoveTab(true);
            }
        }

        private void MoveTab(bool rightwards)
        {
            TabPage currentTab = tabControlMain.SelectedTab;
            int currentIndex = tabControlMain.TabPages.IndexOf(currentTab);
            int indexDiff = rightwards ? +1 : -1;
            int newIndex = currentIndex + indexDiff;
            if (newIndex < 0 || newIndex >= tabControlMain.TabCount) return;

            TabPage adjacentTab = tabControlMain.TabPages[newIndex];
            tabControlMain.TabPages.Remove(adjacentTab);
            tabControlMain.TabPages.Insert(currentIndex, adjacentTab);

            SavedSettingsConfig.Save();
        }

        private void buttonTabAdd_Click(object sender, EventArgs e)
        {
            buttonTabAdd.ContextMenuStrip.Show(Cursor.Position);
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            Process selectedProcess;

            if (listBoxProcessesList.Items.Count == 0)
            {
                return;
            }
            
            // If there is no selection, we automatically choose to the first one
            if (listBoxProcessesList.SelectedIndex == -1)
            {
                selectedProcess = _availableProcesses[0];
            }
            else
            {
                selectedProcess = _availableProcesses[listBoxProcessesList.SelectedIndex];
            }

            if (selectedProcess is null || !AttachToProcess(selectedProcess))
            {
                MessageBox.Show("Could not attach to process!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            panelConnect.Visible = false;
            labelProcessSelect.Text = $"Connected To: {GetDisplayNameForProcess(selectedProcess)}";
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            // Update the process list
            listBoxProcessesList.Items.Clear();
            _availableProcesses = GetAvailableProcesses().OrderBy(p => p.StartTime).ToList();
            foreach (var process in _availableProcesses)
                listBoxProcessesList.Items.Add(GetDisplayNameForProcess(process));

            // Pre-select the first process
            if (listBoxProcessesList.Items.Count != 0)
                listBoxProcessesList.SelectedIndex = 0;
        }

        private void buttonBypass_Click(object sender, EventArgs e)
        {
            Config.Stream.SwitchIO(null);
            labelProcessSelect.Text = "Not connected.";
            panelConnect.Visible = false;
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            Task.Run(() => Config.Stream.SwitchProcess(null, null));
            buttonRefresh_Click(this, new EventArgs());
            panelConnect.Visible = true;
        }

        private void buttonRefreshAndConnect_Click(object sender, EventArgs e)
        {
            buttonRefresh_Click(sender, e);
            buttonConnect_Click(sender, e);
        }
        
        private void listBoxProcessesList_DoubleClick(object sender, EventArgs e)
        {
            buttonConnect_Click(sender, e);
        }

        private void trackBarObjSlotSize_ValueChanged(object sender, EventArgs e)
        {
            ChangeObjectSlotSize(trackBarObjSlotSize.Value);
        }

        private async void ChangeObjectSlotSize(int size)
        {
            _resizeObjSlotTime = 100;
            if (_objSlotResizing)
                return;

            _objSlotResizing = true;

            await Task.Run(() =>
            {
                while (_resizeObjSlotTime > 0)
                {
                    Task.Delay(100).Wait();
                    _resizeObjSlotTime -= 100;
                }
            });

            WatchVariablePanelObjects.SuspendLayout();
            ObjectSlotsManager.ChangeSlotSize(size);
            WatchVariablePanelObjects.ResumeLayout();
            _objSlotResizing = false;
        }

        private void buttonOpenSavestate_Click(object sender, EventArgs e)
        {
            if (openFileDialogSt.ShowDialog() != DialogResult.OK)
                return;
            if (openFileDialogSt.CheckFileExists == true)
            {
                try
                {
                    Config.Stream.OpenSTFile(openFileDialogSt.FileName);
                }
                catch
                {
                    MessageBox.Show("Savestate is corrupted, not a savestate, or doesn't exist", "Invalid Savestate", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            labelProcessSelect.Text = "Connected To: " + Config.Stream.ProcessName;
            panelConnect.Visible = false;
        }

        private void saveAsSavestate()
        {
            StFileIO io = Config.Stream.IO as StFileIO;
            if (io == null)
            {
                MessageBox.Show("The current connection is not an ST file. Open a savestate file to save the savestate.", "Connection not a savestate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            saveFileDialogSt.FileName = io.Name;
            DialogResult dr = saveFileDialogSt.ShowDialog();
            if (dr != DialogResult.OK)
                return;

            io.SaveMemory(saveFileDialogSt.FileName);
        }

        public void SwitchTab(Tabs.STROOPTab stroopTab)
        {
            if (!tabControlMain.TabPages.Contains(stroopTab.Tab))
                SavedSettingsConfig.AddTab(stroopTab.Tab);
            tabControlMain.SelectedTab = stroopTab.Tab;
        }

        public T GetTab<T>() where T : Tabs.STROOPTab => (T)tabsByType[typeof(T)];
        public IEnumerable<Tabs.STROOPTab> EnumerateTabs()
        {
            foreach (var t in tabsByType)
                yield return t.Value;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (Config.Stream != null)
            {
                Config.Stream.OnDisconnect -= _sm64Stream_OnDisconnect;
                Config.Stream.WarnReadonlyOff -= _sm64Stream_WarnReadonlyOff;
            }
            if (isMainForm)
            {
                if (Config.Stream != null)
                {
                    Config.Stream.Dispose();
                    Config.Stream = null;
                }
            }
        }
    }
}
