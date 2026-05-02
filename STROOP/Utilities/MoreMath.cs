using STROOP.Structs;
using STROOP.Structs.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Mathematics;
using STROOP.Variables.SM64MemoryLayout;
using STROOP.Variables.Utilities;

namespace STROOP.Utilities;

public static class STROOPMath
{
    public static Vector2 Min(Vector2 min, Vector2 @new) => new Vector2(Math.Min(min.X, @new.X), Math.Min(min.Y, @new.Y));
    public static Vector2 Max(Vector2 max, Vector2 @new) => new Vector2(Math.Max(max.X, @new.X), Math.Max(max.Y, @new.Y));
    public static Vector3 Min(Vector3 min, Vector3 @new) => new Vector3(Math.Min(min.X, @new.X), Math.Min(min.Y, @new.Y), Math.Min(min.Z, @new.Z));
    public static Vector3 Max(Vector3 max, Vector3 @new) => new Vector3(Math.Max(max.X, @new.X), Math.Max(max.Y, @new.Y), Math.Max(max.Z, @new.Z));

    public static double GetSignedDistanceFromPointToLine(
        double pX, double pZ, double v1X, double v1Z, double v2X, double v2Z, double v3X, double v3Z, int p1Index, int p2Index,
        TriangleClassification classification, bool? misalignmentOffsetNullable = null)
    {
        pX = PuUtilities.GetRelativeCoordinate(pX);
        pZ = PuUtilities.GetRelativeCoordinate(pZ);

        double[] vX = new double[] { v1X, v2X, v3X };
        double[] vZ = new double[] { v1Z, v2Z, v3Z };

        double p1X = vX[p1Index - 1];
        double p1Z = vZ[p1Index - 1];
        double p2X = vX[p2Index - 1];
        double p2Z = vZ[p2Index - 1];

        double dist = MoreMath.GetDistanceFromPointToLine(pX, pZ, p1X, p1Z, p2X, p2Z);
        bool leftOfLine = MoreMath.IsPointLeftOfLine(pX, pZ, p1X, p1Z, p2X, p2Z);
        bool floorTri = MoreMath.IsPointLeftOfLine(v3X, v3Z, v1X, v1Z, v2X, v2Z);
        bool onSideOfLineTowardsTri = floorTri == leftOfLine;
        double signedDist = dist * (onSideOfLineTowardsTri ? 1 : -1);

        bool misalignmentOffset = misalignmentOffsetNullable ?? SavedSettingsConfig.UseMisalignmentOffsetForDistanceToLine;
        if (misalignmentOffset && classification != TriangleClassification.Wall)
        {
            if (p1X == p2X)
            {
                bool thirdPointOnLeft = p1Z >= p2Z == floorTri;
                if ((thirdPointOnLeft && p1X >= 0) || (!thirdPointOnLeft && p1X <= 0))
                {
                    signedDist += 1;
                }
            }
            else if (p1Z == p2Z)
            {
                bool thirdPointOnTop = p1X <= p2X == floorTri;
                if ((thirdPointOnTop && p1Z >= 0) || (!thirdPointOnTop && p1Z <= 0))
                {
                    signedDist += 1;
                }
            }
        }

        return signedDist;
    }

    public static string GetBitString(byte b)
    {
        StringBuilder builder = new StringBuilder();
        for (int i = 7; i >= 0; i--)
        {
            bool bit = (b & (1 << i)) != 0;
            builder.Append(bit ? "1" : "0");
        }

        return builder.ToString();
    }

    public static string GetBitString(object value)
    {
        List<string> bitStrings = TypeUtilities.GetBytes(value).ToList().ConvertAll(b => GetBitString(b));
        bitStrings.Reverse();
        return String.Join("", bitStrings);
    }

    // Float stuff

    public static int GetFloatSign(float floatValue)
    {
        string bitString = GetBitString(floatValue);
        string signChar = bitString.Substring(0, 1);
        return signChar == "0" ? 1 : -1;
    }

    public static int GetFloatExponent(float floatValue)
    {
        string bitString = GetBitString(floatValue);
        string exponentString = bitString.Substring(1, 8);
        int byteValue = 0;
        for (int i = 0; i < 8; i++)
        {
            string bitChar = exponentString.Substring(8 - 1 - i, 1);
            bool bitBool = bitChar == "1";
            if (bitBool) byteValue = (byte)(byteValue | (1 << i));
        }

        int exponent = byteValue - 127;
        return exponent;
    }

    public static double GetFloatMantissa(float floatValue)
    {
        string bitString = GetBitString(floatValue);
        string exponentString = bitString.Substring(9, 23);
        double sum = 1;
        double multiplier = 1;
        for (int i = 0; i < 23; i++)
        {
            multiplier *= 0.5;
            string bitChar = exponentString.Substring(i, 1);
            bool bitBool = bitChar == "1";
            if (bitBool) sum += multiplier;
        }

        return sum;
    }

    public static ushort CalculateAngleFromInputs(int xInput, int yInput, ushort? cameraAngleNullable = null)
    {
        (float effectiveX, float effectiveY) = GetEffectiveInput(xInput, yInput);
        ushort marioAngle = InGameTrigUtilities.InGameATan(effectiveY, -effectiveX);
        ushort cameraAngleRaw = cameraAngleNullable ?? Config.Stream.GetUInt16(CameraConfig.StructAddress + CameraConfig.CentripetalAngleOffset);
        ushort cameraAngle = MoreMath.NormalizeAngleUshort(MoreMath.ReverseAngle(cameraAngleRaw));
        ushort summedAngle = MoreMath.NormalizeAngleUshort(marioAngle + cameraAngle);
        return summedAngle;
    }

    public static (float effectiveX, float effectiveY) GetEffectiveInput(int rawX, int rawY)
    {
        float effectiveX = rawX >= 8 ? rawX - 6 : rawX <= -8 ? rawX + 6 : 0;
        float effectiveY = rawY >= 8 ? rawY - 6 : rawY <= -8 ? rawY + 6 : 0;
        float hypotenuse = (float)Math.Sqrt(effectiveX * effectiveX + effectiveY * effectiveY);
        if (hypotenuse > 64)
        {
            effectiveX *= 64 / hypotenuse;
            effectiveY *= 64 / hypotenuse;
        }

        return (effectiveX, effectiveY);
    }


        public static (int xInput, int yInput) CalculateInputsForAngle(ushort goalAngle, ushort cameraAngle)
        {
            double bestMagnitude = 0;
            int bestX = 0;
            int bestY = 0;

            ushort truncatedGoalAngle = MoreMath.NormalizeAngleTruncated(goalAngle);
            for (int x = -128; x <= 127; x++)
            {
                if (MoreMath.InputIsInDeadZone(x)) continue;
                for (int y = -128; y <= 127; y++)
                {
                    if (MoreMath.InputIsInDeadZone(y)) continue;
                    ushort inputAngle = CalculateAngleFromInputs(x, y, cameraAngle);
                    ushort truncatedInputAngle = MoreMath.NormalizeAngleTruncated(inputAngle);
                    if (truncatedInputAngle == truncatedGoalAngle)
                    {
                        double magnitude = MoreMath.GetEffectiveInputMagnitudeUncapped(x, y);
                        if (magnitude > bestMagnitude)
                        {
                            bestMagnitude = magnitude;
                            bestX = x;
                            bestY = y;
                        }
                    }
                }
            }

            return (bestX, bestY);
        }

        public static (int xInput, int yInput) CalculateInputsForAngleOptimized(ushort goalAngle, ushort cameraAngle)
        {
            double bestMagnitude = 0;
            int bestX = 0;
            int bestY = 0;

            ushort truncatedGoalAngle = MoreMath.NormalizeAngleTruncated(goalAngle);
            ushort reversedCameraAngle = MoreMath.NormalizeAngleUshort(MoreMath.ReverseAngle(cameraAngle));
            ushort goalMarioAngle = MoreMath.NormalizeAngleUshort(goalAngle - reversedCameraAngle);
            double goalMarioAngleRadians = MoreMath.AngleUnitsToRadians(goalMarioAngle);

            bool useX;
            bool positiveA;
            bool positiveB;
            if (goalMarioAngle < 8192)
            {
                useX = false;
                positiveA = true;
                positiveB = false;
            }
            else if (goalMarioAngle < 16384)
            {
                useX = true;
                positiveA = false;
                positiveB = true;
            }
            else if (goalMarioAngle < 24576)
            {
                useX = true;
                positiveA = false;
                positiveB = false;
            }
            else if (goalMarioAngle < 32768)
            {
                useX = false;
                positiveA = false;
                positiveB = false;
            }
            else if (goalMarioAngle < 40960)
            {
                useX = false;
                positiveA = false;
                positiveB = true;
            }
            else if (goalMarioAngle < 49152)
            {
                useX = true;
                positiveA = true;
                positiveB = false;
            }
            else if (goalMarioAngle < 57344)
            {
                useX = true;
                positiveA = true;
                positiveB = true;
            }
            else
            {
                useX = false;
                positiveA = true;
                positiveB = true;
            }

            double ratio = useX ? Math.Cos(goalMarioAngleRadians) / Math.Sin(goalMarioAngleRadians) : Math.Sin(goalMarioAngleRadians) / Math.Cos(goalMarioAngleRadians);
            double ratioAbs = Math.Abs(ratio);
            int max = positiveA ? 121 : 122;

            for (int aMag = 8; aMag <= max; aMag++)
            {
                int a = aMag * (positiveA ? 1 : -1);
                int bMedianMag = (int)(aMag * ratioAbs);
                int bMedian = bMedianMag * (positiveB ? 1 : -1);

                int width = 1;
                for (int b = bMedian - width; b <= bMedian + width; b++)
                {
                    int xEffective = useX ? a : b;
                    int yEffective = useX ? b : a;

                    if (Math.Abs(xEffective) == 1 || Math.Abs(yEffective) == 1) continue;

                    int x = xEffective < 0 ? xEffective - 6 : xEffective > 0 ? xEffective + 6 : 0;
                    int y = yEffective < 0 ? yEffective - 6 : yEffective > 0 ? yEffective + 6 : 0;

                    ushort inputAngle = CalculateAngleFromInputs(x, y, cameraAngle);
                    ushort truncatedInputAngle = MoreMath.NormalizeAngleTruncated(inputAngle);
                    if (truncatedInputAngle == truncatedGoalAngle)
                    {
                        double magnitude = MoreMath.GetEffectiveInputMagnitudeUncapped(x, y);
                        if (magnitude > bestMagnitude)
                        {
                            bestMagnitude = magnitude;
                            bestX = x;
                            bestY = y;
                        }
                    }
                }
            }

            return (bestX, bestY);
        }
}
