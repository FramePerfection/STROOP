using System;
using System.Collections.Generic;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using STROOP.Variables;
using STROOP.Variables.Views;

namespace STROOP.Controls.VariablePanel
{
    public class WatchVariableNumberWrapper<TNumber> : WatchVariableWrapper<TNumber> where TNumber : struct, IConvertible
    {
        static Func<WatchVariableControl, bool> WrapperProperty(Func<WatchVariableNumberWrapper<TNumber>, bool> func) =>
            (ctrl) =>
            {
                if (ctrl.WatchVarWrapper is WatchVariableNumberWrapper<TNumber> wrapper)
                    return func(wrapper);
                return false;
            };

        public static readonly WatchVariableSetting RoundToSetting = new WatchVariableSetting(
            "Round To",
            (ctrl, obj) =>
            {
                if (ctrl.WatchVarWrapper is WatchVariableNumberWrapper<TNumber> num)
                    if (obj is bool doRounding && doRounding == false)
                        num._roundingLimit = -1;
                    else if (obj is int roundingLimit)
                        num._roundingLimit = roundingLimit;
                    else if (obj == null)
                        num._roundingLimit = num._defaultRoundingLimit;
                    else
                        return false;
                else
                    return false;
                return true;
            },
            ((Func<(string, Func<object>, Func<WatchVariableControl, bool>)[]>)(() =>
            {
                var lst = new List<(string, Func<object>, Func<WatchVariableControl, bool>)>();
                lst.Add(("Default", () => null, WrapperProperty(wr => wr._roundingLimit == wr._defaultRoundingLimit)));
                lst.Add(("No Rounding", () => false, WrapperProperty(wr => wr._roundingLimit == -1)));
                for (int i = 0; i < 10; i++)
                {
                    var c = i;
                    lst.Add(($"{i} double places", () => c, WrapperProperty(wr => wr._roundingLimit == c)));
                }

                return lst.ToArray();
            }))()
        );

        public static readonly WatchVariableSetting DisplayAsHexSetting = new WatchVariableSetting(
            "Display as Hex",
            (ctrl, obj) =>
            {
                if (ctrl.WatchVarWrapper is WatchVariableNumberWrapper<TNumber> num)
                    if (obj is bool doHexDisplay)
                        num.displayAsHex = doHexDisplay;
                    else if (obj == null)
                        num.displayAsHex = num._defaultDisplayAsHex;
                    else
                        return false;
                else
                    return false;
                return true;
            },
            ("Default", () => null, WrapperProperty(wr => wr._displayAsHex == wr._defaultDisplayAsHex)),
            ("Hex", () => true, WrapperProperty(wr => wr._displayAsHex)),
            ("double", () => false, WrapperProperty(wr => !wr._displayAsHex))
        );

        protected const int DEFAULT_ROUNDING_LIMIT = 3;
        protected const bool DEFAULT_DISPLAY_AS_HEX = false;
        protected const bool DEFAULT_USE_CHECKBOX = false;
        protected const bool DEFAULT_IS_YAW = false;

        private static readonly int MAX_ROUNDING_LIMIT = 10;

        public bool displayAsHex = false;

        private readonly int _defaultRoundingLimit;
        private int _roundingLimit;

        protected readonly bool _defaultDisplayAsHex;
        protected bool _displayAsHex;
        protected Action<bool> _setDisplayAsHex;

        public WatchVariableNumberWrapper(IVariableView<TNumber> watchVar, WatchVariableControl watchVarControl)
            : base(watchVar, watchVarControl)
        {
            if (int.TryParse(watchVarControl.view.GetValueByKey(CommonViewProperties.roundingLimit), out var roundingLimit))
                _defaultRoundingLimit = roundingLimit;
            else
                _defaultRoundingLimit = DEFAULT_ROUNDING_LIMIT;

            _roundingLimit = _defaultRoundingLimit;
            if (_roundingLimit < -1 || _roundingLimit > MAX_ROUNDING_LIMIT)
                throw new ArgumentOutOfRangeException();

            _defaultDisplayAsHex =
                bool.TryParse(watchVarControl.view.GetValueByKey(CommonViewProperties.useHex), out var viewSetting)
                    ? viewSetting
                    : DEFAULT_DISPLAY_AS_HEX;
            displayAsHex = _displayAsHex = _defaultDisplayAsHex;

            AddNumberContextMenuStripItems();
        }

        public override bool TryParseValue(string value, out TNumber result)
            => ParsingUtilities.TryParseNumber(value, out result);

        protected virtual int? GetHexDigitCount() => (_view as IMemoryBasedVariableView)?.memoryDescriptor.NibbleCount;

        protected string HandleRounding(TNumber unknownValue)
        {
            double value;
            if (typeof(TNumber) == typeof(float) || typeof(TNumber) == typeof(double))
                value = (double)Convert.ChangeType(unknownValue, typeof(double));
            else
                return unknownValue.ToString();

            int? roundingLimit = _roundingLimit >= 0 ? _roundingLimit : (int?)null;
            double roundedValue = roundingLimit.HasValue
                ? Math.Round(value, roundingLimit.Value)
                : value;
            if (SavedSettingsConfig.DontRoundValuesToZero &&
                roundedValue == 0 && value != 0)
            {
                // Specially print values near zero
                string digitsString = roundingLimit?.ToString() ?? "";
                return value.ToString("E" + digitsString);
            }

            return roundedValue.ToString();
        }

        private void AddNumberContextMenuStripItems()
        {
            _watchVarControl.AddSetting(RoundToSetting);
            _watchVarControl.AddSetting(DisplayAsHexSetting);
        }

        public override string GetClass() => "Number";

        public override string DisplayValue(TNumber value)
        {
            if (displayAsHex)
                return HexUtilities.FormatValue(value, GetHexDigitCount(), true);
            else
                return HandleRounding(value);
        }
    }
}
