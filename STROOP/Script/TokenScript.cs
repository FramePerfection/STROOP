﻿using System;
using System.Collections.Generic;
using System.Linq;
using STROOP.Utilities;

namespace STROOP.Script
{
    public class TokenScript
    {
        private ScriptEngine _engine;

        private bool _isEnabled = false;
        private string _text = "";

        public TokenScript()
        {
        }

        public void SetScript(string text)
        {
            _text = text;
        }

        public void SetIsEnabled(bool isEnabled)
        {
            _isEnabled = isEnabled;
        }

        public void Update()
        {
            if (_isEnabled)
            {
                Run();
            }
        }

        public void Run()
        {
            var scriptTab = AccessScope<StroopMainForm>.content.GetTab<Tabs.ScriptTab>();
            List<(string, object)> inputData = scriptTab.watchVariablePanelScript.GetCurrentVariableNamesAndValues();
            List<string> inputItems = new List<string>();
            foreach ((string name, object value) in inputData)
            {
                string valueMark = value is string ? "\"" : "";
                inputItems.Add("\"" + name + "\":" + valueMark + value + valueMark);
            }
            string beforeLine = "var INPUT = {" + string.Join(",", inputItems) + "}; var OUTPUT = {};";
            string afterLine = @"var OUTPUT_STRING = """"; for (var OUTPUT_STRING_NAME in OUTPUT) OUTPUT_STRING += OUTPUT_STRING_NAME + ""\r\n"" + OUTPUT[OUTPUT_STRING_NAME] + ""\r\n""; OUTPUT_STRING";
            string result = GetEngine().Eval(beforeLine + "\r\n" + _text + "\r\n" + afterLine)?.ToString() ?? "";
            List<string> outputItems = result.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            for (int i = 0; i < outputItems.Count - 1; i += 2)
            {
                string name = outputItems[i];
                string value = outputItems[i + 1];
                scriptTab.watchVariablePanelScript.SetVariableValueByName(name, value);
            }
        }

        // Lazily create script engine because it breaks wine
        private ScriptEngine GetEngine()
        {
            if (_engine == null)
            {
                _engine = new ScriptEngine(ScriptEngine.ChakraClsid);
            }
            return _engine;
        }
    }
}
