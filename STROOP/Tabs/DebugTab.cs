using STROOP.Utilities;
using System.Windows.Forms;
using STROOP.Structs.Configurations;

namespace STROOP.Tabs
{
    public partial class DebugTab : STROOPTab
    {
        RadioButton[] _advancedModeSettingRadioButtons;

        public DebugTab()
        {
            InitializeComponent();
        }

        public override string GetDisplayName() => "Debug";

        public override void InitializeTab()
        {
            base.InitializeTab();

            // Debug Image
            pictureBoxDebug.Image = Config.ObjectAssociations.DebugImage.Value;
            panelDebugBorder.BackColor = Config.ObjectAssociations.DebugColor;
            pictureBoxDebug.BackColor = Config.ObjectAssociations.DebugColor.Lighten(0.5);

            // Advanced mode

            radioButtonAdvancedModeOff.Click += (sender, e) =>
            {
                Config.Stream.SetValue((byte)0, DebugConfig.AdvancedModeAddress);
                Config.Stream.SetValue((byte)0, DebugConfig.AdvancedModeSettingAddress);
            };

            _advancedModeSettingRadioButtons = new RadioButton[]
            {
                radioButtonAdvancedModeObjectCounter,
                radioButtonAdvancedModeCheckInfo,
                radioButtonAdvancedModeMapInfo,
                radioButtonAdvancedModeStageInfo,
                radioButtonAdvancedModeEffectInfo,
                radioButtonAdvancedModeEnemyInfo
            };
            for (int i = 0; i < _advancedModeSettingRadioButtons.Length; i++)
            {
                byte localIndex = (byte)i;
                _advancedModeSettingRadioButtons[i].Click += (sender, e) =>
                {
                    Config.Stream.SetValue((byte)1, DebugConfig.AdvancedModeAddress);
                    Config.Stream.SetValue(localIndex, DebugConfig.AdvancedModeSettingAddress);
                };
            }

            // Resource meter

            radioButtonResourceMeterOff.Click += (sender, e) =>
            {
                Config.Stream.SetValue((byte)0, DebugConfig.ResourceMeterAddress);
                Config.Stream.SetValue((ushort)0, DebugConfig.ResourceMeterSettingAddress);
            };

            radioButtonResourceMeter1.Click += (sender, e) =>
            {
                Config.Stream.SetValue((byte)1, DebugConfig.ResourceMeterAddress);
                Config.Stream.SetValue((ushort)0, DebugConfig.ResourceMeterSettingAddress);
            };

            radioButtonResourceMeter2.Click += (sender, e) =>
            {
                Config.Stream.SetValue((byte)1, DebugConfig.ResourceMeterAddress);
                Config.Stream.SetValue((ushort)1, DebugConfig.ResourceMeterSettingAddress);
            };

            // Misc debug

            checkBoxClassicMode.Click += (sender, e) => { Config.Stream.SetValue(checkBoxClassicMode.Checked ? (byte)0x01 : (byte)0x00, DebugConfig.ClassicModeAddress); };

            checkBoxSpawnMode.Click += (sender, e) =>
            {
                Config.Stream.SetValue(checkBoxSpawnMode.Checked ? (byte)0x03 : (byte)0x00, DebugConfig.AdvancedModeSettingAddress);
                Config.Stream.SetValue(checkBoxSpawnMode.Checked ? (byte)0x01 : (byte)0x00, DebugConfig.SpawnModeAddress);
            };

            checkBoxStageSelect.Click += (sender, e) => { Config.Stream.SetValue(checkBoxStageSelect.Checked ? (byte)0x01 : (byte)0x00, DebugConfig.StageSelectAddress); };

            checkBoxFreeMovement.Click += (sender, e) =>
            {
                Config.Stream.SetValue(
                    checkBoxFreeMovement.Checked ? DebugConfig.FreeMovementOnValue : DebugConfig.FreeMovementOffValue,
                    DebugConfig.FreeMovementAddress);
            };
        }

        public override void Update(bool updateView)
        {
            if (!updateView) return;
            base.Update(updateView);

            // Advanced mode
            byte advancedModeOn = Config.Stream.GetByte(DebugConfig.AdvancedModeAddress);
            byte advancedModeSetting = Config.Stream.GetByte(DebugConfig.AdvancedModeSettingAddress);
            if (advancedModeOn % 2 != 0)
            {
                if (advancedModeSetting > 0 && advancedModeSetting < _advancedModeSettingRadioButtons.Length)
                    _advancedModeSettingRadioButtons[advancedModeSetting].Checked = true;
                else
                    _advancedModeSettingRadioButtons[0].Checked = true;
            }
            else
            {
                radioButtonAdvancedModeOff.Checked = true;
            }

            // Resource meter
            byte resourceMeterOn = Config.Stream.GetByte(DebugConfig.ResourceMeterAddress);
            ushort resourceMeterSetting = Config.Stream.GetUInt16(DebugConfig.ResourceMeterSettingAddress);
            if (resourceMeterOn != 0)
            {
                if (resourceMeterSetting != 0)
                    radioButtonResourceMeter2.Checked = true;
                else
                    radioButtonResourceMeter1.Checked = true;
            }
            else
            {
                radioButtonResourceMeterOff.Checked = true;
            }

            // Misc debug
            checkBoxClassicMode.Checked = Config.Stream.GetByte(DebugConfig.ClassicModeAddress) == 0x01;
            checkBoxSpawnMode.Checked = Config.Stream.GetByte(DebugConfig.AdvancedModeSettingAddress) == 0x03
                                        && Config.Stream.GetByte(DebugConfig.SpawnModeAddress) == 0x01;
            checkBoxStageSelect.Checked = Config.Stream.GetByte(DebugConfig.StageSelectAddress) == 0x01;
            checkBoxFreeMovement.Checked = Config.Stream.GetUInt16(DebugConfig.FreeMovementAddress) == DebugConfig.FreeMovementOnValue;
        }
    }
}
