using STROOP.Core;
using System;
using System.Collections.Generic;
using STROOP.Structs;
using System.Windows.Forms;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Variables;
using STROOP.Variables.SM64MemoryLayout;
using STROOP.Variables.Utilities;

namespace STROOP.Tabs
{
    public partial class MarioTab : STROOPTab
    {
        [InitializeBaseAddress]
        static void InitBaseAddresses()
        {
            WatchVariableUtilities.baseAddressGetters[BaseAddressType.Mario] = () => new List<uint> { MarioConfig.StructAddress };
            WatchVariableUtilities.baseAddressGetters[BaseAddressType.MarioObj] = () => new List<uint>
            {
                ProcessStream.Instance.GetUInt32(MarioObjectConfig.PointerAddress)
            };

            WatchVariableUtilities.baseAddressGetters["Floor"] = () =>
            {
                uint floorAddress = Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.FloorTriangleOffset);
                return floorAddress != 0 ? new List<uint>() { floorAddress } : WatchVariableUtilities.BaseAddressListEmpty;
            };
            WatchVariableUtilities.baseAddressGetters["Wall"] = () =>
            {
                uint wallAddress = Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.WallTriangleOffset);
                return wallAddress != 0 ? new List<uint>() { wallAddress } : WatchVariableUtilities.BaseAddressListEmpty;
            };
            WatchVariableUtilities.baseAddressGetters["Ceiling"] = () =>
            {
                uint ceilingAddress = Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.CeilingTriangleOffset);
                return ceilingAddress != 0 ? new List<uint>() { ceilingAddress } : WatchVariableUtilities.BaseAddressListEmpty;
            };
        }

        private static readonly List<string> ALL_VAR_GROUPS =
            new List<string>()
            {
                VariableGroup.Basic,
                VariableGroup.Intermediate,
                VariableGroup.Advanced,
                VariableGroup.HolpMario,
                VariableGroup.HolpPoint,
                VariableGroup.Trajectory,
                VariableGroup.Hacks,
                VariableGroup.Custom,
            };

        private static readonly List<string> VISIBLE_VAR_GROUPS =
            new List<string>()
            {
                VariableGroup.Basic,
                VariableGroup.Intermediate,
                VariableGroup.Custom,
            };

        public MarioTab()
        {
            InitializeComponent();
            watchVariablePanelMario.SetGroups(ALL_VAR_GROUPS, VISIBLE_VAR_GROUPS);
        }

        public override string GetDisplayName() => "Mario";

        public override void InitializeTab()
        {
            base.InitializeTab();
            // Mario Image
            pictureBoxMario.Image = Config.ObjectAssociations.MarioImage.Value;
            panelMarioBorder.BackColor = Config.ObjectAssociations.MarioColor;
            pictureBoxMario.BackColor = Config.ObjectAssociations.MarioColor.Lighten(0.5);

            watchVariablePanelMario.SetGroups(ALL_VAR_GROUPS, VISIBLE_VAR_GROUPS);

            buttonMarioToggleHandsfree.Click += (sender, e) => ButtonUtilities.ToggleHandsfree();

            buttonMarioVisibility.Click += (sender, e) => ButtonUtilities.ToggleVisibility();

            ControlUtilities.InitializeThreeDimensionController(
                CoordinateSystem.Euler,
                true,
                groupBoxMarioPos,
                "MarioPos",
                (float hOffset, float vOffset, float nOffset, bool useRelative) =>
                {
                    ButtonUtilities.TranslateMario(
                        hOffset,
                        nOffset,
                        -1 * vOffset,
                        useRelative);
                });

            ControlUtilities.InitializeScalarController(
                buttonMarioStatsYawN,
                buttonMarioStatsYawP,
                textBoxMarioStatsYaw,
                (float value) => { ButtonUtilities.MarioChangeYaw((int)Math.Round(value)); });
            ControlUtilities.InitializeScalarController(
                buttonMarioStatsHspdN,
                buttonMarioStatsHspdP,
                textBoxMarioStatsHspd,
                (float value) => { ButtonUtilities.MarioChangeHspd(value); });
            ControlUtilities.InitializeScalarController(
                buttonMarioStatsVspdN,
                buttonMarioStatsVspdP,
                textBoxMarioStatsVspd,
                (float value) => { ButtonUtilities.MarioChangeVspd(value); });

            var marioSlidingSpeedGroupBox = splitContainerMario.Panel1.Controls["groupBoxMarioSlidingSpeed"] as GroupBox;
            ControlUtilities.InitializeScalarController(
                buttonMarioSlidingSpeedXn,
                buttonMarioSlidingSpeedXp,
                textBoxMarioSlidingSpeedX,
                (float value) => { ButtonUtilities.MarioChangeSlidingSpeedX(value); });
            ControlUtilities.InitializeScalarController(
                buttonMarioSlidingSpeedZn,
                buttonMarioSlidingSpeedZp,
                textBoxMarioSlidingSpeedZ,
                (float value) => { ButtonUtilities.MarioChangeSlidingSpeedZ(value); });
            ControlUtilities.InitializeScalarController(
                buttonMarioSlidingSpeedHn,
                buttonMarioSlidingSpeedHp,
                textBoxMarioSlidingSpeedH,
                (float value) => { ButtonUtilities.MarioChangeSlidingSpeedH(value); });
            ControlUtilities.InitializeScalarController(
                buttonMarioSlidingSpeedYawN,
                buttonMarioSlidingSpeedYawP,
                textBoxMarioSlidingSpeedYaw,
                (float value) => { ButtonUtilities.MarioChangeSlidingSpeedYaw(value); });

            buttonMarioHOLPGoto.Click += (sender, e) => ButtonUtilities.GotoHOLP();
            ControlUtilities.AddContextMenuStripFunctions(
                buttonMarioHOLPGoto,
                new List<string>() { "Goto HOLP", "Goto HOLP Laterally", "Goto HOLP X", "Goto HOLP Y", "Goto HOLP Z" },
                new List<Action>()
                {
                    () => ButtonUtilities.GotoHOLP((true, true, true)),
                    () => ButtonUtilities.GotoHOLP((true, false, true)),
                    () => ButtonUtilities.GotoHOLP((true, false, false)),
                    () => ButtonUtilities.GotoHOLP((false, true, false)),
                    () => ButtonUtilities.GotoHOLP((false, false, true)),
                });

            buttonMarioHOLPRetrieve.Click += (sender, e) => ButtonUtilities.RetrieveHOLP();
            ControlUtilities.AddContextMenuStripFunctions(
                buttonMarioHOLPRetrieve,
                new List<string>() { "Retrieve HOLP", "Retrieve HOLP Laterally", "Retrieve HOLP X", "Retrieve HOLP Y", "Retrieve HOLP Z" },
                new List<Action>()
                {
                    () => ButtonUtilities.RetrieveHOLP((true, true, true)),
                    () => ButtonUtilities.RetrieveHOLP((true, false, true)),
                    () => ButtonUtilities.RetrieveHOLP((true, false, false)),
                    () => ButtonUtilities.RetrieveHOLP((false, true, false)),
                    () => ButtonUtilities.RetrieveHOLP((false, false, true)),
                });

            ControlUtilities.InitializeThreeDimensionController(
                CoordinateSystem.Euler,
                true,
                groupBoxMarioHOLP,
                "MarioHOLP",
                (float hOffset, float vOffset, float nOffset, bool useRelative) =>
                {
                    ButtonUtilities.TranslateHOLP(
                        hOffset,
                        nOffset,
                        -1 * vOffset,
                        useRelative);
                });
        }
    }
}
