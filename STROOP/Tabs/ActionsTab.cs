using System;
using System.Collections.Generic;
using STROOP.Structs;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Forms;
using STROOP.Variables.SM64MemoryLayout;

namespace STROOP.Tabs
{
    public partial class ActionsTab : STROOPTab
    {
        public ActionsTab()
        {
            InitializeComponent();
        }

        public override string GetDisplayName() => "Actions";

        public override void InitializeTab()
        {
            base.InitializeTab();
            textBoxActionDescription.DoubleClick += (sender, e) => SelectionForm.ShowActionDescriptionSelectionForm();
            textBoxAnimationDescription.DoubleClick += (sender, e) => SelectionForm.ShowAnimationDescriptionSelectionForm();

            ControlUtilities.AddContextMenuStripFunctions(
                textBoxActionDescription,
                new List<string>() { "Select Action", "Free Movement Action", "Open Action Form" },
                new List<Action>()
                {
                    () => SelectionForm.ShowActionDescriptionSelectionForm(),
                    () => Config.Stream.SetValue(MarioConfig.FreeMovementAction, MarioConfig.StructAddress + MarioConfig.ActionOffset),
                    () => new ActionForm().Show(),
                });

            ControlUtilities.AddContextMenuStripFunctions(
                textBoxAnimationDescription,
                new List<string>() { "Select Animation", "Replace Animation" },
                new List<Action>()
                {
                    () => SelectionForm.ShowAnimationDescriptionSelectionForm(),
                    () =>
                    {
                        int? animationToBeReplaced = SelectionForm.GetAnimation("Choose Animation to Be Replaced", "Select Animation");
                        int? animationToReplaceIt = SelectionForm.GetAnimation("Choose Animation to Replace It", "Select Animation");
                        if (animationToBeReplaced == null || animationToReplaceIt == null) return;
                        AnimationUtilities.ReplaceAnimation(animationToBeReplaced.Value, animationToReplaceIt.Value);
                    },
                });
        }

        public override void Update(bool updateView)
        {
            if (!updateView) return;
            base.Update(updateView);

            textBoxActionDescription.Text = TableConfig.MarioActions.GetActionName();
            textBoxAnimationDescription.Text = TableConfig.MarioAnimations.GetAnimationName();
        }
    }
}
