﻿using STROOP.Forms;
using STROOP.Managers;
 using STROOP.Utilities;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using STROOP.Tabs.MapTab;

namespace STROOP.Structs.Configurations
{
    public static class Config
    {
        public static uint RamSize
        {
            get => (uint)(SavedSettingsConfig.UseExpandedRamSize ? 0x800000 : 0x400000);
        }

        public static List<Emulator> Emulators = new List<Emulator>();
        public static ProcessStream Stream;
        public static FileImageGui FileImageGui = new FileImageGui();
        public static ObjectAssociations ObjectAssociations;
        public static StroopMainForm StroopMainForm => AccessScope<StroopMainForm>.content;
        public static TabControlEx TabControlMain;
        public static Label DebugText;
        
        public static ObjectSlotsManager ObjectSlotsManager => StroopMainForm.ObjectSlotsManager;
        public static InjectionManager InjectionManager;

        public static void Print(object formatNullable = null, params object[] args)
        {
            object format = formatNullable ?? "";
            string formatted = String.Format(format.ToString(), args);
            System.Diagnostics.Trace.WriteLine(formatted);
        }

        public static void SetDebugText(object obj)
        {
            DebugText.Visible = true;
            DebugText.Text = obj.ToString();
        }
    }
}
