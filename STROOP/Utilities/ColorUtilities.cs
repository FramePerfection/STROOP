using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OpenTK.Mathematics;
using STROOP.Core.Utilities;

namespace STROOP.Utilities
{
    public static class ColorUtilities
    {
        public static readonly Dictionary<string, string> ColorToParamsDictionary =
            new Dictionary<string, string>()
            {
                ["Red"] = "#FFD7D7",
                ["Orange"] = "#FFE2B7",
                ["Yellow"] = "#FFFFD0",
                ["Green"] = "#CFFFCC",
                ["LightBlue"] = "#CCFFFA",
                ["Blue"] = "#CADDFF",
                ["Purple"] = "#E5CCFF",
                ["Pink"] = "#FFCCFF",
                ["Grey"] = "#D0D0D0",
            };

        public static readonly List<Color> ColorList =
            ColorToParamsDictionary.Values.ToList()
                .ConvertAll(html => ColorTranslator.FromHtml(html));

        private static readonly Dictionary<string, string> ParamsToColorDictionary =
            GeneralUtilities.ReverseDictionary(ColorToParamsDictionary);

        public static Color GetColorFromString(string colorString)
        {
            if (colorString.Substring(0, 1) != "#")
                colorString = ColorToParamsDictionary[colorString];
            return ColorTranslator.FromHtml(colorString);
        }

        public static string ConvertColorToString(Color color)
        {
            string colorParams = ConvertColorToParams(color);
            if (ParamsToColorDictionary.ContainsKey(colorParams))
                return ParamsToColorDictionary[colorParams];
            return colorParams;
        }

        public static string ConvertColorToParams(Color color)
        {
            string r = String.Format("{0:X2}", color.R);
            string g = String.Format("{0:X2}", color.G);
            string b = String.Format("{0:X2}", color.B);
            return "#" + r + g + b;
        }

        public static Color LastCustomColor = SystemColors.Control;

        public static Color GetColorForVariable()
        {
            int? inputtedNumber = GlobalKeyboard.GetCurrentlyInputtedNumber();

            if (inputtedNumber.HasValue &&
                inputtedNumber.Value > 0 &&
                inputtedNumber.Value <= ColorList.Count)
            {
                return ColorList[inputtedNumber.Value - 1];
            }

            return SystemColors.Control;
        }

        public static Color? GetColorForHighlight()
        {
            int? inputtedNumber = GlobalKeyboard.GetCurrentlyInputtedNumber();
            switch (inputtedNumber)
            {
                case 1:
                    return Color.Red;
                case 2:
                    return Color.Orange;
                case 3:
                    return Color.Yellow;
                case 4:
                    return Color.Green;
                case 5:
                    return Color.Blue;
                case 6:
                    return Color.Purple;
                case 7:
                    return Color.Pink;
                case 8:
                    return Color.Brown;
                case 9:
                    return Color.Black;
                case 0:
                    return Color.White;
                default:
                    return null;
            }
        }

        public static Color? ConvertDecimalToColor(string text)
        {
            List<int?> numbersNullable = ParsingUtilities.ParseIntList(text);
            if (numbersNullable.Count != 3) return null;
            if (numbersNullable.Any(number => !number.HasValue)) return null;
            if (numbersNullable.Any(number => number.Value < 0 || number.Value > 255)) return null;
            List<int> numbers = numbersNullable.ConvertAll(number => number.Value);
            return Color.FromArgb(numbers[0], numbers[1], numbers[2]);
        }

        public static string ConvertColorToDecimal(Color color)
        {
            return color.R + "," + color.G + "," + color.B;
        }

        public static Color InterpolateColor(Color c1, Color c2, double amount)
        {
            amount = MoreMath.Clamp(amount, 0, 1);
            byte r = (byte)((c1.R * (1 - amount)) + c2.R * amount);
            byte g = (byte)((c1.G * (1 - amount)) + c2.G * amount);
            byte b = (byte)((c1.B * (1 - amount)) + c2.B * amount);
            return Color.FromArgb(r, g, b);
        }

        public static Color? GetColorFromDialog(Color? defaultColor = null)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.FullOpen = true;
            if (defaultColor.HasValue) colorDialog.Color = defaultColor.Value;
            if (colorDialog.ShowDialog() == DialogResult.OK) return colorDialog.Color;
            return null;
        }

        public static Color AddAlpha(Color color, byte alpha)
        {
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }

        public static Vector4 ColorToVec4(Color color, int alpha = -1) =>
            new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, (alpha == -1 ? color.A : alpha) / 255f);

        public static Color Vec4ToColor(Vector4 color) =>
            Color.FromArgb((byte)(Math.Max(0, Math.Min(255, color.W * 255))),
                (byte)(Math.Max(0, Math.Min(255, color.X * 255))),
                (byte)(Math.Max(0, Math.Min(255, color.Y * 255))),
                (byte)(Math.Max(0, Math.Min(255, color.Z * 255))));

        public static Vector4 ColorFromHSV(float hue, float saturation, float value, float alpha = 1)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            float f = hue / 60 - (float)Math.Floor(hue / 60);

            float v = value;
            float p = value * (1 - saturation);
            float q = value * (1 - f * saturation);
            float t = value * (1 - (1 - f) * saturation);

            if (hi == 0)
                return new Vector4(v, t, p, alpha);
            else if (hi == 1)
                return new Vector4(q, v, p, alpha);
            else if (hi == 2)
                return new Vector4(p, v, t, alpha);
            else if (hi == 3)
                return new Vector4(p, q, v, alpha);
            else if (hi == 4)
                return new Vector4(t, p, v, alpha);
            else
                return new Vector4(v, p, q, alpha);
        }

        public static Vector4 GetRandomColor(int seed)
        {
            Random rnd = new Random(seed);
            return ColorFromHSV((float)rnd.NextDouble() * 360, 0.5f, 0.8f);
        }
    }
}
