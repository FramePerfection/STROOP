using STROOP.Utilities;
using STROOP.Variables.Utilities;

namespace STROOP.Variables.VariablePanel.Cells
{
    public abstract class VariableAngleCell<TUiContext, TNumber> : VariableNumberCell<TUiContext, TNumber>
        where TNumber : struct, IConvertible
        where TUiContext : IUiContext
    {
        public static readonly VariableCellSetting<TUiContext> DisplaySignedCellSetting = new VariableCellSetting<TUiContext>(
            "Angle: Signed",
            CreateBoolWithDefault<VariableAngleCell<TUiContext, TNumber>>((wrapper, val) => wrapper._signed = val, wrapper => wrapper._defaultSigned),
            ("Default", () => null, WrapperProperty<VariableAngleCell<TUiContext, TNumber>>(wr => wr._signed == wr._defaultSigned)),
            ("Unsigned", () => false, WrapperProperty<VariableAngleCell<TUiContext, TNumber>>(wr => !wr._signed)),
            ("Signed", () => true, WrapperProperty<VariableAngleCell<TUiContext, TNumber>>(wr => wr._signed))
        );

        public static readonly VariableCellSetting<TUiContext> AngleUnitTypeCellSetting = new VariableCellSetting<TUiContext>(
            "Angle: Units",
            (ctrl, obj) =>
            {
                if (ctrl.varCellInternal is VariableAngleCell<TUiContext, TNumber> num)
                    if (obj is AngleUnitType type)
                        num._angleUnitType = type;
                    else if (obj == null)
                        num._angleUnitType = num._defaultAngleUnitType;
                    else
                        return false;
                else
                    return false;
                return true;
            },
            ((Func<(string, Func<object>, Func<VariableCellControl<TUiContext>, bool>)[]>)(() =>
            {
                var lst = new List<(string, Func<object>, Func<VariableCellControl<TUiContext>, bool>)>();
                foreach (AngleUnitType angleUnitType in Enum.GetValues(typeof(AngleUnitType)))
                {
                    string stringValue = angleUnitType.ToString();
                    if (stringValue == AngleUnitType.InGameUnits.ToString()) stringValue = "In-Game Units";
                    var value = angleUnitType;
                    lst.Add((stringValue, () => angleUnitType, WrapperProperty<VariableAngleCell<TUiContext, TNumber>>(wr => wr._angleUnitType == angleUnitType)));
                }

                return lst.ToArray();
            }))()
        );

        public static readonly VariableCellSetting<TUiContext> TruncateToMultipleOf16CellSetting = new VariableCellSetting<TUiContext>(
            "Angle: Truncate to Multiple of 16",
            CreateBoolWithDefault<VariableAngleCell<TUiContext, TNumber>>((wrapper, val) => wrapper._truncateToMultipleOf16 = val, wrapper => wrapper._defaultTruncateToMultipleOf16),
            ("Default", () => null, WrapperProperty<VariableAngleCell<TUiContext, TNumber>>(wr => wr._truncateToMultipleOf16 == wr._defaultTruncateToMultipleOf16)),
            ("Truncate to Multiple of 16", () => true, WrapperProperty<VariableAngleCell<TUiContext, TNumber>>(wr => wr._truncateToMultipleOf16)),
            ("Don't Truncate to Multiple of 16", () => false, WrapperProperty<VariableAngleCell<TUiContext, TNumber>>(wr => !wr._truncateToMultipleOf16))
        );

        public static readonly VariableCellSetting<TUiContext> ConstrainToOneRevolutionCellSetting = new VariableCellSetting<TUiContext>(
            "Angle: Constrain to One Revolution",
            CreateBoolWithDefault<VariableAngleCell<TUiContext, TNumber>>((wrapper, val) => wrapper._constrainToOneRevolution = val, wrapper => wrapper._defaultConstrainToOneRevolution),
            ("Default", () => null, WrapperProperty<VariableAngleCell<TUiContext, TNumber>>(wr => wr._constrainToOneRevolution == wr._defaultConstrainToOneRevolution)),
            ("Constrain to One Revolution", () => true, WrapperProperty<VariableAngleCell<TUiContext, TNumber>>(wr => wr._constrainToOneRevolution)),
            ("Don't Constrain to One Revolution", () => false, WrapperProperty<VariableAngleCell<TUiContext, TNumber>>(wr => !wr._constrainToOneRevolution))
        );

        public static readonly VariableCellSetting<TUiContext> ReverseCellSetting = new VariableCellSetting<TUiContext>(
            "Angle: Reverse",
            CreateBoolWithDefault<VariableAngleCell<TUiContext, TNumber>>((wrapper, val) => wrapper._reverse = val, wrapper => wrapper._defaultReverse),
            ("Default", () => null, WrapperProperty<VariableAngleCell<TUiContext, TNumber>>(wr => wr._reverse == wr._defaultReverse)),
            ("Reverse", () => true, WrapperProperty<VariableAngleCell<TUiContext, TNumber>>(wr => wr._reverse)),
            ("Don't Reverse", () => false, WrapperProperty<VariableAngleCell<TUiContext, TNumber>>(wr => !wr._reverse))
        );

        private readonly bool _defaultSigned;
        private bool _signed;

        private readonly AngleUnitType _defaultAngleUnitType;
        private AngleUnitType _angleUnitType;

        private readonly bool _defaultTruncateToMultipleOf16;
        private bool _truncateToMultipleOf16;

        private readonly bool _defaultConstrainToOneRevolution;
        private bool _constrainToOneRevolution;

        private readonly bool _defaultReverse;
        private bool _reverse;

        private readonly Type _baseType;
        private readonly Type _defaultEffectiveType;

        private Type _effectiveType
        {
            get
            {
                if (_constrainToOneRevolution || TypeUtilities.TypeSize[_baseType] == 2)
                    return effectiveSigned ? typeof(short) : typeof(ushort);
                else
                    return effectiveSigned ? typeof(int) : typeof(uint);
            }
        }

        protected abstract bool DisplayAsUnsigned();

        private readonly bool _isYaw;
        private bool effectiveSigned => _signed && (!_isYaw || DisplayAsUnsigned());

        public VariableAngleCell(IVariable<TNumber> watchVar, VariableCellControl<TUiContext> varCellControl)
            : base(watchVar, varCellControl)
        {
            var displayType = (view as IMemoryVariable)?.memoryDescriptor.ClrType ?? typeof(double);
            if (TypeUtilities.StringToType.TryGetValue(varCellControl.view.GetValueByKey(CommonVariableProperties.display) ?? "", out var dType))
                displayType = dType;

            _baseType = displayType;
            _defaultEffectiveType = displayType;
            if (_baseType == null || _defaultEffectiveType == null) throw new ArgumentOutOfRangeException();

            _defaultSigned = TypeUtilities.TypeSign[_defaultEffectiveType];
            _signed = _defaultSigned;

            _defaultAngleUnitType = AngleUnitType.InGameUnits;
            _angleUnitType = _defaultAngleUnitType;

            _defaultTruncateToMultipleOf16 = false;
            _truncateToMultipleOf16 = _defaultTruncateToMultipleOf16;

            _defaultConstrainToOneRevolution = displayType != null && TypeUtilities.TypeSize[displayType] == 4;
            _constrainToOneRevolution = _defaultConstrainToOneRevolution;

            _defaultReverse = false;
            _reverse = _defaultReverse;

            if (bool.TryParse(varCellControl.view.GetValueByKey("yaw"), out var isYaw))
                _isYaw = isYaw;
            else
                _isYaw = DEFAULT_IS_YAW;

            AddAngleContextMenuStripItems();
        }

        public override bool TryParseValue(string value, out TNumber result)
        {
            double? doubleValueNullable = ParsingUtilities.ParseDoubleNullable(value);
            if (doubleValueNullable.HasValue)
            {
                double doubleValue = doubleValueNullable.Value;

                if (_reverse)
                {
                    doubleValue += 32768;
                }

                if (_truncateToMultipleOf16)
                {
                    doubleValue = MoreMath.TruncateToMultipleOf16(doubleValue);
                }

                doubleValue = MoreMath.NormalizeAngleUsingType(doubleValue, _effectiveType);
                doubleValue = (doubleValue / 65536) * GetAngleUnitTypeMaxValue();

                result = (TNumber)Convert.ChangeType(doubleValue, typeof(TNumber));
                return true;
            }

            return base.TryParseValue(value, out result);
        }

        private void AddAngleContextMenuStripItems()
        {
            control.AddSetting(DisplaySignedCellSetting);
            control.AddSetting(AngleUnitTypeCellSetting);
            control.AddSetting(TruncateToMultipleOf16CellSetting);
            control.AddSetting(ConstrainToOneRevolutionCellSetting);
            control.AddSetting(ReverseCellSetting);
        }

        private double GetAngleUnitTypeMaxValue(AngleUnitType? angleUnitTypeNullable = null)
        {
            AngleUnitType angleUnitType = angleUnitTypeNullable ?? _angleUnitType;
            switch (angleUnitType)
            {
                case AngleUnitType.InGameUnits:
                    return 65536;
                case AngleUnitType.HAU:
                    return 4096;
                case AngleUnitType.Degrees:
                    return 360;
                case AngleUnitType.Radians:
                    return 2 * Math.PI;
                case AngleUnitType.Revolutions:
                    return 1;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private double GetAngleUnitTypeAndMaybeSignedMaxValue(AngleUnitType? angleUnitTypeNullable = null, bool? signedNullable = null)
        {
            AngleUnitType angleUnitType = angleUnitTypeNullable ?? _angleUnitType;
            bool signed = signedNullable ?? effectiveSigned;
            double maxValue = GetAngleUnitTypeMaxValue(angleUnitType);
            return signed ? maxValue / 2 : maxValue;
        }

        private double GetAngleUnitTypeAndMaybeSignedMinValue(AngleUnitType? angleUnitTypeNullable = null, bool? signedNullable = null)
        {
            AngleUnitType angleUnitType = angleUnitTypeNullable ?? _angleUnitType;
            bool signed = signedNullable ?? effectiveSigned;
            double maxValue = GetAngleUnitTypeMaxValue(angleUnitType);
            return signed ? -1 * maxValue / 2 : 0;
        }

        public override string DisplayValue(TNumber value)
        {
            double doubleValue = (double)Convert.ChangeType(value, typeof(double));

            doubleValue = (doubleValue / GetAngleUnitTypeMaxValue()) * 65536;
            if (_reverse)
            {
                doubleValue += 32768;
            }

            return doubleValue.ToString();
        }


        protected object HandleAngleRoundingOut(object value)
        {
            double? doubleValueNullable = ParsingUtilities.ParseDoubleNullable(value);
            if (!doubleValueNullable.HasValue) return value;
            double doubleValue = doubleValueNullable.Value;

            if (doubleValue == GetAngleUnitTypeAndMaybeSignedMaxValue())
                doubleValue = GetAngleUnitTypeAndMaybeSignedMinValue();

            return doubleValue;
        }

        protected override int? GetHexDigitCount() => TypeUtilities.TypeSize[_effectiveType] * 2;

        public override string GetClass() => "Angle";
    }
}
