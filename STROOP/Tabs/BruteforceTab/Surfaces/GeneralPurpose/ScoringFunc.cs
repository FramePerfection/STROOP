using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using STROOP.Controls.VariablePanel;
using STROOP.Core;
using STROOP.Core.Utilities;
using STROOP.Core.Variables;
using STROOP.Tabs.BruteforceTab.BF_Utilities;
using STROOP.Utilities;
using STROOP.Variables.Utilities;

namespace STROOP.Tabs.BruteforceTab.Surfaces.GeneralPurpose
{
    partial class ScoringFunc : UserControl
    {
        static Dictionary<string, Type> stringToControllerType = new Dictionary<string, Type>();

        static ScoringFunc()
        {
            foreach (var t in typeof(ScoringFunc).Assembly.GetTypes())
                if (t.GetInterfaces().Any(i => i.FullName == typeof(IMethodController).FullName))
                    if (t.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null) != null)
                        stringToControllerType[t.Name] = t;
        }

        static (int x, int y) GetChildBounds(Control parent)
        {
            int x = 0, y = 0;
            foreach (Control ctrl in parent.Controls)
            {
                x = Math.Max(x, ctrl.Right + ctrl.Margin.Right);
                y = Math.Max(y, ctrl.Bottom + ctrl.Margin.Bottom);
            }

            return (x, y);
        }

        bool _muted = false;

        public bool muted
        {
            get => _muted && AccessScope<BruteforceTab.UnmuteScoringFuncs>.content == null;
            set
            {
                _muted = value;
                pbMute.Image = _muted ? Properties.Resources.checkbox_unchecked : pbMute.InitialImage;
            }
        }

        bool expanded = false;
        int collapsedHeight;
        GeneralPurpose.ScoringFuncPrecursor precursor;
        IMethodController controller;
        ToolTip documentationToolTip;

        Dictionary<string, string> docs = new Dictionary<string, string>()
        {
            ["weight"] = "The value to multiply the result of the scoring function by.\nNegative values are allowed and usually invert the effect of the function.",
            ["frame"] = "The 1-indexed frame number in the m64 to apply the scoring function on. 'last' will always be equal to m64_end.",
        };

        public ScoringFunc()
        {
            InitializeComponent();
            BackColor = Color.AliceBlue;
            documentationToolTip = new ToolTip();
            documentationToolTip.ShowAlways = true;

            Controls.Remove(watchVariablePanelParameters);
            RecalculateSize();
            collapsedHeight = Height;
        }

        DateTime hoverBegin;
        WatchVariableControl hoveringWatchVarControl;

        void UpdateTooltip()
        {
            var newHoveringWatchVarControl = expanded ? watchVariablePanelParameters.hoveringWatchVariableControl : null;
            newHoveringWatchVarControl = newHoveringWatchVarControl ?? variablePanelBaseValues.hoveringWatchVariableControl;
            if (newHoveringWatchVarControl != hoveringWatchVarControl)
            {
                hoveringWatchVarControl = newHoveringWatchVarControl;
                hoverBegin = DateTime.Now;
                documentationToolTip.Hide(FindForm());
                documentationToolTip.Active = false;
            }
            else if (newHoveringWatchVarControl != null && (DateTime.Now - hoverBegin).TotalSeconds >= 1 && docs.TryGetValue(newHoveringWatchVarControl.VarName, out var doc) && !documentationToolTip.Active)
            {
                documentationToolTip.Active = true;
                documentationToolTip.Show(doc, FindForm(), FindForm().PointToClient(Cursor.Position));
            }
        }

        public void Init(GeneralPurpose.ScoringFuncPrecursor precursor, BruteforceTab bruteforceTab)
        {
            this.precursor = precursor;
            watchVariablePanelParameters.ClearVariables();
            labelName.Text = precursor.name;
            bruteforceTab.Updating += UpdateTooltip;
            Disposed += (_, __) => bruteforceTab.Updating -= UpdateTooltip;

            variablePanelBaseValues.AddVariable(new NamedVariableCollection.CustomView<double>(WatchVariableSubclass.Number)
            {
                Name = "weight",
                _getterFunction = () => precursor.weight.Yield(),
                _setterFunction = value =>
                {
                    precursor.weight = value;
                    return true.Yield();
                }
            });

            var wrapper = (WatchVariableSelectionWrapper<WatchVariableNumberWrapper<uint>, uint>)variablePanelBaseValues.AddVariable(
                new NamedVariableCollection.CustomView<uint>(typeof(WatchVariableSelectionWrapper<WatchVariableNumberWrapper<uint>, uint>))
                {
                    Name = "frame",
                    _getterFunction = () => precursor.frame.Yield(),
                    _setterFunction = value =>
                    {
                        precursor.frame = value;
                        return true.Yield();
                    }
                }).WatchVarWrapper;
            wrapper.DisplaySingleOption = false;
            (Func<uint> endFrameValue, Action unregister) = bruteforceTab.GetManualValue<uint>("m64_end", () => wrapper.UpdateOption(0));
            Disposed += (_, __) => unregister();
            if (endFrameValue != null)
            {
                wrapper.options.Add(("last", endFrameValue));
                wrapper.SelectOption(0);
            }

            var ctrls = watchVariablePanelParameters.AddVariables(
                precursor.parameterDefinitions.Select(kvp =>
                {
                    docs[kvp.Key.name] = kvp.Key.documentation;
                    string key = kvp.Key.name;
                    var newWatchVar = BF_VariableUtilties.CreateNamedVariable(kvp.Value, kvp.Key.name);
                    newWatchVar.ValueSet += bruteforceTab.DeferUpdateState;
                    precursor.parameterValues[key] = newWatchVar;
                    return newWatchVar;
                }));
            if (stringToControllerType.TryGetValue(precursor.name, out var controllerType))
            {
                controller = (IMethodController)Activator.CreateInstance(controllerType);
                controller.SetTargetFunc(this);
            }
        }

        void RecalculateSize()
        {
            var (x, y) = GetChildBounds(this);
            ClientSize = new Size(x, y);
        }

        void SetExpanded(bool expanded)
        {
            this.expanded = expanded;
            SuspendLayout();
            watchVariablePanelParameters.Anchor &= ~AnchorStyles.Bottom;
            watchVariablePanelParameters.Anchor |= AnchorStyles.Top;
            if (expanded)
            {
                pbExpand.Image = Properties.Resources.image_down;
                Controls.Add(watchVariablePanelParameters);

                watchVariablePanelParameters.Height = watchVariablePanelParameters.GetAutoHeight();
                watchVariablePanelParameters.Top = collapsedHeight + watchVariablePanelParameters.Margin.Top;
                var bottom = watchVariablePanelParameters.Bottom + watchVariablePanelParameters.Margin.Bottom;
                Height = bottom;
            }
            else
            {
                pbExpand.Image = pbExpand.InitialImage;
                Controls.Remove(watchVariablePanelParameters);
                Height = collapsedHeight;
            }

            watchVariablePanelParameters.Anchor |= AnchorStyles.Bottom;
            watchVariablePanelParameters.Anchor &= ~AnchorStyles.Top;
            ResumeLayout();
        }

        public string GetJson()
        {
            if (muted)
                return null;

            var tabs0 = new string('\t', 2);
            var tabs1 = new string('\t', 3);
            var tabs2 = new string('\t', 4);
            var strBuilder = new StringBuilder();

            strBuilder.AppendLine($"{tabs0}{{");
            strBuilder.AppendLine($"{tabs1}\"func\": \"{precursor.name}\",");
            strBuilder.AppendLine($"{tabs1}\"weight\": {precursor.weight},");
            strBuilder.AppendLine($"{tabs1}\"frame\": {precursor.frame},");
            strBuilder.AppendLine($"{tabs1}\"params\": {{");
            var first = true;
            foreach (var parameterValue in precursor.parameterValues)
            {
                if (!first)
                    strBuilder.AppendLine(",");
                first = false;
                strBuilder.Append($"{tabs2}\"{parameterValue.Key}\": {BruteforceTab.MakeJsonValue(parameterValue.Value.value.ToString())}");
            }

            strBuilder.AppendLine($"\n{tabs1}}}");
            strBuilder.Append($"{tabs0}}}");
            return strBuilder.ToString();
        }

        public void DeleteSelf() => this.GetParent<GeneralPurpose>()?.RemoveMethod(this);

        bool deleting = false;

        public void DeleteFromMap()
        {
            if (deleting)
                return;
            deleting = true;
            controller?.Remove();
            DeleteSelf();
        }

        private void pbExpand_Click(object sender, EventArgs e) => SetExpanded(!expanded);

        private void pbRemove_Click(object sender, EventArgs e) => DeleteSelf();

        private void pbMute_Click(object sender, EventArgs e) => muted = !muted;
    }
}
