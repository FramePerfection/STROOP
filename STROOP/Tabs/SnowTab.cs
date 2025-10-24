using System;
using System.Collections.Generic;
using STROOP.Core.Utilities;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using STROOP.Variables;
using STROOP.Variables.Utilities;
using STROOP.Variables.VariablePanel;

namespace STROOP.Tabs
{
    public partial class SnowTab : STROOPTab
    {
        private static readonly List<string> ALL_VAR_GROUPS =
            new List<string>()
            {
                VariableGroup.Basic,
                VariableGroup.Intermediate,
                VariableGroup.Advanced,
                VariableGroup.Snow,
                VariableGroup.Custom,
            };

        private static readonly List<string> VISIBLE_VAR_GROUPS =
            new List<string>()
            {
                VariableGroup.Basic,
                VariableGroup.Intermediate,
                VariableGroup.Advanced,
                VariableGroup.Snow,
                VariableGroup.Custom,
            };

        private short _numSnowParticles;
        private List<IEnumerable<IWinFormsVariableCell>> _snowParticleCells;

        public SnowTab()
        {
            InitializeComponent();
            _variablePanelSnow.SetGroups(ALL_VAR_GROUPS, VISIBLE_VAR_GROUPS);
        }

        public override string GetDisplayName() => "Snow";

        public override void InitializeTab()
        {
            base.InitializeTab();

            _numSnowParticles = 0;
            _snowParticleCells = new List<IEnumerable<IWinFormsVariableCell>>();

            buttonSnowRetrieve.Click += (sender, e) =>
            {
                int? snowIndexNullable = ParsingUtilities.ParseIntNullable(textBoxSnowIndex.Text);
                if (!snowIndexNullable.HasValue) return;
                int snowIndex = snowIndexNullable.Value;
                if (snowIndex < 0 || snowIndex > _numSnowParticles) return;
                ButtonUtilities.RetrieveSnow((uint)snowIndex);
            };

            ControlUtilities.InitializeThreeDimensionController(
                CoordinateSystem.Euler,
                true,
                groupBoxSnowPosition,
                "SnowPosition",
                (float hOffset, float vOffset, float nOffset, bool useRelative) =>
                {
                    int? snowIndexNullable = ParsingUtilities.ParseIntNullable(textBoxSnowIndex.Text);
                    if (!snowIndexNullable.HasValue) return;
                    int snowIndex = snowIndexNullable.Value;
                    if (snowIndex < 0 || snowIndex > _numSnowParticles) return;
                    ButtonUtilities.TranslateSnow(
                        (uint)snowIndex,
                        hOffset,
                        nOffset,
                        -1 * vOffset,
                        useRelative);
                });
        }

        private List<VariablePrecursor> GetSnowParticleControls(int index)
        {
            uint structOffset = (uint)index * SnowConfig.ParticleStructSize;
            List<uint> offsets = new List<uint>()
            {
                structOffset + SnowConfig.XOffset,
                structOffset + SnowConfig.YOffset,
                structOffset + SnowConfig.ZOffset,
            };
            List<string> names = new List<string>()
            {
                String.Format("Particle {0} X", index),
                String.Format("Particle {0} Y", index),
                String.Format("Particle {0} Z", index),
            };

            var precursors = new List<VariablePrecursor>();
            for (int i = 0; i < 3; i++)
                precursors.Add((names[i], new CustomVariable<int>(VariableSubclass.Number)
                {
                    getter = () => Config.Stream.GetInt32(Config.Stream.GetUInt32(SnowConfig.SnowArrayPointerAddress) + offsets[i]).Yield(),
                    setter = (val) => Config.Stream.SetValue(val, Config.Stream.GetUInt32(SnowConfig.SnowArrayPointerAddress) + offsets[i]).Yield()
                }));

            return precursors;
        }

        public override void Update(bool updateView)
        {
            if (!updateView) return;

            short numSnowParticles = Config.Stream.GetInt16(SnowConfig.CounterAddress);
            if (numSnowParticles > _numSnowParticles) // need to add controls
            {
                for (int i = _numSnowParticles; i < numSnowParticles; i++)
                    _snowParticleCells.Add(_variablePanelSnow.AddVariables(GetSnowParticleControls(i)));
                _numSnowParticles = numSnowParticles;
            }
            else if (numSnowParticles < _numSnowParticles) // need to remove controls
            {
                for (int i = _numSnowParticles - 1; i >= numSnowParticles; i--)
                {
                    var snowParticleControls = _snowParticleCells[i];
                    _snowParticleCells.Remove(snowParticleControls);
                    _variablePanelSnow.RemoveVariables(snowParticleControls);
                }

                _numSnowParticles = numSnowParticles;
            }

            base.Update(updateView);
        }
    }
}
