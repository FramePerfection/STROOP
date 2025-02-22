using STROOP.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Linq;
using System;

namespace STROOP.Structs.Configurations
{
    public static class SavedSettingsConfig
    {
        public class SavedVariable<T>
        {
            T _value;
            public readonly T defaultValue;
            public readonly string name;
            public T value
            {
                get => _value;
                set
                {
                    _value = value;
                    if (_isLoaded)
                        Save();
                }
            }

            public SavedVariable(string name, T defaultValue)
            {
                this.name = name;
                this.defaultValue = defaultValue;
                this._value = defaultValue;
            }
            public static implicit operator T(SavedVariable<T> var) => var.value;
            public void Reset() => value = defaultValue;
        }

        static Dictionary<string, FieldInfo> savedFieldsByName = GetFieldsByNames();
        static Dictionary<string, FieldInfo> GetFieldsByNames()
        {
            var result = new Dictionary<string, FieldInfo>();
            foreach (var savedVariable in typeof(SavedSettingsConfig).GetFields(BindingFlags.Public | BindingFlags.Static))
                if (savedVariable.FieldType.IsGenericType && savedVariable.FieldType.GetGenericTypeDefinition() == typeof(SavedVariable<>))
                    result[savedVariable.Name] = savedVariable;
            return result;
        }
        static Dictionary<Type, (Func<string, object> parse, Func<object, string> tostring)> typeParsers = new Dictionary<Type, (Func<string, object>, Func<object, string>)>()
        {
            [typeof(bool)] = (_ => bool.Parse(_), _ => (Convert.ToBoolean(_)).ToString()),
            [typeof(uint)] = (_ => uint.Parse(_), _ => (Convert.ToUInt32(_)).ToString()),
            [typeof(System.Drawing.Font)] = (_ => FontSerializationHelper.FromString(_), _ => FontSerializationHelper.ToString((System.Drawing.Font)_))
        };

        static bool _isLoaded = false;

        public static SavedVariable<bool> DisplayYawAnglesAsUnsigned = new SavedVariable<bool>("Display Yaw Angles As Unsigned", true);
        public static SavedVariable<bool> VariableValuesFlushRight = new SavedVariable<bool>("Variable Values Flush Right", true);
        public static SavedVariable<bool> StartSlotIndexsFromOne = new SavedVariable<bool>("Start Slot Index From 1", true);
        public static SavedVariable<bool> OffsetGotoRetrieveFunctions = new SavedVariable<bool>("Offset Goto/Retrieve Functions", true);
        public static SavedVariable<bool> MoveCameraWithPu = new SavedVariable<bool>("PU Controller Moves Camera", true);
        public static SavedVariable<bool> ScaleDiagonalPositionControllerButtons = new SavedVariable<bool>("Scale Diagonal Position Controller Buttons", true);
        public static SavedVariable<bool> ExcludeDustForClosestObject = new SavedVariable<bool>("Exclude Dut for Closest Object", true);
        public static SavedVariable<bool> UseMisalignmentOffsetForDistanceToLine = new SavedVariable<bool>("Use Misalignment Offset For Distance To Line", true);
        public static SavedVariable<bool> DontRoundValuesToZero = new SavedVariable<bool>("Don't Round Values to 0", false);
        public static SavedVariable<bool> NeutralizeTrianglesWith0x15 = new SavedVariable<bool>("Neutralize Triangles with 0x15", true);

        public static short NeutralizeTriangleValue(bool? use0x15Nullable = null)
        {
            bool use0x15 = use0x15Nullable ?? NeutralizeTrianglesWith0x15;
            return (short)(use0x15 ? 0x15 : 0);
        }

        public static SavedVariable<bool> CloningUpdatesHolpType = new SavedVariable<bool>("Cloning Updates Holp Type", true);
        public static SavedVariable<bool> UseInGameTrigForAngleLogic = new SavedVariable<bool>("Use In-Game Trig for Angle Logic", false);
        public static SavedVariable<bool> UseExtendedLevelBoundaries = new SavedVariable<bool>("Use Extended Level Boundaries", false);
        public static int TriangleVertexMultiplier => UseExtendedLevelBoundaries ? 4 : 1;

        public static SavedVariable<bool> WatchVarPanelBoldNames = new SavedVariable<bool>("Bold Variable Names", true);
        public static SavedVariable<System.Drawing.Font> WatchVarPanelFontOverride = new SavedVariable<System.Drawing.Font>("VariablePanel Font", null);
        public static SavedVariable<uint> WatchVarPanelNameWidth = new SavedVariable<uint>("VariablePanel Name Width", 120);
        public static SavedVariable<uint> WatchVarPanelValueWidth = new SavedVariable<uint>("VariablePanel Value Width", 80);
        public static SavedVariable<uint> WatchVarPanelHorizontalMargin = new SavedVariable<uint>("VariablePanel Horizontal Margin", 2);
        public static SavedVariable<uint> WatchVarPanelVerticalMargin = new SavedVariable<uint>("VariablePanel Vertical Margin", 2);
        public static SavedVariable<bool> ProcessListShowSimilarProcesses = new SavedVariable<bool>("Show Similar Processes", false);

        public static IEnumerable<SavedVariable<bool>> GetBoolVariables()
        {
            foreach (var field in savedFieldsByName)
                if (field.Value.FieldType.GenericTypeArguments[0] == typeof(bool))
                    yield return (SavedVariable<bool>)field.Value.GetValue(null);
        }

        public static List<TabPage> _allTabs = new List<TabPage>();

        public static void InvokeRecommendedTabOrder()
        {
            InvokeTabOrderCleanly(_allTabs);
            Save();
        }

        public static List<string> InitiallySavedTabOrder;

        public static void InvokeInitiallySavedTabOrder()
        {
            List<TabPage> allTabPages = ControlUtilities.GetTabPages(Config.TabControlMain);
            List<TabPage> initiallySavedTabPages = new List<TabPage>();
            foreach (string tabName in InitiallySavedTabOrder)
            {
                TabPage tabPage = allTabPages.FirstOrDefault(t => t.Text == tabName);
                if (tabPage == null) continue;
                initiallySavedTabPages.Add(tabPage);
            }
            InvokeTabOrderCleanly(initiallySavedTabPages);
        }

        private static void InvokeTabOrder(List<TabPage> tabPages)
        {
            for (int i = 0; i < tabPages.Count; i++)
            {
                TabPage tabPage = tabPages[i];
                Config.TabControlMain.TabPages.Remove(tabPage);
                Config.TabControlMain.TabPages.Insert(i, tabPage);
            }
        }

        /** Doesn't remove the currently selected tab. */
        private static void InvokeTabOrderCleanly(List<TabPage> orderedTabPages)
        {
            // Get the selected tab/index
            TabPage selectedTab = Config.TabControlMain.SelectedTab;
            int selectedIndex = Config.TabControlMain.SelectedIndex;

            // Get the final combined ordering of tab pages
            List<TabPage> allTabPages = ControlUtilities.GetTabPages(Config.TabControlMain);
            List<TabPage> nonOrderedTabPages = allTabPages.FindAll(
                tabPage => !orderedTabPages.Contains(tabPage));
            List<TabPage> combinedTabPages = orderedTabPages.Concat(nonOrderedTabPages).ToList();

            // Remove all but the selected tab
            foreach (TabPage tabPage in allTabPages)
            {
                if (tabPage != selectedTab)
                    Config.TabControlMain.TabPages.Remove(tabPage);
            }

            // Add back all of the non-selected tabs
            for (int i = 0; i < combinedTabPages.Count; i++)
            {
                TabPage tabPage = combinedTabPages[i];
                if (tabPage == selectedTab) continue;
                Config.TabControlMain.TabPages.Insert(i, tabPage);
            }
        }

        public static List<string> InitiallySavedRemovedTabs;

        public static List<TabPage> _removedTabs = new List<TabPage>();

        public static void InvokeInitiallySavedRemovedTabs()
        {
            List<TabPage> removedTabs = _allTabs.FindAll(tab => InitiallySavedRemovedTabs.Contains(tab.Text));
            removedTabs.ForEach(tab => RemoveTab(tab, shouldSave: false));
        }

        public static void RemoveTab(TabPage removeTab, bool shouldSave = true)
        {
            TabPage previousTab = Config.TabControlMain.PreviousTab;
            TabPage currentTab = Config.TabControlMain.SelectedTab;
            _removedTabs.Add(removeTab);
            Config.TabControlMain.TabPages.Remove(removeTab);
            if (removeTab == currentTab && Config.TabControlMain.TabPages.Contains(previousTab))
                Config.TabControlMain.SelectedTab = previousTab;
            if (shouldSave) Save();
        }

        public static void AddTab(TabPage tab)
        {
            _removedTabs.Remove(tab);
            Config.TabControlMain.TabPages.Add(tab);
            Save();
        }

        public static List<ToolStripItem> GetRemovedTabItems()
        {
            List<ToolStripItem> items = new List<ToolStripItem>();

            ToolStripMenuItem itemRestoreAllTabs = new ToolStripMenuItem("Restore All Tabs");
            itemRestoreAllTabs.Click += (sender, e) =>
            {
                List<TabPage> removedTabs = new List<TabPage>(_removedTabs);
                removedTabs.ForEach(tab => AddTab(tab));
            };
            items.Add(itemRestoreAllTabs);
            items.Add(new ToolStripSeparator());

            List<ToolStripMenuItem> tabItems = new List<ToolStripMenuItem>();
            foreach (TabPage tab in _removedTabs)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(tab.Text + " Tab");
                item.Click += (sender, e) => AddTab(tab);
                tabItems.Add(item);
            }
            tabItems.Sort((item1, item2) => item1.Text.CompareTo(item2.Text));
            tabItems.ForEach(item => items.Add(item));

            return items;
        }

        public static List<XElement> ToXML()
        {
            XElement tabOrderXElement = new XElement("TabOrder");
            foreach (TabPage tabPage in Config.TabControlMain.TabPages)
            {
                XElement tabXElement = new XElement("Tab", tabPage.Text);
                tabOrderXElement.Add(tabXElement);
            }

            XElement removedTabsXElement = new XElement("RemovedTabs");
            foreach (TabPage tabPage in _removedTabs)
            {
                XElement tabXElement = new XElement("Tab", tabPage.Text);
                removedTabsXElement.Add(tabXElement);
            }

            var lst = new List<XElement>();
            foreach (var field in savedFieldsByName)
                lst.Add(new XElement(field.Key, typeParsers[field.Value.FieldType.GenericTypeArguments[0]].tostring(
                    field.Value.FieldType.GetProperty(nameof(SavedVariable<int>.value)).GetValue(field.Value.GetValue(null)))));
            lst.Add(tabOrderXElement);
            lst.Add(removedTabsXElement);
            return lst;
        }

        public static void Save()
        {
            DialogUtilities.SaveXmlElements(
                FileType.Xml, "SavedSettings", ToXML(), @"Config/SavedSettings.xml");
        }

        public static void Load(string path)
        {
            var doc = XDocument.Load(path);

            foreach (var element in doc.Root.Elements())
            {
                //This is stupid.
                switch (element.Name.ToString())
                {
                    case "TabOrder":
                        {
                            List<string> tabNames = new List<string>();
                            foreach (var tabName in element.Elements())
                            {
                                tabNames.Add(tabName.Value);
                            }
                            InitiallySavedTabOrder = tabNames;
                        }
                        break;

                    case "RemovedTabs":
                        {
                            List<string> tabNames = new List<string>();
                            foreach (var tabName in element.Elements())
                            {
                                tabNames.Add(tabName.Value);
                            }
                            InitiallySavedRemovedTabs = tabNames;
                        }
                        break;
                    default:
                        if (savedFieldsByName.TryGetValue(element.Name.ToString(), out var var))
                            var.FieldType.GetProperty(nameof(SavedVariable<int>.value))
                                .SetValue(var.GetValue(null), typeParsers[var.FieldType.GenericTypeArguments[0]].parse(element.Value));
                        break;
                }
            }
            _isLoaded = true;
        }

        public static void ResetSavedSettings()
        {
            _isLoaded = false;
            foreach (var savedVariable in savedFieldsByName)
                savedVariable.Value.FieldType.GetMethod(nameof(SavedVariable<int>.Reset)).Invoke(savedVariable.Value.GetValue(null), new object[0]);
            _isLoaded = true;
            Save();
        }
    }
}
