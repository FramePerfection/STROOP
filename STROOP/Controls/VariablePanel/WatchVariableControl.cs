using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using STROOP.Structs;
using STROOP.Utilities;
using STROOP.Variables;
using STROOP.Variables.Views;

namespace STROOP.Controls.VariablePanel
{
    public partial class WatchVariableControl
    {
        public delegate int SortVariables(WatchVariableControl a, WatchVariableControl b);

        public static SortVariables SortNone = (a, b) => 0;
        public static SortVariables SortByPriority = (a, b) => a.view.DislpayPriority.CompareTo(a.view.DislpayPriority);
        public static SortVariables SortByName = (a, b) => a.VarName.CompareTo(b.VarName);

        public static readonly Color DEFAULT_COLOR = SystemColors.Control;
        public static readonly Color FAILURE_COLOR = Color.Red;
        public static readonly Color ADD_TO_CUSTOM_TAB_COLOR = Color.CornflowerBlue;
        public static readonly Color REORDER_RESET_COLOR = Color.Black;
        public static readonly Color ADD_TO_VAR_HACK_TAB_COLOR = Color.SandyBrown;
        public static readonly Color SELECTED_COLOR = Color.FromArgb(51, 153, 255);
        private static readonly int FLASH_DURATION_MS = 1000;

        public readonly IVariableView view;
        public readonly WatchVariableWrapper WatchVarWrapper;

        public readonly List<string> GroupList;

        // Parent control
        public readonly WatchVariablePanel containingPanel;

        private readonly Color _initialBaseColor;
        private Color _baseColor;

        public Color BaseColor
        {
            get { return _baseColor; }
            set
            {
                _baseColor = value;
                currentColor = value;
            }
        }

        public Color currentColor { get; private set; }

        private bool _isFlashing;
        private DateTime _flashStartTime;
        private Color _flashColor;

        public string VarName;

        public Color HighlightColor = Color.Red;
        public bool Highlighted;

        public bool RenameMode;

        public bool alwaysVisible;

        bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected && containingPanel.IsSelected; }
            set { _isSelected = value; }
        }

        public WatchVariableControl(WatchVariablePanel panel, IVariableView view)
        {
            this.view = view;
            this.containingPanel = panel;

            view.OnDelete += () => containingPanel.RemoveVariable(this);

            // Initialize main fields
            VarName = view.Name;
            GroupList = WatchVariableUtilities.ParseVariableGroupList(view.GetValueByKey("groupList") ?? "Custom");
            RenameMode = false;
            IsSelected = false;

            // Initialize color fields
            var colorString = view.GetValueByKey("color");
            Color? backgroundColor;
            if (colorString != null && ColorUtilities.ColorToParamsDictionary.TryGetValue(colorString, out var c))
                backgroundColor = ColorTranslator.FromHtml(c);
            else
                backgroundColor = colorString == null ? (Color?)null : Color.FromName(colorString);
            if (backgroundColor.HasValue && backgroundColor.Value.A == 0)
                backgroundColor = null;
            _initialBaseColor = backgroundColor ?? DEFAULT_COLOR;
            _baseColor = _initialBaseColor;
            currentColor = _baseColor;
            _isFlashing = false;
            _flashStartTime = DateTime.Now;

            if (!WatchVariableWrapperFactory.TryCreateWrapper(view, this, out WatchVarWrapper))
                BaseColor = Color.DarkRed;

            AddSetting(DefaultSettings.BackgroundColorSetting);
            AddSetting(DefaultSettings.HighlightColorSetting);
            AddSetting(DefaultSettings.HighlightSetting);

            if (view is IMemoryBasedVariableView)
            {
                AddSetting(DefaultSettings.FixAddressSetting);
            }
        }

        void ShowMainContextMenu(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                ShowContextMenu();
        }

        public void ShowContextMenu()
        {
            ContextMenuStrip ctx = new ContextMenuStrip();
            if (containingPanel != null)
            {
                var uniqueSettings = new HashSet<WatchVariableSetting>();
                foreach (var selectedVar in containingPanel.GetSelectedVars())
                    foreach (var setting in selectedVar.availableSettings)
                        uniqueSettings.Add(setting.Value);

                var sortedOptions = uniqueSettings.ToList();
                sortedOptions.Sort((a, b) => string.Compare(a.Name, b.Name));
                foreach (var setting in sortedOptions)
                    setting.CreateContextMenuEntry(ctx.Items, containingPanel.GetSelectedVars);

                ctx.Items.Add(new ToolStripSeparator());
                foreach (var item in WatchVariableSelectionUtilities.CreateSelectionToolStripItems(containingPanel.GetSelectedVars(), containingPanel))
                    ctx.Items.Add(item);
            }

            ctx.Show(System.Windows.Forms.Cursor.Position);
        }

        public void UpdateControl()
        {
            WatchVarWrapper.Update();
            UpdateColor();
        }

        private void UpdateColor()
        {
            Color selectedOrBaseColor = IsSelected ? SELECTED_COLOR : _baseColor;
            if (_isFlashing)
            {
                DateTime currentTime = DateTime.Now;
                double timeSinceFlashStart = currentTime.Subtract(_flashStartTime).TotalMilliseconds;
                if (timeSinceFlashStart < FLASH_DURATION_MS)
                    currentColor = ColorUtilities.InterpolateColor(_flashColor, selectedOrBaseColor, timeSinceFlashStart / FLASH_DURATION_MS);
                else
                {
                    currentColor = selectedOrBaseColor;
                    _isFlashing = false;
                }
            }
            else
                currentColor = selectedOrBaseColor;
        }

        public void FlashColor(Color color)
        {
            _flashStartTime = DateTime.Now;
            _flashColor = color;
            _isFlashing = true;
        }

        public bool BelongsToGroup(string variableGroup)
        {
            if (variableGroup == VariableGroup.NoGroup)
                return GroupList.Count == 0;
            return GroupList.Contains(variableGroup);
        }

        public bool BelongsToAnyGroup(List<string> variableGroups)
        {
            return variableGroups.Any(varGroup => BelongsToGroup(varGroup));
        }

        public bool BelongsToAnyGroupOrHasNoGroup(List<string> variableGroups)
        {
            return GroupList.Count == 0 || BelongsToAnyGroup(variableGroups);
        }

        Dictionary<string, WatchVariableSetting> availableSettings = new Dictionary<string, WatchVariableSetting>();

        public void AddSetting(WatchVariableSetting setting)
        {
            availableSettings[setting.Name] = setting;
        }

        public bool ApplySettings(string settingName, object settingValue)
        {
            if (availableSettings.TryGetValue(settingName, out var setting))
                return setting.SetterFunction(this, settingValue);
            return false;
        }

        public void RemoveFromPanel()
        {
            if (containingPanel == null) return;
            containingPanel.RemoveVariable(this);
        }

        public void OpenPanelOptions(Point point)
        {
            if (containingPanel == null) return;
            containingPanel.ContextMenuStrip.Show(point);
        }

        public WatchVariableControl CreateCopy(WatchVariablePanel panel)
        {
            var copy = new WatchVariableControl(panel, view);
            copy.BaseColor = _baseColor;
            copy.VarName = VarName;
            copy.GroupList.Clear();
            copy.GroupList.Add(VariableGroup.Custom);
            return copy;
        }

        public void ToggleFixedAddress(bool? fix)
            => (view as IMemoryBasedVariableView)?.describedMemoryState.ToggleFixedAddress(fix);

        public void ToggleHighlighted(Color? color = null)
        {
            Highlighted = color != null ? true : !Highlighted;
            if (color != null)
                HighlightColor = color.Value;
        }

        public Type GetMemoryType()
        {
            var viewType = view.GetType();
            while (viewType != null)
            {
                if (viewType.IsGenericType)
                {
                    if (viewType.GetGenericTypeDefinition() == typeof(MemoryBasedVariableView<>)
                        || viewType.GetGenericTypeDefinition() == typeof(XmlMemoryBasedVariableView<>))
                        return viewType.GetGenericArguments()[0];
                }

                viewType = viewType.BaseType;
            }

            return null;
        }

        public bool SetValue<T>(T value)
        {
            if (view is IVariableView<T> compatibleView
                && compatibleView.setter(value).Aggregate(true, (a, b) => a && b))
                return true;
            else if (value is IConvertible convertibleValue && view.TrySetValue(convertibleValue).Aggregate(true, (a, b) => a && b))
                return true;
            FlashColor(FAILURE_COLOR);
            return false;
        }

        public XElement ToXml(bool useCurrentState = true)
        {
            Color? color = _baseColor == DEFAULT_COLOR ? (Color?)null : _baseColor;
            if (view is IXmlMemoryBasedVariableView xmlView)
                return xmlView.GetXml();
            return null;
        }

        public List<string> GetVarInfo() => WatchVarWrapper.GetVarInfo();

        public override string ToString() => WatchVarWrapper.ToString();
    }
}
