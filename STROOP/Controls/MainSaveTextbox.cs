using System.Windows.Forms;
using STROOP.Structs.Configurations;
using STROOP.Tabs;

namespace STROOP
{
    public class MainSaveTextbox : TextBox
    {
        private uint _currentValue;

        private uint _offset;
        private uint _mask;
        private int _shift;

        public MainSaveTextbox()
        {
        }

        public void Initialize(int level, int file)
        {
            _offset = (uint)(4 * file);
            _mask = (uint)(0x3 << (2 * level));
            _shift = 2 * level;

            this.Text = _currentValue.ToString();
            this.DoubleClick += (sender, e) => this.SelectAll();
            this.KeyDown += (sender, e) =>
            {
                if (e.KeyData == Keys.Enter)
                {
                    SubmitValue();
                    this.Parent.Focus();
                }
                else if (e.KeyData == Keys.Escape)
                {
                    ResetText();
                    this.Parent.Focus();
                }
            };
            this.LostFocus += (sender, e) => SubmitValue();
        }

        private uint GetValueFromMemory()
        {
            return Config.Stream.GetUInt32(MainSaveTab.CurrentMainSaveAddress + _offset, false, _mask, _shift);
        }

        private void SetValueInMemory(uint value)
        {
            Config.Stream.SetValue(value, MainSaveTab.CurrentMainSaveAddress + _offset, false, _mask, _shift);
        }

        private void SubmitValue()
        {
            byte value;
            if (!byte.TryParse(this.Text, out value))
            {
                this.Text = GetValueFromMemory().ToString();
                return;
            }

            SetValueInMemory(value);
        }

        private void ResetValue()
        {
            uint value = GetValueFromMemory();
            this._currentValue = value;
            this.Text = value.ToString();
        }

        public void UpdateText()
        {
            uint value = GetValueFromMemory();
            if (_currentValue != value)
            {
                this.Text = value.ToString();
                _currentValue = value;
            }
        }
    }
}
