using STROOP.Structs;
using STROOP.Utilities;
using STROOP.Variables.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace STROOP.Tabs
{
    public partial class CoinTab : STROOPTab
    {
        public CoinTab()
        {
            InitializeComponent();
        }

        public override string GetDisplayName() => "Coin";

        public override void InitializeTab()
        {
            base.InitializeTab();
            ControlUtilities.SetTableDoubleBuffered(dataGridViewCoin, true);

            listBoxCoinObjects.DataSource = CoinObject.GetCoinObjects();
            listBoxCoinObjects.ClearSelected();
            listBoxCoinObjects.SelectedValueChanged += (sender, e) => ListBoxSelectionChange();

            buttonCoinCalculate.Click += (sender, e) => CalculateCoinTrajectories();
            buttonCoinClear.Click += (sender, e) => ClearCoinTrajectories();

            Color lightBlue = Color.FromArgb(235, 255, 255);
            Color lightPink = Color.FromArgb(255, 240, 255);
            Color lightYellow = Color.FromArgb(255, 255, 220);

            dataGridViewCoin.Columns[0].DefaultCellStyle.BackColor = lightBlue;
            dataGridViewCoin.Columns[1].DefaultCellStyle.BackColor = lightBlue;
            dataGridViewCoin.Columns[2].DefaultCellStyle.BackColor = lightPink;
            dataGridViewCoin.Columns[3].DefaultCellStyle.BackColor = lightYellow;
            dataGridViewCoin.Columns[4].DefaultCellStyle.BackColor = lightYellow;
            dataGridViewCoin.Columns[5].DefaultCellStyle.BackColor = lightYellow;
        }

        private void ListBoxSelectionChange()
        {
            CoinObject coinObject = listBoxCoinObjects.SelectedItem as CoinObject;
            textBoxCoinHSpeedScale.Text = coinObject.HSpeedScale.ToString();
            textBoxCoinVSpeedScale.Text = coinObject.VSpeedScale.ToString();
            textBoxCoinVSpeedOffset.Text = coinObject.VSpeedOffset.ToString();
            textBoxCoinParamOrder.Text = coinObject.CoinParamOrder.ToString();
            textBoxCoinNumCoins.Text = coinObject.NumCoins.ToString();
        }

        public void ClearCoinTrajectories()
        {
            dataGridViewCoin.Rows.Clear();
        }

        private void CalculateCoinTrajectories()
        {
            ClearCoinTrajectories();

            double? hSpeedScale = ParsingUtilities.ParseIntNullable(textBoxCoinHSpeedScale.Text);
            double? vSpeedScale = ParsingUtilities.ParseIntNullable(textBoxCoinVSpeedScale.Text);
            double? vSpeedOffset = ParsingUtilities.ParseIntNullable(textBoxCoinVSpeedOffset.Text);
            bool coinParamOrderParsed = Enum.TryParse(textBoxCoinParamOrder.Text, out CoinParamOrder coinParamOrder);
            int? numCoins = ParsingUtilities.ParseIntNullable(textBoxCoinNumCoins.Text);

            if (!hSpeedScale.HasValue ||
                !vSpeedScale.HasValue ||
                !vSpeedOffset.HasValue ||
                !coinParamOrderParsed ||
                !numCoins.HasValue)
            {
                DialogUtilities.DisplayMessage(
                    "Could not parse coin param fields.",
                    "Parsing Error");
                return;
            }

            CoinObject coinObject = new CoinObject(
                hSpeedScale: hSpeedScale.Value,
                vSpeedScale: vSpeedScale.Value,
                vSpeedOffset: vSpeedOffset.Value,
                coinParamOrder: coinParamOrder,
                numCoins: numCoins.Value,
                name: "Dummy");

            int? startingRngIndexNullable = ParsingUtilities.ParseIntNullable(
                textBoxCoinCustomizatonStartingRngIndex.Text);
            int startingRngIndex = startingRngIndexNullable ?? RngIndexer.GetRngIndex();

            int? numDecimalDigitsNullable = ParsingUtilities.ParseIntNullable(
                textBoxCoinCustomizatonNumDecimalDigits.Text);
            int numDecimalDigits = numDecimalDigitsNullable ?? 3;

            List<int> rngIndexes = Enumerable.Range(0, 65114).ToList();

            foreach (ushort rngIndex in rngIndexes)
            {
                // rng based values
                ushort rngValue = RngIndexer.GetRngValue(rngIndex);
                int rngToGo = MoreMath.NonNegativeModulus(rngIndex - startingRngIndex, 65114);

                // coin trajectory
                List<CoinTrajectory> coinTrajectories = coinObject.CalculateCoinTrajectories(rngIndex);

                // filter the values
                CoinTrajectoryFilter filter = new CoinTrajectoryFilter(
                    ParsingUtilities.ParseDoubleNullable(textBoxCoinFilterHSpeedMin.Text),
                    ParsingUtilities.ParseDoubleNullable(textBoxCoinFilterHSpeedMax.Text),
                    ParsingUtilities.ParseDoubleNullable(textBoxCoinFilterVSpeedMin.Text),
                    ParsingUtilities.ParseDoubleNullable(textBoxCoinFilterVSpeedMax.Text),
                    ParsingUtilities.ParseDoubleNullable(textBoxCoinFilterAngleMin.Text),
                    ParsingUtilities.ParseDoubleNullable(textBoxCoinFilterAngleMax.Text),
                    ParsingUtilities.ParseIntNullable(textBoxCoinFilterRequiredNumOfQualifiedCoins.Text));
                if (!filter.Qualifies(coinTrajectories)) continue;

                if (!checkBoxCoinCustomizatonDisplayNonQualifiedCoinsOfAQualifiedCoinGroup.Checked)
                {
                    coinTrajectories = coinTrajectories.FindAll(
                        coinTrajectory => filter.Qualifies(coinTrajectory));
                }

                List<double> hSpeedList = coinTrajectories.ConvertAll(
                    coinTrajectory => Math.Round(coinTrajectory.HSpeed, numDecimalDigits));
                List<double> vSpeedList = coinTrajectories.ConvertAll(
                    coinTrajectory => Math.Round(coinTrajectory.VSpeed, numDecimalDigits));
                List<ushort> angleList = coinTrajectories.ConvertAll(
                    coinTrajectory => coinTrajectory.Angle);

                object hSpeedJoined = hSpeedList.Count == 1 ? hSpeedList[0] : (object)String.Join(", ", hSpeedList);
                object vSpeedJoined = vSpeedList.Count == 1 ? vSpeedList[0] : (object)String.Join(", ", vSpeedList);
                object angleJoined = angleList.Count == 1 ? angleList[0] : (object)String.Join(", ", angleList);

                // add a new row to the table
                dataGridViewCoin.Rows.Add(
                    rngIndex, rngValue, rngToGo, hSpeedJoined, vSpeedJoined, angleJoined);
            }
        }

        public override void Update(bool updateView)
        {
            if (!updateView) return;

            double? hSpeedScaleNullable = ParsingUtilities.ParseDoubleNullable(textBoxCoinHSpeedScale.Text);
            if (hSpeedScaleNullable.HasValue)
            {
                double hSpeedScale = hSpeedScaleNullable.Value;
                double hSpeedMin = 0;
                double hSpeedMax = hSpeedScale;
                labelCoinHSpeedRange.Text = String.Format("HSpeed Range: [{0}, {1})", hSpeedMin, hSpeedMax);
            }
            else
            {
                labelCoinHSpeedRange.Text = "HSpeed Range:";
            }

            double? vSpeedScaleNullable = ParsingUtilities.ParseDoubleNullable(textBoxCoinVSpeedScale.Text);
            double? vSpeedOffsetNullable = ParsingUtilities.ParseDoubleNullable(textBoxCoinVSpeedOffset.Text);
            if (vSpeedScaleNullable.HasValue && vSpeedOffsetNullable.HasValue)
            {
                double vSpeedScale = vSpeedScaleNullable.Value;
                double vSpeedOffset = vSpeedOffsetNullable.Value;
                double vSpeedMin = vSpeedOffset;
                double vSpeedMax = vSpeedScale + vSpeedOffset;
                labelCoinVSpeedRange.Text = String.Format("VSpeed Range: [{0}, {1})", vSpeedMin, vSpeedMax);
            }
            else
            {
                labelCoinVSpeedRange.Text = "VSpeed Range:";
            }

            labelCoinTableEntries.Text = "Table Entries: " + dataGridViewCoin.Rows.Count;
        }
    }
}
