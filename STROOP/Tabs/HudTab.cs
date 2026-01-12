using System;
using System.Collections.Generic;
using STROOP.Structs;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Variables.SM64MemoryLayout;

namespace STROOP.Tabs
{
    public partial class HudTab : STROOPTab
    {
        public HudTab()
        {
            InitializeComponent();
        }

        public override string GetDisplayName() => "HUD";

        public override void InitializeTab()
        {
            base.InitializeTab();

            // Hud Image
            pictureBoxHud.Image = Config.ObjectAssociations.HudImage.Value;
            panelHudBorder.BackColor = Config.ObjectAssociations.HudColor;
            pictureBoxHud.BackColor = Config.ObjectAssociations.HudColor.Lighten(0.5);

            buttonFullHp.Click += (sender, e) => ButtonUtilities.FullHp();
            buttonDie.Click += (sender, e) => ButtonUtilities.Die();
            buttonGameOver.Click += (sender, e) => ButtonUtilities.GameOver();
            button99Coins.Click += (sender, e) => ButtonUtilities.Coins99();
            button100CoinStar.Click += (sender, e) => ButtonUtilities.CoinStar100();
            button100Lives.Click += (sender, e) => ButtonUtilities.Lives100();
            buttonStandardHud.Click += (sender, e) => ButtonUtilities.StandardHud();

            buttonTurnOnOffHud.Initialize(
                "Turn Off HUD",
                "Turn On HUD",
                () => ButtonUtilities.SetHudVisibility(false),
                () => ButtonUtilities.SetHudVisibility(true),
                () => (Config.Stream.GetByte(MarioConfig.StructAddress + HudConfig.VisibilityOffset) & HudConfig.VisibilityMask) == 0);

            ControlUtilities.AddContextMenuStripFunctions(
                buttonTurnOnOffHud,
                new List<string>()
                {
                    "Disable HUD by Changing Level Index",
                    "Enable HUD by Changing Level Index",
                    "Disable HUD by Removing Function",
                    "Enable HUD by Removing Function",
                },
                new List<Action>()
                {
                    () => ButtonUtilities.SetHudVisibility(false, true),
                    () => ButtonUtilities.SetHudVisibility(true, true),
                    () => ButtonUtilities.SetHudVisibility(false, false),
                    () => ButtonUtilities.SetHudVisibility(true, false),
                });
        }

        public override void Update(bool updateView)
        {
            if (checkBoxFullHP.Checked)
            {
                ButtonUtilities.FullHp();
            }

            if (!updateView) return;

            buttonTurnOnOffHud.UpdateButton();

            base.Update(updateView);
        }
    }
}
