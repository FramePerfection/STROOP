using STROOP.Structs.Configurations;
using STROOP.Tabs;

namespace STROOP
{
    public class FileCoinScoreTextbox : FileTextbox
    {
        private byte _currentValue;

        public FileCoinScoreTextbox()
        {
        }

        public override void Initialize(uint addressOffset)
        {
            base.Initialize(addressOffset);
            this.Text = _currentValue.ToString();
        }

        private byte GetCoinScoreFromMemory()
        {
            return Config.Stream.GetByte(FileTab.CurrentFileAddress + _addressOffset);
        }

        protected override void SubmitValue()
        {
            byte value;
            if (!byte.TryParse(this.Text, out value))
            {
                this.Text = GetCoinScoreFromMemory().ToString();
                return;
            }

            Config.Stream.SetValue(value, FileTab.CurrentFileAddress + _addressOffset);
        }

        protected override void ResetValue()
        {
            byte value = GetCoinScoreFromMemory();
            this._currentValue = value;
            this.Text = value.ToString();
        }

        public override void UpdateText()
        {
            byte value = GetCoinScoreFromMemory();
            if (_currentValue != value)
            {
                this.Text = value.ToString();
                _currentValue = value;
            }
        }
    }
}
