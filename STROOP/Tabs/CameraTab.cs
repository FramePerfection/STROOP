﻿using System.Collections.Generic;
using System.Linq;
using STROOP.Enums;
using STROOP.Structs;
using STROOP.Utilities;
using STROOP.Structs.Configurations;

namespace STROOP.Tabs
{
    public partial class CameraTab : STROOPTab
    {
        public CameraTab()
        {
            InitializeComponent();
        }

        public override string GetDisplayName() => "Camera";

        public override void InitializeTab()
        {
            base.InitializeTab();
            ControlUtilities.InitializeThreeDimensionController(
                CoordinateSystem.Euler,
                true,
                groupBoxCameraPos,
                buttonCameraPosXn,
                buttonCameraPosXp,
                buttonCameraPosZn,
                buttonCameraPosZp,
                buttonCameraPosXnZn,
                buttonCameraPosXnZp,
                buttonCameraPosXpZn,
                buttonCameraPosXpZp,
                buttonCameraPosYp,
                buttonCameraPosYn,
                textBoxCameraPosXZ,
                textBoxCameraPosY,
                checkBoxCameraPosRelative,
                (float hOffset, float vOffset, float nOffset, bool useRelative) =>
                {
                    ButtonUtilities.TranslateCamera(
                        hOffset,
                        nOffset,
                        -1 * vOffset,
                        useRelative);
                });

            ControlUtilities.InitializeThreeDimensionController(
                CoordinateSystem.Spherical,
                false,
                groupBoxCameraSphericalPos,
                buttonCameraSphericalPosTn,
                buttonCameraSphericalPosTp,
                buttonCameraSphericalPosPn,
                buttonCameraSphericalPosPp,
                buttonCameraSphericalPosTnPn,
                buttonCameraSphericalPosTnPp,
                buttonCameraSphericalPosTpPn,
                buttonCameraSphericalPosTpPp,
                buttonCameraSphericalPosRn,
                buttonCameraSphericalPosRp,
                textBoxCameraSphericalPosTP,
                textBoxCameraSphericalPosR,
                checkBoxCameraSphericalPosPivotOnFocus,
                (float hOffset, float vOffset, float nOffset, bool pivotOnFocus) =>
                {
                    ButtonUtilities.TranslateCameraSpherically(
                        -1 * nOffset,
                        hOffset,
                        vOffset,
                        getSphericalPivotPoint(pivotOnFocus));
                });

            buttonDisableFOVFunctions.Initialize(
                "Disable FOV Functions",
                "Enable FOV Functions",
                () =>
                {
                    List<uint> addresses = CameraConfig.FovFunctionAddresses;
                    for (int i = 0; i < addresses.Count; i++)
                    {
                        Config.Stream.SetValue(0, addresses[i]);
                    }
                },
                () =>
                {
                    List<uint> addresses = CameraConfig.FovFunctionAddresses;
                    List<uint> values = CameraConfig.FovFunctionValues;
                    for (int i = 0; i < addresses.Count; i++)
                    {
                        Config.Stream.SetValue(values[i], addresses[i]);
                    }
                },
                () =>
                {
                    return CameraConfig.FovFunctionAddresses.All(
                        address => Config.Stream.GetUInt32(address) == 0);
                });

            ControlUtilities.InitializeThreeDimensionController(
                CoordinateSystem.Euler,
                true,
                groupBoxCameraFocusPos,
                buttonCameraFocusPosXn,
                buttonCameraFocusPosXp,
                buttonCameraFocusPosZn,
                buttonCameraFocusPosZp,
                buttonCameraFocusPosXnZn,
                buttonCameraFocusPosXnZp,
                buttonCameraFocusPosXpZn,
                buttonCameraFocusPosXpZp,
                buttonCameraFocusPosYp,
                buttonCameraFocusPosYn,
                textBoxCameraFocusPosXZ,
                textBoxCameraFocusPosY,
                checkBoxCameraFocusPosRelative,
                (float hOffset, float vOffset, float nOffset, bool useRelative) =>
                {
                    ButtonUtilities.TranslateCameraFocus(
                        hOffset,
                        nOffset,
                        -1 * vOffset,
                        useRelative);
                });

            ControlUtilities.InitializeThreeDimensionController(
                CoordinateSystem.Spherical,
                false,
                groupBoxCameraFocusSphericalPos,
                buttonCameraFocusSphericalPosTp,
                buttonCameraFocusSphericalPosTn,
                buttonCameraFocusSphericalPosPp,
                buttonCameraFocusSphericalPosPn,
                buttonCameraFocusSphericalPosTpPp,
                buttonCameraFocusSphericalPosTpPn,
                buttonCameraFocusSphericalPosTnPp,
                buttonCameraFocusSphericalPosTnPn,
                buttonCameraFocusSphericalPosRp,
                buttonCameraFocusSphericalPosRn,
                textBoxCameraFocusSphericalPosTP,
                textBoxCameraFocusSphericalPosR,
                null /* checkbox */,
                (float hOffset, float vOffset, float nOffset, bool useRelative) =>
                {
                    ButtonUtilities.TranslateCameraFocusSpherically(
                        nOffset,
                        -1 * hOffset,
                        vOffset);
                });

            // Camera Image
            pictureBoxCamera.Image = Config.ObjectAssociations.CameraImage.Value;
            panelCameraBorder.BackColor = Config.ObjectAssociations.CameraColor;
            pictureBoxCamera.BackColor = Config.ObjectAssociations.CameraColor.Lighten(0.5);
        }

        public override void Update(bool updateView)
        {
            if (!updateView) return;
            base.Update(updateView);

            buttonDisableFOVFunctions.UpdateButton();
        }

        private (float pivotX, float pivotY, float pivotZ) getSphericalPivotPoint(bool pivotOnFocus)
        {
            float pivotX, pivotY, pivotZ;

            if (pivotOnFocus)
            {
                pivotX = Config.Stream.GetSingle(CameraConfig.StructAddress + CameraConfig.FocusXOffset);
                pivotY = Config.Stream.GetSingle(CameraConfig.StructAddress + CameraConfig.FocusYOffset);
                pivotZ = Config.Stream.GetSingle(CameraConfig.StructAddress + CameraConfig.FocusZOffset);
            }
            else // pivot on Mario
            {
                pivotX = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.XOffset);
                pivotY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
                pivotZ = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.ZOffset);
            }
            return (pivotX, pivotY, pivotZ);
        }
    }
}
