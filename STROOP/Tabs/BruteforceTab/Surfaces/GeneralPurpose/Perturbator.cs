using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using STROOP.Controls.VariablePanel;
using STROOP.Utilities;

namespace STROOP.Tabs.BruteforceTab.Surfaces.GeneralPurpose
{
    partial class Perturbator : UserControl
    {
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

        bool expanded = false;
        int collapsedHeight;

        public readonly BruteforceVariableView<uint> minFrameVariable, maxFrameVariable;
        public readonly BruteforceVariableView<float> perturbationChanceVariable;
        public readonly BruteforceVariableView<byte> maxPerturbationVariable;
        List<IBruteforceVariableView> variableViews = new List<IBruteforceVariableView>();
        List<WatchVariableControl> controls = new List<WatchVariableControl>();

        private BruteforceVariableView<T> CreateVar<T>(string name, string bruteforcerType, T defaultValue)
        {
            var result = (BruteforceVariableView<T>)BF_Utilities.BF_VariableUtilties.CreateNamedVariable(name, bruteforcerType, defaultValue);
            variableViews.Add(result);
            return result;
        }

        public Perturbator()
        {
            InitializeComponent();
            BackColor = Color.LightPink;

            Controls.Remove(watchVariablePanelParameters);
            RecalculateSize();
            collapsedHeight = Height;

            minFrameVariable = CreateVar<uint>("u32", "min_frame", 0);
            maxFrameVariable = CreateVar<uint>("u32", "max_frame", 999999);
            perturbationChanceVariable = CreateVar<float>("f32", "perturbation_chance", 0.25f);
            maxPerturbationVariable = CreateVar<byte>("u8", "max_perturbation", 8);
        }

        public void Init(BruteforceTab bruteforceTab)
        {
            watchVariablePanelParameters.ClearVariables();
            labelName.Text = "Perturbator";

            controls = watchVariablePanelParameters.AddVariables(variableViews).ToList();
            foreach (var control in controls)
                control.WatchVarWrapper.ValueSet += bruteforceTab.DeferUpdateControlState;
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

        private void pbExpand_Click(object sender, EventArgs e) => SetExpanded(!expanded);

        private void pbRemove_Click(object sender, EventArgs e)
        {
            this.GetParent<GeneralPurpose>().RemovePerturbator(this);
        }

        public string GetJson()
        {
            var tabs0 = new string('\t', 2);
            var tabs1 = new string('\t', 3);
            var tabs2 = new string('\t', 4);
            var strBuilder = new StringBuilder();

            strBuilder.Append($"{tabs0}{{");
            foreach (var param in controls)
                strBuilder.Append($"{Environment.NewLine}{tabs1}\"{param.view.Name}\": {param.WatchVarWrapper.GetValueText()},");
            strBuilder.Remove(strBuilder.Length - 1, 1);
            strBuilder.AppendLine($"{Environment.NewLine}{tabs0}}}");
            return strBuilder.ToString();
        }
    }
}
