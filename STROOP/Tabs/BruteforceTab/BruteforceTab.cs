using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using Microsoft.WindowsAPICodePack.Dialogs;
using STROOP.Controls.VariablePanel;
using STROOP.Core.Variables;
using STROOP.Tabs.BruteforceTab.BF_Utilities;
using STROOP.Utilities;
using AutomaticParameterGetters = System.Collections.Generic.Dictionary<STROOP.Tabs.BruteforceTab.ValueGetters.GetterFuncs, System.Collections.Generic.HashSet<string>>;

namespace STROOP.Tabs.BruteforceTab
{
    public partial class BruteforceTab : STROOPTab
    {
        public const int MAX_CONSOLE_LINES = 500;

        public class UnmuteScoringFuncs
        {
        }

        static string BRUTEFORCER_PATH = "Bruteforcers";
        static readonly string[] variableSourceFiles = { "MarioData.xml", "CameraData.xml", "ActionsData.xml", "MiscData.xml" };
        static IEnumerable<(Type type, SurfaceAttribute attribute)> moduleTypes;

        static XElement configNode = null;

        [InitializeConfigParser]
        static void InitConfig() => XmlConfigParser.AddConfigParser("BruteforcerModulesPath", ParseBruteforcerModdulesPath);

        static void ParseBruteforcerModdulesPath(XElement node)
        {
            configNode = node;
            BRUTEFORCER_PATH = node.Attribute("path").Value;
        }

        static void SaveBruteforcerModulePath()
        {
            if (configNode == null)
                Program.config.Root.Add(configNode = new XElement(XName.Get("BruteforcerModulesPath")));
            configNode.SetAttributeValue(XName.Get("path"), BRUTEFORCER_PATH);
            configNode.Document.Save(Program.CONFIG_FILE_NAME);
        }

        static BruteforceTab()
        {
            var lst = new List<(Type, SurfaceAttribute)>();
            foreach (var t in typeof(BruteforceTab).Assembly.GetTypes())
                foreach (var attrObj in t.GetCustomAttributes(false))
                {
                    if (attrObj is SurfaceAttribute attr)
                    {
                        lst.Add((t, attr));
                        break;
                    }
                }

            moduleTypes = lst;
        }

        public string modulePath { get; private set; }
        public Surface surface { get; private set; }
        public event Action Updating;

        Dictionary<string, Func<string>> jsonTexts = new Dictionary<string, Func<string>>();
        Dictionary<string, JsonNode> variableKeepObjects = new Dictionary<string, JsonNode>();
        Dictionary<string, (string modifier, string name)> variables;
        string m64File;
        volatile Process bfProcess;
        Dictionary<string, Func<string>> stateGetters = new Dictionary<string, Func<string>>();
        Dictionary<string, Func<string>> parameterGetters = new Dictionary<string, Func<string>>();
        Dictionary<string, Func<string>> controlStateGetters = new Dictionary<string, Func<string>>();
        Dictionary<string, string> docs = new Dictionary<string, string>();
        List<NamedVariableCollection.IView> knownStateVariables = new List<NamedVariableCollection.IView>();
        List<IBruteforceVariableView> manualParameterVariables = new List<IBruteforceVariableView>();
        Queue<string> outputLines = new Queue<string>();
        WatchVariableControl hoveringWatchVarControl;
        ToolTip documentationToolTip;
        ContextMenuStrip moduleStrip;
        DateTime hoverBegin;

        IgnoreScope ignoreWrite = new IgnoreScope();
        bool needsUpdateState = false;
        bool needsUpdateControlState = false;

        IEnumerable<string> GetBruteforcerModulePaths()
        {
            if (Directory.Exists(BRUTEFORCER_PATH))
                foreach (var name in Directory.GetDirectories(BRUTEFORCER_PATH))
                    if (File.Exists($"{name}/main.exe"))
                        yield return name;
        }

        public BruteforceTab()
        {
            InitializeComponent();
            documentationToolTip = new ToolTip();
            var tabStops = new int[16];
            for (int i = 0; i < tabStops.Length; i++)
                tabStops[i] = i * 16;
            txtJsonOutput.SelectionTabs = tabStops;
            txtManualConfig.SelectionTabs = tabStops;

            foreach (var varSrc in variableSourceFiles)
                knownStateVariables.AddRange(XmlConfigParser.OpenWatchVariableControlPrecursors($"Config/{varSrc}"));

            this.AllowDrop = true;
            txtJsonOutput.AllowDrop = true;
            txtJsonOutput.DragOver += (object sender, DragEventArgs e) =>
                e.Effect = DragDropEffects.Copy;
            txtJsonOutput.DragDrop += (object sender, DragEventArgs e) =>
            {
                string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                if (filePaths.Length == 0) return;
                string filePath = filePaths[0];
                string fileName = Path.GetFileName(filePath);
                if (fileName.EndsWith(".json"))
                    ReadJson(filePath);
            };
        }

        public (Func<T> getValue, Action unregister) GetManualValue<T>(string key, Action onValueChange)
        {
            foreach (var manualParameterVar_it in manualParameterVariables)
                if (manualParameterVar_it.Name == key)
                {
                    var manualParameterVar_cap = manualParameterVar_it;
                    manualParameterVar_cap.ValueSet += onValueChange;
                    return (() => (manualParameterVar_cap is BruteforceVariableView<T> compatible) ? compatible._getterFunction().First() : default(T),
                        () => manualParameterVar_cap.ValueSet -= onValueChange);
                }

            return (null, () => { });
        }

        public JsonNode GetJsonText(string key)
        {
            if (variableKeepObjects.TryGetValue(key, out var result))
                return result;
            return null;
        }

        public void DeferUpdateState() => needsUpdateState = true;

        public void DeferUpdateControlState() => needsUpdateControlState = needsUpdateState = true;

        public void UpdateState()
        {
            needsUpdateState = false;
            using (new AccessScope<BruteforceTab>(this))
            {
                var strBuilder = new StringBuilder();
                foreach (var kvp in parameterGetters)
                    strBuilder.AppendLine($"\t\"{kvp.Key}\": {kvp.Value()}, ");
                var text = strBuilder.ToString();
                jsonTexts["bfState"] = () => text;
                WriteJson();
            }
        }

        public override void Update(bool active)
        {
            base.Update(active);
            if (active)
            {
                if (needsUpdateState)
                    UpdateState();
                if (needsUpdateControlState)
                    UpdateControlState();
                var newHoveringWatchVarControl = watchVariablePanelParams.hoveringWatchVariableControl;
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

                Updating?.Invoke();
            }
        }

        private void UpdateControlState()
        {
            if (bfProcess == null)
                return;
            needsUpdateControlState = false;
            using (new AccessScope<BruteforceTab>(this))
            {
                var strBuilder = new StringBuilder();
                strBuilder.AppendLine("{");
                foreach (var kvp in controlStateGetters)
                    strBuilder.AppendLine($"\t\"{kvp.Key}\": {kvp.Value()}, ");

                strBuilder.Remove(strBuilder.Length - 4, 4); // Remove last ',' and linebreak
                strBuilder.AppendLine("\n}");
                var text = strBuilder.ToString() + "\0";
                bfProcess?.StandardInput.Write(text);
                bfProcess?.StandardInput.Flush();
            }
        }

        private void SetM64(string fileName)
        {
            m64File = fileName;
            labelM64.Text = Path.GetFileName(m64File);
            if (fileName != Path.GetFullPath($"{modulePath}/tmp.m64"))
                File.Copy(m64File, $"{modulePath}/tmp.m64", true);
            watchVariablePanelParams.SetVariableValueByName("m64_input", "tmp.m64");
        }

        private void ChooseM64()
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = ".m64 files (*.m64)|*.m64";
            if (ofd.ShowDialog() == DialogResult.OK)
                SetM64(ofd.FileName);
        }

        private void ReadStateDefinition()
        {
            using (var rd = new StreamReader($"{modulePath}/state_definitions.txt"))
            {
                string comment = null;
                while (!rd.EndOfStream)
                {
                    if ((char)rd.Peek() == '"')
                    {
                        rd.Read();
                        var commentBuilder = new StringBuilder();
                        char next;
                        while (!rd.EndOfStream && (next = (char)rd.Read()) != '"')
                            commentBuilder.Append(next);
                        comment = commentBuilder.ToString();
                        while (!rd.EndOfStream && (char)rd.Read() != '\n') ;
                    }
                    else
                    {
                        var line = rd.ReadLine().Trim();
                        if (line.Length > 0 && !line.StartsWith("//"))
                        {
                            var split = line.Split(' ');
                            if (split.Length == 3)
                            {
                                var varName = split[2].Trim().Trim(';');
                                variables.Add(varName, (split[0].Trim(), split[1].Trim()));

                                if (comment != null)
                                    docs[varName] = comment;
                            }
                        }

                        comment = null;
                    }
                }
            }
        }

        private void InitStateGetters()
        {
            foreach (var v in variables)
                foreach (var watchVar_it in knownStateVariables)
                {
                    var watchVar = watchVar_it;
                    var jsonName = watchVar.GetJsonName();
                    if (jsonName == v.Key && watchVar.TryGetNumberValues<double>(out var lst))
                        stateGetters[v.Key] = () =>
                        {
                            var str = lst.FirstOrDefault().ToString();
                            return StringUtilities.MakeJsonValue(str);
                        };
                }
        }

        private void FindAutomaticParameterGetters(string moduleName, out AutomaticParameterGetters automaticParameterGetters)
        {
            automaticParameterGetters = new Dictionary<ValueGetters.GetterFuncs, HashSet<string>>();

            foreach (var v in variables)
            {
                if (ValueGetters.valueGetters.TryGetValue((moduleName, v.Key), out var fns))
                {
                    if (fns.displayName == null)
                    {
                        // null indicates this shall be the only available option
                        var vKey = v.Key;
                        Func<string> fn = () => fns.dic.FirstOrDefault().Value().moduleVariableGetter(vKey);
                        parameterGetters[vKey] = fn;
                        if (v.Value.modifier == "control")
                            controlStateGetters[vKey] = fn;
                    }
                    else
                    {
                        if (docs.TryGetValue(v.Key, out var doc))
                            docs[fns.displayName] = doc;
                        HashSet<string> set;
                        if (!automaticParameterGetters.TryGetValue(fns, out set))
                            automaticParameterGetters[fns] = set = new HashSet<string>();
                        set.Add(v.Key);
                    }
                }
            }
        }

        private void InitAutomaticParameterGetters(AutomaticParameterGetters automaticParameterGetters)
        {
            foreach (var fns in automaticParameterGetters)
                if (fns.Key.displayName != null)
                {
                    var variableName = fns.Key.displayName;
                    var newWatchVar = new BruteforceVariableView<string>(typeof(WatchVariableSelectionWrapper<WatchVariableStringWrapper, string>), variableName)
                    {
                        Name = variableName,
                    };
                    var watchVarControl = watchVariablePanelParams.AddVariable(newWatchVar);
                    var wrapper = (WatchVariableSelectionWrapper<WatchVariableStringWrapper, string>)watchVarControl.WatchVarWrapper;

                    var options = new string[fns.Key.dic.Count + 1];
                    int i = 0;
                    foreach (var ksdda in fns.Key.dic)
                        options[i++] = ksdda.Key;
                    options[options.Length - 1] = "[Keep]";

                    void SetConcreteOption(ValueGetters.Option option)
                    {
                        newWatchVar._setterFunction(option.optionName);
                        jsonTexts[variableName] = () =>
                        {
                            var keepTextBuilder = new StringBuilder();
                            foreach (var var in fns.Value)
                                keepTextBuilder.AppendLine($"\t\"{var}\": {option.moduleVariableGetter(var)},");
                            return keepTextBuilder.ToString();
                        };
                    }

                    string SetSelected(string selectedStr)
                    {
                        Func<string, string> fn = var =>
                        {
                            if (variableKeepObjects.TryGetValue(var, out var node))
                                return node.sourceString;
                            return "0";
                        };
                        if (selectedStr != "[Keep]")
                        {
                            var option = fns.Key.dic[selectedStr]();
                            fn = option.moduleVariableGetter;
                            selectedStr = option.optionName;
                        }

                        SetConcreteOption((selectedStr, fn));
                        return selectedStr;
                    }

                    foreach (var option_it in options)
                    {
                        var option_cap = option_it;
                        Func<string> geledate = () =>
                        {
                            var displayText = SetSelected(option_cap);
                            UpdateState();
                            return displayText;
                        };
                        wrapper.options.Add((option_cap, geledate));
                    }

                    SetSelected("[Keep]");
                    if (fns.Key.defaultOption != null)
                        SetConcreteOption(fns.Key.defaultOption);
                }
        }

        private void FindManualParameters(string moduleName)
        {
            foreach (var v in variables)
            {
                if (!stateGetters.ContainsKey(v.Key)
                    && !ValueGetters.valueGetters.ContainsKey((moduleName, v.Key))
                    && BF_VariableUtilties.fallbackWrapperTypes.TryGetValue(v.Value.name, out var wrapperType))
                {
                    WatchVariableControl ctrl = null;
                    var newWatchVar = BF_VariableUtilties.CreateNamedVariable(v.Value.name, v.Key);
                    manualParameterVariables.Add(newWatchVar);
                    newWatchVar.ValueSet += UpdateState;
                    ctrl = watchVariablePanelParams.AddVariable(newWatchVar);
                    Func<string> fn = () => StringUtilities.MakeJsonValue(newWatchVar.value?.ToString() ?? "0");
                    if (v.Value.modifier == "control")
                    {
                        controlStateGetters[v.Key] = fn;
                        ctrl.BaseColor = ColorUtilities.GetColorFromString("Yellow");
                        newWatchVar.ValueSet += UpdateControlState;
                    }

                    parameterGetters[v.Key] = fn;
                }
            }
        }

        private void InitSurface(string moduleName)
        {
            surface?.Cleanup();
            surface?.Dispose();
            foreach (var t in moduleTypes)
                if (t.attribute.moduleName == moduleName)
                {
                    surface = (Surface)Activator.CreateInstance(t.type);
                    tabSurface.SuspendLayout();
                    foreach (Control ctrl in tabSurface.Controls)
                        ctrl.Dispose();
                    tabSurface.Controls.Clear();
                    tabSurface.Controls.Add(surface);
                    surface.InitJson();
                    tabSurface.ResumeLayout();
                    break;
                }
        }

        private void LoadModule(string modulePath)
        {
            this.modulePath = modulePath;
            string moduleName = Path.GetFileNameWithoutExtension(modulePath);
            watchVariablePanelParams.ClearVariables();
            manualParameterVariables.Clear();
            controlStateGetters.Clear();
            docs.Clear();
            jsonTexts.Clear();

            variables = new Dictionary<string, (string, string)>();

            ReadStateDefinition();
            InitStateGetters();
            FindAutomaticParameterGetters(moduleName, out var automaticParameterGetters);
            FindManualParameters(moduleName);
            ReadJson($"{modulePath}/configuration.json");
            InitAutomaticParameterGetters(automaticParameterGetters);

            using (new AccessScope<BruteforceTab>(this))
                InitSurface(moduleName);

            CreateRecordReferenceLua(modulePath);

            UpdateState();
            btnRun.Enabled = true;
        }

        private void CreateRecordReferenceLua(string modulePath)
        {
            void WriteFrame(StringBuilder builder)
            {
                builder.AppendLine("function RecordFrame()");

                builder.AppendLine("local v;");
                builder.AppendLine("local result = \"\";");

                var knownStateVariablesLookup = new Dictionary<string, NamedVariableCollection.IView>();
                foreach (var helpMeGodIsNotReal in knownStateVariables)
                {
                    var jsonName = helpMeGodIsNotReal.GetJsonName();
                    if (jsonName != null)
                        knownStateVariablesLookup[jsonName] = helpMeGodIsNotReal;
                }

                foreach (var variable in variables)
                {
                    if (variable.Value.modifier == "dynamic"
                        && knownStateVariablesLookup.TryGetValue(variable.Key, out var view)
                        && view is NamedVariableCollection.IMemoryDescriptorView memoryView)
                    {
                        var address = memoryView.describedMemoryState.GetAddressList().FirstOrDefault();
                        if (address == 0)
                            continue;

                        string readMethod = null;
                        string format = null;
                        switch (memoryView.memoryDescriptor.MemoryType)
                        {
                            case Type t when t == typeof(sbyte):
                                readMethod = "bytesigned";
                                format = "%d";
                                break;
                            case Type t when t == typeof(byte):
                                readMethod = "byte";
                                format = "%d";
                                break;
                            case Type t when t == typeof(short):
                                readMethod = "wordsigned";
                                format = "%d";
                                break;
                            case Type t when t == typeof(ushort):
                                readMethod = "word";
                                format = "%d";
                                break;
                            case Type t when t == typeof(int):
                                readMethod = "dwordsigned";
                                format = "%d";
                                break;
                            case Type t when t == typeof(uint):
                                readMethod = "dword";
                                format = "%d";
                                break;
                            case Type t when t == typeof(float):
                                readMethod = "float";
                                format = "%f";
                                break;
                            case Type t when t == typeof(double):
                                readMethod = "double";
                                format = "%f";
                                break;
                        }

                        if (readMethod == null)
                            continue;

                        builder.Append("-- ");
                        builder.AppendLine(memoryView.Name);
                        builder.Append("v = memory.read");
                        builder.Append(readMethod);
                        builder.Append("(0x");
                        builder.Append(address.ToString("X8"));
                        builder.AppendLine(");");
                        builder.AppendLine($"result = result .. string.format(\"\t\\\"{variable.Key}\\\": {format},\\n\", v);");
                    }
                }

                builder.AppendLine(@"result = string.format(""{\n%s\n},\n"", result:sub(1, result: len() - 2));");
                builder.AppendLine("everything = everything .. result;");
                builder.AppendLine("end");
            }

            var scriptOutputBuilder = new StringBuilder();
            scriptOutputBuilder.Append(@"
local everything = """"

function WriteOutput()
    PATH_ROOT = debug.getinfo(1).source:sub(2):match(""(.*\\)"");
    local targetFileName = PATH_ROOT..""ground_truth.json"";
    local file = io.open(targetFileName, ""w"");
    file:write(string.format(""[\n%s\n]\n"", everything:sub(1, everything: len() - 2)));
    file:close();
    print(""Results were written to "" .. targetFileName);
end
");

            WriteFrame(scriptOutputBuilder);

            scriptOutputBuilder.Append(@"
emu.atinput(RecordFrame);
emu.atstop(WriteOutput);
");

            File.WriteAllText($"{modulePath}/record_reference.lua", scriptOutputBuilder.ToString());
        }

        private void ApplyKnownState()
        {
            var strBuilder = new StringBuilder();
            foreach (var v in variables)
                if (stateGetters.TryGetValue(v.Key, out var getter))
                    strBuilder.AppendLine($"\t\"{v.Key}\": {StringUtilities.MakeJsonValue(getter())},");
            var knownState = strBuilder.ToString();
            jsonTexts["knownState"] = () => knownState;
        }

        private void ReadJson(string fileName)
        {
            if (!File.Exists(fileName))
                return;
            var rootObj = JsonNode.ParseJsonObject(File.ReadAllText(fileName));
            using (ignoreWrite.New())
                foreach (var kvp in rootObj.values)
                {
                    foreach (var targetVariable in manualParameterVariables) // If any of the controllable variables match, set them
                        if (targetVariable.GetJsonName() == kvp.Key)
                        {
                            targetVariable.value = StringUtilities.GetJsonValue(targetVariable.GetWrapperType(), kvp.Value.valueObject.ToString()) as IConvertible ?? 0;
                            goto skipNew;
                        }

                    if (!knownStateVariables.Any(view => view.GetJsonName() == kvp.Key))
                        variableKeepObjects[kvp.Key] = kvp.Value;
                    skipNew:;
                }
        }

        private void WriteJson()
        {
            if (ignoreWrite)
                return;
            txtJsonOutput.Clear();
            var strBuilder = new StringBuilder();
            strBuilder.AppendLine("{");

            strBuilder.Append(txtManualConfig.Text);

            foreach (var txt in jsonTexts)
                strBuilder.AppendLine(txt.Value());

            var str = strBuilder.ToString();
            strBuilder.Remove(strBuilder.Length - 5, 5); // Remove last ',' and linebreak
            strBuilder.AppendLine("\n}");
            txtJsonOutput.Text = $"{str.Substring(0, str.LastIndexOf(','))}\n}}";
        }

        private void SaveConfig(string fileName)
        {
            using (new AccessScope<UnmuteScoringFuncs>(new UnmuteScoringFuncs()))
                UpdateState();
            File.WriteAllText(fileName, txtJsonOutput.Text);
        }

        // TODO(Important): Attach spawned thread with a job object
        private void SpawnProcess()
        {
            var i = new ProcessStartInfo();
            i.FileName = $"{modulePath}/main.exe";
            i.Arguments = $" --file=tmp.json --outputmode=m64_and_sequence";
            i.UseShellExecute = false;
            i.WorkingDirectory = modulePath;
            i.CreateNoWindow = true;
            i.RedirectStandardOutput = true;
            i.RedirectStandardInput = true;
            i.RedirectStandardError = true;

            bfProcess = Process.Start(i);

            new System.Threading.Tasks.Task(() =>
            {
                bfProcess?.WaitForExit();
                Invoke((Action)Stop);
            }).Start();

            bfProcess.OutputDataReceived += (_, e) =>
            {
                outputLines.Enqueue(e.Data);
                if (outputLines.Count > MAX_CONSOLE_LINES)
                    outputLines.Dequeue();
                var strBuilder = new StringBuilder();
                foreach (var line in outputLines)
                    strBuilder.AppendLine(line);
                Action doThis = () =>
                {
                    txtOutput.Text = strBuilder.ToString();
                    txtOutput.Select(txtOutput.Text.Length - 1, 0);
                    txtOutput.ScrollToCaret();
                };
                txtOutput.Invoke(doThis);
            };

            bfProcess.BeginOutputReadLine();
        }

        private void Run()
        {
            btnRun.Text = "Stop";
            UpdateState();
            var configFile = $"{modulePath}/tmp.json";
            File.WriteAllText(configFile, txtJsonOutput.Text);
            SpawnProcess();
        }

        private void Stop()
        {
            if (!bfProcess?.HasExited ?? false)
                bfProcess.Kill();
            bfProcess = null;
            btnRun.Text = "Run!";
        }

        private void btnApplyKnownStates_Click(object sender, EventArgs e)
        {
            ApplyKnownState();
            UpdateState();
        }

        private void btnLoadModule_Click(object sender, EventArgs e)
        {
            var clickPosition = Cursor.Position;
            moduleStrip = new ContextMenuStrip();
            var bruteforcerModules = GetBruteforcerModulePaths().ToList();
            if (bruteforcerModules.Count == 0)
            {
                if (MessageBox.Show($"No bruteforcer modules have been found at{Environment.NewLine}" +
                                    $"\"{BRUTEFORCER_PATH}\"{Environment.NewLine}" +
                                    $"Do you want to locate your modules directory now?{Environment.NewLine}" +
                                    "(This should be the \"binaries\" directory from the sm64_bruteforcers repository)",
                        "No bruteforcer modules found",
                        MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var dlg = new CommonOpenFileDialog();
                    dlg.IsFolderPicker = true;
                    if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        BRUTEFORCER_PATH = dlg.FileName;
                        SaveBruteforcerModulePath();
                        bruteforcerModules = GetBruteforcerModulePaths().ToList();
                    }
                }
            }

            if (bruteforcerModules.Count == 0)
                moduleStrip.Items.Add(new ToolStripMenuItem("No modules available") { Enabled = false });
            else
                foreach (var bf_it in Directory.GetDirectories(BRUTEFORCER_PATH))
                    if (File.Exists($"{bf_it}/main.exe"))
                    {
                        var bf = bf_it;
                        moduleStrip.Items.AddHandlerToItem(bf.Substring(BRUTEFORCER_PATH.Length + 1), () =>
                        {
                            LoadModule(bf);
                            ChooseM64();
                        });
                    }

            moduleStrip.Show(clickPosition);
        }

        private void btnChooseM64_Click(object sender, EventArgs e) => ChooseM64();

        private void btnRun_Click(object sender, EventArgs e)
        {
            if (bfProcess == null)
                Run();
            else
                Stop();
        }

        private void btnSaveConfig_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Filter = "json files (*.json)|*.json";
            if (dlg.ShowDialog() == DialogResult.OK)
                SaveConfig(dlg.FileName);
        }

        private void btnLoadConfig_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "json files (*.json)|*.json";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                surface?.Cleanup();
                ReadJson(dlg.FileName);
                surface?.InitJson();
                UpdateState();
            }
        }
    }
}
