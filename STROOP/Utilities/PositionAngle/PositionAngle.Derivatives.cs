using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using STROOP.Controls.VariablePanel;
using STROOP.Core.Utilities;
using STROOP.Variables;

namespace STROOP.Utilities
{
    partial class PositionAngle
    {
        public class HybridPositionAngle : PositionAngle
        {
            static PositionAngle pointCustom = Custom(new Vector3(0));

            public static List<HybridPositionAngle> pointPAs = new List<HybridPositionAngle>()
            {
                new HybridPositionAngle(() => Mario, () => Mario, "Self"),
                new HybridPositionAngle(() => pointCustom, () => pointCustom, "Point")
            };

            public static readonly (string, VariablePanel.SpecialFuncWatchVariables) GenerateBaseVariables =
                ("Base Info",
                    pa =>
                    {
                        T MakePATypeView<T>(T view) where T : IVariable
                        {
                            view.SetValueByKey(CommonVariableProperties.specialType, "PositionAngle");
                            return view;
                        }

                        var vars = new CustomVariable[]
                        {
                            MakePATypeView(new CustomVariable<string>(VariableSubclass.String)
                            {
                                Name = $"{pa.name} Pos Type",
                                Color = "Blue",
                                getter = () => pa.first().ToString().Yield(),
                                setter = newPAString =>
                                {
                                    var newPA = PositionAngle.FromString((string)newPAString);
                                    if (newPA == null)
                                        return false.Yield();
                                    pa.first = () => newPA;
                                    return true.Yield();
                                }
                            }),
                            MakePATypeView(new CustomVariable<string>(VariableSubclass.String)
                            {
                                Name = $"{pa.name} Angle Type",
                                Color = "Blue",
                                getter = () => pa.second().ToString().Yield(),
                                setter = newPAString =>
                                {
                                    var newPA = PositionAngle.FromString((string)newPAString);
                                    if (newPA == null)
                                        return false.Yield();
                                    pa.second = () => newPA;
                                    return true.Yield();
                                }
                            }),
                            new CustomVariable<double>(VariableSubclass.Number)
                            {
                                Name = $"{pa.name} X",
                                Color = "Blue",
                                getter = () => pa.first().X.Yield(),
                                setter = val => pa.first().SetX(val).Yield()
                            },
                            new CustomVariable<double>(VariableSubclass.Number)
                            {
                                Name = $"{pa.name} Y",
                                Color = "Blue",
                                getter = () => pa.first().Y.Yield(),
                                setter = val => pa.first().SetY(val).Yield()
                            },
                            new CustomVariable<double>(VariableSubclass.Number)
                            {
                                Name = $"{pa.name} Z",
                                Color = "Blue",
                                getter = () => pa.first().Z.Yield(),
                                setter = val => pa.first().SetZ(val).Yield()
                            },
                            new CustomVariable<double>(VariableSubclass.Angle)
                            {
                                Name = $"{pa.name} Angle",
                                Color = "Blue",
                                Display = "short",
                                getter = () => pa.second().Angle.Yield(),
                                setter = val => pa.second().SetAngle(val).Yield()
                            },
                        };
                        pa.OnDelete += () =>
                        {
                            foreach (var v in vars)
                                v.OnDelete();
                        };
                        return vars;
                    }
            );

            public static (string, VariablePanel.SpecialFuncWatchVariables) GenerateRelations(HybridPositionAngle relation) =>
                ($"Relations to {relation.name}",
                    (HybridPositionAngle pa) =>
                    {
                        List<IVariable> vars = new List<IVariable>();
                        var distTypes = new[] { "X", "Y", "Z", "H", "", "F", "S" };
                        var distGetters = new Func<PositionAngle, PositionAngle, double>[]
                        {
                            GetXDistance,
                            GetYDistance,
                            GetZDistance,
                            GetHDistance,
                            GetDistance,
                            GetFDistance,
                            GetSDistance,
                        };
                        var distSetters = new Func<PositionAngle, PositionAngle, double, bool>[]
                        {
                            SetXDistance,
                            SetYDistance,
                            SetZDistance,
                            SetHDistance,
                            SetDistance,
                            SetFDistance,
                            SetSDistance,
                        };

                        for (int k = 0; k < distTypes.Length; k++)
                        {
                            string distType = distTypes[k];
                            Func<PositionAngle, PositionAngle, double> getter = distGetters[k];
                            Func<PositionAngle, PositionAngle, double, bool> setter = distSetters[k];

                            vars.Add(new CustomVariable<double>(VariableSubclass.Number)
                            {
                                Color = "LightBlue",
                                Name = $"{distType}Dist {relation.name} To {pa.name}",
                                getter = () => getter(relation, pa).Yield(),
                                setter = (double dist) => setter(relation, pa, dist).Yield()
                            });
                        }

                        vars.Add(new CustomVariable<double>(VariableSubclass.Number)
                        {
                            Color = "LightBlue",
                            Name = $"Angle {relation.name} To {pa.name}",
                            Display = "short",
                            getter = () => GetAngleTo(relation, pa).Yield(),
                            setter = (double angle) => SetAngleTo(relation, pa, angle).Yield()
                        });

                        vars.Add(new CustomVariable<double>(VariableSubclass.Number)
                        {
                            Color = "LightBlue",
                            Name = $"DAngle {relation.name} To {pa.name}",
                            Display = "short",
                            getter = () => GetDAngleTo(relation, pa).Yield(),
                            setter = (double angleDiff) => SetDAngleTo(relation, pa, Convert.ToDouble(angleDiff)).Yield()
                        });

                        vars.Add(new CustomVariable<double>(VariableSubclass.Number)
                        {
                            Color = "LightBlue",
                            Name = $"AngleDiff {relation.name} To {pa.name}",
                            Display = "short",
                            getter = () => GetAngleDifference(relation, pa).Yield(),
                            setter = (double angleDiff) => SetAngleDifference(relation, pa, Convert.ToDouble(angleDiff)).Yield()
                        });

                        Action remove = () =>
                        {
                            foreach (var v in vars)
                                v.OnDelete();
                        };
                        pa.OnDelete += remove;
                        relation.OnDelete += remove;

                        return vars;
                    }
            );


            public Action OnDelete = null;
            public readonly string name;
            public Func<PositionAngle> first, second;

            public HybridPositionAngle(Func<PositionAngle> first, Func<PositionAngle> second, string name = null)
            {
                this.name = name;
                this.first = first;
                this.second = second;
            }

            public override double X => first().X;
            public override double Y => first().Y;
            public override double Z => first().Z;
            public override double Angle => second().Angle;
            public override bool SetX(double value) => first().SetX(value);
            public override bool SetY(double value) => first().SetY(value);
            public override bool SetZ(double value) => first().SetZ(value);
            public override bool SetAngle(double value) => second().SetAngle(value);

            public override string ToString() => name;
        }

        public class TruncatePositionAngle : PositionAngle
        {
            public readonly PositionAngle pa;

            public TruncatePositionAngle(PositionAngle pa)
            {
                this.pa = pa;
            }

            public override double X => (int)pa.X;
            public override double Y => (int)pa.Y;
            public override double Z => (int)pa.Z;
            public override double Angle => (int)pa.Angle;
            public override bool SetX(double value) => pa.SetX((int)value);
            public override bool SetY(double value) => pa.SetY((int)value);
            public override bool SetZ(double value) => pa.SetZ((int)value);
            public override bool SetAngle(double value) => pa.SetAngle((int)value);
        }

        public class FunctionsPositionAngle : PositionAngle
        {
            readonly Func<double>[] getters;
            readonly Func<double, bool>[] setters;

            public FunctionsPositionAngle(IEnumerable<Func<double>> getters, IEnumerable<Func<double, bool>> setters)
            {
                this.getters = getters.ToArray();
                this.setters = setters.ToArray();
            }

            public override double X => getters.Length > 0 ? getters[0]?.Invoke() ?? double.NaN : double.NaN;
            public override double Y => getters.Length > 1 ? getters[1]?.Invoke() ?? double.NaN : double.NaN;
            public override double Z => getters.Length > 2 ? getters[2]?.Invoke() ?? double.NaN : double.NaN;
            public override double Angle => getters.Length > 3 ? getters[3]?.Invoke() ?? double.NaN : double.NaN;
            public override bool SetX(double value) => setters.Length > 0 ? setters[0]?.Invoke(value) ?? false : false;
            public override bool SetY(double value) => setters.Length > 1 ? setters[1]?.Invoke(value) ?? false : false;
            public override bool SetZ(double value) => setters.Length > 2 ? setters[2]?.Invoke(value) ?? false : false;
            public override bool SetAngle(double value) => setters.Length > 3 ? setters[3]?.Invoke(value) ?? false : false;
        }
    }
}
