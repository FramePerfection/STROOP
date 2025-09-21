using STROOP.Core;
using System.Collections.Generic;
using STROOP.Structs;
using STROOP.Utilities;

namespace STROOP.Tabs
{
    public partial class PaintingTab : STROOPTab
    {
        [InitializeBaseAddress]
        static void InitBaseAddresses()
        {
            WatchVariableUtilities.baseAddressGetters["Painting"] = () =>
            {
                uint? paintingAddress = AccessScope<StroopMainForm>.content.GetTab<PaintingTab>().GetPaintingAddress();
                return paintingAddress != null ? new List<uint>() { paintingAddress.Value } : WatchVariableUtilities.BaseAddressListEmpty;
            };
        }

        private class PaintingData
        {
            private readonly string _name;
            private readonly PaintingListTypeEnum _paintingListType;
            private readonly int _index;

            public PaintingData(string name, PaintingListTypeEnum paintingListType, int index)
            {
                _name = name;
                _paintingListType = paintingListType;
                _index = index;
            }

            public override string ToString()
            {
                return _name;
            }

            public uint GetAddress()
            {
                return PaintingConfig.GetAddress(_paintingListType, _index);
            }
        }

        private List<PaintingData> paintingDataList =
            new List<PaintingData>()
            {
                new PaintingData("BoB", PaintingListTypeEnum.Castle, 0),
                new PaintingData("WF", PaintingListTypeEnum.Castle, 2),
                new PaintingData("JRB", PaintingListTypeEnum.Castle, 3),
                new PaintingData("CCM", PaintingListTypeEnum.Castle, 1),
                new PaintingData("HMC", PaintingListTypeEnum.Castle, 6),
                new PaintingData("LLL", PaintingListTypeEnum.Castle, 4),
                new PaintingData("SSL", PaintingListTypeEnum.Castle, 5),
                new PaintingData("DDD", PaintingListTypeEnum.Castle, 7),
                new PaintingData("SL", PaintingListTypeEnum.Castle, 12),
                new PaintingData("WDW", PaintingListTypeEnum.Castle, 8),
                new PaintingData("TTM", PaintingListTypeEnum.Castle, 10),
                new PaintingData("TTM Slide", PaintingListTypeEnum.TTM, 0),
                new PaintingData("THI Tiny", PaintingListTypeEnum.Castle, 9),
                new PaintingData("THI Huge", PaintingListTypeEnum.Castle, 13),
                new PaintingData("TTC", PaintingListTypeEnum.Castle, 11),
                new PaintingData("CotMC", PaintingListTypeEnum.HMC, 0),
            };

        public PaintingTab()
        {
            InitializeComponent();
        }

        public override string GetDisplayName() => "Painting";

        public override void InitializeTab()
        {
            base.InitializeTab();

            foreach (PaintingData paintingData in paintingDataList)
            {
                listBoxPainting.Items.Add(paintingData);
            }
        }

        public uint? GetPaintingAddress()
        {
            PaintingData paintingData = listBoxPainting.SelectedItem as PaintingData;
            return paintingData?.GetAddress();
        }
    }
}
