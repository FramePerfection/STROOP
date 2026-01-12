using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using System.Drawing;
using OpenTK.Mathematics;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using STROOP.Variables.SM64MemoryLayout;
using STROOP.Variables.Utilities;

namespace STROOP.Tabs.MapTab
{
    partial class MapGraphics
    {
        bool isMainMap => glControl.FindForm() is StroopMainForm;

        private void UpdateScale()
        {
            if (!isMainMap)
                return;

            if (mapTab.radioButtonMapControllersScaleCourseDefault.Checked)
                MapViewScale = MapScale.CourseDefault;
            else if (mapTab.radioButtonMapControllersScaleMaxCourseSize.Checked)
                MapViewScale = MapScale.MaxCourseSize;
            else
                MapViewScale = MapScale.Custom;

            if (MapViewScale == MapScale.CourseDefault) MapViewScaleWasCourseDefault = true;
            if (MapViewScale == MapScale.MaxCourseSize) MapViewScaleWasCourseDefault = false;

            switch (MapViewScale)
            {
                case MapScale.CourseDefault:
                case MapScale.MaxCourseSize:
                    RectangleF rectangle = MapViewScale == MapScale.CourseDefault ? mapTab.GetMapLayout().Coordinates : MAX_COURSE_SIZE;
                    List<(float, float)> coordinates = new List<(float, float)>()
                    {
                        (rectangle.Left, rectangle.Top),
                        (rectangle.Right, rectangle.Top),
                        (rectangle.Left, rectangle.Bottom),
                        (rectangle.Right, rectangle.Bottom),
                    };
                    List<(float, float)> rotatedCoordinates = coordinates.ConvertAll(coord =>
                    {
                        (float x, float z) = coord;
                        (double rotatedX, double rotatedZ) = MoreMath.RotatePointAboutPointAnAngularDistance(
                            x, z, 0, 0, 32768 - MapViewAngleValue);
                        return ((float)rotatedX, (float)rotatedZ);
                    });
                    float rotatedXMax = rotatedCoordinates.Max(coord => coord.Item1);
                    float rotatedXMin = rotatedCoordinates.Min(coord => coord.Item1);
                    float rotatedZMax = rotatedCoordinates.Max(coord => coord.Item2);
                    float rotatedZMin = rotatedCoordinates.Min(coord => coord.Item2);
                    float rotatedWidth = rotatedXMax - rotatedXMin;
                    float rotatedHeight = rotatedZMax - rotatedZMin;
                    MapViewScaleValue = Math.Min(
                        glControl.Width / rotatedWidth, glControl.Height / rotatedHeight);
                    break;
                case MapScale.Custom:
                    MapViewScaleValue = ParsingUtilities.ParseFloatNullable(
                                            mapTab.textBoxMapControllersScaleCustom.LastSubmittedText)
                                        ?? DEFAULT_MAP_VIEW_SCALE_VALUE;
                    break;
            }

            if (MapViewScale != MapScale.Custom)
            {
                mapTab.textBoxMapControllersScaleCustom.SubmitTextLoosely(MapViewScaleValue.ToString());
            }
        }

        private void UpdateCenter()
        {
            if (!isMainMap)
                return;

            if (view.mode == MapView.ViewMode.ThreeDimensional)
                return;

            if (mapTab.radioButtonMapControllersCenterBestFit.Checked)
                MapViewCenter = MapCenter.BestFit;
            else if (mapTab.radioButtonMapControllersCenterOrigin.Checked)
                MapViewCenter = MapCenter.Origin;
            else if (mapTab.radioButtonMapControllersCenterMario.Checked)
                MapViewCenter = MapCenter.Mario;
            else
                MapViewCenter = MapCenter.Custom;

            switch (MapViewCenter)
            {
                case MapCenter.BestFit:
                    RectangleF rectangle = MapViewScaleWasCourseDefault ? mapTab.GetMapLayout().Coordinates : MAX_COURSE_SIZE;
                    view.position.X = rectangle.X + rectangle.Width / 2;
                    view.position.Z = rectangle.Y + rectangle.Height / 2;
                    break;
                case MapCenter.Origin:
                    view.position = new Vector3(0.5f);
                    break;
                case MapCenter.Mario:
                    view.position = new Vector3(Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.XOffset),
                        Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset),
                        Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.ZOffset));
                    break;
                case MapCenter.Custom:
                    PositionAngle posAngle = PositionAngle.FromString(
                        mapTab.textBoxMapControllersCenterCustom.LastSubmittedText);
                    if (posAngle != null)
                    {
                        view.position = posAngle.position;
                        break;
                    }

                    List<string> stringValues = ParsingUtilities.ParseStringList(
                        mapTab.textBoxMapControllersCenterCustom.LastSubmittedText, replaceComma: false);
                    if (stringValues.Count >= 3)
                    {
                        view.position.X = ParsingUtilities.ParseFloatNullable(stringValues[0]) ?? 0;
                        view.position.Y = ParsingUtilities.ParseFloatNullable(stringValues[1]) ?? 0;
                        view.position.Z = ParsingUtilities.ParseFloatNullable(stringValues[2]) ?? 0;
                    }
                    else if (stringValues.Count >= 2)
                    {
                        view.position.X = ParsingUtilities.ParseFloatNullable(stringValues[0]) ?? 0;
                        view.position.Z = ParsingUtilities.ParseFloatNullable(stringValues[1]) ?? 0;
                    }
                    else if (stringValues.Count == 1)
                    {
                        view.position = new Vector3(ParsingUtilities.ParseFloatNullable(stringValues[0]) ?? 0);
                    }
                    else
                        view.position = new Vector3();

                    break;
            }

            if (MapViewCenter != MapCenter.Custom)
            {
                mapTab.textBoxMapControllersCenterCustom.SubmitTextLoosely($"{view.position.X}; {view.position.Y}; {view.position.Z}");
            }
        }

        private void UpdateAngle()
        {
            if (!isMainMap)
                return;

            if (mapTab.radioButtonMapControllersAngle0.Checked)
                MapViewAngle = MapAngle.Angle0;
            else if (mapTab.radioButtonMapControllersAngle16384.Checked)
                MapViewAngle = MapAngle.Angle16384;
            else if (mapTab.radioButtonMapControllersAngle32768.Checked)
                MapViewAngle = MapAngle.Angle32768;
            else if (mapTab.radioButtonMapControllersAngle49152.Checked)
                MapViewAngle = MapAngle.Angle49152;
            else if (mapTab.radioButtonMapControllersAngleMario.Checked)
                MapViewAngle = MapAngle.Mario;
            else if (mapTab.radioButtonMapControllersAngleCamera.Checked)
                MapViewAngle = MapAngle.Camera;
            else if (mapTab.radioButtonMapControllersAngleCentripetal.Checked)
                MapViewAngle = MapAngle.Centripetal;
            else
                MapViewAngle = MapAngle.Custom;

            switch (MapViewAngle)
            {
                case MapAngle.Angle0:
                    MapViewAngleValue = 0;
                    break;
                case MapAngle.Angle16384:
                    MapViewAngleValue = 16384;
                    break;
                case MapAngle.Angle32768:
                    MapViewAngleValue = 32768;
                    break;
                case MapAngle.Angle49152:
                    MapViewAngleValue = 49152;
                    break;
                case MapAngle.Mario:
                    MapViewAngleValue = Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
                    break;
                case MapAngle.Camera:
                    MapViewAngleValue = Config.Stream.GetUInt16(CameraConfig.StructAddress + CameraConfig.FacingYawOffset);
                    break;
                case MapAngle.Centripetal:
                    MapViewAngleValue = (float)MoreMath.ReverseAngle(
                        Config.Stream.GetUInt16(CameraConfig.StructAddress + CameraConfig.CentripetalAngleOffset));
                    break;
                case MapAngle.Custom:
                    PositionAngle posAngle = PositionAngle.FromString(
                        mapTab.textBoxMapControllersAngleCustom.LastSubmittedText);
                    if (posAngle != null)
                    {
                        MapViewAngleValue = (float)posAngle.Angle;
                        break;
                    }

                    MapViewAngleValue = ParsingUtilities.ParseFloatNullable(
                                            mapTab.textBoxMapControllersAngleCustom.LastSubmittedText)
                                        ?? DEFAULT_MAP_VIEW_ANGLE_VALUE;
                    break;
            }

            if (MapViewAngle != MapAngle.Custom)
            {
                mapTab.textBoxMapControllersAngleCustom.SubmitTextLoosely(MapViewAngleValue.ToString());
            }
        }

        public void ChangeScale(int sign, object value)
        {
            float? parsed = ParsingUtilities.ParseFloatNullable(value);
            if (!parsed.HasValue) return;
            mapTab.radioButtonMapControllersScaleCustom.Checked = true;
            MapViewScaleValue += sign * parsed.Value;
            if (isMainMap)
                mapTab.textBoxMapControllersScaleCustom.SubmitText(MapViewScaleValue.ToString());
        }

        public void ChangeScale2(int power, object value)
        {
            float? parsed = ParsingUtilities.ParseFloatNullable(value);
            if (!parsed.HasValue) return;
            mapTab.radioButtonMapControllersScaleCustom.Checked = true;
            MapViewScaleValue *= (float)Math.Pow(parsed.Value, power);

            if (isMainMap)
                mapTab.textBoxMapControllersScaleCustom.SubmitText(MapViewScaleValue.ToString());
        }

        public void ChangeCenter(int xSign, int zSign, object value)
        {
            if (!isMainMap)
                return;

            float? parsed = ParsingUtilities.ParseFloatNullable(value);
            if (!parsed.HasValue) return;
            mapTab.radioButtonMapControllersCenterCustom.Checked = true;
            float xOffset = xSign * parsed.Value;
            float zOffset = zSign * parsed.Value;
            (float xOffsetRotated, float zOffsetRotated) = ((float, float))MoreMath.RotatePointAboutPointAnAngularDistance(
                xOffset, zOffset, 0, 0, MapViewAngleValue);
            float multiplier = MapViewCenterChangeByPixels ? 1 / MapViewScaleValue : 1;
            float newCenterXValue = view.position.X + xOffsetRotated * multiplier;
            float newCenterZValue = view.position.Z + zOffsetRotated * multiplier;
            mapTab.textBoxMapControllersCenterCustom.SubmitText($"{newCenterXValue}; {view.position.Y}; {newCenterZValue}");
        }

        public void ChangeAngle(int sign, object value)
        {
            if (!isMainMap)
                return;

            float? parsed = ParsingUtilities.ParseFloatNullable(value);
            if (!parsed.HasValue) return;
            mapTab.radioButtonMapControllersAngleCustom.Checked = true;
            float newAngleValue = MapViewAngleValue + sign * parsed.Value;
            newAngleValue = (float)MoreMath.NormalizeAngleDouble(newAngleValue);
            mapTab.textBoxMapControllersAngleCustom.SubmitText(newAngleValue.ToString());
        }

        public void SetCustomScale(object value)
        {
            if (!isMainMap)
                return;

            mapTab.radioButtonMapControllersScaleCustom.Checked = true;
            mapTab.textBoxMapControllersScaleCustom.SubmitText(value.ToString());
        }

        public void SetCustomCenter(object value)
        {
            if (!isMainMap)
                return;

            mapTab.radioButtonMapControllersCenterCustom.Checked = true;
            mapTab.textBoxMapControllersCenterCustom.SubmitText(value.ToString());
        }

        public void SetCustomAngle(object value)
        {
            if (!isMainMap)
                return;

            mapTab.radioButtonMapControllersAngleCustom.Checked = true;
            mapTab.textBoxMapControllersAngleCustom.SubmitText(value.ToString());
        }
    }
}
