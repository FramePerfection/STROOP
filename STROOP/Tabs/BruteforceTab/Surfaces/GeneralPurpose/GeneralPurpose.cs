using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using STROOP.Utilities;
using STROOP.Tabs.BruteforceTab.BF_Utilities;

namespace STROOP.Tabs.BruteforceTab.Surfaces.GeneralPurpose
{
    [Surface("general_purpose")]
    partial class GeneralPurpose : Surface
    {
        public class ScoringFuncPrecursor
        {
            public class Identifier
            {
                public string name;
                public string documentation;
            }

            public string name = "<undefined>";
            public uint frame = 30;
            public double weight = 1.0;

            public Dictionary<Identifier, string> parameterDefinitions = new Dictionary<Identifier, string>(
                new Core.Utilities.EqualityComparer<Identifier>((a, b) => a.name == b.name, a => a.name.GetHashCode())
            );

            public Dictionary<string, IBruteforceVariableView> parameterValues = new Dictionary<string, IBruteforceVariableView>();

            public Type GetParameterWrapperType(string name)
            {
                if (parameterDefinitions.TryGetValue(new Identifier { name = name }, out var typeString) && BF_VariableUtilties.fallbackWrapperTypes.TryGetValue(typeString, out var type))
                    return type;
                return null;
            }

            public ScoringFuncPrecursor()
            {
            }

            public ScoringFuncPrecursor(ScoringFuncPrecursor src)
            {
                this.name = src.name;
                this.frame = src.frame;
                this.weight = src.weight;
                this.parameterDefinitions = src.parameterDefinitions;
            }
        }

        Dictionary<string, ScoringFuncPrecursor> scoringFuncsByName;

        void ReadFunctionsFile(BruteforceTab parent)
        {
            var tokenBuilder = new System.Text.StringBuilder();
            Stack<string> lastTokens = new Stack<string>();

            bool comment = false;
            scoringFuncsByName?.Clear();
            scoringFuncsByName = new Dictionary<string, ScoringFuncPrecursor>();
            ScoringFuncPrecursor.Identifier lastIdentifier = null;

            ScoringFuncPrecursor currentPrecursor = null;
            using (var rd = new StreamReader($"{parent.modulePath}/param_definitions.txt"))
                while (!rd.EndOfStream)
                {
                    char c = (char)rd.Read();
                    if (c == '/' && rd.Peek() == '/')
                    {
                        comment = true;
                        rd.Read();
                        continue;
                    }

                    switch (c)
                    {
                        case ':':
                            if (currentPrecursor != null)
                                scoringFuncsByName.Add(currentPrecursor.name, currentPrecursor);
                            currentPrecursor = new ScoringFuncPrecursor() { name = tokenBuilder.ToString() };
                            tokenBuilder.Clear();
                            break;
                        case '\n':
                            if (comment && lastIdentifier != null)
                                lastIdentifier.documentation = tokenBuilder.ToString();
                            comment = false;
                            tokenBuilder.Clear();
                            lastTokens.Clear();
                            goto case ' ';
                        case '\t':
                        case ' ':
                            if (tokenBuilder.Length > 0)
                                if (comment)
                                    tokenBuilder.Append(c);
                                else
                                {
                                    lastTokens.Push(tokenBuilder.ToString());
                                    tokenBuilder.Clear();
                                }

                            break;
                        case ';':
                            var nameToken = tokenBuilder.ToString();
                            tokenBuilder.Clear();
                            if (lastTokens.Count != 1)
                                throw new Exception("Invalid bruteforcer parameter definition file!");
                            var typeToken = lastTokens.Pop();
                            lastIdentifier = new ScoringFuncPrecursor.Identifier() { name = nameToken };
                            currentPrecursor.parameterDefinitions[lastIdentifier] = typeToken;
                            lastTokens.Clear();
                            break;
                        default:
                            tokenBuilder.Append(c);
                            break;
                    }
                }

            if (currentPrecursor != null)
                scoringFuncsByName.Add(currentPrecursor.name, currentPrecursor);
        }

        public GeneralPurpose()
        {
            InitializeComponent();
            ParentChanged += (_, __) =>
            {
                var parent = parentTab;
                if (parent != null)
                    ReadFunctionsFile(parent);
            };
        }

        void AddMethod(ScoringFuncPrecursor meth)
        {
            var ctrl = new ScoringFunc();
            ctrl.Init(meth, parentTab);
            flowPanelScoring.Controls.Add(ctrl);
        }

        void AddPerturbator(Perturbator perturbator)
        {
            perturbator.Init(parentTab);
            flowPanelScoring.Controls.Add(perturbator);
        }

        public void RemoveMethod(ScoringFunc ctrl)
        {
            flowPanelScoring.Controls.Remove(ctrl);
            ctrl.DeleteFromMap();
            ctrl.Dispose();
        }

        public void RemovePerturbator(Perturbator ctrl)
        {
            flowPanelScoring.Controls.Remove(ctrl);
            ctrl.Dispose();
        }

        private void btnAddMethod_Click(object sender, EventArgs e)
        {
            var ctr = new ContextMenuStrip();
            foreach (var scoringFunc in scoringFuncsByName)
            {
                var precursor = scoringFunc.Value;
                ctr.Items.AddHandlerToItem(precursor.name, () => AddMethod(new ScoringFuncPrecursor(precursor)));
            }

            ctr.Show(Cursor.Position);
        }

        private void btnAddPerturbator_Click(object sender, EventArgs e)
        {
            AddPerturbator(new Perturbator());
        }

        public override string GetParameter(string parameterName)
        {
            Func<Control, string> buildArrayEntry = null;

            if (parameterName == "scoring_methods")
                buildArrayEntry = ctrl => (ctrl is ScoringFunc func) ? func.GetJson() : null;
            else if (parameterName == "perturbators")
                buildArrayEntry = ctrl => (ctrl is Perturbator perturbator) ? perturbator.GetJson() : null;

            if (buildArrayEntry != null)
            {
                var strBuilder = new System.Text.StringBuilder();
                strBuilder.AppendLine("[");
                bool first = true;
                foreach (var ctrl in flowPanelScoring.Controls)
                {
                    var entryString = buildArrayEntry(ctrl as Control);
                    if (entryString != null)
                    {
                        if (!first)
                            strBuilder.AppendLine(",");
                        strBuilder.Append(entryString);
                        first = false;
                    }
                }

                strBuilder.Append("\n\t]");
                return strBuilder.ToString();
            }

            return base.GetParameter(parameterName);
        }

        public override void InitJson()
        {
            base.InitJson();
            int numPerturbators = 0;

            var scoringJson = parentTab.GetJsonText("scoring_methods") as JsonNodeArray;
            if (scoringJson != null)
                foreach (var node in scoringJson.values)
                {
                    var obj = node as JsonNodeObject;
                    if (obj == null)
                        continue; // Ignore non-objects
                    if (obj.TryGetValue<JsonNodeString>("func", out var funcNode)
                        && scoringFuncsByName.TryGetValue(funcNode.value.Trim('"') ?? "", out var precursorPreset))
                    {
                        var precursor = new ScoringFuncPrecursor(precursorPreset);
                        if (obj.TryGetValue<JsonNodeNumber>("weight", out var weightNode))
                            precursor.weight = weightNode.valueDouble ?? 1.0;
                        else
                            precursor.weight = 1.0;
                        if (obj.TryGetValue<JsonNodeNumber>("frame", out var frameNode))
                            precursor.frame = (uint)(frameNode.valueLong ?? 30);
                        else
                            precursor.frame = 30;

                        if (obj.TryGetValue<JsonNodeObject>("params", out var parametersNode))
                            foreach (var n in parametersNode.values)
                            {
                                if (precursor.parameterDefinitions.TryGetValue(new ScoringFuncPrecursor.Identifier { name = n.Key }, out var bruteforcerType))
                                {
                                    if (n.Value is JsonNodeString stringNode)
                                        precursor.parameterValues[n.Key] = BF_VariableUtilties.CreateNamedVariable(
                                            bruteforcerType,
                                            n.Key,
                                            StringUtilities.GetJsonValue(precursor.GetParameterWrapperType(n.Key), stringNode.value)
                                        );
                                    else
                                        precursor.parameterValues[n.Key] = BF_VariableUtilties.CreateNamedVariable(
                                            bruteforcerType,
                                            n.Key,
                                            n.Value.valueObject
                                        );
                                }
                            }

                        AddMethod(precursor);
                    }
                }

            var perturbatorJson = parentTab.GetJsonText("perturbators") as JsonNodeArray;
            if (perturbatorJson != null)
                foreach (var node in perturbatorJson.values)
                {
                    var obj = node as JsonNodeObject;
                    if (obj == null)
                        continue; // Ignore non-objects
                    numPerturbators++;
                    var perturbator = new Perturbator();
                    if (obj.TryGetValue<JsonNodeNumber>("perturbation_chance", out var perturbationChance))
                        perturbator.perturbationChanceVariable._setterFunction((float)perturbationChance.valueDouble);
                    if (obj.TryGetValue<JsonNodeNumber>("max_perturbation", out var maxPerturbation))
                        perturbator.maxPerturbationVariable._setterFunction((byte)maxPerturbation.valueLong);
                    if (obj.TryGetValue<JsonNodeNumber>("min_frame", out var minFrame))
                        perturbator.minFrameVariable._setterFunction((uint)minFrame.valueLong);
                    if (obj.TryGetValue<JsonNodeNumber>("max_frame", out var maxFrame))
                        perturbator.maxFrameVariable._setterFunction((uint)maxFrame.valueLong);
                    AddPerturbator(perturbator);
                }

            // If no perturbators were in the configuration, add a default one
            if (numPerturbators == 0)
                AddPerturbator(new Perturbator());
            flowPanelScoring.ResumeLayout();
        }

        public override void Cleanup()
        {
            base.Cleanup();
            var allControls = new List<Control>();
            foreach (Control ctrl in flowPanelScoring.Controls)
                allControls.Add(ctrl);
            foreach (var ctrl in allControls)
                if (ctrl is ScoringFunc scoringFunc)
                    RemoveMethod(scoringFunc);
                else if (ctrl is Perturbator perturbator)
                    RemovePerturbator(perturbator);
        }
    }
}
