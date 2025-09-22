using System;
using System.Collections.Generic;
using STROOP.Controls.VariablePanel;
using STROOP.Core.Utilities;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using STROOP.Variables;

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
        private List<IEnumerable<WatchVariableControl>> _snowParticleControls;

        public SnowTab()
        {
            InitializeComponent();
            watchVariablePanelSnow.SetGroups(ALL_VAR_GROUPS, VISIBLE_VAR_GROUPS);
        }

        public override string GetDisplayName() => "Snow";

        public override void InitializeTab()
        {
            base.InitializeTab();

            _numSnowParticles = 0;
            _snowParticleControls = new List<IEnumerable<WatchVariableControl>>();

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

        private List<NamedVariableCollection.IView> GetSnowParticleControls(int index)
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

            var controls = new List<NamedVariableCollection.IView>();
            for (int i = 0; i < 3; i++)
            {
                var view = new NamedVariableCollection.CustomView<int>(WatchVariableSubclass.Number)
                {
                    Name = names[i],
                    _getterFunction = () => Config.Stream.GetInt32(Config.Stream.GetUInt32(SnowConfig.SnowArrayPointerAddress) + offsets[i]).Yield(),
                    _setterFunction = (val) => Config.Stream.SetValue(val, Config.Stream.GetUInt32(SnowConfig.SnowArrayPointerAddress) + offsets[i]).Yield()
                };
                controls.Add(view);
            }

            return controls;
        }

        public override void Update(bool updateView)
        {
            if (!updateView) return;

            short numSnowParticles = Config.Stream.GetInt16(SnowConfig.CounterAddress);
            if (numSnowParticles > _numSnowParticles) // need to add controls
            {
                for (int i = _numSnowParticles; i < numSnowParticles; i++)
                    _snowParticleControls.Add(watchVariablePanelSnow.AddVariables(GetSnowParticleControls(i)));
                _numSnowParticles = numSnowParticles;
            }
            else if (numSnowParticles < _numSnowParticles) // need to remove controls
            {
                for (int i = _numSnowParticles - 1; i >= numSnowParticles; i--)
                {
                    var snowParticleControls = _snowParticleControls[i];
                    _snowParticleControls.Remove(snowParticleControls);
                    watchVariablePanelSnow.RemoveVariables(snowParticleControls);
                }

                _numSnowParticles = numSnowParticles;
            }

            base.Update(updateView);
        }
    }
}
