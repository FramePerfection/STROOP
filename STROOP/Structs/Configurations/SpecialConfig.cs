using STROOP.Core;
using STROOP.Models;
using STROOP.Utilities;
using System;
using System.Collections.Generic;

namespace STROOP.Structs.Configurations
{
    public static class SpecialConfig
    {
        public static int ExtBoundariesShift => SavedSettingsConfig.UseExtendedLevelBoundaries ? 2 : 0;
        public static int ExtBoundariesScale => (1 << ExtBoundariesShift);

        // Cam Hack vars

        private static double _numPans = 0;

        public static double NumPans
        {
            get => _numPans;
            set
            {
                _numPans = Math.Max(0, value);
                AccessScope<StroopMainForm>.content.GetTab<Tabs.CamHackTab>().NotifyNumPanChange((int)_numPans);
            }
        }

        public static double CurrentPan
        {
            get
            {
                if (PanModels.Count == 0) return -1;
                uint globalTimer = Config.Stream.GetUInt32(MiscConfig.GlobalTimerAddress);
                for (int i = 0; i < PanModels.Count; i++)
                {
                    if (globalTimer < PanModels[i].PanStartTime)
                    {
                        return Math.Max(0, i - 1);
                    }
                }

                return PanModels.Count - 1;
            }
        }

        private static double _panCamPos = 0;

        public static double PanCamPos
        {
            get => _panCamPos;
            set
            {
                _panCamPos = value;
                if (_panCamPos != 0) _panCamRotation = 0;
            }
        }

        private static double _panCamAngle = 0;

        public static double PanCamAngle
        {
            get => _panCamAngle;
            set
            {
                _panCamAngle = value;
                if (_panCamAngle != 0) _panCamRotation = 0;
            }
        }

        private static double _panCamRotation = 0;

        public static double PanCamRotation
        {
            get => _panCamRotation;
            set
            {
                _panCamRotation = value;
                if (_panCamRotation != 0)
                {
                    _panCamPos = 0;
                    _panCamAngle = 0;
                }
            }
        }

        public static double PanFOV = 0;

        public static List<PanModel> PanModels = new List<PanModel>();

        // Rng vars

        public static ushort GoalRngIndex
        {
            get => RngIndexer.GetRngIndex(GoalRngValue);
            set => GoalRngValue = RngIndexer.GetRngValue(value);
        }

        public static ushort GoalRngValue = 0;

        // PU vars

        public static int PuParam1 = 0;
        public static int PuParam2 = 1;

        public static double PuHypotenuse
        {
            get => MoreMath.GetHypotenuse(PuParam1, PuParam2);
        }

        // Mupen vars

        public static int MupenLagOffset = 0;

        // Segmented vars

        public static uint SegmentedToVirtualAddress = 0;
        public static uint SegmentedToVirtualOutput => SegmentationUtilities.SegmentedToVirtual(SegmentedToVirtualAddress);
        public static uint VirtualToSegmentedSegment = 0;
        public static uint VirtualToSegmentedAddress = 0;
        public static uint VirtualToSegmentedOutput => SegmentationUtilities.VirtualToSegmented(VirtualToSegmentedSegment, VirtualToSegmentedAddress);


        public static double Map2DScrollSpeed = 1.1;

        // Release Status

        public static uint CustomReleaseStatus = 0;
    }
}
