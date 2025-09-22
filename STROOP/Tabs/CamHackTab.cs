using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using STROOP.Controls.VariablePanel;
using STROOP.Core.Variables;
using STROOP.Models;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using STROOP.Variables.SM64MemoryLayout;

namespace STROOP.Tabs
{
    public partial class CamHackTab : STROOPTab
    {
        [InitializeBaseAddress]
        static void InitBaseAddresses()
        {
            WatchVariableUtilities.baseAddressGetters["CamHack"] = () => new List<uint> { CamHackConfig.StructAddress };
        }

        public CamHackMode CurrentCamHackMode { get; private set; }

        private int _numPans = 0;
        private List<IEnumerable<WatchVariableControl>> _panVars = new List<IEnumerable<WatchVariableControl>>();

        public CamHackTab()
        {
            InitializeComponent();
        }

        public override string GetDisplayName() => "Cam Hack";

        public override void InitializeTab()
        {
            base.InitializeTab();
            CurrentCamHackMode = CamHackMode.REGULAR;

            ControlUtilities.AddContextMenuStripFunctions(
                labelCamHackMode,
                new List<string>() { "Download Camera Hack ROM" },
                new List<Action>()
                {
                    () => System.Diagnostics.Process.Start("http://download1436.mediafire.com/t3unklq170ag/hdd377v5794u319/Camera+Hack+ROM.z64"),
                });


            radioButtonCamHackMode0.Click += (sender, e) => Config.Stream.SetValue(0, CamHackConfig.StructAddress + CamHackConfig.CameraModeOffset);
            radioButtonCamHackMode1RelativeAngle.Click += (sender, e) =>
            {
                Config.Stream.SetValue(1, CamHackConfig.StructAddress + CamHackConfig.CameraModeOffset);
                Config.Stream.SetValue((ushort)0, CamHackConfig.StructAddress + CamHackConfig.AbsoluteAngleOffset);
            };
            radioButtonCamHackMode1AbsoluteAngle.Click += (sender, e) =>
            {
                Config.Stream.SetValue(1, CamHackConfig.StructAddress + CamHackConfig.CameraModeOffset);
                Config.Stream.SetValue((ushort)1, CamHackConfig.StructAddress + CamHackConfig.AbsoluteAngleOffset);
            };
            radioButtonCamHackMode2.Click += (sender, e) => Config.Stream.SetValue(2, CamHackConfig.StructAddress + CamHackConfig.CameraModeOffset);
            radioButtonCamHackMode3.Click += (sender, e) => Config.Stream.SetValue(3, CamHackConfig.StructAddress + CamHackConfig.CameraModeOffset);

            ControlUtilities.InitializeThreeDimensionController(
                CoordinateSystem.Euler,
                true,
                groupBoxCameraHackPos,
                buttonCameraHackPosXn,
                buttonCameraHackPosXp,
                buttonCameraHackPosZn,
                buttonCameraHackPosZp,
                buttonCameraHackPosXnZn,
                buttonCameraHackPosXnZp,
                buttonCameraHackPosXpZn,
                buttonCameraHackPosXpZp,
                buttonCameraHackPosYp,
                buttonCameraHackPosYn,
                textBoxCameraHackPosXZ,
                textBoxCameraHackPosY,
                checkBoxCameraHackPosRelative,
                (float hOffset, float vOffset, float nOffset, bool useRelative) =>
                {
                    ButtonUtilities.TranslateCameraHack(
                        CurrentCamHackMode,
                        hOffset,
                        nOffset,
                        -1 * vOffset,
                        useRelative);
                });

            ControlUtilities.InitializeThreeDimensionController(
                CoordinateSystem.Spherical,
                false,
                groupBoxCameraHackSphericalPos,
                buttonCameraHackSphericalPosTn,
                buttonCameraHackSphericalPosTp,
                buttonCameraHackSphericalPosPn,
                buttonCameraHackSphericalPosPp,
                buttonCameraHackSphericalPosTnPn,
                buttonCameraHackSphericalPosTnPp,
                buttonCameraHackSphericalPosTpPn,
                buttonCameraHackSphericalPosTpPp,
                buttonCameraHackSphericalPosRn,
                buttonCameraHackSphericalPosRp,
                textBoxCameraHackSphericalPosTP,
                textBoxCameraHackSphericalPosR,
                null /* checkbox */,
                (float hOffset, float vOffset, float nOffset, bool _) =>
                {
                    ButtonUtilities.TranslateCameraHackSpherically(
                        CurrentCamHackMode,
                        -1 * nOffset,
                        hOffset,
                        vOffset);
                });

            ControlUtilities.InitializeThreeDimensionController(
                CoordinateSystem.Euler,
                true,
                groupBoxCameraHackFocusPos,
                buttonCameraHackFocusPosXn,
                buttonCameraHackFocusPosXp,
                buttonCameraHackFocusPosZn,
                buttonCameraHackFocusPosZp,
                buttonCameraHackFocusPosXnZn,
                buttonCameraHackFocusPosXnZp,
                buttonCameraHackFocusPosXpZn,
                buttonCameraHackFocusPosXpZp,
                buttonCameraHackFocusPosYp,
                buttonCameraHackFocusPosYn,
                textBoxCameraHackFocusPosXZ,
                textBoxCameraHackFocusPosY,
                checkBoxCameraHackFocusPosRelative,
                (float hOffset, float vOffset, float nOffset, bool useRelative) =>
                {
                    ButtonUtilities.TranslateCameraHackFocus(
                        CurrentCamHackMode,
                        hOffset,
                        nOffset,
                        -1 * vOffset,
                        useRelative);
                });

            ControlUtilities.InitializeThreeDimensionController(
                CoordinateSystem.Spherical,
                false,
                groupBoxCameraHackSphericalFocusPos,
                buttonCameraHackSphericalFocusPosTn,
                buttonCameraHackSphericalFocusPosTp,
                buttonCameraHackSphericalFocusPosPp,
                buttonCameraHackSphericalFocusPosPn,
                buttonCameraHackSphericalFocusPosTnPp,
                buttonCameraHackSphericalFocusPosTnPn,
                buttonCameraHackSphericalFocusPosTpPp,
                buttonCameraHackSphericalFocusPosTpPn,
                buttonCameraHackSphericalFocusPosRp,
                buttonCameraHackSphericalFocusPosRn,
                textBoxCameraHackSphericalFocusPosTP,
                textBoxCameraHackSphericalFocusPosR,
                null /* checkbox */,
                (float hOffset, float vOffset, float nOffset, bool _) =>
                {
                    ButtonUtilities.TranslateCameraHackFocusSpherically(
                        CurrentCamHackMode,
                        nOffset,
                        hOffset,
                        vOffset);
                });

            var cameraHackBothPosGroupBox = splitContainerCamHack.Panel1.Controls["groupBoxCameraHackBothPos"] as GroupBox;
            ControlUtilities.InitializeThreeDimensionController(
                CoordinateSystem.Euler,
                true,
                cameraHackBothPosGroupBox,
                buttonCameraHackBothPosXn,
                buttonCameraHackBothPosXp,
                buttonCameraHackBothPosZn,
                buttonCameraHackBothPosZp,
                buttonCameraHackBothPosXnZn,
                buttonCameraHackBothPosXnZp,
                buttonCameraHackBothPosXpZn,
                buttonCameraHackBothPosXpZp,
                buttonCameraHackBothPosYp,
                buttonCameraHackBothPosYn,
                textBoxCameraHackBothPosXZ,
                textBoxCameraHackBothPosY,
                checkBoxCameraHackBothPosRelative,
                (float hOffset, float vOffset, float nOffset, bool useRelative) =>
                {
                    ButtonUtilities.TranslateCameraHackBoth(
                        CurrentCamHackMode,
                        hOffset,
                        nOffset,
                        -1 * vOffset,
                        useRelative);
                });
        }

        public override HashSet<uint> selection => null;

        public override Action<IEnumerable<ObjectSlot>> objectSlotsClicked => objs =>
        {
            var selectedSlot = objs.Last();
            uint currentCamHackSlot = Config.Stream.GetUInt32(CamHackConfig.StructAddress + CamHackConfig.ObjectOffset);
            uint newCamHackSlot = currentCamHackSlot == selectedSlot.CurrentObject.Address
                ? 0
                : selectedSlot.CurrentObject.Address;
            Config.Stream.SetValue(newCamHackSlot, CamHackConfig.StructAddress + CamHackConfig.ObjectOffset);
        };

        public override void Update(bool updateView)
        {
            UpdatePanning();

            if (!updateView) return;
            base.Update(updateView);

            CamHackMode correctCamHackMode = getCorrectCamHackMode();
            if (CurrentCamHackMode != correctCamHackMode)
            {
                CurrentCamHackMode = correctCamHackMode;
                getCorrespondingRadioButton(correctCamHackMode).Checked = true;
            }
        }

        public void NotifyNumPanChange(int numPans)
        {
            if (numPans > _numPans) // Need to add vars
            {
                for (int i = _numPans; i < numPans; i++)
                {
                    SpecialConfig.PanModels.Add(new PanModel());
                    _panVars.Add(watchVariablePanelCamHack.AddVariables(CreatePanVars(i)));
                }
            }

            if (numPans < _numPans) // Need to remove vars
            {
                for (int i = _numPans - 1; i >= numPans; i--)
                {
                    SpecialConfig.PanModels.RemoveAt(i);
                    var panVars = _panVars[i];
                    _panVars.Remove(panVars);
                    watchVariablePanelCamHack.RemoveVariables(panVars);
                }
            }

            _numPans = numPans;
        }

        private NamedVariableCollection.IView CreatePanVar(
            string name,
            string specialType,
            string color,
            string subclass = null,
            string coord = null,
            string display = null,
            string yaw = null)
        {
            XElement xElement = new XElement("Data", name);
            xElement.Add(new XAttribute("base", "None"));
            xElement.Add(new XAttribute("specialType", specialType));
            xElement.Add(new XAttribute("color", color));
            if (subclass != null) xElement.Add(new XAttribute("subclass", subclass));
            if (coord != null) xElement.Add(new XAttribute("coord", coord));
            if (display != null) xElement.Add(new XAttribute("display", display));
            if (yaw != null) xElement.Add(new XAttribute("yaw", yaw));
            return NamedVariableCollection.ParseXml(xElement);
        }

        private List<NamedVariableCollection.IView> CreatePanVars(int index)
        {
            WatchVariableSpecialUtilities.AddPanEntriesToDictionary(index);
            return new List<NamedVariableCollection.IView>
            {
                CreatePanVar("Global Timer", String.Format("Pan{0}GlobalTimer", index), "Orange"),
                CreatePanVar(String.Format("Pan{0} Start Time", index), String.Format("Pan{0}StartTime", index), "Orange"),
                CreatePanVar(String.Format("Pan{0} End Time", index), String.Format("Pan{0}EndTime", index), "Orange"),
                CreatePanVar(String.Format("Pan{0} Duration", index), String.Format("Pan{0}Duration", index), "Orange"),

                CreatePanVar(String.Format("Pan{0} Ease Start", index), String.Format("Pan{0}EaseStart", index), "LightBlue", subclass: "Boolean"),
                CreatePanVar(String.Format("Pan{0} Ease End", index), String.Format("Pan{0}EaseEnd", index), "LightBlue", subclass: "Boolean"),
                CreatePanVar(String.Format("Pan{0} Ease Degree", index), String.Format("Pan{0}EaseDegree", index), "LightBlue"),

                CreatePanVar(String.Format("Pan{0} Rotate CW", index), String.Format("Pan{0}RotateCW", index), "Yellow", subclass: "Boolean"),

                CreatePanVar(String.Format("Pan{0} Cam Start X", index), String.Format("Pan{0}CamStartX", index), "Green", coord: "X"),
                CreatePanVar(String.Format("Pan{0} Cam Start Y", index), String.Format("Pan{0}CamStartY", index), "Green", coord: "Y"),
                CreatePanVar(String.Format("Pan{0} Cam Start Z", index), String.Format("Pan{0}CamStartZ", index), "Green", coord: "Z"),
                CreatePanVar(String.Format("Pan{0} Cam Start Yaw", index), String.Format("Pan{0}CamStartYaw", index), "Green", subclass: "Angle", display: "ushort", yaw: "true"),
                CreatePanVar(String.Format("Pan{0} Cam Start Pitch", index), String.Format("Pan{0}CamStartPitch", index), "Green", subclass: "Angle", display: "short"),

                CreatePanVar(String.Format("Pan{0} Cam End X", index), String.Format("Pan{0}CamEndX", index), "Red", coord: "X"),
                CreatePanVar(String.Format("Pan{0} Cam End Y", index), String.Format("Pan{0}CamEndY", index), "Red", coord: "Y"),
                CreatePanVar(String.Format("Pan{0} Cam End Z", index), String.Format("Pan{0}CamEndZ", index), "Red", coord: "Z"),
                CreatePanVar(String.Format("Pan{0} Cam End Yaw", index), String.Format("Pan{0}CamEndYaw", index), "Red", subclass: "Angle", display: "ushort", yaw: "true"),
                CreatePanVar(String.Format("Pan{0} Cam End Pitch", index), String.Format("Pan{0}CamEndPitch", index), "Red", subclass: "Angle", display: "short"),

                CreatePanVar(String.Format("Pan{0} Cam Radius Start", index), String.Format("Pan{0}RadiusStart", index), "Blue"),
                CreatePanVar(String.Format("Pan{0} Cam Radius End", index), String.Format("Pan{0}RadiusEnd", index), "Blue"),

                CreatePanVar(String.Format("Pan{0} FOV Start", index), String.Format("Pan{0}FOVStart", index), "Pink"),
                CreatePanVar(String.Format("Pan{0} FOV End", index), String.Format("Pan{0}FOVEnd", index), "Pink"),
            };
        }

        public void UpdatePanning()
        {
            // Short circuit the logic if panning is disabled
            if (SpecialConfig.PanCamPos == 0 &&
                SpecialConfig.PanCamAngle == 0 &&
                SpecialConfig.PanCamRotation == 0 &&
                SpecialConfig.PanFOV == 0) return;

            using (Config.Stream.Suspend())
            {
                int panIndex = (int)SpecialConfig.CurrentPan;
                if (panIndex == -1)
                    return;
                PanModel panModel = SpecialConfig.PanModels[panIndex];

                uint globalTimer = Config.Stream.GetUInt32(MiscConfig.GlobalTimerAddress);
                double camX = Config.Stream.GetSingle(CamHackConfig.StructAddress + CamHackConfig.CameraXOffset);
                double camY = Config.Stream.GetSingle(CamHackConfig.StructAddress + CamHackConfig.CameraYOffset);
                double camZ = Config.Stream.GetSingle(CamHackConfig.StructAddress + CamHackConfig.CameraZOffset);

                if (SpecialConfig.PanCamPos != 0)
                {
                    if (globalTimer <= panModel.PanStartTime)
                    {
                        Config.Stream.SetValue((float)panModel.PanCamStartX, CamHackConfig.StructAddress + CamHackConfig.CameraXOffset);
                        Config.Stream.SetValue((float)panModel.PanCamStartY, CamHackConfig.StructAddress + CamHackConfig.CameraYOffset);
                        Config.Stream.SetValue((float)panModel.PanCamStartZ, CamHackConfig.StructAddress + CamHackConfig.CameraZOffset);
                    }
                    else if (globalTimer >= panModel.PanEndTime)
                    {
                        Config.Stream.SetValue((float)panModel.PanCamEndX, CamHackConfig.StructAddress + CamHackConfig.CameraXOffset);
                        Config.Stream.SetValue((float)panModel.PanCamEndY, CamHackConfig.StructAddress + CamHackConfig.CameraYOffset);
                        Config.Stream.SetValue((float)panModel.PanCamEndZ, CamHackConfig.StructAddress + CamHackConfig.CameraZOffset);
                    }
                    else
                    {
                        double proportion = (globalTimer - panModel.PanStartTime) / (panModel.PanEndTime - panModel.PanStartTime);
                        proportion = EasingUtilities.Ease(panModel.PanEaseDegree, proportion, panModel.PanEaseStart != 0, panModel.PanEaseEnd != 0);
                        camX = panModel.PanCamStartX + proportion * (panModel.PanCamEndX - panModel.PanCamStartX);
                        camY = panModel.PanCamStartY + proportion * (panModel.PanCamEndY - panModel.PanCamStartY);
                        camZ = panModel.PanCamStartZ + proportion * (panModel.PanCamEndZ - panModel.PanCamStartZ);
                        Config.Stream.SetValue((float)camX, CamHackConfig.StructAddress + CamHackConfig.CameraXOffset);
                        Config.Stream.SetValue((float)camY, CamHackConfig.StructAddress + CamHackConfig.CameraYOffset);
                        Config.Stream.SetValue((float)camZ, CamHackConfig.StructAddress + CamHackConfig.CameraZOffset);
                    }
                }

                if (SpecialConfig.PanCamAngle != 0)
                {
                    double camYaw;
                    double camPitch;

                    if (globalTimer <= panModel.PanStartTime)
                    {
                        camYaw = panModel.PanCamStartYaw;
                        camPitch = panModel.PanCamStartPitch;
                    }
                    else if (globalTimer >= panModel.PanEndTime)
                    {
                        camYaw = panModel.PanCamEndYaw;
                        camPitch = panModel.PanCamEndPitch;
                    }
                    else
                    {
                        double proportion = (globalTimer - panModel.PanStartTime) / (panModel.PanEndTime - panModel.PanStartTime);
                        proportion = EasingUtilities.Ease(panModel.PanEaseDegree, proportion, panModel.PanEaseStart != 0, panModel.PanEaseEnd != 0);

                        double yawDist = MoreMath.GetUnsignedAngleDifference(panModel.PanCamStartYaw, panModel.PanCamEndYaw);
                        if (panModel.PanRotateCW != 0 && yawDist != 0) yawDist -= 65536;
                        camYaw = panModel.PanCamStartYaw + proportion * yawDist;
                        camYaw = MoreMath.NormalizeAngleDouble(camYaw);

                        double pitchDist = panModel.PanCamEndPitch - panModel.PanCamStartPitch;
                        camPitch = panModel.PanCamStartPitch + proportion * pitchDist;
                    }

                    (double diffX, double diffY, double diffZ) = MoreMath.SphericalToEuler_AngleUnits(1000, camYaw, camPitch);
                    (double focusX, double focusY, double focusZ) = (camX + diffX, camY + diffY, camZ + diffZ);
                    Config.Stream.SetValue((float)focusX, CamHackConfig.StructAddress + CamHackConfig.FocusXOffset);
                    Config.Stream.SetValue((float)focusY, CamHackConfig.StructAddress + CamHackConfig.FocusYOffset);
                    Config.Stream.SetValue((float)focusZ, CamHackConfig.StructAddress + CamHackConfig.FocusZOffset);
                }

                if (SpecialConfig.PanCamRotation != 0)
                {
                    double radius;
                    double camYaw;
                    double camPitch;

                    if (globalTimer <= panModel.PanStartTime)
                    {
                        radius = panModel.PanRadiusStart;
                        camYaw = panModel.PanCamStartYaw;
                        camPitch = panModel.PanCamStartPitch;
                    }
                    else if (globalTimer >= panModel.PanEndTime)
                    {
                        radius = panModel.PanRadiusEnd;
                        camYaw = panModel.PanCamEndYaw;
                        camPitch = panModel.PanCamEndPitch;
                    }
                    else
                    {
                        double proportion = (globalTimer - panModel.PanStartTime) / (panModel.PanEndTime - panModel.PanStartTime);
                        proportion = EasingUtilities.Ease(panModel.PanEaseDegree, proportion, panModel.PanEaseStart != 0, panModel.PanEaseEnd != 0);

                        double radiusDist = panModel.PanRadiusEnd - panModel.PanRadiusStart;
                        radius = panModel.PanRadiusStart + proportion * radiusDist;

                        double yawDist = MoreMath.GetUnsignedAngleDifference(panModel.PanCamStartYaw, panModel.PanCamEndYaw);
                        if (panModel.PanRotateCW != 0 && yawDist != 0) yawDist -= 65536;
                        camYaw = panModel.PanCamStartYaw + proportion * yawDist;
                        camYaw = MoreMath.NormalizeAngleDouble(camYaw);

                        double pitchDist = panModel.PanCamEndPitch - panModel.PanCamStartPitch;
                        camPitch = panModel.PanCamStartPitch + proportion * pitchDist;
                    }

                    (double offsetX, double offsetY, double offsetZ) = MoreMath.SphericalToEuler_AngleUnits(radius, camYaw, -1 * camPitch);
                    (double radius2D, double angle, double height) = MoreMath.EulerToCylindrical_AngleUnits(offsetX, offsetY, offsetZ);
                    Config.Stream.SetValue((float)radius2D, CamHackConfig.StructAddress + CamHackConfig.RadiusOffset);
                    Config.Stream.SetValue(MoreMath.NormalizeAngleUshort(angle), CamHackConfig.StructAddress + CamHackConfig.ThetaOffset);
                    Config.Stream.SetValue((float)height, CamHackConfig.StructAddress + CamHackConfig.RelativeHeightOffset);
                }

                if (SpecialConfig.PanFOV != 0)
                {
                    if (globalTimer <= panModel.PanStartTime)
                    {
                        Config.Stream.SetValue((float)panModel.PanFOVStart, CameraConfig.FOVStructAddress + CameraConfig.FOVValueOffset);
                    }
                    else if (globalTimer >= panModel.PanEndTime)
                    {
                        Config.Stream.SetValue((float)panModel.PanFOVEnd, CameraConfig.FOVStructAddress + CameraConfig.FOVValueOffset);
                    }
                    else
                    {
                        double proportion = (globalTimer - panModel.PanStartTime) / (panModel.PanEndTime - panModel.PanStartTime);
                        proportion = EasingUtilities.Ease(panModel.PanEaseDegree, proportion, panModel.PanEaseStart != 0, panModel.PanEaseEnd != 0);
                        double fov = panModel.PanFOVStart + proportion * (panModel.PanFOVEnd - panModel.PanFOVStart);
                        Config.Stream.SetValue((float)fov, CameraConfig.FOVStructAddress + CameraConfig.FOVValueOffset);
                    }
                }
            }
        }

        private CamHackMode getCorrectCamHackMode()
        {
            int cameraMode = Config.Stream.GetInt32(CamHackConfig.StructAddress + CamHackConfig.CameraModeOffset);
            ushort absoluteAngle = Config.Stream.GetUInt16(CamHackConfig.StructAddress + CamHackConfig.AbsoluteAngleOffset);
            return cameraMode == 1 && absoluteAngle == 0 ? CamHackMode.RELATIVE_ANGLE :
                cameraMode == 1 ? CamHackMode.ABSOLUTE_ANGLE :
                cameraMode == 2 ? CamHackMode.FIXED_POS :
                cameraMode == 3 ? CamHackMode.FIXED_ORIENTATION : CamHackMode.REGULAR;
        }

        private RadioButton getCorrespondingRadioButton(CamHackMode camHackMode)
        {
            switch (camHackMode)
            {
                case CamHackMode.REGULAR:
                    return radioButtonCamHackMode0;

                case CamHackMode.RELATIVE_ANGLE:
                    return radioButtonCamHackMode1RelativeAngle;

                case CamHackMode.ABSOLUTE_ANGLE:
                    return radioButtonCamHackMode1AbsoluteAngle;

                case CamHackMode.FIXED_POS:
                    return radioButtonCamHackMode2;

                case CamHackMode.FIXED_ORIENTATION:
                    return radioButtonCamHackMode3;

                default:
                    return null;
            }
        }
    }
}
