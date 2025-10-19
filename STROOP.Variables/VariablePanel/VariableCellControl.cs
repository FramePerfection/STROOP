using STROOP.Variables.Utilities;
using System.Drawing;
using System.Xml.Linq;

namespace STROOP.Variables.VariablePanel
{
    public partial class VariableCellControl<TUiContext>
        where TUiContext : IUiContext
    {
        public delegate int SortVariables(IVariableCellUi<TUiContext> a, IVariableCellUi<TUiContext> b);

        public static SortVariables SortNone = (a, b) => 0;

        public static SortVariables SortByPriority = (a, b) =>
            int.Parse(a.control.view.GetValueByKey(CommonVariableProperties.displayPriority) ?? "0")
                .CompareTo(int.Parse(b.control.view.GetValueByKey(CommonVariableProperties.displayPriority) ?? "0"));

        public static SortVariables SortByName = (a, b) => a.control.VarName.CompareTo(b.control.VarName);

        public static readonly Color DEFAULT_COLOR = SystemColors.Control;
        public static readonly Color FAILURE_COLOR = Color.Red;
        public static readonly Color ADD_TO_CUSTOM_TAB_COLOR = Color.CornflowerBlue;
        public static readonly Color REORDER_RESET_COLOR = Color.Black;
        public static readonly Color ADD_TO_VAR_HACK_TAB_COLOR = Color.SandyBrown;
        public static readonly Color SELECTED_COLOR = Color.FromArgb(51, 153, 255);
        private static readonly int FLASH_DURATION_MS = 1000;

        internal readonly IVariable view;

        public readonly List<string> GroupList;

        public readonly IVariablePanel<TUiContext> containingPanel;

        internal readonly IVariableCell varCellInternal;
        public IVariableCellUi<TUiContext> varCell => (IVariableCellUi<TUiContext>)varCellInternal;

        public readonly Color _initialBaseColor;
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

        public VariableCellControl(IVariablePanel<TUiContext> panel, IVariable view)
        {
            this.view = view;
            this.containingPanel = panel;

            view.OnDelete += RemoveFromPanel;

            // Initialize main fields
            GroupList = VariableUtilities.ParseVariableGroupList(view.GetValueByKey("groupList") ?? "Custom");
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

            if (!VariableCellFactory<TUiContext>.TryCreateWrapper(view, this, out varCellInternal))
                BaseColor = Color.DarkRed;

            AddSetting(DefaultSettings.BackgroundColorCellSetting);
            AddSetting(DefaultSettings.HighlightColorCellSetting);
            AddSetting(DefaultSettings.HighlightCellSetting);

            if (view is IMemoryVariable)
            {
                AddSetting(DefaultSettings.FixAddressCellSetting);
            }
        }

        internal void Update()
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

        Dictionary<string, VariableCellSetting<TUiContext>> availableSettings = new Dictionary<string, VariableCellSetting<TUiContext>>();

        public IEnumerable<VariableCellSetting<TUiContext>> AvailableSettings()
            => availableSettings.Values;

        public void AddSetting(VariableCellSetting<TUiContext> cellSetting)
        {
            availableSettings[cellSetting.Name] = cellSetting;
        }

        public bool ApplySettings(string settingName, object settingValue)
        {
            if (availableSettings.TryGetValue(settingName, out var setting))
                return setting.SetterFunction(this, settingValue);
            return false;
        }

        public void RemoveFromPanel()
            => containingPanel?.RemoveVariable(varCell);

        public VariableCellControl<TUiContext> CreateCopy(IVariablePanel<TUiContext> panel)
        {
            var copy = new VariableCellControl<TUiContext>(panel, view);
            copy.BaseColor = _baseColor;
            copy.VarName = VarName;
            copy.GroupList.Clear();
            copy.GroupList.Add(VariableGroup.Custom);
            return copy;
        }

        public void ToggleFixedAddress(bool? fix)
            => (view as IMemoryVariable)?.describedMemoryState.ToggleFixedAddress(fix);

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
                    if (viewType.GetGenericTypeDefinition() == typeof(MemoryVariable<>)
                        || viewType.GetGenericTypeDefinition() == typeof(XmlMemoryVariable<>))
                        return viewType.GetGenericArguments()[0];
                }

                viewType = viewType.BaseType;
            }

            return null;
        }

        public bool SetValue<T>(T value)
        {
            if (view is IVariable<T> compatibleView
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
            if (view is IXmlMemoryVariable xmlView)
                return xmlView.GetXml();
            return null;
        }

        public List<string> GetVarInfo() => varCellInternal.GetVarInfo();

        public override string ToString() => varCellInternal.ToString();
    }
}
