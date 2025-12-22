using System;
using System.Collections.Generic;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using System.Windows.Forms;
using STROOP.Utilities;
using System.Linq;

namespace STROOP.Tabs
{
    public partial class OptionsTab : STROOPTab
    {
        readonly List<SavedSettingsConfig.SavedVariable<bool>> _savedSettingsVariables;
        readonly List<ToolStripMenuItem> _savedSettingsItemList;

        public OptionsTab()
        {
            InitializeComponent();

            if (Program.IsVisualStudioHostProcess()) return;

            _savedSettingsVariables = SavedSettingsConfig.GetBoolVariables().ToList();
            _savedSettingsItemList = new List<ToolStripMenuItem>();
            foreach (var prop_it in _savedSettingsVariables)
            {
                var prop = prop_it;
                checkedListBoxSavedSettings.Items.Add(prop.name, prop.value);
                var item = new ToolStripMenuItem(prop.name);
                item.Checked = prop.value;
                item.Click += (sender, e) => prop.value = (item.Checked = !prop.value);
                _savedSettingsItemList.Add(item);
            }

            checkedListBoxSavedSettings.ItemCheck += (sender, e) => _savedSettingsVariables[e.Index].value = (e.NewValue == CheckState.Checked);

            buttonOptionsResetSavedSettings.Click += (sender, e) => SavedSettingsConfig.ResetSavedSettings();

            // object slot overlays
            List<string> objectSlotOverlayTextList = new List<string>()
            {
                "Held Object",
                "Stood On Object",
                "Ridden Object",
                "Interaction Object",
                "Used Object",
                "Closest Object",
                "Camera Object",
                "Camera Hack Object",
                "Floor Object",
                "Wall Object",
                "Ceiling Object",
                "Collision Object",
                "Parent Object",
                "Child Object",
            };

            List<Func<bool>> objectSlotOverlayGetterList = new List<Func<bool>>()
            {
                () => OverlayConfig.ShowOverlayHeldObject,
                () => OverlayConfig.ShowOverlayStoodOnObject,
                () => OverlayConfig.ShowOverlayRiddenObject,
                () => OverlayConfig.ShowOverlayInteractionObject,
                () => OverlayConfig.ShowOverlayUsedObject,
                () => OverlayConfig.ShowOverlayClosestObject,
                () => OverlayConfig.ShowOverlayCameraObject,
                () => OverlayConfig.ShowOverlayCameraHackObject,
                () => OverlayConfig.ShowOverlayFloorObject,
                () => OverlayConfig.ShowOverlayWallObject,
                () => OverlayConfig.ShowOverlayCeilingObject,
                () => OverlayConfig.ShowOverlayCollisionObject,
                () => OverlayConfig.ShowOverlayParentObject,
                () => OverlayConfig.ShowOverlayChildObject,
            };

            List<Action<bool>> objectSlotOverlaySetterList = new List<Action<bool>>()
            {
                (bool value) => OverlayConfig.ShowOverlayHeldObject = value,
                (bool value) => OverlayConfig.ShowOverlayStoodOnObject = value,
                (bool value) => OverlayConfig.ShowOverlayRiddenObject = value,
                (bool value) => OverlayConfig.ShowOverlayInteractionObject = value,
                (bool value) => OverlayConfig.ShowOverlayUsedObject = value,
                (bool value) => OverlayConfig.ShowOverlayClosestObject = value,
                (bool value) => OverlayConfig.ShowOverlayCameraObject = value,
                (bool value) => OverlayConfig.ShowOverlayCameraHackObject = value,
                (bool value) => OverlayConfig.ShowOverlayFloorObject = value,
                (bool value) => OverlayConfig.ShowOverlayWallObject = value,
                (bool value) => OverlayConfig.ShowOverlayCeilingObject = value,
                (bool value) => OverlayConfig.ShowOverlayCollisionObject = value,
                (bool value) => OverlayConfig.ShowOverlayParentObject = value,
                (bool value) => OverlayConfig.ShowOverlayChildObject = value,
            };

            for (int i = 0; i < objectSlotOverlayTextList.Count; i++)
            {
                checkedListBoxObjectSlotOverlaysToShow.Items.Add(objectSlotOverlayTextList[i], objectSlotOverlayGetterList[i]());
            }

            checkedListBoxObjectSlotOverlaysToShow.ItemCheck += (sender, e) => { objectSlotOverlaySetterList[e.Index](e.NewValue == CheckState.Checked); };

            Action<bool> setAllObjectSlotOverlays = (bool value) =>
            {
                int specialCount = 2;
                int totalCount = checkedListBoxObjectSlotOverlaysToShow.Items.Count;
                for (int i = 0; i < totalCount - specialCount; i++)
                {
                    checkedListBoxObjectSlotOverlaysToShow.SetItemChecked(i, value);
                }
            };
            ControlUtilities.AddContextMenuStripFunctions(
                checkedListBoxObjectSlotOverlaysToShow,
                new List<string>() { "Set All On", "Set All Off" },
                new List<Action>()
                {
                    () => setAllObjectSlotOverlays(true),
                    () => setAllObjectSlotOverlays(false),
                });
        }

        public override string GetDisplayName() => "Options";

        public void AddCogContextMenu(Control cogControl)
        {
            cogControl.ContextMenuStrip = new ContextMenuStrip();
            cogControl.Click += (sender, e) => cogControl.ContextMenuStrip.Show(Cursor.Position);

            _savedSettingsItemList.ForEach(item => cogControl.ContextMenuStrip.Items.Add(item));
            cogControl.ContextMenuStrip.Items.Add(new ToolStripSeparator());


            ToolStripMenuItem resetSavedSettingsItem = new ToolStripMenuItem(buttonOptionsResetSavedSettings.Text);
            resetSavedSettingsItem.Click += (sender, e) => SavedSettingsConfig.ResetSavedSettings();

            ToolStripMenuItem goToOptionsTabItem = new ToolStripMenuItem("Go to Options Tab");
            goToOptionsTabItem.Click += (sender, e) => Config.TabControlMain.SelectedTab = Tab;

            cogControl.ContextMenuStrip.Items.Add(resetSavedSettingsItem);
            cogControl.ContextMenuStrip.Items.Add(goToOptionsTabItem);
        }

        private void textBoxGotoRetrieve_LostFocus(object sender, ref float offset, float defaultOffset)
        {
            float value;
            if (float.TryParse((sender as TextBox).Text, out value))
            {
                offset = value;
            }
            else
            {
                offset = defaultOffset;
                (sender as TextBox).Text = defaultOffset.ToString();
            }
        }

        public override void Update(bool updateView)
        {
            for (int i = 0; i < checkedListBoxSavedSettings.Items.Count; i++)
            {
                bool value = _savedSettingsVariables[i].value;
                checkedListBoxSavedSettings.SetItemChecked(i, value);
                _savedSettingsItemList[i].Checked = value;
            }

            if (!updateView) return;
            base.Update(updateView);
        }
    }
}
