using STROOP.Core;
using STROOP.Structs;
using System;
using System.Collections.Generic;
using STROOP.Structs.Configurations;
using STROOP.Variables;
using STROOP.Variables.Utilities;

namespace STROOP.Tabs
{
    public partial class MainSaveTab : STROOPTab
    {
        public enum MainSaveMode
        {
            MainSave,
            MainSaveSaved
        };

        public static uint CurrentMainSaveAddress => AccessScope<StroopMainForm>.content.GetTab<MainSaveTab>().GetMainSaveAddress();

        [InitializeBaseAddress]
        static void InitializeBaseAddress()
        {
            WatchVariableUtilities.baseAddressGetters[BaseAddressType.MainSave] = () => [CurrentMainSaveAddress];
        }

        public MainSaveMode CurrentMainSaveMode { get; private set; }

        private List<MainSaveTextbox> _mainSaveTextboxes;

        public MainSaveTab()
        {
            InitializeComponent();
        }

        public override string GetDisplayName() => "Main Save";

        public override void InitializeTab()
        {
            base.InitializeTab();

            CurrentMainSaveMode = MainSaveMode.MainSave;

            _mainSaveTextboxes = new List<MainSaveTextbox>();
            for (int row = 1; row <= 15; row++)
            {
                for (int col = 1; col <= 4; col++)
                {
                    string controlName = String.Format("textBoxMainSaveCoinRankRow{0}Col{1}", row, col);
                    MainSaveTextbox mainSaveTextbox = tableLayoutPanelMainSaveCoinRank.Controls[controlName] as MainSaveTextbox;
                    mainSaveTextbox.Initialize(row - 1, col - 1);
                    _mainSaveTextboxes.Add(mainSaveTextbox);
                }
            }

            radioButtonMainSaveStructMainSave.Click += (sender, e) => CurrentMainSaveMode = MainSaveMode.MainSave;

            radioButtonMainSaveStructMainSaveSaved.Click += (sender, e) => CurrentMainSaveMode = MainSaveMode.MainSaveSaved;


            radioButtonMainSaveSoundModeStereo.Click += (sender, e) =>
                Config.Stream.SetValue(MainSaveConfig.SoundModeStereoValue, CurrentMainSaveAddress + MainSaveConfig.SoundModeOffset);

            radioButtonMainSaveSoundModeMono.Click += (sender, e) =>
                Config.Stream.SetValue(MainSaveConfig.SoundModeMonoValue, CurrentMainSaveAddress + MainSaveConfig.SoundModeOffset);

            radioButtonMainSaveSoundModeHeadset.Click += (sender, e) =>
                Config.Stream.SetValue(MainSaveConfig.SoundModeHeadsetValue, CurrentMainSaveAddress + MainSaveConfig.SoundModeOffset);

            buttonMainSaveSave.Click += (sender, e) => Save();
        }

        public ushort GetChecksum(uint? nullableMainSaveAddress = null)
        {
            uint mainSaveAddress = nullableMainSaveAddress ?? CurrentMainSaveAddress;
            ushort checksum = (ushort)(MainSaveConfig.ChecksumConstantValue % 256 + MainSaveConfig.ChecksumConstantValue / 256);
            for (uint i = 0; i < MainSaveConfig.MainSaveStructSize - 4; i++)
            {
                byte b = Config.Stream.GetByte(mainSaveAddress + i);
                checksum += b;
            }

            return checksum;
        }

        private void Save()
        {
            ushort checksum = GetChecksum(MainSaveConfig.MainSaveAddress);

            Config.Stream.SetValue(MainSaveConfig.ChecksumConstantValue, MainSaveConfig.MainSaveAddress + MainSaveConfig.ChecksumConstantOffset);
            Config.Stream.SetValue(checksum, MainSaveConfig.MainSaveAddress + MainSaveConfig.ChecksumOffset);

            Config.Stream.SetValue(MainSaveConfig.ChecksumConstantValue, MainSaveConfig.MainSaveSavedAddress + MainSaveConfig.ChecksumConstantOffset);
            Config.Stream.SetValue(checksum, MainSaveConfig.MainSaveSavedAddress + MainSaveConfig.ChecksumOffset);

            for (int i = 0; i < MainSaveConfig.MainSaveStructSize - 4; i++)
            {
                byte b = Config.Stream.GetByte(MainSaveConfig.MainSaveAddress + (uint)i);
                Config.Stream.SetValue(b, MainSaveConfig.MainSaveSavedAddress + (uint)i);
            }
        }

        private uint GetMainSaveAddress(MainSaveMode? nullableMode = null)
        {
            MainSaveMode mode = nullableMode ?? CurrentMainSaveMode;
            switch (mode)
            {
                case MainSaveMode.MainSave:
                    return MainSaveConfig.MainSaveAddress;
                case MainSaveMode.MainSaveSaved:
                    return MainSaveConfig.MainSaveSavedAddress;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void Update(bool updateView)
        {
            if (!updateView) return;

            foreach (MainSaveTextbox mainSaveTextbox in _mainSaveTextboxes)
            {
                mainSaveTextbox.UpdateText();
            }

            ushort soundModeValue = Config.Stream.GetUInt16(CurrentMainSaveAddress + MainSaveConfig.SoundModeOffset);
            radioButtonMainSaveSoundModeStereo.Checked = soundModeValue == 0;
            radioButtonMainSaveSoundModeMono.Checked = soundModeValue == 1;
            radioButtonMainSaveSoundModeHeadset.Checked = soundModeValue == 2;

            base.Update(updateView);
        }
    }
}
