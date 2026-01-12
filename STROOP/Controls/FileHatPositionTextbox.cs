using STROOP.Structs.Configurations;
using STROOP.Tabs;

namespace STROOP
{
    public class FileHatPositionTextbox : FileTextbox
    {
        private short _currentValue;

        public FileHatPositionTextbox()
        {
        }

        public override void Initialize(uint addressOffset)
        {
            base.Initialize(addressOffset);
            this.Text = _currentValue.ToString();
        }

        private short GetHatLocationValueFromMemory()
        {
            return Config.Stream.GetInt16(FileTab.CurrentFileAddress + _addressOffset);
        }

        protected override void SubmitValue()
        {
            short value;
            if (!short.TryParse(this.Text, out value))
            {
                this.Text = GetHatLocationValueFromMemory().ToString();
                return;
            }

            Config.Stream.SetValue(value, FileTab.CurrentFileAddress + _addressOffset);
        }

        protected override void ResetValue()
        {
            short value = GetHatLocationValueFromMemory();
            this._currentValue = value;
            this.Text = value.ToString();
        }

        public override void UpdateText()
        {
            short value = GetHatLocationValueFromMemory();
            if (_currentValue != value)
            {
                this.Text = value.ToString();
                _currentValue = value;
            }
        }
    }
}
